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
/// <summary>This abstract class provides the base for all types of fonts in GameLib.</summary>
/// <remarks>Most fonts are fairly large objects, so you should not create multiple copies of the same font in your
/// application. It's better to keep them in one central place and share them with code that needs to render text.
/// Fonts are not thread-safe, however, so you should not render text from muliple threads at the same time.
/// </remarks>
public abstract class Font : IDisposable
{ 
  /// <summary>Initializes the font.</summary>
  public Font()
  { handler = new Video.ModeChangedHandler(OnDisplayFormatChanged);
    Video.Video.ModeChanged += handler;
  }
  /// <summary>This destructor calls <see cref="Dispose()"/> to release system resources used by the font.</summary>
  ~Font() { Dispose(true); }
  /// <summary>This method should be called to release system resources used by the font.</summary>
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  /// <summary>Gets the height of the font, in pixels.</summary>
  public abstract int Height { get; }
  /// <summary>Gets the vertical offset that should be added to move the draw position to the next line. This is
  /// equivalent to the height of the font plus spacing between lines.
  /// </summary>
  public abstract int LineSkip { get; }
  /// <summary>Gets/sets the color of the font.</summary>
  /// <remarks>The interpretation of this property is up to the implementing class, so see the documentation for
  /// the derived classes for more details.
  /// </remarks>
  public abstract Color Color { get; set; }
  /// <summary>Gets/sets the color of the area behind the font.</summary>
  /// <remarks>This property can be set to <see cref="System.Drawing.Color.Transparent"/> if you don't want the
  /// background behind the font to be filled in. The interpretation of this property is up to the implementing class,
  /// however, so see the documentation for the derived classes for more details.
  /// </remarks>
  public abstract Color BackColor { get; set; }

  /// <summary>Calculates the size of a string of text.</summary>
  /// <param name="text">The text to use for the calculation.</param>
  /// <returns>A <see cref="Size"/> that contains the amount of space required to render the given text with this
  /// font.
  /// </returns>
  /// <remarks>This method does not perform word-wrapping on the text, and returns a calculation assuming the text
  /// is rendered as a single line.
  /// </remarks>
  public abstract Size CalculateSize(string text);
  /// <summary>Calculates how many characters of the given string would fit into the given width.</summary>
  /// <param name="text">The string to test.</param>
  /// <param name="width">The available width, in pixels.</param>
  /// <returns>The number of characters that would fully fit within the given width.</returns>
  public abstract int HowManyFit(string text, int width);

  /// <summary>Renders text at a given point.</summary>
  /// <param name="dest">The surface to render into.</param>
  /// <param name="text">The text to render.</param>
  /// <param name="pt">The point where the top-left corner of the rendered text should appear.</param>
  /// <returns>Returns the width of the text rendered plus spacing so that you can use the value to calculate where
  /// more text can be rendered (by adding the returned width to the starting position).
  /// </returns>
  public int Render(Surface dest, string text, Point pt) { return Render(dest, text, pt.X, pt.Y); }
  /// <summary>Renders text at a given point.</summary>
  /// <param name="dest">The surface to render into.</param>
  /// <param name="text">The text to render.</param>
  /// <param name="x">The X coordinate where the left side of the rendered text should appear.</param>
  /// <param name="y">The Y coordinate where the top edge of the rendered text should appear.</param>
  /// <returns>Returns the width of the text rendered plus spacing so that you can use the value to calculate where
  /// more text can be rendered (by adding the returned width to the starting position).
  /// </returns>
  public abstract int Render(Surface dest, string text, int x, int y);

  /// <summary>Word-wraps and renders text into a given rectangle.</summary>
  /// <param name="dest">The surface to render into.</param>
  /// <param name="text">The text to render.</param>
  /// <param name="rect">The area into which text will be rendered.</param>
  /// <returns>Returns the end point of the rendering, where rendering can be continue with more text.</returns>
  public Point Render(Surface dest, string text, Rectangle rect)
  { return Render(dest, text, rect, ContentAlignment.TopLeft, 0, 0, breakers);
  }
  /// <summary>Word-wraps and renders text into a given rectangle.</summary>
  /// <param name="dest">The surface to render into.</param>
  /// <param name="text">The text to render.</param>
  /// <param name="rect">The area into which text will be rendered.</param>
  /// <param name="align">The <see cref="ContentAlignment"/> to use inside the rectangle.</param>
  /// <returns>Returns the end point of the rendering, where rendering can be continue with more text.
  /// This only works with top-left alignment, since the endpoints for other types of alignment require before-hand
  /// knowledge of the amount of text. For other alignment types, the value of the returned point is undefined.
  /// </returns>
  public Point Render(Surface dest, string text, Rectangle rect, ContentAlignment align)
  { return Render(dest, text, rect, align, 0, 0, breakers);
  }
  /// <summary>Word-wraps and renders text into a given rectangle.</summary>
  /// <param name="dest">The surface to render into.</param>
  /// <param name="text">The text to render.</param>
  /// <param name="rect">The area into which text will be rendered.</param>
  /// <param name="startx">The X offset into the rectangle at which rendering will begin.</param>
  /// <param name="starty">The Y offset into the rectangle at which rendering will begin.</param>
  /// <returns>Returns the end point of the rendering, where rendering can be continue with more text.</returns>
  /// <remarks>The X and Y offsets into the rectangle are provided to allow continuing rendering where you left off.</remarks>
  public Point Render(Surface dest, string text, Rectangle rect, int startx, int starty)
  { return Render(dest, text, rect, ContentAlignment.TopLeft, startx, starty, breakers);
  }
  /// <summary>Word-wraps and renders text into a given rectangle.</summary>
  /// <param name="dest">The surface to render into.</param>
  /// <param name="text">The text to render.</param>
  /// <param name="rect">The area into which text will be rendered.</param>
  /// <param name="align">The <see cref="ContentAlignment"/> to use inside the rectangle.</param>
  /// <param name="startx">The X offset into the rectangle at which rendering will begin.</param>
  /// <param name="starty">The Y offset into the rectangle at which rendering will begin.</param>
  /// <param name="breakers">An array of characters that will be used to break the text. Those characters will
  /// mark preferred places within the string to break to a new line.
  /// </param>
  /// <returns>Returns the end point of the rendering, where rendering can be continue with more text.
  /// This only works with top-left alignment, since the endpoints for other types of alignment require before-hand
  /// knowledge of the amount of text. For other alignment types, the value of the returned point is undefined.
  /// </returns>
  /// <remarks>The X and Y offsets into the rectangle are provided to allow continuing rendering where you left off,
  /// but they can only be used (nonzero) if the alignment is <see cref="ContentAlignment.TopLeft"/>.
  /// </remarks>
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
    // FIXME: fix this
    x = Forms.Helpers.AlignedLeft(align) ? length + (lines.Length==1 ? startx : 0) : -1;
    if(!Forms.Helpers.AlignedTop(align)) y = -1;
    return new Point(x, y);
  }
  
  /// <summary>Renders text centered within a surface.</summary>
  /// <param name="dest">The surface to render into.</param>
  /// <param name="text">The text to render.</param>
  public void Center(Surface dest, string text)
  { Size size = CalculateSize(text);
    Render(dest, text, (dest.Width-size.Width)/2, (dest.Height-size.Height)/2);
  }
  /// <summary>Renders text centered horizontally within a surface at a specified vertical location.</summary>
  /// <param name="dest">The surface to render into.</param>
  /// <param name="text">The text to render.</param>
  /// <param name="y">The Y coordinate to begin rendering at.</param>
  public void Center(Surface dest, string text, int y)
  { int width = CalculateSize(text).Width;
    Render(dest, text, (dest.Width-width)/2, y);
  }

  /// <summary>Word-wraps text into a specified rectangle.</summary>
  /// <param name="text">The text to word-wrap.</param>
  /// <param name="rect">The area bounding the text.</param>
  /// <returns>An array of integers specifying indices into the string. There is one element for each line of text.
  /// The value of the element is how many characters should be rendered on that line.
  /// </returns>
  /// <remarks>Currently, a newline character will not force a line break. This behavior will be changed in the future.</remarks>
  public int[] WordWrap(string text, Rectangle rect) { return WordWrap(text, rect, 0, 0, breakers); }
  /// <summary>Word-wraps text into a specified rectangle.</summary>
  /// <param name="text">The text to word-wrap.</param>
  /// <param name="rect">The area bounding the text.</param>
  /// <param name="breakers">An array of characters that will be used to break the text. Those characters will
  /// mark preferred places within the string to break to a new line.
  /// </param>
  /// <returns>An array of integers specifying indices into the string. There is one element for each line of text.
  /// The value of the element is how many characters should be rendered on that line.
  /// </returns>
  /// <remarks>Currently, a newline character will not force a line break. This behavior will be changed in the future.</remarks>
  public int[] WordWrap(string text, Rectangle rect, char[] breakers)
  { return WordWrap(text, rect, 0, 0, breakers);
  }
  /// <summary>Word-wraps text into a specified rectangle.</summary>
  /// <param name="text">The text to word-wrap.</param>
  /// <param name="rect">The area bounding the text.</param>
  /// <param name="startx">The X offset into the rectangle at which the text should start.</param>
  /// <param name="starty">The Y offset into the rectangle at which the text should start.</param>
  /// <returns>An array of integers specifying indices into the string. There is one element for each line of text.
  /// The value of the element is how many characters should be rendered on that line.
  /// </returns>
  /// <remarks>Currently, a newline character will not force a line break. This behavior will be changed in the future.</remarks>
  public int[] WordWrap(string text, Rectangle rect, int startx, int starty)
  { return WordWrap(text, rect, startx, starty, breakers);
  }
  /// <summary>Word-wraps text into a specified rectangle.</summary>
  /// <param name="text">The text to word-wrap.</param>
  /// <param name="rect">The area bounding the text.</param>
  /// <param name="startx">The X offset into the rectangle at which the text should start.</param>
  /// <param name="starty">The Y offset into the rectangle at which the text should start.</param>
  /// <param name="breakers">An array of characters that will be used to break the text. Those characters will
  /// mark preferred places within the string to break to a new line.
  /// </param>
  /// <returns>An array of integers specifying indices into the string. There is one element for each line of text.
  /// The value of the element is how many characters should be rendered on that line.
  /// </returns>
  /// <remarks>Currently, a newline character will not force a line break. This behavior will be changed in the future.</remarks>
  // TODO: respect newline characters in text
  public virtual int[] WordWrap(string text, Rectangle rect, int startx, int starty, char[] breakers)
  { if(text.Length==0) return new int[0];

    ArrayList list = new ArrayList(); // make this a class member?
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

  /// <summary>Frees resources used by the font.</summary>
  /// <param name="finalizing">True if this is being called from a destructor and false otherwise.</param>
  /// <remarks>Derived classes that override this method should remember to call the the base class' implementation
  /// as well.
  /// </remarks>
  protected virtual void Dispose(bool finalizing)
  { if(handler!=null)
    { Video.Video.ModeChanged -= handler;
      handler=null;
    }
  }

  /// <summary>This method is called when the video mode changes.</summary>
  /// <remarks>If it's important to know when the video mode changes, override this method. Remember to call
  /// the base class' implementation as well. This might be used to convert font images to a format suitable for
  /// fast blitting to the screen, for instance.
  /// </remarks>
  protected virtual void OnDisplayFormatChanged() { }

  static readonly char[] breakers = new char[] { ' ', '-', '\n' };
  Video.ModeChangedHandler handler;
}

/// <summary>This enum contains different types of font styles, which can be ORed together.</summary>
[Flags]
public enum FontStyle : byte
{ 
  /// <summary>Normal font rendering.</summary>
  Normal=0,
  /// <summary>Renders the font bolded.</summary>
  Bold=1,
  /// <summary>Renders the font italicized.</summary>
  Italic=2,
  /// <summary>Renders the font underlined.</summary>
  Underlined=4
}
/// <summary>This class serves as a base for fonts that can render with different font styles.</summary>
public abstract class StyledFont : Font
{ 
  /// <summary>Gets/sets the rendering style to use with this font.</summary>
  public abstract FontStyle Style { get; set; }
}
#endregion

#region BitmapFont class
/// <summary>This class provides a simple bitmap-based font. It can handle both fixed-width and variable-width fonts.
/// </summary>
/// <remarks>Since this font uses a bitmap for rendering, the foreground color of the font cannot be set.
/// The bitmap associated with this font should be considered to be owned by this font object, and specifically
/// should not be used by other threads simultaneously.
/// </remarks>
public class BitmapFont : Font
{ 
  /// <summary>Initializes a fixed-width font with default spacing.</summary>
  /// <param name="font">A surface containing the font data.</param>
  /// <param name="charset">A list of characters in the font, in the order in which they appear in the image.</param>
  /// <param name="charWidth">The width of the characters in the font.</param>
  /// <remarks>The surface should contain all the characters, in the order specified by <paramref name="charset"/>,
  /// all in a single row. Each character should be <paramref name="charWidth"/> pixels wide, and there should be no
  /// unused space anywhere. If the surface has a color key set, it will be used.
  /// </remarks>
  public BitmapFont(Surface font, string charset, int charWidth) : this(font, charset, charWidth, 1, 2) { }
  /// <summary>Initializes a fixed-width font.</summary>
  /// <param name="font">A surface containing the font data.</param>
  /// <param name="charset">A list of characters in the font, in the order in which they appear in the image.</param>
  /// <param name="charWidth">The width of the characters in the font.</param>
  /// <param name="horzSpacing">The horizontal spacing that will be added after each character.</param>
  /// <param name="vertSpacing">The vertical spacing that will be added between lines.</param>
  /// <remarks>The surface should contain all the characters, in the order specified by <paramref name="charset"/>,
  /// all in a single row. Each character should be <paramref name="charWidth"/> pixels wide, and there should be no
  /// unused space anywhere. If the surface has a color key set, it will be used.
  /// </remarks>
  public BitmapFont(Surface font, string charset, int charWidth, int horzSpacing, int vertSpacing)
  { orig     = font;
    width    = charWidth;
    lineSkip = vertSpacing+font.Height;
    this.charset = charset;
    this.xAdd    = horzSpacing;
    if(font.Width<charset.Length*charWidth)
      throw new ArgumentException("The given font bitmap is too small to have this many characters!");
    OnDisplayFormatChanged();
    bgColor = Color.Transparent;
  }
  /// <summary>Initializes a variable-width font with default spacing.</summary>
  /// <param name="font">A surface containing the font data.</param>
  /// <param name="charset">A list of characters in the font, in the order in which they appear in the image.</param>
  /// <param name="charWidths">An array the same length as <paramref name="charset"/>, containing the width of each
  /// character.
  /// </param>
  /// <remarks>The surface should contain all the characters, in the order specified by <paramref name="charset"/>,
  /// all in a single row. Each character should the number of pixels specified in <paramref name="charWidths"/>
  /// pixels wide, and there should be no unused space anywhere. If the surface has a color key set, it will be used.
  /// </remarks>
  public BitmapFont(Surface font, string charset, int[] charWidths) : this(font, charset, charWidths, 1, 2) { }
  /// <summary>Initializes a variable-width font with default spacing.</summary>
  /// <param name="font">A surface containing the font data.</param>
  /// <param name="charset">A list of characters in the font, in the order in which they appear in the image.</param>
  /// <param name="charWidths">An array the same length as <paramref name="charset"/>, containing the width of each
  /// character.
  /// </param>
  /// <param name="horzSpacing">The horizontal spacing that will be added after each character.</param>
  /// <param name="vertSpacing">The vertical spacing that will be added between lines.</param>
  /// <remarks>The surface should contain all the characters, in the order specified by <paramref name="charset"/>,
  /// all in a single row. Each character should the number of pixels specified in <paramref name="charWidths"/>
  /// pixels wide, and there should be no unused space anywhere. If the surface has a color key set, it will be used.
  /// </remarks>
  public BitmapFont(Surface font, string charset, int[] charWidths, int horzSpacing, int vertSpacing)
  { if(charWidths==null) throw new ArgumentNullException("charWidths");
    if(charset.Length != charWidths.Length)
      throw new ArgumentException("The length of the charset must match the length of charWidths.");
    orig     = font;
    widths   = charWidths;
    lineSkip = vertSpacing+font.Height;
    offsets  = new int[charset.Length];
    for(int i=1; i<charset.Length; i++) offsets[i] = offsets[i-1]+widths[i-1];
    if(offsets[charset.Length-1] + widths[charset.Length-1] > font.Width)
      throw new ArgumentException("The given font bitmap is too small to have this many characters!");
    this.xAdd     = horzSpacing;
    this.charset  = charset;
    OnDisplayFormatChanged();
    bgColor = Color.Transparent;
  }
  
  /// <summary>Gets the height of the font, in pixels.</summary>
  public override int Height { get { return font.Height; } }
  /// <summary>Gets the vertical offset that should be added to move the draw position to the next line. This is
  /// equivalent to the height of the font plus spacing between lines.
  /// </summary>
  public override int LineSkip { get { return lineSkip; } }
  /// <summary>Since the source of data for this font is a bitmap, this property does nothing.</summary>
  public override Color Color { get { return Color.White; } set { } }
  /// <summary>Controls the background color.</summary>
  /// <value>The color filled behind the font. If set to <see cref="System.Drawing.Color.Transparent"/>,
  /// the background will not be filled.
  /// </value>
  public override Color BackColor
  { get { return bgColor; }
    set { bgColor=value; }
  }

  /// <summary>Calculates the size of a string of text.</summary>
  /// <param name="text">The text to use for the calculation.</param>
  /// <returns>A <see cref="Size"/> that contains the amount of space required to render the given text with this
  /// font.
  /// </returns>
  /// <remarks>See <see cref="Font.CalculateSize"/> for more details regarding this method.</remarks>
  public override Size CalculateSize(string text)
  { if(text.Length==0) return new Size(Height, 0);
    if(widths==null)
    { if(text.Length==1) return new Size(Height, width);
      return new Size(Height, (text.Length-1)*(width+xAdd)+width);
    }
    Size ret = new Size(Height, -xAdd);
    for(int i=0; i<text.Length; i++)
    { int off = charset.IndexOf(text[i]);
      if(off!=-1) ret.Width += widths[off]+xAdd;
    }
    return ret;
  }

  /// <summary>Calculates how many characters of the given string would fit into the given width.</summary>
  /// <param name="text">The text to test.</param>
  /// <param name="width">The available width, in pixels.</param>
  /// <returns>The number of characters that would fully fit within the given width.</returns>
  /// <remarks>See <see cref="Font.HowManyFit"/> for more details regarding this method.</remarks>
  public override int HowManyFit(string text, int width)
  { if(text.Length==0) return 0;
    width += xAdd;
    if(widths==null) return width/(this.width+xAdd);
    for(int i=0; i<text.Length; i++)
    { int off = charset.IndexOf(text[i]);
      if(off==-1) continue;
      width -= widths[off]+xAdd;
      if(width<0) return i;
    }
    return text.Length;
  }

  /// <summary>Renders text at a given point.</summary>
  /// <param name="dest">The surface to render into.</param>
  /// <param name="text">The text to render.</param>
  /// <param name="x">The X coordinate where the left side of the rendered text should appear.</param>
  /// <param name="y">The Y coordinate where the top edge of the rendered text should appear.</param>
  /// <returns>Returns the width of the text rendered plus spacing so that you can use the value to calculate where
  /// more text can be rendered (by adding the returned width to the starting position).
  /// </returns>
  public override int Render(Surface dest, string text, int x, int y)
  { int start=x;
    if(widths==null)
    { if(bgColor != Color.Transparent) dest.Fill(new Rectangle(x, y, text.Length*(width+xAdd), lineSkip), bgColor);
      for(int i=0,add=width+xAdd; i<text.Length; i++)
      { int off = charset.IndexOf(text[i]);
        if(off!=-1) font.Blit(dest, new Rectangle(off*width, 0, width, Height), x, y);
        x += add;
      }
    }
    else
    { if(bgColor != Color.Transparent) dest.Fill(new Rectangle(x, y, CalculateSize(text).Width, lineSkip), bgColor);
      for(int i=0; i<text.Length; i++)
      { int off = charset.IndexOf(text[i]);
        if(off==-1) continue;
        font.Blit(dest, new Rectangle(offsets[off], 0, widths[off], Height), x, y);
        x += widths[off]+xAdd;
      }
    }
    return x-start;
  }

  /// <summary>See <see cref="Font.OnDisplayFormatChanged"/> for more details regarding this method.</summary>
  protected override void OnDisplayFormatChanged() { font = orig.IsCompatible() ? orig : orig.CloneDisplay(); }
  
  /// <summary>See <see cref="Font.Dispose(bool)"/> for more details regarding this method.</summary>
  protected override void Dispose(bool finalizing)
  { font.Dispose();
    orig.Dispose();
    base.Dispose(finalizing);
  }

  Surface font, orig;
  string charset;
  int[]  widths, offsets;
  int    width, lineSkip, xAdd;
  Color  bgColor;
}
#endregion

#region TrueType enums & class
/// <summary>This enum contains values which specify how font glyphs should be rendered.</summary>
public enum RenderStyle : byte
{ 
  /// <summary>The glyphs will be rendered solid using the foreground color, with no antialiasing.
  /// This method provides the highest performance, and crisp text.
  /// </summary>
  Solid,
  /// <summary>The glyphs will be rendered solid using the foreground color, and antialiased against a shade
  /// color. The font will be drawn without using alpha blending. This method provides higher performance than
  /// <see cref="Blended"/>, because the cost of the antialiasing is a one-time upfront cost. However, if rendered
  /// against a background other than the previously given shade color, or against backgrounds that are not a solid
  /// color, it may look bad. Being antialiased, the text produced with this method is smoother than that produced by
  /// <see cref="Solid"/>, but some people may consider it to be blurry.
  /// </summary>
  Shaded,
  /// <summary>The glyphs will be rendered solid using the foreground color, and antialiasing will be provided
  /// using an alpha channel. The font will then need to be alpha blended each time it's drawn, which means it
  /// has the lowest performance, but can be drawn against any background, even backgrounds that are not a solid
  /// color. Being antialiased, the text produced with this method is smoother than that produced by
  /// <see cref="Solid"/>, but some people may consider it to be blurry.
  /// </summary>
  Blended
}

/// <summary>This class provides support for rendering TrueType fonts.</summary>
/// <remarks>This class maintains a cache of character images, and can be a very heavy object which may consumes a
/// fair amount of memory. It can be run with a reduced cache, or with no cache, with a corresponding decrease
/// in performance, but even using it with no cache does not make it a lightweight object, however, since the shapes
/// of all the glyphs still need to be stored. For best results, keep just a single copy of each font in memory and
/// share it. See the <see cref="Font"/> base class for more details.
/// </remarks>
public class TrueTypeFont : StyledFont
{ 
  /// <summary>Initializes this font from a file on disk.</summary>
  /// <param name="filename">The path to the TrueType font file.</param>
  /// <param name="pointSize">The size of the font, in points (based on 72dpi).</param>
  public TrueTypeFont(string filename, int pointSize)
  { TTF.Initialize();
    try { font = TTF.OpenFont(filename, pointSize); }
    catch(NullReferenceException) { unsafe { font = new IntPtr(null); } }
    Init();
  }
  /// <summary>Initializes this font from a file on disk.</summary>
  /// <param name="filename">The path to the TrueType font file.</param>
  /// <param name="pointSize">The size of the font, in points (based on 72dpi).</param>
  /// <param name="fontIndex">Some fonts contain multiple faces. This parameter selects which face to use
  /// (zero-based).
  /// </param>
  public TrueTypeFont(string filename, int pointSize, int fontIndex)
  { try { font = TTF.OpenFontIndex(filename, pointSize, fontIndex); }
    catch(NullReferenceException) { unsafe { font = new IntPtr(null); } }
    Init();
  }
  /// <summary>Initializes this font from a stream containing TrueType font data.</summary>
  /// <param name="stream">The stream to read the font from. The stream must be seekable, with its entire set of
  /// data devoted to the font.
  /// </param>
  /// <param name="pointSize">The size of the font, in points (based on 72dpi).</param>
  /// <remarks>The stream will be closed after the font is loaded.</remarks>
  public TrueTypeFont(System.IO.Stream stream, int pointSize) : this(stream, pointSize, true) { }
  /// <summary>Initializes this font from a stream containing TrueType font data.</summary>
  /// <param name="stream">The stream to read the font from. The stream must be seekable, with its entire set of
  /// data devoted to the font.
  /// </param>
  /// <param name="pointSize">The size of the font, in points (based on 72dpi).</param>
  /// <param name="autoClose">If true, the stream will be closed after the font is loaded.</param>
  public TrueTypeFont(System.IO.Stream stream, int pointSize, bool autoClose)
  { SeekableStreamRWOps source = new SeekableStreamRWOps(stream, autoClose);
    unsafe { fixed(SDL.RWOps* ops = &source.ops) font = TTF.OpenFontRW(ops, 0, pointSize); }
    Init();
  }
  /// <summary>Initializes this font from a stream containing TrueType font data.</summary>
  /// <param name="stream">The stream to read the font from. The stream must be seekable, with its entire set of
  /// data devoted to the font.
  /// </param>
  /// <param name="pointSize">The size of the font, in points (based on 72dpi).</param>
  /// <param name="fontIndex">Some fonts contain multiple faces. This parameter selects which face to use
  /// (zero-based).
  /// </param>
  /// <remarks>The stream will be closed after the font is loaded.</remarks>
  public TrueTypeFont(System.IO.Stream stream, int pointSize, int fontIndex) : this(stream, pointSize, fontIndex, true) { }
  /// <summary>Initializes this font from a stream containing TrueType font data.</summary>
  /// <param name="stream">The stream to read the font from. The stream must be seekable, with its entire set of
  /// data devoted to the font.
  /// </param>
  /// <param name="pointSize">The size of the font, in points (based on 72dpi).</param>
  /// <param name="fontIndex">Some fonts contain multiple faces. This parameter selects which face to use
  /// (zero-based).
  /// </param>
  /// <param name="autoClose">If true, the stream will be closed after the font is loaded.</param>
  public TrueTypeFont(System.IO.Stream stream, int pointSize, int fontIndex, bool autoClose)
  { SeekableStreamRWOps source = new SeekableStreamRWOps(stream, autoClose);
    unsafe { fixed(SDL.RWOps* ops = &source.ops) font = TTF.OpenFontIndexRW(ops, 0, pointSize, fontIndex); }
    Init();
  }

  /// <summary>Gets/sets the font style that will be used to render the font.</summary>
  /// <remarks>Not all fonts support all font styles.</remarks>
  public override FontStyle Style
  { get { return fstyle; }
    set
    { if(fstyle==value) return;
      TTF.SetFontStyle(font, (int)value);
      fstyle = (FontStyle)TTF.GetFontStyle(font);
    }
  }
  /// <summary>Gets/sets the rendering style that will be used to render the font.</summary>
  public RenderStyle RenderStyle { get { return rstyle; } set { rstyle=value; } }
  /// <summary>Gets/sets the foreground color of the font.</summary>
  public override Color Color
  { get { return color; }
    set { if(color!=value) { color=value; sdlColor=new SDL.Color(value); } }
  }
  /// <summary>Gets/sets the background color of the font.</summary>
  /// <remarks>
  /// <para>The background color represents the color drawn behind the font. This is not necessarily the same
  /// as the color to antialias with when <see cref="RenderStyle"/> is set to
  /// <see cref="GameLib.Fonts.RenderStyle.Shaded"/>. If <see cref="ShadeColor"/> is not
  /// <see cref="System.Drawing.Color.Transparent"/>, then it will be used instead to shade the font.
  /// </para>
  /// <para>If <see cref="ShadeColorCloak"/> is true, setting this property will actually set the
  /// <see cref="ShadeColor"/> property, and this property will always be
  /// <see cref="System.Drawing.Color.Transparent"/>. This allows the shade color to be controlled by the background
  /// color, which is part of the standard <see cref="Font"/> interface.
  /// </para>
  /// </remarks>
  public override Color BackColor
  { get { return bgColor; }
    set
    { if(shadeCloak) ShadeColor=value;
      else if(bgColor!=value) { bgColor=value; sdlBgColor=new SDL.Color(value); }
    }
  }
  /// <summary>Gets/sets the color used to shade the font when <see cref="RenderStyle"/> is set to
  /// <see cref="GameLib.Fonts.RenderStyle.Shaded"/>.
  /// </summary>
  /// <remarks>If this property is set to <see cref="System.Drawing.Color.Transparent"/>, the <see cref="BackColor"/>
  /// will be used instead to shade the font. If both this and <see cref="BackColor"/> are set to
  /// <see cref="System.Drawing.Color.Transparent"/>, the font will be shaded against a black background.
  /// </remarks>
  public Color ShadeColor
  { get { return shadeColor; }
    set { if(shadeColor!=value) { shadeColor=value; sdlShadeColor=new SDL.Color(value); } }
  }
  /// <summary>Controls whether the <see cref="ShadeColor"/> property will cloak the <see cref="BackColor"/>
  /// property.
  /// </summary>
  /// <remarks>While set to true, altering the <see cref="BackColor"/> property will actually alter the
  /// <see cref="ShadeColor"/> property, and the <see cref="BackColor"/> property will always be
  /// <see cref="System.Drawing.Color.Transparent"/>. This allows the shade color to be controlled by the background
  /// color, which is part of the standard <see cref="Font"/> interface. If this is false, the default, a shaded
  /// font will behave in an intuitive way, but classes that use the generic <see cref="Font"/> interface will not
  /// be able to set <see cref="ShadeColor"/> to something different from the <see cref="BackColor"/>.
  /// </remarks>
  public bool ShadeColorCloak
  { get { return shadeCloak; }
    set
    { if(value!=shadeCloak)
      { if(value && shadeColor==Color.Transparent && bgColor!=Color.Transparent)
        { ShadeColor = bgColor;
          BackColor  = Color.Transparent;
        }
        shadeCloak=value;
      }
    }
  }

  /// <summary>Gets the height of the font, in pixels.</summary>
  public override int Height { get { return TTF.FontHeight(font); } }
  /// <summary>Gets the vertical offset that should be added to move the draw position to the next line. This is
  /// equivalent to the height of the font plus spacing between lines.
  /// </summary>
  public override int LineSkip { get { return TTF.FontLineSkip(font); } }
  /// <summary>Gets the maximum pixel ascent of all glyphs in the font.</summary>
  /// <remarks>The maximum pixel ascent can also be interpreted as the distance from the top of the font to the
  /// baseline.
  /// </remarks>
  public int Ascent { get { return TTF.FontAscent(font); } }
  /// <summary>Gets the maximum pixel descent of all glyphs in the font.</summary>
  /// <remarks>The maximum pixel descent can also be interpreted as the distance from the baseline to the bottom of
  /// the font.
  /// </remarks>
  public int Descent { get { return TTF.FontDescent(font); } }

  /// <summary>Gets the name of the font family.</summary>
  public string FamilyName { get { return TTF.FontFaceFamilyName(font); } }
  /// <summary>Gets the name of the font style.</summary>
  public string StyleName { get { return TTF.FontFaceStyleName(font);  } }

  /// <summary>Gets/sets the maximum size of the glyph cache.</summary>
  /// <value>The maximum number of glyphs allowed in the glyph cache.</value>
  /// <remarks>This class caches the most recently-used glyphs so they don't have to be recalculated every time
  /// they're used. This property can be set to zero to disable the cache entirely, but it's recommended that you
  /// keep it at at least 64. Even using no cache does not make this a lightweight object, since the shapes of
  /// all the glyphs still need to be stored.
  /// </remarks>
  public int MaxCacheSize
  { get { return cacheMax; }
    set
    { if(value<0) throw new ArgumentException("Cache size cannot be negative");
      if(value==0) ClearCache();
      else if(value<list.Count)
      { LinkedList.Node n=list.Tail, p;
        for(int i=0,num=list.Count-value; i<num; n=p,i++) { p=n.Prev; CacheRemove(n); }
      }
      cacheMax = value;
    }
  }
  
  /// <summary>Calculates the size of a string of text.</summary>
  /// <param name="text">The text to use for the calculation.</param>
  /// <returns>A <see cref="Size"/> that contains the amount of space required to render the given text with this
  /// font.
  /// </returns>
  /// <remarks>See <see cref="Font.CalculateSize"/> for more details regarding this method.</remarks>
  public override Size CalculateSize(string text)
  { /*int width, height;
    TTF.SizeUNICODE(font, text, out width, out height); -- DOESN'T WORK CONSISTENTLY */
    Size size = new Size(0, Height);
    for(int i=0; i<text.Length; i++) size.Width += GetChar(text[i]).Advance; // less correct, but more consistent
      //size.Width += i==0 && c.OffsetX<0 ? c.Width+c.OffsetX : i==text.Length-1 ? c.Width : c.Advance; // more correct, less consistent
    return size;
  }

  /// <summary>Calculates how many characters of the given string would fit into the given width.</summary>
  /// <param name="text">The text to test.</param>
  /// <param name="width">The available width, in pixels.</param>
  /// <returns>The number of characters that would fully fit within the given width.</returns>
  /// <remarks>See <see cref="Font.HowManyFit"/> for more details regarding this method.</remarks>
  public override int HowManyFit(string text, int width)
  { int pixels=0;
    for(int i=0; i<text.Length; i++)
    { CachedChar c = GetChar(text[i]);
      /*pixels += i>0 ? c.Advance : 0; // more correct, less consistent
      if(pixels+c.Width+(i==0 && c.OffsetX<0 ? c.OffsetX : 0) > width) return i;*/
      pixels += c.Advance;
      if(pixels>width) return i; // less correct, but more consistent
    }
    return text.Length;
  }

  /// <summary>Renders text at a given point.</summary>
  /// <param name="dest">The surface to render into.</param>
  /// <param name="text">The text to render.</param>
  /// <param name="x">The X coordinate where the left side of the rendered text should appear.</param>
  /// <param name="y">The Y coordinate where the top edge of the rendered text should appear.</param>
  /// <returns>Returns the width of the text rendered plus spacing so that you can use the value to calculate where
  /// more text can be rendered (by adding the returned width to the starting position).
  /// </returns>
  public override int Render(Surface dest, string text, int x, int y)
  { int start = x;
    if(bgColor != Color.Transparent)
    { int width=0;
      for(int i=0; i<text.Length; i++) width += GetChar(text[i]).Advance;
      dest.Fill(new Rectangle(x, y, width, LineSkip), bgColor);
    }
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

  protected class CachedChar : IDisposable
  { public CachedChar() { }
    public CachedChar(char c) { Char=c; }
    ~CachedChar() { Dispose(true); }
    public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }
    
    public Surface Surface;
    public int     OffsetX, OffsetY, Width, Advance;
    public char    Char;
    public bool    Compatible;
    internal CacheIndex Index;
    void Dispose(bool finalizing) { Surface.Dispose(); }
  }
  
  protected CachedChar GetChar(char c)
  { Color shade = shadeColor!=Color.Transparent ? shadeColor : bgColor!=Color.Transparent ? bgColor : Color.Black;
    CacheIndex ind = new CacheIndex(c, color, shade, fstyle, rstyle);
    LinkedList.Node node = (LinkedList.Node)tree[ind];
    CachedChar cc;
    if(node!=null)
    { cc = (CachedChar)node.Data;
      list.Remove(node);
      list.Prepend(node);
      goto done;
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
      { case RenderStyle.Solid: surface = TTF.RenderGlyph_Solid(font, c, sdlColor); break;
        case RenderStyle.Shaded:
          surface = TTF.RenderGlyph_Shaded(font, c, sdlColor,
                                           shadeColor!=Color.Transparent ? sdlShadeColor :
                                             bgColor!=Color.Transparent ? sdlBgColor : new SDL.Color(Color.Black));
          break;
        case RenderStyle.Blended: surface = TTF.RenderGlyph_Blended(font, c, sdlColor); break;
      }
      if(surface==null) TTF.RaiseError();
      cc.Surface = new Surface(surface, true);
      if(rstyle==RenderStyle.Shaded && shade!=Color.Transparent) cc.Surface.SetColorKey(shade);
    }

    if(cacheMax!=0)
    { while(list.Count>=cacheMax) CacheRemove(list.Tail);
      tree[ind]=list.Prepend(cc);
    }
    done:
    if(Video.Video.DisplaySurface!=null)
    { if(compatible==-1) compatible = cc.Surface.IsCompatible() ? 1 : 0;
      if(!cc.Compatible && compatible==0)
      { cc.Surface = cc.Surface.CloneDisplay();
        cc.Compatible = true;
      }
    }
    return cc;
  }

  /// <summary>See <see cref="Font.OnDisplayFormatChanged"/> for more details regarding this method.</summary>
  protected override void OnDisplayFormatChanged() { ClearCache(); compatible=-1; }

  /// <summary>Clears the glyph cache.</summary>
  protected void ClearCache()
  { tree.Clear();
    list.Clear();
  }

  /// <summary>See <see cref="Font.Dispose(bool)"/> for more details regarding this method.</summary>
  protected override void Dispose(bool finalizing)
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
    base.Dispose(finalizing);
  }
  
  internal struct CacheIndex : IComparable
  { public CacheIndex(char c, Color color, Color shadeColor, FontStyle fstyle, RenderStyle rstyle)
    { Char=c; Color=color; ShadeColor=shadeColor; FontStyle=fstyle; RenderStyle=rstyle;
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
      return this.ShadeColor.ToArgb().CompareTo(o.ShadeColor.ToArgb());
    }
    public Color       Color;
    public Color       ShadeColor;
    public char        Char;
    public FontStyle   FontStyle;
    public RenderStyle RenderStyle;
  }

  void CacheRemove(LinkedList.Node node)
  { CachedChar cc = (CachedChar)node.Data;
    cc.Dispose();
    tree.Remove(cc.Index);
    list.Remove(node);
  }

  void Init()
  { unsafe { if(font.ToPointer()==null) { TTF.Deinitialize(); TTF.RaiseError(); } }
    RenderStyle = RenderStyle.Solid;
    Color = Color.White;
    BackColor  = Color.Transparent;
    ShadeColor = Color.Transparent;
  }

  RedBlackTree tree = new RedBlackTree();
  LinkedList   list = new LinkedList();
  Color        color, bgColor, shadeColor;
  int          cacheMax=192, compatible=-1;
  FontStyle    fstyle;
  RenderStyle  rstyle;
  SDL.Color sdlColor, sdlBgColor, sdlShadeColor;
  IntPtr    font;
  bool      shadeCloak;
}
#endregion

} // namespace GameLib.Fonts