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

public class MathConst
{ MathConst() { }

  public const double PI = 3.1415926535897932384626433832795;
  public const double PI2Div256 = 0.0245436926061702596754894014318711; // 2 * PI / 256
  public const double RadsTo256 = 40.74366543152520595683424342337; // multiply a radian by this to get a number from 0-255
  public const double DegsTo256 = 0.711111111111111111111111111112; // multiply a degree by this to get a number from 0-255
}

public class SineTable
{ SineTable() { }
  static SineTable() { for(int i=0; i<256; i++) Sines[i]=System.Math.Sin(MathConst.PI2Div256*i); }
  
  public static double Sin(int angle) { return Sines[angle&0xFF]; }
  public static double Cos(int angle) { return Sines[(angle+64)&0xFF]; }
  public static readonly double[] Sines = new double[256];
}

// TODO: once generics become available, use them to implement this in a generic fashion
public struct VectorF
{ public VectorF(float x, float y) { X=x; Y=y; Z=0; }
  public VectorF(float x, float y, float z) { X=x; Y=y; Z=z; }
  
  public float   Length    { get { return (float)System.Math.Sqrt(LengthSqr); } }
  public float   LengthSqr { get { return X*X+Y*Y+Z*Z; } }
  public VectorF Normal
  { get
    { float length=Length;
      if(length==0) return new VectorF(1, 1, 1);
      return this/length;
    }
  }
  
  public VectorF CrossProduct(VectorF v) { return new VectorF(X*v.Z-Z*v.Y, Z*v.X-X*v.Z, X*v.Y-Y*v.X); }
  public VectorF Rotated(int xa, int ya, int za) { return RotatedX(xa).RotatedY(ya).RotatedZ(za); }
  public VectorF RotatedX(int a)
  { float sin = (float)SineTable.Sin(a), cos = (float)SineTable.Cos(a);
    return new VectorF(X, Y*cos-Z*sin, Y*sin+Z*cos);
  }
  public VectorF RotatedY(int a)
  { float sin = (float)SineTable.Sin(a), cos = (float)SineTable.Cos(a);
    return new VectorF(Z*sin+X*cos, Y, Z*cos-X*sin);
  }
  public VectorF RotatedZ(int a)
  { float sin = (float)SineTable.Sin(a), cos = (float)SineTable.Cos(a);
    return new VectorF(X*cos-Y*sin, X*sin+Y*cos, Z);
  }
  
  public void Normalize() { Assign(Normal); }
  public void Rotate(int xa, int ya, int za) { Assign(Rotated(xa, ya, za)); }
  public void RotateX(int a) { Assign(RotatedX(a)); } 
  public void RotateY(int a) { Assign(RotatedY(a)); } 
  public void RotateZ(int a) { Assign(RotatedZ(a)); } 
  
  public static VectorF operator-(VectorF v) { return new VectorF(-v.X, -v.Y, -v.Z); }
  public static VectorF operator+(VectorF a, VectorF b) { return new VectorF(a.X+b.X, a.Y+b.Y, a.Z+b.Z); }
  public static VectorF operator-(VectorF a, VectorF b) { return new VectorF(a.X-b.X, a.Y-b.Y, a.Z-b.Z); }
  public static VectorF operator*(VectorF a, VectorF b) { return new VectorF(a.X*b.X, a.Y*b.Y, a.Z*b.Z); }
  public static VectorF operator/(VectorF a, VectorF b) { return new VectorF(a.X/b.X, a.Y/b.Y, a.Z/b.Z); }
  public static VectorF operator*(VectorF v, float f)   { return new VectorF(v.X*f, v.Y*f, v.Z*f); }
  public static VectorF operator/(VectorF v, float f)   { return new VectorF(v.X/f, v.Y/f, v.Z/f); }
  
  public float X, Y, Z;
  
  void Assign(VectorF v) { X=v.X; Y=v.Y; Z=v.Z; }
}

} // namespace GameLib.Mathematics