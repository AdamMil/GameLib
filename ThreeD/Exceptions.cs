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

namespace GameLib.Video
{

/// <summary>This is the base class of all OpenGL-related exceptions in GameLib.</summary>
[Serializable]
public class OpenGLException : VideoException
{ 
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public OpenGLException(string message) : base(message) { }
  /// <summary>Initializes this exception.</summary>
  public OpenGLException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>This exception is thrown when OpenGL will not create any more texture handles.</summary>
[Serializable]
public class NoMoreTexturesException : OpenGLException
{ 
  /// <summary>Initializes this exception.</summary>
  public NoMoreTexturesException() : base("GL refuses to create more textures! "+ Interop.OpenGL.GL.glGetError()) { }
  /// <summary>Initializes this exception.</summary>
  public NoMoreTexturesException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>This exception is thrown when OpenGL is will not create a texture because it would not fit in video
/// memory.
/// </summary>
[Serializable]
public class OutOfTextureMemoryException : OpenGLException
{ 
  /// <summary>Initializes this exception.</summary>
  public OutOfTextureMemoryException() : base("Out of texture memory.") { }
  /// <summary>Initializes this exception.</summary>
  public OutOfTextureMemoryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

} // namespace GameLib.Video