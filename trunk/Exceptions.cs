using System;
using GameLib.Interop.OggVorbis;

namespace GameLib
{

public class GameLibException : ApplicationException
{ public GameLibException(string message) : base(message) { }
}

public class DataTooLargeException : NetworkException
{ public DataTooLargeException() : base("The data is too large.") { MaxSize=-1; }
  public DataTooLargeException(int maxSize)
    : base(String.Format("The data is too large. Maximum size is {0}", maxSize)) { MaxSize=maxSize; }
  public DataTooLargeException(string message) : base(message) { MaxSize=-1; }
  public DataTooLargeException(string message, int maxSize) : base(message) { MaxSize=maxSize; }

  public int MaxSize;
}

#region Video
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
#endregion

#region Network
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
#endregion

#region Input
public class InputException : GameLibException
{ public InputException(string message) : base(message) { }
}
#endregion

#region Audio
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
#endregion

} // namespace GameLib