using System;

namespace GameLib
{

public class Config
{ private Config() { }

  public const string SDLImportPath = "SDL";
  public const string SDLMixerImportPath = "SDL_mixer";
  public const string SDLImageImportPath = "SDL_image";
  
  #if BIGENDIAN
  public const bool CompiledBigEndian = true;
  #else
  public const bool CompiledBigEndian = false;
  #endif
}

} // namespace GameLib