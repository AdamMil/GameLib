using System;
using System.Runtime.InteropServices;
using GameLib.Interop.SDL;

namespace GameLib.Interop.SDLImage
{

[System.Security.SuppressUnmanagedCodeSecurity()]
internal class Image
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

	[DllImport(Config.SDLImageImportPath, EntryPoint="IMG_Load", CallingConvention=CallingConvention.Cdecl)]
	public static extern SDL.SDL.Surface* Load(string file);
	[DllImport(Config.SDLImageImportPath, EntryPoint="IMG_Load_RW", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern SDL.SDL.Surface* Load_RW(SDL.SDL.RWOps* ops, int freesrc);
  [DllImport(Config.SDLImageImportPath, EntryPoint="IMG_LoadTyped_RW", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern SDL.SDL.Surface* LoadTyped_RW(SDL.SDL.RWOps* ops, int freesrc, string type);
}

} // namespace GameLib.Interop.SDLImage
