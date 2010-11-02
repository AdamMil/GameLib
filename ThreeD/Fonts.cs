using System;
using System.Collections.Generic;
using System.Drawing;
using AdamMil.Utilities;
using GameLib.Interop.OpenGL;
using GameLib.Interop.SDL;
using GameLib.Interop.SDLTTF;
using GameLib.Video;
using RectanglePacker = AdamMil.Mathematics.Geometry.TwoD.RectanglePacker;

namespace GameLib.Fonts
{

#region GLFont
/// <summary>Provides a base class for fonts that render into an OpenGL display.</summary>
public abstract class GLFont : Font
{
  /// <include file="../documentation.xml" path="//Fonts/Render/Point/Pt/*[@name != 'dest']"/>
  public int Render(string text, PointF pt) { return Render(text, pt.X, pt.Y); }

  /// <include file="../documentation.xml" path="//Fonts/Render/Point/XY/*[@name != 'dest']"/>
  public abstract int Render(string text, float x, float y);

  /// <include file="../documentation.xml" path="//Fonts/Render/Rect/NoAlign/*[@name != 'dest']"/>
  public PointF Render(string text, RectangleF rect)
  {
    return Render(text, rect, ContentAlignment.TopLeft, 0, 0, breakers);
  }

  /// <include file="../documentation.xml" path="//Fonts/Render/Rect/Align/*[@name != 'dest']"/>
  public PointF Render(string text, RectangleF rect, ContentAlignment align)
  {
    return Render(text, rect, align, 0, 0, breakers);
  }

  /// <include file="../documentation.xml" path="//Fonts/Render/Rect/*[self::NoAlign or self::Offset]/*[@name != 'dest']"/>
  /// <remarks>The X and Y offsets into the rectangle are provided to allow continuing rendering where you left off.</remarks>
  public PointF Render(string text, RectangleF rect, int startx, int starty)
  {
    return Render(text, rect, ContentAlignment.TopLeft, startx, starty, breakers);
  }

  /// <include file="../documentation.xml" path="//Fonts/Render/Rect/*[self::Align or self::Offset]/*[@name != 'dest']"/>
  /// <param name="breakers">An array of characters that will be used to break the text. Those characters will
  /// mark preferred places within the string to break to a new line.
  /// </param>
  /// <remarks>The X and Y offsets into the rectangle are provided to allow continuing rendering where you left off,
  /// but they can only be used (nonzero) if the alignment is <see cref="ContentAlignment.TopLeft"/>.
  /// </remarks>
  public virtual PointF Render(string text, RectangleF rect, ContentAlignment align,
                               float startx, float starty, char[] breakers)
  {
    if(align!=ContentAlignment.TopLeft && (startx!=0 || starty!=0))
      throw new ArgumentException("'startx' and 'starty' can only be used with 'align'==TopLeft");
    if(startx<0 || starty<0) throw new ArgumentException("'startx' and 'starty' must be positive");

    int[] lines = WordWrap(text, new Rectangle(0, 0, (int)rect.Width, (int)rect.Height),
                           (int)rect.Width - (int)(rect.Width - startx),
                           (int)rect.Height - (int)(rect.Height - starty), breakers);

    int start=0, length=0, horz;
    float x=rect.X+startx, y=rect.Y+starty;
    if(lines.Length==0) return new PointF(x, y);

    horz = UIHelper.IsAlignedLeft(align) ? 0 : UIHelper.IsAlignedCenter(align) ? 1 : 2;
    if(UIHelper.IsAlignedMiddle(align)) y = rect.Y + ((int)rect.Height-lines.Length*LineHeight)/2;
    else if(UIHelper.IsAlignedBottom(align)) y = rect.Bottom-lines.Length*LineHeight;
    y-=LineHeight;

    for(int i=0; i<lines.Length; i++)
    {
      if(i==1 && align == ContentAlignment.TopLeft) { x=rect.X; y=rect.Y; } // undo the effect of startx and starty
      y += LineHeight;
      string chunk = text.Substring(start, lines[i]);
      if(horz==0) length = Render(chunk, x, y);
      else
      {
        length = CalculateSize(chunk).Width;
        if(horz==1) Render(chunk, rect.X+((int)rect.Width-length)/2, y);
        else Render(chunk, rect.Right-length, y);
      }
      start += lines[i];
    }
    // FIXME: fix this
    x = UIHelper.IsAlignedLeft(align) ? length + (lines.Length==1 ? startx : 0) : -1;
    if(!UIHelper.IsAlignedTop(align)) y = -1;
    return new PointF(x, y);
  }
}
#endregion

#region GLTrueTypeFont
/// <summary>Implements a TrueType font renderer for OpenGL displays.</summary>
public class GLTrueTypeFont : GLFont
{
  int MinimumCacheSize = 64;

  /// <include file="../documentation.xml" path="//Fonts/TrueTypeFont/Cons/File/*"/>
  public GLTrueTypeFont(string filename, int pointSize)
  { TTF.Initialize();
    try { font = TTF.OpenFont(filename, pointSize); }
    catch(NullReferenceException) { unsafe { font = new IntPtr(null); } }
    Init(pointSize);
  }

  /// <include file="../documentation.xml" path="//Fonts/TrueTypeFont/Cons/*[self::File or self::Index]/*"/>
  public GLTrueTypeFont(string filename, int pointSize, int fontIndex)
  { try { font = TTF.OpenFontIndex(filename, pointSize, fontIndex); }
    catch(NullReferenceException) { unsafe { font = new IntPtr(null); } }
    Init(pointSize);
  }

  /// <include file="../documentation.xml" path="//Fonts/TrueTypeFont/Cons/*[self::Stream or self::WillClose]/*"/>
  public GLTrueTypeFont(System.IO.Stream stream, int pointSize) : this(stream, pointSize, true) { }

  /// <include file="../documentation.xml" path="//Fonts/TrueTypeFont/Cons/*[self::Stream or self::AutoClose]/*"/>
  public GLTrueTypeFont(System.IO.Stream stream, int pointSize, bool autoClose)
  { SeekableStreamRWOps source = new SeekableStreamRWOps(stream, autoClose);
    unsafe { fixed(SDL.RWOps* ops = &source.ops) font = TTF.OpenFontRW(ops, 0, pointSize); }
    Init(pointSize);
  }

  /// <include file="../documentation.xml" path="//Fonts/TrueTypeFont/Cons/*[self::Stream or self::Index or self::WillClose]/*"/>
  public GLTrueTypeFont(System.IO.Stream stream, int pointSize, int fontIndex) : this(stream, pointSize, fontIndex, true) { }

  /// <include file="../documentation.xml" path="//Fonts/TrueTypeFont/Cons/*[self::Stream or self::Index or self::AutoClose]/*"/>
  public GLTrueTypeFont(System.IO.Stream stream, int pointSize, int fontIndex, bool autoClose)
  { SeekableStreamRWOps source = new SeekableStreamRWOps(stream, autoClose);
    unsafe { fixed(SDL.RWOps* ops = &source.ops) font = TTF.OpenFontIndexRW(ops, 0, pointSize, fontIndex); }
    Init(pointSize);
  }

  /// <summary>Gets the maximum pixel ascent of all glyphs in the font.</summary>
  /// <remarks>The maximum pixel ascent can also be interpreted as the distance from the top of the font to the
  /// baseline.
  /// </remarks>
  public int Ascent 
  {
    get { return TTF.FontAscent(font); } 
  }

  /// <summary>Gets the maximum pixel descent of all glyphs in the font.</summary>
  /// <remarks>The maximum pixel descent can also be interpreted as the distance from the baseline to the bottom of
  /// the font.
  /// </remarks>
  public int Descent 
  {
    get { return TTF.FontDescent(font); } 
  }

  /// <summary>Gets the height of the font, in pixels.</summary>
  public override int Height 
  {
    get { return TTF.FontHeight(font); } 
  }

  /// <include file="../documentation.xml" path="//Fonts/LineHeight/*"/>
  public override int LineHeight
  {
    get { return TTF.FontLineSkip(font); } 
  }

  /// <summary>Gets the size of the font, in points. This is the value passed to the constructor.</summary>
  public int PointSize
  {
    get; private set;
  }

  /// <summary>Gets the name of the font family.</summary>
  public string FamilyName 
  {
    get { return TTF.FontFaceFamilyName(font); } 
  }
  
  /// <summary>Gets the name of the font style.</summary>
  public string StyleName 
  {
    get { return TTF.FontFaceStyleName(font); } 
  }

  /// <summary>Gets or sets the foreground color of the font.</summary>
  public override Color Color
  {
    get { return color; }
    set { color = value; }
  }
  /// <summary>Gets or sets the background color of the font.</summary>
  /// <remarks>The background color represents the color drawn behind the font.</remarks>
  public override Color BackColor
  {
    get { return bgColor; }
    set { bgColor = value; }
  }

  /// <summary>Gets or sets whether bilinear filtering will be used when scaling the font glyphs. The default is false.</summary>
  public bool BilinearFiltering
  {
    get; set;
  }

  /// <summary>Gets or sets the maximum size of the glyph cache. The default is 128.</summary>
  /// <value>The maximum number of glyphs allowed in the glyph cache.</value>
  /// <remarks>This class caches the most recently-used glyphs so they don't have to be recalculated every time
  /// they're used. Some level of caching is required, so setting this below the minimum will simply use that minimum.
  /// </remarks>
  public int MaxCacheSize
  {
    get { return cacheMax; }
    set
    {
      if(value < 0) throw new ArgumentException("Cache size cannot be negative");
      int realSize = Math.Max(MinimumCacheSize, value);
      if(realSize < list.Count) ClearCache();
      cacheMax = value;
    }
  }

  /// <summary>Gets or sets the rendering style that will be used to render the font.</summary>
  /// <remarks>Note that <see cref="Fonts.RenderStyle.Shaded"/> is not supported. It is recommended that you use
  /// <see cref="Fonts.RenderStyle.Blended"/> instead.
  /// </remarks>
  public RenderStyle RenderStyle
  {
    get { return renderStyle; }
    set
    {
      if(value == RenderStyle.Shaded) throw new NotSupportedException("The Shaded style is not supported.");
      renderStyle = value; 
    }
  }

  /// <summary>Gets or sets the font style that will be used to render the font.</summary>
  /// <remarks>Not all fonts support all font styles.</remarks>
  public FontStyle Style
  {
    get { return fontStyle; }
    set
    {
      if(fontStyle != value)
      {
        TTF.SetFontStyle(font, (int)value);
        fontStyle = (FontStyle)TTF.GetFontStyle(font);
      }
    }
  }

  /// <include file="../documentation.xml" path="//Fonts/CalculateSize/*"/>
  public override Size CalculateSize(string text)
  { 
    Size size = new Size(0, Height);
    for(int i=0; i<text.Length; i++) size.Width += GetChar(text, i, false).Advance;
    return size;
  }

  /// <include file="../documentation.xml" path="//Fonts/HowManyFit/*"/>
  public override int HowManyFit(string text, int width)
  {
    int pixels=0;
    for(int i=0; i<text.Length; i++)
    {
      pixels += GetChar(text, i, false).Advance;
      if(pixels > width) return i;
    }
    return text.Length;
  }

  /// <include file="../documentation.xml" path="//Fonts/Render/Point/XY/*[@name != 'dest']"/>
  public override int Render(string text, float x, float y)
  {
    if(text == null) throw new ArgumentNullException();
    if(text.Length == 0) return 0;

    const int DesiredSourceBlend = GL.GL_SRC_ALPHA, DesiredDestBlend = GL.GL_ONE_MINUS_SRC_ALPHA;
    int oldSourceBlend = GL.glGetIntegerv(GL.GL_BLEND_SRC), oldDestBlend = GL.glGetIntegerv(GL.GL_BLEND_DST);
    bool texturingEnabled = GL.glIsEnabled(GL.GL_TEXTURE_2D), blendingEnabled = GL.glIsEnabled(GL.GL_BLEND);

    if(!blendingEnabled) GL.glEnable(GL.GL_BLEND);
    if(oldSourceBlend != DesiredSourceBlend || oldDestBlend != DesiredDestBlend)
    {
      GL.glBlendFunc(DesiredSourceBlend, DesiredDestBlend);
    }

    if(bgColor.A != 0)
    {
      int width = 0;
      for(int i=0; i<text.Length; i++) width += GetChar(text, i, false).Advance;

      float right = x + width, bottom = y + Height;
      if(texturingEnabled) GL.glDisable(GL.GL_TEXTURE_2D);
      GL.glColor(bgColor);
      GL.glBegin(GL.GL_QUADS);
      GL.glVertex2f(x, y);
      GL.glVertex2f(right, y);
      GL.glVertex2f(right, bottom);
      GL.glVertex2f(x, bottom);
      GL.glEnd();
      if(texturingEnabled) GL.glEnable(GL.GL_TEXTURE_2D);
    }

    if(!texturingEnabled) GL.glEnable(GL.GL_TEXTURE_2D);
    GL.glColor(Color);

    float start = x;
    for(int i=0; i < text.Length; i++)
    {
      CachedChar c = GetChar(text, i, true);
      if(i == 0 || texture == null) // if this is the first character or the texture was recreated, we need to bind it
      {
        EnsureTexture();
        texture.Bind();
        int filter = BilinearFiltering ? GL.GL_LINEAR : GL.GL_NEAREST;
        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, filter);
        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, filter);
        GL.glBegin(GL.GL_QUADS); // if the texture was recreated, GetChar() will have called glEnd()
        renderingQuads = true;
      }

      if(c.GlyphWidth != 0)
      {
        float offsetX = x + c.OffsetX, offsetY = y + c.OffsetY;
        GL.glTexCoord2f(c.TextureX, c.TextureY);
        GL.glVertex2f(offsetX, offsetY);
        GL.glTexCoord2f(c.TextureX + c.TextureWidth, c.TextureY);
        GL.glVertex2f(offsetX + c.GlyphWidth, offsetY);
        GL.glTexCoord2f(c.TextureX + c.TextureWidth, c.TextureY + c.TextureHeight);
        GL.glVertex2f(offsetX + c.GlyphWidth, offsetY + c.GlyphHeight);
        GL.glTexCoord2f(c.TextureX, c.TextureY + c.TextureHeight);
        GL.glVertex2f(offsetX, offsetY + c.GlyphHeight);
      }

      x += c.Advance;
    }
    GL.glEnd();

    if(oldSourceBlend != DesiredSourceBlend || oldDestBlend != DesiredDestBlend)
    {
      GL.glBlendFunc(oldSourceBlend, oldDestBlend);
    }
    if(!blendingEnabled) GL.glDisable(GL.GL_BLEND);
    if(!texturingEnabled) GL.glDisable(GL.GL_TEXTURE_2D);

    return (int)(x - start);
  }

  /// <include file="../documentation.xml" path="//Common/Dispose/*"/>
  protected override void Dispose(bool finalizing)
  {
    ClearCache();

    if(font != IntPtr.Zero)
    { 
      TTF.CloseFont(font);
      TTF.Deinitialize();
      font = IntPtr.Zero;
    }

    base.Dispose(finalizing);
  }

  /// <include file="../documentation.xml" path="//Font/OnDisplayFormatChanged/*"/>
  protected override void OnDisplayFormatChanged()
  {
    ClearCache();
  }

  sealed class CachedChar
  {
    public CachedChar(CacheIndex index)
    {
      Index = index;
      Char  = index.Char;
    }

    public float TextureX, TextureY, TextureWidth, TextureHeight, GlyphWidth, GlyphHeight, OffsetX, OffsetY;
    public int Width, Advance;
    public CacheIndex Index;
    public char Char;
  }

  struct CacheIndex
  {
    public CacheIndex(char c, FontStyle style, bool blended)
    {
      Char    = c;
      Style   = style;
      Blended = blended;
    }

    public override bool Equals(object obj)
    {
      CacheIndex other = (CacheIndex)obj;
      return Char == other.Char && Style == other.Style && Blended == other.Blended;
    }

    public override int GetHashCode()
    {
      return (Char << 9) | ((int)Style << 1) | (Blended ? 1 : 0);
    }

    public readonly char Char;
    public readonly FontStyle Style;
    public readonly bool Blended;
  }

  void ClearCache()
  {
    InvalidateSurface();
    list.Clear();
    nodesByIndex.Clear();
  }

  void EnsureTexture()
  {
    if(texture == null)
    {
      int oldBinding = GL.glGetIntegerv(GL.GL_TEXTURE_BINDING_2D);
      texture = new GLTexture2D(GL.GL_LUMINANCE_ALPHA, cacheSurface);
      texture.Bind();
      GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_CLAMP);
      GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_CLAMP);
      GL.glBindTexture(GL.GL_TEXTURE_2D, oldBinding);
    }
  }

  CachedChar GetChar(string text, int index, bool duringRender)
  {
    CacheIndex cacheIndex = GetCacheIndex(text[index]);

    LinkedListNode<CachedChar> node;
    if(nodesByIndex.TryGetValue(cacheIndex, out node)) // if the node is in the cache...
    {
      list.Remove(node); // move the node to the beginning to keep the list in most-recently-used order
      list.AddFirst(node);
      return node.Value;
    }

    // if the caller doesn't need the actual glyph, we can just return the character information
    if(!duringRender) return GetChar(cacheIndex);

    // if this happened during rendering, we need to finish the current set of primitives so OpenGL doesn't freak out
    // when we delete the texture that it was using
    if(renderingQuads)
    {
      GL.glEnd();
      renderingQuads = false;
    }

    // the character doesn't exist in the cache. see if we can slip it into the current cache image
    if(packer != null)
    {
      CachedChar charToReturn = null;
      for(int i=index; i < text.Length; i++) // we'll also try to pack the next several characters in there as well
      {
        cacheIndex = GetCacheIndex(text[i]);
        if(nodesByIndex.ContainsKey(cacheIndex)) continue;

        using(Surface glyph = GetGlyph(cacheIndex))
        {
          // we'll add a border around the glyph so that if OpenGL does texture filtering it won't pick up pixels from
          // other glyphs
          Size sizeWithBorder = new Size(glyph.Width+2, glyph.Height+2);
          Point? point = packer.TryAdd(sizeWithBorder);
          if(point.HasValue) // it fit
          {
            CachedChar cc = GetChar(cacheIndex);

            if(glyph.Width != 0 && glyph.Height != 0)
            {
              Rectangle areaInsideBorder = new Rectangle(point.Value.X+1, point.Value.Y+1, glyph.Width, glyph.Height);
              glyph.Blit(cacheSurface, areaInsideBorder.Location);
              OpenGL.AddTextureBorder(cacheSurface, areaInsideBorder);
              InvalidateTexture();
              SetTextureArea(cc, areaInsideBorder);
            }

            nodesByIndex[cacheIndex] = list.AddFirst(cc);
            if(i == 0) charToReturn = cc; // we want to return the first character
          }
          else break;// it didn't fit, so that's all we can do for now
        }
      }

      if(charToReturn != null) return charToReturn; // if the first character could be added, return it
    }

    // the character doesn't exist and couldn't be added to the existing cache, so we'll create a new cache texture
    // filled with this character, the next several characters from the string, and the most recently-used characters
    // from the current cache texture
    int cacheSize = Math.Max(MinimumCacheSize, MaxCacheSize);
    Dictionary<CacheIndex,CachedChar> charsToCache = new Dictionary<CacheIndex,CachedChar>(cacheSize);

    // add characters from this string
    for(int i=index; i < text.Length && charsToCache.Count < cacheSize; i++)
    {
      cacheIndex = GetCacheIndex(text[i]);
      if(charsToCache.ContainsKey(cacheIndex)) continue;
      charsToCache[cacheIndex] = nodesByIndex.TryGetValue(cacheIndex, out node) ? node.Value : GetChar(cacheIndex);
    }

    // now add the most recently-used characters from the current cache
    for(node = list.First; node != null && charsToCache.Count < cacheSize; node = node.Next)
    {
      charsToCache[node.Value.Index] = node.Value;
    }
    
    // remove the characters from the list that we couldn't add
    if(node != null)
    {
      do
      {
        LinkedListNode<CachedChar> nextNode = node.Next;
        nodesByIndex.Remove(node.Value.Index);
        list.Remove(node);
        node = nextNode;
      } while(node != null);
    }

    // clear out the old cache (to dispose the old surface and texture)
    InvalidateSurface();

    // put the cached characters into an array so we can access them by ordinal
    CachedChar[] newCachedChars = new CachedChar[charsToCache.Count];
    charsToCache.Values.CopyTo(newCachedChars, 0);

    Surface[] glyphs = new Surface[charsToCache.Count];
    try
    {
      // get the corresponding glyphs
      for(int i=0; i<glyphs.Length; i++) glyphs[i] = GetGlyph(newCachedChars[i].Index);

      // get the sizes of the glyphs
      Size[] sizes = new Size[charsToCache.Count];
      for(int i=0; i<sizes.Length; i++) sizes[i] = glyphs[i].Size;

      // pack the glyphs into a rectangle
      bool requiresPowerOfTwo = !OpenGL.HasNonPowerOfTwoExtension;
      Point[] points = TexturePacker.PackTexture(sizes, true, requiresPowerOfTwo, out packer);
      
      // create a surface to hold the new glyphs
      cacheSurface = new Surface(packer.TotalSize.Width, packer.TotalSize.Height, new PixelFormat(32, true));

      // add the glyphs to the surface
      for(int i=0; i<glyphs.Length; i++)
      {
        if(glyphs[i].Width != 0 && glyphs[i].Height != 0)
        {
          glyphs[i].Blit(cacheSurface, points[i]);
          Rectangle areaInsideBorder = new Rectangle(points[i], glyphs[i].Size);
          OpenGL.AddTextureBorder(cacheSurface, areaInsideBorder);
          SetTextureArea(newCachedChars[i], areaInsideBorder);
        }
      }

      // add the new cached characters to the linked list
      foreach(CachedChar cc in newCachedChars)
      {
        if(!nodesByIndex.ContainsKey(cc.Index)) nodesByIndex[cc.Index] = list.AddFirst(cc);
      }

      return nodesByIndex[GetCacheIndex(text[index])].Value;
    }
    finally
    {
      foreach(Surface glyph in glyphs) glyph.Dispose();
    }
  }

  CacheIndex GetCacheIndex(char c)
  {
    return new CacheIndex(c, Style, RenderStyle == RenderStyle.Blended);
  }

  CachedChar GetChar(CacheIndex index)
  {
    CachedChar cc = new CachedChar(index);
    int minX, maxX, minY, maxY, advance;
    TTF.Check(TTF.GlyphMetrics(font, index.Char, out minX, out maxX, out minY, out maxY, out advance));
    cc.OffsetX = minX;
    cc.OffsetY = Ascent - maxY;
    cc.Width   = maxX - minX;
    cc.Advance = advance;
    return cc;
  }

  unsafe Surface GetGlyph(CacheIndex index)
  {
    FontStyle oldFontStyle = Style;
    try
    {
      Style = index.Style;
      SDL.Color white = new SDL.Color(255, 255, 255);
      SDL.Surface* surfacePtr = index.Blended ?
          TTF.RenderGlyph_Blended(font, index.Char, white) : TTF.RenderGlyph_Solid(font, index.Char, white);
      if(surfacePtr == null) TTF.RaiseError();

      Surface surface = new Surface(surfacePtr, true);
      surface.UsingAlpha = false;
      return surface;
    }
    finally { Style = oldFontStyle; }
  }

  void Init(int pointSize)
  {
    if(font == IntPtr.Zero) 
    { 
      TTF.Deinitialize(); 
      TTF.RaiseError(); 
    }
    PointSize = pointSize;
  }

  void InvalidateSurface()
  {
    InvalidateTexture();
    Utility.Dispose(ref cacheSurface);
    packer = null;
  }

  void InvalidateTexture()
  {
    Utility.Dispose(ref texture);
  }

  void SetTextureArea(CachedChar cc, Rectangle glyphRect)
  {
    float textureWidth = cacheSurface.Width, textureHeight = cacheSurface.Height;
    cc.TextureX = glyphRect.X / textureWidth;
    cc.TextureY = glyphRect.Y / textureHeight;
    cc.TextureWidth  = glyphRect.Width  / textureWidth;
    cc.TextureHeight = glyphRect.Height / textureHeight;

    cc.GlyphWidth  = glyphRect.Width;
    cc.GlyphHeight = glyphRect.Height;
  }

  IntPtr font;
  GLTexture2D texture;
  Surface cacheSurface;
  readonly LinkedList<CachedChar> list = new LinkedList<CachedChar>();
  readonly Dictionary<CacheIndex, LinkedListNode<CachedChar>> nodesByIndex = new Dictionary<CacheIndex, LinkedListNode<CachedChar>>();
  RectanglePacker packer;
  Color color = Color.White, bgColor = Color.Transparent;
  int cacheMax = 128;
  FontStyle fontStyle;
  RenderStyle renderStyle = RenderStyle.Solid;
  bool renderingQuads;
}
#endregion

} // namespace GameLib.Fonts