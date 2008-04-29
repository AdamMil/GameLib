/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2007 Adam Milazzo

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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace GameLib.Mathematics
{

#region MathConst
/// <summary>This class provides some useful constants for math operations.</summary>
public static class MathConst
{ 
  /// <summary>A value that can be used to convert degrees to radians.</summary>
  /// <remarks>If you multiply a degree value by this constant, it will be converted to radians.</remarks>
  public const double DegreesToRadians = Math.PI/180;
  /// <summary>A value that can be used to convert radians to degrees.</summary>
  /// <remarks>If you multiply a radian value by this constant, it will be converted to degrees.</remarks>
  public const double RadiansToDegrees = 180/Math.PI;
}
#endregion

#region GLMath
/// <summary>This class provides some helpful mathematical functions.</summary>
public static class GLMath
{
  /// <summary>Precalculates the sine and cosine factors used for rotation by a given angle, so that multiple points
  /// can be rotated without recalculating the factors.
  /// </summary>
  /// <param name="angle">The angle to rotate by, in radians.</param>
  /// <param name="sin">A variable that will be set to the sine factor.</param>
  /// <param name="cos">A variable that will be set to the cosine factor.</param>
  /// <remarks>These factors can be used with <see cref="TwoD.Math2D.Rotate"/> functions to rotate points and vertices.</remarks>
  public static void GetRotationFactors(double angle, out double sin, out double cos)
  {
    sin = Math.Sin(angle);
    cos = Math.Cos(angle);
  }

  /// <summary>Normalizes an angle to a value from 0 to 2pi (exclusive).</summary>
  /// <param name="angle">The angle to normalize, in radians.</param>
  public static double NormalizeAngle(double angle)
  {
    if(angle < 0)
    {
      do angle += Math.PI*2; while(angle < 0);
    }
    else if(angle >= Math.PI*2)
    {
      do angle -= Math.PI*2; while(angle >= Math.PI*2);
    }

    return angle;
  }

  /// <summary>Performs integer division that rounds towards lower numbers rather than towards zero.</summary>
  /// <param name="numerator">The numerator.</param>
  /// <param name="denominator">The denominator.</param>
  /// <returns><paramref name="numerator"/> divided by <paramref name="denominator"/>, rounded towards lower numbers
  /// rather than truncated towards zero.
  /// </returns>
  public static int FloorDiv(int numerator, int denominator)
  { return (numerator<0 ? (numerator-denominator+(denominator<0 ? -1 : 1)) : numerator) / denominator;
  }

  /// <summary>Returns the absolute value of a <see cref="Fixed32"/>.</summary>
  /// <param name="value">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the absolute value.</returns>
  public static Fixed32 Abs(Fixed32 value) { return value.value<0 ? new Fixed32((uint)-value.value) : value; }
  /// <summary>Returns the arc-cosine of a <see cref="Fixed32"/>.</summary>
  /// <param name="value">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the arc-cosine.</returns>
  public static Fixed32 Acos(Fixed32 value) { return new Fixed32(Math.Acos(value.ToDouble())); }
  /// <summary>Returns the arc-sine of a <see cref="Fixed32"/>.</summary>
  /// <param name="value">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the arc-sine.</returns>
  public static Fixed32 Asin(Fixed32 value) { return new Fixed32(Math.Asin(value.ToDouble())); }
  /// <summary>Returns the arc-tangent of a <see cref="Fixed32"/>.</summary>
  /// <param name="value">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the arc-tangent.</returns>
  public static Fixed32 Atan(Fixed32 value) { return new Fixed32(Math.Atan(value.ToDouble())); }
  /// <summary>Returns the ceiling of a <see cref="Fixed32"/>.</summary>
  /// <param name="value">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the smallest whole number greater than or equal to the given
  /// number.
  /// </returns>
  public static Fixed32 Ceiling(Fixed32 value) { return value.Ceiling; }
  /// <summary>Returns the cosine of a <see cref="Fixed32"/>.</summary>
  /// <param name="value">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the cosine.</returns>
  public static Fixed32 Cos(Fixed32 value) { return new Fixed32(Math.Cos(value.ToDouble())); }
  /// <summary>Returns the floor of a <see cref="Fixed32"/>.</summary>
  /// <param name="value">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the largest whole number less than or equal to the given
  /// number.
  /// </returns>
  public static Fixed32 Floor(Fixed32 value) { return value.Floor; }
  /// <summary>Returns the rounded value of a <see cref="Fixed32"/>.</summary>
  /// <param name="value">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the rounded value.</returns>
  /// <remarks>This method performs banker's rounding, so values with a fractional part of exactly 0.5 will be
  /// rounded towards the nearest even number, or towards zero.
  /// </remarks>
  public static Fixed32 Round(Fixed32 value) { return value.Rounded; }
  /// <summary>Returns the sine of a <see cref="Fixed32"/>.</summary>
  /// <param name="value">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the sine.</returns>
  public static Fixed32 Sin(Fixed32 value) { return new Fixed32(Math.Sin(value.ToDouble())); }
  /// <summary>Returns the square root of a <see cref="Fixed32"/>.</summary>
  /// <param name="value">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the square root.</returns>
  public static Fixed32 Sqrt(Fixed32 value) { return new Fixed32(Math.Sqrt(value.ToDouble())); }
  /// <summary>Returns the tangent of a <see cref="Fixed32"/>.</summary>
  /// <param name="value">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the tangent.</returns>
  public static Fixed32 Tan(Fixed32 value) { return new Fixed32(Math.Tan(value.ToDouble())); }
  /// <summary>Returns the truncated value of a <see cref="Fixed32"/>.</summary>
  /// <param name="value">A <see cref="Fixed32"/> value.</param>
  /// <returns>A new <see cref="Fixed32"/> containing the value truncated towards zero.</returns>
  public static Fixed32 Truncate(Fixed32 value) { return value.Truncated; }

  /// <summary>Returns the absolute value of a <see cref="Fixed64"/>.</summary>
  /// <param name="value">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the absolute value.</returns>
  public static Fixed64 Abs(Fixed64 value) { return value.value<0 ? new Fixed64(-value.value) : value; }
  /// <summary>Returns the arc-cosine of a <see cref="Fixed64"/>.</summary>
  /// <param name="value">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the arc-cosine.</returns>
  public static Fixed64 Acos(Fixed64 value) { return new Fixed64(Math.Acos(value.ToDouble())); }
  /// <summary>Returns the arc-sine of a <see cref="Fixed64"/>.</summary>
  /// <param name="value">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the arc-sine.</returns>
  public static Fixed64 Asin(Fixed64 value) { return new Fixed64(Math.Asin(value.ToDouble())); }
  /// <summary>Returns the arc-tangent of a <see cref="Fixed64"/>.</summary>
  /// <param name="value">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the arc-tangent.</returns>
  public static Fixed64 Atan(Fixed64 value) { return new Fixed64(Math.Atan(value.ToDouble())); }
  /// <summary>Returns the ceiling of a <see cref="Fixed64"/>.</summary>
  /// <param name="value">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the smallest whole number greater than or equal to the given
  /// number.
  /// </returns>
  public static Fixed64 Ceiling(Fixed64 value) { return value.Ceiling; }
  /// <summary>Returns the cosine of a <see cref="Fixed64"/>.</summary>
  /// <param name="value">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the cosine.</returns>
  public static Fixed64 Cos(Fixed64 value) { return new Fixed64(Math.Cos(value.ToDouble())); }
  /// <summary>Returns the floor of a <see cref="Fixed64"/>.</summary>
  /// <param name="value">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the largest whole number less than or equal to the given
  /// number.
  /// </returns>
  public static Fixed64 Floor(Fixed64 value) { return value.Floor; }
  /// <summary>Returns the rounded value of a <see cref="Fixed64"/>.</summary>
  /// <param name="value">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the rounded value.</returns>
  /// <remarks>This method performs banker's rounding, so values with a fractional part of exactly 0.5 will be
  /// rounded towards the nearest even number, or towards zero.
  /// </remarks>
  public static Fixed64 Round(Fixed64 value) { return value.Rounded; }
  /// <summary>Returns the sine of a <see cref="Fixed64"/>.</summary>
  /// <param name="value">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the sine.</returns>
  public static Fixed64 Sin(Fixed64 value) { return new Fixed64(Math.Sin(value.ToDouble())); }
  /// <summary>Returns the square root of a <see cref="Fixed64"/>.</summary>
  /// <param name="value">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the square root.</returns>
  public static Fixed64 Sqrt(Fixed64 value) { return new Fixed64(Math.Sqrt(value.ToDouble())); }
  /// <summary>Returns the tangent of a <see cref="Fixed64"/>.</summary>
  /// <param name="value">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the tangent.</returns>
  public static Fixed64 Tan(Fixed64 value) { return new Fixed64(Math.Tan(value.ToDouble())); }
  /// <summary>Returns the truncated value of a <see cref="Fixed64"/>.</summary>
  /// <param name="value">A <see cref="Fixed64"/> value.</param>
  /// <returns>A new <see cref="Fixed64"/> containing the value truncated towards zero.</returns>
  public static Fixed64 Truncate(Fixed64 value) { return value.Truncated; }
}
#endregion

#region Fixed32
/// <summary>This class provides a fixed-point numeric type with 32 bits of storage in a 16.16 configuration.</summary>
/// <remarks>
/// <para>Floating point math on modern systems is very fast, and I wouldn't recommend using this fixed-point math
/// class for speed. The primary benefit of fixed-point math is that it provides consistency and reliability. Floating
/// point math loses precision when dealing with larger numbers, and the results of arithmetic operations are not
/// always consistent due to precision mismatch between operands. Fixed-point math eliminates these inconsistencies.
/// </para>
/// <para>This class provides 16 bits for the whole part and 16 bits for the fractional part, so the total range is
/// -32768 to approximately 32767.99998.
/// </para>
/// </remarks>
// TODO: use a union like Fixed64 does
[Serializable, StructLayout(LayoutKind.Sequential)]
public struct Fixed32 : IFormattable, IComparable, IConvertible
{ 
  /// <summary>Initializes this fixed-point number from a floating point number.</summary>
  /// <param name="value">A floating point number from which the fixed-point number will be initialized.</param>
  /// <remarks>Due to the greater range and potential precision of a 64-bit double, the value passed may not be
  /// able to be accurately represented.
  /// </remarks>
  /// <exception cref="OverflowException">Thrown if <paramref name="value"/> cannot be represented by this type.</exception>
  public Fixed32(double value) { this.value = FromDouble(value); }
  /// <summary>Initializes this fixed-point class from an integer.</summary>
  /// <param name="value">An integer from which the fixed-point number will be initialized.</param>
  /// <exception cref="OverflowException">Thrown if <paramref name="value"/> cannot be represented by this type.</exception>
  public Fixed32(int value)
  { if(value<-32768 || value>32767) throw new OverflowException();
    this.value = value<<16;
  }
  internal Fixed32(uint value) { this.value = (int)value; } // ugly, but 'int' was already taken

  /// <summary>Gets this number's absolute value.</summary>
  /// <value>A new fixed-point number containing the absolute value of this number.</value>
  public Fixed32 Abs { get { return value<0 ? new Fixed32((uint)-value) : this; } }
  /// <summary>Gets this number's ceiling.</summary>
  /// <value>A new fixed-point number containing the smallest whole number greater than or equal to the current value.</value>
  public Fixed32 Ceiling { get { return new Fixed32((uint)((value+(OneVal-1)) & Trunc)); } }
  /// <summary>Gets this number's floor.</summary>
  /// <value>A new fixed-point number containing the largest whole number less than or equal to the current value.</value>
  public Fixed32 Floor { get { return new Fixed32((uint)(value&Trunc)); } }
  /// <summary>Gets this number's value, rounded.</summary>
  /// <remarks>This method performs banker's rounding, so values with a fractional part of exactly 0.5 will be
  /// rounded towards the nearest even number, or towards zero.
  /// </remarks>
  public Fixed32 Rounded
  { get
    { ushort fp = (ushort)value;
      if(fp<0x8000) return new Fixed32((uint)(value&Trunc));
      else if(fp>0x8000 || (value&OneVal)!=0) return new Fixed32((uint)((value+OneVal)&Trunc));
      else return new Fixed32((uint)(value&Trunc));
    }
  }
  /// <summary>Gets this number's square root.</summary>
  /// <value>A new fixed-point number containing the square root of the current value.</value>
  public Fixed32 Sqrt { get { return new Fixed32(Math.Sqrt(ToDouble())); } }
  /// <summary>Gets this number's value, truncated towards zero.</summary>
  /// <value>A new fixed-point number containing the current value truncated towards zero.</value>
  public Fixed32 Truncated { get { return new Fixed32((uint)((value<0 ? value+(OneVal-1) : value)&Trunc)); } }
  /// <summary>Returns true if this object is equal to the given object.</summary>
  /// <param name="obj">The object to compare against.</param>
  /// <returns>True if <paramref name="obj"/> is a <see cref="Fixed32"/> and has the same value as this one.</returns>
  public override bool Equals(object obj)
  { if(!(obj is Fixed32)) return false;
    return value == ((Fixed32)obj).value;
  }
  /// <summary>Returns a hash code for this <see cref="Fixed32"/>.</summary>
  /// <returns>An integer hash code for this <see cref="Fixed32"/>.</returns>
  public override int GetHashCode() { return value; }
  /// <summary>Converts this <see cref="Fixed32"/> to a <see cref="Fixed64"/>.</summary>
  /// <returns>A <see cref="Fixed64"/> containing the same value.</returns>
  public Fixed64 ToFixed64() { return new Fixed64(((long)(value&Trunc)<<16) | (uint)((ushort)value<<16)); }
  /// <summary>Converts this fixed-point number to a floating-point number.</summary>
  /// <returns>The double value closest to this fixed-point number.</returns>
  public double ToDouble() { return (value>>16) + (ushort)value*0.0000152587890625; } // 1 / (1<<16)
  /// <summary>Returns the integer portion of the fixed-point number.</summary>
  /// <returns>The integer portion of the fixed-point number.</returns>
  public int ToInt()
  { int ret = value>>16;
    if(ret<0 && (ushort)value!=0) ret++;
    return ret;
  }
  /// <summary>Converts this fixed-point number into a string.</summary>
  /// <returns>A string representing this fixed-point number.</returns>
  public override string ToString() { return ToString(null, null); }
  /// <include file="../documentation.xml" path="//Mathematics/Fixed/ToString1/*"/>
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
  public static Fixed32 operator-(Fixed32 val) { return new Fixed32((uint)-val.value); }

  public static Fixed32 operator+(Fixed32 lhs, int rhs) { return new Fixed32((uint)(lhs.value+(rhs<<16))); }
  public static Fixed32 operator-(Fixed32 lhs, int rhs) { return new Fixed32((uint)(lhs.value-(rhs<<16))); }
  public static Fixed32 operator*(Fixed32 lhs, int rhs) { return new Fixed32((uint)(lhs.value*rhs)); }
  public static Fixed32 operator/(Fixed32 lhs, int rhs) { return new Fixed32((uint)(lhs.value/rhs)); }

  public static Fixed32 operator+(Fixed32 lhs, double rhs) { return new Fixed32((uint)(lhs.value+FromDouble(rhs))); }
  public static Fixed32 operator-(Fixed32 lhs, double rhs) { return new Fixed32((uint)(lhs.value-FromDouble(rhs))); }
  public static Fixed32 operator*(Fixed32 lhs, double rhs) { return new Fixed32((uint)(((long)lhs.value*FromDouble(rhs))>>16)); }
  public static Fixed32 operator/(Fixed32 lhs, double rhs) { return new Fixed32((uint)(((long)lhs.value<<16)/FromDouble(rhs))); }

  public static Fixed32 operator+(int lhs, Fixed32 rhs) { return new Fixed32((uint)((lhs<<16)+rhs.value)); }
  public static Fixed32 operator-(int lhs, Fixed32 rhs) { return new Fixed32((uint)((lhs<<16)-rhs.value)); }
  public static Fixed32 operator*(int lhs, Fixed32 rhs) { return new Fixed32((uint)(lhs*rhs.value)); }
  public static Fixed32 operator/(int lhs, Fixed32 rhs) { return new Fixed32((uint)(((long)lhs<<32)/rhs.value)); }

  public static Fixed32 operator+(double lhs, Fixed32 rhs) { return new Fixed32((uint)(FromDouble(lhs)+rhs.value)); }
  public static Fixed32 operator-(double lhs, Fixed32 rhs) { return new Fixed32((uint)(FromDouble(lhs)-rhs.value)); }
  public static Fixed32 operator*(double lhs, Fixed32 rhs) { return new Fixed32((uint)(((long)FromDouble(lhs)*rhs.value)>>16)); }
  public static Fixed32 operator/(double lhs, Fixed32 rhs) { return new Fixed32((uint)(((long)FromDouble(lhs)<<16)/rhs.value)); }

  public static Fixed32 operator+(Fixed32 lhs, Fixed32 rhs) { return new Fixed32((uint)(lhs.value+rhs.value)); }
  public static Fixed32 operator-(Fixed32 lhs, Fixed32 rhs) { return new Fixed32((uint)(lhs.value-rhs.value)); }
  public static Fixed32 operator*(Fixed32 lhs, Fixed32 rhs) { return new Fixed32((uint)(((long)lhs.value*rhs.value)>>16)); }
  public static Fixed32 operator/(Fixed32 lhs, Fixed32 rhs) { return new Fixed32((uint)(((long)lhs.value<<16)/rhs.value)); }
  #endregion

  #region Comparison operators
  public static bool operator<(Fixed32 lhs, Fixed32 rhs) { return lhs.value<rhs.value; }
  public static bool operator<=(Fixed32 lhs, Fixed32 rhs) { return lhs.value<=rhs.value; }
  public static bool operator>(Fixed32 lhs, Fixed32 rhs) { return lhs.value>rhs.value; }
  public static bool operator>=(Fixed32 lhs, Fixed32 rhs) { return lhs.value>=rhs.value; }
  public static bool operator==(Fixed32 lhs, Fixed32 rhs) { return lhs.value==rhs.value; }
  public static bool operator!=(Fixed32 lhs, Fixed32 rhs) { return lhs.value!=rhs.value; }

  public static bool operator<(Fixed32 lhs, int rhs) { return lhs.value<(rhs<<16); }
  public static bool operator<=(Fixed32 lhs, int rhs) { return lhs.value<=(rhs<<16); }
  public static bool operator>(Fixed32 lhs, int rhs) { return lhs.value>(rhs<<16); }
  public static bool operator>=(Fixed32 lhs, int rhs) { return lhs.value>=(rhs<<16); }
  public static bool operator==(Fixed32 lhs, int rhs) { return lhs.value==(rhs<<16); }
  public static bool operator!=(Fixed32 lhs, int rhs) { return lhs.value!=(rhs<<16); }

  public static bool operator<(Fixed32 lhs, double rhs) { return lhs.ToDouble()<rhs; }
  public static bool operator<=(Fixed32 lhs, double rhs) { return lhs.ToDouble()<=rhs; }
  public static bool operator>(Fixed32 lhs, double rhs) { return lhs.ToDouble()>rhs; }
  public static bool operator>=(Fixed32 lhs, double rhs) { return lhs.ToDouble()>=rhs; }
  public static bool operator==(Fixed32 lhs, double rhs) { return lhs.ToDouble()==rhs; }
  public static bool operator!=(Fixed32 lhs, double rhs) { return lhs.ToDouble()!=rhs; }

  public static bool operator<(int lhs, Fixed32 rhs) { return (lhs<<16)<rhs.value; }
  public static bool operator<=(int lhs, Fixed32 rhs) { return (lhs<<16)<=rhs.value; }
  public static bool operator>(int lhs, Fixed32 rhs) { return (lhs<<16)>rhs.value; }
  public static bool operator>=(int lhs, Fixed32 rhs) { return (lhs<<16)>=rhs.value; }
  public static bool operator==(int lhs, Fixed32 rhs) { return (lhs<<16)==rhs.value; }
  public static bool operator!=(int lhs, Fixed32 rhs) { return (lhs<<16)!=rhs.value; }

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
  /// <exception cref="OverflowException">Thrown if <paramref name="value"/> cannot be represented by this type.</exception>
  public static implicit operator Fixed32(int i)
  { if(i<-32768 || i>32767) throw new OverflowException();
    return new Fixed32((uint)(i<<16));
  }
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
  /// <summary>PI*2, approximately 6.283185.</summary>
  public static readonly Fixed32 TwoPI = new Fixed32((uint)(393216 + 18559));
  /// <summary>Zero.</summary>
  public static readonly Fixed32 Zero = new Fixed32((uint)0);
  #endregion

  #region IComparable Members
  /// <include file="../documentation.xml" path="//IComparable/CompareTo/*"/>
  public int CompareTo(object obj)
  { if(obj==null) return 1;
    if(obj is Fixed32) { return value-((Fixed32)obj).value; }
    throw new ArgumentException("'obj' is not a Fixed32");
  }
  #endregion

  #region IConvertible Members
  // FIXME: these should probably do rounding
  /// <include file="../documentation.xml" path="//IConvertible/ToUInt64/*"/>
  [CLSCompliant(false)]
  public ulong ToUInt64(IFormatProvider provider)
  { int n = ToInt();
    if(n<0) throw new OverflowException();
    return (ulong)n;
  }
  /// <include file="../documentation.xml" path="//IConvertible/ToSByte/*"/>
  [CLSCompliant(false)]
  public sbyte ToSByte(IFormatProvider provider)
  { int n = ToInt();
    if(n<sbyte.MinValue || n>sbyte.MaxValue) throw new OverflowException();
    return (sbyte)n;
  }
  /// <include file="../documentation.xml" path="//IConvertible/ToDouble/*"/>
  public double ToDouble(IFormatProvider provider) { return ToDouble(); }
  /// <include file="../documentation.xml" path="//Mathematics/Fixed/ToDateTime/*"/>
  public DateTime ToDateTime(IFormatProvider provider) { throw new InvalidCastException(); }
  /// <include file="../documentation.xml" path="//IConvertible/ToSingle/*"/>
  public float ToSingle(IFormatProvider provider)
  { double d = ToDouble();
    if(d<float.MinValue || d>float.MaxValue) throw new OverflowException();
    return (float)d;
  }
  /// <include file="../documentation.xml" path="//Mathematics/Fixed/ToBoolean/*"/>
  public bool ToBoolean(IFormatProvider provider) { return value==0; }
  /// <include file="../documentation.xml" path="//IConvertible/ToInt32/*"/>
  public int ToInt32(IFormatProvider provider) { return ToInt(); }
  /// <include file="../documentation.xml" path="//IConvertible/ToUInt16/*"/>
  [CLSCompliant(false)]
  public ushort ToUInt16(IFormatProvider provider)
  { int n = ToInt();
    if(n<ushort.MinValue || n>ushort.MaxValue) throw new OverflowException();
    return (ushort)n;
  }
  /// <include file="../documentation.xml" path="//IConvertible/ToInt16/*"/>
  public short ToInt16(IFormatProvider provider)
  { int n = ToInt();
    if(n<short.MinValue || n>short.MaxValue) throw new OverflowException();
    return (short)n;
  }
  /// <include file="../documentation.xml" path="//IConvertible/ToString/*"/>
  public string ToString(IFormatProvider provider) { return ToString(null, provider); }
  /// <include file="../documentation.xml" path="//IConvertible/ToByte/*"/>
  public byte ToByte(IFormatProvider provider)
  { int n = ToInt();
    if(n<byte.MinValue || n>byte.MaxValue) throw new OverflowException();
    return (byte)n;
  }
  /// <include file="../documentation.xml" path="//IConvertible/ToChar/*"/>
  public char ToChar(IFormatProvider provider)
  { int n = ToInt();
    if(n<ushort.MinValue || n>ushort.MaxValue) throw new OverflowException();
    return (char)n;
  }
  /// <include file="../documentation.xml" path="//IConvertible/ToInt64/*"/>
  public long ToInt64(IFormatProvider provider) { return ToInt(); }
  /// <summary>Returns the <see cref="TypeCode"/> for the <see cref="Fixed32"/> type.</summary>
  /// <returns>Returns <see cref="TypeCode.Object"/>.</returns>
  public TypeCode GetTypeCode() { return TypeCode.Object; }
  /// <include file="../documentation.xml" path="//IConvertible/ToDecimal/*"/>
  public decimal ToDecimal(IFormatProvider provider) { return new decimal(ToDouble()); }
  /// <include file="../documentation.xml" path="//IConvertible/ToType/*"/>
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
  /// <include file="../documentation.xml" path="//IConvertible/ToUInt32/*"/>
  [CLSCompliant(false)]
  public uint ToUInt32(IFormatProvider provider)
  { int n = ToInt();
    if(n<0) throw new OverflowException();
    return (uint)n;
  }
  #endregion

  #region IFormattable Members
  /// <include file="../documentation.xml" path="//Mathematics/Fixed/ToString2/*"/>
  public string ToString(string format, IFormatProvider provider)
  { if(format==null) return ToDouble().ToString();
    if(format.Length==0 || char.ToUpper(format[0])!='R') return ToDouble().ToString(format);
    return (value>>16).ToString() + '/' + ((ushort)value).ToString();
  }
  #endregion

  const int Trunc  = unchecked((int)0xFFFF0000);
  const int OneVal = 0x10000;

  static int FromDouble(double value)
  { if(value<-32768 || value>=32768) throw new OverflowException();
    int whole = (int)value;
    int fp = (int)(Math.IEEERemainder(value, 1)*65536.0) & ~Trunc;
    if(whole<0 && fp!=0) fp ^= Trunc;
    return (whole<<16) + fp;
  }

  internal int value;
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
/// -2147483648 to approximately 2147483647.9999999998.
/// </para>
/// </remarks>
[Serializable, StructLayout(LayoutKind.Explicit, Size=8)]
public struct Fixed64 : IFormattable, IComparable, IConvertible
{ 
  /// <summary>Initializes this fixed-point class from a floating point number.</summary>
  /// <param name="value">A floating point number from which the fixed-point number will be initialized.</param>
  /// <remarks>Due to the greater range of a 64-bit double, the value passed may not be able to be accurately
  /// represented.
  /// </remarks>
  /// <exception cref="OverflowException">Thrown if <paramref name="value"/> cannot be represented by this type.</exception>
  public Fixed64(double value) { wholePart=0; this.value=FromDouble(value); } // damn C# requires all fields to be set
  /// <summary>Initializes this fixed-point class from an integer.</summary>
  /// <param name="value">An integer from which the fixed-point number will be initialized.</param>
  public Fixed64(int value) { this.value=0; wholePart=value; }
  internal Fixed64(long value) { wholePart=0; this.value=value; }
  internal Fixed64(int whole, uint frac) { value=frac; wholePart=whole; }
  /// <summary>Gets this number's absolute value.</summary>
  /// <value>A new fixed-point number containing the absolute value of this number.</value>
  public Fixed64 Abs { get { return value<0 ? new Fixed64(-value) : this; } }
  /// <summary>Gets this number's ceiling.</summary>
  /// <value>A new fixed-point number containing the smallest whole number greater than or equal to the current value.</value>
  public Fixed64 Ceiling { get { return new Fixed64((value+(OneVal-1)) & Trunc); } }
  /// <summary>Gets this number's floor.</summary>
  /// <value>A new fixed-point number containing the largest whole number less than or equal to the current value.</value>
  public Fixed64 Floor { get { return new Fixed64(value&Trunc); } }
  /// <summary>Gets this number's value, rounded.</summary>
  /// <remarks>This method performs banker's rounding, so values with a fractional part of exactly 0.5 will be
  /// rounded towards the nearest even number, or towards zero.
  /// </remarks>
  public Fixed64 Rounded
  { get
    { uint fp = (uint)value;
      if(fp<0x80000000) return new Fixed64(value&Trunc);
      else if(fp>0x80000000 || (value&OneVal)!=0) return new Fixed64((value+OneVal)&Trunc);
      else return new Fixed64(value&Trunc);
    }
  }
  /// <summary>Gets this number's square root.</summary>
  /// <value>A new fixed-point number containing the square root of the current value.</value>
  public Fixed64 Sqrt { get { return new Fixed64(Math.Sqrt(ToDouble())); } }
  /// <summary>Gets this number's value, truncated towards zero.</summary>
  /// <value>A new fixed-point number containing the current value truncated towards zero.</value>
  public Fixed64 Truncated { get { return new Fixed64((value<0 ? value+(OneVal-1) : value)&Trunc); } }
  /// <summary>Returns true if this object is equal to the given object.</summary>
  /// <param name="obj">The object to compare against.</param>
  /// <returns>True if <paramref name="obj"/> is a <see cref="Fixed64"/> and has the same value as this one.</returns>
  public override bool Equals(object obj)
  { if(!(obj is Fixed64)) return false;
    return value == ((Fixed64)obj).value;
  }
  /// <summary>Returns a hash code for this <see cref="Fixed64"/>.</summary>
  /// <returns>An integer hash code for this <see cref="Fixed64"/>.</returns>
  public override int GetHashCode() { return wholePart ^ (int)(uint)value; }
  /// <summary>Converts this <see cref="Fixed64"/> to a <see cref="Fixed32"/>.</summary>
  /// <returns>A <see cref="Fixed32"/> containing the approximately the same value.</returns>
  /// <remarks>Due to the greater precision of the <see cref="Fixed64"/> class, the fractional part of the resulting
  /// value may not be exactly the same.
  /// </remarks>
  /// <exception cref="OverflowException">Thrown if the value is outside the range of a <see cref="Fixed32"/>.</exception>
  public Fixed32 ToFixed32()
  { if(wholePart<short.MinValue || wholePart>short.MaxValue) throw new OverflowException();
    return new Fixed32((uint)((short)wholePart<<16) | ((uint)value>>16));
  }
  /// <summary>Converts this fixed-point number to a floating-point number.</summary>
  /// <returns>The double value closest to this fixed-point number.</returns>
  public double ToDouble() { return wholePart + (uint)value*0.00000000023283064365386962890625; } // 1 / (1<<32)
  /// <summary>Returns the integer portion of the fixed-point number.</summary>
  /// <returns>The integer portion of the fixed-point number.</returns>
  public int ToInt()
  { int ret = wholePart;
    if(ret<0 && (uint)value!=0) ret++;
    return ret;
  }
  /// <summary>Converts this fixed-point number into a string.</summary>
  /// <returns>A string representing this fixed-point number.</returns>
  public override string ToString() { return ToString(null, null); }
  /// <include file="../documentation.xml" path="//Mathematics/Fixed/ToString1/*"/>
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
  public static Fixed64 operator-(Fixed64 val) { return new Fixed64(-val.value); }

  public static Fixed64 operator+(Fixed64 lhs, int rhs) { return new Fixed64(lhs.value+((long)rhs<<32)); }
  public static Fixed64 operator-(Fixed64 lhs, int rhs) { return new Fixed64(lhs.value-((long)rhs<<32)); }
  public static Fixed64 operator*(Fixed64 lhs, int rhs) { return new Fixed64(lhs.value*rhs); }
  public static Fixed64 operator/(Fixed64 lhs, int rhs) { return new Fixed64(lhs.value/rhs); }

  public static Fixed64 operator+(Fixed64 lhs, double rhs) { return new Fixed64(lhs.value+FromDouble(rhs)); }
  public static Fixed64 operator-(Fixed64 lhs, double rhs) { return new Fixed64(lhs.value-FromDouble(rhs)); }
  public static Fixed64 operator*(Fixed64 lhs, double rhs) { return lhs * new Fixed64(rhs); }
  public static Fixed64 operator/(Fixed64 lhs, double rhs) { return lhs / new Fixed64(rhs); }

  public static Fixed64 operator+(int lhs, Fixed64 rhs) { return new Fixed64(((long)lhs<<32)+rhs.value); }
  public static Fixed64 operator-(int lhs, Fixed64 rhs) { return new Fixed64(((long)lhs<<32)-rhs.value); }
  public static Fixed64 operator*(int lhs, Fixed64 rhs) { return new Fixed64(lhs*rhs.value); }
  public static Fixed64 operator/(int lhs, Fixed64 rhs) { return new Fixed64(lhs) / rhs; }

  public static Fixed64 operator+(double lhs, Fixed64 rhs) { return new Fixed64(FromDouble(lhs)+rhs.value); }
  public static Fixed64 operator-(double lhs, Fixed64 rhs) { return new Fixed64(FromDouble(lhs)-rhs.value); }
  public static Fixed64 operator*(double lhs, Fixed64 rhs) { return new Fixed64(lhs) * rhs; }
  public static Fixed64 operator/(double lhs, Fixed64 rhs) { return new Fixed64(lhs) / rhs; }

  public static Fixed64 operator+(Fixed64 lhs, Fixed64 rhs) { return new Fixed64(lhs.value+rhs.value); }
  public static Fixed64 operator-(Fixed64 lhs, Fixed64 rhs) { return new Fixed64(lhs.value-rhs.value); }

  public static Fixed64 operator*(Fixed64 lhs, Fixed64 rhs)
  { long a=lhs.value>>32, b=(uint)lhs.value, c=rhs.value>>32, d=(uint)rhs.value;
    return new Fixed64(((a*c)<<32) + b*c + a*d + ((b*d)>>32));
  }

  public static Fixed64 operator/(Fixed64 lhs, Fixed64 rhs)
  { long quot, rem;
    uint fp = (uint)rhs.value;
    int  count;
    if(fp==0) { return new Fixed64(lhs.value / rhs.ToInt()); }

    byte neg=0;
    if(lhs.value<0) { lhs.value=-lhs.value; neg=(byte)~neg; }
    if(rhs.value<0) { rhs.value=-rhs.value; neg=(byte)~neg; }

    count=0; // reduce if we can
    { uint op = (uint)lhs.value, mask=1;
      if((fp&mask)==0 && (op&mask)==0)
      { do { mask<<=1; count++; } while((fp&mask)==0 && (op&mask)==0);
        rhs.value>>=count; lhs.value>>=count;
      }
    }

    if(rhs.value<0x100000000)
    { quot  = Math.DivRem(lhs.value, rhs.value, out rem)<<32;
      quot += Math.DivRem(rem<<32, rhs.value, out rem);
    }
    else if(rhs.value<0x1000000000000)
    { Math.DivRem(lhs.value>>32, rhs.value, out rem);
      quot  = Math.DivRem((rem<<32)+(uint)lhs.value, rhs.value, out rem)<<32;
      quot += Math.DivRem(rem<<16, rhs.value, out rem)<<16;
      quot += Math.DivRem(rem<<16, rhs.value, out rem);
    }
    else // fall back on long division
    { // TODO: optimize for divisor>=dividend
      Union ls = new Union(lhs.value<<count), t = new Union();
      int  bits = 96-count;
      byte bit;

      rem = quot = 0;
      do
      { rem = (rem<<1) | (byte)((ls.Uint&0x80000000)>>31);
        lhs.value = ls.Long;
        ls.Long <<= 1;
        bits--;
      }
      while(rem<rhs.value);

      ls.Long = lhs.value;
      rem >>= 1;
      bits++;

      do
      { rem = (rem<<1) | (byte)((ls.Uint&0x80000000)>>31);
        t.Long = rem - rhs.value;
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
  public static bool operator<(Fixed64 lhs, Fixed64 rhs) { return lhs.value<rhs.value; }
  public static bool operator<=(Fixed64 lhs, Fixed64 rhs) { return lhs.value<=rhs.value; }
  public static bool operator>(Fixed64 lhs, Fixed64 rhs) { return lhs.value>rhs.value; }
  public static bool operator>=(Fixed64 lhs, Fixed64 rhs) { return lhs.value>=rhs.value; }
  public static bool operator==(Fixed64 lhs, Fixed64 rhs) { return lhs.value==rhs.value; }
  public static bool operator!=(Fixed64 lhs, Fixed64 rhs) { return lhs.value!=rhs.value; }

  public static bool operator<(Fixed64 lhs, int rhs) { return lhs.value<((long)rhs<<32); }
  public static bool operator<=(Fixed64 lhs, int rhs) { return lhs.value<=((long)rhs<<32); }
  public static bool operator>(Fixed64 lhs, int rhs) { return lhs.value>((long)rhs<<32); }
  public static bool operator>=(Fixed64 lhs, int rhs) { return lhs.value>=((long)rhs<<32); }
  public static bool operator==(Fixed64 lhs, int rhs) { return lhs.value==((long)rhs<<32); }
  public static bool operator!=(Fixed64 lhs, int rhs) { return lhs.value!=((long)rhs<<32); }

  public static bool operator<(Fixed64 lhs, double rhs) { return lhs.ToDouble()<rhs; }
  public static bool operator<=(Fixed64 lhs, double rhs) { return lhs.ToDouble()<=rhs; }
  public static bool operator>(Fixed64 lhs, double rhs) { return lhs.ToDouble()>rhs; }
  public static bool operator>=(Fixed64 lhs, double rhs) { return lhs.ToDouble()>=rhs; }
  public static bool operator==(Fixed64 lhs, double rhs) { return lhs.ToDouble()==rhs; }
  public static bool operator!=(Fixed64 lhs, double rhs) { return lhs.ToDouble()!=rhs; }

  public static bool operator<(int lhs, Fixed64 rhs) { return ((long)lhs<<32)<rhs.value; }
  public static bool operator<=(int lhs, Fixed64 rhs) { return ((long)lhs<<32)<=rhs.value; }
  public static bool operator>(int lhs, Fixed64 rhs) { return ((long)lhs<<32)>rhs.value; }
  public static bool operator>=(int lhs, Fixed64 rhs) { return ((long)lhs<<32)>=rhs.value; }
  public static bool operator==(int lhs, Fixed64 rhs) { return ((long)lhs<<32)==rhs.value; }
  public static bool operator!=(int lhs, Fixed64 rhs) { return ((long)lhs<<32)!=rhs.value; }

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
  /// <summary>PI*2, approximately 6.283185.</summary>
  public static readonly Fixed64 TwoPI = new Fixed64(25769803776L + 1216271633);
  /// <summary>Zero.</summary>
  public static readonly Fixed64 Zero = new Fixed64((long)0);
  #endregion

  const long Trunc  = unchecked((long)0xFFFFFFFF00000000);
  const long OneVal = 0x100000000L;

  static long FromDouble(double value)
  { if(value<-2147483648 || value>=2147483648) throw new OverflowException();
    int whole = (int)value;
    long fp = (long)(Math.IEEERemainder(value, 1)*4294967296.0) & ~Trunc;
    if(whole<0 && fp!=0) fp ^= Trunc;
    return ((long)whole<<32) + fp;
  }

  #region IComparable Members
  /// <include file="../documentation.xml" path="//IComparable/CompareTo/*"/>
  public int CompareTo(object obj)
  { if(obj==null) return 1;
    if(obj is Fixed64)
    { long ov = ((Fixed64)obj).value;
      return value<ov ? -1 : value>ov ? 1 : 0;
    }
    throw new ArgumentException("'obj' is not a Fixed64");
  }
  #endregion

  #region IConvertible Members
  // FIXME: these should probably do rounding
  /// <include file="../documentation.xml" path="//IConvertible/ToUInt64/*"/>
  [CLSCompliant(false)]
  public ulong ToUInt64(IFormatProvider provider)
  { int n = ToInt();
    if(n<0) throw new OverflowException();
    return (ulong)n;
  }
  /// <include file="../documentation.xml" path="//IConvertible/ToSByte/*"/>
  [CLSCompliant(false)]
  public sbyte ToSByte(IFormatProvider provider)
  { int n = ToInt();
    if(n<sbyte.MinValue || n>sbyte.MaxValue) throw new OverflowException();
    return (sbyte)n;
  }
  /// <include file="../documentation.xml" path="//IConvertible/ToDouble/*"/>
  public double ToDouble(IFormatProvider provider) { return ToDouble(); }
  /// <include file="../documentation.xml" path="//Mathematics/Fixed/ToDateTime/*"/>
  public DateTime ToDateTime(IFormatProvider provider) { throw new InvalidCastException(); }
  /// <include file="../documentation.xml" path="//IConvertible/ToSingle/*"/>
  public float ToSingle(IFormatProvider provider)
  { double d = ToDouble();
    if(d<float.MinValue || d>float.MaxValue) throw new OverflowException();
    return (float)d;
  }
  /// <include file="../documentation.xml" path="//Mathematics/Fixed/ToBoolean/*"/>
  public bool ToBoolean(IFormatProvider provider) { return value==0; }
  /// <include file="../documentation.xml" path="//IConvertible/ToInt32/*"/>
  public int ToInt32(IFormatProvider provider) { return ToInt(); }
  /// <include file="../documentation.xml" path="//IConvertible/ToUInt16/*"/>
  [CLSCompliant(false)]
  public ushort ToUInt16(IFormatProvider provider)
  { int n = ToInt();
    if(n<ushort.MinValue || n>ushort.MaxValue) throw new OverflowException();
    return (ushort)n;
  }
  /// <include file="../documentation.xml" path="//IConvertible/ToInt16/*"/>
  public short ToInt16(IFormatProvider provider)
  { int n = ToInt();
    if(n<short.MinValue || n>short.MaxValue) throw new OverflowException();
    return (short)n;
  }
  /// <include file="../documentation.xml" path="//IConvertible/ToString/*"/>
  public string ToString(IFormatProvider provider) { return ToString(null, provider); }
  /// <include file="../documentation.xml" path="//IConvertible/ToByte/*"/>
  public byte ToByte(IFormatProvider provider)
  { int n = ToInt();
    if(n<byte.MinValue || n>byte.MaxValue) throw new OverflowException();
    return (byte)n;
  }
  /// <include file="../documentation.xml" path="//IConvertible/ToChar/*"/>
  public char ToChar(IFormatProvider provider)
  { int n = ToInt();
    if(n<ushort.MinValue || n>ushort.MaxValue) throw new OverflowException();
    return (char)n;
  }
  /// <include file="../documentation.xml" path="//IConvertible/ToInt64/*"/>
  public long ToInt64(IFormatProvider provider) { return ToInt(); }
  /// <summary>Returns the <see cref="TypeCode"/> for the <see cref="Fixed64"/> type.</summary>
  /// <returns>Returns <see cref="TypeCode.Object"/>.</returns>
  public System.TypeCode GetTypeCode() { return System.TypeCode.Object; }
  /// <include file="../documentation.xml" path="//IConvertible/ToDecimal/*"/>
  public decimal ToDecimal(IFormatProvider provider) { return new decimal(ToDouble()); }
  /// <include file="../documentation.xml" path="//IConvertible/ToType/*"/>
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
  /// <include file="../documentation.xml" path="//IConvertible/ToUInt32/*"/>
  [CLSCompliant(false)]
  public uint ToUInt32(IFormatProvider provider)
  { int n = ToInt();
    if(n<0) throw new OverflowException();
    return (uint)n;
  }
  #endregion

  #region IFormattable Members
  /// <include file="../documentation.xml" path="//Mathematics/Fixed/ToString2/*"/>
  public string ToString(string format, IFormatProvider provider)
  { if(format==null) return ToDouble().ToString();
    if(format.Length==0 || char.ToUpper(format[0])!='R') return ToDouble().ToString(format);
    return wholePart.ToString() + '/' + ((uint)value).ToString();
  }
  #endregion

  #region Union
  [StructLayout(LayoutKind.Explicit)]
  struct Union
  { public Union(long val) { Uint=0; Long=val; }
    #if BIGENDIAN
    [FieldOffset(0)] public long Long;
    [FieldOffset(0)] public uint Uint;
    #else
    [FieldOffset(0)] public long Long;
    [FieldOffset(4)] public uint Uint;
    #endif
  }
  #endregion

  #if BIGENDIAN
  [FieldOffset(0)] internal long val;
  [FieldOffset(0), NonSerialized] int wholePart;
  #else
  [FieldOffset(0)] internal long value;
  [FieldOffset(4), NonSerialized] int wholePart;
  #endif
}
#endregion

#region 2D math
namespace TwoD
{

#region Math2D
/// <summary>This class contains general two-dimensional math functions, such as tests for containment and
/// intersection of various primitives, finding intersection points and areas, and rotating points.
/// </summary>
public static class Math2D
{
  /// <include file="../documentation.xml" path="//Mathematics/GLMath/AngleBetween/*"/>
  public static double AngleBetween(Point start, Point end) { return (end-start).Angle; }
  /// <include file="../documentation.xml" path="//Mathematics/GLMath/AngleBetween/*"/>
  public static double AngleBetween(System.Drawing.Point start, System.Drawing.Point end)
  {
    return (new Point(end)-new Point(start)).Angle;
  }

  #region Contains
  /// <summary>Determines if one circle fully contains another.</summary>
  public static bool Contains(ref Circle outer, ref Circle inner)
  {
    // a circle A contains a circle B if a new circle centered around the A's center, but with a radius of
    // A.Radius - B.Radius contains the B's center point.
    double xd=outer.Center.X-inner.Center.X, yd=outer.Center.Y-inner.Center.Y, radius=outer.Radius-inner.Radius;
    return xd*xd + yd*yd <= radius*radius;
  }

  /// <summary>Determines if a circle fully contains a given line segment.</summary>
  public static bool Contains(ref Circle circle, ref Line segment)
  {
    // a circle contains a line segment if it contains both endpoints
    if(Contains(ref circle, ref segment.Start))
    {
      Point endPoint = segment.End;
      return Contains(ref circle, ref endPoint);
    }
    else
    {
      return false;
    }
  }

  /// <summary>Determines if a circle contains a given point.</summary>
  public static bool Contains(ref Circle circle, ref Point point)
  {
    // a circle contains a point if the distance from the point to the circle's center is less than or equal to
    // the radius. we can square both sides of the comparison to eliminate the Math.Sqrt() operation on the left
    double xd=point.X-circle.Center.X, yd=point.Y-circle.Center.Y;
    return xd*xd + yd*yd <= circle.RadiusSqr;
  }

  /// <summary>Determines if a circle fully contains a given rectangle.</summary>
  public static bool Contains(ref Circle circle, ref Rectangle rect)
  {
    // a circle contains a rectangle if it contains all four corners
    Point point = rect.TopLeft;
    if(!Contains(ref circle, ref point)) return false;
    point.X += rect.Width; // top-right
    if(!Contains(ref circle, ref point)) return false;
    point.Y += rect.Height; // bottom-right
    if(!Contains(ref circle, ref point)) return false;
    point.X = rect.X; // bottom-left
    return Contains(ref circle, ref point);
  }

  /// <summary>Determines if a circle fully contains a given possibly-concave polygon.</summary>
  public static bool Contains(ref Circle circle, Polygon poly)
  {
    // a circle contains a polygon if it contains all the polygon's vertices
    for(int i=poly.Length-1; i>=0; i--)
    {
      Point point = poly[i];
      if(!Contains(ref circle, ref point)) return false;
    }
    return true;
  }
  
  /// <summary>Determines if a rectangle fully contains a given circle.</summary>
  public static bool Contains(ref Rectangle rect, ref Circle circle)
  {
    // a rectangle contains a circle if the rectangle with its width and height both shrunk by the circle's diameter
    // contains the circle's center
    return rect.X+circle.Radius <= circle.Center.X && rect.Y+circle.Radius <= circle.Center.Y &&
           rect.Right-circle.Radius >= circle.Center.X && rect.Bottom-circle.Radius >= circle.Center.Y;
  }

  /// <summary>Determines if a rectangle fully contains a given line segment.</summary>
  public static bool Contains(ref Rectangle rect, ref Line segment)
  {
    // a rectangle contains a line segment if it contains both endpoints
    if(Contains(ref rect, ref segment.Start))
    {
      Point end = segment.End;
      return Contains(ref rect, ref end);
    }
    else
    {
      return false;
    }
  }

  /// <summary>Determines if a rectangle contains a given point.</summary>
  public static bool Contains(ref Rectangle rect, ref Point point)
  {
    return point.X>=rect.X && point.Y>=rect.Y && point.X<=rect.Right && point.Y<=rect.Bottom;
  }

  /// <summary>Determines if one rectangle fully contains another.</summary>
  public static bool Contains(ref Rectangle outer, ref Rectangle inner)
  {
    // a rectangle contains another if it contains both corners of the other.
    Point point = inner.TopLeft;
    if(!Contains(ref outer, ref point)) return false;
    point = inner.BottomRight;
    return Contains(ref outer, ref point);
  }

  /// <summary>Determines if a rectangle fully contains a given possibly-concave polygon.</summary>
  public static bool Contains(ref Rectangle rect, Polygon poly)
  {
    // a rectangle contains a polygon if it contains all the polygon's vertices
    for(int i=poly.Length-1; i>=0; i--)
    {
      Point point = poly[i];
      if(!Contains(ref rect, ref point)) return false;
    }
    return true;
  }
  
  /// <summary>Determines if a convex polygon fully contains a given circle.</summary>
  public static bool Contains(Polygon convexPoly, ref Circle circle)
  {
    // a convex polygon contains a circle if it contains the center point and the distance from the circle's center to
    // each bounding line is greater than or equal to the circle's radius.

    if(convexPoly.Length < 3) return false; // degenerate polygons don't contain anything, we'll say.
    
    if(!Contains(convexPoly, ref circle.Center)) return false;

    for(int i=convexPoly.Length; i>=0; i--)
    {
      Line edge = convexPoly.GetEdge(i);
      // see Intersects(Circle, Line) for the explanation of the following
      double scaledDist = edge.Vector.CrossVector.DotProduct(circle.Center - edge.Start);
      if(circle.RadiusSqr*edge.LengthSqr < scaledDist*scaledDist) return false;
    }
    
    return true;
  }

  /// <summary>Determines if a convex polygon fully contains a given line segment.</summary>
  public static bool Contains(Polygon convexPoly, ref Line line)
  {
    // a convex polygon contains a line segment if it contains both endpoints
    if(convexPoly.Length < 3) return false; // degenerate polygons don't contain anything, we'll say.
    if(Contains(convexPoly, ref line.Start))
    {
      Point end = line.End;
      return Contains(convexPoly, ref end);
    }
    else
    {
      return false;
    }
  }

  /// <summary>Determines if a convex polygon contains a given point.</summary>
  /// <remarks>The polygon can be defined clockwise or counter-clockwise, but must be a convex polygon. If the polygon
  /// is not convex, the results of this method are undefined.
  /// </remarks>
  public static bool Contains(Polygon convexPoly, ref Point point)
  {
    // a convex polygon contains a point if the point is on the "inside" of every bounding line. see Line.WhichSide()
    // for the meaning of "inside". we don't really know which side is the inside, because the polygon could be
    // clockwise or counter-clockwise, but if the point is contained, it will consistently be on the same side of each
    // line. if it's not contained, it will not be on the same side of each line.

    if(convexPoly.Length < 3) return false; // degenerate polygons don't contain anything, we'll say.

    bool pos=false, neg=false; // these variables track which sides of the lines we've seen the point on.
    for(int i=convexPoly.Length-1; i>=0; i--)
    {
      double side = convexPoly.GetEdge(i).WhichSide(point);
      if(side<0) // it was on the negative side.
      {
        if(pos) return false; // if it was previously on the positive side, then it's not contained.
        neg = true; // mark it as having been on the negative side
      }
      else if(side>0) // it was on the positive side.
      {
        if(neg) return false; // if it was previously on the negative side, then it's not contained.
        pos = true; // mark it as having been on the negative side.
      }
      // otherwise, it was colinear with one of the edges. we'll count that as inside.
    }

    return true; // there was no disparity in the side of the lines that the point was on, so it's contained.
  }

  /// <summary>Determines if a convex polygon fully contains a given rectangle.</summary>
  public static bool Contains(Polygon convexPoly, ref Rectangle rect)
  {
    // a convex polygon contains a rectangle if it contains each corner
    if(convexPoly.Length < 3) return false; // degenerate polygons don't contain anything, we'll say.

    Point point = rect.TopLeft;
    if(!Contains(convexPoly, ref point)) return false;
    point.X += rect.Width; // top-right
    if(!Contains(convexPoly, ref point)) return false;
    point.Y += rect.Height; // bottom-right
    if(!Contains(convexPoly, ref point)) return false;
    point.X = rect.X; // bottom-left
    return Contains(convexPoly, ref point);
  }
  
  /// <summary>Determines if a convex polygon fully contains a possibly-concave polygon.</summary>
  public static bool Contains(Polygon convexPoly, Polygon poly)
  {
    // a convex polygon contains another polygon if it contains all the polygon's vertices.
    if(convexPoly.Length < 3) return false; // degenerate polygons don't contain anything, we'll say.

    for(int i=poly.Length-1; i>=0; i--)
    {
      Point point = poly[i];
      if(!Contains(convexPoly, ref point)) return false;
    }
    return true;
  }
  #endregion

  #region Intersects
  /// <summary>Determines if two circles intersect.</summary>
  public static bool Intersects(ref Circle a, ref Circle b)
  {
    // two circles intersect if the distance between their centers is less than or equal to the sum of their radii
    double radius = a.Radius+b.Radius, xd = a.Center.X-b.Center.X, yd = a.Center.Y-b.Center.Y;
    return xd*xd+yd*yd <= radius*radius;
  }
  
  /// <summary>Determines if a circle intersects a line (not a line segment).</summary>
  public static bool Intersects(ref Circle circle, ref Line line)
  {
    // a circle intersects a line if the distance from the center to the line is less than or equal to the radius
    double scaledDist = line.Vector.CrossVector.DotProduct(circle.Center - line.Start);
    // 'scaledDist' is the distance, scaled by the length of the line segment. normally, we'd check for:
    // circle.Radius <= scaledDist / line.Length (1 mul, 1 add, 1 div, 1 sqrt). but mul both sides by line.Length:
    // circle.Radius * line.Length <= scaledDist (2 mul, 1 add, 1 sqrt). and then square both sides:
    // circle.RadiusSqr * line.LengthSqr <= scaledDist * scaledDist (4 mul, 1 add)
    return circle.RadiusSqr * line.LengthSqr <= scaledDist * scaledDist;
  }
  
  /// <summary>Determines if a circle intersects a line segment.</summary>
  public static bool SegmentIntersects(ref Circle circle, ref Line segment)
  {
    // a circle intersects a line segment if the circle contains the point on the segment closest to its center
    return circle.Contains(segment.ClosestPointOnSegment(circle.Center));
  }
  
  /// <summary>Determines if a circle intersects a rectangle.</summary>
  public static bool Intersects(ref Circle circle, ref Rectangle rect)
  {
    // a circle intersects a rectangle if it contains the point within the rectangle nearest the circle's center.
    // if the rectangle is fully to the right of the circle's center, then the X distance from the center to the
    // nearest point will simply be the distance between the rectangle's left edge and the circle's center. if the
    // rectangle is fully on the left, the X distance to the nearest point is the distance between the circle's
    // center and the rectangle's right edge. if neither of those are true, then the rectangle "straddles" the
    // circle's center horizontally. in that case, the X distance to the nearest rectangle point is zero, because the
    // circle definitely intersects the rectangle, at least horizontally. repeating the previous two checks
    // vertically (top/bottom instead of left/right) yields the Y distance to the nearest point within the rectangle.
    // then, we can use the simple check: Xdist^2 + Ydist^2 <= Radius^2
    
    double squaredDist, dist;

    if(circle.Center.X < rect.X) // if the rectangle is fully on the right side of the circle center
    {
      dist = rect.X - circle.Center.X; // the X distance to the rectangle's left edge
      squaredDist = dist * dist;       // accumulate the squared X distance
    }
    else
    {
      dist = rect.Right - circle.Center.X; // X distance to the rectangle's right edge
      if(dist < 0) // if the rectangle is fully on the left side of the circle center
      {
        squaredDist = dist * dist; // accumulate the squared X distance
      }
      else
      {
        squaredDist = 0; // the rectangle intersects the circle horizontally
      }
    }

    if(circle.Center.Y < rect.Y) // if the rectangle is fully below the circle's center
    {
      dist = rect.Y - circle.Center.Y; // the Y distance to the rectangle's top
      squaredDist += dist * dist;      // accumulate the squared Y distance
    }
    else
    {
      dist = rect.Bottom - circle.Center.Y; // Y distance to the rectangle's bottom
      if(dist < 0) // if the rectangle is fully above the circle's center
      {
        squaredDist += dist * dist; // accumulate the squared Y distance
      }
    }

    return squaredDist <= circle.RadiusSqr; // Xdist^2 + Ydist^2 <= Radius^2
  }
  
  /// <summary>Determines if a circle intersects a (possibly convex) polygon.</summary>
  public static bool Intersects(ref Circle circle, Polygon poly)
  {
    // a circle intersects a polygon if it intersects any of the polygon's edges or the circle's center point is within
    // the polygon.

    if(Contains(poly, ref circle.Center)) return true;

    for(int i=poly.Length-1; i>=0; i--)
    {
      Line edge = poly.GetEdge(i);
      if(SegmentIntersects(ref circle, ref edge)) return true;
    }
    return false;
  }

  /// <summary>Determines if two lines (not segments) intersect.</summary>
  /// <remarks>This method can be used to determine if two lines are parallel. If the method returns false, they are
  /// parallel.
  /// </remarks>
  public static bool Intersects(ref Line a, ref Line b)
  {
    // two lines intersect if they are not parallel. they are not parallel if they have different slopes.
    // using the standard slope formula ("rise over run"), we get vector.Y / vector.X. but this runs into
    // problems with divide-by-zero and performance, so we restructure the equation to use multiplication:
    // aY/aX != bY/bX   multiply both sides by aX and bY:
    // aY*bX != bY*aX
    return a.Vector.Y*b.Vector.X != b.Vector.Y*a.Vector.X;
  }

  /// <summary>Determines if two line segments intersect.</summary>
  public static bool SegmentIntersects(ref Line a, ref Line b)
  {
    // see SegmentIntersection(Line, Line) for the explanation.
    double denominator = b.Vector.Y*a.Vector.X - b.Vector.X*a.Vector.Y; // calculate the denominator
    if(denominator == 0) return false; // return false for parallel lines
    double xd = a.Start.X-b.Start.X, yd = a.Start.Y-b.Start.Y; // these parts are the same in both equations
    double Nv = (b.Vector.X*yd - b.Vector.Y*xd) / denominator; // calculate Na
    if(Nv<0 || Nv>1) return false; // if Na lies outside 0-1, the intersection point is not on the 'a' segment
    Nv = (a.Vector.X*yd - a.Vector.Y*xd) / denominator;        // calculate Nb
    return Nv>=0 && Nv<=1; // if Nb lies outside 0-1, the intersection point is not on the 'b' segment either
  }
  
  /// <summary>Returns information about the intersection of two lines, either of which can be infinite or a segment.</summary>
  /// <returns>Returns a <see cref="LineIntersection"/> structure containing information about the intersection.</returns>
  public static LineIntersection IntersectionInfo(ref Line a, ref Line b)
  {
    // see SegmentIntersection(Line, Line) for the explanation.
    double denominator = b.Vector.Y*a.Vector.X - b.Vector.X*a.Vector.Y; // calculate the denominator
    if(denominator == 0) return new LineIntersection(Point.Invalid, false, false); // return invalid for parallel lines

    double xd = a.Start.X-b.Start.X, yd = a.Start.Y-b.Start.Y; // these parts are the same in both equations
    double Na = (b.Vector.X*yd - b.Vector.Y*xd) / denominator; // calculate Na
    double Nb = (a.Vector.X*yd - a.Vector.Y*xd) / denominator; // calculate Nb
    return new LineIntersection(new Point(a.Start.X + a.Vector.X*Na, a.Start.Y + a.Vector.Y*Na),
                                Na>=0 && Na<=1, Nb>=0 && Nb<=1);
  }

  /// <summary>Determines if a line (not a segment) intersects a rectangle.</summary>
  public static bool Intersects(ref Line line, ref Rectangle rect)
  {
    // a line intersects a rectangle if it intersects any side.
    for(int i=0; i<4; i++)
    {
      Line edge = rect.GetEdge(i);
      if(IntersectionInfo(ref line, ref edge).OnSecond) return true;
    }
    return false;
  }
  
  /// <summary>Determines if a line segment intersects a rectangle.</summary>
  public static bool SegmentIntersects(ref Line segment, ref Rectangle rect)
  {
    return SegmentIntersection(ref segment, ref rect).Valid;
  }
  
  /// <summary>Determines if a line (not a segment) intersects a possibly-concave polygon.</summary>
  public static bool Intersects(ref Line line, Polygon poly)
  {
    // a line intersects a polygon if it intersects any of the polygon's edges
    for(int i=poly.Length-1; i>=0; i--)
    {
      Line edge = poly.GetEdge(i);
      if(IntersectionInfo(ref line, ref edge).OnSecond) return true;
    }
    return false;
  }
  
  /// <summary>Determines if a line segment intersects a convex polygon.</summary>
  public static bool SegmentIntersects(ref Line segment, Polygon convexPoly)
  {
    // a line segment intersects a polygon if the polygon contains both endpoints, or the segment intersects any edge.

    // we only need to check one point to test for intersection
    if(Contains(convexPoly, ref segment.Start)) return true;

    for(int i=convexPoly.Length-1; i>=0; i--) // the polygon doesn't fully contain the segment, so test the edges
    {
      Line edge = convexPoly.GetEdge(i);
      if(SegmentIntersects(ref segment, ref edge)) return true;
    }
    return false;
  }
  
  /// <summary>Determines if two rectangles intersect.</summary>
  public static bool Intersects(ref Rectangle a, ref Rectangle b)
  {
    // two rectangles A and B intersect if A's left edge is not to B's right, A's top is not below above B's bottom,
    // A's right is not to B's left, and A's bottom is not above B's top.
    return a.X <= b.Right && a.Y <= b.Bottom && a.Right >= b.X && a.Bottom >= b.Y;
  }

  /// <summary>Determines if a rectangle intersects a convex polygon.</summary>
  public static bool Intersects(ref Rectangle rect, Polygon convexPoly)
  {
    // a rectangle intersects a convex polygon if all corners of either object are contained within the other, or any
    // edge of either object intersect any edge of the other object.

    if(convexPoly.Length == 0) return false;

    // if one point of either is inside the other, there's intersection.
    Point point = convexPoly[0];
    if(Contains(ref rect, ref point)) return true;
    point = rect.TopLeft;
    if(Contains(convexPoly, ref point)) return true;

    for(int i=0; i<4; i++) // if it's not fully contained, then at least one edge must intersect
    {
      Line edge = rect.GetEdge(i);
      if(SegmentIntersects(ref edge, convexPoly)) return true;
    }
    
    return false;
  }

  /// <summary>Determines if a convex polygon intersects another convex polygon.</summary>
  public static bool Intersects(Polygon convexA, Polygon convexB)
  {
    // two convex polygons A and B intersect if B contains all of A's vertices, or any of A's edges intersects B

    // we only need to check one vertex to determine whether A is fully contained
    Point point = convexA[0];
    if(Contains(convexB, ref point)) return true;

    for(int i=convexA.Length-1; i>=0; i--) // if it's not fully contained, then at least one edge must intersect
    {
      Line edge = convexA.GetEdge(i);
      if(SegmentIntersects(ref edge, convexB)) return true;
    }
    
    return false;
  }
  #endregion

  #region Intersection
  /// <summary>Determines if and where two lines (not segments) intersect.</summary>
  /// <remarks>If no intersection occurs, <see cref="Point.Invalid"/> will be returned.</remarks>
  public static Point Intersection(ref Line a, ref Line b)
  {
    // see SegmentIntersection(Line, Line) for the explanation
    double denominator = b.Vector.Y*a.Vector.X - b.Vector.X*a.Vector.Y; // calculate the denominator
    if(denominator == 0) return Point.Invalid; // return false for parallel lines
    double Na = (b.Vector.X*(a.Start.Y-b.Start.Y) - b.Vector.Y*(a.Start.X-b.Start.X)) / denominator; // calculate Na
    return new Point(a.Start.X + a.Vector.X*Na, a.Start.Y + a.Vector.Y*Na);
  }

  /// <summary>Determines if and where two line segments intersect.</summary>
  /// <remarks>If no intersection occurs, <see cref="Point.Invalid"/> will be returned.</remarks>
  public static Point SegmentIntersection(ref Line a, ref Line b)
  {
    // line segments are represented as starting points and vectors. any point Pn on a line segment can be
    // represented by Start + Vector*N where N is from 0 to 1. if we solve for Pa (a point on segment a) == Pb (a
    // point on segment b), we get the following two formulas:
    // a.Start.X + a.Vector.X * Na == b.Start.X + b.Vector.X * Nb and
    // a.Start.Y + a.Vector.Y * Na == b.Start.Y + b.Vector.Y * Nb
    // solving for Na and Nb produces:
    // Na = (b.Vector.X*(a.Start.Y-b.Start.Y) - b.Vector.Y*(a.Start.X-b.Start.X)) /
    //          (b.Vector.Y*a.Vector.X - b.Vector.X*a.Vector.Y)
    // Nb = (a.Vector.X*(a.Start.Y-b.Start.Y) - a.Vector.Y*(a.Start.X-b.Start.X)) /
    //          (b.Vector.Y*a.Vector.X - b.Vector.X*a.Vector.Y)
    // note that they have the same denominator and the same a.Start.X-b.Start.X and a.Start.Y-b.Start.Y parts.
    // if the denominator is zero, the lines are parallel. (See the line intersection method for an explanation of why)

    double denominator = b.Vector.Y*a.Vector.X - b.Vector.X*a.Vector.Y; // calculate the denominator
    if(denominator == 0) return Point.Invalid; // return false for parallel lines
    double xd = a.Start.X-b.Start.X, yd = a.Start.Y-b.Start.Y; // these parts are the same in both equations
    double Nv = (a.Vector.X*yd - a.Vector.Y*xd) / denominator; // calculate Nb
    if(Nv<0 || Nv>1) return Point.Invalid; // if Nb lies outside 0-1, the intersection point is not on the 'b' segment
    Nv = (b.Vector.X*yd - b.Vector.Y*xd) / denominator;        // calculate Na
    if(Nv<0 || Nv>1) return Point.Invalid; // if Na lies outside 0-1, the intersection point is not on the 'a' segment
    // the intersection point can be calculated from either segment if we have the appropriate value of N.
    // since we have Na sitting in 'Nv', we'll use the 'a' vector.
    return new Point(a.Start.X + a.Vector.X*Nv, a.Start.Y + a.Vector.Y*Nv);
  }

  /// <summary>Determines if and where a line (not segment) and a rectangle intersect.</summary>
  /// <remarks>If no intersection occurs, <see cref="Line.Invalid"/> will be returned. If the line intersects only
  /// tangentially, a degenerate (zero-length) line will be returned.
  /// </remarks>
  public static unsafe Line Intersection(ref Line line, ref Rectangle rect)
  {
    // a line intersects a rectangle if it intersects any of the edges.
    Point* points  = stackalloc Point[2]; // allocate 2 points on the stack
    int pointIndex = 0;
    
    for(int i=0; i<4 && pointIndex<2; i++)
    {
      Line edge = rect.GetEdge(0);
      LineIntersection info = IntersectionInfo(ref line, ref edge);
      if(info.OnSecond) points[pointIndex++] = info.Point; // if it intersected the edge, add the point to the list
    }
    
    if(pointIndex == 0) // if no edges intersected, return an invalid line.
    {
      return Line.Invalid;
    }
    else if(pointIndex == 2) // if two points intersected, return the segment between them
    {
      return new Line(points[0], points[1]);
    }
    else // if one point intersected, return a degenerate (zero-length) line segment.
    {
      return new Line(points[0].X, points[0].Y, 0, 0);
    }
  }

  /// <summary>Determines if and where a line (not segment) and a convex polygon intersect.</summary>
  /// <remarks>If no intersection occurs, <see cref="Line.Invalid"/> will be returned. If the line intersects only
  /// tangentially, a degenerate (zero-length) line will be returned.
  /// </remarks>
  public static unsafe Line Intersection(ref Line line, Polygon convexPoly)
  {
    // a line intersects a convex polygon if it intersects any of the edges.
    Point* points  = stackalloc Point[2];
    int pointIndex = 0;

    // test the intersection of the line with each edge, or until we find two intersection points
    for(int i=convexPoly.Length; i>=0 && pointIndex<2; i--)
    {
      Line edge = convexPoly.GetEdge(i);
      LineIntersection info = IntersectionInfo(ref line, ref edge);
      if(info.OnSecond) points[pointIndex++] = info.Point; // if the intersection was on the polygon edge
    }

    if(pointIndex == 0) // if no edges intersected, return an invalid line.
    {
      return Line.Invalid;
    }
    else if(pointIndex == 2) // if two points intersected, return the segment between them
    {
      return new Line(points[0], points[1]);
    }
    else // if one point intersected, return a degenerate (zero-length) line segment.
    {
      return new Line(points[0].X, points[0].Y, 0, 0);
    }
  }

  /// <summary>Determines if and where a line segment and a rectangle intersect.</summary>
  /// <remarks>If no intersection occurs, <see cref="Line.Invalid"/> will be returned. If the line intersects only
  /// tangentially, a degenerate (zero-length) line will be returned.
  /// </remarks>
  public static Line SegmentIntersection(ref Line segment, ref Rectangle rect)
  { 
    // this is the Cohen-Sutherland algorithm line-clipping algorithm

    double rectRight=rect.Right, rectBottom=rect.Bottom;
    Point start=segment.Start, end=segment.End; // copy the segment points so as not to modify the input value
    int startCode, endCode;

    // 'startCode' is a bitfield:
    // bit 0: segment starts above the rectangle
    // bit 1: segment starts below the rectangle
    // bit 2: segment starts to the left of the rectangle
    // bit 3: segment starts to the right of the rectangle
    // if c==0, the start point of the segment is within the rectangle
    startCode = start.Y<rect.Y ? 1 : start.Y>rectBottom ? 2 : 0;
    if(start.X < rect.X) startCode |= 4;
    else if(start.X > rectRight) startCode |= 8;

    // 'endCode' is a bitfield:
    // bit 0: segment ends above the rectangle
    // bit 1: segment ends below the rectangle
    // bit 2: segment ends to the left of the rectangle
    // bit 3: segment ends to the right of the rectangle
    // if c2==0, the end point of the segment is within the rectangle
    endCode = end.Y<rect.Y ? 1 : end.Y>rectBottom ? 2 : 0;
    if(end.X < rect.X) endCode |= 4;
    else if(end.X > rectRight) endCode |= 8;

    while(true)
    {
      // if both points are within the rectangle, return the entire line.
      if(startCode==0 && endCode==0) return new Line(start, end);
      // if the segment can't possibly intersect the rectangle (eg, both the start and end are to the left of it),
      // return invalid.
      if((startCode&endCode) != 0) return Line.Invalid;

      // for each endpoint, we can only clip in one direction each iteration, which is why the clipping statements use
      // "else if" clauses.

      if(startCode != 0) // if the start point is outside the rectangle, clip it to the rectangle.
      { 
        if((startCode&1) != 0) // if it starts above it, interpolate to clip the line against the line colinear with the top
        {
          start.X += (rect.Y-start.Y) * segment.Vector.X / segment.Vector.Y;
          start.Y = rect.Y;
        }
        else if((startCode&2) != 0) // bottom
        {
          start.X -= (start.Y-rectBottom) * segment.Vector.X / segment.Vector.Y;
          start.Y = rectBottom;
        }
        else if((startCode&4) != 0) // left
        {
          start.Y += (rect.X-start.X) * segment.Vector.Y / segment.Vector.X;
          start.X = rect.X;
        }
        else // right
        {
          start.Y -= (start.X-rectRight) * segment.Vector.Y / segment.Vector.X;
          start.X = rectRight;
        }

        // after clipping, check again.
        startCode = start.Y<rect.Y ? 1 : start.Y>rectBottom ? 2 : 0;
        if(start.X < rect.X) startCode |= 4;
        else if(start.X > rectRight) startCode |= 8;
      }

      if(endCode != 0) // if the end point is outside the rectangle
      { 
        if((endCode&1) != 0) // if it starts above it, interpolate to clip the line against the line colinear with the top
        { 
          end.X += (rect.Y-end.Y) * segment.Vector.X / segment.Vector.Y;
          end.Y = rect.Y;
        }
        else if((endCode&2) != 0) // bottom.
        { 
          end.X -= (end.Y-rectBottom) * segment.Vector.X / segment.Vector.Y;
          end.Y = rectBottom;
        }
        else if((endCode&4) != 0) // left
        { 
          end.Y += (rect.X-end.X) * segment.Vector.Y / segment.Vector.X;
          end.X = rect.X;
        }
        else // right
        { 
          end.Y -= (end.X-rectRight) * segment.Vector.Y / segment.Vector.X;
          end.X = rectRight;
        }

        endCode = end.Y<rect.Y ? 1 : end.Y>rectBottom ? 2 : 0;
        if(end.X < rect.X) endCode |= 4;
        else if(end.X > rectRight) endCode |= 8;
      }
    }
  }

  /// <summary>Determines if and where a line segment and a convex polygon intersect.</summary>
  /// <remarks>If no intersection occurs, <see cref="Line.Invalid"/> will be returned. If the line intersects only
  /// tangentially, a degenerate (zero-length) line will be returned.
  /// </remarks>
  public static Line SegmentIntersection(ref Line segment, Polygon convexPoly)
  {
    Point start = segment.Start, end = segment.End; // copy the endpoints so as to not modify the input line
    int sign = convexPoly.IsClockwise() ? 1 : -1; // determine which side of the bounding lines is "outside"

    // for each polygon bounding line, clip the endpoints of the segment that lie "outside" the line to the line.
    for(int i=convexPoly.Length; i>=0; i--)
    {
      Line edge = convexPoly.GetEdge(i);
      bool startOutside = Math.Sign(edge.WhichSide(start)) == sign, // determine whether either of the endpoints are
           endOutside   = Math.Sign(edge.WhichSide(end))   == sign; // "outside" the bounding line.
      if(startOutside)
      { if(endOutside) return Line.Invalid; // if both endpoints are outside the clipping line, there's no intersection
        Line tempSegment = new Line(start, end); // construct a temporary line to hold the start-end segment
        // we can use infinite line intersection because at this point one endpoint is outside and one is inside,
        //so we know the segment intersects the bounding line
        start = Intersection(ref edge, ref tempSegment);
      }
      else if(endOutside)
      {
        Line tempSegment = new Line(start, end); // construct a temporary line to hold the start-end segment
        // we can use infinite line intersection because at this point one endpoint is outside and one is inside,
        // so we know the segment intersects the bounding line
        end = Intersection(ref edge, ref tempSegment);
      }
    }

    return new Line(start, end);
  }

  /// <summary>Determines if and where two rectangles intersect.</summary>
  /// <remarks>If no intersection occurs, a rectangle with a width and height of zero will be returned.</remarks>
  public static Rectangle Intersection(ref Rectangle a, ref Rectangle b)
  {
    // for an explanation of these checks, see Intersects(Rectangle, Rectangle)
    double bRight = b.Right;
    if(a.X > bRight) goto noIntersection;
    double bBottom = b.Bottom;
    if(a.Y > bBottom) goto noIntersection;
    double aRight = a.Right;
    if(aRight < b.X) goto noIntersection;
    double aBottom = a.Bottom;
    if(aBottom < b.Y) goto noIntersection;

    // they intersect, so return a new rectangle with the inward-most of each of the coordinates
    return Rectangle.FromPoints(a.X < b.X ? b.X : a.X, a.Y < b.Y ? b.Y : a.Y,
                                aRight < bRight ? aRight : bRight, aBottom < bBottom ? aBottom : bBottom);
    noIntersection:
    return new Rectangle(); // no intersection, so return an empty rectangle
  }
  #endregion

  #region Rotation
  /// <summary>Rotates a 2D point using precalculated sine and cosine factors.</summary>
  /// <remarks>The sine and cosine factors can be obtained from the <see cref="GLMath.GetRotationFactors"/> function.</remarks>
  public static TwoD.Point Rotate(TwoD.Point point, double sin, double cos)
  {
    return new TwoD.Point(point.X*cos - point.Y*sin, point.X*sin + point.Y*cos);
  }

  /// <summary>Rotates a 2D point in place using precalculated sine and cosine factors.</summary>
  /// <remarks>The sine and cosine factors can be obtained from the <see cref="GLMath.GetRotationFactors"/> function.</remarks>
  [CLSCompliant(false)]
  public static void Rotate(ref TwoD.Point point, double sin, double cos)
  {
    point = new TwoD.Point(point.X*cos - point.Y*sin, point.X*sin + point.Y*cos);
  }

  /// <summary>Rotates an array of points.</summary>
  /// <param name="points">The array of <see cref="TwoD.Point"/> to rotate.</param>
  /// <param name="start">The index at which to start rotating points.</param>
  /// <param name="length">The number of points to rotate.</param>
  /// <param name="angle">The angle by which to rotate the points, in radians.</param>
  public static void Rotate(TwoD.Point[] points, int start, int length, double angle)
  { 
    if(angle == 0) return;
    double sin, cos;
    GLMath.GetRotationFactors(angle, out sin, out cos);
    for(int end=start+length; start<end; start++)
    {
      Rotate(ref points[start], sin, cos);
    }
  }

  /// <summary>Rotates a 2D vector using precalculated sine and cosine factors.</summary>
  /// <remarks>The sine and cosine factors can be obtained from the <see cref="GLMath.GetRotationFactors"/> function.</remarks>
  public static TwoD.Vector Rotate(TwoD.Vector vector, double sin, double cos)
  { return new TwoD.Vector(vector.X*cos - vector.Y*sin, vector.X*sin + vector.Y*cos);
  }

  /// <summary>Rotates a 2D vector in place using precalculated sine and cosine factors.</summary>
  /// <remarks>The sine and cosine factors can be obtained from the <see cref="GLMath.GetRotationFactors"/> function.</remarks>
  [CLSCompliant(false)]
  public static void Rotate(ref TwoD.Vector vector, double sin, double cos)
  { vector = new TwoD.Vector(vector.X*cos - vector.Y*sin, vector.X*sin + vector.Y*cos);
  }

  /// <summary>Rotates an array of vectors.</summary>
  /// <param name="vectors">The array of <see cref="TwoD.Vector"/> to rotate.</param>
  /// <param name="start">The index at which to start rotating vectors.</param>
  /// <param name="length">The number of vectors to rotate.</param>
  /// <param name="angle">The angle by which to rotate the vectors, in radians.</param>
  public static void Rotate(TwoD.Vector[] vectors, int start, int length, double angle)
  { if(angle == 0) return;
    double sin, cos;
    GLMath.GetRotationFactors(angle, out sin, out cos);
    for(int end=start+length; start<end; start++)
      Rotate(ref vectors[start], sin, cos);
  }
  #endregion
}
#endregion

#region Vector
/// <summary>This structure represents a mathematical vector in two-dimensional space.</summary>
[Serializable, StructLayout(LayoutKind.Sequential)]
public struct Vector
{ 
  /// <summary>Initializes this vector from magnitudes along the X and Y axes.</summary>
  /// <param name="x">The magnitude along the X axis.</param>
  /// <param name="y">The magnitude along the Y axis.</param>
  public Vector(double x, double y) { X=x; Y=y; }
  /// <summary>Initializes this vector from a <see cref="Point"/>.</summary>
  /// <param name="pt">A <see cref="Point"/>. The point's X and Y coordinates will become the corresponding
  /// X and Y magnitudes of the vector.
  /// </param>
  public Vector(Point pt) { X=pt.X; Y=pt.Y; }
  /// <summary>Initializes this vector from a <see cref="System.Drawing.Point"/>.</summary>
  /// <param name="pt">A <see cref="System.Drawing.Point"/>. The point's X and Y coordinates will become the
  /// corresponding X and Y magnitudes of the vector.
  /// </param>
  public Vector(System.Drawing.Point pt) { X=pt.X; Y=pt.Y; }
  /// <summary>Initializes this vector from a <see cref="System.Drawing.Size"/>.</summary>
  /// <param name="size">A <see cref="System.Drawing.Size"/>. The size's Width and Height respectively will become
  /// the X and Y magnitudes of the vector.
  /// </param>
  public Vector(System.Drawing.Size size) { X=size.Width; Y=size.Height; }

  /// <summary>Calculates and returns the angle of the vector.</summary>
  /// <value>The angle of the vector, in radians.</value>
  /// <remarks>An angle of zero points directly towards right (towards the positive side of the X axis). Other values
  /// are radian offsets from there.
  /// </remarks>
  public double Angle
  { get
    { double angle = Math.Acos(X/Length);
      if(Y<0) angle = Math.PI*2-angle;
      return angle;
    }
  }

  /// <summary>Gets the cross vector, analagous to the three-dimensional cross product.</summary>
  /// <value>A <see cref="Vector"/> perpendicular to this vector.</value>
  /// <remarks>While there is no real cross product in two dimensions, this property is analogous in that it
  /// returns a perpendicular vector.
  /// </remarks>
  public Vector CrossVector { get { return new Vector(Y, -X); } }
  /// <include file="../documentation.xml" path="//Mathematics/Vector/Length/*"/>
  public double Length
  { get { return System.Math.Sqrt(X*X+Y*Y); }
    set { Normalize(value); }
  }
  /// <summary>Returns the length of this vector, squared.</summary>
  public double LengthSqr { get { return X*X+Y*Y; } }
  /// <include file="../documentation.xml" path="//Mathematics/Vector/Normal/*"/>
  public Vector Normal { get { return this/Length; } }
  /// <summary>Determines whether the vector is valid.</summary>
  /// <remarks>Invalid vectors are returned by some mathematical functions to signal that the function is undefined
  /// given the input. A vector returned by such a function can be tested for validity using this property.
  /// </remarks>
  public bool Valid { get { return !double.IsNaN(X); } }
  /// <include file="../documentation.xml" path="//Mathematics/Vector/DotProduct/*"/>
  public double DotProduct(Vector v) { return X*v.X + Y*v.Y; }
  /// <include file="../documentation.xml" path="//Mathematics/Vector/Normalize/*"/>
  public void Normalize() { this/=Length; }
  /// <include file="../documentation.xml" path="//Mathematics/Vector/Normalize2/*"/>
  public void Normalize(double length) { this /= Length/length; }
  /// <summary>Returns a copy of this vector, normalized to the given length.</summary>
  /// <remarks>Calling this method is invalid when the length of the vector is zero, since the vector would not be
  /// pointing in any direction and could not possibly be scaled to the correct length.
  /// </remarks>
  public Vector Normalized(double length)
  {
    Vector vector = this;
    vector.Normalize(length);
    return vector;
  }
  /// <summary>Rotates this vector by the given number of radians.</summary>
  /// <param name="angle">The angle to rotate by, in radians.</param>
  public void Rotate(double angle) { this = Rotated(angle); }
  /// <summary>Returns a copy of this vector, rotated by the given number of radians.</summary>
  /// <param name="angle">The angle to rotate by, in radians.</param>
  /// <returns>A new vector with the same magnitude as this one, and rotated by the given angle.</returns>
  public Vector Rotated(double angle)
  { double sin = Math.Sin(angle), cos = Math.Cos(angle);
    return new Vector(X*cos-Y*sin, X*sin+Y*cos);
  }
  /// <include file="../documentation.xml" path="//Mathematics/Vector/Equals/*"/>
  public override bool Equals(object obj) { return obj is Vector ? (Vector)obj==this : false; }
  /// <include file="../documentation.xml" path="//Mathematics/Vector/Equals3/*"/>
  public bool Equals(Vector vect, double epsilon)
  { return Math.Abs(vect.X-X)<=epsilon && Math.Abs(vect.Y-Y)<=epsilon;
  }
  /// <summary>Returns a hash code for this <see cref="Vector"/>.</summary>
  /// <returns>An integer hash code for this <see cref="Vector"/>.</returns>
  public unsafe override int GetHashCode()
  { fixed(double* dp=&X) { int* p=(int*)dp; return *p ^ *(p+1) ^ *(p+2) ^ *(p+3); }
  }
  /// <summary>Converts this <see cref="Vector"/> into an equivalent <see cref="Point"/>.</summary>
  /// <returns>Returns a <see cref="Point"/> with X and Y coordinates equal to the X and Y magnitudes of this
  /// vector.
  /// </returns>
  public Point ToPoint() { return new Point(X, Y); }
  /// <summary>Converts this vector into a human-readable string.</summary>
  /// <returns>A human-readable string representation of this vector.</returns>
  public override string ToString() { return string.Format("[{0:f2},{1:f2}]", X, Y); }

  public static Vector operator-(Vector v) { return new Vector(-v.X, -v.Y); }
  public static Vector operator+(Vector a, Vector b) { return new Vector(a.X+b.X, a.Y+b.Y); }
  public static Vector operator-(Vector a, Vector b) { return new Vector(a.X-b.X, a.Y-b.Y); }
  public static Vector operator*(Vector v, double f) { return new Vector(v.X*f, v.Y*f); }
  public static Vector operator*(double f, Vector v) { return new Vector(v.X*f, v.Y*f); }
  public static Vector operator/(Vector v, double f) { return new Vector(v.X/f, v.Y/f); }

  public static bool operator==(Vector a, Vector b) { return a.X==b.X && a.Y==b.Y; }
  public static bool operator!=(Vector a, Vector b) { return a.X!=b.X || a.Y!=b.Y; }

  /// <summary>Returns an invalid vector.</summary>
  /// <remarks>When a function is presented with input for which it is mathematically undefined, it can return an
  /// invalid vector instead of raising an exception. This property will return an invalid vector.
  /// </remarks>
  public static Vector Invalid { get { return new Vector(double.NaN, double.NaN); } }

  /// <summary>The magnitude of this vector along the X axis.</summary>
  public double X;
  /// <summary>The magnitude of this vector along the Y axis.</summary>
  public double Y;
}
#endregion

#region Point
/// <summary>This structure represents a point in two-dimensional space.</summary>
[Serializable, StructLayout(LayoutKind.Sequential)]
public struct Point
{ 
  /// <summary>Initializes this <see cref="Point"/> from a <see cref="System.Drawing.Point"/>.</summary>
  /// <param name="pt">The <see cref="System.Drawing.Point"/> from which this point will be initialized.</param>
  public Point(System.Drawing.Point pt) { X=pt.X; Y=pt.Y; }
  /// <summary>Initializes this <see cref="Point"/> from a <see cref="System.Drawing.PointF"/>.</summary>
  /// <param name="pt">The <see cref="System.Drawing.PointF"/> from which this point will be initialized.</param>
  public Point(System.Drawing.PointF pt) { X=pt.X; Y=pt.Y; }
  /// <summary>Initializes this <see cref="Point"/> from a set of coordinates.</summary>
  /// <param name="x">The point's X coordinate.</param>
  /// <param name="y">The point's Y coordinate.</param>
  public Point(double x, double y) { X=x; Y=y; }

  /// <summary>Determines whether the point is valid.</summary>
  /// <remarks>Invalid points are returned by some mathematical functions to signal that the function is undefined
  /// given the input. A point returned by such a function can be tested for validity using this property.
  /// </remarks>
  public bool Valid { get { return !double.IsNaN(X); } }
  /// <include file="../documentation.xml" path="//Mathematics/Point/DistanceTo/*"/>
  public double DistanceTo(Point point)
  { double xd=point.X-X, yd=point.Y-Y;
    return Math.Sqrt(xd*xd+yd*yd);
  }
  /// <include file="../documentation.xml" path="//Mathematics/Point/DistanceSquaredTo/*"/>
  public double DistanceSquaredTo(Point point)
  { double xd=point.X-X, yd=point.Y-Y;
    return xd*xd+yd*yd;
  }
  /// <summary>Offsets this point by a given amount, translating it in space.</summary>
  /// <param name="xd">The value to add to the point's X coordinate.</param>
  /// <param name="yd">The value to add to the point's Y coordinate.</param>
  public void Offset(double xd, double yd) { X+=xd; Y+=yd; }
  /// <summary>Converts this point to a <see cref="System.Drawing.Point"/>.</summary>
  /// <returns>A <see cref="System.Drawing.Point"/> containing approximately the same coordinates. The coordinates
  /// will be rounded using <see cref="Math.Round(double)"/> in order to convert them to integers.
  /// </returns>
  public System.Drawing.Point ToPoint() { return new System.Drawing.Point((int)Math.Round(X), (int)Math.Round(Y)); }
  /// <summary>Converts this point to a <see cref="System.Drawing.PointF"/>.</summary>
  /// <returns>A <see cref="System.Drawing.PointF"/> containing approximately the same coordinates.</returns>
  public System.Drawing.PointF ToPointF() { return new System.Drawing.PointF((float)X, (float)Y); }
  /// <include file="../documentation.xml" path="//Mathematics/Point/Equals/*"/>
  public override bool Equals(object obj) { return obj is Point ? (Point)obj==this : false; }
  /// <include file="../documentation.xml" path="//Mathematics/Point/Equals3/*"/>
  public bool Equals(Point point, double epsilon)
  { return Math.Abs(point.X-X)<=epsilon && Math.Abs(point.Y-Y)<=epsilon;
  }
  /// <summary>Calculates a hash code for this <see cref="Point"/>.</summary>
  /// <returns>An integer hash code for this <see cref="Point"/>.</returns>
  public unsafe override int GetHashCode()
  { fixed(double* dp=&X) { int* p=(int*)dp; return *p ^ *(p+1) ^ *(p+2) ^ *(p+3); }
  }
  /// <summary>Converts this <see cref="Point"/> into a human-readable string.</summary>
  /// <returns>A human-readable string representation of this <see cref="Point"/>.</returns>
  public override string ToString() { return string.Format("({0:f2},{1:f2})", X, Y); }

  /// <summary>Returns an invalid point.</summary>
  /// <remarks>When a function is presented with input for which it is mathematically undefined, it can return an
  /// invalid point instead of raising an exception. This property will return an invalid point.
  /// </remarks>
  public static Point Invalid { get { return new Point(double.NaN, double.NaN); } }

  public static Vector operator- (Point lhs, Point rhs)  { return new Vector(lhs.X-rhs.X, lhs.Y-rhs.Y); }
  public static Point  operator- (Point lhs, Vector rhs) { return new Point(lhs.X-rhs.X, lhs.Y-rhs.Y); }
  public static Point  operator+ (Point lhs, Vector rhs) { return new Point(lhs.X+rhs.X, lhs.Y+rhs.Y); }
  public static Point  operator+ (Vector lhs, Point rhs) { return new Point(lhs.X+rhs.X, lhs.Y+rhs.Y); }
  public static bool   operator==(Point lhs, Point rhs)  { return lhs.X==rhs.X && lhs.Y==rhs.Y; }
  public static bool   operator!=(Point lhs, Point rhs)  { return lhs.X!=rhs.X || lhs.Y!=rhs.Y; }

  /// <summary>Implicitly converts a <see cref="System.Drawing.Point"/> to a <see cref="Point"/>.</summary>
  /// <param name="point">The <see cref="System.Drawing.Point"/> to convert.</param>
  /// <returns>A <see cref="Point"/> containing the same coordinates as <paramref name="point"/>.</returns>
  public static implicit operator Point(System.Drawing.Point point) { return new Point(point.X, point.Y); }

  /// <summary>This point's X coordinate.</summary>
  public double X;
  /// <summary>This point's Y coordinate.</summary>
  public double Y;
}
#endregion

#region LineIntersection
/// <summary>This structure contains information about the intersection of two lines or line segments.</summary>
/// <remarks>The structure is returned from some line intersection functions. If the intersection is not valid
/// (ie, the lines given were parallel), the <see cref="Point"/> member will be invalid. You can use
/// <see cref="GameLib.Mathematics.TwoD.Point.Valid"/> to check for this condition.
/// </remarks>
public struct LineIntersection
{ 
  /// <summary>Initializes this structure from an intersection point and information about where the intersection
  /// occurred.
  /// </summary>
  /// <param name="point">The point where the lines intersected, or if the lines didn't intersect, an invalid point.
  /// </param>
  /// <param name="onFirst">This should be true if the intersection point lies on the first line segment.</param>
  /// <param name="onSecond">This should be true if the intersection point lies on the second line segment.</param>
  public LineIntersection(Point point, bool onFirst, bool onSecond)
  { Point=point; OnFirst=onFirst; OnSecond=onSecond;
  }
  /// <summary>Determines whether the intersection point lies on both line segments.</summary>
  /// <value>True if the intersection point lies on both line segments. This indicates that a segment intersection
  /// has occurred.
  /// </value>
  public bool OnBoth { get { return OnFirst && OnSecond; } }
  /// <summary>The intersection point, or an invalid point if the lines did not intersect.</summary>
  /// <remarks>If the lines did not intersect (because they were invalid or parallel), this will be an invalid
  /// point. You can use <see cref="GameLib.Mathematics.TwoD.Point.Valid"/> to check for this condition.
  /// </remarks>
  public Point Point;
  /// <summary>Determines whether the intersection point lies on the first line segment.</summary>
  /// <remarks>If true, the second line intersected the first line segment. If both <see cref="OnFirst"/> and
  /// <see cref="OnSecond"/> are true (<see cref="OnBoth"/> is true), then both segments intersected each other.
  /// </remarks>
  public bool OnFirst;
  /// <summary>Determines whether the intersection point lies on the second line segment.</summary>
  /// <remarks>If true, the first line intersected the second line segment. If both <see cref="OnFirst"/> and
  /// <see cref="OnSecond"/> are true (<see cref="OnBoth"/> is true), then both segments intersected each other.
  /// </remarks>
  public bool OnSecond;
}
#endregion

#region Line
/// <summary>This structure represents a line or line segment.</summary>
/// <remarks>The line is stored in parametric form, which means that it's stored as a point and a vector.</remarks>
[Serializable]
public struct Line
{ 
  /// <summary>Initializes this line from a point's coordinates and a vector's axis magnitudes.</summary>
  /// <param name="x">The X coordinate of a point on the line (or the start of the line segment).</param>
  /// <param name="y">The Y coordinate of a point on the line (or the start of the line segment).</param>
  /// <param name="xd">The magnitude along the X axis of the line's direction. If you're defining a line segment,
  /// this should be the distance from <paramref name="x"/> to the endpoint's X coordinate.
  /// </param>
  /// <param name="yd">The magnitude along the Y axis of the line's direction. If you're defining a line segment,
  /// this should be the distance from <paramref name="y"/> to the endpoint's Y coordinate.
  /// </param>
  public Line(double x, double y, double xd, double yd) { Start=new Point(x, y); Vector=new Vector(xd, yd); }
  /// <include file="../documentation.xml" path="//Mathematics/Line/Line/*"/>
  public Line(Point start, Vector vector) { Start=start; Vector=vector; }
  /// <include file="../documentation.xml" path="//Mathematics/Line/Line2/*"/>
  public Line(Point start, Point end) { Start=start; Vector=end-start; }

  /// <summary>Returns the endpoint of the line segment.</summary>
  /// <remarks>This is equivalent to <see cref="Start"/> + <see cref="Vector"/>.</remarks>
  public Point End { get { return Start+Vector; } set { Vector=value-Start; } }
  /// <summary>Calculates and returns the line segment's length.</summary>
  /// <remarks>This returns the length of <see cref="Vector"/>.</remarks>
  public double Length { get { return Vector.Length; } set { Vector.Length=value; } }
  /// <summary>Calculates and returns the square of the line segment's length.</summary>
  /// <remarks>This returns the square of the length of <see cref="Vector"/>.</remarks>
  public double LengthSqr { get { return Vector.LengthSqr; } }
  /// <summary>Determines whether the line is valid.</summary>
  /// <remarks>Invalid lines are returned by some mathematical functions to signal that the function is undefined
  /// given the input. A line returned by such a function can be tested for validity using this property.
  /// </remarks>
  public bool Valid { get { return Start.Valid; } }

  /// <summary>Returns the point on the line segment closest to the given point.</summary>
  public Point ClosestPointOnSegment(Point point)
  {
    // our line is represented as a vector from a starting point. get the vector from the starting point to 'point'
    Vector pvect = point - Start;
    double dot   = pvect.DotProduct(Vector); // get the dot product of the two vectors
    // if it's negative, the vectors are pointing in roughly opposite directions, so we return the start point.
    if(dot <= 0) return Start;
    double lenSqr = LengthSqr;
    // the dot product is ourLen * otherLen * cos(angle). if we compare dot >= ourLen * ourLen, it's the same as
    // comparing otherLen * cos(angle) >= ourLen. the cosine times otherLen produces the place along our vector that
    // a line perpendicular to us, intersecting 'point' would be. if that place is further than the length of our
    // vector, it's beyond the end, so just return the end.
    if(dot >= lenSqr) return End;
    // dot/lenSqr is a value between 0 and 1 specifying the place along our vector
    return Start + Vector * (dot/lenSqr);
  }

  /// <summary>Returns the signed distance from the line to a given point.</summary>
  /// <param name="point">The <see cref="Point"/> to find the distance to.</param>
  /// <returns>Returns the distance from the point to the nearest point on the line. The distance may be positive or
  /// negative, with the sign indicating which side of the line the point is on. For a line defined in a clockwise
  /// manner, a positive value means that the point is "outside" the line and a negative value indicates that the
  /// point is "inside" the line. You can envision it this way: if this line was one of the clipping lines defining
  /// a convex polygon, a point would be "outside" the line if it was on the side that would put it outside the
  /// polygon. The point would be inside the polygon if it was "inside" all of the lines defining it. If you simply
  /// want the distance to the line, use <see cref="Math.Abs(double)"/> to get the absolute value. If you simply want
  /// to know which side of the line a point is on, use <see cref="WhichSide"/>, which is more efficient.
  /// </returns>
  public double DistanceTo(Point point) { return Vector.CrossVector.Normal.DotProduct(point-Start); }
  /// <include file="../documentation.xml" path="//Mathematics/Line/GetPoint/*"/>
  public Point GetPoint(int point)
  { if(point<0 || point>1) throw new ArgumentOutOfRangeException("point", point, "must be 0 or 1");
    return point==0 ? Start : End;
  }
  
  /// <summary>Determines whether this line (not segment) intersects the given circle.</summary>
  public bool Intersects(Circle circle)
  {
    return Math2D.Intersects(ref circle, ref this);
  }

  /// <summary>Determines whether this line segment intersects the given circle.</summary>
  public bool SegmentIntersects(Circle circle)
  {
    return Math2D.SegmentIntersects(ref circle, ref this);
  }

  /// <summary>Determines whether this line (not segment) intersects the given line (not segment).</summary>
  public bool Intersects(Line line)
  {
    return Math2D.Intersects(ref this, ref line);
  }

  /// <summary>Determines whether this line segments intersects the given line segment.</summary>
  public bool SegmentIntersects(Line segment)
  {
    return Math2D.SegmentIntersects(ref this, ref segment);
  }

  /// <summary>Returns the intersection point of two lines (not segments).</summary>
  /// <returns>The intersection point of the two lines, or an invalid point if the lines do not intersect.
  /// You can check if the point is valid with <see cref="Point.Valid"/>.
  /// </returns>
  public Point Intersection(Line line)
  { 
    return Math2D.Intersection(ref this, ref line);
  }

  /// <summary>Returns the intersection point of two lines segments.</summary>
  /// <returns>The intersection point of the two line segments, or an invalid point if the lines do not intersect.
  /// You can check if the point is valid with <see cref="Point.Valid"/>.
  /// </returns>
  public Point SegmentIntersection(Line segment)
  { 
    return Math2D.Intersection(ref this, ref segment);
  }

  /// <summary>Returns information about the intersection of this line or line segment with another line or line
  /// segment.
  /// </summary>
  /// <param name="line">The line or line segment to test for intersection.</param>
  /// <returns>A <see cref="LineIntersection"/> containing information about the intersection of the two
  /// lines or line segments.
  /// </returns>
  public LineIntersection GetIntersectionInfo(Line line)
  { 
    return Math2D.IntersectionInfo(ref this, ref line);
  }

  /// <summary>Determines whether this line (not segment) intersects the given <see cref="Rectangle"/>.</summary>
  public bool Intersects(Rectangle rect)
  {
    return Math2D.Intersects(ref this, ref rect);
  }

  /// <summary>Determines whether this line segment intersects the given rectangle.</summary>
  public bool SegmentIntersects(Rectangle rect)
  { 
    return Math2D.SegmentIntersects(ref this, ref rect);
  }

  /// <summary>Returns the intersection of this line (not segment) with the given <see cref="Rectangle"/>.</summary>
  /// <returns>The portion of the line inside the rectangle, or an <see cref="Invalid"/> if there is no intersection.</returns>
  public Line Intersection(Rectangle rect)
  {
    return Math2D.Intersection(ref this, ref rect);
  }

  /// <summary>Returns the intersection of this line segment with the given <see cref="Rectangle"/>.</summary>
  /// <returns>The portion of the line inside the rectangle, or an <see cref="Invalid"/> if there is no intersection.</returns>
  public Line SegmentIntersection(Rectangle rect)
  { 
    return Math2D.SegmentIntersection(ref this, ref rect);
  }

  /// <summary>Determines whether this line (not segment) intersects the given possibly-concave polygon.</summary>
  public bool Intersects(Polygon poly)
  {
    return Math2D.Intersects(ref this, poly);
  }

  /// <summary>Determines whether this line segment intersects the given convex polygon.</summary>
  public bool SegmentIntersects(Polygon convexPoly)
  { 
    return Math2D.SegmentIntersects(ref this, convexPoly);
  }

  /// <summary>Returns the intersection of the line (not segment) with a convex polygon.</summary>
  /// <returns>The portion of the line inside the polygon, or an <see cref="Invalid"/> if there is no intersection.</returns>
  public Line Intersection(Polygon convexPoly)
  {
    return Math2D.Intersection(ref this, convexPoly);
  }

  /// <summary>Returns the intersection of the line segment with a convex polygon.</summary>
  /// <returns>The portion of the line segment inside the polygon, or an <see cref="Invalid"/> if there is no
  /// intersection.
  /// </returns>
  public Line SegmentIntersection(Polygon convexPoly)
  {
    return Math2D.SegmentIntersection(ref this, convexPoly);
  }

  /// <summary>Determines which side of a line the given point is on.</summary>
  /// <param name="point">The <see cref="Point"/> to test.</param>
  /// <returns>A value indicating which side of the line the point is on. The value's sign indicates which side of
  /// the line the point is on. For a line defined in a clockwise
  /// manner, a positive value means that the point is "outside" the line and a negative value indicates that the
  /// point is "inside" the line. You can envision it this way: if this line was one of the clipping lines defining
  /// a convex polygon, a point would be "outside" the line if it was on the side that would put it outside the
  /// polygon. The point would be inside the polygon if it was "inside" all of the lines defining it.
  /// </returns>
  public double WhichSide(Point point) { return Vector.CrossVector.DotProduct(point-Start); }
  /// <include file="../documentation.xml" path="//Mathematics/Line/Equals/*"/>
  public override bool Equals(object obj) { return obj is Line ? (Line)obj==this : false; }
  /// <include file="../documentation.xml" path="//Mathematics/Line/Equals3/*"/>
  public bool Equals(Line line, double epsilon)
  { return Start.Equals(line.Start, epsilon) && Vector.Equals(line.Vector, epsilon);
  }
  /// <summary>Calculates a hash code for this <see cref="Line"/>.</summary>
  /// <returns>An integer hash code for this <see cref="Line"/>.</returns>
  public override int GetHashCode() { return Start.GetHashCode() ^ Vector.GetHashCode(); }
  /// <summary>Converts this <see cref="Line"/> into a human-readable string.</summary>
  /// <returns>A human-readable string representing this line.</returns>
  public override string ToString() { return string.Format("{0}->{1}", Start, Vector); }
  /// <summary>Creates a <see cref="Line"/> from two points.</summary>
  /// <param name="x1">The X coordinate of the first point (a point on the line, or the start of the line segment).</param>
  /// <param name="y1">The Y coordinate of the first point (a point on the line, or the start of the line segment).</param>
  /// <param name="x2">The X coordinate of the second point (another point on the line, or the end of the line
  /// segment).
  /// </param>
  /// <param name="y2">The Y coordinate of the second point (another point on the line, or the end of the line
  /// segment).
  /// </param>
  /// <returns>A <see cref="Line"/> initialized with those values.</returns>
  /// <remarks>Since the end point will need to be converted into a vector, some miniscule accuracy may be lost.
  /// Most notably, the <see cref="End"/> property may not be exactly equal to the point defined by
  /// <paramref name="x2"/> and <paramref name="y2"/>.
  /// </remarks>
  public static Line FromPoints(double x1, double y1, double x2, double y2) { return new Line(x1, y1, x2-x1, y2-y1); }
  public static bool operator==(Line lhs, Line rhs) { return lhs.Start==rhs.Start && lhs.Vector==rhs.Vector; }
  public static bool operator!=(Line lhs, Line rhs) { return lhs.Start!=rhs.Start || lhs.Vector!=rhs.Vector; }
  /// <summary>Returns an invalid line.</summary>
  /// <remarks>When a function is presented with input for which it is mathematically undefined, it can return an
  /// invalid line instead of raising an exception. This property will return an invalid line.
  /// </remarks>
  public static Line Invalid { get { return new Line(Point.Invalid, new Vector()); } }
  /// <summary>A point on the line, or the start point of the line segment.</summary>
  public Point  Start;
  /// <summary>The line's direction, or the vector from the start point to the end point of the line segment.</summary>
  public Vector Vector;
}
#endregion

#region Circle
/// <summary>This structure represents a circle.</summary>
[Serializable]
public struct Circle
{ 
  /// <summary>Initializes this circle from a center point and a radius.</summary>
  /// <param name="centerX">The X coordinate of the circle's center point.</param>
  /// <param name="centerY">The Y coordinate of the circle's center point.</param>
  /// <param name="radius">The radius of the circle.</param>
  public Circle(double centerX, double centerY, double radius)
  { Center = new Point(centerX, centerY);
    Radius = radius;
  }
  /// <summary>Initializes this circle from a center point and a radius.</summary>
  /// <param name="center">The circle's center point.</param>
  /// <param name="radius">The radius of the circle.</param>
  public Circle(Point center, double radius)
  { Center = center;
    Radius = radius;
  }

  /// <summary>Calculates and returns the area of the circle.</summary>
  public double Area { get { return RadiusSqr*Math.PI; } }

  public double RadiusSqr
  {
    get { return Radius * Radius; }
  }

  /// <summary>Determines whether the given circle is contained within this circle.</summary>
  public bool Contains(Circle circle)
  {
    return Math2D.Contains(ref this, ref circle);
  }

  /// <summary>Determines whether the given line segment is contained within this circle.</summary>
  public bool Contains(Line segment)
  {
    return Math2D.Contains(ref this, ref segment);
  }

  /// <summary>Determines whether the given point is fully contained within this circle.</summary>
  public bool Contains(Point point)
  {
    return Math2D.Contains(ref this, ref point);
  }
  
  /// <summary>Determines whether the given rectangle is fully contained within this circle.</summary>
  public bool Contains(Rectangle rect)
  {
    return Math2D.Contains(ref this, ref rect);
  }

  /// <summary>Determines whether the given possibly-concave polygon is fully contained within this circle.</summary>
  public bool Contains(Polygon poly)
  {
    return Math2D.Contains(ref this, poly);
  }

  public override bool Equals(object obj)
  {
    return obj is Circle ? this == (Circle)obj : false;
  }

  public override int GetHashCode()
  {
    return Center.GetHashCode() ^ Radius.GetHashCode();
  }

  /// <summary>Determines whether the given circle intersects this circle.</summary>
  public bool Intersects(Circle circle)
  {
    return Math2D.Intersects(ref this, ref circle);
  }
  
  /// <summary>Determines whether the given line (not segment) intersects this circle.</summary>
  public bool Intersects(Line line)
  {
    return Math2D.Intersects(ref this, ref line);
  }
  
  /// <summary>Determines whether the given line segment intersects this circle.</summary>
  public bool SegmentIntersects(Line segment)
  {
    return Math2D.SegmentIntersects(ref this, ref segment);
  }

  /// <summary>Determines whether the given rectangle intersects this circle.</summary>
  public bool Intersects(Rectangle rect)
  {
    return Math2D.Intersects(ref this, ref rect);
  }
  
  /// <summary>Determines whether the given convex polygon intersects this circle.</summary>
  public bool Intersects(Polygon convexPoly)
  {
    return Math2D.Intersects(ref this, convexPoly);
  }

  /// <summary>The center point of this circle.</summary>
  public Point Center;
  
  /// <summary>The radius of this circle.</summary>
  public double Radius;

  public static bool operator==(Circle a, Circle b)
  {
    return a.Center.X == b.Center.X && a.Center.Y == b.Center.Y && a.Radius == b.Radius;
  }

  public static bool operator!=(Circle a, Circle b)
  {
    return a.Center.X != b.Center.X || a.Center.Y != b.Center.Y || a.Radius != b.Radius;
  }
}
#endregion

#region Corner
/// <summary>This structure represents a corner (a point, and two connected edges).</summary>
/// <remarks>The two connected edges are stored as vectors from the corner point.</remarks>
[Serializable]
public struct Corner
{ 
  /// <summary>Gets the first edge of the corner. The edge's end point should be approximately equal to
  /// <see cref="Point"/> (the edge ends at <see cref="Point"/>).
  /// </summary>
  public Line Edge0 { get { return new Line(Point+Vector0, -Vector0); } }
  /// <summary>Gets the second edge of the corner.</summary>
  /// <remarks>The edge's start point will be equal to <see cref="Point"/>.</remarks>
  public Line Edge1 { get { return new Line(Point, Vector1); } }
  /// <summary>Gets the signed magnitude of the cross product of the two edge vectors.</summary>
  /// <remarks>Given that the two edges both lie on the same plane, their cross product will be a vector perpendicular
  /// to that plane. The sign of the value determines from which side of the plane the vector extends. This can be
  /// used to determine whether the two corner edges are defined in a clockwise or counter-clockwise manner. A
  /// positive value means the edges indicates a clockwise ordering, and a negative value indicates a
  /// counter-clockwise ordering. A zero value indicates that the two edge vectors are coincident.
  /// </remarks>
  public double CrossZ
  { get
    { Point p0 = Point+Vector0, p2 = Point+Vector1;
      return (Point.X-p0.X)*(p2.Y-Point.Y) - (Point.Y-p0.Y)*(p2.X-Point.X);
    }
  }
  /// <summary>Gets the specified edge.</summary>
  /// <param name="edge">The index of the edge to retrieve, either 0 or 1.</param>
  /// <returns>Returns <see cref="Edge0"/> or <see cref="Edge1"/> depending on whether <paramref name="edge"/> is
  /// 0 or 1, respectively.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="edge"/> is not 0 or 1.</exception>
  public Line GetEdge(int edge)
  { if(edge<0 || edge>1) throw new ArgumentOutOfRangeException("GetEdge", edge, "must be 0 or 1");
    return edge==0 ? Edge0 : Edge1;
  }
  /// <summary>Gets one of the three points that make up this corner.</summary>
  /// <param name="point">The index of the point to retrieve, from -1 to 1.</param>
  /// <returns>
  /// <list type="table">
  /// <listheader><term><paramref name="point"/></term><description>Return value</description></listheader>
  /// <item><term>-1</term><description><see cref="Point"/> + <see cref="Vector0"/></description></item>
  /// <item><term>0</term><description><see cref="Point"/></description></item>
  /// <item><term>1</term><description><see cref="Point"/> + <see cref="Vector1"/></description></item>
  /// </list>
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="point"/> is less than -1 or greater
  /// than 1.
  /// </exception>
  public Point GetPoint(int point)
  { if(point<-1 || point>1) throw new ArgumentOutOfRangeException("GetPoint", point, "must be from -1 to 1");
    return point==0 ? Point : (point==-1 ? Point+Vector0 : Point+Vector1);
  }
  /// <summary>The corner point.</summary>
  public Point Point;
  /// <summary>The vector from the corner point (<see cref="Point"/>) to the beginning of the first edge.</summary>
  public Vector Vector0;
  /// <summary>The vector from the corner point (<see cref="Point"/>) to the end of the second edge.</summary>
  public Vector Vector1;
}
#endregion

#region Rectangle
/// <summary>This structure represents a rectangle.</summary>
[Serializable, StructLayout(LayoutKind.Sequential)]
public struct Rectangle
{ 
  /// <summary>Initializes this rectangle from a <see cref="System.Drawing.Rectangle"/>.</summary>
  /// <param name="rect">The <see cref="System.Drawing.Rectangle"/> from which this rectangle will be initialized.</param>
  public Rectangle(System.Drawing.Rectangle rect)
  { X=rect.X; Y=rect.Y; Width=rect.Width; Height=rect.Height;
  }
  /// <summary>Initializes this rectangle from a <see cref="System.Drawing.RectangleF"/>.</summary>
  /// <param name="rect">The <see cref="System.Drawing.RectangleF"/> from which this rectangle will be initialized.</param>
  public Rectangle(System.Drawing.RectangleF rect) { X=rect.X; Y=rect.Y; Width=rect.Width; Height=rect.Height; }
  /// <summary>Initializes this rectangle from a position and a size.</summary>
  /// <param name="x">The X coordinate of the rectangle's top-left corner.</param>
  /// <param name="y">The Y coordinate of the rectangle's top-left corner.</param>
  /// <param name="width">The rectangle's width. This should not be negative.</param>
  /// <param name="height">The rectangle's height. This should not be negative.</param>
  public Rectangle(double x, double y, double width, double height) { X=x; Y=y; Width=width; Height=height; }
  /// <summary>Initializes this rectangle from a position and a size.</summary>
  /// <param name="location">The rectangle's top-left corner.</param>
  /// <param name="size">The vector from the <paramref name="location"/> to the rectangle's bottom-right conrner.
  /// In other words, a vector holding the width and height of the rectangle.
  /// </param>
  public Rectangle(Point location, Vector size) { X=location.X; Y=location.Y; Width=size.X; Height=size.Y; }
  /// <summary>Initializes this rectangle from two points.</summary>
  /// <param name="corner1">One corner of the rectangle.</param>
  /// <param name="corner2">The opposite corner of the rectangle.</param>
  /// <remarks>Since one corner will need to be converted into a vector, some miniscule accuracy may be lost.</remarks>
  public Rectangle(Point corner1, Point corner2)
  { double x2, y2;
    if(corner1.X<=corner2.X) { X=corner1.X; x2=corner2.X; }
    else { X=corner2.X; x2=corner1.X; }
    if(corner1.Y<=corner2.Y) { Y=corner1.Y; y2=corner2.Y; }
    else { Y=corner2.Y; y2=corner1.Y; }
    Width=x2-X; Height=y2-Y;
  }

  /// <summary>Gets the bottom of the rectangle.</summary>
  /// <remarks>This is equivalent to <see cref="Y"/> + <see cref="Height"/>.</remarks>
  public double Bottom { get { return Y+Height; } }
  /// <summary>Gets the bottom-right corner of the rectangle.</summary>
  /// <remarks>This is equivalent to <see cref="TopLeft"/> + <see cref="Size"/>.</remarks>
  public Point BottomRight { get { return new Point(X+Width, Y+Height); } }
  /// <summary>Gets or sets the top-left corner of the rectangle.</summary>
  public Point Location
  { get { return new Point(X, Y); }
    set { X=value.X; Y=value.Y; }
  }
  /// <summary>Gets the right side of the rectangle.</summary>
  /// <remarks>This is equivalent to <see cref="X"/> + <see cref="Width"/>.</remarks>
  public double Right { get { return X+Width; } }
  /// <summary>Gets or sets the size of the rectangle.</summary>
  public Vector Size
  { get { return new Vector(Width, Height); }
    set { Width=value.X; Height=value.Y; }
  }
  /// <summary>Gets or sets the top-left corner of the rectangle.</summary>
  public Point TopLeft
  { get { return new Point(X, Y); }
    set { X=value.X; Y=value.Y; }
  }

  /// <summary>Determines whether the specified circle is fully contained within this rectangle.</summary>
  public bool Contains(Circle circle)
  {
    return Math2D.Contains(ref this, ref circle);
  }

  /// <summary>Determines whether the specified line segment is fully contained within this rectangle.</summary>
  public bool Contains(Line segment)
  {
    return Math2D.Contains(ref this, ref segment);
  }

  /// <summary>Determines whether the specified point lies within the rectangle.</summary>
  public bool Contains(Point point)
  {
    return Math2D.Contains(ref this, ref point);
  }

  /// <summary>Determines whether this rectangle completely contains the specified rectangle.</summary>
  public bool Contains(Rectangle rect)
  {
    return Math2D.Contains(ref this, ref rect);
  }

  /// <summary>Determines whether the given possibly-concave polygon is completely contained within this rectangle.</summary>
  public bool Contains(Polygon poly)
  {
    return Math2D.Contains(ref this, poly);
  }

  /// <summary>Determines whether the given circle intersects this rectangle.</summary>
  public bool Intersects(Circle circle)
  {
    return Math2D.Intersects(ref circle, ref this);
  }

  /// <summary>Determines whether the given line (not segment) intersects this rectangle</summary>
  public bool Intersects(Line line)
  {
    return Math2D.Intersects(ref line, ref this);
  }

  /// <summary>Determines whether the given line segment intersects this rectangle</summary>
  public bool SegmentIntersects(Line segment)
  {
    return Math2D.SegmentIntersects(ref segment, ref this);
  }

  /// <summary>Determines whether the given rectangle intersects this one.</summary>
  public bool Intersects(Rectangle rect)
  {
    return Math2D.Intersects(ref this, ref rect);
  }

  /// <summary>Determines whether this rectangle intersects the given convex polygon.</summary>
  public bool Intersects(Polygon convexPoly)
  {
    return Math2D.Intersects(ref this, convexPoly);
  }

  /// <summary>Calculates the intersection of the given line (not segment) with this rectangle.</summary>
  /// <returns>Returns the line clipped to this rectangle, or <see cref="Line.Invalid"/> if there was no intersection.</returns>
  public Line Intersection(Line line)
  {
    return Math2D.Intersection(ref line, ref this);
  }

  /// <summary>Calculates the intersection of the given line segment with this rectangle.</summary>
  /// <returns>Returns the line segment clipped to this rectangle, or <see cref="Line.Invalid"/> if there was no intersection.</returns>
  public Line SegmentIntersection(Line segment)
  {
    return Math2D.SegmentIntersection(ref segment, ref this);
  }

  public override bool Equals(object obj) { return obj is Rectangle && this==(Rectangle)obj; }
  /// <summary>Gets an edge of the rectangle.</summary>
  /// <param name="i">The index of the edge to retrieve (from 0 to 3).</param>
  /// <returns>The top, left, right, and bottom edges for respective values of <paramref name="i"/> from 0 to 3.</returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="i"/> is less than 0 or greater than 3.</exception>
  public Line GetEdge(int i)
  { if(i<0 || i>3) throw new ArgumentOutOfRangeException("i", i, "must be from 0 to 3");
    switch(i)
    { case 0: return new Line(X, Y, 0, Height);       // left
      case 1: return new Line(X, Y, Width, 0);        // top
      case 2: return new Line(X+Width, Y, 0, Height); // right
      case 3: return new Line(X, Y+Height, Width, 0); // bottom
      default: return Line.Invalid; // can't get here
    }
  }
  /// <summary>Gets a corner of the rectangle.</summary>
  /// <param name="i">The index of the point to retrieve (from 0 to 3).</param>
  /// <returns>The top-left, top-right, bottom-right, and bottom-left corners for respective values of
  /// <paramref name="i"/> from 0 to 3.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="i"/> is less than 0 or greater than 3.</exception>
  public Point GetPoint(int i)
  { if(i<0 || i>3) throw new ArgumentOutOfRangeException("i", i, "must be from 0 to 3");
    switch(i)
    { case 0: return new Point(X, Y);
      case 1: return new Point(X+Width, Y);
      case 2: return new Point(X+Width, Y+Height);
      case 3: return new Point(X, Y+Height);
      default: return Point.Invalid; // can't get here
    }
  }
  /// <summary>Inflates this rectangle by the given amount.</summary>
  /// <param name="x">The amount to inflate by on the X axis.</param>
  /// <param name="y">The amount to inflate by on the Y axis.</param>
  /// <remarks>All edges will be offset by the given values, so the actual difference in width and height will be
  /// twice the value of x and y.
  /// </remarks>
  public void Inflate(double x, double y) { X-=x; Width+=x*2; Y-=y; Height+=y*2; }
  /// <summary>Returns a copy of this rectangle, inflated by the given amount.</summary>
  /// <param name="x">The amount to inflate by on the X axis.</param>
  /// <param name="y">The amount to inflate by on the Y axis.</param>
  /// <remarks>All edges will be offset by the given values, so the actual difference in width and height will be
  /// twice the value of x and y.
  /// </remarks>
  public Rectangle Inflated(double x, double y) { return new Rectangle(X-x, Y-y, Width+x*2, Height+y*2); }
  /// <summary>Sets this rectangle to the intersection of this rectangle with the specified rectangle.</summary>
  /// <param name="rect">The rectangle to use for intersection.</param>
  /// <remarks>If the rectangles do not intersect, this rectangle will be set to an empty rectangle (with a width
  /// and height of zero).
  /// </remarks>
  public void Intersect(Rectangle rect)
  { 
    this = Math2D.Intersection(ref this, ref rect);
  }
  /// <summary>Returns the intersection of this rectangle with the specified rectangle.</summary>
  /// <param name="rect">The rectangle to use for intersection.</param>
  /// <returns>The intersection of this rectangle and <paramref name="rect"/>, or an empty rectangle (a rectangle
  /// with a width and height of zero) if there is no intersection.
  /// </returns>
  public Rectangle Intersection(Rectangle rect)
  { 
    return Math2D.Intersection(ref this, ref rect);
  }
  /// <summary>Returns the union of this rectangle with the given rectangle.</summary>
  /// <param name="rect">The rectangle with which this rectangle will be combined.</param>
  /// <returns>The smallest rectangle that contains both this and <paramref name="rect"/>.</returns>
  public Rectangle Union(Rectangle rect)
  { Rectangle ret = new Rectangle(X, Y, Width, Height);
    ret.Unite(rect);
    return ret;
  }
  /// <summary>Sets this rectangle to the union of this rectangle with the given rectangle.</summary>
  /// <param name="rect">The rectangle with which this rectangle will be combined.</param>
  /// <remarks>Sets this rectangle to the smallest rectangle that contains both this and <paramref name="rect"/>. If
  /// <paramref name="rect"/> is an empty rectangle, there will be no effect.
  /// </remarks>
  public void Unite(Rectangle rect)
  { 
    if(rect.Width != 0 || rect.Height != 0) // uniting with an empty rectangle has no effect
    {
      if(X>rect.X) { Width += X-rect.X; X=rect.X; }
      if(Y>rect.Y) { Height += Y-rect.Y; Y=rect.Y; }
      if(Right<rect.Right)   Width  += rect.Right-Right;
      if(Bottom<rect.Bottom) Height += rect.Bottom-Bottom;
    }
  }
  /// <summary>Offsets this rectangle by the given amount.</summary>
  /// <param name="x">The amount to offset along the X axis.</param>
  /// <param name="y">The amount to offset along the Y axis.</param>
  /// <remarks>This has the effect of offsetting <see cref="X"/> and <see cref="Y"/>.</remarks>
  public void Offset(double x, double y) { X+=x; Y+=y; }
  /// <summary>Offsets this rectangle by the given amount.</summary>
  /// <param name="vect">A <see cref="Vector"/> specifying the offset.</param>
  /// <remarks>This has the effect of offsetting <see cref="X"/> and <see cref="Y"/>.</remarks>
  public void Offset(Vector vect) { X+=vect.X; Y+=vect.Y; }
  /// <summary>Converts this rectangle into human-readable string.</summary>
  /// <returns>A human-readable string representation of this rectangle.</returns>
  public override string ToString()
  { return string.Format("X={0:F2} Y={1:F2} Width={2:F2} Height={3:F2}", X, Y, Width, Height);
  }
  /// <summary>Calculates a hash code for this <see cref="Rectangle"/>.</summary>
  /// <returns>An integer hash code for this <see cref="Rectangle"/>.</returns>
  public unsafe override int GetHashCode()
  { fixed(double* dp=&X) { int* p=(int*)dp; return *p ^ *(p+4) ^ *(p+8) ^ *(p+12); }
  }
  /// <summary>Initializes a rectangle from two points and returns it.</summary>
  /// <param name="x1">The X coordinate of one corner of the rectangle.</param>
  /// <param name="y1">The Y coordinate of one corner of the rectangle.</param>
  /// <param name="x2">The X coordinate of the opposite corner of the rectangle.</param>
  /// <param name="y2">The Y coordinate of the opposite corner of the rectangle.</param>
  /// <returns></returns>
  public static Rectangle FromPoints(double x1, double y1, double x2, double y2)
  { return new Rectangle(new Point(x1, y1), new Point(x2, y2));
  }

  public static bool operator==(Rectangle a, Rectangle b)
  { return a.X==b.X && a.Y==b.Y && a.Width==b.Width && a.Height==b.Height;
  }

  public static bool operator!=(Rectangle a, Rectangle b)
  { return a.X!=b.X || a.Y!=b.Y || a.Width!=b.Width || a.Height!=b.Height;
  }

  /// <summary>The X coordinate of the top-left corner of the rectangle.</summary>
  public double X;
  /// <summary>The Y coordinate of the top-left corner of the rectangle.</summary>
  public double Y;
  /// <summary>The width of the rectangle. This value should not be negative.</summary>
  public double Width;
  /// <summary>The height of the rectangle. This value should not be negative.</summary>
  public double Height;
}
#endregion

#region Polygon
/// <summary>This class represents a polygon.</summary>
[Serializable]
public sealed class Polygon : ICloneable, ISerializable
{ 
  /// <summary>Initializes this polygon with no points.</summary>
  public Polygon() { points=new Point[4]; }
  /// <summary>Initializes this polygon with three given points.</summary>
  /// <param name="p1">The first <see cref="Point"/>.</param>
  /// <param name="p2">The second <see cref="Point"/>.</param>
  /// <param name="p3">The third <see cref="Point"/>.</param>
  public Polygon(Point p1, Point p2, Point p3) { points = new Point[3] { p1, p2, p3 }; length=3; }
  /// <summary>Initializes this polygon from an array of points.</summary>
  /// <param name="points">The array containing the points to use.</param>
  public Polygon(IList<Point> points) : this(points.Count) { AddPoints(points); }
  /// <summary>Initializes this polygon from an array of points.</summary>
  /// <param name="points">The array containing the points to use.</param>
  /// <param name="nPoints">The number of points to read from the array.</param>
  public Polygon(Point[] points, int nPoints) : this(nPoints) { AddPoints(points, nPoints); }
  /// <summary>Initializes this polygon with the given starting capacity.</summary>
  /// <param name="capacity">The number of points the polygon can initially hold.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="capacity"/> is negative.</exception>
  public Polygon(int capacity)
  { if(capacity<0) throw new ArgumentOutOfRangeException("capacity", capacity, "must not be negative");
    points = new Point[Math.Max(3, capacity)];
  }
  /// <summary>Deserializes this polygon.</summary>
  /// <param name="info">A <see cref="SerializationInfo"/> object.</param>
  /// <param name="context">A <see cref="StreamingContext"/> object.</param>
  /// <remarks>This constructor is used to deserialize a polygon, and generally does not need to be called from user
  /// code.
  /// </remarks>
  private Polygon(SerializationInfo info, StreamingContext context)
  { length = info.GetInt32("length");
    points = new Point[Math.Max(3, length)];
    for(int i=0; i<length; i++) points[i] = (Point)info.GetValue(i.ToString(), typeof(Point));
  }
  /// <summary>Gets or sets one of the polygon's points.</summary>
  /// <param name="index">The index of the point to get or set.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than zero or greater
  /// than or equal to <see cref="Length"/>.
  /// </exception>
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
  /// <summary>Gets or sets the number of points that this polygon is capable of holding without reallocating memory.</summary>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if you try to set the capacity to a value less than
  /// <see cref="Length"/>.
  /// </exception>
  public int Capacity
  { get { return points.Length; }
    set
    { if(value<length)
        throw new ArgumentOutOfRangeException("value", value, "The capacity cannot be less than Length.");
      if(value<3) value = 3;
      if(value==points.Length) return;
      Point[] narr = new Point[value];
      Array.Copy(points, narr, length);
      points = narr;
    }
  }
  /// <summary>Copies the points from this polygon into an array.</summary>
  /// <param name="array">The array to copy into.</param>
  /// <param name="index">The index at which to begin copying.</param>
  public void CopyTo(Point[] array, int index) { Array.Copy(points, 0, array, index, length); }
  /// <summary>Gets the number of points in the polygon.</summary>
  public int Length { get { return length; } }

  #region ICloneable Members
  /// <summary>Returns a clone of this polygon.</summary>
  /// <returns>Returns a new <see cref="Polygon"/> with the same points as this one.</returns>
  public Polygon Clone() { return new Polygon(points, length); }

  object ICloneable.Clone() { return Clone(); }
  #endregion

  #region ISerializable Members
  /// <summary>This method is used to serialize this polygon.</summary>
  /// <param name="info">A <see cref="SerializationInfo"/> object.</param>
  /// <param name="context">A <see cref="StreamingContext"/> object.</param>
  /// <remarks>This method is used to serialize a polygon, and generally does not need to be called from user code.</remarks>
  public void GetObjectData(SerializationInfo info, StreamingContext context)
  { info.AddValue("length", length);
    for(int i=0; i<length; i++) info.AddValue(i.ToString(), points[i]);
  }
  #endregion

  /// <summary>Adds a new point to the polygon.</summary>
  /// <param name="x">The X coordinate of the point.</param>
  /// <param name="y">The Y coordinate of the point.</param>
  /// <returns>Returns the index of the new point.</returns>
  public int AddPoint(double x, double y) { return AddPoint(new Point(x, y)); }
  /// <summary>Adds a new point to the polygon.</summary>
  /// <param name="point">The <see cref="Point"/> to add.</param>
  /// <returns>Returns the index of the new point.</returns>
  public int AddPoint(Point point)
  { if(length==points.Length) ResizeTo(length+1);
    points[length] = point;
    return length++;
  }
  /// <summary>Adds a list of points to the polygon.</summary>
  /// <param name="points">An array of points that will be added to the polygon.</param>
  public void AddPoints(IList<Point> points)
  { ResizeTo(length+points.Count);
    for(int i=0,len=points.Count; i<len; i++) this.points[length++] = points[i];
  }
  /// <summary>Adds a list of points to the polygon.</summary>
  /// <param name="points">An array of points.</param>
  /// <param name="nPoints">The number of points to read from the array.</param>
  public void AddPoints(Point[] points, int nPoints)
  { ResizeTo(length+nPoints);
    for(int i=0; i<nPoints; i++) this.points[length++] = points[i];
  }
  /// <summary>Asserts that this is a valid polygon.</summary>
  /// <exception cref="InvalidOperationException">Thrown if the polygon contains less than three points.</exception>
  public void AssertValid()
  { if(length<3) throw new InvalidOperationException("Not a valid polygon [not enough points]!");
  }
  /// <summary>Removes all points from the polygon.</summary>
  public void Clear() { length = 0; }

  /// <summary>Returns true if the given circle is fully contained within this convex polygon.</summary>
  /// <remarks>The result of calling this method on a nonconvex polygon is undefined.</remarks>
  public bool ConvexContains(Circle circle)
  {
    return Math2D.Contains(this, ref circle);
  }

  /// <summary>Returns true if the given line segment is fully contained within this convex polygon.</summary>
  /// <remarks>The result of calling this method on a nonconvex polygon is undefined.</remarks>
  public bool ConvexContains(Line segment)
  {
    return Math2D.Contains(this, ref segment);
  }

  /// <summary>Returns true if the given point is contained within this convex polygon.</summary>
  /// <remarks>The result of calling this method on a nonconvex polygon is undefined.</remarks>
  public bool ConvexContains(Point point)
  {
    return Math2D.Contains(this, ref point);
  }

  /// <summary>Returns true if the given rectangle is fully contained within this convex polygon.</summary>
  /// <remarks>The result of calling this method on a nonconvex polygon is undefined.</remarks>
  public bool ConvexContains(Rectangle rect)
  {
    return Math2D.Contains(this, ref rect);
  }

  /// <summary>Returns true if the given possibly-concave polygon is fully contained within this convex polygon.</summary>
  /// <remarks>The result of calling this method on a nonconvex polygon is undefined.</remarks>
  public bool ConvexContains(Polygon poly)
  {
    return Math2D.Contains(this, poly);
  }

  /// <summary>Determines whether the given circle intersects this convex polygon.</summary>
  /// <remarks>The result of calling this method on a nonconvex polygon is undefined.</remarks>
  public bool ConvexIntersects(Circle circle)
  {
    return Math2D.Intersects(ref circle, this);
  }

  /// <summary>Determines whether the given line (not segment) intersects this convex polygon.</summary>
  /// <remarks>The result of calling this method on a nonconvex polygon is undefined.</remarks>
  public bool ConvexIntersects(Line line)
  {
    return Math2D.Intersects(ref line, this);
  }

  /// <summary>Determines whether the given line segment intersects this convex polygon.</summary>
  /// <remarks>The result of calling this method on a nonconvex polygon is undefined.</remarks>
  public bool ConvexSegmentIntersects(Line segment)
  {
    return Math2D.Intersects(ref segment, this);
  }

  /// <summary>Determines whether the given rectangle intersects this convex polygon.</summary>
  /// <remarks>The result of calling this method on a nonconvex polygon is undefined.</remarks>
  public bool ConvexIntersects(Rectangle rect)
  {
    return Math2D.Intersects(ref rect, this);
  }

  /// <summary>Determines whether the given convex polygon intersects this convex polygon.</summary>
  /// <remarks>The result of calling this method on a nonconvex polygon is undefined.</remarks>
  public bool ConvexIntersects(Polygon convexPoly)
  {
    return Math2D.Intersects(this, convexPoly);
  }

  /// <summary>Calculates the intersection of the given line (not segment) with this convex polygon.</summary>
  /// <returns>Returns the line clipped to the polygon, or <see cref="Line.Invalid"/> if there was no intersection.</returns>
  /// <remarks>The result of calling this method on a nonconvex polygon is undefined.</remarks>
  public Line ConvexIntersection(Line line)
  {
    return Math2D.Intersection(ref line, this);
  }

  /// <summary>Calculates the intersection of the given line segment with this convex polygon.</summary>
  /// <returns>Returns the line segment clipped to the polygon, or <see cref="Line.Invalid"/> if there was no
  /// intersection.
  /// </returns>
  /// <remarks>The result of calling this method on a nonconvex polygon is undefined.</remarks>
  public Line ConvexSegmentIntersection(Line segment)
  {
    return Math2D.SegmentIntersection(ref segment, this);
  }

  /// <summary>Calculates and returns the area of the polygon.</summary>
  /// <returns>The area of the polygon.</returns>
  public double GetArea()
  { AssertValid();
    double area=0;
    int i;
    for(i=0; i<length-1; i++) area += points[i].X*points[i+1].Y - points[i+1].X*points[i].Y;
    area += points[i].X*points[0].Y - points[0].X*points[i].Y;
    return Math.Abs(area)/2;
  }
  /// <summary>Calculates and returns this polygon's bounding box.</summary>
  /// <returns>The smallest rectangle that contains this polygon.</returns>
  public Rectangle GetBounds()
  { AssertValid();
    Rectangle ret = new Rectangle(double.MaxValue, double.MaxValue, 0, 0);
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
  /// <summary>Calculates and returns the polygon's centroid.</summary>
  /// <returns>The centroid of the polygon.</returns>
  /// <remarks>The centroid of a polygon is its center of mass (assuming it is uniformly dense).</remarks>
  public Point GetCentroid()
  { AssertValid();
    double area=0,x=0,y=0,d;
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
  /// <summary>Gets the specified corner of the polygon.</summary>
  /// <param name="index">The index of the corner to retrieve, from 0 to <see cref="Length"/>-1.</param>
  /// <returns>A <see cref="Corner"/> representing the requested corner.</returns>
  public Corner GetCorner(int index)
  { AssertValid();
    Corner c = new Corner();
    c.Point = this[index];
    c.Vector0 = GetPoint(index-1) - c.Point;
    c.Vector1 = GetPoint(index+1) - c.Point;
    return c;
  }
  /// <summary>Gets the specified edge of the polygon.</summary>
  /// <param name="index">The index of the edge to retrieve, from 0 to <see cref="Length"/>-1.</param>
  /// <returns>A <see cref="Line"/> segment representing the requested edge, built from the vertex at the given index
  /// and the next vertex (wrapping around to zero if <paramref name="index"/> is the last vertex).
  /// </returns>
  public Line GetEdge(int index)
  { if(length<2) throw new InvalidOperationException("Polygon has no edges [not enough points]!");
    return new Line(this[index], GetPoint(index+1));
  }
  /// <summary>Gets the specified point of the polygon.</summary>
  /// <param name="index">The index of the point to retrieve, from -<see cref="Length"/> to <see cref="Length"/>*2-1.</param>
  /// <returns>The requested <see cref="Point"/>.</returns>
  /// <remarks>This method treats the list of points as circular, and allows negative indexes and indexes greater
  /// than or equal to <see cref="Length"/>, as long as the index is from -<see cref="Length"/> to
  /// <see cref="Length"/>*2-1. So if <see cref="Length"/> is 4, indexes of -4 and 7 are okay (they'll return points 0
  /// and 3 respectively), but -5 and 8 are not.
  /// </remarks>
  public Point GetPoint(int index)
  { return index<0 ? this[length+index] : index>=length ? this[index-length] : this[index];
  }
  /// <summary>Inserts a point into the polygon.</summary>
  /// <param name="point">The <see cref="Point"/> to insert.</param>
  /// <param name="index">The index at which the point should be inserted.</param>
  public void InsertPoint(Point point, int index)
  { if(length==points.Length) ResizeTo(length+1);
    if(index<length) for(int i=length; i>index; i--) points[i] = points[i-1];
    length++;
    this[index] = point;
  }
  /// <summary>Determines whether the polygon was defined in a clockwise or counter-clockwise manner.</summary>
  /// <returns>True if the polygon points are defined in a clockwise manner and false otherwise.</returns>
  /// <remarks>This method only makes sense for convex polygons. The result of calling this method on a nonconvex
  /// polygon is undefined.
  /// </remarks>
  public bool IsClockwise()
  { for(int i=0; i<length; i++)
    { int sign = Math.Sign(GetCorner(i).CrossZ);
      if(sign==1) return true;
      else if(sign==-1) return false;
    }
    return true;
  }
  /// <summary>Determines whether the polygon is convex.</summary>
  /// <returns>True if the polygon is convex and false otherwise.</returns>
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
  /// <summary>Offsets the polygon by the given amount by offsetting all the points.</summary>
  /// <param name="offset">A <see cref="Vector"/> containing the offset.</param>
  public void Offset(Vector offset) { Offset(offset.X, offset.Y); }
  /// <summary>Offsets the polygon by the given amount by offsetting all the points.</summary>
  /// <param name="xd">The distance to offset along the X axis.</param>
  /// <param name="yd">The distance to offset along the Y axis.</param>
  public void Offset(double xd, double yd) { for(int i=0; i<length; i++) points[i].Offset(xd, yd); }
  /// <summary>Removes a point from the polygon.</summary>
  /// <param name="index">The index of the point to remove.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than zero or greater
  /// than or equal to <see cref="Length"/>.
  /// </exception>
  public void RemovePoint(int index)
  { if(index<0 || index>=length) throw new ArgumentOutOfRangeException("index");
    if(index != --length) for(int i=index; i<length; i++) points[i]=points[i+1];
  }
  /// <summary>Removes a range of points from the polygon.</summary>
  /// <param name="start">The index of the first point to remove.</param>
  /// <param name="length">The number of points to remove.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if the range given falls outside the value range of points.
  /// </exception>
  public void RemoveRange(int start, int length)
  { if(length==0) return;
    int end = start+length;
    if(start<0 || end<0 || end>this.length || start>=this.length) throw new ArgumentOutOfRangeException();
    for(; end<this.length; end++) points[end-length]=points[end];
    this.length -= length;
  }
  /// <summary>Reverses the order of this polygon's points.</summary>
  /// <remarks>This can be used to convert a convex polygon to and from clockwise ordering.</remarks>
  public void Reverse()
  { Point pt;
    for(int i=0,j=length-1,len=length/2; i<len; j--,i++) { pt = points[i]; points[i] = points[j]; points[j] = pt; }
  }
  /// <summary>Returns a copy of this polygon, with the points in reversed order.</summary>
  /// <returns>A copy of this polygon, with the points reversed.</returns>
  /// <remarks>This can be used to convert a convex polygon to and from clockwise ordering.</remarks>
  public Polygon Reversed()
  { Polygon newPoly = new Polygon(length);
    for(int i=length-1; i>=0; i--) newPoly.AddPoint(points[i]);
    return newPoly;
  }
  /// <summary>Rotates the points in this polygon around the origin (0,0).</summary>
  /// <param name="angle">The angle by which to rotate, in degrees.</param>
  public void Rotate(double angle) { Math2D.Rotate(points, 0, length, angle); }
  /// <summary>Returns a copy of this polygon, with the points rotated around the origin (0,0).</summary>
  /// <param name="angle">The angle by which to rotate, in degrees.</param>
  /// <returns>A copy of this polygon, with the points rotated.</returns>
  public Polygon Rotated(double angle)
  { Polygon poly = new Polygon(points, length);
    poly.Rotate(angle);
    return poly;
  }
  /// <summary>Scales the points in this polygon by the given factors.</summary>
  /// <param name="xScale">The factor by which to multiply the X coordinates.</param>
  /// <param name="yScale">The factor by which to multiply the Y coordinates.</param>
  public void Scale(double xScale, double yScale)
  { for(int i=0; i<length; i++)
    { points[i].X *= xScale;
      points[i].Y *= yScale;
    }
  }
  /// <summary>Returns a copy of this polygon, with the points scaled by the given factors.</summary>
  /// <param name="xScale">The factor by which to multiply the X coordinates.</param>
  /// <param name="yScale">The factor by which to multiply the Y coordinates.</param>
  /// <returns>A copy of this polygon, with the points scaled.</returns>
  public Polygon Scaled(double xScale, double yScale)
  { Polygon poly = new Polygon(points, length);
    poly.Scale(xScale, yScale);
    return poly;
  }
  /// <summary>Splits a non-convex polygon into convex polygons.</summary>
  /// <returns>An array of convex polygons that, together, make up the original polygon.</returns>
  /// <remarks>This method is only valid if the edges of the polygon do not overlap.</remarks>
  public Polygon[] Split()
  { const double epsilon = 1e-20;
  
    Polygon[] test = new Polygon[4], done = new Polygon[4];
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
              {
                if(edge==sei || edge==sei-1 || (sei==0 && edge==poly.Length-1)) continue; // don't try to intersect adjacent edges
                else if(edge == 0 && sei == poly.Length-1) break;
                LineIntersection lint = toExtend.GetIntersectionInfo(poly.GetEdge(sei));
                // we don't want any points that are on the edge being extended (because it wouldn't be an extension)
                // and we want to make sure the other point is actually on the line segment
                if(lint.OnFirst || !lint.OnSecond || !lint.Point.Valid) continue;
                ept = 0;
                d  = lint.Point.DistanceSquaredTo(toExtend.GetPoint(0)); // find the shortest cut
                d2 = lint.Point.DistanceSquaredTo(toExtend.GetPoint(1));
                // if substantially no extension has occurred, don't consider it. we can't compare against zero because
                // sometimes a very, very tiny extension occurs, and due to floating point inaccuracy, the algorithm
                // enters an infinite loop. using an epsilon prevents the infinite loop but increases the false failure
                // rate (when it throws an exception due to not being able to find any splits)
                if(d  < epsilon) d  = double.MaxValue;
                if(d2 < epsilon) d2 = double.MaxValue;
                if(d2<d)   { d=d2; ept=1; } // 'ept' references which point gets moved to do the extension
                if(d<dist) { dist=d; splitEdge=sei; extPoint=ept; splitPoint=lint.Point; }
              }

              if(splitEdge!=-1) // if we can split it with this edge, do it. don't bother trying the other edges
              { poly.InsertPoint(splitPoint, ++splitEdge); // insert the split point
                Polygon new1 = new Polygon(), new2 = new Polygon();
                int extended = poly.Clip(edge+extPoint), other=poly.Clip(extended+(extPoint==0 ? 1 : -1));
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
  /// <summary>Sets the <see cref="Capacity"/> of the polygon to the actual number of points.</summary>
  void TrimExcess() { Capacity = length; }

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
/// <summary>This structure represents a mathematical vector in three-dimensional space.</summary>
[Serializable, StructLayout(LayoutKind.Sequential)]
public struct Vector
{ 
  /// <summary>Initializes this vector from magnitudes along the X, Y, and Z axes.</summary>
  /// <param name="x">The magnitude along the X axis.</param>
  /// <param name="y">The magnitude along the Y axis.</param>
  /// <param name="z">The magnitude along the Z axis.</param>
  public Vector(double x, double y, double z) { X=x; Y=y; Z=z; }
  /// <summary>Initializes this vector from a <see cref="Point"/>.</summary>
  /// <param name="pt">A <see cref="Point"/>. The point's X, Y, and Z coordinates will become the corresponding
  /// X, Y, and Z magnitudes of the vector.
  /// </param>
  public Vector(Point pt) { X=pt.X; Y=pt.Y; Z=pt.Z; }

  /// <include file="../documentation.xml" path="//Mathematics/Vector/Length/*"/>
  public double Length
  { get { return System.Math.Sqrt(X*X+Y*Y+Z*Z); }
    set { Normalize(value); }
  }
  /// <summary>Returns the length of this vector, squared.</summary>
  public double LengthSqr { get { return X*X+Y*Y+Z*Z; } }
  /// <include file="../documentation.xml" path="//Mathematics/Vector/Normal/*"/>
  public Vector Normal { get { return this/Length; } }
  /// <summary>Returns the cross product of this vector with another vector.</summary>
  /// <param name="v">The other operand.</param>
  /// <returns>A <see cref="Vector"/> perpendicular to both this vector and <paramref name="v"/>.</returns>
  public Vector CrossProduct(Vector v) { return new Vector(X*v.Z-Z*v.Y, Z*v.X-X*v.Z, X*v.Y-Y*v.X); }
  /// <include file="../documentation.xml" path="//Mathematics/Vector/DotProduct/*"/>
  public double DotProduct(Vector v) { return X*v.X + Y*v.Y + Z*v.Z; }
  /// <include file="../documentation.xml" path="//Mathematics/Vector/Normalize/*"/>
  public void Normalize() { this /= Length; }
  /// <include file="../documentation.xml" path="//Mathematics/Vector/Normalize2/*"/>
  public void Normalize(double length) { this /= Length/length; }
  /// <summary>Rotates this vector around the X axis.</summary>
  /// <param name="angle">The angle to rotate by, in radians.</param>
  public void RotateX(double angle) { this = RotatedX(angle); } 
  /// <summary>Rotates this vector around the Y axis.</summary>
  /// <param name="angle">The angle to rotate by, in radians.</param>
  public void RotateY(double angle) { this = RotatedY(angle); } 
  /// <summary>Rotates this vector around the Z axis.</summary>
  /// <param name="angle">The angle to rotate by, in radians.</param>
  public void RotateZ(double angle) { this = RotatedZ(angle); } 
  /// <summary>Rotates this vector around an arbitrary axis.</summary>
  /// <param name="vector">The axis to rotate around. This should be a normalized vector.</param>
  /// <param name="angle">The angle to rotate by, in radians.</param>
  public void Rotate(Vector vector, double angle) { this = Rotated(vector, angle); }

  /// <summary>Returns a copy of this vector, rotated around the X axis.</summary>
  /// <param name="angle">The angle to rotate by, in radians.</param>
  /// <returns>A copy of this vector, rotated around the X axis.</returns>
  public Vector RotatedX(double angle)
  { double sin = Math.Sin(angle), cos = Math.Cos(angle);
    return new Vector(X, Y*cos-Z*sin, Y*sin+Z*cos);
  }
  /// <summary>Returns a copy of this vector, rotated around the Y axis.</summary>
  /// <param name="angle">The angle to rotate by, in radians.</param>
  /// <returns>A copy of this vector, rotated around the Y axis.</returns>
  public Vector RotatedY(double angle)
  { double sin = Math.Sin(angle), cos = Math.Cos(angle);
    return new Vector(Z*sin+X*cos, Y, Z*cos-X*sin);
  }
  /// <summary>Returns a copy of this vector, rotated around the Z axis.</summary>
  /// <param name="angle">The angle to rotate by, in radians.</param>
  /// <returns>A copy of this vector, rotated around the Z axis.</returns>
  public Vector RotatedZ(double angle)
  { double sin = Math.Sin(angle), cos = Math.Cos(angle);
    return new Vector(X*cos-Y*sin, X*sin+Y*cos, Z);
  }
  /// <summary>Returns a copy of this vector, rotated around an arbitrary axis.</summary>
  /// <param name="vector">The axis to rotate around. This should be a normalized vector.</param>
  /// <param name="angle">The angle to rotate by, in radians.</param>
  /// <returns>A copy of this vector, rotated around the given axis.</returns>
  public Vector Rotated(Vector vector, double angle)
  { Quaternion a = new Quaternion(vector, angle), b = new Quaternion(this);
    return (a*b*a.Conjugate).V;
  }

  /// <include file="../documentation.xml" path="//Mathematics/Vector/Equals/*"/>
  public override bool Equals(object obj) { return obj is Vector ? (Vector)obj==this : false; }
  /// <include file="../documentation.xml" path="//Mathematics/Vector/Equals3/*"/>
  public bool Equals(Vector vect, double epsilon)
  { return Math.Abs(vect.X-X)<=epsilon && Math.Abs(vect.Y-Y)<=epsilon && Math.Abs(vect.Z-Z)<=epsilon;
  }

  /// <summary>Returns a hash code for this <see cref="Vector"/>.</summary>
  /// <returns>An integer hash code for this <see cref="Vector"/>.</returns>
  public unsafe override int GetHashCode()
  { fixed(double* dp=&X) { int* p=(int*)dp; return *p ^ *(p+1) ^ *(p+2) ^ *(p+3) ^ *(p+4) ^ *(p+5); }
  }
  /// <summary>Converts this <see cref="Vector"/> into an equivalent <see cref="Point"/>.</summary>
  /// <returns>Returns a <see cref="Point"/> with X, Y, and Z coordinates corresponding to the X, Y, and Z magnitudes
  /// of this vector.
  /// </returns>
  public Point ToPoint() { return new Point(X, Y, Z); }
  /// <summary>Converts this vector into a human-readable string.</summary>
  /// <returns>A human-readable string representation of this vector.</returns>
  public override string ToString() { return string.Format("[{0:f2},{1:f2},{2:f2}]", X, Y, Z); }

  public static Vector operator-(Vector v) { return new Vector(-v.X, -v.Y, -v.Z); }
  public static Vector operator+(Vector a, Vector b) { return new Vector(a.X+b.X, a.Y+b.Y, a.Z+b.Z); }
  public static Vector operator-(Vector a, Vector b) { return new Vector(a.X-b.X, a.Y-b.Y, a.Z-b.Z); }
  public static Vector operator*(Vector v, double f) { return new Vector(v.X*f, v.Y*f, v.Z*f); }
  public static Vector operator*(double f, Vector v) { return new Vector(v.X*f, v.Y*f, v.Z*f); }
  public static Vector operator/(Vector v, double f) { return new Vector(v.X/f, v.Y/f, v.Z/f); }
  public static bool   operator==(Vector a, Vector b) { return a.X==b.X && a.Y==b.Y && a.Z==b.Z; }
  public static bool   operator!=(Vector a, Vector b) { return a.X!=b.X || a.Y!=b.Y || a.Z!=b.Z; }

  /// <summary>The magnitude of this vector along the X axis.</summary>
  public double X;
  /// <summary>The magnitude of this vector along the Y axis.</summary>
  public double Y;
  /// <summary>The magnitude of this vector along the Z axis.</summary>
  public double Z;
}
#endregion

#region Point
/// <summary>This structure represents a point in three-dimensional space.</summary>
[Serializable, StructLayout(LayoutKind.Sequential)]
public struct Point
{ 
  /// <summary>Initializes this <see cref="Point"/> from a set of coordinates.</summary>
  /// <param name="x">The point's X coordinate.</param>
  /// <param name="y">The point's Y coordinate.</param>
  /// <param name="z">The point's Z coordinate.</param>
  public Point(double x, double y, double z) { X=x; Y=y; Z=z; }
  /// <include file="../documentation.xml" path="//Mathematics/Point/DistanceTo/*"/>
  public double DistanceTo(Point point)
  { double xd=point.X-X, yd=point.Y-Y, zd=point.Z-Z;
    return Math.Sqrt(xd*xd+yd*yd+zd*zd);
  }
  /// <include file="../documentation.xml" path="//Mathematics/Point/DistanceSquaredTo/*"/>
  public double DistanceCubedTo(Point point)
  { double xd=point.X-X, yd=point.Y-Y, zd=point.Z-Z;
    return xd*xd+yd*yd+zd*zd;
  }
  /// <summary>Offsets this point by a given amount, translating it in space.</summary>
  /// <param name="xd">The value to add to the point's X coordinate.</param>
  /// <param name="yd">The value to add to the point's Y coordinate.</param>
  /// <param name="zd">The value to add to the point's Z coordinate.</param>
  public void Offset(double xd, double yd, double zd) { X+=xd; Y+=yd; Z+=zd; }

  /// <include file="../documentation.xml" path="//Mathematics/Point/Equals/*"/>
  public override bool Equals(object obj) { return obj is Point ? (Point)obj==this : false; }
  /// <include file="../documentation.xml" path="//Mathematics/Point/Equals3/*"/>
  public bool Equals(Point point, double epsilon)
  { return Math.Abs(point.X-X)<=epsilon && Math.Abs(point.Y-Y)<=epsilon && Math.Abs(point.Z-Z)<=epsilon;
  }
  /// <summary>Calculates a hash code for this <see cref="Point"/>.</summary>
  /// <returns>An integer hash code for this <see cref="Point"/>.</returns>
  public unsafe override int GetHashCode()
  { fixed(double* dp=&X) { int* p=(int*)dp; return *p ^ *(p+1) ^ *(p+2) ^ *(p+3) ^ *(p+4) ^ *(p+5); }
  }
  /// <summary>Converts this <see cref="Point"/> into a human-readable string.</summary>
  /// <returns>A human-readable string representation of this <see cref="Point"/>.</returns>
  public override string ToString() { return string.Format("({0:f2},{1:f2},{2:f2})", X, Y, Z); }

  public static Vector operator-(Point lhs, Point rhs)  { return new Vector(lhs.X-rhs.X, lhs.Y-rhs.Y, lhs.Z-rhs.Z); }
  public static Point  operator-(Point lhs, Vector rhs) { return new Point(lhs.X-rhs.X, lhs.Y-rhs.Y, lhs.Z-rhs.Z); }
  public static Point  operator+(Point lhs, Vector rhs) { return new Point(lhs.X+rhs.X, lhs.Y+rhs.Y, lhs.Z+rhs.Z); }
  public static Point  operator+(Vector lhs, Point rhs) { return new Point(lhs.X+rhs.X, lhs.Y+rhs.Y, lhs.Z+rhs.Z); }
  public static bool   operator==(Point lhs, Point rhs) { return lhs.X==rhs.X && lhs.Y==rhs.Y && lhs.Z==rhs.Z; }
  public static bool   operator!=(Point lhs, Point rhs) { return lhs.X!=rhs.X || lhs.Y!=rhs.Y || lhs.Z!=rhs.Z; }

  /// <summary>This point's X coordinate.</summary>
  public double X;
  /// <summary>This point's Y coordinate.</summary>
  public double Y;
  /// <summary>This point's Z coordinate.</summary>
  public double Z;
}
#endregion

#region Line
/// <summary>This structure represents a line.</summary>
[Serializable]
public struct Line
{ 
  /// <summary>Initializes this line from a point's coordinates and a vector's axis magnitudes.</summary>
  /// <param name="x">The X coordinate of a point on the line (or the start of the line segment).</param>
  /// <param name="y">The Y coordinate of a point on the line (or the start of the line segment).</param>
  /// <param name="z">The Z coordinate of a point on the line (or the start of the line segment).</param>
  /// <param name="xd">The magnitude along the X axis of the line's direction. If you're defining a line segment,
  /// this should be the distance from <paramref name="x"/> to the X coordinate of the endpoint.
  /// </param>
  /// <param name="yd">The magnitude along the Y axis of the line's direction. If you're defining a line segment,
  /// this should be the distance from <paramref name="y"/> to the Y coordinate of the endpoint.
  /// </param>
  /// <param name="zd">The magnitude along the Z axis of the line's direction. If you're defining a line segment,
  /// this should be the distance from <paramref name="z"/> to the Z coordinate of the endpoint.
  /// </param>
  public Line(double x, double y, double z, double xd, double yd, double zd)
  { Start=new Point(x, y, z); Vector=new Vector(xd, yd, zd);
  }
  /// <include file="../documentation.xml" path="//Mathematics/Line/Line/*"/>
  public Line(Point start, Vector vector) { Start=start; Vector=vector; }
  /// <include file="../documentation.xml" path="//Mathematics/Line/Line2/*"/>
  public Line(Point start, Point end) { Start=start; Vector=end-start; }
  /// <summary>Returns the endpoint of the line segment.</summary>
  /// <remarks>This is equivalent to <see cref="Start"/> + <see cref="Vector"/>.</remarks>
  public Point End { get { return Start+Vector; } }
  /// <summary>Calculates and returns the line segment's length.</summary>
  /// <remarks>This returns the length of <see cref="Vector"/>.</remarks>
  public double Length { get { return Vector.Length; } }
  /// <summary>Calculates and returns the square of the line segment's length.</summary>
  /// <remarks>This returns the square of the length of <see cref="Vector"/>.</remarks>
  public double LengthSqr { get { return Vector.LengthSqr; } }
  /// <include file="../documentation.xml" path="//Mathematics/Line/GetPoint/*"/>
  public Point GetPoint(int point)
  { if(point<0 || point>1) throw new ArgumentOutOfRangeException("point", point, "must be 0 or 1");
    return point==0 ? Start : End;
  }
  /// <include file="../documentation.xml" path="//Mathematics/Line/Equals/*"/>
  public override bool Equals(object obj) { return obj is Line ? (Line)obj==this : false; }
  /// <include file="../documentation.xml" path="//Mathematics/Line/Equals3/*"/>
  public bool Equals(Line line, double epsilon)
  { return Start.Equals(line.Start, epsilon) && Vector.Equals(line.Vector, epsilon);
  }
  /// <summary>Calculates a hash code for this <see cref="Line"/>.</summary>
  /// <returns>An integer hash code for this <see cref="Line"/>.</returns>
  public override int GetHashCode() { return Start.GetHashCode() ^ Vector.GetHashCode(); }
  /// <summary>Converts this <see cref="Line"/> into a human-readable string.</summary>
  /// <returns>A human-readable string representing this line.</returns>
  public override string ToString() { return string.Format("{0}->{1}", Start, Vector); }
  /// <summary>Creates a <see cref="Line"/> from two points.</summary>
  /// <param name="x1">The X coordinate of the first point (a point on the line, or the start of the line segment).</param>
  /// <param name="y1">The Y coordinate of the first point (a point on the line, or the start of the line segment).</param>
  /// <param name="z1">The Z coordinate of the first point (a point on the line, or the start of the line segment).</param>
  /// <param name="x2">The X coordinate of the second point (another point on the line, or the end of the line
  /// segment).
  /// </param>
  /// <param name="y2">The Y coordinate of the second point (another point on the line, or the end of the line
  /// segment).
  /// </param>
  /// <param name="z2">The Z coordinate of the second point (another point on the line, or the end of the line
  /// segment).
  /// </param>
  /// <returns>A <see cref="Line"/> initialized with those values.</returns>
  /// <remarks>Since the end point will need to be converted into a vector, some miniscule accuracy may be lost.
  /// Most notably, the <see cref="End"/> property may not be exactly equal to <paramref name="end"/>.
  /// </remarks>
  public static Line FromPoints(double x1, double y1, double z1, double x2, double y2, double z2)
  { return new Line(x1, y1, z1, x2-x1, y2-y1, z2-z1);
  }

  public static bool operator==(Line lhs, Line rhs) { return lhs.Start==rhs.Start && lhs.Vector==rhs.Vector; }
  public static bool operator!=(Line lhs, Line rhs) { return lhs.Start!=rhs.Start || lhs.Vector!=rhs.Vector; }

  /// <summary>A point on the line, or the start point of the line segment.</summary>
  public Point  Start;
  /// <summary>The line's direction, or the vector from the start point to the end point of the line segment.</summary>
  public Vector Vector;
}
#endregion

#region Plane
/// <summary>This structure represents a plane.</summary>
[Serializable]
public struct Plane
{
  public override bool Equals(object obj)
  {
    return obj is Plane ? this == (Plane)obj : false;
  }

  public override int GetHashCode()
  {
    return Point.GetHashCode() ^ Normal.GetHashCode();
  }

  /// <summary>A point on the plane.</summary>
  public Point  Point;
  /// <summary>A vector perpendicular to the plane.</summary>
  public Vector Normal;

  public static bool operator==(Plane a, Plane b)
  {
    return a.Point == b.Point && a.Normal == b.Normal;
  }

  public static bool operator!=(Plane a, Plane b)
  {
    return a.Point != b.Point || a.Normal != b.Normal;
  }
}
#endregion

#region Sphere
/// <summary>This structure represents a sphere.</summary>
[Serializable]
public struct Sphere
{ 
  /// <summary>Initializes this sphere from a center point and a radius.</summary>
  /// <param name="centerX">The X coordinate of the sphere's center point.</param>
  /// <param name="centerY">The Y coordinate of the sphere's center point.</param>
  /// <param name="centerZ">The Z coordinate of the sphere's center point.</param>
  /// <param name="radius">The radius of the sphere.</param>
  public Sphere(double centerX, double centerY, double centerZ, double radius)
  { Center=new Point(centerX, centerY, centerZ); Radius=radius;
  }
  /// <summary>Initializes this sphere from a center point and a radius.</summary>
  /// <param name="center">The sphere's center point.</param>
  /// <param name="radius">The radius of the sphere.</param>
  public Sphere(Point center, double radius) { Center=center; Radius=radius; }

  /// <summary>Calculates and returns the area of the sphere.</summary>
  public double Volume { get { return Radius*Radius*Radius*Math.PI*4/3; } }

  /// <summary>Determines whether the given point is contained within the sphere.</summary>
  /// <param name="point">The <see cref="Point"/> to test for containment.</param>
  /// <returns>Returns true if <paramref name="point"/> is contained within this sphere.</returns>
  public bool Contains(Point point) { return (point-Center).LengthSqr < Radius*Radius; }

  public override bool Equals(object obj)
  {
    return obj is Sphere ? this == (Sphere)obj : false;
  }

  public override int GetHashCode()
  {
    return Center.GetHashCode() ^ Radius.GetHashCode();
  }

  /// <summary>The center point of this sphere.</summary>
  public Point Center;
  /// <summary>The radius of this sphere.</summary>
  public double Radius;

  public static bool operator==(Sphere a, Sphere b)
  {
    return a.Radius == b.Radius && a.Center == b.Center;
  }

  public static bool operator!=(Sphere a, Sphere b)
  {
    return a.Radius != b.Radius || a.Center != b.Center;
  }
}
#endregion

#region Quaternion
public struct Quaternion
{ public Quaternion(Vector v) { W=0; V=v; }
  public Quaternion(double w, Vector v) { W=w; V=v; }
  public Quaternion(double w, double x, double y, double z) { W=w; V=new Vector(x, y, z); }
  public Quaternion(Vector axis, double angle)
  { angle *= 0.5;
    W=Math.Cos(angle); V=axis*Math.Sin(angle);
  }

  public Quaternion Conjugate { get { return new Quaternion(W, -V); } }

  public double Length
  { get { return Math.Sqrt(V.X*V.X + V.Y*V.Y + V.Z*V.Z + W*W); }
    set { Normalize(value); }
  }
  
  public double LengthSqr
  { get { return V.X*V.X + V.Y*V.Y + V.Z*V.Z + W*W; }
  }

  public Quaternion Normal { get { return this/Length; } }

  public override bool Equals(object obj) { return obj is Quaternion ? (Quaternion)obj==this : false; }
  public bool Equals(Quaternion q, double epsilon) { return Math.Abs(W-q.W)<=epsilon && V.Equals(q.V, epsilon); }

  public void GetAxisAngle(out Vector axis, out double angle)
  { double scale = V.LengthSqr;
    if(scale==0) { axis=new Vector(0, 0, 1); angle=0; }
    else
    { scale = Math.Sqrt(scale);
      axis  = new Vector(V.X/scale, V.Y/scale, V.Z/scale);
      angle = Math.Acos(W)*2;
    }
  }

  public unsafe override int GetHashCode()
  { fixed(double* dp=&W) { int *p=(int*)dp; return *p ^ *(p+1) ^ V.GetHashCode(); }
  }

  public void Normalize() { this /= Length; }
  public void Normalize(double length) { this /= Length/length; }
  
  public Matrix3 ToMatrix3()
  { double xx=V.X*V.X, xy=V.X*V.Y, xz=V.X*V.Z, xw=V.X*W, yy=V.Y*V.Y, yz=V.Y*V.Z, yw=V.Y*W, zz=V.Z*V.Z, zw=V.Z*W;
    Matrix3 ret = new Matrix3(false);
    ret.M00 = 1 - 2*(yy+zz);  ret.M01 =     2*(xy-zw);  ret.M02 =     2*(xz+yw);
    ret.M10 =     2*(xy+zw);  ret.M11 = 1 - 2*(xx+zz);  ret.M12 =     2*(yz-xw);
    ret.M20 =     2*(xz-yw);  ret.M21 =     2*(yz+xw);  ret.M22 = 1 - 2*(xx+yy);
    return ret;
  }

  public Matrix4 ToMatrix4()
  { double xx=V.X*V.X, xy=V.X*V.Y, xz=V.X*V.Z, xw=V.X*W, yy=V.Y*V.Y, yz=V.Y*V.Z, yw=V.Y*W, zz=V.Z*V.Z, zw=V.Z*W;
    Matrix4 ret = new Matrix4(false);
    ret.M00 = 1 - 2*(yy+zz);  ret.M01 =     2*(xy-zw);  ret.M02 =     2*(xz+yw);
    ret.M10 =     2*(xy+zw);  ret.M11 = 1 - 2*(xx+zz);  ret.M12 =     2*(yz-xw);
    ret.M20 =     2*(xz-yw);  ret.M21 =     2*(yz+xw);  ret.M22 = 1 - 2*(xx+yy);
    ret.M33 = 1;
    return ret;
  }

  public double W;
  public Vector V;

  public static bool operator==(Quaternion a, Quaternion b) { return a.W==b.W && a.V==b.V; }
  public static bool operator!=(Quaternion a, Quaternion b) { return a.W!=b.W || a.V!=b.V; }

  public static Quaternion operator*(Quaternion a, Quaternion b)
  { return new Quaternion(a.W*b.W   - a.V.X*b.V.X - a.V.Y*b.V.Y - a.V.Z*b.V.Z,
                          a.W*b.V.X + a.V.X*b.W   + a.V.Y*b.V.Z - a.V.Z*b.V.Y,
                          a.W*b.V.Y - a.V.X*b.V.Z + a.V.Y*b.W   + a.V.Z*b.V.X,
                          a.W*b.V.Z + a.V.X*b.V.Y - a.V.Y*b.V.X + a.V.Z*b.W);
  }

  public static Quaternion operator*(Quaternion a, double b) { return new Quaternion(a.W*b, a.V*b); }
  public static Quaternion operator/(Quaternion a, double b) { return new Quaternion(a.W/b, a.V/b); }
}
#endregion

#region Matrix3
[StructLayout(LayoutKind.Sequential)]
public sealed class Matrix3
{ public Matrix3() { M00=M11=M22=1; }
  public unsafe Matrix3(double[] data)
  { if(data.Length != Length) throw new ArgumentException("Expected an array of "+Length+" elements.");
    fixed(double* src=data) fixed(double* dest=&M00) for(int i=0; i<Length; i++) dest[i]=src[i];
  }
  public unsafe Matrix3(Matrix3 matrix)
  { fixed(double* src=&matrix.M00) fixed(double* dest=&M00) for(int i=0; i<Length; i++) dest[i]=src[i];
  }
  internal Matrix3(bool dummy) { }
  
  public const int Width=3, Height=3, Length=9;

  public unsafe double this[int index]
  { get
    { if(index<0 || index>=Length) throw new ArgumentOutOfRangeException();
      fixed(double* data=&M00) return data[index];
    }
    set
    { if(index<0 || index>=Length) throw new ArgumentOutOfRangeException();
      fixed(double* data=&M00) data[index]=value;
    }
  }

  public unsafe double this[int i, int j]
  { get
    {
      if(i<0 || i>=Height || j<0 || j>=Width) throw new ArgumentOutOfRangeException();
      fixed(double* data=&M00) return data[i*Height+j];
    }
    set
    {
      if(i<0 || i>=Height || j<0 || j>=Width) throw new ArgumentOutOfRangeException();
      fixed(double* data=&M00) data[i*Height+j]=value;
    }
  }

  public unsafe Matrix3 Transpose
  { get
    { Matrix3 ret = new Matrix3(false);
      fixed(double* src=&M00) fixed(double* dest=&ret.M00)
      { dest[0]=src[0]; dest[1]=src[3]; dest[2]=src[6];
        dest[3]=src[1]; dest[4]=src[4]; dest[5]=src[7];
        dest[6]=src[2]; dest[7]=src[5]; dest[8]=src[8];
      }
      return ret;
    }
  }

  public override bool Equals(object obj)
  { 
    Matrix3 other = obj as Matrix3;
    return other==null ? false : this == other;
  }

  public bool Equals(Matrix3 other)
  {
    return this == other;
  }
  
  public unsafe bool Equals(Matrix3 other, double epsilon)
  { 
    fixed(double* ap=&M00) fixed(double* bp=&other.M00)
      for(int i=0; i<Length; i++) if(Math.Abs(ap[i]-bp[i])>epsilon) return false;
    return true;
  }

  public unsafe override int GetHashCode()
  { int hash = 0;
    fixed(double* dp=&M00) { int* p=(int*)dp; for(int i=0; i<Length*2; i++) hash ^= p[i]; }
    return hash;
  }

  public Vector Multiply(Vector v)
  { return new Vector(M00*v.X + M01*v.Y + M02*v.Z,
                      M10*v.X + M11*v.Y + M12*v.Z,
                      M20*v.X + M21*v.Y + M22*v.Z);
  }

  public void Multiply(IList<Vector> vectors)
  { for(int i=0; i<vectors.Count; i++) vectors[i] = Multiply(vectors[i]);
  }

  public void Scale(double x, double y, double z) { M00*=x; M11*=y; M22*=z; }

  public unsafe double[] ToArray()
  { double[] ret = new double[Length];
    fixed(double* src=&M00) fixed(double* dest=ret) for(int i=0; i<Length; i++) dest[i]=src[i];
    return ret;
  }

  public static unsafe void Add(Matrix3 a, Matrix3 b, Matrix3 dest)
  { fixed(double* ap=&a.M00) fixed(double* bp=&b.M00) fixed(double* dp=&dest.M00)
      for(int i=0; i<Length; i++) dp[i] = ap[i]+bp[i];
  }
  
  public static unsafe void Subtract(Matrix3 a, Matrix3 b, Matrix3 dest)
  { fixed(double* ap=&a.M00) fixed(double* bp=&b.M00) fixed(double* dp=&dest.M00)
      for(int i=0; i<Length; i++) dp[i] = ap[i]-bp[i];
  }

  public static unsafe void Multiply(Matrix3 a, Matrix3 b, Matrix3 dest)
  { fixed(double* ap=&a.M00) fixed(double* bp=&b.M00) fixed(double* dp=&dest.M00)
    { dp[0] = ap[0]*bp[0] + ap[1]*bp[3] + ap[2]*bp[6];
      dp[1] = ap[0]*bp[1] + ap[1]*bp[4] + ap[2]*bp[7];
      dp[2] = ap[0]*bp[2] + ap[1]*bp[5] + ap[2]*bp[8];
      dp[3] = ap[3]*bp[0] + ap[4]*bp[3] + ap[5]*bp[6];
      dp[4] = ap[3]*bp[1] + ap[4]*bp[4] + ap[5]*bp[7];
      dp[5] = ap[3]*bp[2] + ap[4]*bp[5] + ap[5]*bp[8];
      dp[6] = ap[6]*bp[0] + ap[7]*bp[3] + ap[8]*bp[6];
      dp[7] = ap[6]*bp[1] + ap[7]*bp[4] + ap[8]*bp[7];
      dp[8] = ap[6]*bp[2] + ap[7]*bp[5] + ap[8]*bp[8];
    }
  }

  public static Matrix3 Rotation(double x, double y, double z)
  { double a=Math.Cos(x), b=Math.Sin(x), c=Math.Cos(y), d=Math.Sin(y), e=Math.Cos(z), f=Math.Sin(z), ad=a*d, bd=b*d;
    Matrix3 ret = new Matrix3(false);
    ret.M00=c*e;         ret.M01=-(c*f);      ret.M02=d;
    ret.M10=bd*e+a*f;    ret.M11=-(bd*f)+a*e; ret.M12=-(b*c);
    ret.M20=-(ad*e)+b*f; ret.M21=ad*f+b*e;    ret.M22=a*c;
    return ret;
  }

  public static Matrix3 Rotation(double angle, Vector axis)
  { double cos=Math.Cos(angle), sin=Math.Sin(angle);
    Vector axisc1m = axis * (1-cos);
    Matrix3 ret = new Matrix3(false);
	  ret.M00 =          cos  + axis.X*axisc1m.X;
	  ret.M10 =   axis.Z*sin  + axis.Y*axisc1m.X;
	  ret.M20 = -(axis.Y*sin) + axis.Z*axisc1m.X;
	  ret.M01 = -(axis.Z*sin) + axis.X*axisc1m.Y;
	  ret.M11 =          cos  + axis.Y*axisc1m.Y;
	  ret.M21 =   axis.X*sin  + axis.Z*axisc1m.Y;
	  ret.M02 =   axis.Y*sin  + axis.X*axisc1m.Z;
	  ret.M12 = -(axis.X*sin) + axis.Y*axisc1m.Z;
	  ret.M22 =          cos  + axis.Z*axisc1m.Z;
    return ret;
  }

  public static Matrix3 Rotation(Vector start, Vector end)
  { Vector cross = start.CrossProduct(end);
    // if the vectors are colinear, rotate one by 90 degrees and use that.
    if(cross.X==0 && cross.Y==0 && cross.Z==0)
      return start.Equals(end, 0.001) ? new Matrix3() : Rotation(Math.PI, new Vector(-start.Y, start.X, start.Z));
    return Rotation(Math.Acos(start.DotProduct(end)), cross);
  }

  public static Matrix3 RotationX(double angle)
  { double sin=Math.Sin(angle), cos=Math.Cos(angle);
    Matrix3 ret = new Matrix3(false);
    ret.M00=1; ret.M11=cos; ret.M12=-sin; ret.M21=sin; ret.M22=cos;
    return ret;
  }

  public static Matrix3 RotationY(double angle)
  { double sin=Math.Sin(angle), cos=Math.Cos(angle);
    Matrix3 ret = new Matrix3(false);
    ret.M00=cos; ret.M02=sin; ret.M11=1; ret.M20=-sin; ret.M22=cos;
    return ret;
  }

  public static Matrix3 RotationZ(double angle)
  { double sin=Math.Sin(angle), cos=Math.Cos(angle);
    Matrix3 ret = new Matrix3(false);
    ret.M00=cos; ret.M01=-sin; ret.M10=sin; ret.M11=cos; ret.M22=1;
    return ret;
  }

  public static Matrix3 Scaling(double x, double y, double z)
  { Matrix3 ret = new Matrix3(false);
    ret.M00=x; ret.M11=y; ret.M22=z;
    return ret;
  }

  public static Matrix3 Shearing(double xy, double xz, double yx, double yz, double zx, double zy)
  { Matrix3 ret = new Matrix3();
    ret.M01=yx; ret.M02=zx; ret.M10=xy; ret.M12=zy; ret.M20=xz; ret.M21=yz;
    return ret;
  }

  public static unsafe bool operator==(Matrix3 a, Matrix3 b)
  {
    fixed(double* ap=&a.M00) fixed(double* bp=&b.M00) for(int i=0; i<Length; i++) if(ap[i]!=bp[i]) return false;
    return true;
  }

  public static bool operator!=(Matrix3 a, Matrix3 b)
  {
    return !(a == b);
  }

  public static Matrix3 operator+(Matrix3 a, Matrix3 b)
  { Matrix3 ret = new Matrix3(false);
    Add(a, b, ret);
    return ret;
  }

  public static Matrix3 operator-(Matrix3 a, Matrix3 b)
  { Matrix3 ret = new Matrix3(false);
    Subtract(a, b, ret);
    return ret;
  }

  public static Matrix3 operator*(Matrix3 a, Matrix3 b)
  { Matrix3 ret = new Matrix3(false);
    Multiply(a, b, ret);
    return ret;
  }

  public double M00, M01, M02,
                M10, M11, M12,
                M20, M21, M22;
}
#endregion

#region Matrix4
[StructLayout(LayoutKind.Sequential)]
public sealed class Matrix4
{ public Matrix4() { M00=M11=M22=M33=1; }
  public unsafe Matrix4(double[] data)
  { if(data.Length != Length) throw new ArgumentException("Expected an array of "+Length+" elements.");
    fixed(double* src=data) fixed(double* dest=&M00) for(int i=0; i<Length; i++) dest[i]=src[i];
  }
  public unsafe Matrix4(Matrix4 matrix)
  { fixed(double* src=&matrix.M00) fixed(double* dest=&M00) for(int i=0; i<Length; i++) dest[i]=src[i];
  }
  internal Matrix4(bool dummy) { }
  
  public const int Width=4, Height=4, Length=16;

  public unsafe double this[int index]
  { get
    { if(index<0 || index>=Length)
        throw new ArgumentOutOfRangeException("index", index, "must be from 0 to "+(Length-1));
      fixed(double* data=&M00) return data[index];
    }
    set
    { if(index<0 || index>=Length)
        throw new ArgumentOutOfRangeException("index", index, "must be from 0 to "+(Length-1));
      fixed(double* data=&M00) data[index]=value;
    }
  }

  public unsafe double this[int i, int j]
  { get
    { if(i<0 || i>=Height || j<0 || j>=Width)
        throw new ArgumentOutOfRangeException("indices must range from 0 to "+(Width-1));
      fixed(double* data=&M00) return data[i*Height+j];
    }
    set
    { if(i<0 || i>=Height || j<0 || j>=Width)
        throw new ArgumentOutOfRangeException("indices must range from 0 to "+(Width-1));
      fixed(double* data=&M00) data[i*Height+j]=value;
    }
  }

  public unsafe Matrix4 Transpose
  { get
    { Matrix4 ret = new Matrix4(false);
      fixed(double* src=&M00) fixed(double* dest=&ret.M00)
      { dest[0] =src[0];  dest[1] =src[4];  dest[2] =src[8];  dest[3] =dest[12];
        dest[4] =src[1];  dest[5] =src[5];  dest[6] =src[9];  dest[7] =dest[13];
        dest[8] =src[2];  dest[9] =src[6];  dest[10]=src[10]; dest[11]=dest[14];
        dest[12]=src[3];  dest[13]=src[7];  dest[14]=src[11]; dest[15]=dest[15];
      }
      return ret;
    }
  }

  public override bool Equals(object obj)
  { 
    Matrix4 other = obj as Matrix4;
    return other==null ? false : this == other;
  }
  
  public bool Equals(Matrix4 other)
  {
    return this == other;
  }
  
  public unsafe bool Equals(Matrix4 other, double epsilon)
  { 
    fixed(double* ap=&M00) fixed(double* bp=&other.M00)
      for(int i=0; i<Length; i++) if(Math.Abs(ap[i]-bp[i])>epsilon) return false;
    return true;
  }

  public unsafe override int GetHashCode()
  { int hash = 0;
    fixed(double* dp=&M00) { int* p=(int*)dp; for(int i=0; i<Length*2; i++) hash ^= p[i]; }
    return hash;
  }

  public Vector Multiply(Vector v)
  { return new Vector(M00*v.X + M01*v.Y + M02*v.Z + M03,
                      M10*v.X + M11*v.Y + M12*v.Z + M13,
                      M20*v.X + M21*v.Y + M22*v.Z + M23);
  }

  public void Multiply(IList<Vector> vectors)
  { for(int i=0; i<vectors.Count; i++) vectors[i] = Multiply(vectors[i]);
  }

  public void Scale(double x, double y, double z) { M00*=x; M11*=y; M22*=z; }
  public void Translate(double x, double y, double z) { M03+=x; M13+=y; M23+=z; }

  public unsafe double[] ToArray()
  { double[] ret = new double[Length];
    fixed(double* src=&M00) fixed(double* dest=ret) for(int i=0; i<Length; i++) dest[i]=src[i];
    return ret;
  }

  public static unsafe void Add(Matrix4 a, Matrix4 b, Matrix4 dest)
  { fixed(double* ap=&a.M00) fixed(double* bp=&b.M00) fixed(double* dp=&dest.M00)
      for(int i=0; i<Length; i++) dp[i] = ap[i]+bp[i];
  }
  
  public static unsafe void Subtract(Matrix4 a, Matrix4 b, Matrix4 dest)
  { fixed(double* ap=&a.M00) fixed(double* bp=&b.M00) fixed(double* dp=&dest.M00)
      for(int i=0; i<Length; i++) dp[i] = ap[i]-bp[i];
  }

  public static unsafe void Multiply(Matrix4 a, Matrix4 b, Matrix4 dest)
  { fixed(double* ap=&a.M00) fixed(double* bp=&b.M00) fixed(double* dp=&dest.M00)
    { dp[0]  = ap[0]*bp[0]  + ap[1]*bp[4]  + ap[2]*bp[8]   + ap[3]*bp[12];
      dp[1]  = ap[0]*bp[1]  + ap[1]*bp[5]  + ap[2]*bp[9]   + ap[3]*bp[13];
      dp[2]  = ap[0]*bp[2]  + ap[1]*bp[6]  + ap[2]*bp[10]  + ap[3]*bp[14];
      dp[3]  = ap[0]*bp[3]  + ap[1]*bp[7]  + ap[2]*bp[11]  + ap[3]*bp[15];
      dp[4]  = ap[4]*bp[0]  + ap[5]*bp[4]  + ap[6]*bp[8]   + ap[7]*bp[12];
      dp[5]  = ap[4]*bp[1]  + ap[5]*bp[5]  + ap[6]*bp[9]   + ap[7]*bp[13];
      dp[6]  = ap[4]*bp[2]  + ap[5]*bp[6]  + ap[6]*bp[10]  + ap[7]*bp[14];
      dp[7]  = ap[4]*bp[3]  + ap[5]*bp[7]  + ap[6]*bp[11]  + ap[7]*bp[15];
      dp[8]  = ap[8]*bp[0]  + ap[9]*bp[4]  + ap[10]*bp[8]  + ap[11]*bp[12];
      dp[9]  = ap[8]*bp[1]  + ap[9]*bp[5]  + ap[10]*bp[9]  + ap[11]*bp[13];
      dp[10] = ap[8]*bp[2]  + ap[9]*bp[6]  + ap[10]*bp[10] + ap[11]*bp[14];
      dp[11] = ap[8]*bp[3]  + ap[9]*bp[7]  + ap[10]*bp[11] + ap[11]*bp[15];
      dp[12] = ap[12]*bp[0] + ap[13]*bp[4] + ap[14]*bp[8]  + ap[15]*bp[12];
      dp[13] = ap[12]*bp[1] + ap[13]*bp[5] + ap[14]*bp[9]  + ap[15]*bp[13];
      dp[14] = ap[12]*bp[2] + ap[13]*bp[6] + ap[14]*bp[10] + ap[15]*bp[14];
      dp[15] = ap[12]*bp[3] + ap[13]*bp[7] + ap[14]*bp[11] + ap[15]*bp[15];
    }
  }

  public static Matrix4 Rotation(double x, double y, double z)
  { double a=Math.Cos(x), b=Math.Sin(x), c=Math.Cos(y), d=Math.Sin(y), e=Math.Cos(z), f=Math.Sin(z), ad=a*d, bd=b*d;
    Matrix4 ret = new Matrix4(false);
    ret.M00=c*e;         ret.M01=-(c*f);      ret.M02=d;
    ret.M10=bd*e+a*f;    ret.M11=-(bd*f)+a*e; ret.M12=-(b*c);
    ret.M20=-(ad*e)+b*f; ret.M21=ad*f+b*e;    ret.M22=a*c;
    ret.M33=1;
    return ret;
  }

  public static Matrix4 Rotation(double angle, Vector axis)
  { double cos=Math.Cos(angle), sin=Math.Sin(angle);
    Vector axisc1m = axis * (1-cos);
    Matrix4 ret = new Matrix4(false);
	  ret.M00 =          cos  + axis.X*axisc1m.X;
	  ret.M10 =   axis.Z*sin  + axis.Y*axisc1m.X;
	  ret.M20 = -(axis.Y*sin) + axis.Z*axisc1m.X;
	  ret.M01 = -(axis.Z*sin) + axis.X*axisc1m.Y;
	  ret.M11 =          cos  + axis.Y*axisc1m.Y;
	  ret.M21 =   axis.X*sin  + axis.Z*axisc1m.Y;
	  ret.M02 =   axis.Y*sin  + axis.X*axisc1m.Z;
	  ret.M12 = -(axis.X*sin) + axis.Y*axisc1m.Z;
	  ret.M22 =          cos  + axis.Z*axisc1m.Z;
	  ret.M33 = 1;
    return ret;
  }

  public static Matrix4 Rotation(Vector start, Vector end)
  { Vector cross = start.CrossProduct(end);
    // if the vectors are colinear, rotate one by 90 degrees and use that.
    if(cross.X==0 && cross.Y==0 && cross.Z==0)
      return start.Equals(end, 0.001) ? new Matrix4() : Rotation(Math.PI, new Vector(-start.Y, start.X, start.Z));
    return Rotation(Math.Acos(start.DotProduct(end)), cross);
  }

  public static Matrix4 RotationX(double angle)
  { double sin=Math.Sin(angle), cos=Math.Cos(angle);
    Matrix4 ret = new Matrix4(false);
    ret.M00=1; ret.M11=cos; ret.M12=-sin; ret.M21=sin; ret.M22=cos; ret.M33=1;
    return ret;
  }

  public static Matrix4 RotationY(double angle)
  { double sin=Math.Sin(angle), cos=Math.Cos(angle);
    Matrix4 ret = new Matrix4(false);
    ret.M00=cos; ret.M02=sin; ret.M11=1; ret.M20=-sin; ret.M22=cos; ret.M33=1;
    return ret;
  }

  public static Matrix4 RotationZ(double angle)
  { double sin=Math.Sin(angle), cos=Math.Cos(angle);
    Matrix4 ret = new Matrix4(false);
    ret.M00=cos; ret.M01=-sin; ret.M10=sin; ret.M11=cos; ret.M22=1; ret.M33=1;
    return ret;
  }

  public static Matrix4 Scaling(double x, double y, double z)
  { Matrix4 ret = new Matrix4(false);
    ret.M00=x; ret.M11=y; ret.M22=z; ret.M33=1;
    return ret;
  }

  public static Matrix4 Shearing(double xy, double xz, double yx, double yz, double zx, double zy)
  { Matrix4 ret = new Matrix4();
    ret.M01=yx; ret.M02=zx; ret.M10=xy; ret.M12=zy; ret.M20=xz; ret.M21=yz;
    return ret;
  }

  public static Matrix4 Translation(double x, double y, double z)
  { Matrix4 ret = new Matrix4();
    ret.M03=x; ret.M13=y; ret.M23=z;
    return ret;
  }

  public static Matrix4 operator+(Matrix4 a, Matrix4 b)
  { Matrix4 ret = new Matrix4(false);
    Add(a, b, ret);
    return ret;
  }

  public static Matrix4 operator-(Matrix4 a, Matrix4 b)
  { Matrix4 ret = new Matrix4(false);
    Subtract(a, b, ret);
    return ret;
  }

  public static unsafe bool operator==(Matrix4 a, Matrix4 b)
  {
    fixed(double* ap=&a.M00) fixed(double* bp=&b.M00) for(int i=0; i<Length; i++) if(ap[i]!=bp[i]) return false;
    return true;
  }

  public static bool operator!=(Matrix4 a, Matrix4 b)
  {
    return !(a == b);
  }

  public static Matrix4 operator*(Matrix4 a, Matrix4 b)
  { Matrix4 ret = new Matrix4(false);
    Multiply(a, b, ret);
    return ret;
  }

  public double M00, M01, M02, M03,
                M10, M11, M12, M13,
                M20, M21, M22, M23,
                M30, M31, M32, M33;
}
#endregion

} // namespace ThreeD
#endregion

} // namespace GameLib.Mathematics
