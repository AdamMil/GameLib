using System;
using System.Drawing;
using GameLib.Video;
using GameLib.Collections;
using GameLib.Interop.SDL;
using GameLib.Interop.SDLTTF;

namespace GameLib.Fonts
{

#region Abstract classes
public abstract class Font
{ public Font()
  { handler = new Video.ModeChangedHandler(DisplayFormatChanged);
    Video.Video.ModeChanged += handler;
  }

  public abstract int Height { get; }
  public abstract int LineSkip { get; }

  public abstract Size CalculateSize(string text);

  public void Render(Surface dest, string text, Point pt) { Render(dest, text, pt.X, pt.Y); }
  public abstract void Render(Surface dest, string text, int x, int y);

  public void Render(Surface dest, string text, Rectangle rect) { Render(dest, text, rect, breakers); }
  public virtual void Render(Surface dest, string text, Rectangle rect, char[] breakers)
  { int[] lines = WordWrap(text, rect, breakers);
    int start=0, y=rect.Y;
    for(int i=0; i<lines.Length; i++)
    { Render(dest, text.Substring(start, lines[i]), rect.X, y);
      start += lines[i];
      y += LineSkip;
    }
  }

  public int[] WordWrap(string text, Rectangle rect) { return WordWrap(text, rect, breakers); }
  public virtual int[] WordWrap(string text, Rectangle rect, char[] breakers)
  { throw new NotImplementedException();
  }

  protected abstract void DisplayFormatChanged();
  protected void Deinit()
  { if(handler!=null)
    { Video.Video.ModeChanged -= handler;
      handler=null;
    }
  }

  static readonly char[] breakers = new char[] { ' ', '-', ',', ':', ';' };
  Video.ModeChangedHandler handler;
}

[Flags]
public enum FontStyle : byte
{ Normal=0, Bold=1, Italic=2, Underlined=4
}
public abstract class NonFixedFont : Font
{ public abstract FontStyle Style { get; set; }
  public abstract Color     Color { get; set; }
  protected void Init() { Color=Color.White; Style=FontStyle.Normal; }
}
#endregion

#region BitmapFont class
public class BitmapFont : Font, IDisposable
{ public BitmapFont(Surface font, string charset, int charWidth) : this(font, charset, charWidth, 1, font.Height+2) { }
  public BitmapFont(Surface font, string charset, int charWidth, int xAdd, int lineSkip)
  { orig   = font;
    width  = charWidth;
    height = lineSkip;
    this.xAdd    = xAdd;
    this.font    = orig.CloneDisplay();
    this.charset = charset;
  }
  ~BitmapFont() { Dispose(true); }
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }
  
  public override int Height   { get { return font.Height; } }
  public override int LineSkip { get { return height; } }

  public override Size CalculateSize(string text)
  { if(text.Length==0) return new Size(Height, 0);
    if(text.Length==1) return new Size(Height, width);
    return new Size(Height, (text.Length-1)*(width+xAdd)+width);
  }

  public override void Render(Surface dest, string text, int x, int y)
  { for(int i=0,add=width+xAdd; i<text.Length; i++)
    { int off = charset.IndexOf(text[i]);
      if(off!=-1) font.Blit(dest, new Rectangle(off*width, 0, width, Height), x, y);
      x += add;
    }
  }

  protected override void DisplayFormatChanged() { font=orig.CloneDisplay(); }
  protected void Dispose(bool destructor)
  { font.Dispose();
    orig.Dispose();
    base.Deinit();
  }

  Surface font, orig;
  string  charset;
  int     width, height, xAdd;
}
#endregion

#region TrueType enums & class
public enum RenderStyle : byte
{ Solid, Shaded, Blended
}
public class TrueTypeFont : NonFixedFont, IDisposable
{ public TrueTypeFont(string filename, int pointSize)
  { TTF.Initialize();
    font = TTF.OpenFont(filename, pointSize);
    Init();
  }
  public TrueTypeFont(string filename, int pointSize, int fontIndex)
  { font = TTF.OpenFontIndex(filename, pointSize, fontIndex);
    Init();
  }
  public TrueTypeFont(System.IO.Stream stream, int pointSize) : this(stream, pointSize, true) { }
  public TrueTypeFont(System.IO.Stream stream, int pointSize, bool autoClose)
  { SeekableStreamRWOps source = new SeekableStreamRWOps(stream, autoClose);
    unsafe { fixed(SDL.RWOps* ops = &source.ops) font = TTF.OpenFontRW(ops, 0, pointSize); }
    Init();
  }
  public TrueTypeFont(System.IO.Stream stream, int pointSize, int fontIndex) : this(stream, pointSize, fontIndex, true) { }
  public TrueTypeFont(System.IO.Stream stream, int pointSize, int fontIndex, bool autoClose)
  { SeekableStreamRWOps source = new SeekableStreamRWOps(stream, autoClose);
    unsafe { fixed(SDL.RWOps* ops = &source.ops) font = TTF.OpenFontIndexRW(ops, 0, pointSize, fontIndex); }
    Init();
  }
  ~TrueTypeFont() { Dispose(true); }
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  public override FontStyle Style
  { get { return fstyle; }
    set
    { if(fstyle==value) return;
      TTF.SetFontStyle(font, (int)value);
      fstyle = (FontStyle)TTF.GetFontStyle(font);
    }
  }
  public RenderStyle RenderStyle { get { return rstyle;  } set { rstyle=value;  } }
  public override Color Color    { get { return color;   } set { color=value; sdlColor=new SDL.Color(value); } }
  public Color    BackColor      { get { return bgColor; } set { bgColor=value; sdlBgColor=new SDL.Color(value); } }

  public override int Height { get { return TTF.FontHeight(font); } }
  public override int LineSkip { get { return TTF.FontLineSkip(font); } }
  public int Ascent  { get { return TTF.FontAscent(font); } }
  public int Descent { get { return TTF.FontDescent(font); } }

  public string FamilyName { get { return TTF.FontFaceFamilyName(font); } }
  public string StyleName  { get { return TTF.FontFaceStyleName(font);  } }

  public int CacheSize
  { get { return cacheMax; }
    set
    { if(value<0) throw new ArgumentException("Cache size cannot be negative");
      if(value==0) ClearCache();
      else if(value<list.Count)
      { LinkedList.Node n=list.Tail;
        for(int i=0,num=list.Count-value; i<num; n=n.Prev,i++) CacheRemove(n);
      }
      cacheMax = value;
    }
  }
  
  public override Size CalculateSize(string text)
  { int width, height;
    TTF.SizeUNICODE(font, text, out width, out height);
    return new Size(width, height);
  }
  public override void Render(Surface dest, string text, int x, int y)
  { for(int i=0; i<text.Length; i++)
    { CachedChar c = GetChar(text[i]);
      c.Surface.Blit(dest, x+c.OffsetX, y+c.OffsetY);
      x += c.Advance;
    }
  }

  protected struct CacheIndex : IComparable
  { public CacheIndex(char c, Color color, Color bgColor, FontStyle fstyle, RenderStyle rstyle)
    { Char=c; Color=color; BackColor=bgColor; FontStyle=fstyle; RenderStyle=rstyle;
    }
    public int CompareTo(object other)
    { CacheIndex o = (CacheIndex)other;
      int cmp = Char.CompareTo(o.Char);
      if(cmp!=0) return cmp;
      cmp = this.FontStyle.CompareTo(o.FontStyle);
      if(cmp!=0) return cmp;
      cmp = this.RenderStyle.CompareTo(o.RenderStyle);
      if(cmp!=0) return cmp;
      cmp = this.Color.ToArgb().CompareTo(o.Color.ToArgb());
      if(cmp!=0 || this.RenderStyle!=RenderStyle.Shaded) return cmp;
      return this.BackColor.ToArgb().CompareTo(o.BackColor.ToArgb());
    }
    public Color       Color;
    public Color       BackColor;
    public char        Char;
    public FontStyle   FontStyle;
    public RenderStyle RenderStyle;
  }
  protected class CachedChar : IDisposable
  { public CachedChar() { }
    public CachedChar(char c) { Char=c; }
    ~CachedChar() { Dispose(true); }
    public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }
    
    public CacheIndex Index;
    public Surface Surface;
    public int     OffsetX, OffsetY, Advance;
    public char    Char;
    
    void Dispose(bool destructor) { Surface.Dispose(); }
  }
  
  // TODO: implement a priority queue to efficiently limit the cache size
  protected CachedChar GetChar(char c)
  { CacheIndex ind = new CacheIndex(c, color, bgColor, fstyle, rstyle);
    LinkedList.Node node = (LinkedList.Node)tree[ind];
    if(node!=null) return (CachedChar)node.Data;

    CachedChar cc = new CachedChar(c);
    int minx, maxx, miny, maxy, advance;
    TTF.Check(TTF.GlyphMetrics(font, c, out minx, out maxx, out miny, out maxy, out advance));
    cc.Index   = ind;
    cc.OffsetX = Math.Max(minx, 0);
    cc.OffsetY = Ascent-maxy;
    cc.Advance = advance;
    unsafe
    { SDL.Surface* surface=null;
      switch(rstyle)
      { case RenderStyle.Solid:   surface = TTF.RenderGlyph_Solid(font, c, sdlColor); break;
        case RenderStyle.Shaded:  surface = TTF.RenderGlyph_Shaded(font, c, sdlColor, sdlBgColor); break;
        case RenderStyle.Blended: surface = TTF.RenderGlyph_Blended(font, c, sdlColor); break;
      }
      if(surface==null) TTF.RaiseError();
      cc.Surface = new Surface(surface, true).CloneDisplay();
    }

    if(cacheMax!=0)
    { if(list.Count>=cacheMax) CacheRemove(list.Tail);
      tree[ind]=list.Prepend(cc);
    }
    return cc;
  }
  
  protected override void DisplayFormatChanged() { ClearCache(); }

  protected void ClearCache()
  { tree.Clear();
    list.Clear();
  }
  protected void CacheRemove(LinkedList.Node n)
  { CachedChar cc = (CachedChar)n.Data;
    cc.Dispose();
    tree.Remove(cc.Index);
    list.Remove(n);
  }

  protected void Dispose(bool destructor)
  { unsafe
    { if(font.ToPointer()!=null)
      { TTF.CloseFont(font);
        TTF.Deinitialize();
        font = new IntPtr(null);
      }
    }
    if(list.Count>0)
    { foreach(CachedChar c in list) c.Dispose();
      ClearCache();
    }
    base.Deinit();
  }
  
  protected new void Init()
  { unsafe { if(font.ToPointer()==null) { TTF.Deinitialize(); TTF.RaiseError(); } }
    base.Init();
    BackColor   = Color.Black;
    RenderStyle = RenderStyle.Solid;
  }

  protected RedBlackTree tree = new RedBlackTree();
  protected LinkedList   list = new LinkedList();
  protected Color        color, bgColor;
  protected int          cacheMax=512;
  protected FontStyle    fstyle;
  protected RenderStyle  rstyle;
  private SDL.Color sdlColor, sdlBgColor;
  private IntPtr    font;
}
#endregion

} // namespace GameLib.Fonts