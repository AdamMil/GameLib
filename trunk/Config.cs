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
using System.Runtime.InteropServices;

namespace GameLib
{

/// <summary>This class contains all the compile-time configuration necessary to build GameLib.</summary>
public sealed class Config
{ private Config() { }

  /// <summary>The <see cref="DllImportAttribute"/> path to the SDL library.</summary>
  public const string SDLImportPath       = "SDL";
  /// <summary>The <see cref="DllImportAttribute"/> path to the SDL_image library.</summary>
  public const string SDLImageImportPath  = "SDL_image";
  /// <summary>The <see cref="DllImportAttribute"/> path to the SDL_ttf library.</summary>
  public const string SDLTTFImportPath    = "SDL_ttf";
  /// <summary>The <see cref="DllImportAttribute"/> path to the SDL_gfx library.</summary>
  public const string SDLGFXImportPath    = "sdlgfx";
  /// <summary>The <see cref="DllImportAttribute"/> path to the GameLib Mixer library.</summary>
  public const string GLMixerImportPath   = "Mixer";
  /// <summary>The <see cref="DllImportAttribute"/> path to the libsndfile library.</summary>
  public const string SndFileImportPath   = "libsndfile";
  /// <summary>The <see cref="DllImportAttribute"/> path to the GameLib VorbisWrapper library.</summary>
  public const string VorbisImportPath    = "VorbisWrapper";
  /// <summary>The <see cref="DllImportAttribute"/> path to the GameLib Utility library.</summary>
  public const string GLUtilityImportPath = "Utility";

  #if WIN32
  /// <summary>The <see cref="DllImportAttribute"/> path to the OpenGL library.</summary>
  public const string OpenGLImportPath   = "opengl32";
  /// <summary>The <see cref="DllImportAttribute"/> path to the GLU library.</summary>
  public const string GLUImportPath      = "glu32";
  #elif LINUX
  /// <summary>The <see cref="DllImportAttribute"/> path to the OpenGL library.</summary>
  public const string OpenGLImportPath   = "opengl";
  /// <summary>The <see cref="DllImportAttribute"/> path to the GLU library.</summary>
  public const string GLUImportPath      = "glu";
  #endif

  #if WIN32
  /// <summary>The system constant for the EWOULDBLOCK error.</summary>
  public const int EWOULDBLOCK  = 10035;
  /// <summary>The system constant for the EMSGSIZE error.</summary>
  public const int EMSGSIZE     = 10040;
  #elif LINUX
  /// <summary>The system constant for the EWOULDBLOCK error.</summary>
  public const int EWOULDBLOCK  = 11
  /// <summary>The system constant for the EMSGSIZE error.</summary>
  public const int EMSGSIZE     = 90;
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