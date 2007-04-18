/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2007 Adam Milazzo

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
using GameLib.Interop.OggVorbis;

namespace GameLib.Audio
{

/// <summary>This is the base class for all audio-related exceptions in GameLib.</summary>
[Serializable]
public class AudioException : GameLibException
{ 
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public AudioException(string message) : base(message) { }
}

/// <summary>This enum contains values representing various types of Ogg Vorbis decoding errors.</summary>
public enum OggError
{ 
  /// <summary>No error occurred.</summary>
  None = 0,
  /// <summary>Read error while fetching compressed data for decode.</summary>
  Read = Ogg.OggError.Read,
  /// <summary>Internal inconsistency in decode state. Continuing is likely not possible.</summary>
  Fault = Ogg.OggError.Fault,
  /// <summary>Feature not implemented.</summary>
  NotImpl = Ogg.OggError.NotImpl,
  /// <summary>Either an invalid argument, or incompletely initialized argument passed to libvorbisfile call.</summary>
  Invalid = Ogg.OggError.Invalid,
  /// <summary>The given file/data was not recognized as Ogg Vorbis data.</summary>
  NotVorbis = Ogg.OggError.NotVorbis,
  /// <summary>The file/data is apparently an Ogg Vorbis stream, but contains a corrupted or undecipherable header.</summary>
  BadHeader = Ogg.OggError.BadHeader,
  /// <summary>The bitstream format revision of the given stream is not supported.</summary>
  BadVersion = Ogg.OggError.BadVersion,
  /// <summary>The stream does not appear to be an audio stream.</summary>
  NotAudio = Ogg.OggError.NotAudio,
  /// <summary>A packet is not decipherable due to garbage or corruption.</summary>
  BadPacket = Ogg.OggError.BadPacket,
  /// <summary>The given link exists in the Vorbis data stream, but is not decipherable due to garbage or corruption.
  /// </summary>
  BadLink = Ogg.OggError.BadLink,
  /// <summary>The given stream is not seekable.</summary>
  NoSeek = Ogg.OggError.NoSeek
}

/// <summary>This exception is thrown when the ogg vorbis decoder has a problem decoding a stream.</summary>
[Serializable]
public class OggVorbisException : AudioException
{ 
  /// <summary>Initializes this exception.</summary>
  /// <param name="code">The reason the decoder could not properly decode the stream.</param>
  /// <param name="message">The message associated with this exception.</param>
  public OggVorbisException(OggError code, string message)
    : base(String.Format("{2} (error code {0} [{1}])", code, (int)code, message)) { this.code=code; }

  /// <summary>Gets the reason the decoder could not properly decode the stream.</summary>
  /// <value>An <see cref="OggError"/> value that represents the reason the decoder could not properly decode the
  /// stream.
  /// </value>
  public OggError Code
  {
    get { return code; }
    protected set { code = value; }
  }

  /// <summary>The reason the decoder could not properly decode the stream.</summary>
  OggError code;
}

} // namespace GameLib.Audio