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

/// <summary>This delegate is used to provide notification of when a value changes and allow the application to see
/// the old value.
/// </summary>
public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);
/// <summary>This class is used along with <see cref="ValueChangedEventHandler"/> to provide notification of when a
/// value changes and allow the application to see the old value.
/// </summary>
public class ValueChangedEventArgs : EventArgs
{ public ValueChangedEventArgs(object old) { OldValue=old; }
  public object OldValue;
}

/// <summary>This class provides miscellaneous utilities that don't fit anywhere else.</summary>
public sealed class Utility
{ private Utility() { }

  /// <summary>This field provides a global random number generator.</summary>
  public static readonly Random Random = new Random();

  /// <summary>This method converts a directory path into a consistent format.</summary>
  /// <param name="path">A path to a directory.</param>
  /// <returns>A path to the directory using '/' as the directory separation character and ending with a trailing
  /// slash. For instance, passing "c:\code\test" returns "c:/code/test/".
  /// </returns>
  public static string NormalizeDir(string path)
  { path = path.Replace('\\', '/');
    if(path[path.Length-1] != '/') path += '/';
    return path;
  }
}

} // namespace GameLib