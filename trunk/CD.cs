using System;
using GameLib.Interop.SDL;

namespace GameLib.CD
{

public enum CDStatus
{ TrayEmpty=SDL.CDStatus.TrayEmpty, Stopped=SDL.CDStatus.Stopped, Playing=SDL.CDStatus.Playing,
  Paused=SDL.CDStatus.Paused, Error=SDL.CDStatus.Error
}
public enum TrackType
{ Audio=SDL.TrackType.Audio, Data=SDL.TrackType.Data
}

public class Track
{ internal unsafe Track(SDL.CDTrack* track)
  { number = track->Number;
    length = track->Length;
    start  = track->Offset;
    type   = (TrackType)track->Type;
  }
  
  public int Number { get { return number; } }
  public int Start  { get { return start; } }
  public int Length { get { return length; } }
  public TrackType Type { get { return type; } }
  
  int number, length, start;
  TrackType type;
}

public unsafe class Drive : IDisposable
{ internal Drive(int number)
  { cd = SDL.CDOpen(number);
    if(cd==null) SDL.RaiseError();
    name  = SDL.CDName(number); // for some reason, if this is called before CDOpen, then CDOpen will fail
    num   = number;
    oldID = cd->ID+1; // just make sure oldID != cd->ID
    Update();
  }
  ~Drive() { Dispose(true); }
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  public int      ID     { get { Update(); return cd->ID; } }
  public int      Track  { get { Update(); return cd->CurTrack; } }
  public int      Frame  { get { Update(); return cd->CurFrame; } }
  public int      Length { get { Update(); return frames; } }
  public CDStatus Status { get { Update(); return (CDStatus)cd->Status; } }
  public Track[]  Tracks { get { Update(); return tracks; } }

  public bool   CDInDrive { get { CDStatus s = Status; return s!=CDStatus.TrayEmpty && s!=CDStatus.Error; } }

  public int    Number { get { return num; } }
  public string Name   { get { return name; } }

  public void Play() { PlayTracks(0, 0, 0, 0); }
  public void Play(int track)
  { Update();
    Play(tracks[track].Start, tracks[track].Length);
  }
  public void Play(int start, int length)
  { Update();
    if(start<0 || length<0 || start+length>frames) throw new ArgumentOutOfRangeException();
    SDL.Check(SDL.CDPlay(cd, start, length));
  }
  public void PlayTracks(int startTrack, int numTracks) { PlayTracks(startTrack, 0, numTracks, 0); }
  public void PlayTracks(int startTrack, int startFrame, int numTracks, int endFrame)
  { Update();
    if(startTrack<0 || startFrame<0 || startTrack+numTracks>tracks.Length)
      throw new ArgumentOutOfRangeException();
    SDL.Check(SDL.CDPlayTracks(cd, startTrack, startFrame, numTracks, endFrame));
  }
  
  public void Pause()  { SDL.Check(SDL.CDPause(cd)); }
  public void Resume() { SDL.Check(SDL.CDResume(cd)); }
  public void Stop()   { SDL.Check(SDL.CDStop(cd)); }

  public void Eject()  { SDL.Check(SDL.CDEject(cd)); }
  
  void Update()
  { SDL.GetCDStatus(cd);
    if(cd->ID!=oldID)
    { tracks = new Track[cd->NumTracks];
      frames = 0;
      for(int i=0; i<tracks.Length; i++)
      { tracks[i] = new Track(cd->GetTrack(i));
        frames += tracks[i].Length;
      }
      oldID=cd->ID;
    }
  }

  void Dispose(bool destructor)
  { if(cd!=null)
    { SDL.CDClose(cd);
      cd = null;
    }
  }

  SDL.CD* cd;
  Track[] tracks;
  string  name;
  int     num, oldID, frames;
}

public class CD
{ private CD() { }
  
  public static Drive[] Drives { get { return drives; } }
  
  public static void Initialize()
  { if(initCount++==0)
    { SDL.Initialize(SDL.InitFlag.CDRom);
      drives = new Drive[SDL.CDNumDrives()];
      for(int i=0; i<drives.Length; i++) drives[i] = new Drive(i);
    }
  }
  
  public static void Deinitialize()
  { if(initCount==0) throw new InvalidOperationException("Deinitialize called too many times!");
    if(--initCount==0)
    { for(int i=0; i<drives.Length; i++) drives[i].Dispose();
      SDL.Deinitialize(SDL.InitFlag.CDRom);
    }
  }
  
  public static double FramesToSecs(int frames) { return (double)frames/SDL.CDFramesPerSecond; }

  static Drive[] drives;
  static uint initCount;
}

}