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
using GameLib.Audio;

namespace AudioTest
{

class App
{ static void Main()
  { 
    #if DEBUG
    string dataPath = "../../../"; // set to something correct
    #else
    string dataPath = "data/"; // set to something correct
    #endif
    
    Audio.Initialize(); // default: 44100 hz 16-bit stereo
    Audio.AllocateChannels(8); // allocate 8 audio channels
    Audio.ReservedChannels = 1; // reserve the first channel for the music

    // by default, audio sources are streamed from the disk, even wav/au/etc
    VorbisSource ogg = new VorbisSource(dataPath+"music.ogg");
    // but a SampleSource converts any other audio source into a sample,
    // which means it's loaded completely into memory
    SampleSource smp = new SampleSource(new SoundFileSource(dataPath+"himan.wav"));

    while(true)
    { Console.WriteLine("1) Toggle playing of streaming ogg");
      Console.WriteLine("2) Play the sample");
      if(Audio.Channels[0].Playing)
      { Console.WriteLine("3) Speed up streaming ogg");
        Console.WriteLine("4) Slow down streaming ogg");
        Console.WriteLine("5) Fade out streaming ogg");
      }
      Console.WriteLine("6) Quit");
      Console.Write("Enter choice: ");
      switch(GameLib.IO.IOH.Getche())
      { case '1':
          if(Audio.Channels[0].Stopped) // if stopped, play ogg on channel 0
            ogg.Play(Audio.Infinite, Audio.Infinite, 0, 0);
          else Audio.Channels[0].Paused = !Audio.Channels[0].Paused;
          break;
        case '2': smp.Play(); break; // play on any available channel
        case '3': Audio.Channels[0].PlaybackRate += 0.1f; break;
        case '4':
          if(Audio.Channels[0].PlaybackRate > 0.1f)
            Audio.Channels[0].PlaybackRate -= 0.1f;
          break;
        case '5': Audio.Channels[0].FadeOut(450); break;
        case '6': goto done;
      }
      Console.Write("\n\n");
    }

    done:
    Audio.Deinitialize();
  }
}

} // namespace AudioTest