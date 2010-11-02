/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2010 Adam Milazzo

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
using System.Diagnostics;

namespace GameLib
{

/// <summary>This class provides methods to accurately measure the passage of time.</summary>
/// <remarks>Time is normally measured from when the timing system was initialized, but <see cref="Reset"/> can
/// be used to reset the base time point. The class provides tick-based timing (<see cref="Frequency"/> and
/// <see cref="Counter"/>) and real-time timing (<see cref="Milliseconds"/> and <see cref="Seconds"/>). The real-time
/// timers are based on the tick-based timers. The tick-based timers provide the maximum resolution and
/// precision.
/// </remarks>
public static class Timing
{
  static Timing()
  {
    timer = new Stopwatch();
    timer.Start();

    internalTimer = new Stopwatch();
    internalTimer.Start();
  }

  /// <summary>Gets the frequency of the counter, in ticks per second.</summary>
  /// <remarks>This property represents how many ticks are added to <see cref="Counter"/> in one second.</remarks>
  public static long Frequency { get { return Stopwatch.Frequency; } }

  /// <summary>Gets the current counter value, in ticks.</summary>
  /// <remarks><see cref="Frequency"/> ticks will be added to the counter each second.</remarks>
  public static long Counter { get { return timer.ElapsedTicks; } }

  /// <summary>Gets the number of milliseconds that have elapsed since the timer started counting.</summary>
  /// <remarks>This property will overflow after about 49 days of timing, though <see cref="Reset"/> can be
  /// used to reset the base time point, extending the time until overflow.
  /// </remarks>
  [CLSCompliant(false)]
  public static uint Milliseconds { get { return (uint)timer.ElapsedMilliseconds; } }

  /// <summary>Gets the number of milliseconds that have elapsed since the timer started counting.</summary>
  public static long LongMilliseconds { get { return timer.ElapsedMilliseconds; } }

  /// <summary>Gets the number of seconds that have elapsed since the timer started counting.</summary>
  /// <remarks>While likely insignificant, this property will lose precision as time goes on due to the nature of
  /// floating point numbers. In applications where this cannot be tolerated, <see cref="Reset"/> can be used to
  /// reset the timer to zero, restoring the precision. Alternately, you might be able to use <see cref="Counter"/>
  /// for your timing, as it always provides the maximum precision possible.
  /// </remarks>
  public static double Seconds { get { return (double)timer.ElapsedTicks / Frequency; } }

  /// <summary>Resets the timer to zero.</summary>
  /// <remarks>After calling this method, the timer will be reset to zero, so <see cref="Counter"/>,
  /// <see cref="Milliseconds"/>, and <see cref="Seconds"/> will all start over from zero.
  /// </remarks>
  public static void Reset()
  {
    timer.Reset();
    timer.Start();
  }

  /// <summary>Gets the number of elapsed milliseconds since the timer started counting. The timer cannot be reset.</summary>
  internal static uint InternalMilliseconds
  {
    get { return (uint)internalTimer.ElapsedMilliseconds; }
  }

  static readonly Stopwatch timer, internalTimer;
}

} // namespace GameLib
