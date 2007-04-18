/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2007 Adam Milazzo

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
using System.Runtime.InteropServices;

namespace GameLib
{

/// <summary>This class contains all the compile-time configuration necessary to build GameLib.</summary>
public static class Config
{
  /// <summary>The AdamMil.snk public key.</summary>
  internal const string PublicKey = "0024000004800000940000000602000000240000525341310004000001000100b308a842bed4daf1ec3c339d4082be77edd6ea720cdef3710ce9f6a6254a2960bc204598b97d8ad26a17054bca651c15f3a2e488d2111813313c2a7ceca0c05c2de053eed260a8c529b74ff74fc30886dbd0aba8f65780702e2404400c36e5a33f6cda6b24b925cd39acc0aa6c3bdea95cd6b42527064f41e4be491c0c9a01b3";

  /// <summary>The <see cref="DllImportAttribute"/> path to the SDL_image library.</summary>
  public const string SdlImageImportPath  = "SDL_image";
  /// <summary>The <see cref="DllImportAttribute"/> path to the SDL_gfx library.</summary>
  public const string SdlGfxImportPath    = "sdlgfx";
  /// <summary>The <see cref="DllImportAttribute"/> path to the libsndfile library.</summary>
  public const string SndFileImportPath   = "libsndfile-1";

  #if WINDOWS
  /// <summary>The <see cref="DllImportAttribute"/> path to the SDL library.</summary>
  public const string SdlImportPath       = "SDL";
  /// <summary>The <see cref="DllImportAttribute"/> path to the SDL_ttf library.</summary>
  public const string SdlTTFImportPath    = "SDL_ttf";
  /// <summary>The <see cref="DllImportAttribute"/> path to the GameLib Mixer library.</summary>
  public const string GLMixerImportPath   = "GameLib.Mixer.dll";
  /// <summary>The <see cref="DllImportAttribute"/> path to the GameLib Utility library.</summary>
  public const string GLUtilityImportPath = "GameLib.Utility.dll";
  /// <summary>The <see cref="DllImportAttribute"/> path to the GameLib VorbisWrapper library.</summary>
  public const string VorbisImportPath    = "GameLib.VorbisWrapper.dll";
  /// <summary>The <see cref="DllImportAttribute"/> path to the OpenGL library.</summary>
  public const string OpenGLImportPath    = "opengl32";
  /// <summary>The <see cref="DllImportAttribute"/> path to the GLU library.</summary>
  public const string GluImportPath       = "glu32";
  #elif LINUX
  /// <summary>The <see cref="DllImportAttribute"/> path to the SDL library.</summary>
  public const string SdlImportPath       = "libSDL-1.2.so.0";
  /// <summary>The <see cref="DllImportAttribute"/> path to the SDL_ttf library.</summary>
  public const string SdlTTFImportPath    = "libSDL_ttf-2.0.so.0";
  /// <summary>The <see cref="DllImportAttribute"/> path to the GameLib Mixer library.</summary>
  public const string GLMixerImportPath   = "libGameLibMixer.so";
  /// <summary>The <see cref="DllImportAttribute"/> path to the GameLib Utility library.</summary>
  public const string GLUtilityImportPath = "libGameLibUtility.so";
  /// <summary>The <see cref="DllImportAttribute"/> path to the GameLib VorbisWrapper library.</summary>
  public const string VorbisImportPath    = "libGameLibVorbisWrapper.so";
  /// <summary>The <see cref="DllImportAttribute"/> path to the OpenGL library.</summary>
  public const string OpenGLImportPath   = "opengl";
  /// <summary>The <see cref="DllImportAttribute"/> path to the GLU library.</summary>
  public const string GluImportPath      = "glu";
  #endif

  // TODO: mono might emulate winsock's replies, so we need to check these under mono/linux and mono/win32
  #if WINDOWS
  /// <summary>The system constant for the EWOULDBLOCK error.</summary>
  public const int EWOULDBLOCK  = 10035;
  /// <summary>The system constant for the EMSGSIZE error.</summary>
  public const int EMSGSIZE     = 10040;
  #elif LINUX
  /// <summary>The system constant for the EWOULDBLOCK error.</summary>
  public const int EWOULDBLOCK  = 11;
  /// <summary>The system constant for the EMSGSIZE error.</summary>
  public const int EMSGSIZE     = 90;
  #endif

  #if WINDOWS
  public const CallingConvention GluCallbackConvention = CallingConvention.StdCall;
  #elif LINUX
  public const CallingConvention GluCallbackConvention = CallingConvention.Cdecl;
  #endif

  /// <summary>True if compiled for a big-endian system.</summary>
  /// <remarks>This value is controlled by the BIGENDIAN compile time declaration.</remarks>
  #if BIGENDIAN
  public const bool BigEndian = true;
  #else
  public const bool BigEndian = false;
  #endif
}

} // namespace GameLib
