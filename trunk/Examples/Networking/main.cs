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
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using GameLib.IO;
using GameLib.Network;

namespace NetworkingTest
{

#region Message classes
struct Point
{ public Point(int x, int y) { X=x; Y=y; }

  public override string ToString()
  { return string.Format("({0},{1})", X, Y);
  }

  public int X, Y;
}

enum SimpleEnum : byte { Zero, One, Two, Three }; // use a byte to save space

// simple structures/classes (containing only value types) such as these ones
// can be serialized automatically
// StructLayout is required for automatic serialization (serialization
// for classes that don't implement INetSerializeable). also, set Pack=1
// to make sure no bytes are wasted
[StructLayout(LayoutKind.Sequential, Pack=1)]
class Simple
{ public Simple() { } // empty constructors are needed for deserialization
  public Simple(SimpleEnum e, int i, float f, Point p)
  { Enum=e; Int=i; Float=f; Point=p;
  }

  public override string ToString()
  { return string.Format("{0}: Enum={1}, Int={2}, Float={3}, Point={4}",
                         GetType().Name, Enum, Int, Float, Point);
  }

  public SimpleEnum Enum;
  public int    Int;
  public float  Float;
  public Point  Point; // structures can contain other structures, too
}

[StructLayout(LayoutKind.Sequential, Pack=1)]
class OtherSimple : Simple // derivation is okay, too, so you can make a
{ public OtherSimple() { } // hierarchy with a root Message class or whatever
  public OtherSimple(SimpleEnum e, int i, float f, Point p, char c)
    : base(e, i, f, p) { Char=c; }

  public override string ToString()
  { return base.ToString()+", Char="+Char;
  }

  public char Char;
}

// complex structures/classes (classes containing non-value types) are okay
// if they implement INetSerializeable
class Complex : INetSerializeable
{ public Complex() { }
  public Complex(string str, params int[] ints) { String=str; Array=ints; }

  public override string ToString()
  { return string.Format("Complex: Array.Length={0}, String={1}",
                         Array.Length, String);
  }

  #region INetSerializeable Members
  public int SizeOf() { return IOH.CalculateSize(">d?dp", Array, String); }

  public void SerializeTo(byte[] buf, int index) // use formatted binary IO
  { IOH.Write(buf, index, ">d?dp", Array.Length, Array, String);
  }

  public void DeserializeFrom(byte[] buf, int index)
  { Array = new int[IOH.ReadBE4(buf, index)]; index += 4;
    for(int i=0; i<Array.Length; index+=4,i++)
      Array[i] = IOH.ReadBE4(buf, index);
    String = System.Text.Encoding.ASCII.GetString(buf, index+1, buf[index]);
  }
  #endregion

  int[] Array;
  string String;
}
#endregion

class App
{ static void Main()
  { IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 3000); // change port if necessary
    Type[] types = new Type[] // types that we'll be serializing/deserializing
    { typeof(Simple), typeof(OtherSimple), typeof(Complex)
    };

    { Console.WriteLine("Testing medium level networking...");
      Console.WriteLine("TODO: do something here"); // TODO: do something here
      Console.WriteLine();
    }

    #region High-level networking
    { Console.WriteLine("Testing high-level networking...");
      Server server = new Server();
      server.RegisterTypes(types); // registering the types allows them to be
      server.Listen(ep);           // automatically serialized/deserialized
      server.MessageSent += new ServerSentHandler(server_MessageSent);
      server.MessageReceived += new ServerReceivedHandler(server_MessageReceived);

      Client client = new Client();
      client.RegisterTypes(types);
      client.Connect(ep);
      client.MessageSent += new ClientSentHandler(client_MessageSent);
      client.MessageReceived += new ClientReceivedHandler(client_MessageReceived);
      
      server.DefaultFlags = client.DefaultFlags =
        SendFlag.ReliableSequential | SendFlag.NotifySent;
      
      server.Send(null, new byte[] { 1, 2, 3 });
      client.Send(new Simple(SimpleEnum.One, 1, 1, new Point(-1, -1)));
      server.Send(null, new OtherSimple(SimpleEnum.Two, 2, 3.14f,
                                        new Point(2, 2), 'x'));
      client.Send(new Complex("hello!", 1, 2, 3, 4, 5));
      client.DelayedDisconnect(1000);
      while(client.Connected);
      server.Close();
      Console.WriteLine();
    }
    #endregion
    
    Console.Write("Press [ENTER] to quit.");
    Console.ReadLine();
  }

  #region Event handlers
  private static void server_MessageSent(Server s, ServerPlayer p, object msg)
  { Console.WriteLine("Server sent:     {0}", msg);
  }

  private static void server_MessageReceived(Server sender, ServerPlayer player,
                                             object msg)
  { Console.WriteLine("Server received: {0}", msg);
  }

  private static void client_MessageSent(Client client, object msg)
  { Console.WriteLine("Client sent:     {0}", msg);
  }

  private static void client_MessageReceived(Client client, object msg)
  { Console.WriteLine("Client received: {0}", msg);
  }
  #endregion
}

} // namespace NetworkingTest