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

namespace GameLib.Mathematics
{

/// <summary>This class provides some useful constants for math operations.</summary>
public sealed class MathConst
{ private MathConst() { }

  /// <summary>A value that can be used to convert degrees to radians.</summary>
  /// <remarks>If you multiply a degree value by this constant, it will be converted to radians.</remarks>
  public const double DegreesToRadians = Math.PI/180;
  /// <summary>A value that can be used to convert radians to degrees.</summary>
  /// <remarks>If you multiply a radian value by this constant, it will be converted to degrees.</remarks>
  public const double RadiansToDegrees = 180/Math.PI;
  /// <summary>This value is two times pi, the number of radians in a circle.</summary>
  public const double TwoPI = Math.PI*2;

  /// <summary>Returns the absolute value of a <see cref="Fixed32"/>.</summary>
  /// <param name="val">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the absolute value.</returns>
  public static Fixed32 Abs(Fixed32 val) { return val.val<0 ? new Fixed32((uint)-val.val) : val; }
  /// <summary>Returns the arc-cosine of a <see cref="Fixed32"/>.</summary>
  /// <param name="val">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the arc-cosine.</returns>
  public static Fixed32 Acos(Fixed32 val) { return new Fixed32(Math.Acos(val.ToDouble())); }
  /// <summary>Returns the arc-sine of a <see cref="Fixed32"/>.</summary>
  /// <param name="val">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the arc-sine.</returns>
  public static Fixed32 Asin(Fixed32 val) { return new Fixed32(Math.Asin(val.ToDouble())); }
  /// <summary>Returns the arc-tangent of a <see cref="Fixed32"/>.</summary>
  /// <param name="val">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the arc-tangent.</returns>
  public static Fixed32 Atan(Fixed32 val) { return new Fixed32(Math.Atan(val.ToDouble())); }
  /// <summary>Returns the ceiling of a <see cref="Fixed32"/>.</summary>
  /// <param name="val">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the smallest whole number greater than or equal to the given
  /// number.
  /// </returns>
  public static Fixed32 Ceiling(Fixed32 val) { return val.Ceiling; }
  /// <summary>Returns the cosine of a <see cref="Fixed32"/>.</summary>
  /// <param name="val">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the cosine.</returns>
  public static Fixed32 Cos(Fixed32 val) { return new Fixed32(Math.Cos(val.ToDouble())); }
  /// <summary>Returns the floor of a <see cref="Fixed32"/>.</summary>
  /// <param name="val">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the largest whole number less than or equal to the given
  /// number.
  /// </returns>
  public static Fixed32 Floor(Fixed32 val) { return val.Floor; }
  /// <summary>Returns the rounded value of a <see cref="Fixed32"/>.</summary>
  /// <param name="val">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the rounded value.</returns>
  /// <remarks>This method performs banker's rounding, so values with a fractional part of exactly 0.5 will be
  /// rounded towards the nearest even number, or towards zero.
  /// </remarks>
  public static Fixed32 Round(Fixed32 val) { return val.Rounded; }
  /// <summary>Returns the sine of a <see cref="Fixed32"/>.</summary>
  /// <param name="val">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the sine.</returns>
  public static Fixed32 Sin(Fixed32 val) { return new Fixed32(Math.Sin(val.ToDouble())); }
  /// <summary>Returns the square root of a <see cref="Fixed32"/>.</summary>
  /// <param name="val">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the square root.</returns>
  public static Fixed32 Sqrt(Fixed32 val) { return new Fixed32(Math.Sqrt(val.ToDouble())); }
  /// <summary>Returns the tangent of a <see cref="Fixed32"/>.</summary>
  /// <param name="val">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the tangent.</returns>
  public static Fixed32 Tan(Fixed32 val) { return new Fixed32(Math.Tan(val.ToDouble())); }
  /// <summary>Returns the truncated value of a <see cref="Fixed32"/>.</summary>
  /// <param name="val">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the value truncated towards zero.</returns>
  public static Fixed32 Truncate(Fixed32 val) { return val.Truncated; }

  /// <summary>Returns the absolute value of a <see cref="Fixed64"/>.</summary>
  /// <param name="val">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the absolute value.</returns>
  public static Fixed64 Abs(Fixed64 val) { return val.val<0 ? new Fixed64(-val.val) : val; }
  /// <summary>Returns the arc-cosine of a <see cref="Fixed64"/>.</summary>
  /// <param name="val">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the arc-cosine.</returns>
  public static Fixed64 Acos(Fixed64 val) { return new Fixed64(Math.Acos(val.ToDouble())); }
  /// <summary>Returns the arc-sine of a <see cref="Fixed64"/>.</summary>
  /// <param name="val">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the arc-sine.</returns>
  public static Fixed64 Asin(Fixed64 val) { return new Fixed64(Math.Asin(val.ToDouble())); }
  /// <summary>Returns the arc-tangent of a <see cref="Fixed64"/>.</summary>
  /// <param name="val">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the arc-tangent.</returns>
  public static Fixed64 Atan(Fixed64 val) { return new Fixed64(Math.Atan(val.ToDouble())); }
  /// <summary>Returns the ceiling of a <see cref="Fixed64"/>.</summary>
  /// <param name="val">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the smallest whole number greater than or equal to the given
  /// number.
  /// </returns>
  public static Fixed64 Ceiling(Fixed64 val) { return val.Ceiling; }
  /// <summary>Returns the cosine of a <see cref="Fixed64"/>.</summary>
  /// <param name="val">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the cosine.</returns>
  public static Fixed64 Cos(Fixed64 val) { return new Fixed64(Math.Cos(val.ToDouble())); }
  /// <summary>Returns the floor of a <see cref="Fixed64"/>.</summary>
  /// <param name="val">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the largest whole number less than or equal to the given
  /// number.
  /// </returns>
  public static Fixed64 Floor(Fixed64 val) { return val.Floor; }
  /// <summary>Returns the rounded value of a <see cref="Fixed64"/>.</summary>
  /// <param name="val">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the rounded value.</returns>
  /// <remarks>This method performs banker's rounding, so values with a fractional part of exactly 0.5 will be
  /// rounded towards the nearest even number, or towards zero.
  /// </remarks>
  public static Fixed64 Round(Fixed64 val) { return val.Rounded; }
  /// <summary>Returns the sine of a <see cref="Fixed64"/>.</summary>
  /// <param name="val">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the sine.</returns>
  public static Fixed64 Sin(Fixed64 val) { return new Fixed64(Math.Sin(val.ToDouble())); }
  /// <summary>Returns the square root of a <see cref="Fixed64"/>.</summary>
  /// <param name="val">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the square root.</returns>
  public static Fixed64 Sqrt(Fixed64 val) { return new Fixed64(Math.Sqrt(val.ToDouble())); }
  /// <summary>Returns the tangent of a <see cref="Fixed64"/>.</summary>
  /// <param name="val">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the tangent.</returns>
  public static Fixed64 Tan(Fixed64 val) { return new Fixed64(Math.Tan(val.ToDouble())); }
  /// <summary>Returns the truncated value of a <see cref="Fixed64"/>.</summary>
  /// <param name="val">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the value truncated towards zero.</returns>
  public static Fixed64 Truncate(Fixed64 val) { return val.Truncated; }
}

/// <summary>This class provides some helpful mathematical functions.</summary>
public sealed class GLMath
{ private GLMath() { }
  /// <include file="documentation.xml" path="//Mathematics/GLMath/AngleBetween/*"/>
  public static double AngleBetween(TwoD.Point start, TwoD.Point end) { return (end-start).Angle; }
  /// <include file="documentation.xml" path="//Mathematics/GLMath/AngleBetween/*"/>
  public static double AngleBetween(System.Drawing.Point start, System.Drawing.Point end)
  { return (new TwoD.Point(end)-new TwoD.Point(start)).Angle;
  }

  /// <summary>Performs integer division that rounds towards lower numbers rather than towards zero.</summary>
  /// <param name="numerator">The numerator.</param>
  /// <param name="denominator">The denominator.</param>
  /// <returns><paramref name="numerator"/> divided by <paramref name="denominator"/>, rounded towards lower numbers
  /// rather than towards zero.
  /// </returns>
  public static int FloorDiv(int numerator, int denominator)
  { return (numerator<0 ? (numerator-denominator+1) : numerator) / denominator;
  }
}

#region Fixed32
/// <summary>This class provides a fixed-point numeric type with 32 bits of storage in a 16.16 configuration.</summary>
/// <remarks>
/// <para>Floating point math on modern systems is very fast, and I wouldn't recommend using this fixed-point math
/// class for speed. The primary benefit of fixed-point math is that it provides consistency and reliability. Floating
/// point math loses precision when dealing with larger numbers, and the results of arithmetic operations are not
/// always consistent due to precision mismatch between operands. Fixed-point math eliminates these inconsistencies.
/// </para>
/// <para>This class provides 16 bits for the whole part and 16 bits for the fractional part, so the total range is
/// approximately -32768 to 32767.99998.
/// </para>
/// </remarks>
[Serializable, System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct Fixed32 : IFormattable, IComparable, IConvertible
{ 
  /// <summary>Initializes this fixed-point class from a floating point number.</summary>
  /// <param name="value">A floating point number from which the fixed-point number will be initialized.</param>
  /// <remarks>Due to the greater range and potential precision of a 64-bit double, the value passed may not be
  /// able to be accurately represented.
  /// </remarks>
  public Fixed32(double value) { val=FromDouble(value); }
  /// <summary>Initializes this fixed-point class from an integer.</summary>
  /// <param name="value">An integer from which the fixed-point number will be initialized.</param>
  /// <remarks>Due to the greater range of a 32-bit integer, the value passed may not be accurately represented.</remarks>
  public Fixed32(int value) { val=value<<16; }
  internal Fixed32(uint value) { val=(int)value; } // ugly, but 'int' was already taken

  /// <summary>Gets this number's absolute value.</summary>
  /// <value>A new fixed-point number containing the absolute value of this number.</value>
  public Fixed32 Abs { get { return val<0 ? new Fixed32((uint)-val) : this; } }
  /// <summary>Gets this number's ceiling.</summary>
  /// <value>A new fixed-point number containing the smallest whole number greater than or equal to the current value.</value>
  public Fixed32 Ceiling { get { return new Fixed32((uint)((val+(OneVal-1)) & Trunc)); } }
  /// <summary>Gets this number's ceiling.</summary>
  /// <value>A new fixed-point number containing the largest whole number less than or equal to the current value.</value>
  public Fixed32 Floor { get { return new Fixed32((uint)(val&Trunc)); } }
  /// <summary>Gets this number's value, rounded.</summary>
  /// <remarks>This method performs banker's rounding, so values with a fractional part of exactly 0.5 will be
  /// rounded towards the nearest even number, or towards zero.
  /// </remarks>
  public Fixed32 Rounded
  { get
    { ushort fp = (ushort)val;
      if(fp<0x8000) return new Fixed32((uint)(val&Trunc));
      else if(fp>0x8000 || (val&OneVal)!=0) return new Fixed32((uint)((val+OneVal)&Trunc));
      else return new Fixed32((uint)(val&Trunc));
    }
  }
  /// <summary>Gets this number's square root.</summary>
  /// <value>A new fixed-point number containing the square root of the current value.</value>
  public Fixed32 Sqrt { get { return new Fixed32(Math.Sqrt(ToDouble())); } }
  /// <summary>Gets this number's value, truncated towards zero.</summary>
  public Fixed32 Truncated { get { return new Fixed32((uint)((val<0 ? val+(OneVal-1) : val)&Trunc)); } }
  /// <summary>Returns true if this object is equal to the given object.</summary>
  /// <param name="obj">The object to compare against.</param>
  /// <returns>True if <paramref name="obj"/> is a <see cref="Fixed32"/> and has the same value as this one.</returns>
  public override bool Equals(object obj)
  { if(!(obj is Fixed32)) return false;
    return val == ((Fixed32)obj).val;
  }
  /// <summary>Returns a hash code for this value.</summary>
  /// <returns>A hash code for this value.</returns>
  public override int GetHashCode() { return val; }
  /// <summary>Converts this <see cref="Fixed32"/> to a <see cref="Fixed64"/>.</summary>
  /// <returns>A <see cref="Fixed64"/> containing the same value.</returns>
  public Fixed64 ToFixed64() { return new Fixed64(((long)(val&Trunc)<<16) | (uint)((ushort)val<<16)); }
  /// <summary>Converts this fixed-point number to a floating-point number.</summary>
  /// <returns>The double value closest to this fixed-point number.</returns>
  public double ToDouble() { return (val>>16) + (ushort)val*0.0000152587890625; } // 1 / (1<<16)
  /// <summary>Returns the integer portion of the fixed-point number.</summary>
  /// <returns>The integer portion of the fixed-point number.</returns>
  public int ToInt()
  { int ret = val>>16;
    if(ret<0 && (ushort)val!=0) ret++;
    return ret;
  }
  /// <summary>Converts this fixed-point number into a string.</summary>
  /// <returns>A string representing this fixed-point number.</returns>
  public override string ToString() { return ToString(null, null); }
  /// <include file="documentation.xml" path="//Mathematics/Fixed/ToString1/*"/>
  public string ToString(string format) { return ToString(format, null); }
  /// <summary>Converts a string into a fixed-point value.</summary>
  /// <param name="s">The string to convert.</param>
  /// <returns>A <see cref="Fixed32"/> containing the closest value.</returns>
  public static Fixed32 Parse(string s)
  { int pos = s.ToLower().IndexOf('e');
    if(pos==-1)
    { pos = s.IndexOf('.');
      if(pos==-1)
      { pos = s.IndexOf('/');
        if(pos==-1) return new Fixed32((uint)(short.Parse(s)<<16)); // integer
        else return new Fixed32((uint)((int.Parse(s.Substring(0, pos))<<16) + int.Parse(s.Substring(pos+1)))); // raw
      }
      else // number with fractional part
      { int val, frac;
        if(pos==0) val=0;
        else
        { string ws = s.Substring(0, pos);
          val = ws.Length==0 ? 0 : short.Parse(ws)<<16;
        }
        frac = (int)(double.Parse(s.Substring(pos))*65536.0+0.5);
        return new Fixed32((uint)(val<0 ? val-frac : val+frac));
      }
    }
    else return new Fixed32(double.Parse(s)); // scientific notation
  }

  #region Arithmetic operators
  public static Fixed32 operator-(Fixed32 val) { return new Fixed32((uint)-val.val); }

  public static Fixed32 operator+(Fixed32 lhs, int rhs) { return new Fixed32((uint)(lhs.val+(rhs<<16))); }
  public static Fixed32 operator-(Fixed32 lhs, int rhs) { return new Fixed32((uint)(lhs.val-(rhs<<16))); }
  public static Fixed32 operator*(Fixed32 lhs, int rhs) { return new Fixed32((uint)(lhs.val*rhs)); }
  public static Fixed32 operator/(Fixed32 lhs, int rhs) { return new Fixed32((uint)(lhs.val/rhs)); }

  public static Fixed32 operator+(Fixed32 lhs, double rhs) { return new Fixed32((uint)(lhs.val+FromDouble(rhs))); }
  public static Fixed32 operator-(Fixed32 lhs, double rhs) { return new Fixed32((uint)(lhs.val-FromDouble(rhs))); }
  public static Fixed32 operator*(Fixed32 lhs, double rhs) { return new Fixed32((uint)(((long)lhs.val*FromDouble(rhs))>>16)); }
  public static Fixed32 operator/(Fixed32 lhs, double rhs) { return new Fixed32((uint)(((long)lhs.val<<16)/FromDouble(rhs))); }

  public static Fixed32 operator+(int lhs, Fixed32 rhs) { return new Fixed32((uint)((lhs<<16)+rhs.val)); }
  public static Fixed32 operator-(int lhs, Fixed32 rhs) { return new Fixed32((uint)((lhs<<16)-rhs.val)); }
  public static Fixed32 operator*(int lhs, Fixed32 rhs) { return new Fixed32((uint)(lhs*rhs.val)); }
  public static Fixed32 operator/(int lhs, Fixed32 rhs) { return new Fixed32((uint)(((long)lhs<<32)/rhs.val)); }

  public static Fixed32 operator+(double lhs, Fixed32 rhs) { return new Fixed32((uint)(FromDouble(lhs)+rhs.val)); }
  public static Fixed32 operator-(double lhs, Fixed32 rhs) { return new Fixed32((uint)(FromDouble(lhs)-rhs.val)); }
  public static Fixed32 operator*(double lhs, Fixed32 rhs) { return new Fixed32((uint)(((long)FromDouble(lhs)*rhs.val)>>16)); }
  public static Fixed32 operator/(double lhs, Fixed32 rhs) { return new Fixed32((uint)(((long)FromDouble(lhs)<<16)/rhs.val)); }

  public static Fixed32 operator+(Fixed32 lhs, Fixed32 rhs) { return new Fixed32((uint)(lhs.val+rhs.val)); }
  public static Fixed32 operator-(Fixed32 lhs, Fixed32 rhs) { return new Fixed32((uint)(lhs.val-rhs.val)); }
  public static Fixed32 operator*(Fixed32 lhs, Fixed32 rhs) { return new Fixed32((uint)(((long)lhs.val*rhs.val)>>16)); }
  public static Fixed32 operator/(Fixed32 lhs, Fixed32 rhs) { return new Fixed32((uint)(((long)lhs.val<<16)/rhs.val)); }
  #endregion

  #region Comparison operators
  public static bool operator<(Fixed32 lhs, Fixed32 rhs) { return lhs.val<rhs.val; }
  public static bool operator<=(Fixed32 lhs, Fixed32 rhs) { return lhs.val<=rhs.val; }
  public static bool operator>(Fixed32 lhs, Fixed32 rhs) { return lhs.val>rhs.val; }
  public static bool operator>=(Fixed32 lhs, Fixed32 rhs) { return lhs.val>=rhs.val; }
  public static bool operator==(Fixed32 lhs, Fixed32 rhs) { return lhs.val==rhs.val; }
  public static bool operator!=(Fixed32 lhs, Fixed32 rhs) { return lhs.val!=rhs.val; }

  public static bool operator<(Fixed32 lhs, int rhs) { return lhs.val<(rhs<<16); }
  public static bool operator<=(Fixed32 lhs, int rhs) { return lhs.val<=(rhs<<16); }
  public static bool operator>(Fixed32 lhs, int rhs) { return lhs.val>(rhs<<16); }
  public static bool operator>=(Fixed32 lhs, int rhs) { return lhs.val>=(rhs<<16); }
  public static bool operator==(Fixed32 lhs, int rhs) { return lhs.val==(rhs<<16); }
  public static bool operator!=(Fixed32 lhs, int rhs) { return lhs.val!=(rhs<<16); }

  public static bool operator<(Fixed32 lhs, double rhs) { return lhs.ToDouble()<rhs; }
  public static bool operator<=(Fixed32 lhs, double rhs) { return lhs.ToDouble()<=rhs; }
  public static bool operator>(Fixed32 lhs, double rhs) { return lhs.ToDouble()>rhs; }
  public static bool operator>=(Fixed32 lhs, double rhs) { return lhs.ToDouble()>=rhs; }
  public static bool operator==(Fixed32 lhs, double rhs) { return lhs.ToDouble()==rhs; }
  public static bool operator!=(Fixed32 lhs, double rhs) { return lhs.ToDouble()!=rhs; }

  public static bool operator<(int lhs, Fixed32 rhs) { return (lhs<<16)<rhs.val; }
  public static bool operator<=(int lhs, Fixed32 rhs) { return (lhs<<16)<=rhs.val; }
  public static bool operator>(int lhs, Fixed32 rhs) { return (lhs<<16)>rhs.val; }
  public static bool operator>=(int lhs, Fixed32 rhs) { return (lhs<<16)>=rhs.val; }
  public static bool operator==(int lhs, Fixed32 rhs) { return (lhs<<16)==rhs.val; }
  public static bool operator!=(int lhs, Fixed32 rhs) { return (lhs<<16)!=rhs.val; }

  public static bool operator<(double lhs, Fixed32 rhs) { return lhs<rhs.ToDouble(); }
  public static bool operator<=(double lhs, Fixed32 rhs) { return lhs<=rhs.ToDouble(); }
  public static bool operator>(double lhs, Fixed32 rhs) { return lhs>rhs.ToDouble(); }
  public static bool operator>=(double lhs, Fixed32 rhs) { return lhs>=rhs.ToDouble(); }
  public static bool operator==(double lhs, Fixed32 rhs) { return lhs==rhs.ToDouble(); }
  public static bool operator!=(double lhs, Fixed32 rhs) { return lhs!=rhs.ToDouble(); }
  #endregion
  
  /// <summary>Implicitly converts an integer to a <see cref="Fixed32"/>.</summary>
  /// <param name="i">An integer.</param>
  /// <returns>A <see cref="Fixed32"/> representing the given integer.</returns>
  public static implicit operator Fixed32(int i) { return new Fixed32((uint)(i<<16)); }
  /// <summary>Implicitly converts a double to a <see cref="Fixed32"/>.</summary>
  /// <param name="d">A double value.</param>
  /// <returns>A <see cref="Fixed32"/> representing the given double.</returns>
  public static implicit operator Fixed32(double d) { return new Fixed32((uint)FromDouble(d)); }

  #region Useful constants
  /// <summary>Napier's constant, approximately 2.718282.</summary>
  public static readonly Fixed32 E = new Fixed32((uint)(131072 + 47073));
  /// <summary>The smallest positive value that can be represented by a <see cref="Fixed32"/>.</summary>
  public static readonly Fixed32 Epsilon  = new Fixed32((uint)1);
  /// <summary>The maximum value that can be represented by a <see cref="Fixed32"/>.</summary>
  public static readonly Fixed32 MaxValue = new Fixed32((uint)0x7FFFFFFF);
  /// <summary>The minimum value that can be represented by a <see cref="Fixed32"/>.</summary>
  public static readonly Fixed32 MinValue = new Fixed32((uint)0x80000000);
  /// <summary>Negative one.</summary>
  public static readonly Fixed32 MinusOne = new Fixed32((uint)0xFFFF0000);
  /// <summary>One.</summary>
  public static readonly Fixed32 One = new Fixed32((uint)OneVal);
  /// <summary>PI, approximately 3.141593.</summary>
  public static readonly Fixed32 PI = new Fixed32((uint)(196608 + 9279));
  /// <summary>PI/2, approximately 1.570796.</summary>
  public static readonly Fixed32 PIover2 = new Fixed32((uint)(65536 + 37408));
  /// <summary>PI/2, approximately 6.283185.</summary>
  public static readonly Fixed32 TwoPI = new Fixed32((uint)(393216 + 18559));
  /// <summary>Zero.</summary>
  public static readonly Fixed32 Zero = new Fixed32((uint)0);
  #endregion

  #region IComparable Members
  /// <include file="documentation.xml" path="//IComparable/CompareTo/*"/>
  public int CompareTo(object obj)
  { if(obj==null) return 1;
    if(obj is Fixed32) { return val-((Fixed32)obj).val; }
    throw new ArgumentException("'obj' is not a Fixed32");
  }
  #endregion

  #region IConvertible Members
  // FIXME: these should probably do rounding
  /// <include file="documentation.xml" path="//IConvertible/ToUInt64/*"/>
  public ulong ToUInt64(IFormatProvider provider)
  { int n = ToInt();
    if(n<0) throw new OverflowException();
    return (ulong)n;
  }
  /// <include file="documentation.xml" path="//IConvertible/ToSByte/*"/>
  public sbyte ToSByte(IFormatProvider provider)
  { int n = ToInt();
    if(n<sbyte.MinValue || n>sbyte.MaxValue) throw new OverflowException();
    return (sbyte)n;
  }
  /// <include file="documentation.xml" path="//IConvertible/ToDouble/*"/>
  public double ToDouble(IFormatProvider provider) { return ToDouble(); }
  /// <include file="documentation.xml" path="//Mathematics/Fixed/ToDateTime/*"/>
  public DateTime ToDateTime(IFormatProvider provider) { throw new InvalidCastException(); }
  /// <include file="documentation.xml" path="//IConvertible/ToSingle/*"/>
  public float ToSingle(IFormatProvider provider)
  { double d = ToDouble();
    if(d<float.MinValue || d>float.MaxValue) throw new OverflowException();
    return (float)d;
  }
  /// <include file="documentation.xml" path="//Mathematics/Fixed/ToBoolean/*"/>
  public bool ToBoolean(IFormatProvider provider) { return val==0; }
  /// <include file="documentation.xml" path="//IConvertible/ToInt32/*"/>
  public int ToInt32(IFormatProvider provider) { return ToInt(); }
  /// <include file="documentation.xml" path="//IConvertible/ToUInt16/*"/>
  public ushort ToUInt16(IFormatProvider provider)
  { int n = ToInt();
    if(n<ushort.MinValue || n>ushort.MaxValue) throw new OverflowException();
    return (ushort)n;
  }
  /// <include file="documentation.xml" path="//IConvertible/ToInt16/*"/>
  public short ToInt16(IFormatProvider provider)
  { int n = ToInt();
    if(n<short.MinValue || n>short.MaxValue) throw new OverflowException();
    return (short)n;
  }
  /// <include file="documentation.xml" path="//IConvertible/ToString/*"/>
  public string ToString(IFormatProvider provider) { return ToString(null, provider); }
  /// <include file="documentation.xml" path="//IConvertible/ToByte/*"/>
  public byte ToByte(IFormatProvider provider)
  { int n = ToInt();
    if(n<byte.MinValue || n>byte.MaxValue) throw new OverflowException();
    return (byte)n;
  }
  /// <include file="documentation.xml" path="//IConvertible/ToChar/*"/>
  public char ToChar(IFormatProvider provider)
  { int n = ToInt();
    if(n<ushort.MinValue || n>ushort.MaxValue) throw new OverflowException();
    return (char)n;
  }
  /// <include file="documentation.xml" path="//IConvertible/ToInt64/*"/>
  public long ToInt64(IFormatProvider provider) { return ToInt(); }
  /// <summary>Returns the <see cref="TypeCode"/> for the <see cref="Fixed32"/> type.</summary>
  /// <returns>Returns <see cref="TypeCode.Object"/>.</returns>
  public TypeCode GetTypeCode() { return TypeCode.Object; }
  /// <include file="documentation.xml" path="//IConvertible/ToDecimal/*"/>
  public decimal ToDecimal(IFormatProvider provider) { return new decimal(ToDouble()); }
  /// <include file="documentation.xml" path="//IConvertible/ToType/*"/>
  public object ToType(Type conversionType, IFormatProvider provider)
  { if(conversionType==typeof(int)) return ToInt32(provider);
    if(conversionType==typeof(double)) return ToDouble();
    if(conversionType==typeof(string)) return ToString(null, provider);
    if(conversionType==typeof(float)) return ToSingle(provider);
    if(conversionType==typeof(short)) return ToInt16(provider);
    if(conversionType==typeof(ushort)) return ToUInt16(provider);
    if(conversionType==typeof(uint)) return ToUInt32(provider);
    if(conversionType==typeof(long)) return ToInt64(provider);
    if(conversionType==typeof(ulong)) return ToUInt64(provider);
    if(conversionType==typeof(bool)) return ToBoolean(provider);
    if(conversionType==typeof(short)) return ToInt16(provider);
    if(conversionType==typeof(byte)) return ToByte(provider);
    if(conversionType==typeof(sbyte)) return ToSByte(provider);
    if(conversionType==typeof(decimal)) return ToDecimal(provider);
    if(conversionType==typeof(char)) return ToChar(provider);
    if(conversionType==typeof(Fixed32)) return this;
    if(conversionType==typeof(Fixed64)) return ToFixed64();
    throw new InvalidCastException();
  }
  /// <include file="documentation.xml" path="//IConvertible/ToUInt32/*"/>
  public uint ToUInt32(IFormatProvider provider)
  { int n = ToInt();
    if(n<0) throw new OverflowException();
    return (uint)n;
  }
  #endregion

  #region IFormattable Members
  /// <include file="documentation.xml" path="//Mathematics/Fixed/ToString2/*"/>
  public string ToString(string format, IFormatProvider provider)
  { if(format==null) return ToDouble().ToString();
    if(format.Length==0 || char.ToUpper(format[0])!='R') return ToDouble().ToString(format);
    return (val>>16).ToString() + '/' + ((ushort)val).ToString();
  }
  #endregion

  const int Trunc  = unchecked((int)0xFFFF0000);
  const int OneVal = 0x10000;

  static int FromDouble(double value)
  { int whole = (int)value;
    int fp = (int)(Math.IEEERemainder(value, 1)*65536.0) & ~Trunc;
    if(whole<0) fp ^= Trunc;
    return (whole<<16) + fp;
  }

  internal int val;
}
#endregion

#region Fixed64
/// <summary>This class provides a fixed-point numeric type with 64 bits of storage in a 32.32 configuration.</summary>
/// <remarks>
/// <para>Floating point math on modern systems is very fast, and I wouldn't recommend using this fixed-point math
/// class for speed. The primary benefit of fixed-point math is that it provides consistency precision. Floating
/// point math loses precision when dealing with larger numbers, and the results of arithmetic operations are not
/// always consistent due to precision mismatch between operands. Fixed-point math eliminates these inconsistencies.
/// </para>
/// <para>This class provides 32 bits for the whole part and 32 bits for the fractional part, so the total range is
/// approximately -2147483648 to 2147483647.9999999998.
/// </para>
/// </remarks>
[Serializable, System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size=8)]
public struct Fixed64 : IFormattable, IComparable, IConvertible
{ 
  /// <summary>Initializes this fixed-point class from a floating point number.</summary>
  /// <param name="value">A floating point number from which the fixed-point number will be initialized.</param>
  /// <remarks>Due to the greater range of a 64-bit double, the value passed may not be able to be accurately
  /// represented.
  /// </remarks>
  public Fixed64(double value) { wholePart=0; val=FromDouble(value); } // damn C# requires all fields to be set
  /// <summary>Initializes this fixed-point class from an integer.</summary>
  /// <param name="value">An integer from which the fixed-point number will be initialized.</param>
  public Fixed64(int value) { val=0; wholePart=value; }
  internal Fixed64(long value) { wholePart=0; val=value; }
  internal Fixed64(int whole, uint frac) { val=frac; wholePart=whole; }
  /// <summary>Gets this number's absolute value.</summary>
  /// <value>A new fixed-point number containing the absolute value of this number.</value>
  public Fixed64 Abs { get { return val<0 ? new Fixed64(-val) : this; } }
  /// <summary>Gets this number's ceiling.</summary>
  /// <value>A new fixed-point number containing the smallest whole number greater than or equal to the current value.</value>
  public Fixed64 Ceiling { get { return new Fixed64((val+(OneVal-1)) & Trunc); } }
  /// <summary>Gets this number's ceiling.</summary>
  /// <value>A new fixed-point number containing the largest whole number less than or equal to the current value.</value>
  public Fixed64 Floor { get { return new Fixed64(val&Trunc); } }
  /// <summary>Gets this number's value, rounded.</summary>
  /// <remarks>This method performs banker's rounding, so values with a fractional part of exactly 0.5 will be
  /// rounded towards the nearest even number, or towards zero.
  /// </remarks>
  public Fixed64 Rounded
  { get
    { uint fp = (uint)val;
      if(fp<0x80000000) return new Fixed64(val&Trunc);
      else if(fp>0x80000000 || (val&OneVal)!=0) return new Fixed64((val+OneVal)&Trunc);
      else return new Fixed64(val&Trunc);
    }
  }
  /// <summary>Gets this number's square root.</summary>
  /// <value>A new fixed-point number containing the square root of the current value.</value>
  public Fixed64 Sqrt { get { return new Fixed64(Math.Sqrt(ToDouble())); } }
  /// <summary>Gets this number's value, truncated towards zero.</summary>
  public Fixed64 Truncated { get { return new Fixed64((val<0 ? val+(OneVal-1) : val)&Trunc); } }
  /// <summary>Returns true if this object is equal to the given object.</summary>
  /// <param name="obj">The object to compare against.</param>
  /// <returns>True if <paramref name="obj"/> is a <see cref="Fixed64"/> and has the same value as this one.</returns>
  public override bool Equals(object obj)
  { if(!(obj is Fixed64)) return false;
    return val == ((Fixed64)obj).val;
  }
  /// <summary>Returns a hash code for this value.</summary>
  /// <returns>A hash code for this value.</returns>
  public override int GetHashCode() { return wholePart ^ (int)(uint)val; }
  /// <summary>Converts this <see cref="Fixed64"/> to a <see cref="Fixed32"/>.</summary>
  /// <returns>A <see cref="Fixed32"/> containing the approximately the same value.</returns>
  /// <remarks>Due to the greater precision of the <see cref="Fixed64"/> class, the fractional part of the resulting
  /// value may not be exactly the same.
  /// </remarks>
  /// <exception cref="OverflowException">Thrown if the value is outside the range of a <see cref="Fixed32"/>.</exception>
  public Fixed32 ToFixed32()
  { if(wholePart<short.MinValue || wholePart>short.MaxValue) throw new OverflowException();
    return new Fixed32((uint)((short)wholePart<<16) | ((uint)val>>16));
  }
  /// <summary>Converts this fixed-point number to a floating-point number.</summary>
  /// <returns>The double value closest to this fixed-point number.</returns>
  public double ToDouble() { return wholePart + (uint)val*0.00000000023283064365386962890625; } // 1 / (1<<32)
  /// <summary>Returns the integer portion of the fixed-point number.</summary>
  /// <returns>The integer portion of the fixed-point number.</returns>
  public int ToInt()
  { int ret = wholePart;
    if(ret<0 && (uint)val!=0) ret++;
    return ret;
  }
  /// <summary>Converts this fixed-point number into a string.</summary>
  /// <returns>A string representing this fixed-point number.</returns>
  public override string ToString() { return ToString(null, null); }
  /// <include file="documentation.xml" path="//Mathematics/Fixed/ToString1/*"/>
  public string ToString(string format) { return ToString(format, null); }
  /// <summary>Converts a string into a fixed-point value.</summary>
  /// <param name="s">The string to convert.</param>
  /// <returns>A <see cref="Fixed64"/> containing the closest value.</returns>
  public static Fixed64 Parse(string s)
  { int pos = s.IndexOf('e');
    if(pos==-1)
    { pos = s.IndexOf('.');
      if(pos==-1)
      { pos = s.IndexOf('/');
        if(pos==-1) return new Fixed64((long)int.Parse(s)<<32); // integer
        else return new Fixed64(int.Parse(s.Substring(0, pos)), uint.Parse(s.Substring(pos+1))); // raw
      }
      else // number with fractional part
      { int val;
        uint frac;
        if(pos==0) val=0;
        else
        { string ws = s.Substring(0, pos);
          val = ws.Length==0 ? 0 : int.Parse(ws);
        }
        frac = (uint)(double.Parse(s.Substring(pos))*4294967296.0+0.5);
        return new Fixed64(val<0 ? ((long)val<<32)-frac : ((long)val<<32)+frac);
      }
    }
    else return new Fixed64(double.Parse(s)); // scientific notation
  }

  #region Arithmetic operators
  public static Fixed64 operator-(Fixed64 val) { return new Fixed64(-val.val); }

  public static Fixed64 operator+(Fixed64 lhs, int rhs) { return new Fixed64(lhs.val+((long)rhs<<32)); }
  public static Fixed64 operator-(Fixed64 lhs, int rhs) { return new Fixed64(lhs.val-((long)rhs<<32)); }
  public static Fixed64 operator*(Fixed64 lhs, int rhs) { return new Fixed64(lhs.val*rhs); }
  public static Fixed64 operator/(Fixed64 lhs, int rhs) { return new Fixed64(lhs.val/rhs); }

  public static Fixed64 operator+(Fixed64 lhs, double rhs) { return new Fixed64(lhs.val+FromDouble(rhs)); }
  public static Fixed64 operator-(Fixed64 lhs, double rhs) { return new Fixed64(lhs.val-FromDouble(rhs)); }
  public static Fixed64 operator*(Fixed64 lhs, double rhs) { return lhs * new Fixed64(rhs); }
  public static Fixed64 operator/(Fixed64 lhs, double rhs) { return lhs / new Fixed64(rhs); }

  public static Fixed64 operator+(int lhs, Fixed64 rhs) { return new Fixed64(((long)lhs<<32)+rhs.val); }
  public static Fixed64 operator-(int lhs, Fixed64 rhs) { return new Fixed64(((long)lhs<<32)-rhs.val); }
  public static Fixed64 operator*(int lhs, Fixed64 rhs) { return new Fixed64(lhs*rhs.val); }
  public static Fixed64 operator/(int lhs, Fixed64 rhs) { return new Fixed64(lhs) / rhs; }

  public static Fixed64 operator+(double lhs, Fixed64 rhs) { return new Fixed64(FromDouble(lhs)+rhs.val); }
  public static Fixed64 operator-(double lhs, Fixed64 rhs) { return new Fixed64(FromDouble(lhs)-rhs.val); }
  public static Fixed64 operator*(double lhs, Fixed64 rhs) { return new Fixed64(lhs) * rhs; }
  public static Fixed64 operator/(double lhs, Fixed64 rhs) { return new Fixed64(lhs) / rhs; }

  public static Fixed64 operator+(Fixed64 lhs, Fixed64 rhs) { return new Fixed64(lhs.val+rhs.val); }
  public static Fixed64 operator-(Fixed64 lhs, Fixed64 rhs) { return new Fixed64(lhs.val-rhs.val); }

  public static Fixed64 operator*(Fixed64 lhs, Fixed64 rhs)
  { long a=lhs.ToInt(), b=(uint)lhs.val, c=rhs.ToInt(), d=(uint)rhs.val;
    return new Fixed64(((a*c)<<32) + b*c + a*d + ((b*d)>>32));
  }

  public static Fixed64 operator/(Fixed64 lhs, Fixed64 rhs)
  { long quot, rem;
    uint fp = (uint)rhs.val;
    int  count;
    if(fp==0) { return new Fixed64(lhs.val / rhs.ToInt()); }

    byte neg=0;
    if(lhs.val<0) { lhs.val=-lhs.val; neg=(byte)~neg; }
    if(rhs.val<0) { rhs.val=-rhs.val; neg=(byte)~neg; }

    count=0; // reduce if we can
    { uint op = (uint)lhs.val, mask=1;
      if((fp&mask)==0 && (op&mask)==0)
      { do { mask<<=1; count++; } while((fp&mask)==0 && (op&mask)==0);
        rhs.val>>=count; lhs.val>>=count;
      }
    }

    if(rhs.val<0x100000000)
    { quot  = Math.DivRem(lhs.val, rhs.val, out rem)<<32;
      quot += Math.DivRem(rem<<32, rhs.val, out rem);
    }
    else if(rhs.val<0x1000000000000)
    { Math.DivRem(lhs.val>>32, rhs.val, out rem);
      quot  = Math.DivRem((rem<<32)+(uint)lhs.val, rhs.val, out rem)<<32;
      quot += Math.DivRem(rem<<16, rhs.val, out rem)<<16;
      quot += Math.DivRem(rem<<16, rhs.val, out rem);
    }
    else // fall back on long division
    { // TODO: optimize for divisor>=dividend
      Union ls = new Union(lhs.val<<count), t = new Union();
      int  bits = 96-count;
      byte bit;

      rem = quot = 0;
      do
      { rem = (rem<<1) | (byte)((ls.Uint&0x80000000)>>31);
        lhs.val = ls.Long;
        ls.Long <<= 1;
        bits--;
      }
      while(rem<rhs.val);
      
      ls.Long = lhs.val;
      rem >>= 1;
      bits++;
      
      do
      { rem = (rem<<1) | (byte)((ls.Uint&0x80000000)>>31);
        t.Long = rem - rhs.val;
        bit  = (byte)((~t.Uint&0x80000000)>>31);
        quot = (quot<<1) | bit;
        if(bit!=0) rem=t.Long;
        ls.Long <<= 1;
      } while(--bits>0);
    }
    
    return new Fixed64(neg==0 ? quot : -quot);
  }
  #endregion

  #region Comparison operators
  public static bool operator<(Fixed64 lhs, Fixed64 rhs) { return lhs.val<rhs.val; }
  public static bool operator<=(Fixed64 lhs, Fixed64 rhs) { return lhs.val<=rhs.val; }
  public static bool operator>(Fixed64 lhs, Fixed64 rhs) { return lhs.val>rhs.val; }
  public static bool operator>=(Fixed64 lhs, Fixed64 rhs) { return lhs.val>=rhs.val; }
  public static bool operator==(Fixed64 lhs, Fixed64 rhs) { return lhs.val==rhs.val; }
  public static bool operator!=(Fixed64 lhs, Fixed64 rhs) { return lhs.val!=rhs.val; }

  public static bool operator<(Fixed64 lhs, int rhs) { return lhs.val<((long)rhs<<32); }
  public static bool operator<=(Fixed64 lhs, int rhs) { return lhs.val<=((long)rhs<<32); }
  public static bool operator>(Fixed64 lhs, int rhs) { return lhs.val>((long)rhs<<32); }
  public static bool operator>=(Fixed64 lhs, int rhs) { return lhs.val>=((long)rhs<<32); }
  public static bool operator==(Fixed64 lhs, int rhs) { return lhs.val==((long)rhs<<32); }
  public static bool operator!=(Fixed64 lhs, int rhs) { return lhs.val!=((long)rhs<<32); }

  public static bool operator<(Fixed64 lhs, double rhs) { return lhs.ToDouble()<rhs; }
  public static bool operator<=(Fixed64 lhs, double rhs) { return lhs.ToDouble()<=rhs; }
  public static bool operator>(Fixed64 lhs, double rhs) { return lhs.ToDouble()>rhs; }
  public static bool operator>=(Fixed64 lhs, double rhs) { return lhs.ToDouble()>=rhs; }
  public static bool operator==(Fixed64 lhs, double rhs) { return lhs.ToDouble()==rhs; }
  public static bool operator!=(Fixed64 lhs, double rhs) { return lhs.ToDouble()!=rhs; }

  public static bool operator<(int lhs, Fixed64 rhs) { return ((long)lhs<<32)<rhs.val; }
  public static bool operator<=(int lhs, Fixed64 rhs) { return ((long)lhs<<32)<=rhs.val; }
  public static bool operator>(int lhs, Fixed64 rhs) { return ((long)lhs<<32)>rhs.val; }
  public static bool operator>=(int lhs, Fixed64 rhs) { return ((long)lhs<<32)>=rhs.val; }
  public static bool operator==(int lhs, Fixed64 rhs) { return ((long)lhs<<32)==rhs.val; }
  public static bool operator!=(int lhs, Fixed64 rhs) { return ((long)lhs<<32)!=rhs.val; }

  public static bool operator<(double lhs, Fixed64 rhs) { return lhs<rhs.ToDouble(); }
  public static bool operator<=(double lhs, Fixed64 rhs) { return lhs<=rhs.ToDouble(); }
  public static bool operator>(double lhs, Fixed64 rhs) { return lhs>rhs.ToDouble(); }
  public static bool operator>=(double lhs, Fixed64 rhs) { return lhs>=rhs.ToDouble(); }
  public static bool operator==(double lhs, Fixed64 rhs) { return lhs==rhs.ToDouble(); }
  public static bool operator!=(double lhs, Fixed64 rhs) { return lhs!=rhs.ToDouble(); }
  #endregion
  
  /// <summary>Implicitly converts an integer to a <see cref="Fixed64"/>.</summary>
  /// <param name="i">An integer.</param>
  /// <returns>A <see cref="Fixed64"/> representing the given integer.</returns>
  public static implicit operator Fixed64(int i) { return new Fixed64((long)i<<32); }
  /// <summary>Implicitly converts a double to a <see cref="Fixed64"/>.</summary>
  /// <param name="d">A double value.</param>
  /// <returns>A <see cref="Fixed64"/> representing the given double.</returns>
  public static implicit operator Fixed64(double d) { return new Fixed64(FromDouble(d)); }

  #region Useful constants
  /// <summary>Napier's constant, approximately 2.718282.</summary>
  public static readonly Fixed64 E = new Fixed64(8589934592L + 3084996963);
  /// <summary>The smallest positive value that can be represented by a <see cref="Fixed32"/>.</summary>
  public static readonly Fixed64 Epsilon = new Fixed64((long)1);
  /// <summary>The maximum value that can be represented by a <see cref="Fixed32"/>.</summary>
  public static readonly Fixed64 MaxValue = new Fixed64(0x7FFFFFFFFFFFFFFFL);
  /// <summary>The minimum value that can be represented by a <see cref="Fixed32"/>.</summary>
  public static readonly Fixed64 MinValue = new Fixed64(unchecked((long)0x8000000000000000));
  /// <summary>Negative one.</summary>
  public static readonly Fixed64 MinusOne = new Fixed64(Trunc);
  /// <summary>One.</summary>
  public static readonly Fixed64 One = new Fixed64(OneVal);
  /// <summary>PI, approximately 3.141593.</summary>
  public static readonly Fixed64 PI = new Fixed64(12884901888L + 608135817);
  /// <summary>PI/2, approximately 1.570796.</summary>
  public static readonly Fixed64 PIover2 = new Fixed64(4294967296L  + 2451551556);
  /// <summary>PI/2, approximately 6.283185.</summary>
  public static readonly Fixed64 TwoPI = new Fixed64(25769803776L + 1216271633);
  /// <summary>Zero.</summary>
  public static readonly Fixed64 Zero = new Fixed64((long)0);
  #endregion

  const long Trunc  = unchecked((long)0xFFFFFFFF00000000);
  const long OneVal = 0x100000000L;

  static long FromDouble(double value)
  { int whole = (int)value;
    long fp = (long)(Math.IEEERemainder(value, 1)*4294967296.0) & ~Trunc;
    if(whole<0) fp ^= Trunc;
    return ((long)whole<<32) + fp;
  }

  #region IComparable Members
  /// <include file="documentation.xml" path="//IComparable/CompareTo/*"/>
  public int CompareTo(object obj)
  { if(obj==null) return 1;
    if(obj is Fixed64)
    { long ov = ((Fixed64)obj).val;
      return val<ov ? -1 : val>ov ? 1 : 0;
    }
    throw new ArgumentException("'obj' is not a Fixed64");
  }
  #endregion

  #region IConvertible Members
  // FIXME: these should probably do rounding
  /// <include file="documentation.xml" path="//IConvertible/ToUInt64/*"/>
  public ulong ToUInt64(IFormatProvider provider)
  { int n = ToInt();
    if(n<0) throw new OverflowException();
    return (ulong)n;
  }
  /// <include file="documentation.xml" path="//IConvertible/ToSByte/*"/>
  public sbyte ToSByte(IFormatProvider provider)
  { int n = ToInt();
    if(n<sbyte.MinValue || n>sbyte.MaxValue) throw new OverflowException();
    return (sbyte)n;
  }
  /// <include file="documentation.xml" path="//IConvertible/ToDouble/*"/>
  public double ToDouble(IFormatProvider provider) { return ToDouble(); }
  /// <include file="documentation.xml" path="//Mathematics/Fixed/ToDateTime/*"/>
  public DateTime ToDateTime(IFormatProvider provider) { throw new InvalidCastException(); }
  /// <include file="documentation.xml" path="//IConvertible/ToSingle/*"/>
  public float ToSingle(IFormatProvider provider)
  { double d = ToDouble();
    if(d<float.MinValue || d>float.MaxValue) throw new OverflowException();
    return (float)d;
  }
  /// <include file="documentation.xml" path="//Mathematics/Fixed/ToBoolean/*"/>
  public bool ToBoolean(IFormatProvider provider) { return val==0; }
  /// <include file="documentation.xml" path="//IConvertible/ToInt32/*"/>
  public int ToInt32(IFormatProvider provider) { return ToInt(); }
  /// <include file="documentation.xml" path="//IConvertible/ToUInt16/*"/>
  public ushort ToUInt16(IFormatProvider provider)
  { int n = ToInt();
    if(n<ushort.MinValue || n>ushort.MaxValue) throw new OverflowException();
    return (ushort)n;
  }
  /// <include file="documentation.xml" path="//IConvertible/ToInt16/*"/>
  public short ToInt16(IFormatProvider provider)
  { int n = ToInt();
    if(n<short.MinValue || n>short.MaxValue) throw new OverflowException();
    return (short)n;
  }
  /// <include file="documentation.xml" path="//IConvertible/ToString/*"/>
  public string ToString(IFormatProvider provider) { return ToString(null, provider); }
  /// <include file="documentation.xml" path="//IConvertible/ToByte/*"/>
  public byte ToByte(IFormatProvider provider)
  { int n = ToInt();
    if(n<byte.MinValue || n>byte.MaxValue) throw new OverflowException();
    return (byte)n;
  }
  /// <include file="documentation.xml" path="//IConvertible/ToChar/*"/>
  public char ToChar(IFormatProvider provider)
  { int n = ToInt();
    if(n<ushort.MinValue || n>ushort.MaxValue) throw new OverflowException();
    return (char)n;
  }
  /// <include file="documentation.xml" path="//IConvertible/ToInt64/*"/>
  public long ToInt64(IFormatProvider provider) { return ToInt(); }
  /// <summary>Returns the <see cref="TypeCode"/> for the <see cref="Fixed64"/> type.</summary>
  /// <returns>Returns <see cref="TypeCode.Object"/>.</returns>
  public System.TypeCode GetTypeCode() { return System.TypeCode.Object; }
  /// <include file="documentation.xml" path="//IConvertible/ToDecimal/*"/>
  public decimal ToDecimal(IFormatProvider provider) { return new decimal(ToDouble()); }
  /// <include file="documentation.xml" path="//IConvertible/ToType/*"/>
  public object ToType(Type conversionType, IFormatProvider provider)
  { if(conversionType==typeof(int)) return ToInt32(provider);
    if(conversionType==typeof(double)) return ToDouble();
    if(conversionType==typeof(string)) return ToString(null, provider);
    if(conversionType==typeof(float)) return ToSingle(provider);
    if(conversionType==typeof(uint)) return ToUInt32(provider);
    if(conversionType==typeof(long)) return ToInt64(provider);
    if(conversionType==typeof(ulong)) return ToUInt64(provider);
    if(conversionType==typeof(bool)) return ToBoolean(provider);
    if(conversionType==typeof(short)) return ToInt16(provider);
    if(conversionType==typeof(ushort)) return ToUInt16(provider);
    if(conversionType==typeof(byte)) return ToByte(provider);
    if(conversionType==typeof(sbyte)) return ToSByte(provider);
    if(conversionType==typeof(decimal)) return ToDecimal(provider);
    if(conversionType==typeof(char)) return ToChar(provider);
    if(conversionType==typeof(Fixed32)) return ToFixed32();
    if(conversionType==typeof(Fixed64)) return this;
    throw new InvalidCastException();
  }
  /// <include file="documentation.xml" path="//IConvertible/ToUInt32/*"/>
  public uint ToUInt32(IFormatProvider provider)
  { int n = ToInt();
    if(n<0) throw new OverflowException();
    return (uint)n;
  }
  #endregion

  #region IFormattable Members
  /// <include file="documentation.xml" path="//Mathematics/Fixed/ToString2/*"/>
  public string ToString(string format, IFormatProvider provider)
  { if(format==null) return ToDouble().ToString();
    if(format.Length==0 || char.ToUpper(format[0])!='R') return ToDouble().ToString(format);
    return wholePart.ToString() + '/' + ((uint)val).ToString();
  }
  #endregion
  
  #region Union
  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
  struct Union
  { public Union(long val) { Uint=0; Long=val; }
    #if BIGENDIAN
    [System.Runtime.InteropServices.FieldOffset(0)] public long Long;
    [System.Runtime.InteropServices.FieldOffset(0)] public uint Uint;
    #else
    [System.Runtime.InteropServices.FieldOffset(0)] public long Long;
    [System.Runtime.InteropServices.FieldOffset(4)] public uint Uint;
    #endif
  }
  #endregion
  
  #if BIGENDIAN
  [System.Runtime.InteropServices.FieldOffset(0)] internal long val;
  [System.Runtime.InteropServices.FieldOffset(0)] int wholePart;
  #else
  [System.Runtime.InteropServices.FieldOffset(0)] internal long val;
  [System.Runtime.InteropServices.FieldOffset(4)] int wholePart;
  #endif
}
#endregion

// TODO: once generics become available, implement these in a type-generic fashion
#region 2D math
namespace TwoD
{

#region Vector
public struct Vector
{ public Vector(double x, double y) { X=x; Y=y; }
  public Vector(Point pt) { X=pt.X; Y=pt.Y; }

  public double Angle
  { get
    { double angle = Math.Acos(X/Length);
      if(Y>0) angle = MathConst.TwoPI-angle;
      return angle;
    }
  }

  public Vector CrossVector { get { return new Vector(Y, -X); } }
  public double Length
  { get { return System.Math.Sqrt(X*X+Y*Y); }
    set { Normalize(value); }
  }
  public double LengthSqr { get { return X*X+Y*Y; } }
  public Vector Normal { get { return this/Length; } }
  
  public double DotProduct(Vector v) { return X*v.X + Y*v.Y; }
  public void  Normalize() { Assign(Normal); }
  public void  Normalize(double length) { Assign(this / (Length/length)); }

  public void Rotate(double angle) { Assign(Rotated(angle)); }
  public Vector Rotated(double angle)
  { double sin = Math.Sin(angle), cos = Math.Cos(angle);
    return new Vector(X*cos-Y*sin, X*sin+Y*cos);
  }
  
  public override bool Equals(object obj) { return obj is Vector ? (Vector)obj==this : false; }
  public bool Equals(Vector vect, double epsilon)
  { return Math.Abs(vect.X-X)<=epsilon && Math.Abs(vect.Y-Y)<=epsilon;
  }
  public override int GetHashCode() { unsafe { fixed(Vector* v=&this) return *(int*)&v->X ^ *(int*)&v->Y; } }
  public Point ToPoint() { return new Point(X, Y); }
  public override string ToString() { return string.Format("[{0:f2},{1:f2}]", X, Y); }

  public static Vector operator-(Vector v) { return new Vector(-v.X, -v.Y); }
  public static Vector operator+(Vector a, Vector b) { return new Vector(a.X+b.X, a.Y+b.Y); }
  public static Vector operator-(Vector a, Vector b) { return new Vector(a.X-b.X, a.Y-b.Y); }
  public static Vector operator*(Vector a, Vector b) { return new Vector(a.X*b.X, a.Y*b.Y); }
  public static Vector operator/(Vector a, Vector b) { return new Vector(a.X/b.X, a.Y/b.Y); }
  public static Vector operator*(Vector v, double f)  { return new Vector(v.X*f, v.Y*f); }
  public static Vector operator/(Vector v, double f)  { return new Vector(v.X/f, v.Y/f); }

  public static bool operator==(Vector a, Vector b) { return a.X==b.X && a.Y==b.Y; }
  public static bool operator!=(Vector a, Vector b) { return a.X!=b.X || a.Y!=b.Y; }

  public static Vector Invalid { get { return new Vector(double.NaN, double.NaN); } }

  public double X, Y;

  void Assign(Vector v) { X=v.X; Y=v.Y; }
}
#endregion

#region Point
public struct Point
{ public Point(System.Drawing.Point pt) { X=pt.X; Y=pt.Y; }
  public Point(double x, double y) { X=x; Y=y; }

  public bool Valid { get { return !double.IsNaN(X); } }

  public double DistanceTo(Point point)
  { double xd=point.X-X, yd=point.Y-Y;
    return Math.Sqrt(xd*xd+yd*yd);
  }
  public double DistanceSquaredTo(Point point)
  { double xd=point.X-X, yd=point.Y-Y;
    return xd*xd+yd*yd;
  }

  public void Offset(double xd, double yd) { X+=xd; Y+=yd; }

  public System.Drawing.Point ToPoint() { return new System.Drawing.Point((int)Math.Round(X), (int)Math.Round(Y)); }

  public override bool Equals(object obj) { return obj is Point ? (Point)obj==this : false; }
  public bool Equals(Point point, double epsilon)
  { return Math.Abs(point.X-X)<=epsilon && Math.Abs(point.Y-Y)<=epsilon;
  }
  public override int GetHashCode() { unsafe { fixed(Point* v=&this) return *(int*)&v->X ^ *(int*)&v->Y; } }
  public override string ToString() { return string.Format("({0:f2},{1:f2})", X, Y); }

  public static Point Invalid { get { return new Point(double.NaN, double.NaN); } }
  public static Vector operator- (Point lhs, Point rhs)  { return new Vector(lhs.X-rhs.X, lhs.Y-rhs.Y); }
  public static Point  operator- (Point lhs, Vector rhs) { return new Point(lhs.X-rhs.X, lhs.Y-rhs.Y); }
  public static Point  operator+ (Point lhs, Vector rhs) { return new Point(lhs.X+rhs.X, lhs.Y+rhs.Y); }
  public static bool   operator==(Point lhs, Point rhs)  { return lhs.X==rhs.X && lhs.Y==rhs.Y; }
  public static bool   operator!=(Point lhs, Point rhs)  { return lhs.X!=rhs.X || lhs.Y!=rhs.Y; }
  public static implicit operator Point(System.Drawing.Point point) { return new Point(point.X, point.Y); }

  public double X, Y;
}
#endregion

#region Line
public struct LineIntersectInfo
{ public LineIntersectInfo(Point point, bool onFirst, bool onSecond)
  { Point=point; OnFirst=onFirst; OnSecond=onSecond;
  }
  public bool OnBoth { get { return OnFirst && OnSecond; } }
  public Point Point;
  public bool  OnFirst, OnSecond;
}

public struct Line
{ public Line(double x, double y, double xd, double yd) { Start=new Point(x, y); Vector=new Vector(xd, yd); }
  public Line(Point start, Vector vector) { Start=start; Vector=vector; }
  public Line(Point start, Point end) { Start=start; Vector=end-start; }

  public Point End { get { return Start+Vector; } set { Vector=value-Start; } }
  public double Length { get { return Vector.Length; } set { Vector.Length=value; } }
  public double LengthSqr { get { return Vector.LengthSqr; } }
  public bool Valid { get { return Start.Valid; } }

  public Line ConvexIntersection(Polygon poly)
  { poly.AssertValid();
    Point start = Start, end = End;
    int sign = poly.IsClockwise() ? 1 : -1;

    for(int i=0; i<poly.Length; i++)
    { Line edge = poly.GetEdge(i);
      bool sout = Math.Sign(edge.WhichSide(start))==sign, eout = Math.Sign(edge.WhichSide(end))==sign;
      if(sout)
      { if(eout) return Line.Invalid;
        start = edge.LineIntersection(new Line(start, end));
      }
      else if(eout) end = edge.LineIntersection(new Line(start, end));
    }
    if(start==end) return Line.Invalid;
    for(int i=0; i<poly.Length; i++)
    { Line edge = poly.GetEdge(i);
      if(Math.Sign(edge.WhichSide(start))==sign && Math.Sign(edge.WhichSide(end))==sign) return Line.Invalid;
    }
    return new Line(start, end);
  }
  
  public bool ConvexIntersects(Polygon poly) { return ConvexIntersection(poly).Valid; }

  public double DistanceTo(Point point) { return Vector.CrossVector.Normal.DotProduct(point-Start); }

  public Point GetPoint(int point)
  { if(point<0 || point>1) throw new ArgumentOutOfRangeException("point", point, "must be 0 or 1");
    return point==0 ? Start : End;
  }

  public LineIntersectInfo GetIntersection(Line line)
  { Point p2 = End, p4 = line.End;
    double d = (p4.Y-line.Start.Y)*(p2.X-Start.X) - (p4.X-line.Start.X)*(p2.Y-Start.Y), ua, ub;
    if(d==0) return new LineIntersectInfo(Point.Invalid, false, false);
    ua = ((p4.X-line.Start.X)*(Start.Y-line.Start.Y) - (p4.Y-line.Start.Y)*(Start.X-line.Start.X)) / d;
    ub = ((p2.X-Start.X)*(Start.Y-line.Start.Y) - (p2.Y-Start.Y)*(Start.X-line.Start.X)) / d;
    return new LineIntersectInfo(new Point(Start.X + Vector.X*ua, Start.Y + Vector.Y*ua), ua>=0&&ua<=1, ub>=0&&ub<=1);
  }

  public Point Intersection(Line segment)
  { Point p2 = End, p4 = segment.End;
    double d = (p4.Y-segment.Start.Y)*(p2.X-Start.X) - (p4.X-segment.Start.X)*(p2.Y-Start.Y), ua, ub;
    if(d==0) return Point.Invalid;
    ua = ((p4.X-segment.Start.X)*(Start.Y-segment.Start.Y) - (p4.Y-segment.Start.Y)*(Start.X-segment.Start.X)) / d;
    if(ua<0 || ua>1) return Point.Invalid;
    ub = ((p2.X-Start.X)*(Start.Y-segment.Start.Y) - (p2.Y-Start.Y)*(Start.X-segment.Start.X)) / d;
    if(ub<0 || ub>1) return Point.Invalid;
    return new Point(Start.X + Vector.X*ua, Start.Y + Vector.Y*ua);
  }
  
  public Line Intersection(Rectangle rect)
  { double x2=rect.Right, y2=rect.Bottom;
    Point start=Start, end=End;
    int c, c2;

    c = start.Y<rect.Y ? 1 : start.Y>y2 ? 2 : 0;
    if(start.X<rect.X) c |= 4;
    else if(start.X>x2) c |= 8;

    c2 = end.Y<rect.Y ? 1 : end.Y>y2 ? 2 : 0;
    if(end.X<rect.X) c2 |= 4;
    else if(end.X>x2) c2 |= 8;

    if(c==0 && c2==0) return new Line(start, end);
    if((c&c2) != 0) return Line.Invalid;

    if(c!=0)
    { if((c&1)!=0)
      { start.X += (rect.Y-start.Y) * Vector.X / Vector.Y;
        start.Y = rect.Y;
      }
      else if((c&2)!=0)
      { start.X -= (start.Y-y2) * Vector.X / Vector.Y;
        start.Y = y2;
      }
      else if((c&4)!=0)
      { start.Y += (rect.X-start.X) * Vector.Y / Vector.X;
        start.X = rect.X;
      }
      else
      { start.Y -= (start.X-x2) * Vector.Y / Vector.X;
        start.X = x2;
      }
    }
    if(c2!=0)
    { if((c2&1)!=0)
      { end.X += (rect.Y-end.Y) * Vector.X / Vector.Y;
        end.Y = rect.Y;
      }
      else if((c2&2)!=0)
      { end.X -= (end.Y-y2) * Vector.X / Vector.Y;
        end.Y = y2;
      }
      else if((c2&4)!=0)
      { end.Y += (rect.X-end.X) * Vector.Y / Vector.X;
        end.X = rect.X;
      }
      else
      { end.Y -= (end.X-x2) * Vector.Y / Vector.X;
        end.X = x2;
      }
    }

    c = start.Y<rect.Y ? 1 : start.Y>y2 ? 2 : 0;
    if(start.X<rect.X) c |= 4;
    else if(start.X>x2) c |= 8;

    c2 = end.Y<rect.Y ? 1 : end.Y>y2 ? 2 : 0;
    if(end.X<rect.X) c2 |= 4;
    else if(end.X>x2) c2 |= 8;

    if((c&c2) != 0) return Line.Invalid;
    return new Line(start, end);
  }

  public bool Intersects(Line segment) { return Intersection(segment).Valid; }

  public bool Intersects(Rectangle rect) { return Intersection(rect).Valid; }

  public Point LineIntersection(Line line)
  { Point p2 = End, p4 = line.End;
    double d = (p4.Y-line.Start.Y)*(p2.X-Start.X) - (p4.X-line.Start.X)*(p2.Y-Start.Y);
    if(d==0) return Point.Invalid;
    d = ((p4.X-line.Start.X)*(Start.Y-line.Start.Y) - (p4.Y-line.Start.Y)*(Start.X-line.Start.X)) / d;
    return new Point(Start.X + Vector.X*d, Start.Y + Vector.Y*d);
  }
  public bool LineIntersects(Line line) { return Intersection(line).Valid; }

  public double WhichSide(Point point) { return Vector.CrossVector.DotProduct(point-Start); }

  public override bool Equals(object obj) { return obj is Line ? (Line)obj==this : false; }
  public bool Equals(Line line, double epsilon)
  { return Start.Equals(line.Start, epsilon) && Vector.Equals(line.Vector, epsilon);
  }
  public override int GetHashCode() { return Start.GetHashCode() ^ Vector.GetHashCode(); }
  public override string ToString() { return string.Format("{0}->{1}", Start, Vector); }

  public static Line FromPoints(Point start, Point end) { return new Line(start, end-start); }
  public static Line FromPoints(double x1, double y1, double x2, double y2) { return new Line(x1, y1, x2-x1, y2-y1); }
  
  public static bool operator==(Line lhs, Line rhs) { return lhs.Start==rhs.Start && lhs.Vector==rhs.Vector; }
  public static bool operator!=(Line lhs, Line rhs) { return lhs.Start!=rhs.Start || lhs.Vector!=rhs.Vector; }
  
  public static Line Invalid { get { return new Line(Point.Invalid, new Vector()); } }

  public Point  Start;
  public Vector Vector;
}
#endregion

#region Circle
public struct Circle
{ public Circle(double centerX, double centerY, double radius) { Center=new Point(centerX, centerY); Radius=radius; }
  public Circle(Point center, double radius) { Center=center; Radius=radius; }

  public double Area { get { return (Radius*Radius*Math.PI); } }

  public bool Contains(Point point) { return (point-Center).Length <= Radius; }

  public Point Center;
  public double Radius;
}
#endregion

#region Corner
public struct Corner
{ public Line Edge0 { get { return new Line(Point+Vector0, -Vector0); } }
  public Line Edge1 { get { return new Line(Point, Vector1); } }

  public double CrossZ
  { get
    { Point p0 = GetPoint(-1), p2 = GetPoint(1);
      return (Point.X-p0.X)*(p2.Y-Point.Y) - (Point.Y-p0.Y)*(p2.X-Point.X);
    }
  }

  public Line GetEdge(int edge)
  { if(edge<0 || edge>1) throw new ArgumentOutOfRangeException("GetEdge", edge, "must be 0 or 1");
    return edge==0 ? Edge0 : Edge1;
  }

  public Point GetPoint(int point)
  { if(point<-1 || point>1) throw new ArgumentOutOfRangeException("GetPoint", point, "must be from -1 to 1");
    return point==0 ? Point : (point==-1 ? Point+Vector0 : Point+Vector1);
  }

  public Point Point;
  public Vector Vector0, Vector1;
}
#endregion

#region Rectangle
public struct Rectangle
{ public Rectangle(System.Drawing.Rectangle rect)
  { X=rect.X; Y=rect.Y; Width=rect.Width; Height=rect.Height;
  }
  public Rectangle(System.Drawing.RectangleF rect) { X=rect.X; Y=rect.Y; Width=rect.Width; Height=rect.Height; }
  public Rectangle(double x, double y, double width, double height) { X=x; Y=y; Width=width; Height=height; }
  public Rectangle(Point location, Vector size) { X=location.X; Y=location.Y; Width=size.X; Height=size.Y; }

  public double Bottom { get { return Y+Height; } }
  public Point BottomRight { get { return new Point(X+Width, Y+Height); } }
  public Point Location { get { return new Point(X, Y); } }
  public double Right { get { return X+Width; } }
  public Vector Size { get { return new Vector(Width, Height); } }
  public Point TopLeft { get { return new Point(X, Y); } }
  
  public bool Contains(Point point) { return point.Y>=Y && point.Y<Bottom && point.X>=X && point.X<Right; }

  public bool Contains(Rectangle rect) { return Contains(rect.Location) && Contains(rect.BottomRight); }

  public Line GetEdge(int i)
  { if(i<0 || i>=4) throw new ArgumentOutOfRangeException("i", i, "must be from 0 to 3");
    switch(i)
    { case 0: return new Line(X, Y, 0, Height);
      case 1: return new Line(X, Y, Width, 0);
      case 2: return new Line(X+Width, Y, 0, Height);
      case 3: return new Line(X, Y+Height, Width, 0);
      default: return Line.Invalid;
    }
  }

  public void Inflate(double x, double y) { X-=x; Width+=x*2; Y-=y; Height+=y*2; }
  public Rectangle Inflated(double x, double y) { return new Rectangle(X-x, Y-y, Width+x*2, Height+y*2); }

  public void Intersect(Rectangle rect)
  { double x2=Right, ox2=rect.Right;
    if(X<rect.X)
    { if(x2<rect.X) goto abort;
      X=rect.X;
    }
    else if(X>=ox2) goto abort;

    if(x2>ox2)
    { if(X>=ox2) goto abort;
      Width -= ox2-x2;
    }
    else if(X<rect.X) goto abort;

    double y2=Bottom, oy2=rect.Bottom;
    if(Y<rect.Y)
    { if(y2<rect.Y) goto abort;
      Y=rect.Y;
    }
    else if(Y>=oy2) goto abort;

    if(y2>oy2)
    { if(Y>=oy2) goto abort;
      Height -= oy2-y2;
    }
    else if(Y<rect.Y) goto abort;
    
    return;
    abort:
    X=Y=Width=Height=0;
  }

  public Rectangle Intersection(Rectangle rect)
  { Rectangle ret = new Rectangle(X, Y, Width, Height);
    ret.Intersect(rect);
    return ret;
  }

  public bool Intersects(Rectangle rect)
  { return Contains(rect.Location) || Contains(rect.BottomRight) || rect.Contains(Location) ||
           rect.Contains(BottomRight);
  }

  public void Offset(double x, double y) { X+=x; Y+=y; }
  public void Offset(Vector vect) { X+=vect.X; Y+=vect.Y; }

  public override string ToString()
  { return string.Format("X={0:F2} Y={1:F2} Width={2:F2} Height={3:F2}", X, Y, Width, Height);
  }

  public Rectangle Union(Rectangle rect)
  { Rectangle ret = new Rectangle(X, Y, Width, Height);
    ret.Unite(rect);
    return ret;
  }
  
  public void Unite(Rectangle rect)
  { if(X>rect.X) { Width += X-rect.X; X=rect.X; }
    if(Y>rect.Y) { Height += Y-rect.Y; Y=rect.Y; }
    if(Right<rect.Right)   Width  += rect.Right-Right;
    if(Bottom<rect.Bottom) Height += rect.Bottom-Bottom;
  }

  public double X, Y, Width, Height;
}
#endregion

#region Polygon
public class Polygon : ICloneable
{ public Polygon() { points=new Point[4]; }
  public Polygon(Point p1, Point p2, Point p3) { points = new Point[3] { p1, p2, p3 }; length=3; }
  public Polygon(Point[] points) : this(points.Length) { AddPoints(points); }
  public Polygon(Point[] points, int nPoints) : this(nPoints) { AddPoints(points, nPoints); }
  public Polygon(int capacity)
  { if(capacity<0) throw new ArgumentOutOfRangeException("capacity", capacity, "must not be negative");
    int size=4;
    while(size<capacity) size*=2;
    points = new Point[size];
  }

  public Point this[int index]
  { get
    { if(index<0 || index>=length) throw new ArgumentOutOfRangeException();
      return points[index];
    }
    set
    { if(index<0 || index>=length) throw new ArgumentOutOfRangeException();
      points[index]=value;
    }
  }
  public int Length { get { return length; } }

  public int AddPoint(double x, double y) { return AddPoint(new Point(x, y)); }
  public int AddPoint(Point point)
  { if(length==points.Length) ResizeTo(length+1);
    points[length] = point;
    return length++;
  }

  public void AddPoints(Point[] points) { AddPoints(points, points.Length); }
  public void AddPoints(Point[] points, int nPoints)
  { ResizeTo(length+nPoints);
    for(int i=0; i<nPoints; i++) this.points[length++] = points[i];
  }

  public void AssertValid()
  { if(length<3) throw new InvalidOperationException("Not a valid polygon [not enough points]!");
  }

  public void Clear() { length=0; }

  public object Clone() { return new Polygon(points, length); }

  public bool ConvexContains(Point point)
  { int  sign;
    bool pos=false, neg=false;
    for(int i=0; i<length; i++)
    { sign = Math.Sign(GetEdge(i).WhichSide(point));
      if(sign==-1) { if(pos) return false; neg=true; }
      else if(sign==1) { if(neg) return false; pos=true; }
      else return false;
    }
    return true;
  }

  public bool ConvexIntersects(Line segment)
  { if(ConvexContains(segment.Start) || ConvexContains(segment.End)) return true;
    for(int i=0; i<length-1; i++)
    { LineIntersectInfo info = segment.GetIntersection(GetEdge(i));
      if(info.OnBoth) return true;
    }
    return false;
  }

  public bool ConvexIntersects(Rectangle rect)
  { return ConvexIntersects(new Line(rect.X, rect.Y, rect.Width, 0)) ||
           ConvexIntersects(new Line(rect.X, rect.Y, 0, rect.Height)) ||
           ConvexIntersects(new Line(rect.X, rect.Bottom, rect.Width, 0)) ||
           ConvexIntersects(new Line(rect.Right, rect.Y, 0, rect.Height));
  }

  public bool ConvexIntersects(Polygon poly)
  { for(int i=0; i<length-1; i++) if(poly.ConvexIntersects(GetEdge(i))) return true;
    return false;
  }

  public double GetArea()
  { double area=0;
    int i;
    for(i=0; i<length-1; i++) area += points[i].X*points[i+1].Y - points[i+1].X*points[i].Y;
    area += points[i].X*points[0].Y - points[0].X*points[i].Y;
    return Math.Abs(area)/2;
  }

  public Rectangle GetBounds()
  { Rectangle ret = new Rectangle(double.MaxValue, double.MaxValue, 0, 0);
    double x2=double.MinValue, y2=double.MinValue;
    for(int i=0; i<length; i++)
    { if(points[i].X<ret.X) ret.X = points[i].X;
      if(points[i].X>x2) x2 = points[i].X;
      if(points[i].Y<ret.Y) ret.Y = points[i].Y;
      if(points[i].Y>y2) y2 = points[i].Y;
    }
    ret.Width  = x2-ret.X;
    ret.Height = y2-ret.Y;
    return ret;
  }

  public Point GetCentroid()
  { double area=0,x=0,y=0,d;
    for(int i=0,j; i<length; i++)
    { j = i+1==length ? 0 : i+1;
      d = points[i].X*points[j].Y - points[j].X*points[i].Y;
      x += (points[i].X+points[j].X)*d;
      y += (points[i].Y+points[j].Y)*d;
      area += d;
    }
    if(area<0) { area=-area; x=-x; y=-y; }
    area *= 3;
    return new Point(x/area, y/area);
  }

  public Corner GetCorner(int index)
  { AssertValid();
    Corner c = new Corner();
    c.Point = this[index];
    c.Vector0 = GetPoint(index-1) - c.Point;
    c.Vector1 = GetPoint(index+1) - c.Point;
    return c;
  }

  public Line GetEdge(int index)
  { if(length<2) throw new InvalidOperationException("Polygon has no edges [not enough points]!");
    return Line.FromPoints(this[index], GetPoint(index+1));
  }

  public Point GetPoint(int index)
  { return index<0 ? this[length+index] : index>=length ? this[index-length] : this[index];
  }

  public void InsertPoint(Point point, int index)
  { if(length==points.Length) ResizeTo(length+1);
    if(index<length) for(int i=length; i>index; i--) points[i] = points[i-1];
    length++;
    this[index] = point;
  }

  public bool IsClockwise()
  { for(int i=0; i<length; i++)
    { int sign = Math.Sign(GetCorner(i).CrossZ);
      if(sign==1) return true;
      else if(sign==-1) return false;
    }
    return true;
  }

  public bool IsConvex()
  { bool neg=false, pos=false;
    for(int i=0; i<length; i++) 
    { double z = GetCorner(i).CrossZ;
      if(z<0)
      { if(pos) return false;
        neg=true;
      }
      else if(z>0)
      { if(neg) return false;
        pos=true;
      }
    }
    return true;
  }

  public void Offset(Vector dist) { Offset(dist.X, dist.Y); }
  public void Offset(double xd, double yd)
  { for(int i=0; i<length; i++) points[i].Offset(xd, yd);
  }

  public void RemovePoint(int index)
  { if(index<0 || index>=length) throw new ArgumentOutOfRangeException("index");
    if(index != --length) for(int i=index; i<length; i++) points[i]=points[i+1];
  }
  
  public void RemoveRange(int start, int length)
  { if(length==0) return;
    int end = start+length;
    if(start<0 || end<0 || end>this.length || start>=this.length) throw new ArgumentOutOfRangeException();
    for(; end<this.length; end++) points[end-length]=points[end];
    this.length -= length;
  }

  public void Reverse()
  { Polygon newPoly = new Polygon(length);
    for(int i=length-1; i>=0; i--) newPoly.AddPoint(points[i]);
    this.points = newPoly.points;
  }

  public Polygon Reversed()
  { Polygon newPoly = new Polygon(length);
    for(int i=length-1; i>=0; i--) newPoly.AddPoint(points[i]);
    return newPoly;
  }

  public Polygon[] SplitIntoConvexPolygons()
  { Polygon[] test = new Polygon[4], done = new Polygon[4];
    int tlen=1, dlen=0;

    test[0] = new Polygon(points, length);
    do
    { for(int pi=0,len=tlen; pi<len; pi++)
      { Polygon poly = test[pi];

        if(--tlen>0) // remove the current polygon
        { test[pi] = test[tlen];
          if(tlen<len) { pi--; len--; }
        }

        if(poly.length<3) continue;
        // remove corners with coincident/parallel edges.
        for(int ci=poly.length-2; ci>=1; ci--) if(poly.GetCorner(ci).CrossZ==0) poly.RemovePoint(ci);
        if(poly.length<3) continue;

        int sign = Math.Sign(poly.GetCorner(0).CrossZ);
        for(int ci=1; ci<poly.length; ci++)
        { Corner c = poly.GetCorner(ci);
          // if the sign is different, then the polygon is not convex, and splitting at this corner will result in
          // a simplification
          if(Math.Sign(c.CrossZ) != sign)
          { double dist = double.MaxValue, d, d2;
            Point splitPoint=new Point();
            int   splitEdge=-1, extPoint=-1, ept;
            for(int ei=0; ei<2; ei++) // try to extend each of the edges that make up this corner
            { Line toExtend = c.GetEdge(ei);
              int edge = ci-1+ei;
              for(int sei=0; sei<poly.length; sei++) // test the edge with the intersection of every other edge
              { if(sei==0) // don't try to intersect adjacent edges
                { if(edge==poly.Length-1) continue;
                }
                else if(sei==poly.Length-1)
                { if(edge==0) break;
                }
                else if(edge==sei || edge==sei-1) continue;
                LineIntersectInfo lint = toExtend.GetIntersection(poly.GetEdge(sei));
                // we don't want any points that are on the edge being extended (because it wouldn't be an extension)
                // and we want to make sure the other point is actually on the line segment
                if(!lint.Point.Valid || lint.OnFirst || !lint.OnSecond) continue;
                ept = 0;
                d  = lint.Point.DistanceSquaredTo(toExtend.GetPoint(0)); // find the shortest cut
                d2 = lint.Point.DistanceSquaredTo(toExtend.GetPoint(1));
                if(d2<d)   { d=d2; ept=1; } // 'ept' references which point gets moved to do the extension
                if(d<dist) { dist=d; splitEdge=sei; extPoint=ept; splitPoint=lint.Point; }
              }
              if(splitEdge!=-1) // if we could split it with this edge, do it. don't bother trying the other edge
              { poly.InsertPoint(splitPoint, ++splitEdge); // insert the split point
                Polygon new1 = new Polygon(), new2 = new Polygon();
                int extended = poly.Clip(ci-1+ei+extPoint), other=poly.Clip(extended+(extPoint==0 ? 1 : -1));
                int npi = splitEdge;
                if(extended>=splitEdge) { extended++; other++; }
                // 'extended' is the point that was extended. 'other' is the other side of the edge being extended
                do // circle around the polygon, starting at the new point, adding points until we hit 'extended'
                { new1.AddPoint(poly.points[npi]);
                  // if this polygon contains the edge being extended, then it must not contain the point being extended
                  if(npi==other) other=-1;
                  if(++npi>=poly.length) npi-=poly.length;
                } while(npi != extended);
                if(other!=-1) new1.AddPoint(poly.points[npi++]); // add the extended point to the appropriate polygon
                do // continue circling, adding points to the other polygon, and end by adding the split point again
                { if(npi>=poly.length) npi-=poly.length;
                  new2.AddPoint(poly.points[npi]);
                } while(npi++ != splitEdge);
                test = AddPoly(new1, test, tlen++); // add the two polygons
                test = AddPoly(new2, test, tlen++);
                goto outer; // and continue the main loop
              }
            }
            if(splitEdge==-1) // if no split points could be found, give up
              throw new NotSupportedException("Unable to split polygon. This might not be a simple polygon.");
          }
        }
        done = AddPoly(poly, done, dlen++); // all the signs are the same, it's convex, so add it to the 'done' list
        outer:;
      }
    } while(tlen>0);
    if(dlen==done.Length) return done; // return an array of the proper size
    else
    { Polygon[] narr = new Polygon[dlen];
      Array.Copy(done, narr, dlen);
      return narr;
    }
  }
  
  int Clip(int index)
  { if(index<0) index += length;
    else if(index>=length) index -= length;
    return index;
  }

  void ResizeTo(int capacity)
  { int clen = points==null ? 0 : points.Length;
    if(clen<capacity)
    { Point[] narr = new Point[Math.Max(capacity, clen*2)];
      if(length>0) Array.Copy(points, narr, length);
      points = narr;
    }
  }

  static Polygon[] AddPoly(Polygon poly, Polygon[] array, int index)
  { if(index>=array.Length)
    { Polygon[] narr = new Polygon[array.Length*2];
      Array.Copy(array, narr, array.Length);
      array=narr;
    }
    array[index] = poly;
    return array;
  }

  Point[] points;
  int length;
}
#endregion

} // namespace TwoD
#endregion

#region 3D math
namespace ThreeD
{

#region Vector
public struct Vector
{ public Vector(double x, double y, double z) { X=x; Y=y; Z=z; }
  public Vector(Point pt) { X=pt.X; Y=pt.Y; Z=pt.Z; }
  
  public double  Length
  { get { return System.Math.Sqrt(X*X+Y*Y+Z*Z); }
    set { Normalize(value); }
  }
  public double  LengthSqr { get { return X*X+Y*Y+Z*Z; } }
  public Vector Normal { get { return this/Length; } }
  
  public Vector CrossProduct(Vector v) { return new Vector(X*v.Z-Z*v.Y, Z*v.X-X*v.Z, X*v.Y-Y*v.X); }
  public double  DotProduct(Vector v) { return X*v.X + Y*v.Y + Z*v.Z; }
  public void Normalize() { Assign(Normal); }
  public void Normalize(double length) { Assign(this / (Length/length)); }

  public void Rotate(double xangle, double yangle, double zangle) { Assign(Rotated(xangle, yangle, zangle)); }
  public void RotateX(double angle) { Assign(RotatedX(angle)); } 
  public void RotateY(double angle) { Assign(RotatedY(angle)); } 
  public void RotateZ(double angle) { Assign(RotatedZ(angle)); } 
  
  public Vector Rotated(double xangle, double yangle, double zangles)
  { return RotatedX(xangle).RotatedY(xangle).RotatedZ(xangle);
  }
  public Vector RotatedX(double angle)
  { double sin = Math.Sin(angle), cos = Math.Cos(angle);
    return new Vector(X, Y*cos-Z*sin, Y*sin+Z*cos);
  }
  public Vector RotatedY(double angle)
  { double sin = Math.Sin(angle), cos = Math.Cos(angle);
    return new Vector(Z*sin+X*cos, Y, Z*cos-X*sin);
  }
  public Vector RotatedZ(double angle)
  { double sin = Math.Sin(angle), cos = Math.Cos(angle);
    return new Vector(X*cos-Y*sin, X*sin+Y*cos, Z);
  }
  
  public override bool Equals(object obj) { return obj is Vector ? (Vector)obj==this : false; }
  public bool Equals(Vector vect, double epsilon)
  { return Math.Abs(vect.X-X)<=epsilon && Math.Abs(vect.Y-Y)<=epsilon && Math.Abs(vect.Z-Z)<=epsilon;
  }
  public override int GetHashCode()
  { unsafe { fixed(Vector* v=&this) return *(int*)&v->X ^ *(int*)&v->Y ^ *(int*)&v->Z; }
  }
  public Point ToPoint() { return new Point(X, Y, Z); }
  public override string ToString() { return string.Format("[{0:f2},{1:f2},{2:f2}]", X, Y, Z); }

  public static Vector operator-(Vector v) { return new Vector(-v.X, -v.Y, -v.Z); }
  public static Vector operator+(Vector a, Vector b) { return new Vector(a.X+b.X, a.Y+b.Y, a.Z+b.Z); }
  public static Vector operator-(Vector a, Vector b) { return new Vector(a.X-b.X, a.Y-b.Y, a.Z-b.Z); }
  public static Vector operator*(Vector a, Vector b) { return new Vector(a.X*b.X, a.Y*b.Y, a.Z*b.Z); }
  public static Vector operator/(Vector a, Vector b) { return new Vector(a.X/b.X, a.Y/b.Y, a.Z/b.Z); }
  public static Vector operator*(Vector v, double f)   { return new Vector(v.X*f, v.Y*f, v.Z*f); }
  public static Vector operator/(Vector v, double f)   { return new Vector(v.X/f, v.Y/f, v.Z/f); }
  public static bool   operator==(Vector a, Vector b) { return a.X==b.X && a.Y==b.Y && a.Z==b.Z; }
  public static bool   operator!=(Vector a, Vector b) { return a.X!=b.X || a.Y!=b.Y || a.Z!=b.Z; }
  
  public double X, Y, Z;
  
  void Assign(Vector v) { X=v.X; Y=v.Y; Z=v.Z; }
}
#endregion

#region Point
public struct Point
{ public Point(double x, double y, double z) { X=x; Y=y; Z=z; }

  public double DistanceTo(Point point)
  { double xd=point.X-X, yd=point.Y-Y, zd=point.Z-Z;
    return Math.Sqrt(xd*xd+yd*yd+zd*zd);
  }
  public double DistanceCubedTo(Point point)
  { double xd=point.X-X, yd=point.Y-Y, zd=point.Z-Z;
    return xd*xd+yd*yd+zd*zd;
  }

  public void Offset(double xd, double yd, double zd) { X+=xd; Y+=yd; Z+=zd; }

  public override bool Equals(object obj) { return obj is Point ? (Point)obj==this : false; }
  public bool Equals(Point point, double epsilon)
  { return Math.Abs(point.X-X)<=epsilon && Math.Abs(point.Y-Y)<=epsilon && Math.Abs(point.Z-Z)<=epsilon;
  }
  public override int GetHashCode()
  { unsafe { fixed(Point* v=&this) return *(int*)&v->X ^ *(int*)&v->Y ^ *(int*)&v->Z; }
  }
  public override string ToString() { return string.Format("({0:f2},{1:f2},{2:f2})", X, Y, Z); }

  public static Vector operator-(Point lhs, Point rhs)  { return new Vector(lhs.X-rhs.X, lhs.Y-rhs.Y, lhs.Z-rhs.Z); }
  public static Point  operator-(Point lhs, Vector rhs) { return new Point(lhs.X-rhs.X, lhs.Y-rhs.Y, lhs.Z-rhs.Z); }
  public static Point  operator+(Point lhs, Vector rhs) { return new Point(lhs.X+rhs.X, lhs.Y+rhs.Y, lhs.Z+rhs.Z); }
  public static bool   operator==(Point lhs, Point rhs) { return lhs.X==rhs.X && lhs.Y==rhs.Y && lhs.Z==rhs.Z; }
  public static bool   operator!=(Point lhs, Point rhs) { return lhs.X!=rhs.X || lhs.Y!=rhs.Y || lhs.Z!=rhs.Z; }
  
  public double X, Y, Z;
}
#endregion

#region Line
public struct Line
{ public Line(double x, double y, double z, double xd, double yd, double zd) { Start=new Point(x, y, z); Vector=new Vector(xd, yd, zd); }
  public Line(Point start, Vector vector) { Start=start; Vector=vector; }

  public Point End { get { return Start+Vector; } }
  public double Length { get { return Vector.Length; } }
  public double LengthSqr { get { return Vector.LengthSqr; } }

  public Point GetPoint(int point)
  { if(point<0 || point>1) throw new ArgumentOutOfRangeException("point", point, "must be 0 or 1");
    return point==0 ? Start : End;
  }

  public override bool Equals(object obj) { return obj is Line ? (Line)obj==this : false; }
  public bool Equals(Line line, double epsilon)
  { return Start.Equals(line.Start, epsilon) && Vector.Equals(line.Vector, epsilon);
  }
  public override int GetHashCode() { return Start.GetHashCode() ^ Vector.GetHashCode(); }
  public override string ToString() { return string.Format("{0}->{1}", Start, Vector); }

  public static Line FromPoints(Point start, Point end) { return new Line(start, end-start); }
  public static Line FromPoints(double x1, double y1, double z1, double x2, double y2, double z2)
  { return new Line(x1, y1, z1, x2-x1, y2-y1, z2-z1);
  }

  public static bool operator==(Line lhs, Line rhs) { return lhs.Start==rhs.Start && lhs.Vector==rhs.Vector; }
  public static bool operator!=(Line lhs, Line rhs) { return lhs.Start!=rhs.Start || lhs.Vector!=rhs.Vector; }

  public Point  Start;
  public Vector Vector;
}
#endregion

public struct Plane
{ public Point  Point;
  public Vector Normal;
}

public struct Sphere
{ public Point Center;
  public double Radius;
}

} // namespace ThreeD
#endregion

} // namespace GameLib.Mathematics
