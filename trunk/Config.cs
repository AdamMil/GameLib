using System;
using System.Runtime.InteropServices;

namespace GameLib
{

public class Config
{ private Config() { }

  public const string SDLImportPath      = "SDL";
  public const string SDLImageImportPath = "SDL_image";
  public const string SDLTTFImportPath   = "SDL_ttf";
  public const string SDLGFXImportPath   = "sdlgfx";
  public const string GLMixerImportPath  = "Mixer";
  public const string SndFileImportPath  = "libsndfile";
  public const string VorbisImportPath   = "VorbisWrapper";
  #if WIN32
  public const string OpenGLImportPath   = "opengl32";
  public const string GLUImportPath      = "glu32";
  #else
  public const string OpenGLImportPath   = "opengl";
  public const string GLUImportPath      = "glu";
  #endif

  #if BIGENDIAN
  public const bool CompiledBigEndian = true;
  #else
  public const bool CompiledBigEndian = false;
  #endif
}

} // namespace GameLib