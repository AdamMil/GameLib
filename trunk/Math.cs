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

public sealed class MathConst
{ private MathConst() { }

  public const double DegreesToRadians = Math.PI/180;
  public const double RadiansToDegrees = 180/Math.PI;
  public const double TwoPI            = Math.PI*2;
}

public sealed class GLMath
{ private GLMath() { }
  public static float AngleBetween(TwoD.Point start, TwoD.Point end) { return (end-start).Angle; }
  public static int FloorDiv(int numerator, int denominator)
  { return (numerator<0 ? (numerator-denominator+1) : numerator) / denominator;
  }
}

// FIXME: for negative numbers, the fractional part is complemented!
// FIXME: ToString("R") does not produce round-trip safe values
// FIXME: Parse() is not as good as it could be!

/*#region Fixed32
// TODO: make sure int*int==long
// TODO: check if int<<n ==long
public struct Fixed32 : IComparable, IConvertible
{ public Fixed32(int value) { val=value; }
  public Fixed32(double value) { val=FromDouble(value); }

  public Fixed32 Abs();
  public Fixed32 Ceiling();
  public Fixed32 Floor();
  public Fixed32 Round();
  public Fixed32 Sqrt();

  public override bool Equals(object obj)
  { if(!(obj is Fixed32)) return false;
    return val == ((Fixed32)obj).val;
  }
  
  public override int GetHashCode() { return (int)val ^ (int)(val>>32); }
  
  public double ToDouble();
  public int ToInt() { return (int)(val.val>>16); }
  public override string ToString();

  public static Fixed32 Abs(Fixed32 val) { return val.Abs(); }
  public static Fixed32 Ceiling(Fixed32 val) { return val.Ceiling(); }
  public static Fixed32 Floor(Fixed32 val) { return val.Floor(); }
  public static Fixed32 Round(Fixed32 val) { return val.Round(); }
  public static Fixed32 Sqrt(Fixed32 val) { return val.Sqrt(); }

  public static Fixed32 Parse(string s);
  
  public static Fixed32 operator-(Fixed32 val) { return new Fixed32(0-val.val); }

  public static Fixed32 operator+(Fixed32 lhs, int rhs) { return new Fixed32(lhs.val+((long)rhs<<16)); }
  public static Fixed32 operator-(Fixed32 lhs, int rhs) { return new Fixed32(lhs.val-((long)rhs<<16)); }
  public static Fixed32 operator*(Fixed32 lhs, int rhs) { return new Fixed32(lhs.val*rhs); }
  public static Fixed32 operator/(Fixed32 lhs, int rhs) { return new Fixed32(lhs.val/rhs); }

  public static Fixed32 operator+(Fixed32 lhs, double rhs) { return new Fixed32(lhs.val+FromDouble(rhs)); }
  public static Fixed32 operator-(Fixed32 lhs, double rhs) { return new Fixed32(lhs.val-FromDouble(rhs)); }
  public static Fixed32 operator*(Fixed32 lhs, double rhs) { return new Fixed32((lhs.val*FromDouble(rhs))>>16); }
  public static Fixed32 operator/(Fixed32 lhs, double rhs) { return new Fixed32(((long)lhs.val<<16)/FromDouble(rhs)); }

  public static Fixed32 operator+(int lhs, Fixed32 rhs) { return new Fixed32(((long)lhs<<16)+rhs.val); }
  public static Fixed32 operator-(int lhs, Fixed32 rhs) { return new Fixed32(((long)lhs<<16)-rhs.val); }
  public static Fixed32 operator*(int lhs, Fixed32 rhs) { return new Fixed32(lhs*rhs.val); }
  public static Fixed32 operator/(int lhs, Fixed32 rhs) { return new Fixed32(((long)lhs<<32) / rhs.val); }

  public static Fixed32 operator+(double lhs, Fixed32 rhs) { return new Fixed32(FromDouble(lhs)+rhs.val); }
  public static Fixed32 operator-(double lhs, Fixed32 rhs) { return new Fixed32(FromDouble(lhs)-rhs.val); }
  public static Fixed32 operator*(double lhs, Fixed32 rhs) { return new Fixed32((FromDouble(lhs)*rhs.val)>>16); }
  public static Fixed32 operator/(double lhs, Fixed32 rhs) { return new Fixed32(((long)FromDouble(lhs)<<16)/rhs.val); }

  public static Fixed32 operator+(Fixed32 lhs, Fixed32 rhs) { return new Fixed32(lhs.val+rhs.val); }
  public static Fixed32 operator-(Fixed32 lhs, Fixed32 rhs) { return new Fixed32(lhs.val-rhs.val); }
  public static Fixed32 operator*(Fixed32 lhs, Fixed32 rhs) { return new Fixed32((lhs.val*rhs.val)>>16); }
  public static Fixed32 operator/(Fixed32 lhs, Fixed32 rhs) { return new Fixed32(((long)lhs.val<<16)/rhs.val); }

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

  public static bool operator<(Fixed32 lhs, double rhs) { return lhs.val<FromDouble(rhs); }
  public static bool operator<=(Fixed32 lhs, double rhs) { return lhs.val<=FromDouble(rhs); }
  public static bool operator>(Fixed32 lhs, double rhs) { return lhs.val>FromDouble(rhs); }
  public static bool operator>=(Fixed32 lhs, double rhs) { return lhs.val>=FromDouble(rhs); }
  public static bool operator==(Fixed32 lhs, double rhs) { return lhs.val==FromDouble(rhs); }
  public static bool operator!=(Fixed32 lhs, double rhs) { return lhs.val!=FromDouble(rhs); }

  public static bool operator<(int lhs, Fixed32 rhs) { return (lhs<<16)<rhs.val; }
  public static bool operator<=(int lhs, Fixed32 rhs) { return (lhs<<16)<=rhs.val; }
  public static bool operator>(int lhs, Fixed32 rhs) { return (lhs<<16)>rhs.val; }
  public static bool operator>=(int lhs, Fixed32 rhs) { return (lhs<<16)>=rhs.val; }
  public static bool operator==(int lhs, Fixed32 rhs) { return (lhs<<16)==rhs.val; }
  public static bool operator!=(int lhs, Fixed32 rhs) { return (lhs<<16)!=rhs.val; }

  public static bool operator<(double lhs, Fixed32 rhs) { return FromDouble(lhs)<rhs.val; }
  public static bool operator<=(double lhs, Fixed32 rhs) { return FromDouble(lhs)<=rhs.val; }
  public static bool operator>(double lhs, Fixed32 rhs) { return FromDouble(lhs)>rhs.val; }
  public static bool operator>=(double lhs, Fixed32 rhs) { return FromDouble(lhs)>=rhs.val; }
  public static bool operator==(double lhs, Fixed32 rhs) { return FromDouble(lhs)==rhs.val; }
  public static bool operator!=(double lhs, Fixed32 rhs) { return FromDouble(lhs)!=rhs.val; }

  public static readonly Fixed32 Epsilon  = new Fixed32(1);
  public static readonly Fixed32 MinValue = new Fixed32((int)0xFFFFFFFF);
  public static readonly Fixed32 MaxValue = new Fixed32(0x7FFFFFFF);
  public static readonly Fixed32 PI;
  public static readonly Fixed32 E;

  static int FromDouble(double value);
  
  int val;

  #region IComparable Members
  public int CompareTo(object obj)
  {
    // TODO:  Add Fixed32.CompareTo implementation
    return 0;
  }
  #endregion

  #region IConvertible Members

  public ulong ToUInt64(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToUInt64 implementation
    return 0;
  }

  public sbyte ToSByte(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToSByte implementation
    return 0;
  }

  public double ToDouble(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToDouble implementation
    return 0;
  }

  public DateTime ToDateTime(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToDateTime implementation
    return new DateTime ();
  }

  public float ToSingle(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToSingle implementation
    return 0;
  }

  public bool ToBoolean(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToBoolean implementation
    return false;
  }

  public int ToInt32(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToInt32 implementation
    return 0;
  }

  public ushort ToUInt16(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToUInt16 implementation
    return 0;
  }

  public short ToInt16(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToInt16 implementation
    return 0;
  }

  public string ToString(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToString implementation
    return null;
  }

  public byte ToByte(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToByte implementation
    return 0;
  }

  public char ToChar(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToChar implementation
    return '\0';
  }

  public long ToInt64(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToInt64 implementation
    return 0;
  }

  public System.TypeCode GetTypeCode()
  {
    // TODO:  Add Fixed64.GetTypeCode implementation
    return new System.TypeCode ();
  }

  public decimal ToDecimal(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToDecimal implementation
    return 0;
  }

  public object ToType(Type conversionType, IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToType implementation
    return null;
  }

  public uint ToUInt32(IFormatProvider provider)
  {
    // TODO:  Add Fixed64.ToUInt32 implementation
    return 0;
  }

  #endregion
}
#endregion*/

#region Fixed64
[Serializable, System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct Fixed64 : IFormattable, IComparable, IConvertible
{ public Fixed64(double value) { val=FromDouble(value); }
  public Fixed64(int value) { val=(long)value<<32; }
  private Fixed64(long value) { val=value; }

  public Fixed64 Abs() { return val<0 ? new Fixed64(-val) : this; }

  public Fixed64 Ceiling()
  { if(val<0)
    { uint fp = (uint)val;
      return new Fixed64(fp==0 ? val : ((val+OneVal)&Trunc));
    }
    else return new Fixed64((val+OneVal) & Trunc);
  }

  public Fixed64 Floor()
  { if(val<0)
    { uint fp = (uint)val;
      return new Fixed64(fp==0 ? val : (val&Trunc));
    }
    else return new Fixed64(val&Trunc);
  }

  public Fixed64 Round() // does banker's rounding
  { uint fp = (uint)val;
    if(fp<0x80000000) return new Fixed64(val&Trunc);
    else if(fp>0x80000000 || (val&OneVal)!=0) return new Fixed64((val+OneVal)&Trunc);
    else return new Fixed64(val&Trunc);
  }

  public Fixed64 Sqrt() { throw new NotImplementedException(); }

  public Fixed64 Truncated()
  { if(val<0)
    { uint fp = (uint)val;
      return new Fixed64((fp==0 ? val : val+0x100000000)&Trunc);
    }
    else return new Fixed64(val&Trunc);
  }

  public override bool Equals(object obj)
  { if(!(obj is Fixed64)) return false;
    return val == ((Fixed64)obj).val;
  }
  
  public override int GetHashCode() { return (int)val ^ (int)(val>>32); }
  
  public double ToDouble()
  { int ret = (int)(val>>32);
    if(ret<0)
    { uint fp = (uint)val;
      return fp==0 ? ret : (ret+1) + fp*-0.00000000023283064365386962890625; // 1 / (1<<32)
    }
    else return ret + (uint)val*0.00000000023283064365386962890625;
  }

  public int ToInt()
  { int ret = (int)(val>>32);
    if(ret<0 && (uint)val!=0) ret++;
    return ret;
  }

  public override string ToString() { return ToString(null, null); }
  public string ToString(string format) { return ToString(format, null); }

  #region Trig functions, etc
  public static Fixed64 Abs(Fixed64 val) { return val.val<0 ? new Fixed64(-val.val) : val; }
  public static Fixed64 Acos(Fixed64 val) { throw new NotImplementedException(); }
  public static Fixed64 Asin(Fixed64 val) { throw new NotImplementedException(); }
  public static Fixed64 Atan(Fixed64 val) { throw new NotImplementedException(); }
  public static Fixed64 Ceiling(Fixed64 val) { return val.Ceiling(); }
  public static Fixed64 Cos(Fixed64 val) { throw new NotImplementedException(); }
  public static Fixed64 Floor(Fixed64 val) { return val.Floor(); }
  public static Fixed64 Round(Fixed64 val) { return val.Round(); }
  public static Fixed64 Sin(Fixed64 val) { throw new NotImplementedException(); }
  public static Fixed64 Sqrt(Fixed64 val) { return val.Sqrt(); }
  public static Fixed64 Tan(Fixed64 val) { throw new NotImplementedException(); }
  public static Fixed64 Truncate(Fixed64 val) { return val.Truncated(); }
  #endregion

  public static Fixed64 Parse(string s)
  { int pos = s.IndexOf('e');
    if(pos==-1)
    { pos = s.IndexOf('.');
      if(pos==-1)
      { pos = s.IndexOf('/');
        if(pos==-1) return new Fixed64(long.Parse(s)<<32);
        else return new Fixed64(((long)int.Parse(s.Substring(0, pos))<<32) + uint.Parse(s.Substring(pos+1)));
      }
      else
      { long val = pos==0 ? 0 : (long.Parse(s.Substring(0, pos))<<32);
        return new Fixed64(val + (uint)(double.Parse(s.Substring(pos))*4294967296.0));
      }
    }
    else return new Fixed64(double.Parse(s));
  }

  #region Math operators
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
  public static Fixed64 operator/(Fixed64 lhs, Fixed64 rhs) // TODO: make sure this works for big endian machines
  { long quot, rem;
    uint fp = (uint)rhs.val;
    int  count;
    if(fp==0) return new Fixed64(lhs.val / rhs.ToInt());

    bool neg=false;
    if(lhs.val<0) { lhs.val=-lhs.val; neg=!neg; }
    if(rhs.val<0) { rhs.val=-rhs.val; neg=!neg; }

    count=0; // reduce a bit, if we can
    { uint op = (uint)lhs.val, mask=1;
      if((fp&mask)==0 && (op&mask)==0)
      { do { mask<<=1; count++; } while((fp&mask)==0 && (op&mask)==0);
        rhs.val>>=count;
      }
    }

    if(rhs.val<0x100000000)
    { lhs.val>>=count;
      Math.DivRem(lhs.val>>32, rhs.val, out rem);
      quot  = Math.DivRem((rem<<32)+(int)lhs.val, rhs.val, out rem)<<32;
      quot += Math.DivRem(rem<<32, rhs.val, out rem);
    }
    else if(rhs.val<0x1000000000000)
    { lhs.val>>=count;
      uint n = (uint)(lhs.val>>32);
      Math.DivRem(n>>16, rhs.val, out rem);
      Math.DivRem((rem<<16)+(n&0xFFFF), rhs.val, out rem);
      n = (uint)lhs.val;
      quot  = Math.DivRem((rem<<16)+(n>>16), rhs.val, out rem)<<48;
      quot += Math.DivRem((rem<<16)+(n&0xFFFF), rhs.val, out rem)<<32;
      quot += Math.DivRem(rem<<16, rhs.val, out rem)<<16;
      quot += Math.DivRem(rem<<16, rhs.val, out rem);
    }
    else
    { if((rhs.val>>32) > lhs.val) return new Fixed64(0);

      Union ls = new Union(lhs.val), rs = new Union(rhs.val);
      Union  t = new Union();
      int  bits = 96-count;
      byte bit;

      rem = quot = 0;
      do
      { rem = (rem<<1) | (byte)((ls.Uint&0x80000000)>>31);
        lhs.val = ls.Long;
        ls.Long <<= 1;
        bits--;
      }
      while(rem<rs.Long);
      
      ls.Long = lhs.val;
      rem >>= 1;
      bits++;
      
      do
      { rem = (rem<<1) | (byte)((ls.Uint&0x80000000)>>31);
        t.Long = rem - rs.Long;
        bit  = (byte)((~t.Uint&0x80000000)>>31);
        quot = (quot<<1) | bit;
        if(bit!=0) rem=t.Long;
        ls.Long <<= 1;
      } while(--bits>0);
    }
    return new Fixed64(neg ? -quot : quot);
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

  public static bool operator<(Fixed64 lhs, double rhs) { return lhs.val<FromDouble(rhs); }
  public static bool operator<=(Fixed64 lhs, double rhs) { return lhs.val<=FromDouble(rhs); }
  public static bool operator>(Fixed64 lhs, double rhs) { return lhs.val>FromDouble(rhs); }
  public static bool operator>=(Fixed64 lhs, double rhs) { return lhs.val>=FromDouble(rhs); }
  public static bool operator==(Fixed64 lhs, double rhs) { return lhs.val==FromDouble(rhs); }
  public static bool operator!=(Fixed64 lhs, double rhs) { return lhs.val!=FromDouble(rhs); }

  public static bool operator<(int lhs, Fixed64 rhs) { return ((long)lhs<<32)<rhs.val; }
  public static bool operator<=(int lhs, Fixed64 rhs) { return ((long)lhs<<32)<=rhs.val; }
  public static bool operator>(int lhs, Fixed64 rhs) { return ((long)lhs<<32)>rhs.val; }
  public static bool operator>=(int lhs, Fixed64 rhs) { return ((long)lhs<<32)>=rhs.val; }
  public static bool operator==(int lhs, Fixed64 rhs) { return ((long)lhs<<32)==rhs.val; }
  public static bool operator!=(int lhs, Fixed64 rhs) { return ((long)lhs<<32)!=rhs.val; }

  public static bool operator<(double lhs, Fixed64 rhs) { return FromDouble(lhs)<rhs.val; }
  public static bool operator<=(double lhs, Fixed64 rhs) { return FromDouble(lhs)<=rhs.val; }
  public static bool operator>(double lhs, Fixed64 rhs) { return FromDouble(lhs)>rhs.val; }
  public static bool operator>=(double lhs, Fixed64 rhs) { return FromDouble(lhs)>=rhs.val; }
  public static bool operator==(double lhs, Fixed64 rhs) { return FromDouble(lhs)==rhs.val; }
  public static bool operator!=(double lhs, Fixed64 rhs) { return FromDouble(lhs)!=rhs.val; }
  #endregion
  
  public static implicit operator Fixed64(int i) { return new Fixed64((long)i<<32); }
  public static implicit operator Fixed64(double d) { return new Fixed64(FromDouble(d)); }

  #region Useful constants
  public static readonly Fixed64 E        = new Fixed64(8589934592L + 3084996963);
  public static readonly Fixed64 Epsilon  = new Fixed64((long)1);
  public static readonly Fixed64 MaxValue = new Fixed64(0x7FFFFFFFFFFFFFFFL);
  public static readonly Fixed64 MinValue = new Fixed64(unchecked((long)0x8000000000000001));
  public static readonly Fixed64 MinusOne = new Fixed64(Trunc);
  public static readonly Fixed64 One      = new Fixed64(OneVal);
  public static readonly Fixed64 PI       = new Fixed64(12884901888L + 608135817);
  public static readonly Fixed64 Zero     = new Fixed64((long)0);
  #endregion

  const long Trunc  = unchecked((long)0xFFFFFFFF00000000);
  const long OneVal = unchecked(0x100000000L);

  static long FromDouble(double value)
  { uint fp = (uint)(Math.IEEERemainder(value, 1)*4294967296.0);
    int whole = value<0 && fp!=0 ? (int)value-1 : (int)value;
    return ((long)whole<<32) + fp;
  }

  long val;

  #region IComparable Members
  public int CompareTo(object obj)
  { if(obj==null) return 1;
    if(obj is Fixed64)
    { Fixed64 o = (Fixed64)obj;
      return val<o.val ? -1 : val>o.val ? 1 : 0;
    }
    throw new ArgumentException("'obj' is not a Fixed64");
  }
  #endregion

  #region IConvertible Members
  // FIXME: these should probably do rounding
  public ulong ToUInt64(IFormatProvider provider)
  { int n = ToInt();
    if(n<0) throw new OverflowException();
    return (ulong)n;
  }

  public sbyte ToSByte(IFormatProvider provider)
  { int n = ToInt();
    if(n<sbyte.MinValue || n>sbyte.MaxValue) throw new OverflowException();
    return (sbyte)n;
  }

  public double ToDouble(IFormatProvider provider) { return ToDouble(); }
  public DateTime ToDateTime(IFormatProvider provider) { throw new InvalidCastException(); }

  public float ToSingle(IFormatProvider provider)
  { double d = ToDouble();
    if(d<float.MinValue || d>float.MaxValue) throw new OverflowException();
    return (float)d;
  }

  public bool ToBoolean(IFormatProvider provider) { return val==0; }

  public int ToInt32(IFormatProvider provider) { return (int)(val>>32); }

  public ushort ToUInt16(IFormatProvider provider)
  { int n = ToInt();
    if(n<ushort.MinValue || n>ushort.MaxValue) throw new OverflowException();
    return (ushort)n;
  }

  public short ToInt16(IFormatProvider provider)
  { int n = ToInt();
    if(n<short.MinValue || n>short.MaxValue) throw new OverflowException();
    return (short)n;
  }

  public string ToString(IFormatProvider provider) { return ToString(null, provider); }

  public byte ToByte(IFormatProvider provider)
  { int n = ToInt();
    if(n<byte.MinValue || n>byte.MaxValue) throw new OverflowException();
    return (byte)n;
  }

  public char ToChar(IFormatProvider provider)
  { int n = ToInt();
    if(n<ushort.MinValue || n>ushort.MaxValue) throw new OverflowException();
    return (char)n;
  }

  public long ToInt64(IFormatProvider provider) { return ToInt(); }

  public System.TypeCode GetTypeCode() { return System.TypeCode.Object; }

  public decimal ToDecimal(IFormatProvider provider) { return new decimal(ToDouble()); }

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
    throw new InvalidCastException();
  }

  public uint ToUInt32(IFormatProvider provider)
  { int n = ToInt();
    if(n<0) throw new OverflowException();
    return (uint)n;
  }
  #endregion

  #region IFormattable Members
  public string ToString(string format, IFormatProvider formatProvider)
  { if(format==null) format="F";
    switch(char.ToUpper(format[0]))
    { case 'F':
        string uf = format.Length==1 ? "F15" : format.ToUpper();
        int whole = (int)(val>>32);
        uint   fp = (uint)val;
        if(fp!=0 && uf[1]!='0')
        { string s = whole<0 ? whole<-1 ? (whole+1).ToString() : "-0" : whole.ToString();
          s += '.';
          return s += ((whole<0 ? (uint)-fp : fp) * 0.00000000023283064365386962890625).ToString(uf).TrimEnd('0').Substring(2);
        }
        else return whole.ToString();
      case 'R': return ((int)(val>>32)).ToString() + '/' + ((uint)val).ToString();
      default: return ToDouble().ToString(format);
    }
  }
  #endregion
  
  
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
}
#endregion

// TODO: once generics become available, implement these in a type-generic fashion
#region 2D math
namespace TwoD
{

#region Vector
public struct Vector
{ public Vector(float x, float y) { X=x; Y=y; }
  public Vector(Point pt) { X=pt.X; Y=pt.Y; }

  public float Angle
  { get
    { float angle = (float)Math.Acos(X/Length);
      if(Y>0) angle = (float)MathConst.TwoPI-angle;
      return angle;
    }
  }

  public Vector CrossVector { get { return new Vector(-Y, X); } }
  public float  Length    { get { return (float)System.Math.Sqrt(X*X+Y*Y); } }
  public float  LengthSqr { get { return X*X+Y*Y; } }
  public Vector Normal { get { return this/Length; } }
  
  public float DotProduct(Vector v) { return X*v.X + Y*v.Y; }
  public void  Normalize() { Assign(Normal); }

  public void Rotate(float angle) { Assign(Rotated(angle)); }
  public Vector Rotated(float angle)
  { float sin = (float)Math.Sin(angle), cos = (float)Math.Cos(angle);
    return new Vector(X*cos-Y*sin, X*sin+Y*cos);
  }
  
  public override bool Equals(object obj) { return obj is Vector ? (Vector)obj==this : false; }
  public bool Equals(Vector vect, float epsilon)
  { return Math.Abs(vect.X-X)<=epsilon && Math.Abs(vect.Y-Y)<=epsilon;
  }
  public override int GetHashCode() { return X.GetHashCode() ^ Y.GetHashCode(); }
  public override string ToString() { return string.Format("[{0:f},{1:f}]", X, Y); }

  public static Vector operator-(Vector v) { return new Vector(-v.X, -v.Y); }
  public static Vector operator+(Vector a, Vector b) { return new Vector(a.X+b.X, a.Y+b.Y); }
  public static Vector operator-(Vector a, Vector b) { return new Vector(a.X-b.X, a.Y-b.Y); }
  public static Vector operator*(Vector a, Vector b) { return new Vector(a.X*b.X, a.Y*b.Y); }
  public static Vector operator/(Vector a, Vector b) { return new Vector(a.X/b.X, a.Y/b.Y); }
  public static Vector operator*(Vector v, float f)  { return new Vector(v.X*f, v.Y*f); }
  public static Vector operator/(Vector v, float f)  { return new Vector(v.X/f, v.Y/f); }

  public static bool operator==(Vector a, Vector b) { return a.X==b.X && a.Y==b.Y; }
  public static bool operator!=(Vector a, Vector b) { return a.X!=b.X || a.Y!=b.Y; }

  public float X, Y;

  void Assign(Vector v) { X=v.X; Y=v.Y; }
}
#endregion

#region Point
public struct Point
{ public Point(float x, float y) { X=x; Y=y; }

  public bool Valid { get { return !float.IsNaN(X); } }

  public float DistanceTo(Point point)
  { float xd=point.X-X, yd=point.Y-Y;
    return (float)Math.Sqrt(xd*xd+yd*yd);
  }
  public float DistanceSquaredTo(Point point)
  { float xd=point.X-X, yd=point.Y-Y;
    return xd*xd+yd*yd;
  }

  public void Offset(float xd, float yd) { X+=xd; Y+=yd; }

  public System.Drawing.Point ToPoint() { return new System.Drawing.Point((int)Math.Round(X), (int)Math.Round(Y)); }

  public override bool Equals(object obj) { return obj is Point ? (Point)obj==this : false; }
  public bool Equals(Point point, float epsilon)
  { return Math.Abs(point.X-X)<=epsilon && Math.Abs(point.Y-Y)<=epsilon;
  }
  public override int GetHashCode() { return X.GetHashCode() ^ Y.GetHashCode(); }
  public override string ToString() { return string.Format("({0:f},{1:f})", X, Y); }

  public static Point Invalid { get { return new Point(float.NaN, float.NaN); } }
  public static Vector operator- (Point lhs, Point rhs)  { return new Vector(lhs.X-rhs.X, lhs.Y-rhs.Y); }
  public static Point  operator- (Point lhs, Vector rhs) { return new Point(lhs.X-rhs.X, lhs.Y-rhs.Y); }
  public static Point  operator+ (Point lhs, Vector rhs) { return new Point(lhs.X+rhs.X, lhs.Y+rhs.Y); }
  public static bool   operator==(Point lhs, Point rhs)  { return lhs.X==rhs.X && lhs.Y==rhs.Y; }
  public static bool   operator!=(Point lhs, Point rhs)  { return lhs.X!=rhs.X || lhs.Y!=rhs.Y; }
  public static implicit operator Point(System.Drawing.Point point) { return new Point(point.X, point.Y); }

  public float X, Y;
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
{ public Line(float x, float y, float xd, float yd) { Start=new Point(x, y); Vector=new Vector(xd, yd); }
  public Line(Point start, Vector vector) { Start=start; Vector=vector; }

  public Point End { get { return Start+Vector; } }
  public float Length { get { return Vector.Length; } }
  public float LengthSqr { get { return Vector.LengthSqr; } }

  public float DistanceTo(Point point) { return Vector.CrossVector.Normal.DotProduct(point-Start); }

  public Point GetPoint(int point)
  { if(point<0 || point>1) throw new ArgumentOutOfRangeException("point", point, "must be 0 or 1");
    return point==0 ? Start : End;
  }

  public LineIntersectInfo GetIntersection(Line line)
  { Point p2 = End, p4 = line.End;
    float d = (p4.Y-line.Start.Y)*(p2.X-Start.X) - (p4.X-line.Start.X)*(p2.Y-Start.Y), ua, ub;
    if(d==0) return new LineIntersectInfo(Point.Invalid, false, false);
    ua = ((p4.X-line.Start.X)*(Start.Y-line.Start.Y) - (p4.Y-line.Start.Y)*(Start.X-line.Start.X)) / d;
    ub = ((p2.X-Start.X)*(Start.Y-line.Start.Y) - (p2.Y-Start.Y)*(Start.X-line.Start.X)) / d;
    return new LineIntersectInfo(new Point(Start.X + Vector.X*ua, Start.Y + Vector.Y*ua), ua>=0&&ua<=1, ub>=0&&ub<=1);
  }

  public Point Intersection(Line segment)
  { Point p2 = End, p4 = segment.End;
    float d = (p4.Y-segment.Start.Y)*(p2.X-Start.X) - (p4.X-segment.Start.X)*(p2.Y-Start.Y), ua, ub;
    if(d==0) return Point.Invalid;
    ua = ((p4.X-segment.Start.X)*(Start.Y-segment.Start.Y) - (p4.Y-segment.Start.Y)*(Start.X-segment.Start.X)) / d;
    if(ua<0 || ua>1) return Point.Invalid;
    ub = ((p2.X-Start.X)*(Start.Y-segment.Start.Y) - (p2.Y-Start.Y)*(Start.X-segment.Start.X)) / d;
    if(ub<0 || ub>1) return Point.Invalid;
    return new Point(Start.X + Vector.X*ua, Start.Y + Vector.Y*ua);
  }
  public bool Intersects(Line segment) { return Intersection(segment).Valid; }

  public Point LineIntersection(Line line)
  { Point p2 = End, p4 = line.End;
    float d = (p4.Y-line.Start.Y)*(p2.X-Start.X) - (p4.X-line.Start.X)*(p2.Y-Start.Y);
    if(d==0) return Point.Invalid;
    d = ((p4.X-line.Start.X)*(Start.Y-line.Start.Y) - (p4.Y-line.Start.Y)*(Start.X-line.Start.X)) / d;
    return new Point(Start.X + Vector.X*d, Start.Y + Vector.Y*d);
  }
  public bool LineIntersects(Line line) { return Intersection(line).Valid; }

  public float WhichSide(Point point) { return Vector.CrossVector.DotProduct(point-Start); }

  public override bool Equals(object obj) { return obj is Line ? (Line)obj==this : false; }
  public bool Equals(Line line, float epsilon)
  { return Start.Equals(line.Start, epsilon) && Vector.Equals(line.Vector, epsilon);
  }
  public override int GetHashCode() { return Start.GetHashCode() ^ Vector.GetHashCode(); }
  public override string ToString() { return string.Format("{0}->{1}", Start, Vector); }

  public static Line FromPoints(Point start, Point end) { return new Line(start, end-start); }
  public static Line FromPoints(float x1, float y1, float x2, float y2) { return new Line(x1, y1, x2-x1, y2-y1); }
  
  public static bool operator==(Line lhs, Line rhs) { return lhs.Start==rhs.Start && lhs.Vector==rhs.Vector; }
  public static bool operator!=(Line lhs, Line rhs) { return lhs.Start!=rhs.Start || lhs.Vector!=rhs.Vector; }

  public Point  Start;
  public Vector Vector;
}
#endregion

#region Circle
public struct Circle
{ public Circle(float centerX, float centerY, float radius) { Center=new Point(centerX, centerY); Radius=radius; }
  public Circle(Point center, float radius) { Center=center; Radius=radius; }

  public float Area { get { return (float)(Radius*Radius*Math.PI); } }

  public bool Contains(Point point) { return (point-Center).Length <= Radius; }

  public Point Center;
  public float Radius;
}
#endregion

#region Corner
public struct Corner
{ public Line Edge0 { get { return new Line(Point+Vector0, -Vector0); } }
  public Line Edge1 { get { return new Line(Point, Vector1); } }

  public float CrossZ
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
  { X=(float)rect.X; Y=(float)rect.Y; Width=(float)rect.Width; Height=(float)rect.Height;
  }
  public Rectangle(System.Drawing.RectangleF rect) { X=rect.X; Y=rect.Y; Width=rect.Width; Height=rect.Height; }
  public Rectangle(float x, float y, float width, float height) { X=x; Y=y; Width=width; Height=height; }
  public Rectangle(Point location, Vector size) { X=location.X; Y=location.Y; Width=size.X; Height=size.Y; }

  public float Bottom { get { return Y+Height; } }
  public Point BottomRight { get { return new Point(X+Width, Y+Height); } }
  public Point Location { get { return new Point(X, Y); } }
  public float Right { get { return X+Width; } }
  public Vector Size { get { return new Vector(Width, Height); } }
  
  public bool Contains(Point point) { return point.Y>=Y && point.Y<Bottom && point.X>=X && point.X<Right; }

  public bool Contains(Rectangle rect) { return Contains(rect.Location) && Contains(rect.BottomRight); }

  public void Intersect(Rectangle rect)
  { float x2=Right, ox2=rect.Right;
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

    float y2=Bottom, oy2=rect.Bottom;
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

  public bool Intersects(Line line)
  { if(Contains(line.Start) || Contains(line.End)) return true;
    LineIntersectInfo info = line.GetIntersection(new Line(X, Y, Width, 0));
    if(info.OnBoth) return true;
    info = line.GetIntersection(new Line(X, Y, 0, Height));
    if(info.OnBoth) return true;
    info = line.GetIntersection(new Line(X, Bottom-float.Epsilon, Width, 0));
    if(info.OnBoth) return true;
    info = line.GetIntersection(new Line(Right-float.Epsilon, Y, 0, Height));
    return info.OnBoth;
  }

  public bool Intersects(Rectangle rect)
  { return Contains(rect.Location) || Contains(rect.BottomRight) || rect.Contains(Location) ||
           rect.Contains(BottomRight);
  }

  public Rectangle Union(Rectangle rect)
  { Rectangle ret = new Rectangle(X, Y, Width, Height);
    ret.Unite(rect);
    return ret;
  }
  
  public void Unite(Rectangle rect)
  { if(X<rect.X) X=rect.X;
    if(Y<rect.Y) Y=rect.Y;
    if(Right>rect.Right)   Width  += rect.Right-Right;
    if(Bottom>rect.Bottom) Height += rect.Bottom-Bottom;
  }

  public void Offset(float x, float y) { X+=x; Y+=y; }
  public void Offset(Vector vect) { X+=vect.X; Y+=vect.Y; }

  public float X, Y, Width, Height;
}
#endregion

#region Polygon
public class Polygon
{ public Polygon() { points=new Point[4]; }
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

  public int AddPoint(float x, float y) { return AddPoint(new Point(x, y)); }
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

  public void Clear() { length=0; }

  public bool ConvexContains(Point point)
  { int  sgn;
    bool pos=false, neg=false;
    for(int i=0; i<length; i++)
    { sgn = Math.Sign(GetEdge(i).WhichSide(point));
      if(sgn==-1) { if(pos) return false; neg=true; }
      else if(sgn==1) { if(neg) return false; pos=true; }
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
           ConvexIntersects(new Line(rect.X, rect.Bottom-float.Epsilon, rect.Width, 0)) ||
           ConvexIntersects(new Line(rect.Right-float.Epsilon, rect.Y, 0, rect.Height));
  }

  public bool ConvexIntersects(Polygon poly)
  { for(int i=0; i<length-1; i++) if(poly.ConvexIntersects(GetEdge(i))) return true;
    return false;
  }

  public float GetArea()
  { float area=0;
    int i;
    for(i=0; i<length-1; i++) area += points[i].X*points[i+1].Y - points[i+1].X*points[i].Y;
    area += points[i].X*points[0].Y - points[0].X*points[i].Y;
    return Math.Abs(area)/2;
  }

  public Rectangle GetBounds()
  { Rectangle ret = new Rectangle(float.MaxValue, float.MaxValue, 0, 0);
    float x2=float.MinValue, y2=float.MinValue;
    for(int i=0; i<points.Length; i++)
    { if(points[i].X<ret.X) ret.X = points[i].X;
      if(points[i].Y<ret.Y) ret.Y = points[i].Y;
      if(points[i].X>x2) x2 = points[i].X;
      if(points[i].Y>y2) y2 = points[i].Y;
    }
    ret.Width  = x2-ret.X;
    ret.Height = y2-ret.Y;
    return ret;
  }

  public Point GetCentroid()
  { float area=0,x=0,y=0,d;
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
  { if(length<3) throw new InvalidOperationException("Not a valid polygon [not enough points]!");
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
  { bool neg=false, pos=false;
    for(int i=0; i<length; i++) 
    { float z = GetCorner(i).CrossZ;
      if(z<0)
      { if(pos) throw new InvalidOperationException("Not a simple, convex polygon!");
        neg=true;
      }
      else if(z>0)
      { if(neg) throw new InvalidOperationException("Not a simple, convex polygon!");
        pos=true;
      }
    }
    return pos;
  }

  public bool IsConvex()
  { bool neg=false, pos=false;
    for(int i=0; i<length; i++) 
    { float z = GetCorner(i).CrossZ;
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

        int sgn = Math.Sign(poly.GetCorner(0).CrossZ);
        for(int ci=1; ci<poly.length; ci++)
        { Corner c = poly.GetCorner(ci);
          // if the sign is different, then the polygon is not convex, and splitting at this corner will result in
          // a simplification
          if(Math.Sign(c.CrossZ) != sgn)
          { float dist = float.MaxValue, d, d2;
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
  { if(points.Length<capacity)
    { Point[] narr = new Point[Math.Max(capacity, points.Length*2)];
      Array.Copy(points, narr, length);
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
{ public Vector(float x, float y, float z) { X=x; Y=y; Z=z; }
  public Vector(Point pt) { X=pt.X; Y=pt.Y; Z=pt.Z; }
  
  public float  Length    { get { return (float)System.Math.Sqrt(X*X+Y*Y+Z*Z); } }
  public float  LengthSqr { get { return X*X+Y*Y+Z*Z; } }
  public Vector Normal { get { return this/Length; } }
  
  public Vector CrossProduct(Vector v) { return new Vector(X*v.Z-Z*v.Y, Z*v.X-X*v.Z, X*v.Y-Y*v.X); }
  public float  DotProduct(Vector v) { return X*v.X + Y*v.Y + Z*v.Z; }
  public void Normalize() { Assign(Normal); }

  public void Rotate(float xangle, float yangle, float zangle) { Assign(Rotated(xangle, yangle, zangle)); }
  public void RotateX(float angle) { Assign(RotatedX(angle)); } 
  public void RotateY(float angle) { Assign(RotatedY(angle)); } 
  public void RotateZ(float angle) { Assign(RotatedZ(angle)); } 
  
  public Vector Rotated(float xangle, float yangle, float zangles)
  { return RotatedX(xangle).RotatedY(xangle).RotatedZ(xangle);
  }
  public Vector RotatedX(float angle)
  { float sin = (float)Math.Sin(angle), cos = (float)Math.Cos(angle);
    return new Vector(X, Y*cos-Z*sin, Y*sin+Z*cos);
  }
  public Vector RotatedY(float angle)
  { float sin = (float)Math.Sin(angle), cos = (float)Math.Cos(angle);
    return new Vector(Z*sin+X*cos, Y, Z*cos-X*sin);
  }
  public Vector RotatedZ(float angle)
  { float sin = (float)Math.Sin(angle), cos = (float)Math.Cos(angle);
    return new Vector(X*cos-Y*sin, X*sin+Y*cos, Z);
  }
  
  public override bool Equals(object obj) { return obj is Vector ? (Vector)obj==this : false; }
  public bool Equals(Vector vect, float epsilon)
  { return Math.Abs(vect.X-X)<=epsilon && Math.Abs(vect.Y-Y)<=epsilon && Math.Abs(vect.Z-Z)<=epsilon;
  }
  public override int GetHashCode() { return (X+Y+Z).GetHashCode(); }
  public override string ToString() { return string.Format("[{0:f},{1:f},{2:f}]", X, Y, Z); }

  public static Vector operator-(Vector v) { return new Vector(-v.X, -v.Y, -v.Z); }
  public static Vector operator+(Vector a, Vector b) { return new Vector(a.X+b.X, a.Y+b.Y, a.Z+b.Z); }
  public static Vector operator-(Vector a, Vector b) { return new Vector(a.X-b.X, a.Y-b.Y, a.Z-b.Z); }
  public static Vector operator*(Vector a, Vector b) { return new Vector(a.X*b.X, a.Y*b.Y, a.Z*b.Z); }
  public static Vector operator/(Vector a, Vector b) { return new Vector(a.X/b.X, a.Y/b.Y, a.Z/b.Z); }
  public static Vector operator*(Vector v, float f)   { return new Vector(v.X*f, v.Y*f, v.Z*f); }
  public static Vector operator/(Vector v, float f)   { return new Vector(v.X/f, v.Y/f, v.Z/f); }
  public static bool   operator==(Vector a, Vector b) { return a.X==b.X && a.Y==b.Y && a.Z==b.Z; }
  public static bool   operator!=(Vector a, Vector b) { return a.X!=b.X || a.Y!=b.Y || a.Z!=b.Z; }
  
  public float X, Y, Z;
  
  void Assign(Vector v) { X=v.X; Y=v.Y; Z=v.Z; }
}
#endregion

#region Point
public struct Point
{ public Point(float x, float y, float z) { X=x; Y=y; Z=z; }

  public float DistanceTo(Point point)
  { float xd=point.X-X, yd=point.Y-Y, zd=point.Z-Z;
    return (float)Math.Sqrt(xd*xd+yd*yd+zd*zd);
  }
  public float DistanceCubedTo(Point point)
  { float xd=point.X-X, yd=point.Y-Y, zd=point.Z-Z;
    return xd*xd+yd*yd+zd*zd;
  }

  public void Offset(float xd, float yd, float zd) { X+=xd; Y+=yd; Z+=zd; }

  public override bool Equals(object obj) { return obj is Point ? (Point)obj==this : false; }
  public bool Equals(Point point, float epsilon)
  { return Math.Abs(point.X-X)<=epsilon && Math.Abs(point.Y-Y)<=epsilon && Math.Abs(point.Z-Z)<=epsilon;
  }
  public override int GetHashCode() { return (X+Y+Z).GetHashCode(); }
  public override string ToString() { return string.Format("({0:f},{1:f},{2:f})", X, Y, Z); }

  public static Vector operator-(Point lhs, Point rhs)  { return new Vector(lhs.X-rhs.X, lhs.Y-rhs.Y, lhs.Z-rhs.Z); }
  public static Point  operator-(Point lhs, Vector rhs) { return new Point(lhs.X-rhs.X, lhs.Y-rhs.Y, lhs.Z-rhs.Z); }
  public static Point  operator+(Point lhs, Vector rhs) { return new Point(lhs.X+rhs.X, lhs.Y+rhs.Y, lhs.Z+rhs.Z); }
  public static bool   operator==(Point lhs, Point rhs) { return lhs.X==rhs.X && lhs.Y==rhs.Y && lhs.Z==rhs.Z; }
  public static bool   operator!=(Point lhs, Point rhs) { return lhs.X!=rhs.X || lhs.Y!=rhs.Y || lhs.Z!=rhs.Z; }
  
  public float X, Y, Z;
}
#endregion

#region Line
public struct Line
{ public Line(float x, float y, float z, float xd, float yd, float zd) { Start=new Point(x, y, z); Vector=new Vector(xd, yd, zd); }
  public Line(Point start, Vector vector) { Start=start; Vector=vector; }

  public Point End { get { return Start+Vector; } }
  public float Length { get { return Vector.Length; } }
  public float LengthSqr { get { return Vector.LengthSqr; } }

  public Point GetPoint(int point)
  { if(point<0 || point>1) throw new ArgumentOutOfRangeException("point", point, "must be 0 or 1");
    return point==0 ? Start : End;
  }

  public override bool Equals(object obj) { return obj is Line ? (Line)obj==this : false; }
  public bool Equals(Line line, float epsilon)
  { return Start.Equals(line.Start, epsilon) && Vector.Equals(line.Vector, epsilon);
  }
  public override int GetHashCode() { return Start.GetHashCode() ^ Vector.GetHashCode(); }
  public override string ToString() { return string.Format("{0}->{1}", Start, Vector); }

  public static Line FromPoints(Point start, Point end) { return new Line(start, end-start); }
  public static Line FromPoints(float x1, float y1, float z1, float x2, float y2, float z2)
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
  public float Radius;
}

} // namespace ThreeD
#endregion

} // namespace GameLib.Mathematics