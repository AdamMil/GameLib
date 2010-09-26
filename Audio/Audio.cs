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
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GameLib.Interop;
using GameLib.Interop.SDL;
using GameLib.Interop.GLMixer;
using GameLib.Interop.SndFile;

// FIXME: find out why vorbis is unstable

namespace GameLib.Audio
{

#region Structs, Enums, and Delegates
[Flags]
public enum SampleFormat : short
{
  Eight=GLMixer.Format.Eight, Sixteen=GLMixer.Format.Sixteen, BitsPart=GLMixer.Format.BitsPart,
  Signed=GLMixer.Format.Signed, BigEndian=GLMixer.Format.BigEndian, FloatingPoint=GLMixer.Format.FloatingPoint,

  U8=Eight, U16=Sixteen, S8=Eight|Signed, S16=Sixteen|Signed,
  U8BE=U8|BigEndian, U16BE=U16|BigEndian, S8BE=S8|BigEndian, S16BE=S16|BigEndian,
  U8Sys=GLMixer.Format.U8Sys, U16Sys=GLMixer.Format.U16Sys, S8Sys=GLMixer.Format.S8Sys, S16Sys=GLMixer.Format.S16Sys,

  Float=GLMixer.Format.Float, Double=GLMixer.Format.Double,

  Default=S16Sys
}

public enum Speakers { None, Mono=1, Stereo=2 }
public enum ChannelStatus { Stopped, Playing, Paused }
public enum Fade { None, In, Out }
public enum PlayPolicy { Fail, Oldest, Priority, OldestPriority }
public enum MixPolicy { DontDivide, Divide }
public enum FilterCombination { Series, ParallelSum, ParallelAverage }

public struct SizedArray
{
  public SizedArray(byte[] array) { this.array=array; Length=array.Length; }

  public SizedArray(byte[] array, int length)
  {
    this.array=array; Length=length;
  }

  public byte[] Array
  {
    get { return array; }
  }

  public byte[] Shrink()
  {
    if(array.Length > Length)
    {
      byte[] ret = new byte[Length];
      System.Array.Copy(array, ret, Length);
      array = ret;
    }
    return array;
  }

  public readonly int Length;
  byte[] array;
}

public struct AudioFormat
{
  public AudioFormat(int frequency, SampleFormat format, byte channels)
  {
    Frequency=frequency; Format=format; Channels=channels;
  }

  public byte SampleSize { get { return (byte)((int)(Format&SampleFormat.BitsPart)>>3); } }
  public byte FrameSize { get { return (byte)(SampleSize*Channels); } }
  public int ByteRate { get { return FrameSize*Frequency; } }

  public override bool Equals(object obj)
  {
    return obj is AudioFormat ? this == (AudioFormat)obj : false;
  }

  public override int GetHashCode()
  {
    return Frequency | ((int)Format<<16) | ((int)Channels<<24);
  }

  public int Frequency;
  public SampleFormat Format;
  public byte Channels;

  public static bool operator==(AudioFormat a, AudioFormat b)
  {
    return a.Frequency == b.Frequency && a.Format == b.Format && a.Channels == b.Channels;
  }

  public static bool operator!=(AudioFormat a, AudioFormat b)
  {
    return a.Frequency != b.Frequency || a.Format != b.Format || a.Channels != b.Channels;
  }
}

public delegate void ChannelFinishedHandler(Channel channel);
#endregion

#region Audio sources
#region AudioSource
public abstract class AudioSource : IDisposable
{
  public int Both
  {
    get { return left; }
    set { Audio.CheckVolume(value); left=right=value; }
  }

  public abstract bool CanRewind { get; }
  public abstract bool CanSeek { get; }

  public AudioFormat Format
  {
    get { return format; }
  }

  public int Left
  {
    get { return left; }
    set { Audio.CheckVolume(value); left=value; }
  }

  public int Length
  {
    get { return length; }
    protected set { length = value; }
  }

  public float PlaybackRate { get { return rate; } set { Audio.CheckRate(rate); lock(this) rate=value; } }

  public abstract int Position { get; set; }

  public int Priority { get; set; }

  public int Right
  {
    get { return right; }
    set { Audio.CheckVolume(value); right=value; }
  }

  public void Dispose()
  {
    Dispose(false);
    GC.SuppressFinalize(this);
  }

  public virtual void Rewind() { Position=0; }

  public void GetVolume(out int left, out int right) { left=this.left; right=this.right; }
  public void SetVolume(int left, int right) { Left=left; Right=right; }

  public Channel Play() { return Play(0, Audio.Infinite, 0, Audio.FreeChannel); }
  public Channel Play(int loops) { return Play(loops, Audio.Infinite, 0, Audio.FreeChannel); }
  public Channel Play(int loops, int timeoutMs) { return Play(loops, timeoutMs, 0, Audio.FreeChannel); }
  public Channel Play(int loops, int timeoutMs, int position) { return Play(loops, timeoutMs, position, Audio.FreeChannel); }
  public Channel Play(int loops, int timeoutMs, int position, int channel)
  {
    if(channel<0) return Audio.StartPlaying(channel, this, loops, position, Fade.None, 0, timeoutMs);
    else
    {
      Channel c = Audio.Channels[channel];
      lock(c)
      {
        c.StartPlaying(this, loops, position, Fade.None, 0, timeoutMs);
        return c;
      }
    }
  }

  public Channel FadeIn(int fadeMs) { return FadeIn(fadeMs, 0, Audio.Infinite, Audio.FreeChannel, 0); }
  public Channel FadeIn(int fadeMs, int loops) { return FadeIn(fadeMs, loops, Audio.Infinite, Audio.FreeChannel, 0); }
  public Channel FadeIn(int fadeMs, int loops, int timeoutMs) { return FadeIn(fadeMs, loops, timeoutMs, Audio.FreeChannel, 0); }
  public Channel FadeIn(int fadeMs, int loops, int timeoutMs, int channel) { return FadeIn(fadeMs, loops, timeoutMs, channel, 0); }
  public Channel FadeIn(int fadeMs, int loops, int timeoutMs, int channel, int position)
  {
    if(channel<0) return Audio.StartPlaying(channel, this, loops, position, Fade.In, fadeMs, timeoutMs);
    else
    {
      Channel c = Audio.Channels[channel];
      lock(Audio.Channels[channel])
      {
        c.StartPlaying(this, loops, position, Fade.In, fadeMs, timeoutMs);
        return c;
      }
    }
  }

  protected virtual void Dispose(bool finalizing)
  {
    buffer = null;
  }

  protected int BytesToFrames(int bytes)
  {
    int frames = bytes/Format.FrameSize;
    if(frames*Format.FrameSize != bytes) throw new ArgumentException("length must be multiple of the frame size");
    return frames;
  }

  public int ReadBytes(byte[] buffer, int length) { return ReadBytes(buffer, 0, length); }
  public abstract int ReadBytes(byte[] buffer, int index, int length);

  public virtual byte[] ReadAll()
  {
    lock(this)
    {
      byte[] ret;
      int pos=0;
      if(CanSeek)
      {
        pos = Position;
        Rewind();
      }
      if(length>=0)
      {
        ret = new byte[length*Format.FrameSize];
        if(ReadBytes(ret, ret.Length)!=ret.Length) throw new EndOfStreamException();
      }
      else ret=ReadAllUL();
      if(CanSeek) Position=pos;
      return ret;
    }
  }

  protected byte[] ReadAllUL()
  {
    byte[] buf = new byte[16384];
    int toRead=buf.Length, index=0, read = ReadBytes(buf, index, toRead), length;
    while(read!=0 && read==toRead)
    {
      index = buf.Length;
      byte[] nb = new byte[buf.Length*2];
      Array.Copy(buf, nb, buf.Length);
      buf = nb;
      read = ReadBytes(buf, index, index);
    }
    length = index+read;
    if(length==buf.Length) return buf;
    byte[] ret = new byte[length];
    Array.Copy(buf, ret, length);
    return ret;
  }

  public unsafe int ReadFrames(int[] dest, int index, int frames) { return ReadFrames(dest, index, frames, -1, -1); }

  public unsafe int ReadFrames(int[] dest, int index, int frames, int volume)
  {
    return ReadFrames(dest, index, frames, volume, volume);
  }

  public unsafe int ReadFrames(int[] dest, int index, int frames, int left, int right)
  {
    if(index < 0 || frames < 0 || index+frames > dest.Length) throw new ArgumentOutOfRangeException();
    fixed(int* ptr=dest) return ReadFrames(ptr+index, frames, left, right);
  }

  [CLSCompliant(false)]
  public unsafe int ReadFrames(int* dest, int frames) { return ReadFrames(dest, frames, -1, -1); }

  [CLSCompliant(false)]
  public unsafe int ReadFrames(int* dest, int frames, int volume) { return ReadFrames(dest, frames, volume, volume); }

  [CLSCompliant(false)]
  public virtual unsafe int ReadFrames(int* dest, int frames, int left, int right)
  {
    lock(this)
    {
      int toRead=length<0 ? frames : Math.Min(length-curPos, frames), bytes=toRead*Format.FrameSize, read, samples;
      SizeBuffer(bytes);
      read    = ReadBytes(buffer, bytes);
      samples = read/Format.SampleSize;
      fixed(byte* src = buffer)
        GLMixer.Check(GLMixer.ConvertMix(dest, src, (uint)samples, (ushort)Format.Format, Format.Channels,
                                         (ushort)(left <0 ? Audio.MaxVolume : left),
                                         (ushort)(right<0 ? Audio.MaxVolume : right)));
      read = Format.Channels==1 ? samples : samples/2;
      return read;
    }
  }

  protected void SizeBuffer(int size) { if(buffer==null || buffer.Length<size) buffer=new byte[size]; }

  protected byte[] buffer;
  [CLSCompliant(false)] protected AudioFormat format;
  float rate=1f;
  protected int curPos;
  int left=Audio.MaxVolume, right=Audio.MaxVolume, length=-1;
  internal int playing;
}
#endregion

#region ToneGenerator
public enum ToneType { Sine, Square, Saw, Triangle };
public class ToneGenerator : AudioSource
{
  public ToneGenerator()
  {
    format=new AudioFormat(Audio.Initialized ? Audio.Format.Frequency : 22050, SampleFormat.S16Sys, 1);
    toneFreq=200;
  }
  public ToneGenerator(ToneType type) : this() { this.type=type; }
  public ToneGenerator(ToneType type, float frequency) : this() { this.type=type; Frequency=frequency; }
  public ToneGenerator(ToneType type, float frequency, int sampleRate) : this()
  {
    this.type  = type;
    format     = new AudioFormat(0, SampleFormat.S16Sys, 1);
    Frequency  = frequency;
    SampleRate = sampleRate;
  }

  public override bool CanRewind { get { return true; } }
  public override bool CanSeek { get { return true; } }

  public float Frequency
  {
    get { return toneFreq; }
    set
    {
      if(value<=0) throw new ArgumentOutOfRangeException("Frequency", value, "must be positive");
      toneFreq=value;
    }
  }

  public override int Position { get { return curPos; } set { curPos=value; } }

  public int SampleRate
  {
    get { return Format.Frequency; }
    set
    {
      if(value <= 0) throw new ArgumentOutOfRangeException("SampleRate", value, "must be positive");
      lock(this) format.Frequency = value;
    }
  }

  public ToneType Type { get { return type; } set { type=value; } }

  public override byte[] ReadAll()
  {
    throw new NotSupportedException("ToneGenerator is an infinite data source and can't be read in its entirety.");
  }

  public unsafe override int ReadBytes(byte[] buf, int index, int length)
  {
    length /= 2;
    fixed(byte* bp=buf)
    {
      short* data=(short*)bp;
      switch(type)
      {
        case ToneType.Saw:
        {
          float samplesPerHertz = Format.Frequency/toneFreq, invSph = 1f / samplesPerHertz;
          float tonePos = (float)Math.IEEERemainder(curPos, samplesPerHertz);
          if(tonePos < 0) tonePos += samplesPerHertz;

          for(int i=0; i<length; i++)
          {
            data[i] = (short)((int)(tonePos*invSph*65534)-32767);
            tonePos += 1;
            if(tonePos > samplesPerHertz) tonePos -= samplesPerHertz;
          }
          break;
        }

        case ToneType.Square:
        {
          float samplesPerHertz = Format.Frequency / toneFreq, halfSph = samplesPerHertz*0.5f;
          float negOutThresh = samplesPerHertz-1, posOutThresh = halfSph-1, negInThresh = halfSph+1;
          //float posOutThresh = samplesPerHertz*0.5f, negInThresh = posOutThresh+2;
          float tonePos = (float)Math.IEEERemainder(curPos, samplesPerHertz);
          if(tonePos < 0) tonePos += samplesPerHertz;
          //tonePos += 1; // rotate the number space by one so that we can save one if statement evaluation per iteration

          for(int i=0; i<length; i++)
          {
            // this is the simplified version
            if(tonePos < 1) data[i] = (short)((1-tonePos)*32767);
            else if(tonePos > negOutThresh) data[i] = (short)((tonePos-samplesPerHertz)*32767);
            else if(tonePos < posOutThresh) data[i] = (short)32767;
            else if(tonePos > negInThresh) data[i] = (short)-32767;
            else data[i] = (short)((halfSph-tonePos)*32767);

            // this is the more optimized version with the rotated number space
            /*if(tonePos < 2) data[i] = (short)((tonePos < 1 ? tonePos-1 : 2-tonePos)*32767);
            else if(tonePos < posOutThresh) data[i] = (short)32767;
            else if(tonePos < negInThresh) data[i] = (short)((negInThresh-tonePos-1)*32767);
            else data[i] = (short)-32767;**/

            tonePos += 1;
            if(tonePos >= samplesPerHertz) tonePos -= samplesPerHertz;
          }
          break;
        }

        case ToneType.Sine:
        {
          float scale = 2*(float)Math.PI*toneFreq/SampleRate;
          for(int i=0; i<length; i++) data[i] = (short)(Math.Sin((i+curPos)*scale)*32767);
          break;
        }

        case ToneType.Triangle:
        {
          float samplesPerHertz = Format.Frequency / toneFreq, halfSph = samplesPerHertz*0.5f, invHalf = 1/halfSph;
          float tonePos = (float)Math.IEEERemainder(curPos, samplesPerHertz);
          if(tonePos < 0) tonePos += samplesPerHertz;

          for(int i=0; i<length; i++)
          {
            data[i] = (short)((int)((tonePos > halfSph ? samplesPerHertz-tonePos : tonePos)*invHalf*65534)-32767);
            tonePos += 1;
            if(tonePos >= samplesPerHertz) tonePos -= samplesPerHertz;
          }
          break;
        }
      }
    }
    curPos += length;
    return length*2;
  }

  ToneType type;
  float toneFreq;
}
#endregion

#region StreamSource
public abstract class StreamSource : AudioSource
{
  public override bool CanRewind { get { return stream.CanSeek; } }
  public override bool CanSeek { get { return stream.CanSeek; } }
  public override int Position
  {
    get { return curPos; }
    set { if(value!=curPos) lock(this) { stream.Position=startPos+value*Format.FrameSize; curPos=value; } }
  }

  public override int ReadBytes(byte[] buf, int index, int length)
  {
    BytesToFrames(length);
    lock(this)
    {
      int read = stream.Read(buf, index, length);
      if(read>=0) curPos += read/Format.FrameSize;
      return read;
    }
  }

  protected override void Dispose(bool finalizing)
  {
    base.Dispose(finalizing);
    if(stream != null)
    {
      if(autoClose) stream.Close();
      stream = null;
    }
  }

  protected void Init(Stream stream, AudioFormat format, int startPos, int length, bool autoClose)
  {
    if(stream==null) throw new ArgumentNullException("stream");
    if(this.stream!=null) throw new InvalidOperationException("This object has already been initialized");
    this.stream=stream;
    this.format=format;
    this.startPos=startPos;
    this.Length=length<0 ? length : length/format.FrameSize;
    this.autoClose=autoClose;
    curPos = stream.CanSeek ? (int)(stream.Position-startPos/format.FrameSize) : 0;
  }

  protected Stream stream;
  protected int startPos;
  protected bool autoClose;
}
#endregion

#region RawSource
public class RawSource : StreamSource
{
  public RawSource(string filename, AudioFormat format) : this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), format) { }
  public RawSource(string filename, AudioFormat format, int start, int length) : this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), format, start, length) { }
  public RawSource(Stream stream, AudioFormat format) : this(stream, format, 0, stream.CanSeek ? (int)stream.Length : -1, true) { }
  public RawSource(Stream stream, AudioFormat format, bool autoClose) : this(stream, format, 0, stream.CanSeek ? (int)stream.Length : -1, autoClose) { }
  public RawSource(Stream stream, AudioFormat format, int start, int length) : this(stream, format, start, length, true) { }
  public RawSource(Stream stream, AudioFormat format, int start, int length, bool autoClose)
  {
    Init(stream, format, start, length, autoClose);
  }
}
#endregion

#region SoundFileSource
public class SoundFileSource : AudioSource
{
  public SoundFileSource(string filename)
  {
    SF.Info info = new SF.Info();
    info.format = SF.Format.CPUEndian;
    sndfile = SF.Open(filename, SF.OpenMode.Read, ref info);
    Init(ref info);
  }
  public SoundFileSource(string filename, AudioFormat format) // for RAW
  {
    SF.Info info = new SF.Info();
    InitInfo(ref info, format);
    sndfile = SF.Open(filename, SF.OpenMode.Read, ref info);
    Init(ref info);
  }
  public SoundFileSource(Stream stream) : this(stream, true) { }
  public SoundFileSource(Stream stream, bool autoClose)
  {
    SF.Info info = new SF.Info();
    virtualIO = new StreamVirtualIO(stream, autoClose);
    unsafe
    {
      fixed(SF.VirtualIO* io = &virtualIO.virtualIO)
        sndfile = SF.OpenVirtual(io, SF.OpenMode.Read, ref info, new IntPtr(null));
    }
    Init(ref info);
  }
  public SoundFileSource(Stream stream, AudioFormat format) : this(stream, format, true) { } // for RAW
  public SoundFileSource(Stream stream, AudioFormat format, bool autoClose)
  {
    SF.Info info = new SF.Info();
    InitInfo(ref info, format);
    virtualIO = new StreamVirtualIO(stream, autoClose);
    unsafe
    {
      fixed(SF.VirtualIO* io = &virtualIO.virtualIO)
        sndfile = SF.OpenVirtual(io, SF.OpenMode.Read, ref info, new IntPtr(null));
    }
    Init(ref info);
  }

  ~SoundFileSource() { Dispose(true); }

  public override bool CanRewind { get { return true; } }
  public override bool CanSeek { get { return true; } }
  public override int Position
  {
    get { return curPos; }
    set
    {
      if(value==curPos) return;
      lock(this) curPos=(int)SF.Seek(sndfile, value, SF.SeekType.Absolute);
    }
  }

  public override int ReadBytes(byte[] buf, int index, int length)
  {
    int frames = BytesToFrames(length);
    lock(this)
    {
      int read;
      unsafe { fixed(byte* pbuf = buf) read=(int)SF.ReadShorts(sndfile, (short*)(pbuf+index), frames); }
      if(read>=0) curPos += read;
      return read*Format.FrameSize;
    }
  }

  protected unsafe override void Dispose(bool finalizing)
  {
    if(sndfile.ToPointer()!=null)
    {
      SF.Close(sndfile);
      sndfile = new IntPtr(null);
      Utility.Dispose(ref virtualIO);
    }
    base.Dispose(finalizing);
  }

  void Init(ref SF.Info info)
  {
    unsafe { if(sndfile.ToPointer()==null) throw new FileNotFoundException(); }
    format.Channels  = (byte)info.channels;
    format.Frequency = info.samplerate;
    format.Format    = SampleFormat.S16;
#if BIGENDIAN
  if(info.Has(SF.Format.CPUEndian) || info.Has(SF.Format.BigEndian))
#else
    if(!info.Has(SF.Format.CPUEndian) && info.Has(SF.Format.BigEndian))
#endif
    {
      format.Format |= SampleFormat.BigEndian;
    }

    Length = (int)info.frames;
  }

  static void InitInfo(ref SF.Info info, AudioFormat format)
  {
    info.channels   = format.Channels;
    info.samplerate = (int)format.Frequency;
    info.format     = SF.Format.RAW;
    if((format.Format&SampleFormat.S16)==SampleFormat.S16) info.format |= SF.Format.PCM_S16;
    else if((format.Format&SampleFormat.U8)==SampleFormat.U8) info.format |= SF.Format.PCM_U8;
    else if((format.Format&SampleFormat.S8)==SampleFormat.S8) info.format |= SF.Format.PCM_S8;
    if((format.Format&SampleFormat.BigEndian)!=0) info.format |= SF.Format.BigEndian;
    else info.format |= SF.Format.LittleEndian;
  }

  IntPtr sndfile;
  StreamVirtualIO virtualIO;
}
#endregion

#region SampleSource
public class SampleSource : AudioSource
{
  public SampleSource(AudioSource stream) : this(stream, true) { }
  public SampleSource(AudioSource stream, bool mixerFormat)
  {
    data   = stream.ReadAll();
    format = stream.Format;

    if(mixerFormat)
    {
      data = Audio.Convert(data, Format, Audio.Format).Shrink();
      format = Audio.Format;
    }
    Length = data.Length/Format.FrameSize;
  }
  public SampleSource(AudioSource stream, AudioFormat convertTo)
  {
    format = convertTo;
    data = Audio.Convert(stream.ReadAll(), stream.Format, format).Shrink();
    Length = data.Length/Format.FrameSize;
  }

  public override bool CanRewind { get { return true; } }
  public override bool CanSeek { get { return true; } }
  public override int Position
  {
    get { return curPos; }
    set
    {
      if(value!=curPos)
      {
        if(value<0 || value>Length) throw new ArgumentOutOfRangeException("Position");
        lock(this) curPos = value;
      }
    }
  }

  public override byte[] ReadAll() { return (byte[])data.Clone(); }

  public override int ReadBytes(byte[] buf, int index, int length)
  {
    int frames = BytesToFrames(length);
    lock(this)
    {
      int toRead = Math.Min(frames, this.Length-curPos);
      Array.Copy(data, curPos*Format.FrameSize, buf, index, toRead*Format.FrameSize);
      curPos += toRead;
      return toRead*Format.FrameSize;
    }
  }

  [CLSCompliant(false)]
  public override unsafe int ReadFrames(int* dest, int frames, int left, int right)
  {
    lock(this)
    {
      int toRead=Math.Min(Length-curPos, frames), samples=toRead*Format.Channels;
      fixed(byte* src = data)
        GLMixer.Check(GLMixer.ConvertMix(dest, src+curPos*Format.FrameSize, (uint)samples,
                                         (ushort)Format.Format, Format.Channels,
                                         (ushort)(left <0 ? Audio.MaxVolume : left),
                                         (ushort)(right<0 ? Audio.MaxVolume : right)));
      curPos += toRead;
      return toRead;
    }
  }

  protected override void Dispose(bool finalizing)
  {
    data = null;
    base.Dispose(finalizing);
  }

  protected byte[] data;
}
#endregion
#endregion

#region Audio filters
#region FilterCollection
public sealed class FilterCollection : Collection<AudioFilter>
{
  public FilterCollection() { }
  public FilterCollection(object lockObj) { LockObj = lockObj; }

  protected override void ClearItems()
  {
    if(LockObj!=null) lock(LockObj) base.ClearItems();
    else base.ClearItems();
  }

  protected override void InsertItem(int index, AudioFilter item)
  {
    Validate(item);
    if(LockObj!=null) lock(LockObj) base.InsertItem(index, item);
    else base.InsertItem(index, item);
  }

  protected override void RemoveItem(int index)
  {
    if(LockObj!=null) lock(LockObj) base.RemoveItem(index);
    else base.RemoveItem(index);
  }

  protected override void SetItem(int index, AudioFilter item)
  {
    Validate(item);
    if(LockObj!=null) lock(LockObj) base.SetItem(index, item);
    else base.SetItem(index, item);
  }

  static void Validate(AudioFilter item)
  {
    if(item==null) throw new ArgumentNullException("filter", "Filter must not be null.");
  }

  internal object LockObj;
}
#endregion

#region AudioFilter
public class AudioFilter
{
  public AudioFilter() { type=FilterCombination.Series; }
  public AudioFilter(FilterCombination type) { this.type=type; }

  public FilterCombination Combination
  {
    get { return type; }
    set { type=value; if(value==FilterCombination.Series) parallel=null; }
  }

  public FilterCollection Filters
  {
    get { if(filters==null) filters=new FilterCollection(); return filters; }
  }

  public virtual void Stop(Channel channel)
  {
    if(filters!=null) foreach(AudioFilter filter in filters) filter.Stop(channel);
  }

  [CLSCompliant(false)]
  protected virtual unsafe void Filter(Channel channel, int nchannel, float* input, float* output, int samples,
                                       AudioFormat format) { }

  [CLSCompliant(false)]
  protected virtual unsafe void Filter(Channel channel, float** channels, int samples, AudioFormat format)
  {
    if(filters!=null && filters.Count!=0)
    {
      if(type==FilterCombination.Series || filters.Count==1)
        for(int i=0; i<format.Channels; i++)
          for(int j=0; j<filters.Count; j++) filters[j].Filter(channel, i, channels[i], channels[i], samples, format);
      else
      {
        if(parallel==null || parallel.Length!=samples*filters.Count) parallel = new float[samples*filters.Count];
        for(int i=0; i<format.Channels; i++)
          fixed(float* buf=parallel)
          {
            for(int j=0; j<filters.Count; j++)
              filters[j].Filter(channel, i, channels[i], buf+j*samples, samples, format);
            for(int j=samples, k=0, len=parallel.Length; j<len; j++)
            {
              buf[k] += buf[j];
              if(++k==samples) k=0;
            }
            if(type==FilterCombination.ParallelAverage)
            {
              float* dest=channels[i];
              float mul=1f/filters.Count;
              for(int j=0; j<samples; j++) dest[j] = buf[j]*mul;
            }
            else Unsafe.Copy(buf, channels[i], samples*sizeof(float));
          }
      }
    }
    for(int i=0; i<format.Channels; i++) Filter(channel, i, channels[i], channels[i], samples, format);
  }

  [CLSCompliant(false)]
  internal protected unsafe virtual void MixFilter(Channel channel, int* buffer, int frames, AudioFormat format)
  {
    float** channels = stackalloc float*[format.Channels];
    for(int i=0; i<format.Channels; i++)
    {
      float* data = stackalloc float[frames];
      channels[i] = data;
    }

    Audio.Deinterlace(buffer, channels, frames, format);
    Filter(channel, channels, frames, format);
    Unsafe.Clear(buffer, frames*sizeof(int));
    Audio.Interlace(buffer, channels, frames, format);
  }

  FilterCollection filters;
  float[] parallel;
  FilterCombination type;
}
#endregion

#region BiquadFilter
public class BiquadFilter : AudioFilter
{
  public BiquadFilter() { }
  public BiquadFilter(float c0, float c1, float c2, float c3, float c4) { Set(c0, c1, c2, c3, c4); }
  public BiquadFilter(float a0, float a1, float a2, float b0, float b1, float b2) { Set(a0, a1, a2, b0, b1, b2); }

  [CLSCompliant(false)]
  protected unsafe override void Filter(Channel channel, int nchannel, float* input, float* output, int samples,
                                        AudioFormat format)
  {
    float c0, c1, c2, c3, c4, in0, in1, in2, out1, out2;
    Context[] history;

    lock(this)
    {
      c0=C0; c1=C1; c2=C2; c3=C3; c4=C4;
      history = History;
      if(history==null || history.Length!=format.Channels)
      {
        Context[] narr = new Context[format.Channels];
        if(history!=null) Array.Copy(history, narr, Math.Min(history.Length, format.Channels));
        History = history = narr;
      }
      in1 =history[nchannel].In1; in2 =history[nchannel].In2;
      out1=history[nchannel].Out1; out2=history[nchannel].Out2;
    }

    for(int i=0; i<samples; i++)
    {
      in0 = input[i];
      output[i] = c0*in0 + c1*in1 + c2*in2 - c3*out1 - c4*out2;
      in2=in1; in1=in0; out2=out1; out1=output[i];
    }

    history[nchannel].In1=in1; history[nchannel].In2=in2;
    history[nchannel].Out1=out1; history[nchannel].Out2=out2;
  }

  public void Set(float c0, float c1, float c2, float c3, float c4)
  {
    lock(this)
    {
      C0=c0; C1=c1; C2=c2; C3=c3; C4=c4;
      History=null;
    }
  }

  public void Set(float a0, float a1, float a2, float b0, float b1, float b2)
  {
    lock(this)
    {
      C0=b0/a0; C1=b1/a0; C2=b2/a0; C3=a1/a0; C4=a2/a0;
      History=null;
    }
  }

  public float C0, C1, C2, C3, C4;

  struct Context { public float In1, In2, Out1, Out2; }
  Context[] History;
}
#endregion

#region EqFilter
public enum EqFilterType
{
  BandPass, LowPass, HighPass, BandPassWithGain, Notch, AllPass, Peaking, LowShelf, HighShelf
}
public enum EqParamType { Bandwidth, Q, Slope };

public class EqFilter : BiquadFilter
{
  public EqFilter() { type=EqFilterType.BandPass; paramType=EqParamType.Q; }
  public EqFilter(EqFilterType type, float frequency)
  {
    this.type=type; freq=frequency; paramType=EqParamType.Q;
  }
  public EqFilter(EqFilterType type, float frequency, EqParamType paramType, float parameter)
  {
    this.type=type; freq=frequency; this.paramType=paramType; param=parameter;
  }

  public EqFilterType Type { get { return type; } set { type=value; changed=true; } }

  public float Gain
  {
    get { return gain; }
    set { gain=value; changed=true; }
  }
  public float Frequency
  {
    get { return freq; }
    set { freq=value; changed=true; }
  }
  public float Parameter
  {
    get { return param; }
    set { param=value; changed=true; }
  }
  public EqParamType ParameterType
  {
    get { return paramType; }
    set { paramType=value; changed=true; }
  }

  [CLSCompliant(false)]
  protected override unsafe void Filter(Channel channel, int nchannel, float* input, float* output, int samples,
                                        AudioFormat format)
  {
    if(changed) Recalculate();
    base.Filter(channel, nchannel, input, output, samples, format);
  }

  // http://www.musicdsp.org/files/Audio-EQ-Cookbook.txt
  void Recalculate()
  {
    changed=false;
    double A = (type==EqFilterType.Peaking || type==EqFilterType.LowShelf || type==EqFilterType.HighShelf)
             ? Math.Pow(10, gain/40) : Math.Sqrt(Math.Pow(10, gain/20));
    double w0 = Math.PI*2*freq/Audio.Format.Frequency, w0cos=Math.Cos(w0), w0sin=Math.Sin(w0);
    double alpha, p=param, a0, a1, a2, b0, b1, b2;

    switch(paramType)
    {
      case EqParamType.Q: alpha = w0sin/(p*2); break;
      case EqParamType.Bandwidth: alpha = w0sin*Math.Sinh(Math.Log(2)/2 * p * w0/w0sin); break;
      case EqParamType.Slope:
        if(type==EqFilterType.LowShelf || type==EqFilterType.HighShelf)
          alpha = w0sin/2 * Math.Sqrt((A+1/A) * (1/p-1) + 2);
        else throw new ArgumentException("Invalid parameter type. (Slope is only valid for shelf EQs)", "ParameterType");
        break;
      default: throw new ArgumentException("Invalid parameter type.", "ParameterType");
    }

    switch(type)
    {
      case EqFilterType.LowPass:
        b0 = (1-w0cos)/2;
        b1 = 1 - w0cos;
        b2 = (1-w0cos)/2;
        a0 = 1 + alpha;
        a1 = -2*w0cos;
        a2 = 1 - alpha;
        break;
      case EqFilterType.HighPass:
        b0 = (1+w0cos)/2;
        b1 = -(1+w0cos);
        b2 = (1+w0cos)/2;
        a0 = 1 + alpha;
        a1 = -2*w0cos;
        a2 = 1 - alpha;
        break;
      case EqFilterType.BandPass:
        b0 = alpha;
        b1 = 0;
        b2 = -alpha;
        a0 = 1 + alpha;
        a1 = -2*w0cos;
        a2 = 1 - alpha;
        break;
      case EqFilterType.BandPassWithGain:
        b0 = p*alpha;
        b1 = 0;
        b2 = -p*alpha;
        a0 = 1 + alpha;
        a1 = -2*w0cos;
        a2 = 1 - alpha;
        break;
      case EqFilterType.Notch:
        b0 = 1;
        b1 = -2*w0cos;
        b2 = 1;
        a0 = 1 + alpha;
        a1 = -2*w0cos;
        a2 = 1 - alpha;
        break;
      case EqFilterType.AllPass:
        b0 = 1 - alpha;
        b1 = -2*w0cos;
        b2 = 1 + alpha;
        a0 = 1 + alpha;
        a1 = -2*w0cos;
        a2 = 1 - alpha;
        break;
      case EqFilterType.Peaking:
        b0 = 1 + alpha*A;
        b1 = -2*w0cos;
        b2 = 1 - alpha*A;
        a0 = 1 + alpha/A;
        a1 = -2*w0cos;
        a2 = 1 - alpha/A;
        break;
      case EqFilterType.LowShelf:
      case EqFilterType.HighShelf:
        double beta = w0sin * Math.Sqrt((A*A+1)*(1/p-1)+2*A);
        if(type==EqFilterType.LowShelf)
        {
          b0 =    A*((A+1) - (A-1)*w0cos + beta);
          b1 =  2*A*((A-1) - (A+1)*w0cos);
          b2 =    A*((A+1) - (A-1)*w0cos - beta);
          a0 =       (A+1) + (A-1)*w0cos + beta;
          a1 =   -2*((A-1) + (A+1)*w0cos);
          a2 =       (A+1) + (A-1)*w0cos - beta;
        }
        else
        {
          b0 =    A*((A+1) + (A-1)*w0cos + beta);
          b1 = -2*A*((A-1) + (A+1)*w0cos);
          b2 =    A*((A+1) + (A-1)*w0cos - beta);
          a0 =       (A+1) - (A-1)*w0cos + beta;
          a1 =    2*((A-1) - (A+1)*w0cos);
          a2 =       (A+1) - (A-1)*w0cos - beta;
        }
        break;
      default: throw new ArgumentException("Invalid filter type.", "Type");
    }
    Set((float)(b0/a0), (float)(b1/a0), (float)(b2/a0), (float)(a1/a0), (float)(a2/a0));
  }

  float gain, param, freq;
  EqParamType paramType;
  EqFilterType type;
  bool changed=true;
}
#endregion

#region GraphicEqualizer
public static class GraphicEqualizer
{
  public static AudioFilter Make(int nbands)
  {
    if(nbands==5) return Make(5, 62.5, 2);
    else if(nbands==10) return Make(10, 31.25, 1);
    else if(nbands==15) return Make(15, 25, 2.0/3);
    else if(nbands==30) return Make(30, 25, 1.0/3);
    else throw new NotSupportedException("Unsupported number of bands: "+nbands.ToString()+". Either use 5, 10, 15, "+
                                         "or 30 bands, or specify the frequency and bandwidth manually.");
  }

  public static AudioFilter Make(int nbands, double startFreq, double bandwidth)
  {
    if(nbands<=0 || startFreq<=0 || bandwidth<=0)
      throw new ArgumentException("nbands, startFreq, and bandwidth must all be positive.");

    AudioFilter filter = new AudioFilter();
    double mul = (float)Math.Pow(2, bandwidth);
    for(int i=0; i<nbands; i++)
    {
      filter.Filters.Add(new EqFilter(EqFilterType.Peaking, (float)startFreq,
                                      EqParamType.Bandwidth, (float)bandwidth));
      startFreq *= mul;
    }

    return filter;
  }
}
#endregion
#endregion

#region Channel class
public sealed class Channel
{
  internal Channel(int channel) { number=channel; Reset(); }

  public event ChannelFinishedHandler Finished;

  public int Age { get { return source==null ? 0 : (int)(Timing.Milliseconds-startTime); } }

  public int Both
  {
    get { return left; }
    set { Audio.CheckVolume(value); left = right = value; }
  }

  public Fade Fading { get { return fade; } }

  public FilterCollection Filters { get { if(filters==null) filters=new FilterCollection(this); return filters; } }

  public int Left
  {
    get { return left; }
    set { Audio.CheckVolume(value); left=value; }
  }

  public int Number { get { return number; } }
  public bool Paused { get { return paused; } set { paused=value; } }
  public float PlaybackRate { get { return rate; } set { Audio.CheckRate(rate); lock(this) rate=value; } }
  public bool Playing { get { return Status==ChannelStatus.Playing; } }
  public int Position { get { return position; } set { lock(this) position=value; } }
  public int Priority { get { return priority; } }

  public int Right
  {
    get { return right; }
    set { Audio.CheckVolume(value); right=value; }
  }

  public AudioSource Source { get { return source; } }

  public ChannelStatus Status
  {
    get { return source==null ? ChannelStatus.Stopped : paused ? ChannelStatus.Paused : ChannelStatus.Playing; }
  }

  public bool Stopped { get { return Status==ChannelStatus.Stopped; } }

  public void FadeOut(int fadeMs)
  {
    if(fadeMs < 0) throw new ArgumentOutOfRangeException("fadeMs", "cannot be negative");
    lock(this)
    {
      if(source==null) return;
      fade      = Fade.Out;
      fadeTime  = (uint)fadeMs;
      fadeStart = Timing.Milliseconds;
      fadeLeft  = EffectiveLeft;
      fadeRight = EffectiveRight;
    }
  }

  public void GetVolume(out int left, out int right) { left=this.left; right=this.right; }
  public void SetVolume(int left, int right) { Left=left; Right=right; }

  public void Pause() { paused=true; }
  public void Resume() { paused=false; }
  public void Stop() { lock(this) StopPlaying(); }

  internal void Reset() { rate=1f; left=Audio.MaxVolume; right=Audio.MaxVolume; }

  internal void StartPlaying(AudioSource source, int loops, int position, Fade fade, int fadeMs, int timeoutMs)
  {
    if(fadeMs < 0) throw new ArgumentOutOfRangeException();
    StopPlaying();
    lock(source)
    {
      if(!source.CanRewind && loops!=0) throw new ArgumentException("Can't play loop sources that can't be rewound");
      if(!source.CanSeek && source.playing>0)
        throw new ArgumentException("Can't play unseekable streams more than once at a time");
      this.source   = source;
      this.loops    = loops;
      this.fade     = fade;
      this.timeout  = timeoutMs;
      this.position = position;
      if(!source.CanSeek && source.CanRewind && position==0) source.Rewind();
      priority  = source.Priority;
      paused    = false;
      startTime = Timing.Milliseconds;
      source.playing++;
      convert = !source.Format.Equals(Audio.Format);
      if(convert)
      {
        GLMixer.AudioConversion cvt = Audio.SetupConversion(source.Format, Audio.Format);
        sdMul=cvt.lenMul; sdDiv=cvt.lenDiv;
      }
      else convBuf=null;
      if(fade!=Fade.None)
      {
        fadeTime  = (uint)fadeMs;
        fadeLeft  = fade==Fade.In ? 0 : EffectiveLeft;
        fadeRight = fade==Fade.In ? 0 : EffectiveRight;
        fadeStart = Timing.Milliseconds;
      }
    }
  }

  internal void StopPlaying()
  {
    if(source==null) return;
    lock(source)
    {
      if(filters!=null) for(int i=0; i<filters.Count; i++) filters[i].Stop(this);
      Audio.OnFiltersFinished(this);
      if(Finished!=null) Finished(this);
      Audio.OnChannelFinished(this);
      source = null;
    }
  }

  internal unsafe void Mix(int* stream, int frames, FilterCollection filters)
  {
    if(source==null || paused) return;
    lock(source)
    {
      if(source.Length==0) return;
      AudioFormat format = source.Format;
      float rate = EffectiveRate;
      int left = EffectiveLeft, right=EffectiveRight, read, toRead, samples;
      bool convert = this.convert;

      if(timeout!=Audio.Infinite && Age>timeout)
      {
        StopPlaying();
        return;
      }
      if(fade!=Fade.None)
      {
        uint fadeSoFar = Timing.Milliseconds-fadeStart;
        int ltarg, rtarg;
        if(fade==Fade.In) { ltarg=left; rtarg=right; }
        else ltarg=rtarg=0;
        if(fadeSoFar>fadeTime)
        {
          if(fade==Fade.Out) { StopPlaying(); return; }
          left  = ltarg;
          right = rtarg;
          fade  = Fade.None;
        }
        else
        {
          left  = fadeLeft  + (ltarg-fadeLeft)*(int)fadeSoFar/(int)fadeTime;
          right = fadeRight + (rtarg-fadeRight)*(int)fadeSoFar/(int)fadeTime;
        }
      }

      if(source.CanSeek) source.Position = position;
      if(convert || rate!=1f) // FIXME: this path can result in 'toRead' not being a frame multiple!!!
      {
        int index=0, mustWrite = frames*Audio.Format.FrameSize, framesRead;
        bool stop=false;
        if(rate==1f) toRead = (int)((long)mustWrite*sdDiv/sdMul);
        else
        {
          int shift = format.FrameSize>>1;
          format.Frequency = (int)(format.Frequency*rate);
          if(format.Frequency==0) return;
          convert = true;
          GLMixer.AudioConversion cvt = Audio.SetupConversion(format, Audio.Format);
          toRead = (int)((long)mustWrite*cvt.lenDiv/cvt.lenMul+shift)>>shift<<shift;
        }

        // FIXME: HACK: this hacks the 'toRead' not frame multiple problem by duplicating samples
        // this means it's possible that a few samples may be added each buffer fill. this is not correct output!
        if(format.FrameSize>1)
        {
          int diff = toRead & (format.FrameSize-1);
          if(diff>0) toRead += format.FrameSize-diff;
        }

        int len = Math.Max(toRead, mustWrite);
        if(convBuf==null || convBuf.Length<len) convBuf = new byte[len];

        while(index<toRead)
        {
          read = source.ReadBytes(convBuf, index, toRead-index);
          if(read==0)
          {
            if(loops==0)
            {
              Array.Clear(convBuf, index, toRead-index);
              stop = true;
              break;
            }
            else
            {
              source.Rewind();
              position = source.Position;
              if(loops!=Audio.Infinite) loops--;
            }
          }
          index += read;
        }
        SizedArray sa = Audio.Convert(convBuf, format, Audio.Format, toRead, mustWrite);
        if(sa.Array!=convBuf) convBuf = sa.Array;
        framesRead = sa.Length/Audio.Format.FrameSize;
        samples    = framesRead*Audio.Format.Channels;
        if((this.filters==null || this.filters.Count==0) && (filters==null || filters.Count==0))
          fixed(byte* src = convBuf)
            GLMixer.Check(GLMixer.ConvertMix(stream, src, (uint)samples, (ushort)Audio.Format.Format,
                                             Audio.Format.Channels, (ushort)left, (ushort)right));
        else
        {
          int* buffer = stackalloc int[samples];
          GameLib.Interop.Unsafe.Clear(buffer, samples*sizeof(int));
          fixed(byte* src = convBuf)
            GLMixer.Check(GLMixer.ConvertMix(buffer, src, (uint)samples,
                                             (ushort)Audio.Format.Format, Audio.Format.Channels,
                                             (ushort)Audio.MaxVolume, (ushort)Audio.MaxVolume));
          if(this.filters!=null)
            for(int i=0; i<this.filters.Count; i++) this.filters[i].MixFilter(this, buffer, framesRead, Audio.Format);
          if(filters!=null)
            for(int i=0; i<filters.Count; i++) filters[i].MixFilter(this, buffer, framesRead, Audio.Format);
          GLMixer.Check(GLMixer.Mix(stream, buffer, (uint)samples, (ushort)left, (ushort)right));
        }

        if(stop) { StopPlaying(); return; }
      }
      else
      {
        toRead=frames;
        while(true)
        {
          if((this.filters==null || this.filters.Count==0) && (filters==null || filters.Count==0))
          {
            read    = source.ReadFrames(stream, toRead, left, right);
            samples = read*Audio.Format.Channels;
          }
          else
          {
            int* buffer = stackalloc int[toRead];
            GameLib.Interop.Unsafe.Clear(buffer, toRead*sizeof(int));
            read    = source.ReadFrames(buffer, toRead, -1, -1);
            samples = read*Audio.Format.Channels;
            if(read>0)
            {
              if(this.filters!=null)
                for(int i=0; i<this.filters.Count; i++) this.filters[i].MixFilter(this, buffer, read, format);
              if(filters!=null) for(int i=0; i<filters.Count; i++) filters[i].MixFilter(this, buffer, read, format);
              GLMixer.Check(GLMixer.Mix(stream, buffer, (uint)samples, (ushort)left, (ushort)right));
            }
          }
          toRead -= read;
          if(toRead<=0) break;
          stream += samples;
          if(read==0)
          {
            if(loops==0) { StopPlaying(); return; }
            else
            {
              source.Rewind();
              position = source.Position;
              if(loops!=Audio.Infinite) loops--;
            }
          }
        }
      }
      position = source.Position;
    }
  }

  int EffectiveLeft { get { int v=source.Left; return v==Audio.MaxVolume ? left  : (left *v)>>8; } }
  int EffectiveRight { get { int v=source.Right; return v==Audio.MaxVolume ? right : (right*v)>>8; } }
  float EffectiveRate { get { return source.PlaybackRate*rate; } }

  AudioSource source;
  FilterCollection filters;
  byte[] convBuf;
  float rate=1f;
  uint startTime, fadeStart, fadeTime;
  int left=Audio.MaxVolume, right=Audio.MaxVolume, fadeLeft, fadeRight;
  int timeout, number, position, loops, priority, sdMul, sdDiv;
  Fade fade;
  bool paused, convert;
}
#endregion

#region Audio class
public static class Audio
{
  public const int Infinite=-1, FreeChannel=-1, MaxVolume=256;

  public static FilterCollection Filters
  {
    get { if(filters==null) filters=new FilterCollection(callback); return filters; }
  }

  public static FilterCollection PostFilters
  {
    get { if(postFilters==null) postFilters=new FilterCollection(callback); return postFilters; }
  }

  public static event ChannelFinishedHandler ChannelFinished;

  public static bool Initialized { get { return init; } }
  public static AudioFormat Format { get { AssertInit(); return format; } }
  public static object SyncRoot { get { AssertInit(); return callback; } }

  public static int MasterVolume
  {
    get { AssertInit(); return (int)GLMixer.GetMixVolume(); }
    set { AssertInit(); CheckVolume(value); GLMixer.SetMixVolume((ushort)value); }
  }
  public static int ReservedChannels
  {
    get { AssertInit(); return reserved; }
    set
    {
      AssertInit();
      if(reserved<0 || reserved>chans.Length) throw new ArgumentOutOfRangeException("value");
      reserved=value;
    }
  }

  public static ReadOnlyCollection<Channel> Channels { get { return Array.AsReadOnly(chans); } }
  public static PlayPolicy PlayPolicy { get { return playPolicy; } set { playPolicy=value; } }
  public static MixPolicy MixPolicy { get { return mixPolicy; } set { mixPolicy=value; } }

  // TODO: should this be counted like the others?
  public static bool Initialize() { return Initialize(22050, SampleFormat.Default, Speakers.Stereo, 50); }
  public static bool Initialize(int frequency) { return Initialize(frequency, SampleFormat.Default, Speakers.Stereo, 50); }
  public static bool Initialize(int frequency, SampleFormat format) { return Initialize(frequency, format, Speakers.Stereo, 50); }
  public static bool Initialize(int frequency, SampleFormat format, Speakers chans) { return Initialize(frequency, format, chans, 50); }
  public unsafe static bool Initialize(int frequency, SampleFormat format, Speakers chans, int bufferMs)
  {
    if(frequency < 0 || bufferMs < 0) throw new ArgumentOutOfRangeException();
    if(init) throw new InvalidOperationException("Already initialized. Deinitialize first to change format");
    if((format&SampleFormat.FloatingPoint)!=0)
      throw new ArgumentException("Floating point format not supported by the underlying API.", "format");

    callback    = new GLMixer.MixCallback(FillBuffer);
    groups      = new List<List<int>>();
    SDL.Initialize(SDL.InitFlag.Audio);
    init        = true;

    try
    {
      GLMixer.Check(GLMixer.Init((uint)frequency, (ushort)format, (byte)chans, (uint)bufferMs, callback, new IntPtr(null)));

      uint freq, bytes;
      ushort form;
      byte chan;
      GLMixer.Check(GLMixer.GetFormat(out freq, out form, out chan, out bytes));
      Audio.format = new AudioFormat((int)freq, (SampleFormat)form, chan);

      if(filters!=null) filters.LockObj = callback;
      if(postFilters!=null) postFilters.LockObj = callback;

      SDL.PauseAudio(0);
      return freq==frequency && form==(short)format && chan==(byte)chans;
    }
    catch { Deinitialize(); throw; }
  }

  public static void Deinitialize()
  {
    if(init)
    {
      SDL.PauseAudio(1);
      lock(callback)
      {
        Stop();
        if(postFilters!=null) for(int i=0; i<postFilters.Count; i++) postFilters[i].Stop(null);
        GLMixer.Quit();
        SDL.Deinitialize(SDL.InitFlag.Audio);
        callback = null;
        chans    = new Channel[0];
        groups   = null;
        init     = false;
      }
    }
  }

  public static void AllocateChannels(int numChannels) { AllocateChannels(numChannels, true); }
  public static void AllocateChannels(int numChannels, bool resetChannels)
  {
    AssertInit();
    if(numChannels<0) throw new ArgumentOutOfRangeException("numChannels");
    lock(callback)
    {
      for(int i=numChannels; i<chans.Length; i++)
      {
        chans[i].Stop();
        if(resetChannels) chans[i].Reset();
      }
      Channel[] narr = new Channel[numChannels];
      Array.Copy(chans, narr, chans.Length);
      for(int i=chans.Length; i<numChannels; i++) narr[i] = new Channel(i);
      chans = narr;
      if(numChannels<reserved) reserved=numChannels;
    }
  }

  public static int AddGroup()
  {
    AssertInit();
    lock(groups)
    {
      for(int i=0; i<groups.Count; i++)
        if(groups[i]==null)
        {
          groups[i] = new List<int>();
          return i;
        }
      groups.Add(new List<int>());
      return groups.Count-1;
    }
  }

  public static void RemoveGroup(int group) { lock(groups) { GetGroup(group); groups[ToGroup(group)]=null; } }

  public static void GroupChannel(int channel, int group)
  {
    CheckChannel(channel);
    lock(groups)
    {
      List<int> list = GetGroup(group);
      if(!list.Contains(channel)) list.Add(channel);
    }
  }

  public static void GroupRange(int start, int end, int group)
  {
    CheckChannel(start); CheckChannel(end);
    if(start>end) throw new ArgumentException("start should be <= end");
    lock(groups)
    {
      List<int> list = GetGroup(group);
      for(; start<=end; start++) if(!list.Contains(start)) list.Add(start);
    }
  }

  public static void UngroupChannel(int channel, int group) { lock(groups) GetGroup(group).Remove(channel); }

  public static int GroupSize(int group) { lock(groups) return GetGroup(group).Count; }

  public static ReadOnlyCollection<int> GetGroupChannels(int group)
  {
    lock(groups) return GetGroup(group).AsReadOnly();
  }

  public static int OldestChannel(bool unreserved) { return OldestChannel(-1, unreserved); }
  public static int OldestChannel(int group, bool unreserved)
  {
    int i, oi;
    int age=0;
    if(group==-1)
    {
      AssertInit();
      lock(callback)
        for(oi=unreserved ? 0 : reserved, i=oi; i<chans.Length; i++) if(chans[i].Age>age) { age=chans[i].Age; oi=i; }
    }
    else
      lock(groups)
      {
        List<int> list = GetGroup(group);
        for(oi=i=0; i<list.Count; i++)
        {
          int chan = list[i];
          if(!unreserved && chan<reserved) continue;
          if(chans[chan].Age>age) { age=chans[chan].Age; oi=chan; }
        }
      }
    return oi;
  }

  public static void FadeOut(int fadeMs) { FadeOut(-1, fadeMs); }
  public static void FadeOut(int group, int fadeMs)
  {
    if(group==-1) lock(callback) for(int i=0; i<chans.Length; i++) chans[i].FadeOut(fadeMs);
    else
      lock(groups)
      {
        List<int> list = GetGroup(group);
        for(int i=0; i<list.Count; i++) chans[list[i]].FadeOut(fadeMs);
      }
  }

  public static void Pause() { Pause(-1); }
  public static void Pause(int group)
  {
    if(group==-1) lock(callback) for(int i=0; i<chans.Length; i++) chans[i].Pause();
    else
      lock(groups)
      {
        List<int> list = GetGroup(group);
        for(int i=0; i<list.Count; i++) chans[list[i]].Pause();
      }
  }

  public static void Resume() { Resume(-1); }
  public static void Resume(int group)
  {
    if(group==-1) lock(callback) for(int i=0; i<chans.Length; i++) chans[i].Resume();
    else
      lock(groups)
      {
        List<int> list = GetGroup(group);
        for(int i=0; i<list.Count; i++) chans[list[i]].Resume();
      }
  }

  public static void Stop() { Stop(-1); }
  public static void Stop(int group)
  {
    if(group==-1) lock(callback) for(int i=0; i<chans.Length; i++) chans[i].Stop();
    else
      lock(groups)
      {
        List<int> list = GetGroup(group);
        for(int i=0; i<list.Count; i++) chans[list[i]].Stop();
      }
  }

  [CLSCompliant(false)]
  public static GLMixer.AudioConversion SetupConversion(AudioFormat srcFormat, AudioFormat destFormat)
  {
    GLMixer.AudioConversion cvt = new GLMixer.AudioConversion();
    if(srcFormat.Equals(destFormat)) { cvt.lenMul=cvt.lenDiv=1; }
    cvt.srcRate    = (int)srcFormat.Frequency;
    cvt.srcFormat  = (ushort)srcFormat.Format;
    cvt.srcChans   = srcFormat.Channels;
    cvt.destRate   = (int)destFormat.Frequency;
    cvt.destFormat = (ushort)destFormat.Format;
    cvt.destChans  = destFormat.Channels;
    GLMixer.Check(GLMixer.SetupConversion(ref cvt));
    return cvt;
  }

  public static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, SampleFormat destFormat)
  {
    return Convert(srcData, srcFormat, destFormat, -1);
  }
  public static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, SampleFormat destFormat, int length)
  {
    AudioFormat df = srcFormat;
    df.Format = destFormat;
    return Convert(srcData, srcFormat, df, length, -1);
  }
  public static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, AudioFormat destFormat)
  {
    return Convert(srcData, srcFormat, destFormat, -1, -1);
  }
  public static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, AudioFormat destFormat, int length)
  {
    return Convert(srcData, srcFormat, destFormat, length, -1);
  }
  public static SizedArray Convert(byte[] srcData, AudioFormat srcFormat, AudioFormat destFormat,
                                   int length, int mustWrite)
  {
    if(srcFormat.Equals(destFormat)) return new SizedArray(srcData);
    unsafe
    {
      GLMixer.AudioConversion cvt = new GLMixer.AudioConversion();
      if(length<0) length = srcData.Length;
      cvt.srcRate    = (int)srcFormat.Frequency;
      cvt.srcFormat  = (ushort)srcFormat.Format;
      cvt.srcChans   = srcFormat.Channels;
      cvt.destRate   = (int)destFormat.Frequency;
      cvt.destFormat = (ushort)destFormat.Format;
      cvt.destChans  = destFormat.Channels;
      cvt.len        = length;

      GLMixer.Check(GLMixer.SetupConversion(ref cvt));
      if(mustWrite!=-1) cvt.lenCvt=mustWrite;

      if(srcData.Length>=cvt.lenCvt)
      {
        fixed(byte* buf = srcData)
        {
          cvt.buf = buf;
          GLMixer.Check(GLMixer.Convert(ref cvt));
        }
        return new SizedArray(srcData, cvt.lenCvt);
      }
      else
      {
        byte[] ret = new byte[cvt.lenCvt];
        Array.Copy(srcData, ret, length);
        fixed(byte* buf = ret)
        {
          cvt.buf = buf;
          GLMixer.Check(GLMixer.Convert(ref cvt));
        }
        return new SizedArray(ret, cvt.lenCvt);
      }
    }
  }

  internal static unsafe void Deinterlace(int* buffer, float** channels, int frames, AudioFormat format)
  {
    if(format.Channels==1) GLMixer.ConvertAccumulator(channels[0], buffer, (uint)frames, (ushort)SampleFormat.Float);
    else if(format.Channels==2)
    {
      int* left=stackalloc int[frames], right=stackalloc int[frames];
      for(int i=0; i<frames; i++)
      {
        left[i]  = *buffer++;
        right[i] = *buffer++;
      }
      GLMixer.ConvertAccumulator(channels[0], left, (uint)frames, (ushort)SampleFormat.Float);
      GLMixer.ConvertAccumulator(channels[1], right, (uint)frames, (ushort)SampleFormat.Float);
    }
    else
    {
      int* src=stackalloc int[frames];
      for(int c=0, inc=format.Channels; c<inc; c++)
      {
        for(int i=c, j=0; j<frames; i+=inc, j++) src[j]=buffer[i];
        GLMixer.ConvertAccumulator(channels[c], src, (uint)frames, (ushort)SampleFormat.Float);
      }
    }
  }

  internal static unsafe void Interlace(int* buffer, float** channels, int frames, AudioFormat format)
  {
    if(format.Channels==1)
      GLMixer.ConvertMix(buffer, channels[0], (uint)frames, (ushort)SampleFormat.Float,
                         format.Channels, MaxVolume, MaxVolume);
    else if(format.Channels==2)
    {
      int* left=stackalloc int[frames], right=stackalloc int[frames];
      GLMixer.ConvertMix(left, channels[0], (uint)frames, (ushort)SampleFormat.Float,
                         format.Channels, MaxVolume, MaxVolume);
      GLMixer.ConvertMix(right, channels[1], (uint)frames, (ushort)SampleFormat.Float,
                         format.Channels, MaxVolume, MaxVolume);
      for(int i=0; i<frames; i++)
      {
        *buffer++ = left[i];
        *buffer++ = right[i];
      }
    }
    else
    {
      int* src=stackalloc int[frames];
      for(int c=0, inc=format.Channels; c<inc; c++)
      {
        GLMixer.ConvertMix(src, channels[c], (uint)frames, (ushort)SampleFormat.Float,
                           format.Channels, MaxVolume, MaxVolume);
        for(int i=c, j=0; j<frames; i+=inc, j++) buffer[i]=src[j];
      }
    }
  }

  internal static void OnFiltersFinished(Channel channel)
  {
    if(filters!=null) lock(callback) for(int i=0; i<filters.Count; i++) filters[i].Stop(channel);
  }

  internal static void OnChannelFinished(Channel channel)
  {
    if(ChannelFinished!=null) ChannelFinished(channel);
  }

  internal static Channel StartPlaying(int channel, AudioSource source, int loops, int position, Fade fade, int fadeMs, int timeoutMs)
  {
    AssertInit();
    if(reserved==chans.Length) return null;

    IList<int> group = null;
    bool tried = false;
    do
    {
      if(channel==FreeChannel)
      {
        for(int i=reserved; i<chans.Length; i++)
          if(chans[i].Status==ChannelStatus.Stopped) // try to lock as little as possible
          {
            tried=true;
            lock(chans[i])
              if(chans[i].Status==ChannelStatus.Stopped)
              {
                chans[i].StartPlaying(source, loops, position, fade, fadeMs, timeoutMs);
                return chans[i];
              }
          }
      }
      else
        lock(groups)
        {
          group = GetGroup(channel);
          for(int i=0; i<group.Count; i++)
          {
            int chan = group[i];
            if(chan<reserved) continue;
            if(chans[chan].Status==ChannelStatus.Stopped) // try to lock as little as possible
            {
              tried = true;
              lock(chans[chan])
                if(chans[chan].Status==ChannelStatus.Stopped)
                {
                  chans[chan].StartPlaying(source, loops, position, fade, fadeMs, timeoutMs);
                  return chans[chan];
                }
            }
          }
        }
    } while(!tried);

    switch(playPolicy)
    {
      case PlayPolicy.Oldest:
        lock(callback)
        {
          int oi = reserved, age = 0;
          if(channel==FreeChannel)
            for(int i=reserved; i<chans.Length; i++) { if(chans[i].Age>age) { age=chans[i].Age; oi=i; } }
          else
            lock(groups)
              for(int i=0; i<group.Count; i++)
              {
                int chan = group[i];
                if(chan<reserved) continue;
                if(chans[chan].Age>age) { age=chans[chan].Age; oi=chan; }
              }
          lock(chans[oi]) chans[oi].StartPlaying(source, loops, position, fade, fadeMs, timeoutMs);
          return chans[oi];
        }
      case PlayPolicy.Priority:
        lock(callback)
        {
          int pi=reserved, prio=int.MaxValue;
          if(channel==FreeChannel)
            for(int i=reserved; i<chans.Length; i++) { if(chans[i].Priority<prio) { prio=chans[i].Priority; pi=i; } }
          else
            lock(groups)
              for(int i=0; i<group.Count; i++)
              {
                int chan = group[i];
                if(chan<reserved) continue;
                if(chans[chan].Priority<prio) { prio=chans[chan].Priority; pi=chan; }
              }
          lock(chans[pi]) chans[pi].StartPlaying(source, loops, position, fade, fadeMs, timeoutMs);
          return chans[pi];
        }
      case PlayPolicy.OldestPriority:
        lock(callback)
        {
          int pi=reserved, oi, prio=int.MaxValue, age=0;
          if(channel==FreeChannel)
          {
            for(int i=reserved; i<chans.Length; i++) if(chans[i].Priority<prio) { prio=chans[i].Priority; pi=i; }
            oi=pi;
            for(int i=reserved; i<chans.Length; i++)
              if(chans[i].Priority==prio && chans[i].Age>age) { oi=i; age=chans[i].Age; }
          }
          else
            lock(groups)
            {
              for(int i=0; i<group.Count; i++)
              {
                int chan = group[i];
                if(chan<reserved) continue;
                if(chans[chan].Priority<prio) { prio=chans[chan].Priority; pi=chan; }
              }
              oi=pi;
              for(int i=0; i<group.Count; i++)
              {
                int chan = group[i];
                if(chan<reserved) continue;
                if(chans[chan].Priority==prio && chans[chan].Age>age) { oi=chan; age=chans[chan].Age; }
              }
            }
          lock(chans[oi]) chans[oi].StartPlaying(source, loops, position, fade, fadeMs, timeoutMs);
          return chans[oi];
        }
      default: return null;
    }
  }

  internal static void CheckRate(float rate)
  {
    if(rate<0f) throw new ArgumentOutOfRangeException("PlaybackRate");
  }
  internal static void CheckChannel(int channel)
  {
    AssertInit();
    if(channel!=FreeChannel && (channel<0 || channel>=chans.Length)) throw new ArgumentOutOfRangeException("channel");
  }
  internal static void CheckVolume(int volume)
  {
    if(volume<0 || volume>Audio.MaxVolume) throw new ArgumentOutOfRangeException("volume");
  }

  static int ToGroup(int group) { return -group-2; }
  static List<int> GetGroup(int group)
  {
    AssertInit();
    int index = ToGroup(group);
    if(index<0 || index>=groups.Count) throw new ArgumentException("Invalid group ID");
    List<int> list = (List<int>)groups[ToGroup(group)];
    if(list==null) throw new ArgumentException("Invalid group ID");
    return list;
  }

  static void AssertInit()
  {
    if(!init) throw new InvalidOperationException("Audio has not been initialized.");
  }

  static unsafe void FillBuffer(int* stream, uint frames, IntPtr context)
  {
    try
    {
      lock(callback)
      {
        for(int i=0; i<chans.Length; i++) lock(chans[i]) chans[i].Mix(stream, (int)frames, filters);
        if(postFilters!=null)
          for(int i=0; i<postFilters.Count; i++) postFilters[i].MixFilter(null, stream, (int)frames, format);
        if(MixPolicy==MixPolicy.Divide) GLMixer.Check(GLMixer.DivideAccumulator(chans.Length));
      }
    }
    catch(Exception e)
    {
      if(Events.Events.Initialized)
        try { Events.Events.PushEvent(new Events.ExceptionEvent(Events.ExceptionLocation.AudioThread, e)); }
        catch { }
    }
  }

  static AudioFormat format;
  static FilterCollection filters, postFilters;
  static GLMixer.MixCallback callback;
  static Channel[] chans = new Channel[0];
  static List<List<int>> groups;
  static int reserved;
  static PlayPolicy playPolicy = PlayPolicy.Fail;
  static MixPolicy mixPolicy  = MixPolicy.DontDivide;
  static bool init;
}
#endregion

} // namespace GameLib.Audio