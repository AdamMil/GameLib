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
using GameLib.Interop.OggVorbis;

namespace GameLib
{

public class GameLibException : ApplicationException
{ public GameLibException(string message) : base(message) { }
  public GameLibException(string message, Exception innerException) : base(message, innerException) { }
}

public class DataTooLargeException : GameLibException
{ public DataTooLargeException() : base("The data is too large.") { MaxSize=-1; }
  public DataTooLargeException(int maxSize)
    : base(String.Format("The data is too large. Maximum size is {0}", maxSize)) { MaxSize=maxSize; }
  public DataTooLargeException(string message) : base(message) { MaxSize=-1; }
  public DataTooLargeException(string message, int maxSize) : base(message) { MaxSize=maxSize; }

  public int MaxSize;
}

public class CodecNotFoundException : GameLibException
{ public CodecNotFoundException() : base("The requested codec could not be found") { }
  public CodecNotFoundException(string codec) : base("The "+codec+" codec could not be found") { }
}

#region Video
namespace Video
{
public class VideoException : GameLibException
{ public VideoException(string message) : base(message) { }
}

public class SurfaceLostException : VideoException
{ public SurfaceLostException() : base("The surface has been lost.") { }
  public SurfaceLostException(string message) : base(message) { }
}

public class OpenGLException : VideoException
{ public OpenGLException(string message) : base(message) { }
}

public class NoMoreTexturesException : OpenGLException
{ public NoMoreTexturesException() : base("GL refuses to create more textures!") { }
}

public class OutOfTextureMemoryException : OpenGLException
{ public OutOfTextureMemoryException() : base("Out of texture memory.") { }
}

} // namespace Video
#endregion

#region Network
namespace Network
{
public class NetworkException : GameLibException
{ public NetworkException(string message) : base(message) { }
}

public class ConnectionLostException : NetworkException
{ public ConnectionLostException() : base("The connection has been lost.") { }
  public ConnectionLostException(string message) : base(message) { }
}

public class HandshakeException : NetworkException
{ public HandshakeException() : base("An error occurred during handshaking. Connection aborted") { }
  public HandshakeException(string message) : base(message) { }
}
} // namespace Network
#endregion

#region Input
namespace Input
{
public class InputException : GameLibException
{ public InputException(string message) : base(message) { }
}
} // namespace Input
#endregion

#region Audio
namespace Audio
{ 

public class AudioException : GameLibException
{ public AudioException(string message) : base(message) { }
}

public enum OggError
{ Read=Ogg.OggError.Read, Fault=Ogg.OggError.Fault, NotImpl=Ogg.OggError.NotImpl, Invalid=Ogg.OggError.Invalid,
  NotVorbis=Ogg.OggError.NotVorbis, BadHeader=Ogg.OggError.BadHeader, BadVersion=Ogg.OggError.BadVersion,
  NotAudio =Ogg.OggError.NotAudio,  BadPacket=Ogg.OggError.BadPacket, BadLink   =Ogg.OggError.BadLink,
  NoSeek   =Ogg.OggError.NoSeek
}

public class OggVorbisException : AudioException
{ public OggVorbisException(OggError code, string message)
    : base(String.Format("{0} (error code {1})", message, (int)code)) { this.code=code; }
  public OggError Code { get { return code; } }
  
  protected OggError code;
}

} // namespace GameLib.Audio
#endregion

} // namespace GameLib