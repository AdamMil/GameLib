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

namespace GameLib
{

/// <summary>This delegate is used to provide notification of when a value changes and allow the application to see
/// the old value.
/// </summary>
public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);
/// <summary>This class is used along with <see cref="ValueChangedEventHandler"/> to provide notification of when a
/// value changes and allow the application to see the old value.
/// </summary>
public class ValueChangedEventArgs : EventArgs
{ 
  /// <summary>Initializes this class.</summary>
  /// <param name="old">The old value of the property, field, etc. that changed.</param>
  public ValueChangedEventArgs(object old) { OldValue=old; }
  /// <summary>The old value of the property, field, etc. that changed.</summary>
  public object OldValue;
}

/// <summary>This class provides miscellaneous utilities that don't fit anywhere else.</summary>
public static class Utility
{
  /// <summary>Disposes the given object and sets it to null.</summary>
  public static void Dispose<T>(ref T obj) where T : class, IDisposable
  {
    if(obj != null)
    {
      obj.Dispose();
      obj = null;
    }
  }

  /// <summary>Disposes the given object, if it's not null.</summary>
  public static void Dispose(IDisposable obj)
  {
    if(obj != null) obj.Dispose();
  }

  /// <summary>Disposes the given object, if it's disposable and not null.</summary>
  public static void TryDispose(object obj)
  {
    if(obj != null)
    {
      IDisposable disposable = obj as IDisposable;
      if(disposable != null) disposable.Dispose();
    }
  }

  /// <summary>Disposes the given object and sets it to null.</summary>
  public static void TryDispose<T>(ref T obj) where T : class
  {
    if(obj != null)
    {
      IDisposable disposable = obj as IDisposable;
      if(disposable != null) disposable.Dispose();
      obj = null;
    }
  }

  /// <summary>This field provides a global random number generator.</summary>
  public static readonly Random Random = new Random();
}

} // namespace GameLib