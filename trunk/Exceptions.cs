/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2006 Adam Milazzo

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
using GameLib.Interop.OggVorbis;

namespace GameLib
{

/// <summary>This is the base class for all exceptions specific to GameLib.</summary>
public class GameLibException : ApplicationException
{ 
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public GameLibException(string message) : base(message) { }
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  /// <param name="innerException">The exception that caused the current exception.</param>
  public GameLibException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>This exception is thrown when the data given to a method is too large for it to handle.</summary>
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

  /// <summary>Gets the maximum size allowed.</summary>
  /// <value>The maximum size allowed, or -1 if the maximum size was not given.</value>
  public int MaxSize { get { return maxSize; } }

  /// <summary>The maximum size allowed.</summary>
  /// <value>The maximum size allowed, or -1 if the maximum size was not given.</value>
  protected int maxSize;
}

/// <summary>This exception is thrown when an encoder or decoder could not be found for the given data.</summary>
public class CodecNotFoundException : GameLibException
{ 
  /// <summary>Initializes this exception.</summary>
  public CodecNotFoundException() : base("The requested codec could not be found") { }
  /// <summary>Initializes this exception.</summary>
  /// <param name="codec">The name of the codec that could not be found.</param>
  public CodecNotFoundException(string codec) : base("The "+codec+" codec could not be found") { }
}

#region Video
namespace Video
{
/// <summary>This is the base class for all video-related exceptions in GameLib.</summary>
public class VideoException : GameLibException
{ 
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public VideoException(string message) : base(message) { }
}

/// <summary>This exception is thrown when the operating system has deallocated the video memory for a
/// <see cref="GameLib.Video.Surface"/>. The surface must be reloaded with image data. This should not happen on
/// modern systems.
/// </summary>
public class SurfaceLostException : VideoException
{ 
  /// <summary>Initializes this exception.</summary>
  public SurfaceLostException() : base("The surface has been lost.") { }
}

/// <summary>This is the base class of all OpenGL-related exceptions in GameLib.</summary>
public class OpenGLException : VideoException
{ 
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public OpenGLException(string message) : base(message) { }
}

/// <summary>This exception is thrown when OpenGL will not create any more texture handles.</summary>
public class NoMoreTexturesException : OpenGLException
{ 
  /// <summary>Initializes this exception.</summary>
  public NoMoreTexturesException() : base("GL refuses to create more textures!") { }
}

/// <summary>This exception is thrown when OpenGL is will not create a texture because it would not fit in video
/// memory.
/// </summary>
public class OutOfTextureMemoryException : OpenGLException
{ 
  /// <summary>Initializes this exception.</summary>
  public OutOfTextureMemoryException() : base("Out of texture memory.") { }
}

} // namespace Video
#endregion

#region Network
namespace Network
{
/// <summary>This is the base class for all network-related exceptions in GameLib.</summary>
public class NetworkException : GameLibException
{ 
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public NetworkException(string message) : base(message) { }
}

/// <summary>This exception is thrown when a network connection has been lost.</summary>
public class ConnectionLostException : NetworkException
{ 
  /// <summary>Initializes this exception.</summary>
  public ConnectionLostException() : base("The connection has been lost.") { }
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public ConnectionLostException(string message) : base(message) { }
}

/// <summary>This exception is thrown when a remote connection did not use the expected handshake.</summary>
public class HandshakeException : NetworkException
{ 
  /// <summary>Initializes this exception.</summary>
  public HandshakeException() : base("An error occurred during handshaking. Connection aborted") { }
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public HandshakeException(string message) : base(message) { }
}
} // namespace Network
#endregion

#region Input
namespace Input
{
/// <summary>This is the base class for all input-related exceptions in GameLib.</summary>
public class InputException : GameLibException
{ 
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public InputException(string message) : base(message) { }
}
} // namespace Input
#endregion

#region Audio
namespace Audio
{ 

/// <summary>This is the base class for all audio-related exceptions in GameLib.</summary>
public class AudioException : GameLibException
{ 
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public AudioException(string message) : base(message) { }
}

/// <summary>This enum contains values representing various types of Ogg Vorbis decoding errors.</summary>
public enum OggError
{ 

  /// <summary>Read error while fetching compressed data for decode.</summary>
  Read = Ogg.OggError.Read,
  /// <summary>Internal inconsistency in decode state. Continuing is likely not possible.</summary>
  Fault = Ogg.OggError.Fault,
  /// <summary>Feature not implemented.</summary>
  NotImpl = Ogg.OggError.NotImpl,
  /// <summary>Either an invalid argument, or incompletely initialized argument passed to libvorbisfile call.</summary>
  Invalid = Ogg.OggError.Invalid,
  /// <summary>The given file/data was not recognized as Ogg Vorbis data.</summary>
  NotVorbis = Ogg.OggError.NotVorbis,
  /// <summary>The file/data is apparently an Ogg Vorbis stream, but contains a corrupted or undecipherable header.</summary>
  BadHeader = Ogg.OggError.BadHeader,
  /// <summary>The bitstream format revision of the given stream is not supported.</summary>
  BadVersion = Ogg.OggError.BadVersion,
  /// <summary>The stream does not appear to be an audio stream.</summary>
  NotAudio = Ogg.OggError.NotAudio,
  /// <summary>A packet is not decipherable due to garbage or corruption.</summary>
  BadPacket = Ogg.OggError.BadPacket,
  /// <summary>The given link exists in the Vorbis data stream, but is not decipherable due to garbage or corruption.
  /// </summary>
  BadLink = Ogg.OggError.BadLink,
  /// <summary>The given stream is not seekable.</summary>
  NoSeek = Ogg.OggError.NoSeek
}

/// <summary>This exception is thrown when the ogg vorbis decoder has a problem decoding a stream.</summary>
public class OggVorbisException : AudioException
{ 
  /// <summary>Initializes this exception.</summary>
  /// <param name="code">The reason the decoder could not properly decode the stream.</param>
  /// <param name="message">The message associated with this exception.</param>
  public OggVorbisException(OggError code, string message)
    : base(String.Format("{2} (error code {0} [{1}])", code, (int)code, message)) { this.code=code; }

  /// <summary>Gets the reason the decoder could not properly decode the stream.</summary>
  /// <value>An <see cref="OggError"/> value that represents the reason the decoder could not properly decode the
  /// stream.
  /// </value>
  public OggError Code { get { return code; } }

  /// <summary>The reason the decoder could not properly decode the stream.</summary>
  protected OggError code;
}

} // namespace GameLib.Audio
#endregion

} // namespace GameLib