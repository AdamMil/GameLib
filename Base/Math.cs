/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2009 Adam Milazzo

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

#region GLMath
/// <summary>This class provides some helpful mathematical functions.</summary>
public static class GLMath
{
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

} // namespace GameLib.Mathematics
