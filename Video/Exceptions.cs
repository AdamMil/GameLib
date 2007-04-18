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

namespace GameLib.Video
{

/// <summary>This is the base class for all video-related exceptions in GameLib.</summary>
[Serializable]
public class VideoException : GameLibException
{ 
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public VideoException(string message) : base(message) { }
}

/// <summary>This exception is thrown when the operating system has deallocated the video memory for a
/// <see cref="GameLib.Video.Surface"/>. The surface must be reloaded with image data. This should not happen on
/// modern systems.
/// </summary>
[Serializable]
public class SurfaceLostException : VideoException
{ 
  /// <summary>Initializes this exception.</summary>
  public SurfaceLostException() : base("The surface has been lost.") { }
}

} // namespace GameLib.Video