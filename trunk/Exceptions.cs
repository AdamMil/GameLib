using System;

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

} // namespace GameLib