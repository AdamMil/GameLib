using System;
using System.IO;
using System.Runtime.InteropServices;

namespace GameLib.Interop.SDL
{

[System.Security.SuppressUnmanagedCodeSecurity()]
internal class SDL
{

	[CallConvCdecl] public unsafe delegate int SeekHandler(RWOps* ops, int offset, SeekType type);
	[CallConvCdecl] public unsafe delegate int ReadHandler(RWOps* ops, byte* data, int size, int maxnum);
	[CallConvCdecl] public unsafe delegate int WriteHandler(RWOps* ops, byte* data, int size, int num);
	[CallConvCdecl] public unsafe delegate int CloseHandler(RWOps* ops);

  #region Constants
  public const uint CDFramesPerSecond=75;
  #endregion
  
  #region Enums
  [Flags]
  public enum InitFlag : uint
  { Nothing=0, Timer=0x0001, Audio=0x0010, Video=0x0020, CDRom=0x0100, Joystick=0x0200, Everything=0xFFFF,
    NoParachute=0x100000, EventThread=0x1000000
  }
  [Flags]
	public enum VideoFlag : uint
	{ None = 0,
	  SWSurface   = 0x00000000, HWSurface = 0x00000001, AsyncBlit    = 0x00000004, 
		AnyFormat   = 0x10000000, HWPalette = 0x20000000, DoubleBuffer = 0x40000000,
		FullScreen  = 0x80000000, OpenGL    = 0x00000002, OpenGLBlit   = 0x0000000A,
		Resizable   = 0x00000010, NoFrame   = 0x00000020, RLEAccel     = 0x00004000,
		SrcColorKey = 0x00001000, SrcAlpha  = 0x00010000,
	}
	public enum EventType : byte
	{ None, Active, KeyDown, KeyUp, MouseMove, MouseDown, MouseUp, JoyAxis, JoyBall, JoyHat, JoyDown, JoyUp,
	  Quit, SysWMEvent, VideoResize=16, VideoExpose, UserEvent0=24, NumEvents=32
	}
  [Flags]
	public enum FocusType : byte
	{ MouseFocus=1, InputFocus=2, AppActive=4
	}
  [Flags]
	public enum HatPos : byte
	{ Centered=0, Up=1, Right=2, Down=4, Left=8,
	  UpRight=Up|Right, UpLeft=Up|Left, DownRight=Down|Right, DownLeft=Down|Left
	}
	public enum TrackType : byte
	{ Audio=0, Data=4
	}
	public enum CDStatus : int
	{ TrayEmpty, Stopped, Playing, Paused, Error=-1
	}
	public enum SeekType : int
	{ Absolute, Relative, FromEnd
	}
  #endregion

  #region Structs
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct Rect
	{ public Rect(System.Drawing.Rectangle rect)
		{ X = (short)rect.X; Y = (short)rect.Y; Width = (ushort)rect.Width; Height = (ushort)rect.Height;
		}
		public Rect(int x, int y) { X=(short)x; Y=(short)y; Width=Height=1; }
		public System.Drawing.Rectangle ToRectangle() { return new System.Drawing.Rectangle(X, Y, Width, Height); }
		public short  X, Y;
		public ushort Width, Height;
	}
	[StructLayout(LayoutKind.Explicit, Size=4)]
	public struct Color
	{ [FieldOffset(0)] public byte Red;
	  [FieldOffset(1)] public byte Green;
	  [FieldOffset(2)] public byte Blue;

	  [FieldOffset(0)] public uint Value;
	}
  [StructLayout(LayoutKind.Sequential, Pack=4)]
	public unsafe struct Palette
	{ public int Entries;
	  public Color* Colors;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public unsafe struct PixelFormat
	{ public Palette* Palette;
		public byte BitsPerPixel, BytesPerPixel;
		public byte Rloss, Gloss, Bloss, Aloss;
		public byte Rshift, Gshift, Bshift, Ashift;
		public uint Rmask, Gmask, Bmask, Amask;
		public uint Key;
		public byte Alpha;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public unsafe struct Surface
	{ public  uint Flags;
		public  PixelFormat* Format;
		public  uint   Width, Height;
		public  ushort Pitch;
		public  void*  Pixels;
		private int    Offset;
		private IntPtr HWData;
		public  Rect   ClipRect;
		private uint   Unused;
		public  uint   Locked;
		private IntPtr Map;
		private uint   FormatVersion;
		private int    RefCount;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct KeySym {
		public byte Scan;
		public int  Sym;
		public int  Mod;
		public char Unicode;
	}
	[StructLayout(LayoutKind.Explicit, Pack=4)]
	public struct Event
	{ [FieldOffset(0)] public EventType        Type;
	  [FieldOffset(0)] public ActiveEvent      Active;
	  [FieldOffset(0)] public KeyboardEvent    Keyboard;
	  [FieldOffset(0)] public MouseMoveEvent   MouseMove;
	  [FieldOffset(0)] public MouseButtonEvent MouseButton;
	  [FieldOffset(0)] public JoyAxisEvent     JoyAxis;
	  [FieldOffset(0)] public JoyBallEvent     JoyBall;
	  [FieldOffset(0)] public JoyHatEvent      JoyHat;
	  [FieldOffset(0)] public JoyButtonEvent   JoyButton;
	  [FieldOffset(0)] public ResizeEvent      Resize;
	  [FieldOffset(0)] public ExposeEvent      Expose;
	  [FieldOffset(0)] public QuitEvent        Quit;
	  [FieldOffset(0)] public UserEvent        User;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct ActiveEvent
	{ public EventType Type;
		public bool      Focused;
		public FocusType State;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct KeyboardEvent
	{ public EventType Type;
		public bool   Down;
		public byte   Device, State;
		public KeySym Key;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct MouseMoveEvent
	{ public EventType Type;
		public byte   Device, State;
		public ushort X, Y;
		public short  Xrel, Yrel;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct MouseButtonEvent
	{ public EventType Type;
		public byte   Device, Button, State;
		public ushort X, Y;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct JoyAxisEvent
	{ public EventType Type;
		public byte  Device, Axis;
		public short Value;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct JoyBallEvent
	{ public EventType Type;
		public byte  Device, Ball;
		public short Xrel, Yrel;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct JoyHatEvent
	{ public EventType Type;
		public byte   Device, Hat;
		public HatPos Position;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct JoyButtonEvent
	{ public EventType Type;
		public byte Device, Button;
		public bool Down;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct ResizeEvent
	{ public EventType Type;
		public uint Width, Height;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct ExposeEvent
	{ public EventType Type;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct QuitEvent
	{ public EventType Type;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct UserEvent
	{ public EventType Type;
		public int    Code;
		public IntPtr Data1;
		public IntPtr Data2;
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct CDTrack
	{ public byte      Number;
		public TrackType Type;
		public uint      Length, Offset; // in frames
	}
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct CD
	{ public unsafe CDTrack* GetTrack(uint track)
	  { if(track>=NumTracks) throw new ArgumentOutOfRangeException("track", track, "Must be <NumTracks");
	    fixed(byte* data = Tracks) return (CDTrack*)data+track;
	  }
	  public int      ID;
		public CDStatus Status;
		public uint NumTracks;
		public uint CurTrack;
		public uint CurFrame;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=1200)]
		public byte[] Tracks;
	};
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public unsafe struct RWOps
	{ public void* Seek, Read, Write, Close;
	  uint type;
	  void* p1, p2, p3;
	}
  #endregion
  
  #region General
  [DllImport(Config.SDLImportPath, EntryPoint="SDL_Init", CallingConvention=CallingConvention.Cdecl)]
  public static extern int Init(InitFlag flags);
  [DllImport(Config.SDLImportPath, EntryPoint="SDL_InitSubSystem", CallingConvention=CallingConvention.Cdecl)]
  public static extern int InitSubSystem(InitFlag systems);
  [DllImport(Config.SDLImportPath, EntryPoint="SDL_QuitSubSystem", CallingConvention=CallingConvention.Cdecl)]
  public static extern void QuitSubSystem(InitFlag systems);
  [DllImport(Config.SDLImportPath, EntryPoint="SDL_Quit", CallingConvention=CallingConvention.Cdecl)]
  public static extern void Quit();
  [DllImport(Config.SDLImportPath, EntryPoint="SDL_WasInit", CallingConvention=CallingConvention.Cdecl)]
  public static extern uint WasInit(InitFlag systems);
  [DllImport(Config.SDLImportPath, EntryPoint="SDL_GetError", CallingConvention=CallingConvention.Cdecl)]
  public static extern string GetError();
  [DllImport(Config.SDLImportPath, EntryPoint="SDL_ClearError", CallingConvention=CallingConvention.Cdecl)]
  public static extern void ClearError();
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_GetTicks", CallingConvention=CallingConvention.Cdecl)]
	public static extern uint GetTicks();
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_EnableKeyRepeat", CallingConvention=CallingConvention.Cdecl)]
	public static extern int EnableKeyRepeat(int rate, int delay);

	[DllImport(Config.SDLImportPath, EntryPoint="SDL_RWFromFile", CallingConvention=CallingConvention.Cdecl)]
	public static extern RWOps* RWFromFile(string file, string mode);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_RWFromMem", CallingConvention=CallingConvention.Cdecl)]
	public static extern RWOps* RWFromMem(byte[] mem, int size);
  #endregion

  #region Video
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_VideoModeOK", CallingConvention=CallingConvention.Cdecl)]
	public static extern int VideoModeOK(uint width, uint height, uint depth, uint flags);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_SetVideoMode", CallingConvention=CallingConvention.Cdecl)]
	public static extern Surface* SetVideoMode(uint width, uint height, uint depth, uint flags);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_FreeSurface", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern void FreeSurface(Surface* surface);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_Flip", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int Flip(Surface* screen);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_UpdateRect", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int UpdateRect(Surface* screen, int x, int y, int width, int height);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_UpdateRects", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int UpdateRects(Surface* screen, uint numRects, Rect* rects);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_FillRect", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int FillRect(Surface* surface, ref Rect rect, uint color);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_MapRGBA", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern uint MapRGBA(PixelFormat* format, byte r, byte g, byte b, byte a);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_GetRGBA", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern void GetRGBA(uint pixel, PixelFormat* format, out byte r, out byte g, out byte b, out byte a);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_ShowCursor", CallingConvention=CallingConvention.Cdecl)]
	public static extern int ShowCursor(int toggle);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_WarpMouse", CallingConvention=CallingConvention.Cdecl)]
	public static extern void WarpMouse(ushort x, ushort y);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_CreateRGBSurface", CallingConvention=CallingConvention.Cdecl)]
	public static extern Surface* CreateRGBSurface(uint flags, uint width, uint height, uint depth, uint Rmask, uint Gmask, uint Bmask, uint Amask);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_UpperBlit", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int BlitSurface(Surface* src, Rect* srcrect, Surface* dest, Rect* destrect);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_GetVideoSurface", CallingConvention=CallingConvention.Cdecl)]
	public static extern Surface* GetVideoSurface();
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_SetPalette", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int SetPalette(Surface* surface, uint flags, Color* colors, uint firstColor, uint numColors);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_SetGamma", CallingConvention=CallingConvention.Cdecl)]
	public static extern int SetGamma(float red, float green, float blue);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_GetGammaRamp", CallingConvention=CallingConvention.Cdecl)]
	public static extern int GetGammaRamp(ushort[] red, ushort[] green, ushort[] blue);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_SetGammaRamp", CallingConvention=CallingConvention.Cdecl)]
	public static extern int SetGammaRamp(ushort[] red, ushort[] green, ushort[] blue);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_LockSurface", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int LockSurface(Surface* surface);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_UnlockSurface", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern void UnlockSurface(Surface* surface);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_SetColorKey", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int SetColorKey(Surface* surface, uint flag, uint key);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_SetClipRect", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern void SetClipRect(Surface* surface, ref Rect rect);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_GetClipRect", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern void GetClipRect(Surface* surface, ref Rect rect);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_DisplayFormat", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern Surface* DisplayFormat(Surface* surface);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_DisplayFormatAlpha", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern Surface* DisplayFormatAlpha(Surface* surface);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_ConvertSurface", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern Surface* ConvertSurface(Surface* src, PixelFormat* format, uint flags);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_SetAlpha", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int SetAlpha(Surface* src, uint flag, byte alpha);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_ListModes", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern Rect** ListModes(PixelFormat* format, uint flags);
  #endregion

  #region Audio
  // use Mixer from SDLMixerInterop.cs
  #endregion
  
  #region Events
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_PollEvent", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int PollEvent(Event* evt);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_WaitEvent", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int WaitEvent(Event* evt);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_PushEvent", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int PushEvent(Event* evt);
  #endregion

  #region Window manager
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_WM_SetCaption", CallingConvention=CallingConvention.Cdecl)]
	public static extern void SetCaption(string title, string icon);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_WM_GetCaption", CallingConvention=CallingConvention.Cdecl)]
	public static extern void GetCaption(out string title, out string icon);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_WM_SetIcon", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern void SetIcon(Surface* icon, ref byte mask);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_WM_IconifyWindow", CallingConvention=CallingConvention.Cdecl)]
	public static extern int IconifyWindow();
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_WM_GrabInput", CallingConvention=CallingConvention.Cdecl)]
	public static extern int GrabInput(int grab);
  #endregion
  
  #region Joysticks
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_NumJoysticks", CallingConvention=CallingConvention.Cdecl)]
	public static extern int NumJoysticks();
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_JoystickName", CallingConvention=CallingConvention.Cdecl)]
	public static extern string JoystickName(int index);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_JoystickOpen", CallingConvention=CallingConvention.Cdecl)]
	public static extern IntPtr JoystickOpen(int index);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_JoystickIndex", CallingConvention=CallingConvention.Cdecl)]
	public static extern int JoystickIndex(IntPtr joystick);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_JoystickNumAxes", CallingConvention=CallingConvention.Cdecl)]
	public static extern int JoystickNumAxes(IntPtr joystick);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_JoystickNumBalls", CallingConvention=CallingConvention.Cdecl)]
	public static extern int JoystickNumBalls(IntPtr joystick);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_JoystickNumHats", CallingConvention=CallingConvention.Cdecl)]
	public static extern int JoystickNumHats(IntPtr joystick);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_JoystickNumButtons", CallingConvention=CallingConvention.Cdecl)]
	public static extern int JoystickNumButtons(IntPtr joystick);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_JoystickClose", CallingConvention=CallingConvention.Cdecl)]
	public static extern void JoystickClose(IntPtr joystick);
  #endregion

  #region OpenGL
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_GL_SwapBuffers", CallingConvention=CallingConvention.Cdecl)]
	public static extern void SwapBuffers();
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_GL_SetAttribute", CallingConvention=CallingConvention.Cdecl)]
	public static extern int SetAttribute(int attribute, int value);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_GL_GetAttribute", CallingConvention=CallingConvention.Cdecl)]
	public static extern int GetAttribute(int attribute, out int value);
  #endregion

  #region CDROM
	#if FALSE
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_CDNumDrives", CallingConvention=CallingConvention.Cdecl)]
	public static extern int CDNumDrives();
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_CDName", CallingConvention=CallingConvention.Cdecl)]
	public static extern string CDName(int drive);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_CDOpen", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern CD* CDOpen(int drive);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_CDStatus", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern CDStatus GetCDStatus(CD* cdrom);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_CDPlay", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int CDPlay(CD* cdrom, int start, int length);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_CDPlayTracks", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int CDPlayTracks(CD* cdrom, int startTrack, int startFrame, int numTracks, int numFrames);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_CDPause", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int CDPause(CD* cdrom);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_CDResume", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int CDResume(CD* cdrom);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_CDStop", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int CDStop(CD* cdrom);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_CDEject", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern int CDEject(CD* cdrom);
	[DllImport(Config.SDLImportPath, EntryPoint="SDL_CDClose", CallingConvention=CallingConvention.Cdecl)]
	public unsafe static extern void CDClose(CD* cdrom);
	#endif
  #endregion
  
  #region Non-SDL helper functions
  public static void RaiseError() // TODO: add parameter specifying type of exception to throw (instead of GameLibException)
  { string error = GetError();
    if(error==null) throw new GameLibException("GameLib sez: Something bad happened, but SDL disagrees");
    ClearError();
    if(error.IndexOf("Surface was lost")!=-1) throw new SurfaceLostException(error);
    else throw new GameLibException(error);
  }
  
  public static void Initialize() { if(initCount++==0) Init(InitFlag.Nothing); }
  public static void Deinitialize()
  { if(initCount==0) throw new InvalidOperationException("Deinitialize called too many times!");
    if(--initCount==0) Quit();
  }
  #endregion
  
  static uint initCount;
}

internal class StreamSource
{ public unsafe StreamSource(Stream stream)
  { if(stream==null) throw new ArgumentNullException("stream");
    else if(!stream.CanSeek || !stream.CanRead)
      throw new ArgumentException("Stream must be seekable and readable", "stream");
    this.stream = stream;
    seek  = new SDL.SeekHandler(OnSeek);
    read  = new SDL.ReadHandler(OnRead);
    write = new SDL.WriteHandler(OnWrite);
    close = new SDL.CloseHandler(OnClose);
    ops.Seek  = new DelegateMarshaller(seek).ToPointer();
    ops.Read  = new DelegateMarshaller(read).ToPointer();
    ops.Write = new DelegateMarshaller(write).ToPointer();
    ops.Close = new DelegateMarshaller(close).ToPointer();
  }

	unsafe int OnSeek(SDL.RWOps* ops, int offset, SDL.SeekType type)
	{ long pos=-1;
	  switch(type)
	  { case SDL.SeekType.Absolute: pos = stream.Seek(offset, SeekOrigin.Begin); break;
	    case SDL.SeekType.Relative: pos = stream.Seek(offset, SeekOrigin.Current); break;
	    case SDL.SeekType.FromEnd:  pos = stream.Seek(offset, SeekOrigin.End); break;
	  }
	  return (int)pos;
	}
	
	unsafe int OnRead(SDL.RWOps* ops, byte* data, int size, int maxnum)
	{ if(size<=0 || maxnum<=0) return 0;

	  byte[] buf = new byte[size];
	  int i=0, read;
	  try
	  { for(; i<maxnum; i++)
	    { read = stream.Read(buf, 0, size);
	      if(read!=size) { return i==0 ? -1 : i; }
	      for(int j=0; j<size; j++) *data++=buf[j];
	    }
	    return i;
	  }
	  catch { return i==0 ? -1 : i; }
	}

	unsafe int OnWrite(SDL.RWOps* ops, byte* data, int size, int num)
	{ if(!stream.CanWrite) return -1;
	  if(size<=0 || num<=0) return 0;
	  int total=size*num, len = Math.Min(total, 1024);
	  byte[] buf = new byte[len];
	  try
	  { do
	    { if(total<len) len=total;
	      for(int i=0; i<len; i++) buf[i]=*data++;
	      stream.Write(buf, 0, len);
	      total -= len;
	    } while(total>0);
	    return size;
	  }
	  catch { return -1; }
	}
	
	unsafe int OnClose(SDL.RWOps* ops) { stream=null; return 0; }
	
	internal SDL.RWOps ops;
	Stream stream;
	SDL.SeekHandler  seek;
	SDL.ReadHandler  read;
	SDL.WriteHandler write;
	SDL.CloseHandler close;
}

} // namespace GameLib.InterOp.SDL