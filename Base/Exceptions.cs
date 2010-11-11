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
using System.Runtime.Serialization;

namespace GameLib
{

/// <summary>This is the base class for all exceptions specific to GameLib.</summary>
[Serializable]
public class GameLibException : Exception
{
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public GameLibException(string message) : base(message) { }
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  /// <param name="innerException">The exception that caused the current exception.</param>
  public GameLibException(string message, Exception innerException) : base(message, innerException) { }
  /// <summary>Initializes this exception.</summary>
  public GameLibException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>This exception is thrown when the data given to a method is too large for it to handle.</summary>
[Serializable]
public class DataTooLargeException : GameLibException
{
  /// <summary>Initializes this exception.</summary>
  public DataTooLargeException() : base("The data is too large.") { maxSize=-1; }
  /// <summary>Initializes this exception.</summary>
  /// <param name="maxSize">The maximum size allowed.</param>
  public DataTooLargeException(int maxSize)
    : base(String.Format("The data is too large. Maximum size is {0}", maxSize)) { this.maxSize=maxSize; }
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public DataTooLargeException(string message) : base(message) { maxSize=-1; }
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  /// <param name="maxSize">The maximum size allowed.</param>
  public DataTooLargeException(string message, int maxSize) : base(message) { this.maxSize=maxSize; }
  /// <summary>Initializes this exception.</summary>
  public DataTooLargeException(SerializationInfo info, StreamingContext context) : base(info, context) { }

  /// <summary>Gets the maximum size allowed.</summary>
  /// <value>The maximum size allowed, or -1 if the maximum size was not given.</value>
  public int MaxSize
  {
    get { return maxSize; }
    protected set { maxSize = value; }
  }

  /// <summary>The maximum size allowed.</summary>
  /// <value>The maximum size allowed, or -1 if the maximum size was not given.</value>
  int maxSize;
}

/// <summary>This exception is thrown when an encoder or decoder could not be found for the given data.</summary>
[Serializable]
public class CodecNotFoundException : GameLibException
{
  /// <summary>Initializes this exception.</summary>
  public CodecNotFoundException() : base("The requested codec could not be found") { }
  /// <summary>Initializes this exception.</summary>
  /// <param name="codec">The name of the codec that could not be found.</param>
  public CodecNotFoundException(string codec) : base("The "+codec+" codec could not be found") { }
  /// <summary>Initializes this exception.</summary>
  public CodecNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

} // namespace GameLib