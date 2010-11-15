/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2010 Adam Milazzo

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
using System.Collections.Generic;
using KnownColor = System.Drawing.KnownColor;
using SysColor   = System.Drawing.Color;

// TODO: see if we can eliminate the type initialization check every time a static member is accessed (due to the struct missing
// the beforefieldinit flag because it has a static constructor), perhaps using a trick similar to the one we used for
// the Timing class

namespace GameLib
{

#region BlendMode
/// <summary>Represents different ways that colors can be blended.</summary>
public enum BlendMode
{
  /// <summary>The blend color is used.</summary>
  Normal,
  /// <summary>The two colors are added, with channels saturating at white. (This blend mode is also called "linear dodge".)</summary>
  Add,
  /// <summary>Not quite the opposite of <see cref="Add"/>, this causes dark colors to be truncated instead of light ones.
  /// Negative intensities produce black. (This blend mode is also called "linear burn".)
  /// </summary>
  Subtract,
  /// <summary>The resulting color is the absolute value of the difference between the two colors. Blending with black produces
  /// no change, while blending with white inverts the image.
  /// </summary>
  Difference,
  /// <summary>In a way, the opposite of <see cref="Difference"/>. Colors further apart are brighter, but colors closer together
  /// can be either brighter or darker depending on whether they themselves are bright or dark. Dark and light colors close
  /// together become dark, while medium-valued colors close together become bright.
  /// </summary>
  Negation,
  /// <summary>Similar to <see cref="Difference"/>, but with lower contrast.  Blending with black produces no change, while
  /// blending with white inverts the image.
  /// </summary>
  Exclusion,
  /// <summary>The two colors are multiplied together. Blending with black results in black, while blending with white produces
  /// no change.
  /// </summary>
  Multiply,
  /// <summary>The opposite of <see cref="Multiply"/>. Multiplies the inverses of the two colors. Blending with black produces
  /// no change, while blending with white results in white.
  /// </summary>
  Screen,
  /// <summary>Combines <see cref="Multiply"/> and <see cref="Screen"/> such that the blend channel is multiplied with the base
  /// channel if the base channel is less than medium value (i.e. dark), and screened with the base channel if it's
  /// not less than medium (i.e. light).
  /// </summary>
  Overlay,
  /// <summary>Performs a bitwise exclusive or of the two colors.</summary>
  Xor,
  /// <summary>Darkens the base color by increasing the contrast with the blend color. Blending with white has no effect.</summary>
  ColorBurn,
  /// <summary>Lightens the base color by decreasing the contrast with the blend color. Blending with black has no effect.</summary>
  ColorDodge,
  /// <summary>Burns or dodges the color such that the base channel is burned if the blend channel is less than medium value and
  /// dodged otherwise. (The burn and dodge are not exactly the same as <see cref="ColorBurn"/> and <see cref="ColorDodge"/>.)
  /// The effect is similar to shining a diffused spotlight on the image.
  /// </summary>
  SoftLight,
  /// <summary>Combines <see cref="Multiply"/> and <see cref="Screen"/> such that the base channel is multiplied if the blend
  /// channel is less than medium value and screened otherwise. The effect is similar to shining a harsh spotlight on the image.
  /// </summary>
  HardLight,
  /// <summary>Burns or dodges the color (by increasing or decreasing the contrast) such that the base channel is burned if the
  /// blend channel is less than medium value and dodged otherwise. (The burn and dodge are not exactly the same as
  /// <see cref="ColorBurn"/> and <see cref="ColorDodge"/>.)
  /// </summary>
  VividLight,
  /// <summary>Burns or dodges the color (by increasing or decreasing the brightness) such that the base channel is burned if the
  /// blend channel is less than medium value and dodged otherwise. (The burn and dodge are not exactly the same as
  /// <see cref="ColorBurn"/> and <see cref="ColorDodge"/>.)
  /// </summary>
  LinearLight,
  /// <summary>If the blend channel is less than medium value, then the blend channel is used only if the base channel is
  /// lighter. Otherwise, if the blend channel is not less than medium value, then the blend channel is used only if the base
  /// channel is darker.
  /// </summary>
  PinLight,
  /// <summary>Adds the two colors together. Channels that are not fully saturated are reduced to zero. The result is a color
  /// that's one of the eight colors that can be formed by combining the fully bright and saturated primary colors.
  /// </summary>
  HardMix,
  /// <summary>Takes the brighter of each color channel.</summary>
  Lighten,
  /// <summary>Takes the darker of each color channel.</summary>
  Darken,
  /// <summary>Nonlinearly blends between the base and blend colors.</summary>
  Reflect,
  /// <summary>The opposite of <see cref="Reflect"/>.</summary>
  Glow,
  /// <summary>A variation of <see cref="Reflect"/>.</summary>
  Freeze,
  /// <summary>The opposite of <see cref="Freeze"/>.</summary>
  Heat,
  // NOTE: the following blending modes must remain grouped together and must not be reordered
  /// <summary>Preserves the HCL chroma and luma of the base color while adopting the hue of the blend color.</summary>
  Hue,
  /// <summary>Preserves the HCL hue and luma of the base color while adopting the chroma of the blend color.</summary>
  Chroma,
  /// <summary>Preserves the HCL hue and chroma of the base color while adopting the luma of the blend color. This blend mode is
  /// also called "luminosity".
  /// </summary>
  Luma,
  /// <summary>Preserves the HCL luma of the base color while adopting  the hue and chroma of the blend color.</summary>
  Color,
  /// <summary>Preserves the HSV saturation and value (brightness) of the base color while adopting the hue of the blend color.</summary>
  HSVHue,
  /// <summary>Preserves the HSV hue and value (brightness) of the base color while adopting the saturation of the blend color.</summary>
  HSVSaturation,
  /// <summary>Preserves the HSV hue and saturation of the base color while adopting the value (brightness) of the blend color.</summary>
  Brightness,
  /// <summary>Preserves the HSV value (brightness) of the base color while adopting the hue and saturation of the blend color.</summary>
  HSVColor,
  /// <summary>Preserves the HSL saturation and lightness of the base color while adopting the hue of the blend color.</summary>
  HSLHue,
  /// <summary>Preserves the HSL hue and lightness of the base color while adopting the saturation of the blend color.</summary>
  HSLSaturation,
  /// <summary>Preserves the HSL hue and saturation of the base color while adopting the lightness of the blend color.</summary>
  Lightness,
  /// <summary>Preserves the HSL lightness of the base color while adopting the hue and saturation of the blend color.</summary>
  HSLColor,
}
#endregion

/// <summary>Represents an RGBA color (i.e. a color composed of red, green, and blue components, and an alpha value).</summary>
// NOTE: the layout of this structure must match the layout of the SDL color structure (4 bytes: red, green, blue, alpha). we
// could use a different layout, but then we'd need to translate between them, so we might as well use the SDL color layout.
public struct Color
{
  /// <summary>Initializes a new, gray, opaque <see cref="Color"/> from a brightness value.</summary>
  public Color(byte brightness)
  {
    red   = green = blue = brightness;
    alpha = 255;
  }

  /// <summary>Initializes a new, gray <see cref="Color"/> from a brightness value and an alpha value.</summary>
  public Color(byte brightness, byte alpha)
  {
    red = green = blue = brightness;
    this.alpha = alpha;
  }

  /// <summary>Initializes a new, opaque <see cref="Color"/> from red, green, and blue components.</summary>
  public Color(byte red, byte green, byte blue)
  {
    this.red   = red;
    this.green = green;
    this.blue  = blue;
    this.alpha = 255;
  }

  /// <summary>Initializes a new <see cref="Color"/> from red, green, and blue components, and an alpha value.</summary>
  public Color(byte red, byte green, byte blue, byte alpha)
  {
    this.red   = red;
    this.green = green;
    this.blue  = blue;
    this.alpha = alpha;
  }

  /// <summary>Initializes a new <see cref="Color"/> by using a new alpha value with an existing color.</summary>
  public Color(Color baseColor, byte alpha)
  {
    red        = baseColor.red;
    green      = baseColor.green;
    blue       = baseColor.blue;
    this.alpha = alpha;
  }

  /// <summary>Initializes a new <see cref="Color"/> using an integer value obtained from <see cref="Value"/>.</summary>
  [CLSCompliant(false)]
  public Color(uint value) : this()
  {
    Value = value;
  }

  static Color()
  {
    // create maps of named color to value and value to named color
    string[] names = Enum.GetNames(typeof(KnownColor));
    KnownColor[] values = (KnownColor[])Enum.GetValues(typeof(KnownColor));
    namedColors = new Dictionary<string, uint>(names.Length);
    colorNames = new Dictionary<uint, string>(names.Length);
    for(int i=0; i<names.Length; i++)
    {
      SysColor color = SysColor.FromKnownColor(values[i]);
      uint colorValue = ((Color)color).Value;
      namedColors.Add(names[i].ToLowerInvariant(), colorValue);
      if(!color.IsSystemColor) colorNames[colorValue] = names[i]; // we don't want system colors because they name UI components
    }

    // create a square root table used for the soft light blend mode. the values are scaled to the range of 0-64770, so the
    // square root of 64 is not stored as 8, but rather as 32448, since sqrt(64.0/255)*64770 ~= 32448. this gives us more
    // resolution while simultaneously simplifying the math in the soft blend mode
    sqrtTable = new ushort[256];
    for(int i=0; i<sqrtTable.Length; i++) sqrtTable[i] = (ushort)Math.Round(Math.Sqrt(i/255.0)*(255*254));

    // initialize the predefined color values
    Empty   = new Color();
    Black   = new Color(0, 0, 0);
    Blue    = new Color(0, 0, 255);
    Cyan    = new Color(0, 255, 255);
    Gray    = new Color(128, 128, 128);
    Green   = new Color(0, 128, 0);
    Lime    = new Color(0, 255, 0);
    Magenta = new Color(255, 0, 255);
    Maroon  = new Color(128, 0, 0);
    Navy    = new Color(0, 0, 128);
    Olive   = new Color(128, 128, 0);
    Purple  = new Color(128, 0, 128);
    Red     = new Color(255, 0, 0);
    Silver  = new Color(192, 192, 192);
    Teal    = new Color(0, 128, 128);
    Yellow  = new Color(255, 255, 0);
    White   = new Color(255, 255, 255);
  }

  /// <summary>Gets the alpha value of the color, from 0 (transparent) to 255 (opaque).</summary>
  public byte Alpha
  {
    get { return alpha; }
  }

  /// <summary>Gets the blue component of the color, from 0 to 255.</summary>
  public byte B
  {
    get { return blue; }
  }

  /// <summary>Gets the green component of the color, from 0 to 255.</summary>
  public byte G
  {
    get { return green; }
  }

  /// <summary>Gets whether the color has a zero value for all components, and for the alpha value. This is true if the structure
  /// is uninitialized.
  /// </summary>
  public bool IsEmpty
  {
    get { return Value == 0; }
  }

  /// <summary>Gets whether the color is opaque (i.e. it has an alpha value of 255).</summary>
  public bool IsOpaque
  {
    get { return alpha == 255; }
  }

  /// <summary>Gets whether the color is transparent (i.e. it has an alpha value of 0).</summary>
  public bool IsTransparent
  {
    get { return alpha == 0; }
  }

  /// <summary>Gets the name of the color, or null if it is not a known color name.</summary>
  public string Name
  {
    get
    {
      string name;
      colorNames.TryGetValue(Value, out name);
      return name;
    }
  }

  /// <summary>Gets the red component of the color, from 0 to 255.</summary>
  public byte R
  {
    get { return red; }
  }


  /// <summary>Gets the color as an integer value that can be passed to <see cref="Color(uint)"/>. This value should not be
  /// serialized, as it is not guaranteed to be portable across different machine architectures.
  /// </summary>
  [CLSCompliant(false)]
  public unsafe uint Value
  {
    get
    {
      // this was measured as being faster than combining the components manually
      fixed(Color* p=&this) return *(uint*)p;
    }
    private set
    {
      fixed(Color* p=&this) *(uint*)p = value;
    }
  }

  /// <summary>Determines whether an object equals this color.</summary>
  public override bool Equals(object obj)
  {
    return obj is Color && Value == ((Color)obj).Value;
  }

  /// <summary>Gets a hash code for this color.</summary>
  public override int GetHashCode()
  {
    return (int)Value;
  }

  /// <summary>Converts the color to grayscale, using the standard gamma correction (30% red, 59% green, and 11% blue).</summary>
  public Color ToGrayscale()
  {
    // the integer version is faster than the floating point one
    return new Color((byte)((red*30 + green*59 + blue*11 + 50) / 100), alpha);
  }

  /// <summary>Converts this color into a string of the form "#rrggbb" or "#rrggbbaa" where "rr", "gg", "bb", and "aa" represent
  /// the red, green, blue, and alpha values respectively, in hexadecimal. The alpha value is only given if it is not equal to
  /// 255. For example, <see cref="Color.Yellow"/> would be represented as "#ffff00" and <see cref="Color.Empty"/> as
  /// "#00000000".
  /// </summary>
  public unsafe string ToHexString()
  {
    const string hexChars = "0123456789abcdef";
    bool isOpaque = IsOpaque; // guard against IsOpaque being changed by another thread, causing buffer overrun
    char* chars = stackalloc char[isOpaque ? 7 : 9];
    chars[0] = '#';
    chars[1] = hexChars[red>>4];
    chars[2] = hexChars[red&0xF];
    chars[3] = hexChars[green>>4];
    chars[4] = hexChars[green&0xF];
    chars[5] = hexChars[blue>>4];
    chars[6] = hexChars[blue&0xF];
    if(!isOpaque)
    {
      chars[7] = hexChars[alpha>>4];
      chars[8] = hexChars[alpha&0xF];
    }
    return new string(chars, 0, isOpaque ? 7 : 9);
  }

  /// <summary>Converts this color into an HCL (hue-chroma-luma) color.</summary>
  /// <param name="hue">A variable that will receive the hue, as a value from 0 to 1, representing degrees from 0 to 360.</param>
  /// <param name="chroma">A variable that will receive the chroma, from 0 to 1.</param>
  /// <param name="luma">A variable that will receive the luma, from 0 to 1.</param>
  /// <include file="../documentation.xml" path="//Color/HCL/*"/>
  public void ToHCL(out float hue, out float chroma, out float luma)
  {
    // http://www.wikimedia.org/en/wiki/HSL_and_HSV#Hue_and_chroma and http://www.wikimedia.org/en/wiki/HSL_and_HSV#Lightness
    float r = red*(1f/255), g = green*(1f/255), b = blue*(1f/255), min = r, max = r;

    if(g < min) min = g;
    else max = g;

    if(b < min) min = b;
    else if(b > max) max = b;

    float delta = max - min;
    chroma = delta;
    luma   = 0.299f*r + 0.587f*g + 0.114f*b;

    if(delta == 0)
    {
      hue = 0;
    }
    else
    {
      float inverseDelta = (1f/6) / delta, h;
      if(r == max) h = (g-b)*inverseDelta;
      else if(g == max) h = (b-r)*inverseDelta + 1f/3;
      else h = (r-g)*inverseDelta + 2f/3;

      if(h >= 1) h -= 1;
      else if(h < 0) h += 1;

      hue = h;
    }
  }

  /// <summary>Converts this color into an HSL (hue-saturation-lightness) color.</summary>
  /// <param name="hue">A variable that will receive the hue, as a value from 0 to 1, representing degrees from 0 to 360.</param>
  /// <param name="saturation">A variable that will receive the saturation, from 0 to 1. Note that this is not the same as the
  /// saturation component of the HSV (hue-saturation-value, or HSB [hue-saturation-brightness]) color space.
  /// </param>
  /// <param name="lightness">A variable that will receive the lightness, from 0 to 1.</param>
  /// <include file="../documentation.xml" path="//Color/HSLHSV/*"/>
  public void ToHSL(out float hue, out float saturation, out float lightness)
  {
    // http://www.wikimedia.org/en/wiki/HSL_and_HSV#Lightness and http://www.wikimedia.org/en/wiki/HSL_and_HSV#Saturation
    float r = red*(1f/255), g = green*(1f/255), b = blue*(1f/255), min = r, max = r;

    if(g < min) min = g;
    else max = g;

    if(b < min) min = b;
    else if(b > max) max = b;

    float maxPlusMin = max + min;
    lightness = maxPlusMin * 0.5f;

    float delta = max - min;
    if(delta == 0)
    {
      hue        = 0;
      saturation = 0;
    }
    else
    {
      float inverseDelta = (1f/6) / delta, v;
      if(r == max) v = (g-b)*inverseDelta;
      else if(g == max) v = (b-r)*inverseDelta + 1f/3;
      else v = (r-g)*inverseDelta + 2f/3;

      if(v >= 1) v -= 1;
      else if(v < 0) v += 1;
      hue = v;

      v = delta / (1 - Math.Abs(maxPlusMin-1));
      if(v > 1) v = 1; // it's possible that it can go slightly over, due to floating point error
      saturation = v;
    }
  }

  /// <summary>Converts this color into an HSV (hue-saturation-value, also known as HSB [hue-saturation-brightness]) color.</summary>
  /// <param name="hue">A variable that will receive the hue, as a value from 0 to 1, representing degrees from 0 to 360.</param>
  /// <param name="saturation">A variable that will receive the saturation, from 0 to 1. Note that this is not the same as the
  /// saturation component of  the HSL (hue-saturation-lightness) color space.
  /// </param>
  /// <param name="value">A variable that will receive the value (brightness), from 0 to 1.</param>
  /// <include file="../documentation.xml" path="//Color/HSLHSV/*"/>
  public void ToHSV(out float hue, out float saturation, out float value)
  {
    // http://www.wikimedia.org/en/wiki/HSL_and_HSV#Lightness and http://www.wikimedia.org/en/wiki/HSL_and_HSV#Saturation
    float r = red*(1f/255), g = green*(1f/255), b = blue*(1f/255), min = r, max = r;

    if(g < min) min = g;
    else max = g;

    if(b < min) min = b;
    else if(b > max) max = b;

    value = max;

    float delta = max - min;
    if(delta == 0)
    {
      hue        = 0;
      saturation = 0;
    }
    else
    {
      float inverseDelta = (1f/6) / delta, h;
      if(r == max) h = (g-b)*inverseDelta;
      else if(g == max) h = (b-r)*inverseDelta + 1f/3;
      else h = (r-g)*inverseDelta + 2f/3;

      if(h >= 1) h -= 1;
      else if(h < 0) h += 1;

      hue        = h;
      saturation = delta / max;
    }
  }

  /// <summary>Converts this color into a human-readable string.</summary>
  public override string ToString()
  {
    return Name ?? ToHexString();
  }

  /// <summary>Compares two colors to see if they're equal.</summary>
  public static unsafe bool operator==(Color a, Color b)
  {
    return *(uint*)&a == *(uint*)&b;
  }

  /// <summary>Compares two colors to see if they're unequal.</summary>
  public static unsafe bool operator!=(Color a, Color b)
  {
    return *(uint*)&a != *(uint*)&b;
  }

  /// <summary>Implicitly converts a <see cref="SysColor"/> into the equivalent <see cref="Color"/>.</summary>
  public static implicit operator Color(SysColor color)
  {
    return new Color(color.R, color.G, color.B, color.A);
  }

  /// <summary>Implicitly converts a <see cref="Color"/> into an equivalent <see cref="SysColor"/>.</summary>
  public static implicit operator SysColor(Color color)
  {
    return SysColor.FromArgb(color.alpha, color.red, color.green, color.blue);
  }

  /// <summary>Blends two colors together, returning a color that is the average of <paramref name="a"/> and
  /// <paramref name="b"/>. The alpha channels are also averaged.
  /// </summary>
  public static Color Blend(Color a, Color b)
  {
    return new Color((byte)((a.red+b.red)/2), (byte)((a.green+b.green)/2), (byte)((a.blue+b.blue)/2),
                     (byte)((a.alpha+b.alpha)/2));
  }

  /// <summary>Blends two colors together using the given blending factor, which must be from 0 to 1. If
  /// <paramref name="blendFactor"/> is close to zero, a color close to <paramref name="a"/> is returned, and if
  /// <paramref name="blendFactor"/> is close to one, a color close to <paramref name="b"/> is returned.
  /// The alpha channels are also blended.
  /// </summary>
  public static Color Blend(Color a, Color b, float blendFactor)
  {
    if(blendFactor < 0 || blendFactor > 1) throw new ArgumentOutOfRangeException();
    return Blend(a, b, (byte)(blendFactor*255+0.5f));
  }

  /// <summary>Blends two colors together using the given alpha value. If <paramref name="blendAlpha"/> is close to 0, a color
  /// close to <paramref name="a"/> is returned, and if <paramref name="blendAlpha"/> is close to 255, a color close to
  /// <paramref name="b"/> is returned. The alpha channels are also blended.
  /// </summary>
  public static Color Blend(Color a, Color b, byte blendAlpha)
  {
    byte alpha = a.alpha == b.alpha ? a.alpha : Blend(a.alpha, b.alpha, blendAlpha); // alpha is likely to be the same
    return new Color(Blend(a.red, b.red, blendAlpha), Blend(a.green, b.green, blendAlpha), Blend(a.blue, b.blue, blendAlpha),
                     alpha);
  }

  /// <summary>Blends two colors together using the given <see cref="BlendMode"/>. The resulting color always has the alpha value
  /// of the base color.
  /// </summary>
  public static Color Blend(Color baseColor, Color blendColor, BlendMode blendMode)
  {
    Color color;
    if(blendColor.alpha == 0)
    {
      color = baseColor;
    }
    else
    {
      // blend the two colors together ignoring their alpha values to get the result color
      if(blendMode < BlendMode.Hue) // if we're using channel-based methods
      {
        color = new Color(Blend(baseColor.red, blendColor.red, blendMode), Blend(baseColor.green, blendColor.green, blendMode),
                          Blend(baseColor.blue, blendColor.blue, blendMode), baseColor.alpha);
      }
      else // if we're using whole-color-based methods
      {
        float baseH, baseS, baseL, blendH, blendS, blendL;
        if(blendMode < BlendMode.HSVHue) // if we're using the HCL color model (so S=chroma and L=luma)...
        {
          baseColor.ToHCL(out baseH, out baseS, out baseL);
          blendColor.ToHCL(out blendH, out blendS, out blendL);

          switch(blendMode)
          {
            case BlendMode.Hue: color = Color.FromHCL(blendH, baseS, baseL); break;
            case BlendMode.Chroma: color = Color.FromHCL(baseH, blendS, baseL); break;
            case BlendMode.Luma: color = Color.FromHCL(baseH, baseS, blendL); break;
            default: /*case BlendMode.Color:*/ color = Color.FromHCL(blendH, blendS, baseL); break;
          }
        }
        else if(blendMode < BlendMode.HSLHue) // we're using the HSV color model (so S=saturation and L=value)
        {
          baseColor.ToHSV(out baseH, out baseS, out baseL);
          blendColor.ToHSV(out blendH, out blendS, out blendL);

          switch(blendMode)
          {
            case BlendMode.HSVHue: color = Color.FromHSV(blendH, baseS, baseL); break;
            case BlendMode.HSVSaturation: color = Color.FromHSV(baseH, blendS, baseL); break;
            case BlendMode.Brightness: color = Color.FromHSV(baseH, baseS, blendL); break;
            default: /*case BlendMode.HSVColor:*/ color = Color.FromHSV(blendH, blendS, baseL); break;
          }
        }
        else // we're using the HSL color model (so S=saturation and L=lightness)
        {
          baseColor.ToHSL(out baseH, out baseS, out baseL);
          blendColor.ToHSL(out blendH, out blendS, out blendL);

          switch(blendMode)
          {
            case BlendMode.HSLHue: color = Color.FromHSL(blendH, baseS, baseL); break;
            case BlendMode.HSLSaturation: color = Color.FromHSL(baseH, blendS, baseL); break;
            case BlendMode.Lightness: color = Color.FromHSL(baseH, baseS, blendL); break;
            case BlendMode.HSLColor: color = Color.FromHSL(blendH, blendS, baseL); break;
            default: throw new ArgumentException();
          }
        }

        // the color calculated above has an alpha of 255, but we want it to have the alpha of the base color. if the base color
        // has a different alpha, and the blend color is opaque (so we won't enter the alpha blending block below, which takes
        // care of it for us), then update the alpha value
        if(baseColor.alpha != 255 && blendColor.alpha == 255) color = new Color(color, baseColor.alpha);
      }

      // now apply the blend color's alpha value to blend the result color with the base color. in 
      if(blendColor.alpha != 255)
      {
        color = new Color(Blend(baseColor.red, color.red, blendColor.alpha),
                          Blend(baseColor.green, color.green, blendColor.alpha),
                          Blend(baseColor.blue, color.blue, blendColor.alpha), baseColor.alpha);
      }
    }

    return color;
  }

  /// <summary>Returns a new <see cref="Color"/> constructed from the given HCL (hue-chroma-luma) color. Not all HCL colors can
  /// be represented in the RGB gamut. Colors outside the gamut will be clipped by adjusting the chroma downward.
  /// </summary>
  /// <param name="hue">The hue, from 0 to 1, representing 0 to 360 degrees. Values outside [0,1) are also accepted.</param>
  /// <param name="chroma">The chroma, from 0 to 1.</param>
  /// <param name="luma">The luma, from 0 to 1.</param>
  /// <include file="../documentation.xml" path="//Color/HCL/*"/>
  public static Color FromHCL(float hue, float chroma, float luma)
  {
    if(chroma < 0 || chroma > 1 || luma < 0 || luma > 1) throw new ArgumentOutOfRangeException();

    if(hue >= 1)
    {
      hue -= (int)hue; // e.g. 2.25 -> 2.25 - 2 == 0.25, 2 -> 2 - 2 == 0
    }
    else if(hue < 0)
    {
      hue -= (int)hue - 1; // e.g. -2.25 -> -2.25 - -3 == -2.25 + 3 == 0.75, BUT -2 -> -2 - -3 == -2 + 3 == 1
      if(hue == 1) hue = 0; // if the hue was already an integer, then it'll have become equal to 1, so make it zero
    }

    float hueFace = hue*6;
    int hueFaceInt = (int)hueFace, term = (hueFaceInt+1) & ~1;
    float hff = (hueFaceInt&1) == 0 ? hueFace - term : term - hueFace;
    bool clipped = false;

    recalculate:
    float x = chroma * hff, r, g, b;
    switch(hueFaceInt)
    {
      case 0: r = chroma; g = x; b = 0; break;
      case 1: r = x; g = chroma; b = 0; break;
      case 2: r = 0; g = chroma; b = x; break;
      case 3: r = 0; g = x; b = chroma; break;
      case 4: r = x; g = 0; b = chroma; break;
      default: r = chroma; g = 0; b = x; break;
    }

    x  = luma - 0.299f*r - 0.587f*g - 0.114f*b;
    r += x;
    g += x;
    b += x;

    if(!clipped) // if we haven't checked whether the color is in-gamut, do so
    {
      float min = r, max = r;
      if(g < min) min = g;
      else max = g;
      if(b < min) min = b;
      else if(b > max) max = b;

      // if it's out of gamut, adjust the chroma and retry. in all cases, the chroma will be adjusted downwards.
      // if we take v1=1, v2=hff, and v3=0, then (assuming hueFaceInt==0, for example)
      //
      // r = luma + chroma*v1 - chroma*v1*0.3 - chroma*v2*0.59 - chroma*v3*0.11 =
      //     luma + chroma*(v1 - 0.3*v1 - 0.59*v2 - 0.11*v3) =
      //     luma + chroma*(0.7*v1 - 0.59*v2 - 0.11*v3)
      //
      // since none of v1, v2, or v3 depend on luma or chroma, so we simply can consider that r,g,b = luma + chroma*whatever.
      // then we'll take the maximum and minimum r,g,b values and see if they're out of bounds.
      //
      // IN CASE MAX > 1:
      // luma + chroma*whatever = max   (where max > 1)
      //
      // now we need to calculate a scaling factor that we can multiply chroma by so that luma + chroma*whatever = 1.
      // in other words, we need a factor such that the difference between the cases when the factor is and is not applied is
      // equal to the difference between the current value (max) and the desired value (1)
      //
      // chroma*whatever - scale*chroma*whatever = max - 1
      // chroma*whatever*(1 - scale) = max - 1
      // 1 - scale = (max - 1) / (chroma*whatever)
      // scale - 1 = (1 - max) / (chroma*whatever)
      // scale = (1 - max) / (chroma*whatever) + 1
      //
      // IN CASE MIN < 0
      // luma + chroma*whatever = min   (where min < 0)
      //
      // we need to calculate a scaling factor that we can multiply chroma by so that luma + chroma*whatever = 0.
      // in other words, we need a factor such that the difference between the cases when the factor is and is not applied is
      // equal to the difference between the current value (min) and the desired value (0)
      //
      // chroma*whatever - scale*chroma*whatever = min
      // chroma*whatever*(1 - scale) = min
      // 1 - scale = min / (chroma*whatever)
      // scale - 1 = -min / (chroma*whatever)
      // scale = -min / (chroma*whatever) + 1

      if(max > 1) chroma *= (1 - max) / (max - luma) + 1;
      else if(min < 0) chroma *= -min / (min - luma) + 1;

      clipped = true; // remember that we clipped to the upper bound
      goto recalculate; // and recalculate r, g, and b
    }

    // if we adjusted the chroma in an attempt to clip the color to the RGB gamut, the clipping may not be exact, due to floating
    // point error (i.e. they may be slightly less than 0 or greater than 1), but we don't need to clamp the values because they
    // should be close enough to not cause the result to be out of bounds

    return new Color(ScaleAndRound(r), ScaleAndRound(g), ScaleAndRound(b));
  }

  /// <summary>Returns a new <see cref="Color"/> constructed from the given HSL (hue-saturation-lightness) color.</summary>
  /// <param name="hue">The hue, from 0 to 1, representing 0 to 360 degrees. Values outside [0,1) are also accepted.</param>
  /// <param name="saturation">The saturation, from 0 to 1. Note that this is not the same as the saturation component of the HSV
  /// (hue-saturation-value, or HSB [hue-saturation-brightness]) color space.
  /// </param>
  /// <param name="lightness">The lightness, from 0 to 1.</param>
  /// <include file="../documentation.xml" path="//Color/HSLHSV/*"/>
  public static Color FromHSL(float hue, float saturation, float lightness)
  {
    if(saturation < 0 || saturation > 1 || lightness < 0 || lightness > 1) throw new ArgumentOutOfRangeException();
    float chroma = saturation * (1 - Math.Abs(lightness*2-1));
    return FromHS(hue, chroma, lightness - chroma*0.5f);
  }

  /// <summary>Returns a new <see cref="Color"/> constructed from the given HSV (hue-saturation-value, also known as HSB
  /// [hue-saturation-brightness]) color.
  /// </summary>
  /// <param name="hue">The hue, from 0 to 1, representing 0 to 360 degrees. Values outside [0,1) are also accepted.</param>
  /// <param name="saturation">The saturation, from 0 to 1. Note that this is not the same as the saturation component of the HSL
  /// (hue-saturation-lightness) color space.
  /// </param>
  /// <param name="value">The value (brightness), from 0 to 1.</param>
  /// <include file="../documentation.xml" path="//Color/HSLHSV/*"/>
  public static Color FromHSV(float hue, float saturation, float value)
  {
    if(saturation < 0 || saturation > 1 || value < 0 || value > 1) throw new ArgumentOutOfRangeException();
    float chroma = saturation*value;
    return FromHS(hue, chroma, value - chroma);
  }

  /// <summary>Returns a new <see cref="Color"/> constructed from the given <see cref="KnownColor"/> value.</summary>
  public static Color FromKnownColor(KnownColor color)
  {
    return SysColor.FromKnownColor(color);
  }

  /// <summary>Returns a new, opaque <see cref="Color"/> constructed from the given red, green, and blue values, which must be
  /// from 0 to 255.
  /// </summary>
  public static Color FromRGB(int red, int green, int blue)
  {
    return FromRGBA(red, green, blue, 255);
  }

  /// <summary>Returns a new <see cref="Color"/> constructed from the given red, green, blue, and alpha values, which must be
  /// from 0 to 255.
  /// </summary>
  public static Color FromRGBA(int red, int green, int blue, int alpha)
  {
    if(((red|green|blue|alpha) & ~0xFF) != 0) throw new ArgumentOutOfRangeException(); // ensure all values are from 0-255
    return new Color((byte)red, (byte)green, (byte)blue, (byte)alpha);
  }

  /// <summary>Returns a new gray <see cref="Color"/> constructed from the given brightness value, which must be from 0 to 255.</summary>
  public static Color FromValue(int brightness)
  {
    return FromValue(brightness, 255);
  }

  /// <summary>Returns a new gray <see cref="Color"/> constructed from the given brightness and alpha values, which must be from
  /// 0 to 255.
  /// </summary>
  public static Color FromValue(int brightness, int alpha)
  {
    if(((brightness|alpha) & ~0xFF) != 0) throw new ArgumentOutOfRangeException(); // ensure all values are from 0-255
    return new Color((byte)brightness, (byte)alpha);
  }

  /// <summary>Parses a string (as returned from <see cref="ToString"/> or <see cref="ToHexString"/>) back into a
  /// <see cref="Color"/>.
  /// </summary>
  /// <remarks>The method can handle named colors and hex strings in the following formats: #rgb, #rgba, #rrggbb, and #rrggbbaa,
  /// where r, g, b, and a represent hex digits. The formats with only 3 or 4 characters work such that #abc == #aabbcc.
  /// </remarks>
  public static Color Parse(string str)
  {
    if(str == null) throw new ArgumentNullException();
    Color color;
    if(!TryParse(str, out color)) throw new FormatException("\"" + str + "\" is not a valid color.");
    return color;
  }

  /// <summary>Attempts to parse a string (as returned from <see cref="ToString"/> or <see cref="ToHexString"/>) back into a
  /// <see cref="Color"/>. True is returned if the parse was successful and false if not.
  /// </summary>
  /// <remarks>The method can handle named colors and hex strings in the following formats: #rgb, #rgba, #rrggbb, and #rrggbbaa,
  /// where r, g, b, and a represent hex digits. The formats with only 3 or 4 characters work such that #abc == #aabbcc.
  /// </remarks>
  public static bool TryParse(string str, out Color color)
  {
    if(!string.IsNullOrEmpty(str))
    {
      if(str[0] == '#') // if it starts with a '#' character, try parsing it has a hex string
      {
        uint rgba = 0;
        if(str.Length == 7 || str.Length == 9) // if it uses two characters per byte...
        {
          for(int i=str.Length-2; i > 0; i-=2)
          {
            int highNibble = GetNibble(str[i]), lowNibble = GetNibble(str[i+1]);
            if(highNibble == -1 || lowNibble == -1) goto failed;
            rgba = (rgba<<8) | ((uint)highNibble<<4) | (uint)lowNibble;
          }
          if(str.Length == 7) rgba |= 0xFF000000;
        }
        else if(str.Length == 4 || str.Length == 5) // if it uses one character per byte...
        {
          for(int i=str.Length-1; i > 0; i--)
          {
            int nibble = GetNibble(str[i]);
            if(nibble == -1) goto failed;
            rgba = (rgba<<8) | ((uint)nibble<<4) | (uint)nibble;
          }
          if(str.Length == 4) rgba |= 0xFF000000;
        }
        else // it's not a valid length
        {
          goto failed;
        }

        color = new Color((byte)rgba, (byte)(rgba>>8), (byte)(rgba>>16), (byte)(rgba>>24));
        return true;
      }
      else
      {
        uint value;
        if(!namedColors.TryGetValue(str.ToLowerInvariant(), out value)) goto failed;
        color = new Color(value);
        return true;
      }
    }

    failed:
    color = Color.Empty;
    return false;
  }

  /// <summary>Gets an empty color (#00000000).</summary>
  public static readonly Color Empty;
  /// <summary>Gets the color black (#000000).</summary>
  public static readonly Color Black;
  /// <summary>Gets the color blue (#0000ff).</summary>
  public static readonly Color Blue;
  /// <summary>Gets the color cyan (#00ffff).</summary>
  public static readonly Color Cyan;
  /// <summary>Gets the color gray (#808080).</summary>
  public static readonly Color Gray;
  /// <summary>Gets the color green (#008000).</summary>
  public static readonly Color Green;
  /// <summary>Gets the color lime (#00ff00).</summary>
  public static readonly Color Lime;
  /// <summary>Gets the color magenta (#ff00ff).</summary>
  public static readonly Color Magenta;
  /// <summary>Gets the color maroon (#800000).</summary>
  public static readonly Color Maroon;
  /// <summary>Gets the color navy (#000080).</summary>
  public static readonly Color Navy;
  /// <summary>Gets the color olive (#808000).</summary>
  public static readonly Color Olive;
  /// <summary>Gets the color purple (#800080).</summary>
  public static readonly Color Purple;
  /// <summary>Gets the color red (#ff0000).</summary>
  public static readonly Color Red;
  /// <summary>Gets the color silver (#c0c0c0).</summary>
  public static readonly Color Silver;
  /// <summary>Gets the color teal (#008080).</summary>
  public static readonly Color Teal;
  /// <summary>Gets the color yellow (#ffff00).</summary>
  public static readonly Color Yellow;
  /// <summary>Gets the color white (#ffffff).</summary>
  public static readonly Color White;

  byte red, green, blue, alpha;

  static byte Blend(byte a, byte b, byte blendAlpha)
  {
    // we want a + round((b-a)*alpha). with byte values, that would be a + ((b-a)*blendAlpha+128)/255. but to avoid the division,
    // we solve (255*255+128+n)/256 == 255.5 and get n=255. so we add 128+255=383 and divide by 256
    return (byte)(a + ((b-a)*blendAlpha+383)/256);
  }

  static byte Blend(byte baseValue, byte blendValue, BlendMode blendMode)
  {
    // in various places we're supposed to multiply by 255 and divide by a value that could be up to 255. for instance, a*255/b.
    // to avoid the multiplication, we'll do either a*256/b or (a*256-1)/b (since the *256 can be converted into a shift). the
    // former is out of bounds if a==255 and the latter is out of bounds if a==0, so we choose which one to use depending on how
    // we intend to perform clamping
    //
    // similarly, in some places we want to divide by 255, for instance a*b/255. to avoid the division, we instead do
    // (a*b+255)/256 (since the /256 can be converted into a shift). we don't simply do a*b/256 because if a=b=255, then the
    // result is not equal to 255 as it should be. adding 255 fixes this.

    int value;
    switch(blendMode)
    {
      case BlendMode.Add: // a+b
        value = baseValue + blendValue;
        goto clampMax;

      case BlendMode.ColorBurn: // 1 - (1-a)/b
        // we want the result in [0,255] rather than [0,1], so we should multiply by 255. but instead of that, we'll use the
        // trick of multiplying by 256 and subtracting one, described above
        if(blendValue == 0)
        {
          return (byte)0;
        }
        else
        {
          value = 255 - ((255-baseValue)*256 - 1)/blendValue;
          goto clampMin;
        }

      case BlendMode.ColorDodge: // a / (1-b)
        // we want the result in [0,255] rather than [0,1], so we should multiply by 255. but instead of that, we'll use the
        // trick of multiplying by 256, described above
        if(blendValue == 255)
        {
          return (byte)255;
        }
        else
        {
          value = baseValue*256 / (255-blendValue);
          goto clampMax;
        }

      case BlendMode.Darken: // min(a,b)
        return baseValue < blendValue ? baseValue : blendValue;

      case BlendMode.Difference: // abs(a-b)
        value = blendValue - baseValue;
        if(value < 0) value = -value;
        return (byte)value;

      case BlendMode.Exclusion: // a + b - 2ab
        // with byte values, 2ab is in [0,130050]. we want it in the range of [0,510], so we have to divide by 255. or, we can
        // simply not multiply by 2 in the first place and get ab in [0,65025]. then we need to divide by 127.5. we don't want to
        // do that either, so we solve (65025+n)/128 == 510, and get n=255. so we can add 255 and divide by 128
        return (byte)(baseValue + blendValue - (baseValue*blendValue+255)/128);

      case BlendMode.Freeze: // 1 - (1-a)^2/b
        if(blendValue == 0)
        {
          return 0;
        }
        else
        {
          value = 255 - baseValue;
          value = 255 - value*value/blendValue;
          goto clampMin;
        }

      case BlendMode.Glow: // b^2 / (1-a)
        if(baseValue == 255)
        {
          return 255;
        }
        else
        {
          value = blendValue*blendValue / (255-baseValue);
          goto clampMax;
        }

      case BlendMode.HardLight: // 2ab [if b<0.5], 1 - 2*(1-a)*(1-b) [otherwise]
        // with byte values, 2ab is in [0,32385], but we want it in the range of [0,255]. so we should divide by 127, but to
        // avoid the division we'll instead solve (32385+n)/128 == 255 and get n==255.
        if(blendValue < 128) return (byte)((baseValue*blendValue+255) >> 7);
        else return (byte)(255 - (((255-baseValue)*(255-blendValue)+255) >> 7));

      case BlendMode.HardMix: // 0 [if a+b<1], 1 [otherwise]
        return baseValue+blendValue < 255 ? (byte)0 : (byte)255;

      case BlendMode.Heat: // 1 - (1-b)^2/a
        if(baseValue == 0)
        {
          return 0;
        }
        else
        {
          value = 255 - blendValue;
          value = 255 - value*value/baseValue;
          goto clampMin;
        }

      case BlendMode.Lighten: // max(a,b)
        return baseValue > blendValue ? baseValue : blendValue;

      case BlendMode.LinearLight: // a+2b-1 [if b<0.5], a + 2(b-0.5) [otherwise]
        if(blendValue < 128)
        {
          value = baseValue + 2*blendValue - 255;
          goto clampMin;
        }
        else
        {
          value = baseValue + 2*(blendValue-128);
          goto clampMax;
        }

      case BlendMode.Multiply: // ab
        return (byte)((baseValue*blendValue+255) >> 8);

      case BlendMode.Negation:
        value = 255 - baseValue - blendValue;
        if(value < 0) value = -value;
        return (byte)(255 - value);

      case BlendMode.Normal:
        return blendValue;

      case BlendMode.Overlay: // 2ab [if a<0.5], 1 - 2*(1-a)*(1-b) [otherwise]
        // the +255 is explained in the note for HardLight
        if(baseValue < 128) return (byte)((baseValue*blendValue + 255) >> 7);
        else return (byte)(255 - (((255-baseValue)*(255-blendValue) + 255) >> 7));

      case BlendMode.PinLight: // min(a,2b) [if b<0.5], max(a,2(1-b)) [otherwise]
        if(blendValue < 128)
        {
          value = 2*blendValue;
          return (byte)(baseValue < value ? baseValue : value);
        }
        else
        {
          value = 2*(blendValue-128);
          return (byte)(baseValue > value ? baseValue : value);
        }

      case BlendMode.Reflect: // a^2/(1-b)
        if(blendValue == 255)
        {
          return 255;
        }
        else
        {
          value = baseValue*baseValue / (255-blendValue);
          goto clampMax;
        }

      case BlendMode.Screen: // 1 - (1-a)*(1-b)
        return (byte)(255 - (((255-baseValue)*(255-blendValue) + 255) >> 8));

      case BlendMode.SoftLight: // 2ab + a^2(1-2b) [if b<0.5], sqrt(a)*(2b-1) - 2a*(1-b) [otherwise]
        if(blendValue < 128)
        {
          // with byte values, 2ab is within [0,64770] and a^2(1-2b) is within [0,16581375]. we need the two terms to contribute
          // equally, so we solve (16581375+n)/256 == 64770 and get n == -255. then, the formula as a whole should be within
          // [0,64770]. but we want the whole formula to be within [0,255], so we solve (64770+n)/256 == 255 and get n==510.
          return (byte)((2*baseValue*blendValue + ((baseValue*baseValue*(255-2*blendValue)-255)/256) + 510)/256);
        }
        else
        {
          // with byte values, sqrt(a)*(2b-1) is within [0,16516350] and 2a*(1-b) is within [0,64770]. we need the two terms to
          // contribute equally, so instead of multiplying the second term by 2, we multiply by 2*255. then the total expression
          // is within [0,16516350]. we need to scale this to [0,255], so we should divide by 64770. it'd be faster to use a
          // power of two, so we'll divide by 65536 (2^16) instead, and solve (16516350+n)/65536 == 255, and find that
          // n == 195330
          return (byte)((sqrtTable[baseValue]*(2*blendValue-255) + 510*baseValue*(255-blendValue) + 195330) >> 16);
        }

      case BlendMode.Subtract: // a+b-1
        value = baseValue + blendValue - 255;
        goto clampMin;

      case BlendMode.VividLight: // a/(1-2b) [if b<0.5], 1 - (1-a)/(2(b-0.5)) [otherwise]
        if(blendValue < 128)
        {
          value = 255 - (((255-baseValue)*256) - 1)/(blendValue*2+1);
          goto clampMin;
        }
        else
        {
          value = baseValue*256 / (255-2*(blendValue-128));
          goto clampMax;
        }

      case BlendMode.Xor:
        return (byte)(baseValue ^ blendValue);

      default: throw new ArgumentException();
    }

    clampMin:
    return value < 0 ? (byte)0 : (byte)value;

    clampMax:
    return value > 255 ? (byte)255 : (byte)value;
  }

  /// <summary>Creates a color, given its hue, chroma, and minimum component value.</summary>
  static Color FromHS(float hue, float chroma, float min)
  {
    if(hue >= 1)
    {
      hue -= (int)hue;
    }
    else if(hue < 0)
    {
      hue -= (int)hue - 1;
      if(hue == 1) hue = 0; // if the hue was already an integer, then it'll have become equal to 1, so make it zero
    }

    // optimize calculation of hff = (1 - abs(hueFace%2 - 1))
    // 0   -> 0   = H-0
    // 0.1 -> 0.1 = H-0
    // 1   -> 1   = 2-H
    // 1.1 -> 0.9 = 2-H
    // 2   -> 0   = H-2
    // 2.1 -> 0.1 = H-2
    // 3   -> 1   = 4-H
    // 3.1 -> 0.9 = 4-H
    // etc...
    float hueFace = hue*6;
    int hueFaceInt = (int)hueFace, term = (hueFaceInt+1) & ~1;
    float v = chroma * ((hueFaceInt&1) == 0 ? hueFace - term : term - hueFace), r, g, b;
    switch(hueFaceInt)
    {
      case 0: r = chroma; g = v; b = 0; break;
      case 1: r = v; g = chroma; b = 0; break;
      case 2: r = 0; g = chroma; b = v; break;
      case 3: r = 0; g = v; b = chroma; break;
      case 4: r = v; g = 0; b = chroma; break;
      default: r = chroma; g = 0; b = v; break;
    }

    return new Color(ScaleAndRound(r+min), ScaleAndRound(g+min), ScaleAndRound(b+min));
  }

  static int GetNibble(char c)
  {
    if(c >= '0' && c <= '9') return c - '0';
    else if(c >= 'a' && c <= 'f') return c - ('a' - 10);
    else if(c >= 'A' && c <= 'F') return c - ('A' - 10);
    else return -1;
  }

  static byte ScaleAndRound(float f)
  {
    return (byte)(f*255+0.5f);
  }

  readonly static Dictionary<string, uint> namedColors;
  readonly static Dictionary<uint, string> colorNames;
  readonly static ushort[] sqrtTable;
}

} // namespace GameLib
