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