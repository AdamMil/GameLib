/*
This is an example application for the GameLib multimedia/gaming library.
http://www.adammil.net/
Copyright (C) 2004-2005 Adam Milazzo

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
using AdamMil.IO;
using GameLib.Network;
using System.IO;

namespace NetworkingTest
{

  #region Message classes
  struct Point
  {
    public Point(int x, int y) { X=x; Y=y; }

    public override string ToString()
    {
      return string.Format("({0},{1})", X, Y);
    }

    public int X, Y;
  }

  enum SimpleEnum : byte { Zero, One, Two, Three }; // use a byte to save space

  // simple structures/classes (containing only blittable types) such as these ones
  // can be serialized automatically
  // StructLayout is required for automatic serialization (serialization
  // for classes that don't implement INetSerializable). also, set Pack=1
  // to make sure no bytes are wasted
  [StructLayout(LayoutKind.Sequential, Pack=1)]
  class Simple
  {
    // default constructors are needed for deserialization of reference types
    public Simple() { }

    public Simple(SimpleEnum e, int i, float f, Point p)
    {
      Enum=e; Int=i; Float=f; Point=p;
    }

    public override string ToString()
    {
      return string.Format("{0}: Enum={1}, Int={2}, Float={3}, Point={4}",
                           GetType().Name, Enum, Int, Float, Point);
    }

    public int Int;
    public float Float;
    public Point Point; // structures can contain other structures, too
    public SimpleEnum Enum;
  }

  [StructLayout(LayoutKind.Sequential, Pack=1)]
  class OtherSimple : Simple // derivation is okay, too, so you can make a
  {                          // hierarchy with a root Message class or whatever
    public OtherSimple() { }
    public OtherSimple(SimpleEnum e, int i, float f, Point p, char c)
      : base(e, i, f, p) { Char=c; }

    public override string ToString()
    {
      return base.ToString()+", Char="+Char;
    }

    public char Char;
  }

  // complex structures/classes (classes containing non-blittable types) are
  // okay if they implement INetSerializable
  class Complex : INetSerializable
  {
    public Complex() { }
    public Complex(string str, params int[] ints) { String=str; Array=ints; }

    public override string ToString()
    {
      return string.Format("Complex: Array.Length={0}, String={1}",
                           Array.Length, String);
    }

    #region INetSerializeable Members
    // use formatted binary IO (less efficient, but oh-so-simple)
    public int SizeOf() { return IOH.CalculateSize(">*v?dp", Array, String); }

    public void SerializeTo(byte[] buf, int index, out Stream attachedStream)
    {
      attachedStream = null;
      IOH.Write(buf, index, ">*v?dp", Array, String);
    }

    public int DeserializeFrom(byte[] buf, int index, Stream attachedStream)
    {
      int bytesRead;
      object[] data = IOH.Read(buf, index, ">*v?dp", out bytesRead);
      Array  = (int[])data[0];
      String = (string)data[1];
      return bytesRead;
    }
    #endregion

    int[] Array;
    string String;
  }
  #endregion

  class App
  {
    static void Main()
    { // change the following port if something is using it
      IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 3000);

      // Medium-level networking involves using NetLink objects directly
      #region Medium-level networking
      {
        Console.WriteLine("Testing medium level networking...");

        // create the server/client NetLink objects
        TcpListener sv = new TcpListener(ep);
        sv.Start();
        NetLink client = new NetLink(ep);
        NetLink server = new NetLink(sv.AcceptSocket());
        sv.Stop();

        string str = "Hello, world!";
        client.Send(Encoding.ASCII.GetBytes(str));
        Console.WriteLine("Client sent: "+str);
        Console.WriteLine("Server received: "+ Encoding.ASCII.GetString(server.Receive()));
        Console.WriteLine();

        client.Close();
        server.Close();
      }
      #endregion

      // High-level networking involves using the Client, Server, etc classes
      #region High-level networking
      {
        Type[] types = new Type[] // types that we'll be serializing/deserializing
        { typeof(Simple), typeof(OtherSimple), typeof(Complex), typeof(float),
        };

        Console.WriteLine("Testing high-level networking...");
        Server server = new Server();
        server.MessageSent     += new ServerMessageHandler(server_MessageSent);
        server.MessageReceived += new ServerMessageHandler(server_MessageReceived);
        // registering the types allows them to be automatically serialized/deserialized
        server.MessageConverter.RegisterTypes(types);
        server.Listen(ep);

        Client client = new Client();
        client.MessageSent     += new ClientMessageHandler(client_MessageSent);
        client.MessageReceived += new ClientMessageHandler(client_MessageReceived);
        client.MessageConverter = server.MessageConverter;
        client.Connect(ep);

        // we can use NoCopy because we know that no buffer will be modified
        // between the time when Send() is called and the actual packet is sent
        SendFlag sendFlags = SendFlag.Reliable | SendFlag.NotifySent | SendFlag.NoCopy;

        client.Send(new Complex[] { new Complex("hello!", 1, 2, 3, 4, 5), new Complex("goodbye.", 1, 2, 3) },
                    sendFlags);
        server.SendToAll(new byte[] { 1, 2, 3 }, sendFlags);
        server.SendToAll(new OtherSimple(SimpleEnum.Two, 2, 3.14f, new Point(2, 2), 'x'),
                         sendFlags);
        server.SendToAll(new float[] { 1.1f, 2.2f, 3.3f }, sendFlags);
        client.Send(new Simple(SimpleEnum.One, 1, 1, new Point(-1, -1)), sendFlags);

        client.Send(new Complex("hello!", 1, 2, 3, 4, 5), sendFlags);
        client.DelayedDisconnect(1000); // give it up to 1 second to send all remaining data
        while(client.IsConnected) { }
        server.Deinitialize();
        Console.WriteLine();
      }
      #endregion

      Console.Write("Press [ENTER] to quit.");
      Console.ReadLine();
    }

    #region Event handlers
    private static void server_MessageSent(Server s, ServerPlayer p, object msg)
    {
      Console.WriteLine("Server sent:     {0}", msg);
    }

    private static void server_MessageReceived(Server sender, ServerPlayer player, object msg)
    {
      Console.WriteLine("Server received: {0}", msg);
    }

    private static void client_MessageSent(Client client, object msg)
    {
      Console.WriteLine("Client sent:     {0}", msg);
    }

    private static void client_MessageReceived(Client client, object msg)
    {
      Console.WriteLine("Client received: {0}", msg);
    }
    #endregion
  }

} // namespace NetworkingTest