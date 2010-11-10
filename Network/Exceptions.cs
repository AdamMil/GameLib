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
using System.Runtime.Serialization;

namespace GameLib.Network
{

/// <summary>This is the base class for all network-related exceptions in GameLib.</summary>
[Serializable]
public class NetworkException : GameLibException
{ 
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public NetworkException(string message) : base(message) { }
  /// <summary>Initializes this exception.</summary>
  public NetworkException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>This exception is thrown when a network connection has been lost.</summary>
[Serializable]
public class ConnectionLostException : NetworkException
{ 
  /// <summary>Initializes this exception.</summary>
  public ConnectionLostException() : base("The connection has been lost.") { }
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public ConnectionLostException(string message) : base(message) { }
  /// <summary>Initializes this exception.</summary>
  public ConnectionLostException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>This exception is thrown when a remote connection did not use the expected handshake.</summary>
[Serializable]
public class HandshakeException : NetworkException
{ 
  /// <summary>Initializes this exception.</summary>
  public HandshakeException() : base("An error occurred during handshaking. Connection aborted") { }
  /// <summary>Initializes this exception.</summary>
  /// <param name="message">The message associated with this exception.</param>
  public HandshakeException(string message) : base(message) { }
  /// <summary>Initializes this exception.</summary>
  public HandshakeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

} // namespace GameLib.Network