/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2009 Adam Milazzo

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
using System.Collections.ObjectModel;
using GameLib.Interop.SDL;

namespace GameLib.CD
{

/// <summary>This enum contains the possible status values for a CD-ROM drive.</summary>
public enum CDStatus
{ 
  /// <summary>The drive has no disc loaded.</summary>
  TrayEmpty=SDL.CDStatus.TrayEmpty,
  /// <summary>The drive has a disc loaded, but is not currently playing.</summary>
  Stopped=SDL.CDStatus.Stopped,
  /// <summary>The drive has a disc loaded and is playing it.</summary>
  Playing=SDL.CDStatus.Playing,
  /// <summary>The drive has a disc loaded and is paused.</summary>
  Paused=SDL.CDStatus.Paused,
  /// <summary>An error has occurred trying to use this drive.</summary>
  Error=SDL.CDStatus.Error
}

/// <summary>This enum contains the types of tracks that can be encountered.</summary>
public enum TrackType
{ 
  /// <summary>The track contains CD-Audio and can be played.</summary>
  Audio=SDL.TrackType.Audio,
  /// <summary>The track does not contain CD-Audio and should not be played.</summary>
  Data=SDL.TrackType.Data
}

/// <summary>This struct represents a track on a CD.</summary>
public struct Track
{ internal unsafe Track(SDL.CDTrack* track)
  { number = track->Number;
    length = track->Length;
    start  = track->Offset;
    type   = (TrackType)track->Type;
  }

  /// <summary>Gets the track number of this track.</summary>
  /// <value>The track number. The first track is track 0.</value>
  public int Number { get { return number; } }
  /// <summary>The offset into the CD at which this track starts, in frames.</summary>
  /// <value>The offset into the CD at which this track starts, in frames.</value>
  /// <remarks><see cref="CD.FramesToSeconds"/> can be used to convert this to real time.</remarks>
  public int Start { get { return start; } }
  /// <summary>The length of this track, in frames.</summary>
  /// <value>The length of this track, in frames.</value>
  /// <remarks><see cref="CD.FramesToSeconds"/> can be used to convert this to real time.</remarks>
  public int Length { get { return length; } }
  /// <summary>The type of this track.</summary>
  /// <value>The <see cref="TrackType"/> of this track.</value>
  public TrackType Type { get { return type; } }

  public override bool Equals(object obj)
  {
    return obj is Track ? this == (Track)obj : false;
  }

  public override int GetHashCode()
  {
    return (number<<16) | ((int)type<<24) | length ^ start;
  }

  int number, length, start;
  TrackType type;

  public static bool operator==(Track a, Track b)
  {
    return a.number == b.number && a.length == b.length && a.start == b.start && a.type == b.type;
  }

  public static bool operator!=(Track a, Track b)
  {
    return !(a == b);
  }
}

/// <summary>This class represents a CD-ROM drive.</summary>
public unsafe sealed class Drive
{ internal Drive(int number)
  { cd = SDL.CDOpen(number); // TODO: allow this to fail?
    if(cd==null) SDL.RaiseError();
    name = SDL.CDName(number); // for some reason, if this is called before CDOpen, then CDOpen will fail
    num = number;
  }

  ~Drive() { Dispose(true); }

  /// <summary>Gets the track number of the currently playing track.</summary>
  /// <value>The track number (zero-based) of the currently playing track.</value>
  /// <remarks>This is only valid if the drive status is <see cref="CDStatus.Playing"/> or
  /// <see cref="CDStatus.Paused"/>.
  /// </remarks>
  public int Track { get { Update(false); return cd->CurTrack; } }
  /// <summary>Gets the frame number of the currently playing frame.</summary>
  /// <value>The frame number of the currently playing frame.</value>
  /// <remarks>This is only valid if the drive status is <see cref="CDStatus.Playing"/> or
  /// <see cref="CDStatus.Paused"/>.
  /// </remarks>
  public int Frame { get { Update(false); return cd->CurFrame; } }
  /// <summary>Gets the offset into the currently playing CD, in seconds.</summary>
  /// <value>The offset into the currently playing CD, in seconds.</value>
  /// <remarks>This is only valid if the drive status is <see cref="CDStatus.Playing"/> or
  /// <see cref="CDStatus.Paused"/>.
  /// </remarks>
  public double Time { get { return CD.FramesToSeconds(Frame); } }
  /// <summary>Gets the length of the CD, in frames.</summary>
  /// <value>The length of the CD, in frames.</value>
  /// <remarks>This property is only valid if a CD is loaded in the drive. This property has some small overhead in
  /// retrieval, so try to avoid evaluating it too often.
  /// </remarks>
  public int Length { get { Update(true); return frames; } }
  /// <summary>Gets the current status of this CD-ROM drive.</summary>
  /// <value>The <see cref="CDStatus"/> of this CD-ROM drive.</value>
  public CDStatus Status { get { Update(false); return (CDStatus)cd->Status; } }
  /// <summary>Gets an array containing the tracks in the CD.</summary>
  /// <value>A <see cref="Track"/> array containing the tracks in the CD.</value>
  /// <remarks>This array will likely change if the CD is changed.
  /// This property has some small overhead in retrieval, so try to avoid evaluating it too often.
  /// </remarks>
  public ReadOnlyCollection<Track> Tracks { get { Update(true); return Array.AsReadOnly(tracks); } }
  /// <summary>Returns true if there's a CD in this CD-ROM drive.</summary>
  /// <value>A boolean indicating whether there's a CD in this CD-ROM drive.</value>
  public bool CDInDrive { get { CDStatus s = Status; return s!=CDStatus.TrayEmpty && s!=CDStatus.Error; } }

  /// <summary>Gets this drive's drive number.</summary>
  /// <value>The zero-based drive number of this CD-ROM drive. 0 represents the system's default CD-ROM drive.</value>
  public int Number { get { return num; } }
  /// <summary>Gets this drive's name.</summary>
  /// <value>The human-readable, system-dependent identifier for the CD-ROM drive.</value>
  /// <remarks>While this property often returns paths, such as "E:" or "/dev/cdrom", this should not be relied upon.
  /// </remarks>
  public string Name { get { return name; } }

  /// <summary>Plays the CD from the beginning to the end, or until <see cref="Stop"/> is called.</summary>
  public void Play() { PlayTracks(0, 0, 0, 0); }
  /// <summary>Plays the specified track.</summary>
  /// <param name="track">The zero-based track number to play.</param>
  public void Play(int track)
  { Update(true);
    Play(tracks[track].Start, tracks[track].Length);
  }
  /// <summary>Plays a range of frames on the CD.</summary>
  /// <param name="start">The frame to start playing at.</param>
  /// <param name="length">The number of frames to play.</param>
  /// <remarks><see cref="CD.SecondsToFrames"/> can be used to convert real time to frames.</remarks>
  /// <exception cref="ArgumentOutOfRangeException">The range given is outside the valid range for this CD.</exception>
  public void Play(int start, int length)
  { if(start<0 || length<0 || start+length>Length) throw new ArgumentOutOfRangeException(); // Length calls Update()
    SDL.Check(SDL.CDPlay(cd, start, length));
  }
  /// <summary>Plays a range of time on the CD.</summary>
  /// <param name="start">The time to start playing at, in seconds.</param>
  /// <param name="length">The number of seconds to play.</param>
  /// <remarks>Since the values must be converted to frames, the playback may not be exactly as specified.</remarks>
  /// <exception cref="ArgumentOutOfRangeException">The range given is outside the valid range for this CD.</exception>
  public void Play(double start, double length)
  { Play((int)Math.Floor(start*CD.FramesPerSecond), CD.SecondsToFrames(length));
  }
  /// <summary>Plays a range of tracks on the CD.</summary>
  /// <param name="startTrack">The track to start playing at.</param>
  /// <param name="numTracks">The number of tracks to play.</param>
  /// <exception cref="ArgumentOutOfRangeException">The range given is outside the valid range for this CD.</exception>
  public void PlayTracks(int startTrack, int numTracks) { PlayTracks(startTrack, 0, numTracks, 0); }
  /// <summary>Plays a range of tracks on the CD.</summary>
  /// <param name="startTrack">The track to start playing at.</param>
  /// <param name="startOffset">The frame offset within <paramref name="startTrack"/> to start playing at.</param>
  /// <param name="numTracks">The number of tracks to play.</param>
  /// <param name="endOffset">The frame offset into the last track at which playback will stop.</param>
  /// <exception cref="ArgumentOutOfRangeException">The range given is outside the valid range for this CD.</exception>
  public void PlayTracks(int startTrack, int startOffset, int numTracks, int endOffset)
  { Update(true);
    if(startTrack<0 || startOffset<0 || startTrack+numTracks>tracks.Length) throw new ArgumentOutOfRangeException();
    SDL.Check(SDL.CDPlayTracks(cd, startTrack, startOffset, numTracks, endOffset));
  }
  /// <summary>Plays a range of tracks on the CD.</summary>
  /// <param name="startTrack">The track to start playing at.</param>
  /// <param name="startOffset">The offset within <paramref name="startTrack"/> to start playing at, in seconds.</param>
  /// <param name="numTracks">The number of tracks to play.</param>
  /// <param name="endOffset">The offset into the last track at which playback will stop, in seconds.</param>
  /// <remarks>Since the offsets must be converted to frames, the playback may not be exactly as specified.</remarks>
  /// <exception cref="ArgumentOutOfRangeException">The range given is outside the valid range for this CD.</exception>
  public void PlayTracks(int startTrack, double startOffset, int numTracks, double endOffset)
  { PlayTracks(startTrack, (int)Math.Floor(startOffset*CD.FramesPerSecond), numTracks, CD.SecondsToFrames(endOffset));
  }

  /// <summary>Pauses playback on this CD-ROM drive.</summary>
  /// <remarks>This is only valid if a CD is loaded in the drive.</remarks>
  public void Pause() { SDL.Check(SDL.CDPause(cd)); }
  /// <summary>Resumes playback on this CD-ROM drive.</summary>
  /// <remarks>This is only valid if a CD is loaded in the drive.</remarks>
  public void Resume() { SDL.Check(SDL.CDResume(cd)); }
  /// <summary>Stops playback on this CD-ROM drive.</summary>
  /// <remarks>This is only valid if a CD is loaded in the drive.</remarks>
  public void Stop() { SDL.Check(SDL.CDStop(cd)); }

  /// <summary>Ejects the CD-ROM from the drive.</summary>
  public void Eject() { SDL.Check(SDL.CDEject(cd)); }

  internal void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  void Update(bool updateTracks)
  { SDL.GetCDStatus(cd);
    if(tracks==null || tracks.Length!=cd->NumTracks) tracks = new Track[cd->NumTracks];
    if(updateTracks) // TODO: find some way to detect when the disc changes
    { frames = 0;
      for(int i=0; i<tracks.Length; i++)
      { tracks[i] = new Track(cd->GetTrack(i));
        frames += tracks[i].Length;
      }
    }
  }

  void Dispose(bool finalizing)
  { if(cd!=null)
    { SDL.CDClose(cd);
      cd = null;
    }
  }

  SDL.CD* cd;
  Track[] tracks;
  string  name;
  int num, frames;
}

/// <summary>This class controls the CD-ROM subsystem.</summary>
/// <remarks><see cref="Initialize"/> and <see cref="Deinitialize"/> must be called to initialize and deinitialize
/// the CD-ROM subsystem.
/// </remarks>
public static class CD
{ 
  /// <summary>Returns the number of frames in one second of CD audio.</summary>
  /// <value>The number of frames in one second of CD audio.</value>
  /// <remarks>A frame is about 1/75th of a second.</remarks>
  public const int FramesPerSecond = (int)SDL.CDFramesPerSecond;

  /// <summary>Gets an array of CD-ROM drives.</summary>
  /// <value>A <see cref="Drive"/> array holding the available drives.</value>
  public static ReadOnlyCollection<Drive> Drives { get { return roDrives; } }

  /// <summary>Initializes the CD-ROM subsystem.</summary>
  /// <remarks>This method can be called multiple times. The <see cref="Deinitialize"/> method should be called the
  /// same number of times to finally deinitialize the system.
  /// </remarks>
  public static void Initialize()
  { if(initCount++==0)
    { SDL.Initialize(SDL.InitFlag.CDRom);
      drives = new Drive[SDL.CDNumDrives()];
      for(int i=0; i<drives.Length; i++) drives[i] = new Drive(i);
      roDrives = new ReadOnlyCollection<Drive>(drives);
    }
  }

  /// <summary>Deinitializes the CD-ROM subsystem.</summary>
  /// <remarks>This method should be called the same number of times that <see cref="Initialize"/> has been called
  /// in order to deinitialize the CD-ROM subsystem. This does not necessarily stop CDs that are playing.
  /// </remarks>
  public static void Deinitialize()
  { if(initCount==0) throw new InvalidOperationException("Deinitialize called too many times!");
    if(--initCount==0)
    { for(int i=0; i<drives.Length; i++) drives[i].Dispose();
      roDrives = null;
      drives   = null;
      SDL.Deinitialize(SDL.InitFlag.CDRom);
    }
  }

  /// <summary>Converts a number of frames into the equivalent number of seconds.</summary>
  /// <param name="frames">The number of frames.</param>
  /// <returns>The equivalent number of seconds.</returns>
  public static double FramesToSeconds(int frames) { return frames/(double)FramesPerSecond; }
  /// <summary>Converts a number of seconds into the equivalent number of frames.</summary>
  /// <param name="seconds">The number of seconds.</param>
  /// <returns>The equivalent number of frames, rounded up.</returns>
  /// <remarks>If you'd prefer to have the value rounded down, you can use the following formula:
  /// Math.Floor(seconds*CD.FramesPerSecond)
  /// </remarks>
  public static int SecondsToFrames(double seconds) { return (int)Math.Ceiling(seconds*FramesPerSecond); }

  static ReadOnlyCollection<Drive> roDrives;
  static Drive[] drives;
  static uint initCount;
}

} // namespace GameLib.CD