/*
This is an example application for the GameLib multimedia/gaming library.
http://www.adammil.net/
Copyright (C) 2004 Adam Milazzo

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
using GameLib.CD;

namespace CDAudioTest
{

class App
{ static void Main()
  { CD.Initialize();
    Drive playDrive = null;
    Track playTrack = null;
    
    foreach(Drive drive in CD.Drives)
    { Console.Write("Drive {0} - ", drive.Number);
      if(drive.CDInDrive)
      { Console.WriteLine("CD in drive, {0} tracks", drive.Tracks.Length);
        foreach(Track track in drive.Tracks)
        { Console.WriteLine("  Track {0}: type={1}, start={2}, length={3}",
                            track.Number, track.Type, track.Start, track.Length);
          if(playTrack==null && track.Type==TrackType.Audio)
          { playDrive = drive; playTrack = track;
          }
        }
      }
      else Console.WriteLine("No cd in drive");
    }

    Console.WriteLine();
    if(playTrack==null) Console.WriteLine("Nothing to play!");
    else
    { Console.WriteLine("Playing track {0} of drive {1}...", playTrack.Number,
                        playDrive.Number);
      playDrive.Play(playTrack.Number);
    }

    Console.WriteLine("Press [ENTER] to quit.");
    Console.ReadLine();
    if(playDrive!=null) playDrive.Stop();
    
    CD.Deinitialize();
  }
}

} // namespace CDAudioTest