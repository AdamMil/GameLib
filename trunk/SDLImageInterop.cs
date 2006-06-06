/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2006 Adam Milazzo

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
using GameLib.Interop.SDL;

namespace GameLib.Interop.SDLImage
{

[System.Security.SuppressUnmanagedCodeSecurity()]
internal sealed class Image
{ public class Type
  { public const string BMP="BMP";
    public const string PNM="PNM";
    public const string XPM="XPM";
    public const string XCF="XCF";
    public const string PCX="PCX";
    public const string GIF="GIF";
    public const string JPG="JPG";
    public const string TIF="TIF";
    public const string PNG="PNG";
    public const string LBM="LBM";
    public static readonly string[] Types = new string[]
    { BMP, PNM, XPM, XCF, PCX, GIF, JPG, TIF, PNG, LBM
    };
  }

	[DllImport(Config.SDLImageImportPath, ExactSpelling=true, EntryPoint="IMG_Load", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern SDL.SDL.Surface* Load(string file);
	[DllImport(Config.SDLImageImportPath, ExactSpelling=true, EntryPoint="IMG_Load_RW", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern SDL.SDL.Surface* Load_RW(SDL.SDL.RWOps* ops, int freesrc);
  [DllImport(Config.SDLImageImportPath, ExactSpelling=true, EntryPoint="IMG_LoadTyped_RW", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern SDL.SDL.Surface* LoadTyped_RW(SDL.SDL.RWOps* ops, int freesrc, string type);
}

} // namespace GameLib.Interop.SDLImage
