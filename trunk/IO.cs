/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2004 Adam Milazzo

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
using System.IO;

namespace GameLib.IO
{

#region StreamStream
public class StreamStream : Stream, IDisposable
{ public StreamStream(Stream stream, long start, long length) : this(stream, start, length, false, false) { }
  public StreamStream(Stream stream, long start, long length, bool shared)
    : this(stream, start, length, shared, false) { }
  public StreamStream(Stream stream, long start, long length, bool shared, bool closeInner)
  { if(!stream.CanSeek && (stream.Position!=start || shared))
      throw new ArgumentException("If using an unseekable stream, 'start' must equal 'stream.Position' and "+
                                  "shared must be false");
    if(stream==null) throw new ArgumentNullException("stream");
    if(start<0 || length<0) throw new ArgumentOutOfRangeException("'start' or 'count'", "cannot be negative");
    this.stream=stream; this.start=start; this.length=length; this.shared=shared; this.closeInner=closeInner;
  }
  ~StreamStream() { Dispose(true); }
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  public override bool CanRead { get { AssertOpen(); return stream.CanRead; } }
  public override bool CanSeek { get { AssertOpen(); return stream.CanSeek; } }
  public override bool CanWrite { get { AssertOpen(); return stream.CanWrite; } }

  public override long Length { get { AssertOpen(); return length; } }
  public override long Position { get { AssertOpen(); return position; } set { Seek(value, SeekOrigin.Begin); } }

  public override void Close()
  { if(stream==null) return;
    if(closeInner) stream.Close();
    stream=null;
  }
  public override void Flush() { AssertOpen(); stream.Flush(); }

  public override int Read(byte[] buffer, int offset, int count)
  { AssertOpen();
    if(shared) stream.Position = position+start;
    int ret = stream.Read(buffer, offset, count);
    position += ret;
    return ret;
  }

  public override int ReadByte()
  { AssertOpen();
    if(shared) stream.Position = position+start;
    int ret = stream.ReadByte();
    if(ret!=-1) position++;
    return ret;
  }

  public override long Seek(long offset, SeekOrigin origin)
  { AssertOpen();
    switch(origin)
    { case SeekOrigin.Current: offset+=position; break;
      case SeekOrigin.End: offset=length-offset; break;
    }
    if(offset<0 || offset>length)
      throw new ArgumentOutOfRangeException("Cannot seek outside the bounds of this stream.");
    return position = stream.Seek(offset+start, SeekOrigin.Begin)-start;
  }

  public override void SetLength(long value)
  { AssertOpen();
    if(value>length) throw new NotSupportedException("Cannot increase length of a StreamStream");
    length=value;
    if(position>length) Position=length;
  }

  public override void Write(byte[] buffer, int offset, int count)
  { AssertOpen();
    if(count>length-position) throw new ArgumentException("Cannot write past the end of a StreamStream");
    if(shared) stream.Position = position+start;
    stream.Write(buffer, offset, count);
    position = stream.Position-start;
  }

  public override void WriteByte(byte value)
  { AssertOpen();
    if(position==length) throw new ArgumentException("Cannot write past the end of a StreamStream");
    if(shared) stream.Position = position+start;
    stream.WriteByte(value);
    position++;
  }

  protected void AssertOpen()
  { if(stream==null) throw new ObjectDisposedException("Stream", "The inner stream was already closed.");
  }
  
  protected void Dispose(bool destructing) { Close(); }

  Stream stream;
  long start, length, position;
  bool shared, closeInner;
}
#endregion

public class IOH
{ private IOH() {}

  public static char Getch()  { return GameLib.Interop.GLUtility.Utility.Getch(); }
  public static char Getche() { return GameLib.Interop.GLUtility.Utility.Getche(); }

  public static int CalculateSize(string format, params object[] parms)
  { int  length=0;
    char c;
    bool unicode=false;

    for(int i=0,j=0,flen=format.Length,prefix; i<flen; i++)
    { c = format[i];
      if(char.IsDigit(c))
      { prefix = c-'0';
        while(++i<flen && char.IsDigit(c=format[i])) prefix = prefix*10 + c-'0';
        if(i==flen) throw new ArgumentException("Missing operator at "+i.ToString(), "format");
      }
      else if(c=='?')
      { if(j==parms.Length)
          throw new ArgumentException(string.Format("Not enough arguments (expecting array near {0})!", i));
        Array arr = parms[j] as Array;
        if(arr==null)
          throw new ArgumentException(string.Format("Argument {0} is not an array! (It's {1})", j,
                                                    parms[j]==null ? "null" : parms[j].GetType().ToString()));
        j++; prefix=arr.Length;
        if(++i==flen) throw new ArgumentException("Expected something after '?' at "+i.ToString(), "format");
        c = format[i];
      }
      else prefix=(c=='s' || c=='p' ? -1 : 1);

      switch(c)
      { case 'b': case 'B': case 'x': length += prefix; break;
        case 'w': case 'W': length += prefix*2; break;
        case 'd': case 'D': case 'f': length += prefix*4; break;
        case 'q': case 'Q': case 'F': length += prefix*8; break;
        case 'c': length += unicode ? prefix*2 : prefix; break;
        case 's':
          if(prefix==-1)
          { if(j==parms.Length)
              throw new ArgumentException(string.Format("Not enough arguments (expecting string near {0})!", i));
            string str = parms[j] as string;
            if(str==null)
              throw new ArgumentException(string.Format("Argument {0} is not a string! (It's {1})", j,
                                                        parms[j]==null ? "null" : parms[j].GetType().ToString()));
            j++; prefix=str.Length;
          }
          length += unicode ? prefix*2 : prefix;
          break;
        case 'p':
          if(prefix==-1)
          { if(j==parms.Length)
              throw new ArgumentException(string.Format("Not enough arguments (expecting string near {0})!", i));
            string str = parms[j] as string;
            if(str==null)
              throw new ArgumentException(string.Format("Argument {0} is not a string! (It's {1})", j,
                                                        parms[j]==null ? "null" : parms[j].GetType().ToString()));
            j++; prefix=str.Length;
          }
          prefix = Math.Min(prefix, 255);
          length += (unicode ? prefix*2 : prefix) + 1;
          break;
        case 'A': unicode=false; break;
        case 'U': unicode=true; break;
        case '<': case '>': case '=': break;
        default: throw new ArgumentException(string.Format("Unknown character '{0}' at {1}", c, i), "format");
      }
    }
    return length;
  }

  public static int CopyStream(Stream source, Stream dest) { return CopyStream(source, dest, false); }
  public static int CopyStream(Stream source, Stream dest, bool rewindSource)
  { if(rewindSource) source.Position=0;
    byte[] buf = new byte[1024];
    int read, total=0;
    while(true)
    { read = source.Read(buf, 0, 1024);
      total += read;
      if(read==0) return total;
      dest.Write(buf, 0, read);
    }
  }

  public static byte[] Read(Stream stream, int length)
  { byte[] buf = new byte[length];
    Read(stream, buf, 0, length);
    return buf;
  }
  public static int Read(Stream stream, byte[] buf) { return Read(stream, buf, 0, buf.Length); }
  public static int Read(Stream stream, byte[] buf, int length) { return Read(stream, buf, 0, length); }
  public static int Read(Stream stream, byte[] buf, int offset, int length)
  { int read=0, total=0;
    while(true)
    { read = stream.Read(buf, offset+read, length-read);
      total += read;
      if(total==length) return length;
      if(read==0) throw new EndOfStreamException();
    }
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

  public static long ReadLE8(Stream stream)
  { return (uint)ReadLE4(stream)|((long)ReadLE4(stream)<<32);
  }
  
  public static long ReadLE8(byte[] buf, int index)
  { return (uint)ReadLE4(buf, index)|((long)ReadLE4(buf, index+4)<<32);
  }

  public static long ReadBE8(Stream stream)
  { return ((long)ReadBE4(stream)<<32)|(uint)ReadBE4(stream);
  }

  public static long ReadBE8(byte[] buf, int index)
  { return ((long)ReadBE4(buf, index)<<32)|(uint)ReadBE4(buf, index+4);
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

  public static ulong ReadLE8U(Stream stream)
  { return ReadLE4U(stream)|((ulong)ReadLE4U(stream)<<32);
  }
  
  public static ulong ReadLE8U(byte[] buf, int index)
  { return ReadLE4U(buf, index)|((ulong)ReadLE4U(buf, index+4)<<32);
  }

  public static ulong ReadBE8U(Stream stream)
  { return ((ulong)ReadBE4U(stream)<<32)|ReadBE4U(stream);
  }

  public static ulong ReadBE8U(byte[] buf, int index)
  { return ((ulong)ReadBE4U(buf, index)<<32)|ReadBE4U(buf, index+4);
  }

  public static float ReadFloat(Stream stream)
  { unsafe
    { byte[] buf = Read(stream, sizeof(float));
      fixed(byte* ptr=buf) return *(float*)ptr;
    }
  }

  public static float ReadFloat(byte[] buf, int index)
  { unsafe { fixed(byte* ptr=buf) return *(float*)(ptr+index); }
  }

  public static double ReadDouble(Stream stream)
  { unsafe
    { byte[] buf = Read(stream, sizeof(double));
      fixed(byte* ptr=buf) return *(double*)ptr;
    }
  }

  public static double ReadDouble(byte[] buf, int index)
  { unsafe { fixed(byte* ptr=buf) return *(double*)(ptr+index); }
  }

  public static void Skip(Stream stream, int bytes)
  { if(stream.CanSeek) stream.Position += bytes;
    else if(bytes<8) while(bytes-- > 0) stream.ReadByte();
    else if(bytes<=512) Read(stream, bytes);
    else
    { byte[] buf = new byte[512];
      while(bytes>0)
      { stream.Read(buf, 0, Math.Min(bytes, 512));
        bytes -= 512;
      }
    }
  }

  #region Formatted binary write
  public static int Write(byte[] buf, string format, params object[] parms) { return Write(buf, 0, format, parms); }
  public static int Write(byte[] buf, int index, string format, params object[] parms)
  { char c;
    #if BIGENDIAN
    bool bigendian=true,unicode=false;
    #else
    bool bigendian=false,unicode=false;
    #endif

    int i=0, j=0, origIndex=index;
    try
    { for(int flen=format.Length,prefix; i<flen; i++)
      { c = format[i];
        if(char.IsDigit(c))
        { prefix = c-'0';
          while(++i<flen && char.IsDigit(c=format[i])) prefix = prefix*10 + c-'0';
          if(i==flen) throw new ArgumentException("Missing operator at "+i.ToString(), "format");
        }
        else if(c=='?') { prefix=((Array)parms[j]).Length; c=format[++i]; }
        else prefix=(c=='s' || c=='p' ? -1 : 1);
        if(prefix==0) continue;
        switch(c)
        { case 'x': Array.Clear(buf, index, prefix); index += prefix; break;
          case 'b':
            { sbyte[] arr = parms[j] as sbyte[];
              if(arr==null) do buf[index++] = (byte)Convert.ToSByte(parms[j++]); while(--prefix!=0);
              else for(int k=0; k<prefix; k++) buf[index++] = (byte)arr[k];
            }
            break;
          case 'B':
            { byte[] arr = parms[j] as byte[];
              if(arr==null) do buf[index++] = Convert.ToByte(parms[j++]); while(--prefix!=0);
              else { Array.Copy(arr, 0, buf, index, prefix); j++; }
              index += prefix;
            }
            break;
          // WHERE ARE MY C-STYLE MACROS?!
          case 'w':
            { short[] arr = parms[j] as short[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; index+=2,k++) WriteBE2(buf, index, arr[k]);
                else for(int k=0; k<prefix; index+=2,k++) WriteLE2(buf, index, arr[k]);
                j++;
              }
              else if(bigendian)
                do { WriteBE2(buf, index, Convert.ToInt16(parms[j++])); index+=2; } while(--prefix!=0);
              else
                do { WriteLE2(buf, index, Convert.ToInt16(parms[j++])); index+=2; } while(--prefix!=0);
            }
            break;
          case 'W':
            { ushort[] arr = parms[j] as ushort[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; index+=2,k++) WriteBE2U(buf, index, arr[k]);
                else for(int k=0; k<prefix; index+=2,k++) WriteLE2U(buf, index, arr[k]);
                j++;
              }
              else if(bigendian)
                do { WriteBE2U(buf, index, Convert.ToUInt16(parms[j++])); index+=2; } while(--prefix!=0);
              else
                do { WriteLE2U(buf, index, Convert.ToUInt16(parms[j++])); index+=2; } while(--prefix!=0);
            }
            break;
          case 'd':
            { int[] arr = parms[j] as int[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; index+=4,k++) WriteBE4(buf, index, arr[k]);
                else for(int k=0; k<prefix; index+=4,k++) WriteLE4(buf, index, arr[k]);
                j++;
              }
              else if(bigendian)
                do { WriteBE4(buf, index, Convert.ToInt32(parms[j++])); index+=4; } while(--prefix!=0);
              else
                do { WriteLE4(buf, index, Convert.ToInt32(parms[j++])); index+=4; } while(--prefix!=0);
            }
            break;
          case 'D':
            { uint[] arr = parms[j] as uint[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; index+=4,k++) WriteBE4U(buf, index, arr[k]);
                else for(int k=0; k<prefix; index+=4,k++) WriteLE4U(buf, index, arr[k]);
                j++;
              }
              else if(bigendian)
                do { WriteBE4U(buf, index, Convert.ToUInt32(parms[j++])); index+=4; } while(--prefix!=0);
              else
                do { WriteLE4U(buf, index, Convert.ToUInt32(parms[j++])); index+=4; } while(--prefix!=0);
            }
            break;
          case 'f':
            { float[] arr = parms[j] as float[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; index+=4,k++) WriteFloat(buf, index, arr[k]);
                else for(int k=0; k<prefix; index+=4,k++) WriteFloat(buf, index, arr[k]);
                j++;
              }
              else if(bigendian)
                do { WriteFloat(buf, index, Convert.ToSingle(parms[j++])); index+=4; } while(--prefix!=0);
              else
                do { WriteFloat(buf, index, Convert.ToSingle(parms[j++])); index+=4; } while(--prefix!=0);
            }
            break;
          case 'F':
            { double[] arr = parms[j] as double[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; index+=8,k++) WriteDouble(buf, index, arr[k]);
                else for(int k=0; k<prefix; index+=8,k++) WriteDouble(buf, index, arr[k]);
                j++;
              }
              else if(bigendian)
                do { WriteDouble(buf, index, Convert.ToDouble(parms[j++])); index+=8; } while(--prefix!=0);
              else
                do { WriteDouble(buf, index, Convert.ToDouble(parms[j++])); index+=8; } while(--prefix!=0);
            }
            break;
          case 'q':
            { long[] arr = parms[j] as long[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; index+=8,k++) WriteBE8(buf, index, arr[k]);
                else for(int k=0; k<prefix; index+=8,k++) WriteLE8(buf, index, arr[k]);
                j++;
              }
              else if(bigendian)
                do { WriteBE8(buf, index, Convert.ToInt64(parms[j++])); index+=8; } while(--prefix!=0);
              else
                do { WriteLE8(buf, index, Convert.ToInt64(parms[j++])); index+=8; } while(--prefix!=0);
            }
            break;
          case 'Q':
            { ulong[] arr = parms[j] as ulong[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; index+=8,k++) WriteBE8U(buf, index, arr[k]);
                else for(int k=0; k<prefix; index+=8,k++) WriteLE8U(buf, index, arr[k]);
                j++;
              }
              else if(bigendian)
                do { WriteBE8U(buf, index, Convert.ToUInt64(parms[j++])); index+=8; } while(--prefix!=0);
              else
                do { WriteLE8U(buf, index, Convert.ToUInt64(parms[j++])); index+=8; } while(--prefix!=0);
            }
            break;
          case 'c':
            { char[] arr = parms[j] as char[];
              if(arr!=null)
              { if(unicode)
                { if(bigendian)
                    for(int k=0; k<prefix; index+=2,k++) WriteBE2U(buf, index, (ushort)arr[k]);
                  else
                    for(int k=0; k<prefix; index+=2,k++) WriteLE2U(buf, index, (ushort)arr[k]);
                }
                else
                { byte[] bytes = System.Text.Encoding.ASCII.GetBytes(arr);
                  Array.Copy(bytes, 0, buf, index, bytes.Length);
                  index += bytes.Length;
                }
                j++;
              }
              else if(unicode)
              { if(bigendian)
                  do { WriteBE2U(buf, index, (ushort)Convert.ToChar(parms[j++])); index+=2; } while(--prefix!=0);
                else
                  do { WriteLE2U(buf, index, (ushort)Convert.ToChar(parms[j++])); index+=2; } while(--prefix!=0);
              }
              else
              { arr = new char[prefix];
                for(int k=0; k<prefix; k++) arr[k]=Convert.ToChar(parms[j++]);
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(arr);
                Array.Copy(bytes, 0, buf, index, bytes.Length);
                index += bytes.Length;
              }
            }
            break;
          case 's':
            { string str = (string)parms[j++];
              if(prefix==-1) prefix=str.Length;
              int slen = Math.Min(prefix, str.Length);
              prefix -= slen;
              if(unicode)
              { if(bigendian) for(int k=0; k<slen; index+=2,k++) WriteBE2U(buf, index, (ushort)str[k]);
                else for(int k=0; k<slen; index+=2,k++) WriteLE2U(buf, index, (ushort)str[k]);
                Array.Clear(buf, index, prefix); index += prefix*2;
              }
              else
              { byte[] bytes = new byte[slen];
                System.Text.Encoding.ASCII.GetBytes(str, 0, slen, bytes, 0);
                Array.Copy(bytes, 0, buf, index, slen);
                index += slen;
                Array.Clear(buf, index, prefix); index += prefix;
              }
            }
            break;
          case 'p':
            { string str = (string)parms[j++];
              if(prefix==-1) prefix=Math.Min(str.Length, 255);
              else if(prefix>255) throw new ArgumentException("Prefix for 'p' cannot be >255");
              buf[index++] = (byte)prefix;
              int slen = Math.Min(prefix, str.Length);
              prefix -= slen;
              if(unicode)
              { if(bigendian) for(int k=0; k<slen; index+=2,k++) WriteBE2U(buf, index, (ushort)str[k]);
                else for(int k=0; k<slen; index+=2,k++) WriteLE2U(buf, index, (ushort)str[k]);
                Array.Clear(buf, index, prefix); index += prefix*2;
              }
              else
              { byte[] bytes = new byte[slen];
                System.Text.Encoding.ASCII.GetBytes(str, 0, slen, bytes, 0);
                Array.Copy(bytes, 0, buf, index, slen);
                index += slen;
                Array.Clear(buf, index, prefix); index += prefix;
              }
            }
            break;
          case 'A': unicode=false; break;
          case 'U': unicode=true; break;
          case '<': bigendian=false; break;
          case '>': bigendian=true; break;
          #if BIGENDIAN
          case '=': bigendian=true; break;
          #else
          case '=': bigendian=false; break;
          #endif
          default: throw new ArgumentException(string.Format("Unknown character '{0}'", c, i), "format");
        }
      }
    }
    catch(Exception e)
    { throw new ArgumentException(string.Format("Error near char {0}, near parameter {1} -- {2}", e.Message), e);
    }
    return index-origIndex;
  }

  public static int Write(Stream stream, string format, params object[] parms)
  { char c;
    #if BIGENDIAN
    bool bigendian=true,unicode=false;
    #else
    bool bigendian=false,unicode=false;
    #endif

    int i=0, j=0, origPos=(int)stream.Position;
    try
    { for(int flen=format.Length,prefix; i<flen; i++)
      { c = format[i];
        if(char.IsDigit(c))
        { prefix = c-'0';
          while(++i<flen && char.IsDigit(c=format[i])) prefix = prefix*10 + c-'0';
          if(i==flen) throw new ArgumentException("Missing operator at "+i.ToString(), "format");
        }
        else if(c=='?') { prefix=((Array)parms[j]).Length; c=format[++i]; }
        else prefix=(c=='s' || c=='p' ? -1 : 1);
        if(prefix==0) continue;
        switch(c)
        { case 'x': do stream.WriteByte(0); while(--prefix!=0); break;
          case 'b':
            { sbyte[] arr = parms[j] as sbyte[];
              if(arr==null) do stream.WriteByte((byte)Convert.ToSByte(parms[j++])); while(--prefix!=0);
              else for(int k=0; k<prefix; k++) stream.WriteByte((byte)arr[k]);
            }
            break;
          case 'B':
            { byte[] arr = parms[j] as byte[];
              if(arr==null) do stream.WriteByte((byte)Convert.ToSByte(parms[j++])); while(--prefix!=0);
              else IOH.Write(stream, arr);
            }
            break;
          // WHERE ARE MY C-STYLE MACROS?!
          case 'w':
            { short[] arr = parms[j] as short[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; k++) WriteBE2(stream, arr[k]);
                else for(int k=0; k<prefix; k++) WriteLE2(stream, arr[k]);
                j++;
              }
              else if(bigendian) do WriteBE2(stream, Convert.ToInt16(parms[j++])); while(--prefix!=0);
              else do WriteLE2(stream, Convert.ToInt16(parms[j++])); while(--prefix!=0);
            }
            break;
          case 'W':
            { ushort[] arr = parms[j] as ushort[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; k++) WriteBE2U(stream, arr[k]);
                else for(int k=0; k<prefix; k++) WriteLE2U(stream, arr[k]);
                j++;
              }
              else if(bigendian) do WriteBE2U(stream, Convert.ToUInt16(parms[j++])); while(--prefix!=0);
              else do WriteLE2U(stream, Convert.ToUInt16(parms[j++])); while(--prefix!=0);
            }
            break;
          case 'd':
            { int[] arr = parms[j] as int[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; k++) WriteBE4(stream, arr[k]);
                else for(int k=0; k<prefix; k++) WriteLE4(stream, arr[k]);
                j++;
              }
              else if(bigendian) do WriteBE4(stream, Convert.ToInt32(parms[j++])); while(--prefix!=0);
              else do WriteLE4(stream, Convert.ToInt32(parms[j++])); while(--prefix!=0);
            }
            break;
          case 'D':
            { uint[] arr = parms[j] as uint[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; k++) WriteBE4U(stream, arr[k]);
                else for(int k=0; k<prefix; k++) WriteLE4U(stream, arr[k]);
                j++;
              }
              else if(bigendian) do WriteBE4U(stream, Convert.ToUInt32(parms[j++])); while(--prefix!=0);
              else do WriteLE4U(stream, Convert.ToUInt32(parms[j++])); while(--prefix!=0);
            }
            break;
          case 'f':
            { float[] arr = parms[j] as float[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; k++) WriteFloat(stream, arr[k]);
                else for(int k=0; k<prefix; k++) WriteFloat(stream, arr[k]);
                j++;
              }
              else if(bigendian) do WriteFloat(stream, Convert.ToSingle(parms[j++])); while(--prefix!=0);
              else do WriteFloat(stream, Convert.ToSingle(parms[j++])); while(--prefix!=0);
            }
            break;
          case 'F':
            { double[] arr = parms[j] as double[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; k++) WriteDouble(stream, arr[k]);
                else for(int k=0; k<prefix; k++) WriteDouble(stream, arr[k]);
                j++;
              }
              else if(bigendian) do WriteDouble(stream, Convert.ToDouble(parms[j++])); while(--prefix!=0);
              else do WriteDouble(stream, Convert.ToDouble(parms[j++])); while(--prefix!=0);
            }
            break;
          case 'q':
            { long[] arr = parms[j] as long[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; k++) WriteBE8(stream, arr[k]);
                else for(int k=0; k<prefix; k++) WriteLE8(stream, arr[k]);
                j++;
              }
              else if(bigendian) do WriteBE8(stream, Convert.ToInt64(parms[j++])); while(--prefix!=0);
              else do WriteLE8(stream, Convert.ToInt64(parms[j++])); while(--prefix!=0);
            }
            break;
          case 'Q':
            { ulong[] arr = parms[j] as ulong[];
              if(arr!=null)
              { if(bigendian) for(int k=0; k<prefix; k++) WriteBE8U(stream, arr[k]);
                else for(int k=0; k<prefix; k++) WriteLE8U(stream, arr[k]);
                j++;
              }
              else if(bigendian) do WriteBE8U(stream, Convert.ToUInt64(parms[j++])); while(--prefix!=0);
              else do WriteLE8U(stream, Convert.ToUInt64(parms[j++])); while(--prefix!=0);
            }
            break;
          case 'c':
            { char[] arr = parms[j] as char[];
              if(arr!=null)
              { if(unicode)
                { if(bigendian) for(int k=0; k<prefix; k++) WriteBE2U(stream, (ushort)arr[k]);
                  else for(int k=0; k<prefix; k++) WriteLE2U(stream, (ushort)arr[k]);
                }
                else Write(stream, System.Text.Encoding.ASCII.GetBytes(arr));
                j++;
              }
              else if(unicode)
              { if(bigendian) do WriteBE2U(stream, (ushort)Convert.ToChar(parms[j++])); while(--prefix!=0);
                else do WriteLE2U(stream, (ushort)Convert.ToChar(parms[j++])); while(--prefix!=0);
              }
              else
              { arr = new char[prefix];
                for(int k=0; k<prefix; k++) arr[k]=Convert.ToChar(parms[j++]);
                Write(stream, System.Text.Encoding.ASCII.GetBytes(arr));
              }
            }
            break;
          case 's':
            { string str = (string)parms[j++];
              if(prefix==-1) prefix=str.Length;
              int slen = Math.Min(prefix, str.Length);
              prefix -= slen;
              if(unicode)
              { if(bigendian) for(int k=0; k<slen; k++) WriteBE2U(stream, (ushort)str[k]);
                else for(int k=0; k<slen; k++) WriteLE2U(stream, (ushort)str[k]);
                while(prefix--!=0) stream.WriteByte(0);
              }
              else
              { byte[] bytes = new byte[slen];
                System.Text.Encoding.ASCII.GetBytes(str, 0, slen, bytes, 0);
                Write(stream, bytes);
                while(prefix--!=0) stream.WriteByte(0);
              }
            }
            break;
          case 'p':
            { string str = (string)parms[j++];
              if(prefix==-1) prefix=Math.Min(str.Length, 255);
              else if(prefix>255) throw new ArgumentException("Prefix for 'p' cannot be >255");
              stream.WriteByte((byte)prefix);
              int slen = Math.Min(prefix, str.Length);
              prefix -= slen;
              if(unicode)
              { if(bigendian) for(int k=0; k<slen; k++) WriteBE2U(stream, (ushort)str[k]);
                else for(int k=0; k<slen; k++) WriteLE2U(stream, (ushort)str[k]);
                while(prefix--!=0) stream.WriteByte(0);
              }
              else
              { byte[] bytes = new byte[slen];
                System.Text.Encoding.ASCII.GetBytes(str, 0, slen, bytes, 0);
                Write(stream, bytes);
                while(prefix--!=0) stream.WriteByte(0);
              }
            }
            break;
          case 'A': unicode=false; break;
          case 'U': unicode=true; break;
          case '<': bigendian=false; break;
          case '>': bigendian=true; break;
          #if BIGENDIAN
          case '=': bigendian=true; break;
          #else
          case '=': bigendian=false; break;
          #endif
          default: throw new ArgumentException(string.Format("Unknown character '{0}'", c, i), "format");
        }
      }
    }
    catch(Exception e)
    { throw new ArgumentException(string.Format("Error near char {0}, near parameter {1} -- {2}", e.Message), e);
    }
    return (int)stream.Position-origPos;
  }
  #endregion

  public static void Write(Stream stream, byte[] data) { stream.Write(data, 0, data.Length); }

  public static int WriteString(Stream stream, string str)
  { return WriteString(stream, str, System.Text.Encoding.ASCII);
  }
  public static int WriteString(Stream stream, string str, System.Text.Encoding encoding)
  { byte[] buf = encoding.GetBytes(str);
    stream.Write(buf, 0, buf.Length);
    return buf.Length;
  }
  
  public static int WriteString(byte[] buf, int index, string str)
  { return WriteString(buf, index, str, System.Text.Encoding.ASCII);
  }
  public static int WriteString(byte[] buf, int index, string str, System.Text.Encoding encoding)
  { byte[] sbuf = encoding.GetBytes(str);
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

  public static void WriteLE8(Stream stream, long val)
  { WriteLE4(stream, (int)val);
    WriteLE4(stream, (int)(val>>32));
  }

  public static void WriteLE8(byte[] buf, int index, long val)
  { WriteLE4(buf, index, (int)val);
    WriteLE4(buf, index+4, (int)(val>>32));
  }
  
  public static void WriteBE8(Stream stream, long val)
  { WriteLE4(stream, (int)(val>>32));
    WriteLE4(stream, (int)val);
  }

  public static void WriteBE8(byte[] buf, int index, long val)
  { WriteLE4(buf, index, (int)(val>>32));
    WriteLE4(buf, index+4, (int)val);
  }
  
  public static void WriteLE2U(Stream stream, ushort val)
  { stream.WriteByte((byte)val);
    stream.WriteByte((byte)(val>>8));
  }

  public static void WriteLE2U(byte[] buf, int index, ushort val)
  { buf[index]   = (byte)val;
    buf[index+1] = (byte)(val>>8);
  }
  
  public static void WriteBE2U(Stream stream, ushort val)
  { stream.WriteByte((byte)(val>>8));
    stream.WriteByte((byte)val);
  }
  
  public static void WriteBE2U(byte[] buf, int index, ushort val)
  { buf[index]   = (byte)(val>>8);
    buf[index+1] = (byte)val;
  }
  
  public static void WriteLE4U(Stream stream, uint val)
  { stream.WriteByte((byte)val);
    stream.WriteByte((byte)(val>>8));
    stream.WriteByte((byte)(val>>16));
    stream.WriteByte((byte)(val>>24));
  }

  public static void WriteLE4U(byte[] buf, int index, uint val)
  { buf[index]   = (byte)val;
    buf[index+1] = (byte)(val>>8);
    buf[index+2] = (byte)(val>>16);
    buf[index+3] = (byte)(val>>24);
  }
  
  public static void WriteBE4U(Stream stream, uint val)
  { stream.WriteByte((byte)(val>>24));
    stream.WriteByte((byte)(val>>16));
    stream.WriteByte((byte)(val>>8));
    stream.WriteByte((byte)val);
  }

  public static void WriteBE4U(byte[] buf, int index, uint val)
  { buf[index]   = (byte)(val>>24);
    buf[index+1] = (byte)(val>>16);
    buf[index+2] = (byte)(val>>8);
    buf[index+3] = (byte)val;
  }

  public static void WriteLE8U(Stream stream, ulong val)
  { WriteLE4U(stream, (uint)val);
    WriteLE4U(stream, (uint)(val>>32));
  }

  public static void WriteLE8U(byte[] buf, int index, ulong val)
  { WriteLE4U(buf, index, (uint)val);
    WriteLE4U(buf, index+4, (uint)(val>>32));
  }
  
  public static void WriteBE8U(Stream stream, ulong val)
  { WriteLE4U(stream, (uint)(val>>32));
    WriteLE4U(stream, (uint)val);
  }

  public static void WriteBE8U(byte[] buf, int index, ulong val)
  { WriteLE4U(buf, index, (uint)(val>>32));
    WriteLE4U(buf, index+4, (uint)val);
  }

  public static void WriteFloat(Stream stream, float val)
  { unsafe
    { byte[] buf = new byte[sizeof(float)];
      fixed(byte* pbuf=buf) *(float*)pbuf = val;
      stream.Write(buf, 0, sizeof(float));
    }
  }
  
  public static void WriteFloat(byte[] buf, int index, float val)
  { unsafe { fixed(byte* pbuf=buf) *(float*)(pbuf+index) = val; }
  }

  public static void WriteDouble(Stream stream, double val)
  { unsafe
    { byte[] buf = new byte[sizeof(double)];
      fixed(byte* pbuf=buf) *(double*)pbuf = val;
      stream.Write(buf, 0, sizeof(double));
    }
  }
  
  public static void WriteDouble(byte[] buf, int index, double val)
  { unsafe { fixed(byte* pbuf=buf) *(double*)(pbuf+index) = val; }
  }
}

} // namespace GameLib.IO