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

namespace GameLib
{

public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);
public class ValueChangedEventArgs : EventArgs
{ public ValueChangedEventArgs(object old) { OldValue=old; }
  public object OldValue;
}

public struct Pair // TODO: look through code and find places this could be used
{ public Pair(object a, object b) { First=a; Second=b; }

  public bool IsType(Type type) { return IsType(type, false); }
  public bool IsType(Type type, bool allowNull)
  { return allowNull ? (First==null || First.GetType()==type) && (Second==null || Second.GetType()==type) :
                       First!=null && First.GetType()==type && Second!=null && Second.GetType()==type;
  }
  public void CheckType(Type type) { CheckType(type, false); }
  public void CheckType(Type type, bool allowNull)
  { if(!IsType(type, allowNull))
      throw new ArgumentException(String.Format("Expected a pair of {0} (nulls {1}allowed)", type,
                                                allowNull ? "" : "not "));
  }

  public object First, Second;
}

internal class Global
{ private Global() { }
  public static Random Rand = new Random();
}

} // namespace GameLib