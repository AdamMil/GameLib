/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2005 Adam Milazzo

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

// TODO: clean up code for binary formatted output (it's ugly)
// TODO: allow a '*' prefix in binary reading that uses the number as the prefix for the next code, so we can read arrays without breaking up the format string

namespace GameLib.IO
{

#region StreamStream
/// <summary>This class provides a stream based on a portion of an existing stream.</summary>
/// <remarks>Many methods taking stream arguments expect the entire range of the stream to be devoted to the data
/// being read by that method. You may want to use a stream containing other data as well with one of those methods.
/// This class allows you to create a stream from a section of another stream. This class is not thread-safe and
/// should not be used by multiple threads simultaneously.
/// </remarks>
public class StreamStream : Stream, IDisposable
{ 
  /// <param name="stream">The underlying stream. It is assumed that the underlying stream will not be used by other
  /// code while this stream is in use. The underlying stream will not be closed automatically when this stream is
  /// closed.
  /// </param>
  /// <remarks>If you want to use the underlying stream in other code while this stream is in use, use one of the
  /// other constructors, such as <see cref="StreamStream(Stream,long,long,bool)"/>.
  /// </remarks>
  /// <include file='documentation.xml' path='//IO/StreamStream/StreamStream/*'/>
  public StreamStream(Stream stream, long start, long length) : this(stream, start, length, false, false) { }
  /// <param name="stream">The underlying stream. The underlying stream will not be closed automatically when this
  /// stream is closed. Unseekable streams cannot be shared (<paramref name="shared"/> must be false).
  /// </param>
  /// <param name="shared">If set to true, the underlying stream will be seeked to the expected position before
  /// each operation in case some other code has moved the file pointer. If set to false, it is assumed that other
  /// code will not touch the underlying stream while this stream is in use, so the additional seeking can be avoided.
  /// </param>
  /// <include file='documentation.xml' path='//IO/StreamStream/StreamStream/*'/>
  public StreamStream(Stream stream, long start, long length, bool shared)
    : this(stream, start, length, shared, false) { }
  /// <param name="stream">The underlying stream. Unseekable streams cannot be shared (<paramref name="shared"/> must
  /// be false).
  /// </param>
  /// <param name="shared">If set to true, the underlying stream will be seeked to the expected position before
  /// each operation in case some other code has moved the file pointer. If set to false, it is assumed that other
  /// code will not touch the underlying stream while this stream is in use, so the additional seeking can be avoided.
  /// </param>
  /// <param name="closeInner">If set to true, the underlying stream will be closed automatically when this stream
  /// is closed.
  /// </param>
  /// <include file='documentation.xml' path='//IO/StreamStream/StreamStream/*'/>
  public StreamStream(Stream stream, long start, long length, bool shared, bool closeInner)
  { if(stream==null) throw new ArgumentNullException("stream");
    if(start<0 || length<0) throw new ArgumentOutOfRangeException("'start' or 'length'", "cannot be negative");
    if(stream.CanSeek) stream.Position = start;
    else
    { if(shared) throw new ArgumentException("If using an unseekable stream, 'shared' must be false");
      if(stream.Position > start+length)
        throw new ArgumentException("The stream is unseekable and its Position is already past the end of the range.");
      if(stream.Position < start) IOH.Skip(stream, start-stream.Position);
      else if(stream.Position > start) position = stream.Position - start;
    }
    this.stream=stream; this.start=start; this.length=length; this.shared=shared; this.closeInner=closeInner;
    if(!closeInner) GC.SuppressFinalize(this);
  }
  /// <summary>The destructor calls <see cref="Dispose"/> to close the stream.</summary>
  ~StreamStream() { Dispose(true); }
  /// <summary>This method closes the stream with <see cref="Close"/>.</summary>
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  /// <summary>Returns true if the underlying stream can be read from.</summary>
  public override bool CanRead { get { AssertOpen(); return stream.CanRead; } }
  /// <summary>Returns true if the underlying stream can be seeked.</summary>
  public override bool CanSeek { get { AssertOpen(); return stream.CanSeek; } }
  /// <summary>Returns true if the underlying stream can be written to.</summary>
  public override bool CanWrite { get { AssertOpen(); return stream.CanWrite; } }

  /// <summary>Returns the length of this stream.</summary>
  public override long Length { get { AssertOpen(); return length; } }
  /// <summary>Gets/sets the current position within this stream.</summary>
  /// <remarks>You cannot seek past the end of a <see cref="StreamStream"/>. However, you can first use
  /// <see cref="SetLength"/> to increase the size of the stream and then seek past what was previously the end.
  /// <seealso cref="Seek"/>
  /// </remarks>
  public override long Position { get { AssertOpen(); return position; } set { Seek(value, SeekOrigin.Begin); } }

  /// <summary>Closes the stream.</summary>
  /// <remarks>If this stream was constructed with the <c>closeInner</c> parameter set to true, the underlying
  /// stream will be closed as well.
  /// </remarks>
  public override void Close()
  { if(stream==null) return;
    if(closeInner) stream.Close();
    stream=null;
  }
  /// <summary>Flushes the underlying stream.</summary>
  public override void Flush() { AssertOpen(); stream.Flush(); }

  /// <summary>Reads a sequence of bytes from the underlying stream and advances the position within this stream by
  /// the number of bytes read.
  /// </summary>
  /// <param name="buffer">An array of bytes into which data will be read.</param>
  /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data
  /// read from the underlying stream.
  /// </param>
  /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
  /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested
  /// if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
  /// </returns>
  public override int Read(byte[] buffer, int offset, int count)
  { AssertOpen();
    if(shared) stream.Position = position+start;
    if(count>length-position) count = (int)(length-position);
    int ret = stream.Read(buffer, offset, count);
    position += ret;
    return ret;
  }

  /// <summary>Reads a byte from the stream and advances the position within this stream by one byte, or returns -1
  /// if at the end of the stream.
  /// </summary>
  /// <returns>The unsigned byte cast to an integer, or -1 if at the end of the stream.</returns>
  public override int ReadByte()
  { AssertOpen();
    if(position>=length) return -1;
    if(shared) stream.Position = position+start;
    int ret = stream.ReadByte();
    if(ret!=-1) position++;
    return ret;
  }

  /// <summary>Sets the position within the current stream.</summary>
  /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
  /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain
  /// the new position.
  /// </param>
  /// <returns>The new position within the current stream.</returns>
  /// <remarks>You cannot seek past the end of a <see cref="StreamStream"/>. However, you can first use
  /// <see cref="SetLength"/> to increase the size of the stream and then seek past what was previously the end.
  /// <seealso cref="Position"/>
  /// </remarks>
  public override long Seek(long offset, SeekOrigin origin)
  { AssertOpen();
    if(origin==SeekOrigin.Current) offset+=position;
    else if(origin==SeekOrigin.End) offset+=length;
    if(offset<0 || offset>length)
      throw new ArgumentOutOfRangeException("Cannot seek outside the bounds of this stream.");
    return position = stream.Seek(start+offset, SeekOrigin.Begin)-start;
  }

  /// <summary>Sets the length of the current stream.</summary>
  /// <param name="length">The desired length of the current stream in bytes.</param>
  /// <remarks>This method does not check the length of the underlying stream, nor does it call
  /// <see cref="SetLength"/> on the underlying stream. If, after altering this stream's length,
  /// <see cref="Position"/> would be past the end of the range, it will be set to the end of the new range. For
  /// unseekable streams, the position cannot be adjusted, so an exception will be thrown if the new length would
  /// require seeking an unseekable stream.
  /// </remarks>
  /// <exception cref="ArgumentException">Thrown if the underlying stream cannot seek (<see cref="CanSeek"/>
  /// is false) and the new length would require the <see cref="Position"/> property to be adjusted to be within the
  /// new range.
  /// </exception>
  public override void SetLength(long length)
  { AssertOpen();
    if(!stream.CanSeek && position>length)
      throw new ArgumentException("The underlying stream is unseekable and setting the length to this value would "+
                                  "require seeking.", "length");
    this.length=length;
    if(position>length) Position=length;
  }

  /// <summary>Writes a sequence of bytes to the underlying stream and advances the current position by the number of
  /// bytes written.
  /// </summary>
  /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes starting from
  /// <paramref name="offset"/> to the underlying stream.
  /// </param>
  /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to
  /// the underlying stream.
  /// </param>
  /// <param name="count">The number of bytes to be written to the underlying stream.</param>
  /// <remarks>You cannot write past the end of a <see cref="StreamStream"/>. However, you can first use
  /// <see cref="SetLength"/> to increase the size of the stream and then write data past what was previously the end.
  /// </remarks>
  public override void Write(byte[] buffer, int offset, int count)
  { AssertOpen();
    if(count>length-position)
      throw new ArgumentException("Cannot write past the end of a StreamStream (try resizing with SetLength first?)");
    if(shared) stream.Position = position+start;
    stream.Write(buffer, offset, count);
    position = stream.Position-start;
  }

  /// <summary>Writes a byte to the current position in the stream and advances the current position by one byte.</summary>
  /// <param name="value">The byte to write to the stream.</param>
  /// <remarks>You cannot write past the end of a <see cref="StreamStream"/>. However, you can first use
  /// <see cref="SetLength"/> to increase the size of the stream and then write data past what was previously the end.
  /// </remarks>
  public override void WriteByte(byte value)
  { AssertOpen();
    if(position>=length) throw new InvalidOperationException("Cannot write past the end of a StreamStream "+
                                                             "(try resizing with SetLength first?)");
    if(shared) stream.Position = position+start;
    stream.WriteByte(value);
    position++;
  }

  /// <summary>Gets the underlying stream.</summary>
  /// <value>The underlying <see cref="Stream"/> object, or null if this stream is closed.</value>
  protected Stream InnerStream { get { return stream; } }

  /// <summary>Throws an exception if the stream is not open.</summary>
  /// <exception cref="InvalidOperationException">Thrown if this stream has been closed.</exception>
  protected void AssertOpen()
  { if(stream==null) throw new InvalidOperationException("The inner stream was already closed.");
  }

  /// <summary>Disposes resources used by this stream.</summary>
  /// <param name="finalizing">True if this method is being called from a finalizer and false otherwise.</param>
  /// <remarks>If overriden in a derived class, remember to call the base implementation.</remarks>
  protected void Dispose(bool finalizing) { Close(); }

  Stream stream;
  long start, length, position;
  bool shared, closeInner;
}
#endregion

#region IOH
/// <summary>This class provides helpers for stream, console, and buffer IO.</summary>
public sealed class IOH
{ private IOH() {}

  #region Console input
  /// <summary>Returns true if console input is waiting to be read.</summary>
  public static bool KbHit { get { return GameLib.Interop.GLUtility.Utility.KbHit(); } }
  /// <summary>Reads a character from standard input without using line buffering.</summary>
  /// <returns>The next character from standard input.</returns>
  /// <remarks>The standard <see cref="Console.Read"/> method uses line buffering, which means that it will not return
  /// until the user presses Enter on the keyboard. This method does not use line buffering, so it returns as soon as
  /// the user presses a character key. This method does not echo the character back to the user.
  /// </remarks>
  public static char Getch() { return GameLib.Interop.GLUtility.Utility.Getch(); }
  /// <summary>Reads a character from standard input without using line buffering and echos it to standard output.</summary>
  /// <returns>The next character from standard input.</returns>
  /// <remarks>The standard <see cref="Console.Read"/> method uses line buffering, which means that it will not return
  /// until the user presses Enter on the keyboard. This method does not use line buffering, so it returns as soon as
  /// the user presses a character key. This also echos the character back to standard output.
  /// </remarks>
  public static char Getche() { return GameLib.Interop.GLUtility.Utility.Getche(); }
  #endregion

  #region CalculateOutputs
  /// <summary>Calculates how many objects would be read by <see cref="Read(Stream,string)"/> and the like.</summary>
  /// <param name="format">The format string to use to calculate the number of objects.</param>
  /// <returns>The number of objects that would be read from the data source.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static int CalculateOutputs(string format)
  { if(format==null) throw new ArgumentNullException("format");
    int num=0;
    for(int i=0,flen=format.Length; i<flen; i++)
    { char c = format[i];
      switch(c)
      { case 'b': case 'B': case 'w': case 'W': case 'd': case 'D': case 'f': case 'F':
        case 'q': case 'Q': case 'c': case 'p': case 's':
          num++; break;
        case 'A': case 'U': case '<': case '>': case '=': break;
        case '?': throw new ArgumentException("Objects of unknown size (prefix='?') are not supported.", "format");
        default:
          if(char.IsDigit(c) || char.IsWhiteSpace(c)) break;
          throw new ArgumentException(string.Format("Unknown character '{0}'", c), "format");
      }
    }
    return num;
  }
  #endregion

  #region CalculateSize
  /// <summary>This method calculates how many bytes would be written by <see cref="Write(Stream,string,object[])"/>
  /// etc if given the specified format string and variable-length arguments to match.
  /// </summary>
  /// <param name="format">The format string to use to calculate the length.</param>
  /// <param name="parms">The variable-width parameters where a prefix was not given as expected by the formatter.
  /// Fixed-width parameters should not be included. If you want to include fixed-width parameters too, use
  /// <see cref="CalculateSize(bool,string,object[])"/>.
  /// </param>
  /// <returns>The number of bytes that would be written.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static int CalculateSize(string format, params object[] parms) { return CalculateSize(false, format, parms); }

  /// <summary>Calculates how many bytes would be written by <see cref="Write(Stream,string,object[])"/>
  /// etc if given the specified format string and arguments to match.
  /// </summary>
  /// <param name="allParms">If true, this method will expect all parameters to be passed to this method. Otherwise,
  /// it will only expect the variable-length parameters where a prefix was not given.
  /// </param>
  /// <param name="format">The format string to use to calculate the length.</param>
  /// <param name="parms">The parameters expected by the formatter.</param>
  /// <returns>The number of bytes that would be written.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static int CalculateSize(bool allParms, string format, params object[] parms)
  { int  length=0;
    char c;
    bool unicode=false;

    int i=0, j=0;
    try
    { for(int flen=format.Length,prefix; i<flen; i++)
      { c = format[i];
        if(char.IsDigit(c))
        { prefix = c-'0';
          while(++i<flen && char.IsDigit(c=format[i])) prefix = prefix*10 + c-'0';
          if(i==flen) throw new ArgumentException("Missing format code after prefix", "format");
          if(allParms)
          { if(j>=parms.Length) throw new ArgumentException("Not enough arguments");
            if(c=='s' || c=='p' || (parms[j] as Array)!=null) j++;
            else if(c!='x')
            { j+=prefix;
              if(j>parms.Length) throw new ArgumentException("Not enough arguments, or invalid prefix");
            }
          }
        }
        else if(c=='?')
        { if(j>=parms.Length) throw new ArgumentException("Not enough arguments (expecting array/string)!");
          if(++i==flen) throw new ArgumentException("Expected something after '?' at "+i.ToString(), "format");
          c=format[i];
          if(c=='s' || c=='p') prefix = -1;
          else prefix=((Array)parms[j++]).Length;
        }
        else if(c=='s' || c=='p') { prefix = -1; }
        else
        { prefix=1;
          if(allParms) switch(c)
          { case 'x': case 'A': case 'U': case '<': case '>': case '=': break;
            default: j++; break;
          }
        }

        switch(c)
        { case 'b': case 'B': case 'x': length += prefix; break;
          case 'w': case 'W': length += prefix*2; break;
          case 'd': case 'D': case 'f': length += prefix*4; break;
          case 'q': case 'Q': case 'F': length += prefix*8; break;
          case 'c': length += unicode ? prefix*2 : prefix; break;
          case 's':
            if(prefix==-1)
            { if(j>=parms.Length) throw new ArgumentException("Not enough arguments (expecting string)!");
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
            { if(j>=parms.Length) throw new ArgumentException("Not enough arguments (expecting string)!");
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
          default:
            if(char.IsWhiteSpace(c)) break;
            throw new ArgumentException(string.Format("Unknown character '{0}'", c), "format");
        }
      }
    }
    catch(Exception e)
    { throw new ArgumentException(string.Format("Error near char {0}, near parameter {1} -- {2}", i, j, e.Message), e);
    }
    return length;
  }
  #endregion

  #region CopyStream
  /// <summary>Copies a source stream into a destination stream.</summary>
  /// <param name="source">The source stream to copy.</param>
  /// <param name="dest">The destination stream into which the source data will be written.</param>
  /// <returns>The number of bytes copied.</returns>
  /// <remarks>Data is copied from the source stream's current position until the end.</remarks>
  public static int CopyStream(Stream source, Stream dest) { return CopyStream(source, dest, false); }
  /// <summary>Copies a source stream into a destination stream.</summary>
  /// <param name="source">The source stream to copy.</param>
  /// <param name="dest">The destination stream into which the source data will be written.</param>
  /// <param name="rewindSource">If true, the source stream's <see cref="Stream.Position"/> property will be set
  /// to 0 first to ensure that the entire source stream is copied.
  /// </param>
  /// <returns>The number of bytes copied.</returns>
  public static int CopyStream(Stream source, Stream dest, bool rewindSource)
  { if(rewindSource) source.Position=0;
    byte[] buf = new byte[1024];
    int read, total=0;
    while(true)
    { read = source.Read(buf, 0, 1024);
      if(read==0) return total;
      total += read;
      dest.Write(buf, 0, read);
    }
  }
  #endregion

  #region Reading
  /// <summary>Reads the given number of bytes from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <param name="length">The number of bytes to read.</param>
  /// <returns>A byte array containing <paramref name="length"/> bytes of data.</returns>
  /// <remarks>This calls <see cref="Stream.Read"/> repeatedly until enough data can be read or the end of the stream
  /// is reached.
  /// </remarks>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached before all data could be read.
  /// </exception>
  public static byte[] Read(Stream stream, int length)
  { byte[] buf = new byte[length];
    Read(stream, buf, 0, length, true);
    return buf;
  }
  /// <summary>Fills the given buffer with data from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <param name="buf">The array to fill with data.</param>
  /// <returns>The number of bytes read. This will always be equal to the length of the buffer passed in.</returns>
  /// <remarks>This calls <see cref="Stream.Read"/> repeatedly until enough data can be read or the end of the stream
  /// is reached.
  /// </remarks>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached before all data could be read.
  /// </exception>
  public static int Read(Stream stream, byte[] buf) { return Read(stream, buf, 0, buf.Length, true); }
  /// <summary>Reads the given number of bytes from a stream into a buffer.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <param name="buf">The array in which the data will be stored.</param>
  /// <param name="length">The number of bytes to read.</param>
  /// <returns>The number of bytes read. This will always be equal to <paramref name="length"/>.</returns>
  /// <remarks>This calls <see cref="Stream.Read"/> repeatedly until enough data can be read or the end of the stream
  /// is reached.
  /// </remarks>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached before all data could be read.
  /// </exception>
  public static int Read(Stream stream, byte[] buf, int length) { return Read(stream, buf, 0, length, true); }
  /// <summary>Reads the given number of bytes from a stream into a buffer.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <param name="buf">The array in which the data will be stored.</param>
  /// <param name="offset">The offset into the buffer at which data will be stored.</param>
  /// <param name="length">The number of bytes to read.</param>
  /// <returns>The number of bytes read. This will always be equal to <paramref name="length"/>.</returns>
  /// <remarks>This calls <see cref="Stream.Read"/> repeatedly until enough data can be read or the end of the stream
  /// is reached.
  /// </remarks>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached before all data could be read.
  /// </exception>
  public static int Read(Stream stream, byte[] buf, int offset, int length)
  { return Read(stream, buf, 0, length, true);
  }
  /// <summary>Tries to fill the given buffer with data from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <param name="buf">The array in which the data will be stored.</param>
  /// <param name="throwOnEOF">If true, this method will throw <see cref="EndOfStreamException"/> if not all data
  /// could be read.
  /// </param>
  /// <returns>The number of bytes read.</returns>
  /// <remarks>This calls <see cref="Stream.Read"/> repeatedly until enough data can be read or the end of the stream
  /// is reached.
  /// </remarks>
  /// <exception cref="EndOfStreamException">Thrown if <paramref name="throwOnEOF"/> is true and the end of the
  /// stream is reached before all data could be read.
  /// </exception>
  public static int Read(Stream stream, byte[] buf, bool throwOnEOF)
  { return Read(stream, buf, 0, buf.Length, throwOnEOF);
  }
  /// <summary>Tries to read the given number of bytes from a stream into a buffer.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <param name="buf">The array in which the data will be stored.</param>
  /// <param name="length">The maximum number of bytes to read.</param>
  /// <param name="throwOnEOF">If true, this method will throw <see cref="EndOfStreamException"/> if not all data
  /// could be read.
  /// </param>
  /// <returns>The number of bytes read.</returns>
  /// <remarks>This calls <see cref="Stream.Read"/> repeatedly until enough data can be read or the end of the stream
  /// is reached.
  /// </remarks>
  /// <exception cref="EndOfStreamException">Thrown if <paramref name="throwOnEOF"/> is true and the end of the
  /// stream is reached before all data could be read.
  /// </exception>
  public static int Read(Stream stream, byte[] buf, int length, bool throwOnEOF)
  { return Read(stream, buf, 0, length, throwOnEOF);
  }
  /// <summary>Tries to read the given number of bytes from a stream into a buffer.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <param name="buf">The array in which the data will be stored.</param>
  /// <param name="offset">The offset into the buffer at which data will be stored.</param>
  /// <param name="length">The maximum number of bytes to read.</param>
  /// <param name="throwOnEOF">If true, this method will throw <see cref="EndOfStreamException"/> if not all data
  /// could be read.
  /// </param>
  /// <returns>The number of bytes read.</returns>
  /// <remarks>This calls <see cref="Stream.Read"/> repeatedly until enough data can be read or the end of the stream
  /// is reached.
  /// </remarks>
  /// <exception cref="EndOfStreamException">Thrown if <paramref name="throwOnEOF"/> is true and the end of the
  /// stream is reached before all data could be read.
  /// </exception>
  public static int Read(Stream stream, byte[] buf, int offset, int length, bool throwOnEOF)
  { int read=0, total=0;
    while(true)
    { read = stream.Read(buf, offset+read, length-read);
      total += read;
      if(total==length) return length;
      if(read==0)
      { if(throwOnEOF) throw new EndOfStreamException();
        return total;
      }
    }
  }

  /// <summary>Reads the given number of bytes from the stream and converts them into a string.</summary>
  /// <param name="stream">The stream from which the string will be read.</param>
  /// <param name="length">The number of bytes to read.</param>
  /// <returns>The string read from the stream.</returns>
  /// <remarks>This method assumes the string is stored in ASCII encoding.</remarks>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached before all data could be read.
  /// </exception>
  public static string ReadString(Stream stream, int length)
  { return ReadString(stream, length, System.Text.Encoding.ASCII);
  }
  /// <summary>Reads the given number of bytes from the stream and converts them to a string.</summary>
  /// <param name="stream">The stream from which the string will be read.</param>
  /// <param name="length">The number of bytes to read.</param>
  /// <param name="encoding">The character encoding to use to decode the string.</param>
  /// <returns>The string read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached before all data could be read.
  /// </exception>
  public static string ReadString(Stream stream, int length, System.Text.Encoding encoding)
  { return encoding.GetString(Read(stream, length));
  }

  /// <summary>Reads the next byte from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The byte value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before the byte could be read.
  /// </exception>
  public static byte Read1(Stream stream)
  { int i = stream.ReadByte();
    if(i==-1) throw new EndOfStreamException();
    return (byte)i;
  }

  /// <summary>Reads a little-endian short (2 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public static short ReadLE2(Stream stream) { return (short)(Read1(stream)|(Read1(stream)<<8)); }

  /// <summary>Reads a little-endian short (2 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public static short ReadLE2(byte[] buf, int index) { return (short)(buf[index]|(buf[index+1]<<8)); }

  /// <summary>Reads a big-endian short (2 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public static short ReadBE2(Stream stream) { return (short)((Read1(stream)<<8)|Read1(stream)); }

  /// <summary>Reads a big-endian short (2 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public static short ReadBE2(byte[] buf, int index) { return (short)((buf[index]<<8)|buf[index+1]); }

  /// <summary>Reads a little-endian integer (4 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public static int ReadLE4(Stream stream)
  { return (int)(Read1(stream)|(Read1(stream)<<8)|(Read1(stream)<<16)|(Read1(stream)<<24));
  }

  /// <summary>Reads a little-endian integer (4 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public static int ReadLE4(byte[] buf, int index)
  { return (int)(buf[index]|(buf[index+1]<<8)|(buf[index+2]<<16)|(buf[index+3]<<24));
  }

  /// <summary>Reads a big-endian integer (4 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public static int ReadBE4(Stream stream)
  { return (int)((Read1(stream)<<24)|(Read1(stream)<<16)|(Read1(stream)<<8)|Read1(stream));
  }

  /// <summary>Reads a big-endian integer (4 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public static int ReadBE4(byte[] buf, int index)
  { return (int)((buf[index]<<24)|(buf[index+1]<<16)|(buf[index+2]<<8)|buf[index+3]);
  }

  /// <summary>Reads a little-endian long (8 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public static long ReadLE8(Stream stream)
  { byte[] buf = Read(stream, 8);
    return ReadLE4U(buf, 0)|((long)ReadLE4(buf, 4)<<32);
  }

  /// <summary>Reads a little-endian long (8 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public static long ReadLE8(byte[] buf, int index) { return ReadLE4U(buf, index)|((long)ReadLE4(buf, index+4)<<32); }

  /// <summary>Reads a big-endian long (8 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public static long ReadBE8(Stream stream)
  { byte[] buf = Read(stream, 8);
    return ((long)ReadBE4(buf, 0)<<32)|ReadBE4U(buf, 4);
  }

  /// <summary>Reads a big-endian long (8 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public static long ReadBE8(byte[] buf, int index)
  { return ((long)ReadBE4(buf, index)<<32)|(uint)ReadBE4(buf, index+4);
  }

  /// <summary>Reads a little-endian unsigned short (2 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public static ushort ReadLE2U(Stream stream) { return (ushort)(Read1(stream)|(Read1(stream)<<8)); }

  /// <summary>Reads a little-endian unsigned short (2 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public static ushort ReadLE2U(byte[] buf, int index) { return (ushort)(buf[index]|(buf[index+1]<<8)); }

  /// <summary>Reads a big-endian unsigned short (2 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public static ushort ReadBE2U(Stream stream) { return (ushort)((Read1(stream)<<8)|Read1(stream)); }

  /// <summary>Reads a big-endian unsigned short (2 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public static ushort ReadBE2U(byte[] buf, int index) { return (ushort)((buf[index]<<8)|buf[index+1]); }

  /// <summary>Reads a little-endian unsigned integer (4 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public static uint ReadLE4U(Stream stream)
  { return (uint)(Read1(stream)|(Read1(stream)<<8)|(Read1(stream)<<16)|(Read1(stream)<<24));
  }

  /// <summary>Reads a little-endian unsigned integer (4 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public static uint ReadLE4U(byte[] buf, int index)
  { return (uint)(buf[index]|(buf[index+1]<<8)|(buf[index+2]<<16)|(buf[index+3]<<24));
  }

  /// <summary>Reads a big-endian unsigned integer (4 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public static uint ReadBE4U(Stream stream)
  { return (uint)((Read1(stream)<<24)|(Read1(stream)<<16)|(Read1(stream)<<8)|Read1(stream));
  }

  /// <summary>Reads a big-endian unsigned integer (4 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public static uint ReadBE4U(byte[] buf, int index)
  { return (uint)((buf[index]<<24)|(buf[index+1]<<16)|(buf[index+2]<<8)|buf[index+3]);
  }

  /// <summary>Reads a little-endian unsigned long (8 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public static ulong ReadLE8U(Stream stream)
  { byte[] buf = Read(stream, 8);
    return ReadLE4U(buf, 0)|((ulong)ReadLE4U(buf, 4)<<32);
  }

  /// <summary>Reads a little-endian unsigned long (8 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public static ulong ReadLE8U(byte[] buf, int index)
  { return ReadLE4U(buf, index)|((ulong)ReadLE4U(buf, index+4)<<32);
  }

  /// <summary>Reads a big-endian unsigned long (8 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public static ulong ReadBE8U(Stream stream)
  { byte[] buf = Read(stream, 8);
    return ((ulong)ReadBE4U(buf, 0)<<32)|ReadBE4U(buf, 4);
  }

  /// <summary>Reads a big-endian unsigned long (8 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public static ulong ReadBE8U(byte[] buf, int index)
  { return ((ulong)ReadBE4U(buf, index)<<32)|ReadBE4U(buf, index+4);
  }

  /// <summary>Reads an IEEE754 float (4 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public unsafe static float ReadFloat(Stream stream)
  { byte* buf = stackalloc byte[4];
    buf[0]=Read1(stream); buf[1]=Read1(stream); buf[2]=Read1(stream); buf[3]=Read1(stream);
    return *(float*)buf;
  }

  /// <summary>Reads an IEEE754 float (4 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public unsafe static float ReadFloat(byte[] buf, int index)
  { fixed(byte* ptr=buf) return *(float*)(ptr+index);
  }

  /// <summary>Reads an IEEE754 double (8 bytes) from a stream.</summary>
  /// <param name="stream">The stream to read from.</param>
  /// <returns>The value read from the stream.</returns>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all data could be read.
  /// </exception>
  public unsafe static double ReadDouble(Stream stream)
  { byte[] buf = Read(stream, sizeof(double));
    fixed(byte* ptr=buf) return *(double*)ptr;
  }

  /// <summary>Reads an IEEE754 double (8 bytes) from a byte array.</summary>
  /// <param name="buf">The byte array to read from.</param>
  /// <param name="index">The index from which to begin reading.</param>
  /// <returns>The value read from the array.</returns>
  public unsafe static double ReadDouble(byte[] buf, int index)
  { fixed(byte* ptr=buf) return *(double*)(ptr+index);
  }
  #endregion

  #region Skip
  /// <summary>Skips forward a number of bytes in a stream.</summary>
  /// <param name="stream">The stream to seek forward in.</param>
  /// <param name="bytes">The number of bytes to skip.</param>
  /// <remarks>This method works on both seekable and non-seekable streams, but is more efficient with seekable ones.</remarks>
  /// <exception cref="EndOfStreamException">Thrown if the end of the stream was reached before all bytes could be
  /// skipped.
  /// </exception>
  public static void Skip(Stream stream, long bytes)
  { if(bytes<0) throw new ArgumentException("cannot be negative", "bytes");
    if(stream.CanSeek) stream.Position += bytes;
    else if(bytes<8) { int b = (int)bytes; while(b-- > 0) Read1(stream); } // these numbers are just chosen at random. they should probably be optimized
    else if(bytes<=512) Read(stream, (int)bytes);
    else
    { byte[] buf = new byte[512];
      int read;
      while(bytes>0)
      { read = stream.Read(buf, 0, (int)Math.Min(bytes, 512));
        if(read==0) throw new EndOfStreamException();
        bytes -= read;
      }
    }
  }
  #endregion

  #region Formatted binary read
  /// <summary>Reads formatted binary data from a stream.</summary>
  /// <param name="stream">The <see cref="Stream"/> to read from.</param>
  /// <param name="format">The format string controlling how the data will be read.</param>
  /// <returns>An array of <see cref="System.Object"/> containing the data read.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static object[] Read(Stream stream, string format)
  { int dummy;
    return Read(stream, format, out dummy);
  }
  /// <summary>Reads formatted binary data from a stream.</summary>
  /// <param name="stream">The <see cref="Stream"/> to read from.</param>
  /// <param name="format">The format string controlling how the data will be read.</param>
  /// <param name="bytesRead">An integer in which the number of bytes read from <paramref name="stream"/>
  /// will be stored.
  /// </param>
  /// <returns>An array of <see cref="System.Object"/> containing the data read.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static object[] Read(Stream stream, string format, out int bytesRead)
  { object[] ret = new object[CalculateOutputs(format)];
    bytesRead = Read(stream, format, ret, 0);
    return ret;
  }
  /// <summary>Reads formatted binary data from a stream.</summary>
  /// <param name="stream">The <see cref="Stream"/> to read from.</param>
  /// <param name="format">The format string controlling how the data will be read.</param>
  /// <param name="output">An array of <see cref="System.Object"/> into which the data will be read.</param>
  /// <returns>The number of bytes read from <paramref name="stream"/>.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static int Read(Stream stream, string format, object[] output) { return Read(stream, format, output, 0); }
  /// <summary>Reads formatted binary data from a stream.</summary>
  /// <param name="stream">The <see cref="Stream"/> to read from.</param>
  /// <param name="format">The format string controlling how the data will be read.</param>
  /// <param name="output">An array of <see cref="System.Object"/> into which the data will be read.</param>
  /// <param name="index">The index into <paramref name="output"/> at which writing will begin.</param>
  /// <returns>The number of bytes read from <paramref name="stream"/>.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static int Read(Stream stream, string format, object[] output, int index)
  { if(stream==null || output==null) throw new ArgumentNullException();
    if(index<0 || index+CalculateOutputs(format)>output.Length) throw new ArgumentOutOfRangeException();

    int i=0, length=0;
    char c;
    bool bigendian=!BitConverter.IsLittleEndian, unicode=false;
    try
    { for(int flen=format.Length,prefix; i<flen; i++)
      { c = format[i];
        if(char.IsDigit(c))
        { prefix = c-'0';
          while(++i<flen && char.IsDigit(c=format[i])) prefix = prefix*10 + c-'0';
          if(i==flen) throw new ArgumentException("Missing operator at "+i.ToString(), "format");
        }
        else if(c=='s') throw new ArgumentException("Strings of unknown size are not supported.", "format");
        else prefix=-1;
        if(prefix==0) continue;
        
        switch(c)
        { case 'x':
            if(prefix==-1) { length++; IOH.Read1(stream); }
            else
            { length += prefix;
              IOH.Skip(stream, prefix);
            }
            break;

          case 'b':
            if(prefix==-1) { output[index++]=(sbyte)Read1(stream); length++; }
            else
            { sbyte[] arr = new sbyte[prefix];
              if(prefix<8) for(int j=0; j<prefix; j++) arr[j] = (sbyte)Read1(stream);
              else
                unsafe
                { byte[] buf = Read(stream, prefix);
                  fixed(byte* src=buf) fixed(sbyte* dest=arr) Interop.Unsafe.Copy(src, dest, prefix);
                }
              output[index++] = arr;
              length += prefix;
            }
            break;

          case 'B':
            output[index++] = (prefix==-1 ? Read1(stream) : (object)Read(stream, prefix));
            length += prefix==-1 ? 1 : prefix;
            break;

          case 'w':
            if(prefix==-1) { output[index++]=bigendian ? ReadBE2(stream) : ReadLE2(stream); length+=2; }
            else
            { short[] arr = new short[prefix];
              if(bigendian) for(int j=0; j<prefix; j++) arr[j] = ReadBE2(stream);
              else for(int j=0; j<prefix; j++) arr[j] = ReadLE2(stream);
              output[index++] = arr;
              length += prefix*2;
            }
            break;

          case 'W':
            if(prefix==-1) { output[index++]=bigendian ? ReadBE2U(stream) : ReadLE2U(stream); length+=2; }
            else
            { ushort[] arr = new ushort[prefix];
              if(bigendian) for(int j=0; j<prefix; j++) arr[j] = ReadBE2U(stream);
              else for(int j=0; j<prefix; j++) arr[j] = ReadLE2U(stream);
              output[index++] = arr;
              length += prefix*2;
            }
            break;

          case 'd':
            if(prefix==-1) { output[index++]=bigendian ? ReadBE4(stream) : ReadLE4(stream); length += 4; }
            else
            { int[] arr = new int[prefix];
              if(bigendian) for(int j=0; j<prefix; j++) arr[j] = ReadBE4(stream);
              else for(int j=0; j<prefix; j++) arr[j] = ReadLE4(stream);
              output[index++] = arr;
              length += prefix*4;
            }
            break;

          case 'D':
            if(prefix==-1) { output[index++]=bigendian ? ReadBE4U(stream) : ReadLE4U(stream); length += 4; }
            else
            { uint[] arr = new uint[prefix];
              if(bigendian) for(int j=0; j<prefix; j++) arr[j] = ReadBE4U(stream);
              else for(int j=0; j<prefix; j++) arr[j] = ReadLE4U(stream);
              output[index++] = arr;
              length += prefix*4;
            }
            break;

          case 'f':
            if(prefix==-1) { output[index++]=ReadFloat(stream); length += 4; }
            else
            { float[] arr = new float[prefix];
              for(int j=0; j<prefix; j++) arr[j] = ReadFloat(stream);
              output[index++] = arr;
              length += prefix*4;
            }
            break;

          case 'F':
            if(prefix==-1) { output[index++]=ReadDouble(stream); length += 8; }
            else
            { double[] arr = new double[prefix];
              for(int j=0; j<prefix; j++) arr[j] = ReadDouble(stream);
              output[index++] = arr;
              length += prefix*8;
            }
            break;

          case 'q':
            if(prefix==-1) { output[index++]=bigendian ? ReadBE8(stream) : ReadLE8(stream); length += 8; }
            else
            { long[] arr = new long[prefix];
              if(bigendian) for(int j=0; j<prefix; j++) arr[j] = ReadBE8(stream);
              else for(int j=0; j<prefix; j++) arr[j] = ReadLE8(stream);
              output[index++] = arr;
              length += prefix*8;
            }
            break;

          case 'Q':
            if(prefix==-1) { output[index++]=bigendian ? ReadBE8U(stream) : ReadLE8U(stream); length += 8; }
            else
            { ulong[] arr = new ulong[prefix];
              if(bigendian) for(int j=0; j<prefix; j++) arr[j] = ReadBE8U(stream);
              else for(int j=0; j<prefix; j++) arr[j] = ReadLE8U(stream);
              output[index++] = arr;
              length += prefix*8;
            }
            break;

          case 'c':
            if(prefix==-1)
            { if(unicode) { output[index++]=bigendian ? (char)ReadBE2U(stream) : (char)ReadLE2U(stream); length+=2; }
              else { output[index++]=(char)Read1(stream); length++; }
            }
            else
            { char[] arr = new char[prefix];
              if(unicode)
              { if(bigendian) for(int j=0; j<prefix; j++) arr[j] = (char)ReadBE2U(stream);
                else for(int j=0; j<prefix; j++) arr[j] = (char)ReadLE2U(stream);
                length += prefix*2;
              }
              else
              { for(int j=0; j<prefix; j++) arr[j] = (char)Read1(stream);
                length += prefix;
              }
              output[index++] = arr;
            }
            break;

          case 's':
            if(unicode)
              unsafe
              { char* arr = stackalloc char[prefix];
                if(bigendian) for(int j=0; j<prefix; j++) arr[j] = (char)ReadBE2U(stream);
                else for(int j=0; j<prefix; j++) arr[j] = (char)ReadLE2U(stream);
                output[index++] = new string(arr, 0, prefix);
                length += prefix*2;
              }
            else
            { output[index++] = System.Text.Encoding.ASCII.GetString(Read(stream, prefix));
              length += prefix;
            }
            break;

          case 'p':
          { int len=Read1(stream), slen;
            if(prefix==-1) { prefix=len; slen=0; }
            else if(len<prefix) throw new ArgumentException("Prefix is greater than length of pascal string.");
            else slen = len-prefix;

            if(unicode)
              unsafe
              { char* arr = stackalloc char[prefix];
                if(bigendian) for(int j=0; j<prefix; j++) arr[j] = (char)ReadBE2U(stream);
                else for(int j=0; j<prefix; j++) arr[j] = (char)ReadLE2U(stream);
                output[index++] = new string(arr, 0, prefix);
                length += len*2;
                slen *= 2;
              }
            else
            { output[index++] = System.Text.Encoding.ASCII.GetString(Read(stream, prefix));
              length += len;
            }
            if(slen!=0) IOH.Skip(stream, slen*2);
            break;
          }

          case 'A': unicode=false; break;
          case 'U': unicode=true; break;
          case '<': bigendian=false; break;
          case '>': bigendian=true; break;
          case '=': bigendian=!BitConverter.IsLittleEndian; break;
          default:
            if(char.IsWhiteSpace(c)) break;
            throw new ArgumentException(string.Format("Unknown character '{0}'", c), "format");
        }
      }
    }
    catch(Exception e)
    { throw new ArgumentException(string.Format("Error near char {0}, near output {1} -- {2}", i, index, e.Message), e);
    }

    return length;
  }
  
  /// <summary>Reads formatted binary data from a byte array.</summary>
  /// <param name="buf">An array of <see cref="System.Byte"/> from which the data will be read.</param>
  /// <param name="format">The format string controlling how the data will be read.</param>
  /// <returns>An array of <see cref="System.Object"/> containing the data read.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static object[] Read(byte[] buf, string format)
  { int dummy;
    return Read(buf, format, out dummy);
  }
  /// <summary>Reads formatted binary data from a byte array.</summary>
  /// <param name="buf">An array of <see cref="System.Byte"/> from which the data will be read.</param>
  /// <param name="format">The format string controlling how the data will be read.</param>
  /// <param name="bytesRead">An integer in which the number of bytes read from <paramref name="buf"/>
  /// will be stored.
  /// </param>
  /// <returns>An array of <see cref="System.Object"/> containing the data read.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static object[] Read(byte[] buf, string format, out int bytesRead)
  { object[] ret = new object[CalculateOutputs(format)];
    bytesRead = Read(buf, 0, format, ret, 0);
    return ret;
  }
  /// <summary>Reads formatted binary data from a byte array.</summary>
  /// <param name="buf">An array of <see cref="System.Byte"/> from which the data will be read.</param>
  /// <param name="format">The format string controlling how the data will be read.</param>
  /// <param name="output">An array of <see cref="System.Object"/> into which the data will be written.</param>
  /// <returns>The number of bytes read from <paramref name="buf"/>.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static int Read(byte[] buf, string format, object[] output)
  { return Read(buf, 0, format, output, 0);
  }
  /// <summary>Reads formatted binary data from a byte array.</summary>
  /// <param name="buf">An array of <see cref="System.Byte"/> from which the data will be read.</param>
  /// <param name="srcIndex">The offset into <paramref name="buf"/> at which reading will begin.</param>
  /// <param name="format">The format string controlling how the data will be read.</param>
  /// <returns>An array of <see cref="System.Object"/> containing the data read.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static object[] Read(byte[] buf, int srcIndex, string format)
  { int dummy;
    return Read(buf, srcIndex, format, out dummy);
  }
  /// <summary>Reads formatted binary data from a byte array.</summary>
  /// <param name="buf">An array of <see cref="System.Byte"/> from which the data will be read.</param>
  /// <param name="srcIndex">The offset into <paramref name="buf"/> at which reading will begin.</param>
  /// <param name="format">The format string controlling how the data will be read.</param>
  /// <param name="bytesRead">An integer in which the number of bytes read from <paramref name="stream"/>
  /// will be stored.
  /// </param>
  /// <returns>An array of <see cref="System.Object"/> containing the data read.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static object[] Read(byte[] buf, int srcIndex, string format, out int bytesRead)
  { object[] ret = new object[CalculateOutputs(format)];
    bytesRead = Read(buf, srcIndex, format, ret);
    return ret;
  }
  /// <summary>Reads formatted binary data from a byte array.</summary>
  /// <param name="buf">An array of <see cref="System.Byte"/> from which the data will be read.</param>
  /// <param name="srcIndex">The offset into <paramref name="buf"/> at which reading will begin.</param>
  /// <param name="format">The format string controlling how the data will be read.</param>
  /// <param name="output">An array of <see cref="System.Object"/> into which the data will be written.</param>
  /// <returns>The number of bytes read from <paramref name="buf"/>.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static int Read(byte[] buf, int srcIndex, string format, object[] output)
  { return Read(buf, srcIndex, format, output, 0);
  }
  /// <summary>Reads formatted binary data from a byte array.</summary>
  /// <param name="buf">An array of <see cref="System.Byte"/> from which the data will be read.</param>
  /// <param name="srcIndex">The offset into <paramref name="buf"/> at which reading will begin.</param>
  /// <param name="format">The format string controlling how the data will be read.</param>
  /// <param name="output">An array of <see cref="System.Object"/> into which the data will be written.</param>
  /// <param name="destIndex">The index into <paramref name="output"/> at which writing will begin.</param>
  /// <returns>The number of bytes read from <paramref name="buf"/>.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static unsafe int Read(byte[] buf, int srcIndex, string format, object[] output, int destIndex)
  { if(buf==null || output==null) throw new ArgumentNullException();
    if(srcIndex<0 || destIndex<0 || destIndex+CalculateOutputs(format)>output.Length)
      throw new ArgumentOutOfRangeException();

    int i=0, start=srcIndex;
    char c;
    bool bigendian=!BitConverter.IsLittleEndian, unicode=false;
    try
    { for(int flen=format.Length,prefix; i<flen; i++)
      { c = format[i];
        if(char.IsDigit(c))
        { prefix = c-'0';
          while(++i<flen && char.IsDigit(c=format[i])) prefix = prefix*10 + c-'0';
          if(i==flen) throw new ArgumentException("Missing operator at "+i.ToString(), "format");
        }
        else if(c=='s') throw new ArgumentException("Strings of unknown size are not supported.", "format");
        else prefix=-1;
        if(prefix==0) continue;

        switch(c)
        { case 'x':
            if(prefix==-1) srcIndex+=prefix;
            else srcIndex += prefix;
            break;

          case 'b':
            if(prefix==-1) output[destIndex++] = (sbyte)buf[srcIndex++];
            else
            { sbyte[] arr = new sbyte[prefix];
              fixed(byte* src=buf) fixed(sbyte* dest=arr) Interop.Unsafe.Copy(src+srcIndex, dest, prefix);
              output[destIndex++] = arr;
              srcIndex += prefix;
            }
            break;

          case 'B':
            if(prefix==-1) output[destIndex++] = buf[srcIndex++];
            else
            { byte[] arr = new byte[prefix];
              fixed(byte* src=buf) fixed(byte* dest=arr) Interop.Unsafe.Copy(src+srcIndex, dest, prefix);
              output[destIndex++] = arr;
              srcIndex += prefix;
            }
            break;

          case 'w':
            if(prefix==-1)
            { output[destIndex++] = bigendian ? ReadBE2(buf, srcIndex) : ReadLE2(buf, srcIndex);
              srcIndex += 2;
            }
            else
            { short[] arr = new short[prefix];
              if(bigendian==!BitConverter.IsLittleEndian)
              { int len = prefix*2;
                if(srcIndex+len>buf.Length) throw new ArgumentOutOfRangeException("Insufficient data.");
                fixed(byte* src=buf) fixed(void* dest=arr) Interop.Unsafe.Copy(src, dest, len);
                srcIndex += len;
              }
              else
                for(int j=0; j<prefix; srcIndex+=2,j++)
                  #if BIGENDIAN
                  arr[j] = ReadLE2(buf, srcIndex);
                  #else
                  arr[j] = ReadBE2(buf, srcIndex);
                  #endif

              output[destIndex++] = arr;
            }
            break;

          case 'W':
            if(prefix==-1)
            { output[destIndex++] = bigendian ? ReadBE2U(buf, srcIndex) : ReadLE2U(buf, srcIndex);
              srcIndex += 2;
            }
            else
            { ushort[] arr = new ushort[prefix];
              if(bigendian==!BitConverter.IsLittleEndian)
              { int len = prefix*2;
                if(srcIndex+len>buf.Length) throw new ArgumentOutOfRangeException("Insufficient data.");
                fixed(byte* src=buf) fixed(void* dest=arr) Interop.Unsafe.Copy(src, dest, len);
                srcIndex += len;
              }
              else
                for(int j=0; j<prefix; srcIndex+=2,j++)
                  #if BIGENDIAN
                  arr[j] = ReadLE2U(buf, srcIndex);
                  #else
                  arr[j] = ReadBE2U(buf, srcIndex);
                  #endif

              output[destIndex++] = arr;
            }
            break;

          case 'd':
            if(prefix==-1)
            { output[destIndex++] = bigendian ? ReadBE4(buf, srcIndex) : ReadLE4(buf, srcIndex);
              srcIndex += 4;
            }
            else
            { int[] arr = new int[prefix];
              if(bigendian==!BitConverter.IsLittleEndian)
              { int len = prefix*4;
                if(srcIndex+len>buf.Length) throw new ArgumentOutOfRangeException("Insufficient data.");
                fixed(byte* src=buf) fixed(void* dest=arr) Interop.Unsafe.Copy(src, dest, len);
                srcIndex += len;
              }
              else
                for(int j=0; j<prefix; srcIndex+=4,j++)
                  #if BIGENDIAN
                  arr[j] = ReadLE4(buf, srcIndex);
                  #else
                  arr[j] = ReadBE4(buf, srcIndex);
                  #endif

              output[destIndex++] = arr;
            }
            break;

          case 'D':
            if(prefix==-1)
            { output[destIndex++] = bigendian ? ReadBE4U(buf, srcIndex) : ReadLE4U(buf, srcIndex);
              srcIndex += 4;
            }
            else
            { uint[] arr = new uint[prefix];
              if(bigendian==!BitConverter.IsLittleEndian)
              { int len = prefix*4;
                if(srcIndex+len>buf.Length) throw new ArgumentOutOfRangeException("Insufficient data.");
                fixed(byte* src=buf) fixed(void* dest=arr) Interop.Unsafe.Copy(src, dest, len);
                srcIndex += len;
              }
              else
                for(int j=0; j<prefix; srcIndex+=4,j++)
                  #if BIGENDIAN
                  arr[j] = ReadLE4U(buf, srcIndex);
                  #else
                  arr[j] = ReadBE4U(buf, srcIndex);
                  #endif

              output[destIndex++] = arr;
            }
            break;

          case 'f':
            if(prefix==-1)
            { output[destIndex++] = ReadFloat(buf, srcIndex);
              srcIndex += 4;
            }
            else
            { int len = prefix*4;
              if(srcIndex+len>buf.Length) throw new ArgumentOutOfRangeException("Insufficient data.");
              float[] arr = new float[prefix];
              fixed(byte* src=buf) fixed(void* dest=arr) Interop.Unsafe.Copy(src, dest, len);
              srcIndex += len;
              output[destIndex++] = arr;
            }
            break;

          case 'F':
            if(prefix==-1)
            { output[destIndex++] = ReadDouble(buf, srcIndex);
              srcIndex += 8;
            }
            else
            { int len = prefix*8;
              if(srcIndex+len>buf.Length) throw new ArgumentOutOfRangeException("Insufficient data.");
              double[] arr = new double[prefix];
              fixed(byte* src=buf) fixed(void* dest=arr) Interop.Unsafe.Copy(src, dest, len);
              srcIndex += len;
              output[destIndex++] = arr;
            }
            break;

          case 'q':
            if(prefix==-1)
            { output[destIndex++] = bigendian ? ReadBE8(buf, srcIndex) : ReadLE8(buf, srcIndex);
              srcIndex += 8;
            }
            else
            { long[] arr = new long[prefix];
              if(bigendian==!BitConverter.IsLittleEndian)
              { int len = prefix*8;
                if(srcIndex+len>buf.Length) throw new ArgumentOutOfRangeException("Insufficient data.");
                fixed(byte* src=buf) fixed(void* dest=arr) Interop.Unsafe.Copy(src, dest, len);
                srcIndex += len;
              }
              else
                for(int j=0; j<prefix; srcIndex+=8,j++)
                  #if BIGENDIAN
                  arr[j] = ReadLE8(buf, srcIndex);
                  #else
                  arr[j] = ReadBE8(buf, srcIndex);
                  #endif

              output[destIndex++] = arr;
            }
            break;

          case 'Q':
            if(prefix==-1)
            { output[destIndex++] = bigendian ? ReadBE8U(buf, srcIndex) : ReadLE8U(buf, srcIndex);
              srcIndex += 8;
            }
            else
            { ulong[] arr = new ulong[prefix];
              if(bigendian==!BitConverter.IsLittleEndian)
              { int len = prefix*8;
                if(srcIndex+len>buf.Length) throw new ArgumentOutOfRangeException("Insufficient data.");
                fixed(byte* src=buf) fixed(void* dest=arr) Interop.Unsafe.Copy(src, dest, len);
                srcIndex += len;
              }
              else
                for(int j=0; j<prefix; srcIndex+=8,j++)
                  #if BIGENDIAN
                  arr[j] = ReadLE8U(buf, srcIndex);
                  #else
                  arr[j] = ReadBE8U(buf, srcIndex);
                  #endif

              output[destIndex++] = arr;
            }
            break;

          case 'c':
            if(prefix==-1)
            { if(unicode)
              { output[destIndex++] = bigendian ? (char)ReadBE2U(buf, srcIndex) : (char)ReadLE2U(buf, srcIndex);
                srcIndex += 2;
              }
              else output[destIndex++] = (char)buf[srcIndex++];
            }
            else
            { char[] arr = new char[prefix];
              if(bigendian==!BitConverter.IsLittleEndian)
              { int len = prefix*2;
                if(srcIndex+len>buf.Length) throw new ArgumentOutOfRangeException("Insufficient data.");
                fixed(byte* src=buf) fixed(void* dest=arr) Interop.Unsafe.Copy(src, dest, len);
                srcIndex += len;
              }
              else
                for(int j=0; j<prefix; srcIndex+=2,j++)
                  #if BIGENDIAN
                  arr[j] = (char)ReadLE2U(buf, srcIndex);
                  #else
                  arr[j] = (char)ReadBE2U(buf, srcIndex);
                  #endif

              output[destIndex++] = arr;
            }
            break;

          case 's':
            if(unicode)
            { char* arr = stackalloc char[prefix];

              if(bigendian==!BitConverter.IsLittleEndian)
              { int len = prefix*2;
                if(srcIndex+len>buf.Length) throw new ArgumentOutOfRangeException("Insufficient data.");
                fixed(byte* src=buf) Interop.Unsafe.Copy(src, arr, len);
                srcIndex += len;
              }
              else
                for(int j=0; j<prefix; srcIndex+=2,j++)
                  #if BIGENDIAN
                  arr[j] = (char)ReadLE2U(buf, srcIndex);
                  #else
                  arr[j] = (char)ReadBE2U(buf, srcIndex);
                  #endif

              output[destIndex++] = new string(arr, 0, prefix);
            }
            else
            { output[destIndex++] = System.Text.Encoding.ASCII.GetString(buf, srcIndex, prefix);
              srcIndex += prefix;
            }
            break;

          case 'p':
          { int len=buf[srcIndex++], slen;
            if(prefix==-1) { prefix=len; slen=0; }
            else if(len<prefix) throw new ArgumentException("Prefix is greater than length of pascal string.");
            else slen = len-prefix;

            if(unicode)
            { char* arr = stackalloc char[prefix];

              if(bigendian==!BitConverter.IsLittleEndian)
              { len = prefix*2;
                if(srcIndex+len>buf.Length) throw new ArgumentOutOfRangeException("Insufficient data.");
                fixed(byte* src=buf) Interop.Unsafe.Copy(src, arr, len);
                srcIndex += len;
              }
              else
                for(int j=0; j<prefix; srcIndex+=2,j++)
                  #if BIGENDIAN
                  arr[j] = (char)ReadLE2U(buf, srcIndex);
                  #else
                  arr[j] = (char)ReadBE2U(buf, srcIndex);
                  #endif

              output[destIndex++] = new string(arr, 0, prefix);
              slen *= 2;
            }
            else
            { output[destIndex++] = System.Text.Encoding.ASCII.GetString(buf, srcIndex, prefix);
              srcIndex += prefix;
            }
            srcIndex += slen;
            break;
          }

          case 'A': unicode=false; break;
          case 'U': unicode=true; break;
          case '<': bigendian=false; break;
          case '>': bigendian=true; break;
          case '=': bigendian=!BitConverter.IsLittleEndian; break;
          default:
            if(char.IsWhiteSpace(c)) break;
            throw new ArgumentException(string.Format("Unknown character '{0}'", c), "format");
        }
      }
    }
    catch(Exception e)
    { throw new ArgumentException(string.Format("Error near char {0}, near output {1} -- {2}", i, destIndex, e.Message), e);
    }

    return srcIndex-start;
  }
  #endregion

  #region Formatted binary write
  /// <summary>Returns formatted binary data in a byte array.</summary>
  /// <param name="format">The string used to format the binary data.</param>
  /// <param name="parms">Parameters referenced by the format string.</param>
  /// <returns>A byte array containing the binary data.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static byte[] Write(string format, params object[] parms)
  { byte[] output = new byte[CalculateSize(true, format, parms)];
    #if DEBUG
    try
    { if(output.Length != Write(output, 0, format, parms)) throw new IndexOutOfRangeException();
    }
    catch(IndexOutOfRangeException e)
    { throw new GameLibException("The lengths don't match! This is a bug in GameLib's formatted output. "+
                                 "Please report it, along with the values you passed to this function.", e);
    }
    #else
    Write(output, 0, format, parms);
    #endif
    return output;
  }
  /// <summary>Writes formatted binary data to a byte array.</summary>
  /// <param name="buf">The byte array to write to.</param>
  /// <param name="format">The string used to format the binary data.</param>
  /// <param name="parms">Parameters referenced by the format string.</param>
  /// <returns>The number of bytes written.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static int Write(byte[] buf, string format, params object[] parms) { return Write(buf, 0, format, parms); }
  /// <summary>Writes formatted binary data to a byte array.</summary>
  /// <param name="buf">The byte array to write to.</param>
  /// <param name="index">The index into the array from which to begin writing.</param>
  /// <param name="format">The string used to format the binary data.</param>
  /// <param name="parms">Parameters referenced by the format string.</param>
  /// <returns>The number of bytes written.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static int Write(byte[] buf, int index, string format, params object[] parms)
  { char c;
    bool bigendian=!BitConverter.IsLittleEndian, unicode=false;

    int i=0, j=0, origIndex=index;
    try
    { for(int flen=format.Length,prefix; i<flen; i++)
      { c = format[i];
        if(char.IsDigit(c))
        { prefix = c-'0';
          while(++i<flen && char.IsDigit(c=format[i])) prefix = prefix*10 + c-'0';
          if(i==flen) throw new ArgumentException("Missing operator at "+i.ToString(), "format");
        }
        else if(c=='?')
        { c=format[++i]; 
          if(c=='s' || c=='p') prefix = -1;
          else prefix=((Array)parms[j]).Length;
        }
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
          // I WANT MACROS!!
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
                Array.Clear(buf, index, prefix*2); index += prefix*2;
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
          case '=': bigendian=!BitConverter.IsLittleEndian; break;
          default:
            if(char.IsWhiteSpace(c)) break;
            throw new ArgumentException(string.Format("Unknown character '{0}'", c), "format");
        }
      }
    }
    catch(Exception e)
    { throw new ArgumentException(string.Format("Error near char {0}, near parameter {1} -- {2}", i, j, e.Message), e);
    }
    return index-origIndex;
  }

  /// <summary>Writes formatted binary data to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="format">The string used to format the binary data.</param>
  /// <param name="parms">Parameters referenced by the format string.</param>
  /// <returns>The number of bytes written.</returns>
  /// <include file='documentation.xml' path='//IO/IOH/BinaryFormat/*'/>
  public static int Write(Stream stream, string format, params object[] parms)
  { if(stream==null || format==null || parms==null) throw new ArgumentNullException();

    char c;
    bool bigendian=!BitConverter.IsLittleEndian, unicode=false;

    int i=0, j=0, origPos=(int)stream.Position;
    try
    { for(int flen=format.Length,prefix; i<flen; i++)
      { c = format[i];
        if(char.IsDigit(c))
        { prefix = c-'0';
          while(++i<flen && char.IsDigit(c=format[i])) prefix = prefix*10 + c-'0';
          if(i==flen) throw new ArgumentException("Missing operator at "+i.ToString(), "format");
        }
        else if(c=='?')
        { c=format[++i]; 
          if(c=='s' || c=='p') prefix = -1;
          else prefix=((Array)parms[j]).Length;
        }
        else prefix=(c=='s' || c=='p' ? -1 : 1);
        if(prefix==0) continue;

        switch(c)
        { case 'x':
            if(prefix<8) do stream.WriteByte(0); while(--prefix!=0);
            else Write(stream, new byte[prefix]);
            break;
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
          // I WANT MACROS!!
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
                prefix *= 2;
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
                prefix *= 2;
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
          case '=': bigendian=!BitConverter.IsLittleEndian; break;
          default:
            if(char.IsWhiteSpace(c)) break;
            throw new ArgumentException(string.Format("Unknown character '{0}'", c), "format");
        }
      }
    }
    catch(Exception e)
    { throw new ArgumentException(string.Format("Error near char {0}, near parameter {1} -- {2}", i, j, e.Message), e);
    }
    return (int)stream.Position-origPos;
  }
  #endregion

  #region Writing
  /// <summary>Writes an array of data to a stream.</summary>
  /// <param name="stream">The stream to which data will be written.</param>
  /// <param name="data">The array of data to write to the stream.</param>
  public static int Write(Stream stream, byte[] data) { stream.Write(data, 0, data.Length); return data.Length; }

  /// <summary>Encodes a string as ASCII and writes it to a stream.</summary>
  /// <param name="stream">The stream to which the string will be written.</param>
  /// <param name="str">The string to write.</param>
  /// <returns>The number of bytes written to the stream.</returns>
  public static int WriteString(Stream stream, string str)
  { return WriteString(stream, str, System.Text.Encoding.ASCII);
  }
  /// <summary>Encodes a string using the given encoding and writes it to a stream.</summary>
  /// <param name="stream">The stream to which the string will be written.</param>
  /// <param name="str">The string to write.</param>
  /// <param name="encoding">The encoding to use to encode the string.</param>
  /// <returns>The number of bytes written to the stream.</returns>
  public static int WriteString(Stream stream, string str, System.Text.Encoding encoding)
  { return Write(stream, encoding.GetBytes(str));
  }
  /// <summary>Encodes a string as ASCII and writes it to a byte array.</summary>
  /// <param name="buf">The byte array to which the string will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="str">The string to write.</param>
  /// <returns>The number of bytes written to the array.</returns>
  public static int WriteString(byte[] buf, int index, string str)
  { return WriteString(buf, index, str, System.Text.Encoding.ASCII);
  }
  /// <summary>Encodes a string using the given encoding and writes it to a byte array.</summary>
  /// <param name="buf">The byte array to which the string will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="str">The string to write.</param>
  /// <param name="encoding">The encoding to use to encode the string.</param>
  /// <returns>The number of bytes written to the array.</returns>
  public static int WriteString(byte[] buf, int index, string str, System.Text.Encoding encoding)
  { byte[] sbuf = encoding.GetBytes(str);
    Array.Copy(sbuf, 0, buf, index, sbuf.Length);
    return sbuf.Length;
  }

  /// <summary>Writes a little-endian short (2 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteLE2(Stream stream, short val)
  { stream.WriteByte((byte)val);
    stream.WriteByte((byte)(val>>8));
  }

  /// <summary>Writes a little-endian short (2 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteLE2(byte[] buf, int index, short val)
  { buf[index]   = (byte)val;
    buf[index+1] = (byte)(val>>8);
  }

  /// <summary>Writes a big-endian short (2 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteBE2(Stream stream, short val)
  { stream.WriteByte((byte)(val>>8));
    stream.WriteByte((byte)val);
  }

  /// <summary>Writes a big-endian short (2 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteBE2(byte[] buf, int index, short val)
  { buf[index]   = (byte)(val>>8);
    buf[index+1] = (byte)val;
  }

  /// <summary>Writes a little-endian integer (4 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteLE4(Stream stream, int val)
  { stream.WriteByte((byte)val);
    stream.WriteByte((byte)(val>>8));
    stream.WriteByte((byte)(val>>16));
    stream.WriteByte((byte)(val>>24));
  }

  /// <summary>Writes a little-endian integer (4 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteLE4(byte[] buf, int index, int val)
  { buf[index]   = (byte)val;
    buf[index+1] = (byte)(val>>8);
    buf[index+2] = (byte)(val>>16);
    buf[index+3] = (byte)(val>>24);
  }

  /// <summary>Writes a big-endian integer (4 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteBE4(Stream stream, int val)
  { stream.WriteByte((byte)(val>>24));
    stream.WriteByte((byte)(val>>16));
    stream.WriteByte((byte)(val>>8));
    stream.WriteByte((byte)val);
  }

  /// <summary>Writes a big-endian integer (4 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteBE4(byte[] buf, int index, int val)
  { buf[index]   = (byte)(val>>24);
    buf[index+1] = (byte)(val>>16);
    buf[index+2] = (byte)(val>>8);
    buf[index+3] = (byte)val;
  }

  /// <summary>Writes a little-endian long (8 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteLE8(Stream stream, long val)
  { WriteLE4(stream, (int)val);
    WriteLE4(stream, (int)(val>>32));
  }

  /// <summary>Writes a little-endian long (8 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteLE8(byte[] buf, int index, long val)
  { WriteLE4(buf, index, (int)val);
    WriteLE4(buf, index+4, (int)(val>>32));
  }

  /// <summary>Writes a big-endian long (8 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteBE8(Stream stream, long val)
  { WriteBE4(stream, (int)(val>>32));
    WriteBE4(stream, (int)val);
  }

  /// <summary>Writes a big-endian long (8 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteBE8(byte[] buf, int index, long val)
  { WriteBE4(buf, index, (int)(val>>32));
    WriteBE4(buf, index+4, (int)val);
  }

  /// <summary>Writes a little-endian unsigned short (2 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteLE2U(Stream stream, ushort val)
  { stream.WriteByte((byte)val);
    stream.WriteByte((byte)(val>>8));
  }

  /// <summary>Writes a little-endian unsigned short (2 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteLE2U(byte[] buf, int index, ushort val)
  { buf[index]   = (byte)val;
    buf[index+1] = (byte)(val>>8);
  }

  /// <summary>Writes a big-endian unsigned short (2 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteBE2U(Stream stream, ushort val)
  { stream.WriteByte((byte)(val>>8));
    stream.WriteByte((byte)val);
  }

  /// <summary>Writes a big-endian unsigned short (2 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteBE2U(byte[] buf, int index, ushort val)
  { buf[index]   = (byte)(val>>8);
    buf[index+1] = (byte)val;
  }

  /// <summary>Writes a little-endian unsigned integer (4 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteLE4U(Stream stream, uint val)
  { stream.WriteByte((byte)val);
    stream.WriteByte((byte)(val>>8));
    stream.WriteByte((byte)(val>>16));
    stream.WriteByte((byte)(val>>24));
  }

  /// <summary>Writes a little-endian unsigned integer (4 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteLE4U(byte[] buf, int index, uint val)
  { buf[index]   = (byte)val;
    buf[index+1] = (byte)(val>>8);
    buf[index+2] = (byte)(val>>16);
    buf[index+3] = (byte)(val>>24);
  }

  /// <summary>Writes a big-endian unsigned integer (4 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteBE4U(Stream stream, uint val)
  { stream.WriteByte((byte)(val>>24));
    stream.WriteByte((byte)(val>>16));
    stream.WriteByte((byte)(val>>8));
    stream.WriteByte((byte)val);
  }

  /// <summary>Writes a big-endian unsigned integer (4 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteBE4U(byte[] buf, int index, uint val)
  { buf[index]   = (byte)(val>>24);
    buf[index+1] = (byte)(val>>16);
    buf[index+2] = (byte)(val>>8);
    buf[index+3] = (byte)val;
  }

  /// <summary>Writes a little-endian unsigned long (8 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteLE8U(Stream stream, ulong val)
  { WriteLE4U(stream, (uint)val);
    WriteLE4U(stream, (uint)(val>>32));
  }

  /// <summary>Writes a little-endian unsigned long (8 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteLE8U(byte[] buf, int index, ulong val)
  { WriteLE4U(buf, index, (uint)val);
    WriteLE4U(buf, index+4, (uint)(val>>32));
  }

  /// <summary>Writes a big-endian unsigned long (8 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteBE8U(Stream stream, ulong val)
  { WriteBE4U(stream, (uint)(val>>32));
    WriteBE4U(stream, (uint)val);
  }

  /// <summary>Writes a big-endian unsigned long (8 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public static void WriteBE8U(byte[] buf, int index, ulong val)
  { WriteBE4U(buf, index, (uint)(val>>32));
    WriteBE4U(buf, index+4, (uint)val);
  }

  /// <summary>Writes an IEEE754 float (4 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public unsafe static void WriteFloat(Stream stream, float val)
  { byte* buf = (byte*)&val;
    stream.WriteByte(buf[0]); stream.WriteByte(buf[1]); stream.WriteByte(buf[2]); stream.WriteByte(buf[3]);
  }

  /// <summary>Writes an IEEE754 float (4 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public unsafe static void WriteFloat(byte[] buf, int index, float val)
  { fixed(byte* pbuf=buf) *(float*)(pbuf+index) = val;
  }

  /// <summary>Writes an IEEE754 double (8 bytes) to a stream.</summary>
  /// <param name="stream">The stream to write to.</param>
  /// <param name="val">The value to write.</param>
  public unsafe static void WriteDouble(Stream stream, double val)
  { byte[] buf = new byte[sizeof(double)];
    fixed(byte* pbuf=buf) *(double*)pbuf = val;
    stream.Write(buf, 0, sizeof(double));
  }

  /// <summary>Writes an IEEE754 double (8 bytes) to a byte array.</summary>
  /// <param name="buf">The byte array to which the value will be written.</param>
  /// <param name="index">The index from which to begin writing.</param>
  /// <param name="val">The value to write.</param>
  public unsafe static void WriteDouble(byte[] buf, int index, double val)
  { fixed(byte* pbuf=buf) *(double*)(pbuf+index) = val;
  }
  #endregion
}
#endregion

} // namespace GameLib.IO