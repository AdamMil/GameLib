using System;
using System.Runtime.InteropServices;

namespace GameLib
{

public class Config
{ private Config() { }

  public const string SDLImportPath       = "SDL";
  public const string SDLImageImportPath  = "SDL_image";
  public const string SDLTTFImportPath    = "SDL_ttf";
  public const string SDLGFXImportPath    = "sdlgfx";
  public const string GLMixerImportPath   = "Mixer";
  public const string SndFileImportPath   = "libsndfile";
  public const string VorbisImportPath    = "VorbisWrapper";
  public const string GLUtilityImportPath = "Utility";

  #if WIN32
  public const string OpenGLImportPath   = "opengl32";
  public const string GLUImportPath      = "glu32";
  #elif LINUX
  public const string OpenGLImportPath   = "opengl";
  public const string GLUImportPath      = "glu";
  #endif
  
  #if WIN32
  public const int EWOULDBLOCK  = 10035;
  public const int EMSGSIZE     = 10040;
  #elif LINUX
  public const int EWOULDBLOCK  = 11
  public const int EMSGSIZE     = 90;
  #endif

  #if BIGENDIAN
  public const bool BigEndian = true;
  #else
  public const bool BigEndian = false;
  #endif
}

} // namespace GameLib