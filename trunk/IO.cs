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

  public static short ReadLE2(byte[] buf, int index)
  { return (short)((buf[index+1]<<8)|buf[index]);
  }

  public static short ReadBE2(Stream stream)
  { byte[] buf = Read(stream, 2);
    return (short)((buf[0]<<8)|buf[1]);
  }

  public static short ReadBE2(byte[] buf, int index)
  { return (short)((buf[index]<<8)|buf[index+1]);
  }

  public static int ReadLE4(Stream stream)
  { byte[] buf = Read(stream, 4);
    return (int)((buf[3]<<24)|(buf[2]<<16)|(buf[1]<<8)|buf[0]);
  }
  
  public static int ReadLE4(byte[] buf, int index)
  { return (int)((buf[index+3]<<24)|(buf[index+2]<<16)|(buf[index+1]<<8)|buf[index]);
  }

  public static int ReadBE4(Stream stream)
  { byte[] buf = Read(stream, 4);
    return (int)((buf[0]<<24)|(buf[1]<<16)|(buf[2]<<8)|buf[3]);
  }

  public static int ReadBE4(byte[] buf, int index)
  { return (int)((buf[index]<<24)|(buf[index+1]<<16)|(buf[index+2]<<8)|buf[index+3]);
  }

  public static ushort ReadLE2U(Stream stream)
  { byte[] buf = Read(stream, 2);
    return (ushort)((buf[1]<<8)|buf[0]);
  }

  public static ushort ReadLE2U(byte[] buf, int index)
  { return (ushort)((buf[index+1]<<8)|buf[index]);
  }

  public static ushort ReadBE2U(Stream stream)
  { byte[] buf = Read(stream, 2);
    return (ushort)((buf[0]<<8)|buf[1]);
  }

  public static ushort ReadBE2U(byte[] buf, int index)
  { return (ushort)((buf[index]<<8)|buf[index+1]);
  }

  public static uint ReadLE4U(Stream stream)
  { byte[] buf = Read(stream, 4);
    return (uint)((buf[3]<<24)|(buf[2]<<16)|(buf[1]<<8)|buf[0]);
  }

  public static uint ReadLE4U(byte[] buf, int index)
  { return (uint)((buf[index+3]<<24)|(buf[index+2]<<16)|(buf[index+1]<<8)|buf[index]);
  }

  public static uint ReadBE4U(Stream stream)
  { byte[] buf = Read(stream, 4);
    return (uint)((buf[0]<<24)|(buf[1]<<16)|(buf[2]<<8)|buf[3]);
  }
  
  public static uint ReadBE4U(byte[] buf, int index)
  { return (uint)((buf[index]<<24)|(buf[index+1]<<16)|(buf[index+2]<<8)|buf[index+3]);
  }

  public static int WriteString(Stream stream, string str)
  { byte[] buf = System.Text.Encoding.ASCII.GetBytes(str);
    stream.Write(buf, 0, buf.Length);
    return buf.Length;
  }
  
  public static int WriteString(byte[] buf, int index, string str)
  { byte[] sbuf = System.Text.Encoding.ASCII.GetBytes(str);
    Array.Copy(sbuf, 0, buf, index, sbuf.Length);
    return sbuf.Length;
  }
  
  public static void WriteLE2(Stream stream, short val)
  { stream.WriteByte((byte)val);
    stream.WriteByte((byte)(val>>8));
  }

  public static void WriteLE2(byte[] buf, int index, short val)
  { buf[index]   = (byte)val;
    buf[index+1] = (byte)(val>>8);
  }
  
  public static void WriteBE2(Stream stream, short val)
  { stream.WriteByte((byte)(val>>8));
    stream.WriteByte((byte)val);
  }
  
  public static void WriteBE2(byte[] buf, int index, short val)
  { buf[index]   = (byte)(val>>8);
    buf[index+1] = (byte)val;
  }
  
  public static void WriteLE4(Stream stream, int val)
  { stream.WriteByte((byte)val);
    stream.WriteByte((byte)(val>>8));
    stream.WriteByte((byte)(val>>16));
    stream.WriteByte((byte)(val>>24));
  }

  public static void WriteLE4(byte[] buf, int index, int val)
  { buf[index]   = (byte)val;
    buf[index+1] = (byte)(val>>8);
    buf[index+2] = (byte)(val>>16);
    buf[index+3] = (byte)(val>>24);
  }
  
  public static void WriteBE4(Stream stream, int val)
  { stream.WriteByte((byte)(val>>24));
    stream.WriteByte((byte)(val>>16));
    stream.WriteByte((byte)(val>>8));
    stream.WriteByte((byte)val);
  }

  public static void WriteBE4(byte[] buf, int index, int val)
  { buf[index]   = (byte)(val>>24);
    buf[index+1] = (byte)(val>>16);
    buf[index+2] = (byte)(val>>8);
    buf[index+3] = (byte)val;
  }
}

} // namespace GameLib.IO