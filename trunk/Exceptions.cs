using System;
using GameLib.Interop.OggVorbis;

namespace GameLib
{

public class GameLibException : ApplicationException
{ public GameLibException(string message) : base(message) { }
}

public class VideoException : GameLibException
{ public VideoException(string message) : base(message) { }
}

public class SurfaceLostException : VideoException
{ public SurfaceLostException(string message) : base(message) { }
}

public class InputException : GameLibException
{ public InputException(string message) : base(message) { }
}

public class AudioException : GameLibException
{ public AudioException(string message) : base(message) { }
}

public class OpenGLException : GameLibException
{ public OpenGLException(string message) : base(message) { }
}

public enum OggError
{ Read=Ogg.OggError.Read, Fault=Ogg.OggError.Fault, NotImpl=Ogg.OggError.NotImpl, Invalid=Ogg.OggError.Invalid,
  NotVorbis=Ogg.OggError.NotVorbis, BadHeader=Ogg.OggError.BadHeader, BadVersion=Ogg.OggError.BadVersion,
  NotAudio =Ogg.OggError.NotAudio,  BadPacket=Ogg.OggError.BadPacket, BadLink   =Ogg.OggError.BadLink,
  NoSeek   =Ogg.OggError.NoSeek
}
public class OggVorbisException : GameLibException
{ public OggVorbisException(OggError code, string message)
    : base(String.Format("{0} (error code {1})", message, (int)code)) { this.code=code; }
  public OggError Code { get { return code; } }
  
  protected OggError code;
}

} // namespace GameLib