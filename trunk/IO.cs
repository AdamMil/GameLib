using System;
using System.IO;

namespace GameLib.IO
{

public class IOH
{ private IOH() {}

  public static byte[] Read(Stream stream, int length)
  { byte[] buf = new byte[length];
    Read(stream, buf, 0, length);
    return buf;
  }
  public static int Read(Stream stream, byte[] buf, int length) { return Read(stream, buf, 0, length); }
  public static int Read(Stream stream, byte[] buf, int offset, int length)
  { if(stream.Read(buf, offset, length)!=length) throw new EndOfStreamException();
    return length;
  }

  public static string ReadString(Stream stream, int length)
  { return ReadString(stream, length, System.Text.Encoding.ASCII);
  }
  public static string ReadString(Stream stream, int length, System.Text.Encoding encoding)
  { return encoding.GetString(Read(stream, length));
  }
  
  public static byte Read1(Stream stream)
  { int i = stream.ReadByte();
    if(i==-1) throw new EndOfStreamException();
    return (byte)i;
  }

  public static short ReadLE2(Stream stream)
  { byte[] buf = Read(stream, 2);
    return (short)((buf[1]<<8)|buf[0]);
  }

  public static short ReadBE2(Stream stream)
  { byte[] buf = Read(stream, 2);
    return (short)((buf[0]<<8)|buf[1]);
  }

  public static int ReadLE4(Stream stream)
  { byte[] buf = Read(stream, 4);
    return (int)((buf[3]<<24)|(buf[2]<<16)|(buf[1]<<8)|buf[0]);
  }

  public static int ReadBE4(Stream stream)
  { byte[] buf = Read(stream, 4);
    return (int)((buf[0]<<24)|(buf[1]<<16)|(buf[2]<<8)|buf[3]);
  }

  public static ushort ReadLE2U(Stream stream)
  { byte[] buf = Read(stream, 2);
    return (ushort)((buf[1]<<8)|buf[0]);
  }

  public static ushort ReadBE2U(Stream stream)
  { byte[] buf = Read(stream, 2);
    return (ushort)((buf[0]<<8)|buf[1]);
  }

  public static uint ReadLE4U(Stream stream)
  { byte[] buf = Read(stream, 4);
    return (uint)((buf[3]<<24)|(buf[2]<<16)|(buf[1]<<8)|buf[0]);
  }

  public static uint ReadBE4U(Stream stream)
  { byte[] buf = Read(stream, 4);
    return (uint)((buf[0]<<24)|(buf[1]<<16)|(buf[2]<<8)|buf[3]);
  }
}

} // namespace GameLib.IO