using System;
using System.IO;
using System.Runtime.InteropServices;

namespace GameLib.Interop.SDLTTF
{

[System.Security.SuppressUnmanagedCodeSecurity()]
internal class TTF
{
  #region General
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_Init", CallingConvention=CallingConvention.Cdecl)]
	public static extern int Init();
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_Quit", CallingConvention=CallingConvention.Cdecl)]
	public static extern void Quit();

	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_OpenFont", CallingConvention=CallingConvention.Cdecl)]
	public static extern IntPtr OpenFont(string filename, int ptsize);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_OpenFontIndex", CallingConvention=CallingConvention.Cdecl)]
	public static extern IntPtr OpenFontIndex(string filename, int ptsize, int index);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_OpenFontRW", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern IntPtr OpenFontRW(SDL.SDL.RWOps* ops, int freesrc, int ptsize);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_OpenFontIndexRW", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern IntPtr OpenFontIndexRW(SDL.SDL.RWOps* ops, int freesrc, int ptsize, int index);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_CloseFont", CallingConvention=CallingConvention.Cdecl)]
	public static extern void CloseFont(IntPtr font);
	
	public static string GetError() { return SDL.SDL.GetError(); }
  #endregion
  
  #region Font properties
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_SetFontStyle", CallingConvention=CallingConvention.Cdecl)]
	public static extern void SetFontStyle(IntPtr font, int style);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_GetFontStyle", CallingConvention=CallingConvention.Cdecl)]
	public static extern int GetFontStyle(IntPtr font);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_FontHeight", CallingConvention=CallingConvention.Cdecl)]
	public static extern int FontHeight(IntPtr font);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_FontAscent", CallingConvention=CallingConvention.Cdecl)]
	public static extern int FontAscent(IntPtr font);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_FontDescent", CallingConvention=CallingConvention.Cdecl)]
	public static extern int FontDescent(IntPtr font);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_FontLineSkip", CallingConvention=CallingConvention.Cdecl)]
	public static extern int FontLineSkip(IntPtr font);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_GlyphMetrics", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
	public static extern int GlyphMetrics(IntPtr font, char ch, out int minx, out int maxx, out int miny, out int maxy, out int advance);
  [DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_FontFaceIsFixedWidth", CallingConvention=CallingConvention.Cdecl)]
  public static extern int FontFaceIsFixedWidth(IntPtr font);
  [DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_FontFaceFamilyName", CallingConvention=CallingConvention.Cdecl)]
  public static extern string FontFaceFamilyName(IntPtr font);
  [DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_FontFaceStyleName", CallingConvention=CallingConvention.Cdecl)]
  public static extern string FontFaceStyleName(IntPtr font);
	#endregion
	
	#region Rendering
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_SizeUNICODE", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
	public static extern int SizeUNICODE(IntPtr font, string text, out int width, out int height);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_RenderUNICODE_Solid", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern SDL.SDL.Surface* RenderUNICODE_Solid(IntPtr font, string text, SDL.SDL.Color fg);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_RenderGlyph_Solid", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern SDL.SDL.Surface* RenderGlyph_Solid(IntPtr font, char ch, SDL.SDL.Color fg);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_RenderUNICODE_Shaded", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern SDL.SDL.Surface* RenderUNICODE_Shaded(IntPtr font, string text, SDL.SDL.Color fg, SDL.SDL.Color bg);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_RenderGlyph_Shaded", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern SDL.SDL.Surface* RenderGlyph_Shaded(IntPtr font, char ch, SDL.SDL.Color fg, SDL.SDL.Color bg);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_RenderUNICODE_Blended", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern SDL.SDL.Surface* RenderUNICODE_Blended(IntPtr font, string text, SDL.SDL.Color fg);
	[DllImport(Config.SDLTTFImportPath, EntryPoint="TTF_RenderGlyph_Blended", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern SDL.SDL.Surface* RenderGlyph_Blended(IntPtr font, char ch, SDL.SDL.Color fg);
	#endregion
	
	#region Non-TTF helper functions
  public static void Check(int res) { if(res!=0) RaiseError(); }
  public static void RaiseError()
  { SDL.SDL.RaiseError(); // TODO: make use of exception type parameter (see SDL.RaiseError)
  }
  
  public static void Initialize()
  { if(initCount++==0)
    { try { SDL.SDL.Initialize(SDL.SDL.InitFlag.Video); }
      catch(Exception e) { initCount--; throw e; }
      int res = Init();
      if(res!=0)
      { initCount--;
        SDL.SDL.Deinitialize(SDL.SDL.InitFlag.Video);
        RaiseError();
      }
    }
  }

  public static void Deinitialize()
  { if(initCount==0) throw new InvalidOperationException("Deinitialize called too many times!");
    if(--initCount==0)
    { Quit();
      SDL.SDL.Deinitialize(SDL.SDL.InitFlag.Video);
    }
  }
	#endregion

	static uint initCount;
}

} // namespace GameLib.Interop.SDLTTF