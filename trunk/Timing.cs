using System;
using GameLib.Interop.SDL;

namespace GameLib
{

public class Timing
{ private Timing() { }

  public static uint Ticks { get { return SDL.GetTicks(); } }
}

}