using System;
using System.Runtime.InteropServices;
using GameLib.Interop;

namespace GameLib.Interop.MikMod
{

internal class MM
{ [Flags]
  public enum Flag
  { SixteenBit=0x0001, Stereo=0x0002, SoftwareFX=0x0004, SoftwareMusic=0x0008, HiQuality=0x0010,
    Surround=0x0100, Interpolate=0x0200, ReverseStereo=0x0400
  }
}

} // namespace GameLib.Interop.MikMod