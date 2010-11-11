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

namespace GameLib
{

// NOTE: the layout of this structure must match the layout of the SDL color structure (4 bytes: red, green, blue, alpha). we
// could use a different layout, but then we'd need to translate between them, so we might as well use the SDL color layout.

/// <summary>Represents an RGBA color (i.e. a color composed of red, green, and blue components, and an alpha value).</summary>
public struct Color
{
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

  /// <summary>Converts this color into a string of the form "#rrggbb" or "#rrggbbaa" where "rr", "gg", "bb", and "aa" represent
  /// the red, green, blue, and alpha values respectively, in hexadecimal. The alpha value is only given if it is not equal to
  /// 255. For example, <see cref="Color.Yellow"/> would be represented as "#ffff00" and <see cref="Color.Empty"/> as
  /// "#00000000".
  /// </summary>
  public unsafe string ToHexString()
  {
    const string hexChars = "0123456789abcdef";
    bool isOpaque = IsOpaque; // guard against a highly unlikely, but possible race condition that could lead to buffer overrun
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

  /// <summary>Converts this color into an HSV (hue-saturation-value, also known as HSB [hue-saturation-brightness]) color.</summary>
  /// <param name="hue">A variable that will receive the hue, as a value from 0 to 1, representing degrees from 0 to 360.</param>
  /// <param name="saturation">A variable that will receive the saturation, from 0 to 1.</param>
  /// <param name="value">A variable that will receive the value (brightness), from 0 to 1.</param>
  public void ToHSV(out float hue, out float saturation, out float value)
  {
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
      saturation = delta / max;
      float inverseDelta = (1f/6) / delta, deltaR = max - r, deltaG = max - g, deltaB = max - b, h;

      if(deltaR == 0) h = (deltaB-deltaG)*inverseDelta;
      else if(deltaG == 0) h = (deltaR-deltaB)*inverseDelta + 1f/3;
      else h = (deltaG-deltaR)*inverseDelta + 2f/3;

      if(h >= 1) h -= 1;
      else if(h < 0) h += 1;

      hue = h;
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

  /// <summary>Blends two colors together.</summary>
  /// <returns>A <see cref="Color"/> that is the average of <paramref name="a"/> and <paramref name="b"/>.</returns>
  public static Color Blend(Color a, Color b)
  {
    return new Color((byte)((a.red+b.red)/2), (byte)((a.green+b.green)/2), (byte)((a.blue+b.blue)/2),
                     (byte)((a.alpha+b.alpha)/2));
  }

  /// <summary>Blends two colors together using the given blending factor, which must be from 0 to 1. If
  /// <paramref name="blendFactor"/> is close to zero, a color close to <paramref name="a"/> is returned, and if
  /// <paramref name="blendFactor"/> is close to one, a color close to <paramref name="b"/> is returned.
  /// </summary>
  public static Color Blend(Color a, Color b, float blendFactor)
  {
    if(blendFactor < 0 || blendFactor > 1) throw new ArgumentOutOfRangeException();
    byte alpha = a.alpha == b.alpha ? a.alpha : Blend(a.alpha, b.alpha, blendFactor); // alpha is likely to be the same
    return new Color(Blend(a.red, b.red, blendFactor), Blend(a.green, b.green, blendFactor), Blend(a.blue, b.blue, blendFactor),
                     alpha);
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

  /// <summary>Returns a new <see cref="Color"/> constructed from the given HSV (hue-saturation-value, also known as HSB
  /// [hue-saturation-brightness]) color.
  /// </summary>
  /// <param name="hue">The hue, from 0 to 1, representing 0 to 360 degrees. Values outside [0,1) are also accepted.</param>
  /// <param name="saturation">The saturation, from 0 to 1.</param>
  /// <param name="value">The value (brightness), from 0 to 1.</param>
  public static Color FromHSV(float hue, float saturation, float value)
  {
    if(saturation < 0 || saturation > 1 || value < 0 || value > 1) throw new ArgumentOutOfRangeException();

    if(hue >= 1)
    {
      hue -= (int)hue; // e.g. 2.25 -> 2.25 - 2 == 0.25, 2 -> 2 - 2 == 0
    }
    else if(hue < 0)
    {
      hue -= (int)hue - 1; // e.g. -2.25 -> -2.25 - -3 == -2.25 + 3 == 0.75, BUT -2 -> -2 - -3 == -2 + 3 == 1
      if(hue == 1) hue = 0; // if the hue was already an integer, then it'll have become equal to 1, so make it zero
    }

    float hueFace = hue*6, v1 = value * (1 - saturation);
    int hueFaceInt = (int)hueFace;
    float hueFaceFraction = hueFace - hueFaceInt;
    float v2 = value * (1 - saturation * hueFaceFraction), v3 = value * (1 - saturation * (1 - hueFaceFraction));
    float r, g, b;

    switch(hueFaceInt)
    {
      case 0: r = value; g = v3; b = v1; break;
      case 1: r = v2; g = value; b = v1; break;
      case 2: r = v1; g = value; b = v3; break;
      case 3: r = v1; g = v2; b = value; break;
      case 4: r = v3; g = v1; b = value; break;
      default: r = value; g = v1; b = v2; break;
    }

    return new Color(ScaleAndRound(r), ScaleAndRound(g), ScaleAndRound(b));
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
      if(str[0] == '#')
      {
        uint rgba = 0;
        if(str.Length == 7 || str.Length == 9)
        {
          for(int i=str.Length-2; i > 0; i-=2)
          {
            int highNibble = GetNibble(str[i]), lowNibble = GetNibble(str[i+1]);
            if(highNibble == -1 || lowNibble == -1) goto failed;
            rgba = (rgba<<8) | ((uint)highNibble<<4) | (uint)lowNibble;
          }
          if(str.Length == 7) rgba |= 0xFF000000;
        }
        else if(str.Length == 4 || str.Length == 5)
        {
          for(int i=str.Length-1; i > 0; i--)
          {
            int nibble = GetNibble(str[i]);
            if(nibble == -1) goto failed;
            rgba = (rgba<<8) | ((uint)nibble<<4) | (uint)nibble;
          }
          if(str.Length == 4) rgba |= 0xFF000000;
        }
        else
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

  static byte Blend(byte a, byte b, float blendFactor)
  {
    return (byte)(a + (b-a)*blendFactor + 0.5f);
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
}

} // namespace GameLib