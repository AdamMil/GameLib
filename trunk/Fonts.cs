/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2004 Adam Milazzo

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*/

using System;
using System.Collections;
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
    Color = Color.White;
    BackColor = Color.Black;
  }

  public abstract int Height { get; }
  public abstract int LineSkip { get; }
  public abstract Color Color { get; set; }
  public abstract Color BackColor { get; set; }

  public abstract Size CalculateSize(string text);
  public abstract int  HowManyFit(string text, int pixels);

  public int Render(Surface dest, string text, Point pt) { return Render(dest, text, pt.X, pt.Y); }
  public abstract int Render(Surface dest, string text, int x, int y);

  public Point Render(Surface dest, string text, Rectangle rect)
  { return Render(dest, text, rect, ContentAlignment.TopLeft, 0, 0, breakers);
  }
  public Point Render(Surface dest, string text, Rectangle rect, ContentAlignment align)
  { return Render(dest, text, rect, align, 0, 0, breakers);
  }
  public Point Render(Surface dest, string text, Rectangle rect, int startx, int starty)
  { return Render(dest, text, rect, ContentAlignment.TopLeft, startx, starty, breakers);
  }
  public virtual Point Render(Surface dest, string text, Rectangle rect, ContentAlignment align,
                              int startx, int starty, char[] breakers)
  { if(align!=ContentAlignment.TopLeft && (startx!=0 || starty!=0))
      throw new ArgumentException("'startx' and 'starty' can only be used with 'align'==TopLeft");
    if(startx<0 || starty<0) throw new ArgumentException("'startx' and 'starty' must be positive");

    int[] lines = WordWrap(text, rect, startx, starty, breakers);

    int  start=0, x=rect.X+startx, y=rect.Y+starty, length=0, horz;
    if(lines.Length==0) return new Point(x, y);

    horz = Forms.Helpers.AlignedLeft(align) ? 0 : Forms.Helpers.AlignedCenter(align) ? 1 : 2;
    if(Forms.Helpers.AlignedMiddle(align)) y = rect.Y + (rect.Height-lines.Length*LineSkip)/2;
    else if(Forms.Helpers.AlignedBottom(align)) y = rect.Bottom-lines.Length*LineSkip;
    y-=LineSkip;

    for(int i=0; i<lines.Length; i++)
    { if(i==1) { x=rect.X; y=rect.Y; }
      y += LineSkip;
      string chunk = text.Substring(start, lines[i]);
      if(horz==0) length = Render(dest, chunk, x, y);
      else
      { length = CalculateSize(chunk).Width;
        if(horz==1) Render(dest, chunk, rect.X+(rect.Width-length)/2, y);
        else Render(dest, chunk, rect.Right-length, y);
      }
      start += lines[i];
    }
    return align==ContentAlignment.TopLeft ? new Point(length + (lines.Length==1 ? startx : 0), y)
                                           : new Point(-1, -1);
  }
  
  // TODO: remove these?
  public void Center(Surface dest, string text) { CenterOff(dest, text, 0, 0); }
  public void Center(Surface dest, string text, int y)
  { int width = CalculateSize(text).Width;
    Render(dest, text, (dest.Width-width)/2, y);
  }
  public void CenterOff(Surface dest, string text, int yoffset) { CenterOff(dest, text, yoffset, 0); }
  public void CenterOff(Surface dest, string text, int yoffset, int xoffset)
  { Size size = CalculateSize(text);
    Render(dest, text, (dest.Width-size.Width)/2+xoffset, (dest.Height-size.Height)/2+yoffset);
  }

  public int[] WordWrap(string text, Rectangle rect) { return WordWrap(text, rect, 0, 0, breakers); }
  public int[] WordWrap(string text, Rectangle rect, char[] breakers)
  { return WordWrap(text, rect, 0, 0, breakers);
  }
  public int[] WordWrap(string text, Rectangle rect, Point start)
  { return WordWrap(text, rect, start.X, start.Y, breakers);
  }
  public int[] WordWrap(string text, Rectangle rect, Point start, char[] breakers)
  { return WordWrap(text, rect, start.X, start.Y, breakers);
  }
  public int[] WordWrap(string text, Rectangle rect, int startx, int starty)
  { return WordWrap(text, rect, startx, starty, breakers);
  }
  public virtual int[] WordWrap(string text, Rectangle rect, int startx, int starty, char[] breakers)
  { if(text.Length==0) return new int[0];

    ArrayList list = new ArrayList();
    // HACK: LineSkip should never be less than Height, but sometimes it is, so we take the minimum
    int x=rect.X+startx, start=0, end=0, pend, length=0, plen=0, height=Math.Min(Height, LineSkip);
    int rwidth=rect.Width-startx, rheight=rect.Height-starty;
    if(height>rheight) return new int[0];

    while(true)
    { for(pend=end; end<text.Length; end++)
      { char c=text[end];
        for(int i=0; i<breakers.Length; i++) if(c==breakers[i]) { end++; goto extend; }
      }
      extend:
      plen    = length;
      length += CalculateSize(text.Substring(pend, end-pend)).Width;

      if(length>rwidth)
      { height += LineSkip;
        if(pend==start)
        { if(rwidth<rect.Width)
          { if(height>=rheight) break;
            end=pend; length=plen; list.Add(0); goto cont;
          }
          for(length=plen; pend<end; pend++)
          { length += CalculateSize(text[pend].ToString()).Width;
            if(length>rwidth) break;
            plen=length;
          }
          if(pend==start) goto done; // can't possibly fit. prevent infinite loops
          end = pend;
        }
        else { end=pend; length=plen; }
        list.Add(end-start);
        if(height>rheight) break;
        start  = end;
        length = 0;
        cont:
        rwidth = rect.Width;
      }
      else if(end==text.Length)
      { list.Add(end-start);
        break;
      }
    }
    done:
    return (int[])list.ToArray(typeof(int));
  }

  protected virtual void DisplayFormatChanged() { }
  protected void Deinit()
  { if(handler!=null)
    { Video.Video.ModeChanged -= handler;
      handler=null;
    }
  }

  static readonly char[] breakers = new char[] { ' ', '-', '\n' };
  Video.ModeChangedHandler handler;
}

[Flags]
public enum FontStyle : byte
{ Normal=0, Bold=1, Italic=2, Underlined=4
}
public abstract class NonFixedFont : Font
{ public abstract FontStyle Style { get; set; }
}
#endregion

#region BitmapFont class
public class BitmapFont : Font, IDisposable
{ public BitmapFont(IBlittable font, string charset, int charWidth) : this(font, charset, charWidth, 1, font.Height+2) { }
  public BitmapFont(IBlittable font, string charset, int charWidth, int xAdd, int lineSkip)
  { orig   = font;
    width  = charWidth;
    height = lineSkip;
    this.xAdd    = xAdd;
    this.charset = charset;
    DisplayFormatChanged();
  }
  ~BitmapFont() { Dispose(true); }
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }
  
  public override int Height   { get { return font.Height; } }
  public override int LineSkip { get { return height; } }
  public override Color Color  { get { return Color.White; } set { } }
  public override Color BackColor { get { return Color.Black; } set { } }

  public override Size CalculateSize(string text)
  { if(text.Length==0) return new Size(Height, 0);
    if(text.Length==1) return new Size(Height, width);
    return new Size(Height, (text.Length-1)*(width+xAdd)+width);
  }
  
  public override int HowManyFit(string text, int pixels)
  { int chars = text.Length;
    if(chars==0) return 0;
    pixels -= width; chars--;
    if(pixels<0) return 0;
    return pixels/(width+xAdd)+1;
  }

  public override int Render(Surface dest, string text, int x, int y)
  { int start=x;
    for(int i=0,add=width+xAdd; i<text.Length; i++)
    { int off = charset.IndexOf(text[i]);
      if(off!=-1) font.Blit(dest, new Rectangle(off*width, 0, width, Height), x, y);
      x += add;
    }
    return x-start;
  }

  protected override void DisplayFormatChanged()
  { IBlittable compatible = orig.CreateCompatible();
    font = compatible==null ? orig : compatible;
  }

  protected void Dispose(bool destructor) { base.Deinit(); }

  IBlittable font, orig;
  string charset;
  int    width, height, xAdd;
}
#endregion

#region TrueType enums & class
public enum RenderStyle : byte
{ Solid, Shaded, Blended
}
public class TrueTypeFont : NonFixedFont, IDisposable
{ public TrueTypeFont(string filename, int pointSize)
  { TTF.Initialize();
    try { font = TTF.OpenFont(filename, pointSize); }
    catch(NullReferenceException) { unsafe { font = new IntPtr(null); } }
    Init();
  }
  public TrueTypeFont(string filename, int pointSize, int fontIndex)
  { try { font = TTF.OpenFontIndex(filename, pointSize, fontIndex); }
    catch(NullReferenceException) { unsafe { font = new IntPtr(null); } }
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
  public RenderStyle RenderStyle  { get { return rstyle;  } set { rstyle=value;  } }
  public override Color Color
  { get { return color; }
    set { if(color!=value) { color=value; sdlColor=new SDL.Color(value); } }
  }
  public override Color BackColor
  { get { return bgColor; }
    set { if(bgColor!=value) { bgColor=value; sdlBgColor=new SDL.Color(value); } }
  }

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
  { /*int width, height; -- DOESN'T WORK CONSISTENTLY
    TTF.SizeUNICODE(font, text, out width, out height);*/
    Size size = new Size(0, Height);
    for(int i=0; i<text.Length; i++) size.Width += GetChar(text[i]).Advance; // less correct, but more consistent
      //size.Width += i==0 && c.OffsetX<0 ? c.Width+c.OffsetX : i==text.Length-1 ? c.Width : c.Advance;
    return size;
  }

  public override int HowManyFit(string text, int pixels)
  { int width=0;
    for(int i=0; i<text.Length; i++)
    { CachedChar c = GetChar(text[i]);
      /*width += i>0 ? c.Advance : 0;
      if(width+c.Width+(i==0 && c.OffsetX<0 ? c.OffsetX : 0) > pixels) return i;*/
      width+=c.Advance;
      if(width>pixels) return i; // less correct, but more consistent
    }
    return text.Length;
  }

  public override int Render(Surface dest, string text, int x, int y)
  { int start = x;
    for(int i=0; i<text.Length; i++)
    { CachedChar c = GetChar(text[i]);
      if(i==0 && c.OffsetX<0)
      { Rectangle rect = c.Surface.Bounds;
        rect.X -= c.OffsetX; rect.Width += c.OffsetX;
        c.Surface.Blit(dest, rect, x, y+c.OffsetY);
      }
      else c.Surface.Blit(dest, x+c.OffsetX, y+c.OffsetY);
      x += c.Advance;
    }
    return x-start;
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
    public int     OffsetX, OffsetY, Width, Advance;
    public char    Char;
    public bool    Compatible;
    
    void Dispose(bool destructor) { Surface.Dispose(); }
  }
  
  // TODO: implement a priority queue or something to efficiently limit the cache size
  protected CachedChar GetChar(char c)
  { CacheIndex ind = new CacheIndex(c, color, bgColor, fstyle, rstyle);
    LinkedList.Node node = (LinkedList.Node)tree[ind];
    CachedChar cc;
    if(node!=null)
    { cc = (CachedChar)node.Data;
      if(!cc.Compatible && Video.Video.DisplaySurface!=null)
      { cc.Surface = cc.Surface.CloneDisplay();
        cc.Compatible = true;
      }
      list.Remove(node);
      list.Prepend(node);
      return cc;
    }

    cc = new CachedChar(c);
    int minx, maxx, miny, maxy, advance;
    TTF.Check(TTF.GlyphMetrics(font, c, out minx, out maxx, out miny, out maxy, out advance));
    cc.Index   = ind;
    cc.OffsetX = minx;
    cc.OffsetY = Ascent-maxy;
    cc.Width   = maxx-minx;
    cc.Advance = advance;
    unsafe
    { SDL.Surface* surface=null;
      switch(rstyle)
      { case RenderStyle.Solid:   surface = TTF.RenderGlyph_Solid(font, c, sdlColor); break;
        case RenderStyle.Shaded:  surface = TTF.RenderGlyph_Shaded(font, c, sdlColor, sdlBgColor); break;
        case RenderStyle.Blended: surface = TTF.RenderGlyph_Blended(font, c, sdlColor); break;
      }
      if(surface==null) TTF.RaiseError();
      cc.Surface = new Surface(surface, true);
      if(Video.Video.DisplaySurface!=null)
      { cc.Surface = cc.Surface.CloneDisplay();
        cc.Compatible = true;
      }
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
  
  protected void Init()
  { unsafe { if(font.ToPointer()==null) { TTF.Deinitialize(); TTF.RaiseError(); } }
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