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
using System.Drawing;
using System.IO;
using GameLib.IO;

namespace GameLib.Video
{

enum PSDChannel { Alpha=-1, Red=0, Green=1, Blue=2 };

#region PSDLayer
public class PSDLayer : IDisposable
{ public PSDLayer(Rectangle bounds) : this(null, bounds, null) { }
  public PSDLayer(Rectangle bounds, string name) : this(null, bounds, name) { }
  public PSDLayer(Surface surface) : this(surface, surface.Bounds, null) { }
  public PSDLayer(Surface surface, string name) : this(surface, surface.Bounds, name) { }
  public PSDLayer(Surface surface, Rectangle bounds) : this(surface, bounds, null) { }
  public PSDLayer(Surface surface, Rectangle bounds, string name)
  { Surface=surface; Bounds=bounds; Name=name; Channels=surface.UsingAlpha ? 4 : 3; Blend="norm"; Opacity=255;
  }
  ~PSDLayer() { Dispose(true); }
  public void Dispose() { Dispose(false); GC.SuppressFinalize(this); }

  public int Width  { get { return Bounds.Width; } }
  public int Height { get { return Bounds.Height; } }

  public Rectangle Bounds;
  public Surface   Surface;
  public string    Name, Blend;
  public int       Channels;
  public byte      Opacity, Clipping, Flags;
  
  protected void Dispose(bool destructing)
  { if(Surface!=null)
    { Surface.Dispose();
      Surface = null;
    }
  }

  internal PSDLayer(Stream stream)
  { int y=IOH.ReadBE4(stream), x=IOH.ReadBE4(stream), y2=IOH.ReadBE4(stream), x2=IOH.ReadBE4(stream), chans;
    Bounds = new Rectangle(x, y, x2-x, y2-y);

    chans = IOH.ReadBE2(stream);
    PSDCodec.ValidateChannels(chans);
    channels = new PSDChannel[chans];
    for(int i=0; i<channels.Length; i++)
    { channels[i] = (PSDChannel)IOH.ReadBE2(stream);
      dataLength += IOH.ReadBE4(stream);
    }
    Channels = channels.Length;

    if(IOH.ReadString(stream, 4)!="8BIM") throw new ArgumentException("Unknown blend signature");
    Blend    = IOH.ReadString(stream, 4);
    Opacity  = IOH.Read1(stream);
    Clipping = IOH.Read1(stream);
    Flags    = IOH.Read1(stream);
    IOH.Skip(stream, 1); // reserved

    int extraBytes = IOH.ReadBE4(stream); // extra layer data
    int bytes = IOH.ReadBE4(stream); // layer mask size
    IOH.Skip(stream, bytes); extraBytes -= bytes+4;
    bytes = IOH.ReadBE4(stream); // layer blending size
    IOH.Skip(stream, bytes); extraBytes -= bytes+4;

    bytes = IOH.Read1(stream); extraBytes -= bytes+1;
    Name = IOH.ReadString(stream, bytes);
    IOH.Skip(stream, extraBytes);
  }

  internal PSDChannel[] channels;
  internal int dataLength, savedPos;
}
#endregion

#region PSDImage
public class PSDImage : IDisposable
{ ~PSDImage() { Dispose(true); }
  public void Dispose() { Dispose(false); }

  public PSDLayer[] Layers;
  public Surface Flattened;
  public int Width, Height, Channels;
  
  void Dispose(bool destructing)
  { if(Layers!=null) foreach(PSDLayer layer in Layers) layer.Dispose();
    if(Flattened!=null) { Flattened.Dispose(); Flattened=null; }
  }
}
#endregion

#region PSDCodec
public class PSDCodec
{ public PSDImage FinishReading()
  { AssertReading();
    PSDImage img = image;

    if(autoClose) stream.Close();
    image  = null;
    stream = null;
    state  = State.Nothing;
    return img;
  }

  public void FinishWriting()
  { AssertWriting();
    if(state!=State.Flattened) throw new ArgumentException("The flattened image has not been written yet.");
    Abort(); // this doubles as the finalizer
  }

  public PSDImage Read(string filename)
  { StartReading(filename);
    ReadRemainingLayers();
    ReadFlattened();
    return FinishReading();
  }
  public PSDImage Read(Stream stream)
  { StartReading(stream);
    ReadRemainingLayers();
    ReadFlattened();
    return FinishReading();
  }
  public PSDImage Read(Stream stream, bool autoClose)
  { StartReading(stream, autoClose);
    ReadRemainingLayers();
    ReadFlattened();
    return FinishReading();
  }

  public PSDLayer ReadNextLayer()
  { AssertLayerRead();
    try { ReadLayerData(image.Layers[layer]); return image.Layers[layer++]; }
    catch(Exception e) { Abort(); throw e; }
  }

  public Surface ReadFlattened()
  { if(state==State.Nothing || !reading) throw new InvalidOperationException("A read operation is not in progress");
    if(state!=State.Header) throw new InvalidOperationException("Invalid codec state");
    if(image.Layers!=null && layer<image.Layers.Length)
      throw new InvalidOperationException("Not all the layers have been read yet!");
    IOH.Skip(stream, savedPos-(int)stream.Position);
    try { return image.Flattened = ReadImageData(); }
    catch(Exception e) { Abort(); throw e; }
  }

  public PSDLayer[] ReadRemainingLayers()
  { AssertReading();
    if(image.Layers!=null) while(layer<image.Layers.Length) ReadNextLayer();
    return image.Layers;
  }

  public void SkipLayer()
  { AssertLayerRead();
    try { IOH.Skip(stream, image.Layers[layer++].dataLength); }
    catch(Exception e) { Abort(); throw e; }
  }

  public PSDImage StartReading(string filename)
  { return StartReading(File.Open(filename, FileMode.Open, FileAccess.Read));
  }
  public PSDImage StartReading(Stream stream) { return StartReading(stream, true); }
  public PSDImage StartReading(Stream stream, bool autoClose)
  { PSDImage img = new PSDImage();
    try
    { AssertNothing();
      if(IOH.ReadString(stream, 4) != "8BPS") throw new ArgumentException("Not a photoshop file");
      int value = IOH.ReadBE2U(stream);
      if(value != 1) throw new NotSupportedException("Unsupported PSD version number: "+value);
      IOH.Skip(stream, 6);
      img.Channels = IOH.ReadBE2U(stream);
      ValidateChannels(img.Channels);
      img.Height = IOH.ReadBE4(stream);
      img.Width  = IOH.ReadBE4(stream);
      value = IOH.ReadBE2U(stream);
      if(value != 8) throw new NotSupportedException("Unsupported channel depth: "+value);
      value = IOH.ReadBE2U(stream);
      if(value != 3) throw new NotSupportedException("Unsupported color mode: "+value);

      IOH.Skip(stream, IOH.ReadBE4(stream)); // skip color block
      IOH.Skip(stream, IOH.ReadBE4(stream)); // skip image resources

      int miscLen = IOH.ReadBE4(stream);
      savedPos = (int)stream.Position + miscLen;
      if(miscLen!=0) // length of miscellaneous info section
      { IOH.Skip(stream, 4);
        int numLayers = Math.Abs(IOH.ReadBE2(stream));
        if(numLayers==0) { IOH.Skip(stream, miscLen-6); goto done; }

        img.Layers = new PSDLayer[numLayers];
        for(int i=0; i<numLayers; i++) img.Layers[i] = new PSDLayer(stream);
      }
    }
    catch(Exception e) { if(autoClose) stream.Close(); img.Dispose(); throw e; }

    done:
    this.stream = stream;
    this.autoClose = autoClose;
    image = img;
    reading = true;
    layer = 0;
    state = State.Header;
    return image;
  }

  public void StartWriting(PSDImage image, string filename)
  { StartWriting(image, File.Open(filename, FileMode.Create), true);
  }
  public void StartWriting(PSDImage image, Stream stream) { StartWriting(image, stream, true); }
  public void StartWriting(PSDImage image, Stream stream, bool autoClose)
  { try
    { AssertNothing();
      if(image.Width<0 || image.Height<0) throw new ArgumentException("Image has invalid size");
      ValidateChannels(image.Channels);
      if(image.Layers!=null)
        foreach(PSDLayer layer in image.Layers)
        { ValidateChannels(layer.Channels);
          if(layer.Bounds.X<0 || layer.Bounds.Y<0 || layer.Bounds.Right>image.Width ||
             layer.Bounds.Bottom>image.Height)
            throw new ArgumentException("Invalid layer bounds: "+layer.Bounds.ToString());
          if(layer.Blend.Length!=4) throw new ArgumentException("Invalid blend mode: "+layer.Blend);
        }
      if(!stream.CanSeek) throw new ArgumentException("A seekable stream is required for PSD writing", "stream");

      IOH.WriteString(stream, "8BPS"); // signature
      IOH.WriteBE2(stream, 1); // version
      for(int i=0; i<6; i++) stream.WriteByte(0); // reserved
      IOH.WriteBE2(stream, (short)image.Channels); // channels
      IOH.WriteBE4(stream, image.Height);
      IOH.WriteBE4(stream, image.Width);
      IOH.WriteBE2(stream, 8); // bit depth (per channel)
      IOH.WriteBE2(stream, 3); // color mode (3=RGB)

      IOH.WriteBE4(stream, 0); // color data section
      IOH.WriteBE4(stream, 0); // psd resources section
      
      if(image.Layers!=null && image.Layers.Length>0)
      { savedPos = (int)stream.Position;
        IOH.WriteBE4(stream, 0); // size of the miscellaneous info section (to be filled later)
        IOH.WriteBE4(stream, 0); // size of the layer section (to be filled later)
        IOH.WriteBE2(stream, (short)image.Layers.Length); // number of layers (yes, it's negative for a reason)

        for(int i=0; i<image.Layers.Length; i++)
        { PSDLayer layer = image.Layers[i];
          IOH.WriteBE4(stream, layer.Bounds.Y); // layer top
          IOH.WriteBE4(stream, layer.Bounds.X); // layer left
          IOH.WriteBE4(stream, layer.Bounds.Bottom); // layer bottom
          IOH.WriteBE4(stream, layer.Bounds.Right);  // layer right
          IOH.WriteBE2(stream, (short)layer.Channels); // number of channels
          layer.savedPos = (int)stream.Position+2;
          for(short id=(short)(layer.Channels==4 ? -1 : 0); id<3; id++) // channel information
          { IOH.WriteBE2(stream, id); // channel ID
            IOH.WriteBE4(stream, 0);  // data length (to be filled later)
          }
          IOH.WriteString(stream, "8BIM"); // blend mode signature
          IOH.WriteString(stream, layer.Blend); // blend mode
          stream.WriteByte(layer.Opacity);  // opacity (255=opaque)
          stream.WriteByte(layer.Clipping); // clipping (0=base)
          stream.WriteByte(layer.Flags);    // flags
          stream.WriteByte(0); // reserved
          string name = layer.Name==null ? "Layer "+(i+1) : layer.Name;
          if(name.Length>255) name = name.Substring(0, 255);
          int extraLen = 8 + (name.Length+4)/4*4;
          IOH.WriteBE4(stream, extraLen); // size of the extra layer infomation
          IOH.WriteBE4(stream, 0); // layer mask
          IOH.WriteBE4(stream, 0); // layer blending size
          stream.WriteByte((byte)name.Length);
          IOH.WriteString(stream, name); // layer name
          if(((name.Length+1)&3) != 0) for(int j=4-((name.Length+1)&3); j>0; j--) stream.WriteByte(0); // name padding
        }
      }
      else IOH.WriteBE4(stream, 0); // misc info section
    }
    catch(Exception e) { if(autoClose) stream.Close(); throw e; }

    this.autoClose = autoClose;
    this.stream = stream;
    this.image = image;
    this.layer = 0;
    reading = false;
    state = image.Layers==null || image.Layers.Length==0 ? State.Layers : State.Header;
  }

  public void Write(PSDImage image, string filename) { Write(image, File.Open(filename, FileMode.Create)); }
  public void Write(PSDImage image, Stream stream) { Write(image, stream, true); }
  public void Write(PSDImage image, Stream stream, bool autoClose)
  { StartWriting(image, stream, autoClose);
    if(image.Layers!=null) foreach(PSDLayer layer in image.Layers) WriteLayer(layer.Surface);
    WriteFlattened(image.Flattened);
    FinishWriting();
  }

  public void Write(Surface surface, string filename) { Write(surface, File.Open(filename, FileMode.Create)); }
  public void Write(Surface surface, Stream stream) { Write(surface, stream, true); }
  public void Write(Surface surface, Stream stream, bool autoClose)
  { PSDImage image = new PSDImage();
    image.Width = surface.Width;
    image.Height = surface.Height;
    image.Channels = surface.UsingAlpha ? 4 : 3;
    image.Flattened = surface;
    Write(image, stream, autoClose);
  }
  
  public void WriteFlattened(Surface surface)
  { AssertWriting();
    if(state!=State.Layers) throw new InvalidOperationException("Not all the layers have been written yet!");
    if(surface==null) throw new ArgumentNullException("surface");
    if(surface.Depth<24) surface = surface.Clone(new PixelFormat(surface.UsingAlpha ? 32 : 24, surface.UsingAlpha));
    try { WriteImageData(surface); state=State.Flattened; }
    catch(Exception e) { Abort(); throw e; }
  }

  public void WriteLayer() { WriteLayer(null); }
  public void WriteLayer(Surface surface)
  { AssertLayerWrite();
    try
    { PSDLayer layer = image.Layers[this.layer++];
      int sw = surface==null ? 0 : surface.Width, sh = surface==null ? 0 : surface.Height;
      if(sw!=layer.Width || sh!=layer.Height) throw new ArgumentException("Surface size doesn't match layer size!");
      if(surface==null)
      { for(int i=0; i<layer.Channels; i++) IOH.WriteBE2(stream, 0);
        int endPos = (int)stream.Position;
        stream.Position = layer.savedPos;
        for(int i=0; i<layer.Channels; i++)
        { IOH.WriteBE4(stream, 2);
          stream.Position += 2;
        }
        stream.Position = endPos;
      }
      else
      { int[] sizes = new int[layer.Channels];
        if(surface.Depth<24)
          surface = surface.Clone(new PixelFormat(surface.UsingAlpha ? 32 : 24, surface.UsingAlpha));
        for(int i=0; i<layer.Channels; i++) sizes[i] = WriteChannelData(layer.Surface, i);
        int endPos = (int)stream.Position;
        stream.Position = layer.savedPos;
        for(int i=0; i<layer.Channels; i++)
        { IOH.WriteBE4(stream, sizes[i]);
          stream.Position += 2;
        }
        stream.Position = endPos;
      }

      if(this.layer==image.Layers.Length)
      { int endPos=(int)stream.Position, dist=endPos-savedPos-4;
        stream.Position = savedPos;
        IOH.WriteBE4(stream, dist); // miscellaneous info section size
        IOH.WriteBE4(stream, dist-4); // layer section size
        stream.Position = endPos;

        state = State.Layers;
      }
    }
    catch(Exception e) { Abort(); throw e; }
  }

  public static bool IsPSD(string filename)
  { return IsPSD(File.Open(filename, FileMode.Open, FileAccess.Read), true);
  }
  public static bool IsPSD(Stream stream) { return IsPSD(stream, false); }
  public static bool IsPSD(Stream stream, bool autoClose)
  { long position = autoClose ? 0 : stream.Position;
    if(stream.Length-stream.Position<6) return false;
    if(IOH.ReadString(stream, 4) != "8BPS") return false;
    if(IOH.ReadBE2U(stream) != 1) return false;
    if(autoClose) stream.Close();
    else stream.Position = position;
    return true;
  }

  public static PSDImage ReadPSD(string filename) { return new PSDCodec().Read(filename); }
  public static PSDImage ReadPSD(Stream stream) { return new PSDCodec().Read(stream); }
  public static PSDImage ReadPSD(Stream stream, bool autoClose) { return new PSDCodec().Read(stream, autoClose); }

  public static Surface ReadComposite(string filename)
  { PSDCodec codec = new PSDCodec();
    return ReadComposite(codec, codec.StartReading(filename));
  }
  public static Surface ReadComposite(Stream stream)
  { PSDCodec codec = new PSDCodec();
    return ReadComposite(codec, codec.StartReading(stream));
  }
  public static Surface ReadComposite(Stream stream, bool autoClose)
  { PSDCodec codec = new PSDCodec();
    return ReadComposite(codec, codec.StartReading(stream, autoClose));
  }

  public static void WritePSD(PSDImage image, string filename) { new PSDCodec().Write(image, filename); }
  public static void WritePSD(PSDImage image, Stream stream) { new PSDCodec().Write(image, stream); }
  public static void WritePSD(PSDImage image, Stream stream, bool autoClose)
  { new PSDCodec().Write(image, stream, autoClose);
  }

  public static void WritePSD(Surface surface, string filename) { new PSDCodec().Write(surface, filename); }
  public static void WritePSD(Surface surface, Stream stream) { new PSDCodec().Write(surface, stream); }
  public static void WritePSD(Surface surface, Stream stream, bool autoClose)
  { new PSDCodec().Write(surface, stream, autoClose);
  }

  static Surface ReadComposite(PSDCodec codec, PSDImage image)
  { if(image.Layers!=null) for(int i=0; i<image.Layers.Length; i++) codec.SkipLayer();
    Surface composite = codec.ReadFlattened();
    codec.FinishReading();
    return composite;
  }

  static internal void ValidateChannels(int channels)
  { if(channels<3 && channels>4) throw new NotSupportedException("Unsupported number of channels: "+channels);
  }

  void Abort()
  { if(autoClose) stream.Close();
    if(reading && image!=null) image.Dispose();

    stream  = null;
    image   = null;
    state   = State.Nothing;
    reading = false;
  }

  void AssertLayerRead()
  { AssertReading();
    if(state!=State.Header) throw new InvalidOperationException("Invalid codec state.");
    if(image.Layers==null || layer>=image.Layers.Length)
      throw new InvalidOperationException("All the layers have already been read!");
  }
  
  void AssertLayerWrite()
  { AssertWriting();
    if(state!=State.Header) throw new InvalidOperationException("Invalid codec state.");
    if(layer>=image.Layers.Length) throw new InvalidOperationException("All the layers have already been written!");
  }

  void AssertNothing()
  { if(state!=State.Nothing)
      throw new InvalidOperationException("A read or write operation is currently in progress");
  }
  
  void AssertReading()
  { if(state==State.Nothing || !reading) throw new InvalidOperationException("A read operation is not in progress.");
  }
  
  void AssertWriting()
  { if(state==State.Nothing || reading) throw new InvalidOperationException("A write operation is not in progress.");
  }
  
  Surface ReadImageData()
  { PSDChannel[] channels = new PSDChannel[image.Channels];
    for(int i=0,id=channels.Length==4 ? -1 : 0; id<3; i++,id++) channels[i] = (PSDChannel)id;
    return ReadImageData(image.Width, image.Height, channels, false);
  }

  unsafe Surface ReadImageData(int width, int height, PSDChannel[] chans, bool layer)
  { Surface surface = new Surface(width, height, chans.Length==3 ? 24 : 32,
                                  chans.Length==3 ? SurfaceFlag.None : SurfaceFlag.SrcAlpha);

    byte[] linebuf=null;
    int[]  lengths=null;
    bool   compressed=false;
    int    maxlen, yi=0, xinc=chans.Length, yinc = (int)surface.Pitch - width*xinc;

    surface.Lock();
    try
    { for(int chan=0; chan<chans.Length; chan++)
      { if(layer || chan==0)
        { int value = IOH.ReadBE2(stream);
          if(value!=0 && value!=1) throw new NotSupportedException("Unsupported compression type: "+value);
          compressed = value==1;
        }

        byte* dest = (byte*)surface.Data;
        switch(chans[chan])
        { case PSDChannel.Alpha: dest += MaskToOffset(surface.Format.AlphaMask); break;
          case PSDChannel.Red:   dest += MaskToOffset(surface.Format.RedMask); break;
          case PSDChannel.Green: dest += MaskToOffset(surface.Format.GreenMask); break;
          case PSDChannel.Blue:  dest += MaskToOffset(surface.Format.BlueMask); break;
        }

        if(compressed)
        { if(layer || chan==0)
          { if(lengths==null) lengths = new int[layer ? height : height*chans.Length];
            maxlen = 0;
            for(int y=0; y<lengths.Length; y++)
            { lengths[y] = IOH.ReadBE2U(stream);
              if(lengths[y]>maxlen) maxlen=lengths[y];
            }
            linebuf = new byte[maxlen];
          }
          fixed(byte* lbptr=linebuf)
            for(int yend=yi+height; yi<yend; yi++)
            { byte* src = lbptr;
              int  f;
              byte b;
              IOH.Read(stream, linebuf, lengths[yi]);
              for(int i=0; i<width;)
              { f=*src++;
                if(f>=128)
                { if(f==128) continue;
                  f=257-f;
                  b=*src++;
                  i += f;
                  do { *dest=b; dest+=xinc; } while(--f != 0);
                }
                else
                { i += ++f;
                  do { *dest=*src++; dest+=xinc; } while(--f != 0);
                }
              }
              dest += yinc;
            }
          if(layer) yi=0;
        }
        else
        { int length = width*height;
          if(length<=65536)
          { byte[] data = new byte[length];
            IOH.Read(stream, data);
            fixed(byte* sdata=data)
            { byte* src=sdata;
              if(yinc==0) do { *dest=*src++; dest+=xinc; } while(--length != 0);
              else
                for(int y=0; y<height; y++)
                { int xlen = width;
                  do { *dest=*src++; dest+=xinc; } while(--xlen != 0);
                  dest += yinc;
                }
            }
          }
          else
          { byte[] data = new byte[width];
            fixed(byte* sdata=data)
              for(int y=0; y<height; y++)
              { int xlen = width;
                IOH.Read(stream, data);
                byte* src=sdata;
                do { *dest=*src++; dest+=xinc; } while(--xlen != 0);
                dest += yinc;
              }
          }
        }
      }
    }
    finally { surface.Unlock(); }
    return surface;
  }

  Surface ReadLayerData(PSDLayer layer)
  { return layer.Surface = ReadImageData(layer.Width, layer.Height, layer.channels, true);
  }

  int WriteChannelData(Surface surface, int channel)
  { surface.Lock();
    try
    { int thresh=surface.Width*surface.Height, pos=(int)stream.Position;
      int len = WritePackedBits(surface, channel, thresh, 0);
      if(len==-1)
      { stream.Position=pos;
        stream.SetLength(pos);
        WriteRawBits(surface, channel, true);
        return thresh+2;
      }
      else return len+2;
    }
    finally { surface.Unlock(); }
  }

  void WriteImageData(Surface surface)
  { surface.Lock();
    try
    { int chans=surface.UsingAlpha?4:3, thresh = surface.Width*surface.Height*chans, pos = (int)stream.Position;
      int len = surface.Height*2*chans;

      IOH.WriteBE2(stream, 1); // packbits compression
      stream.Position += len;
      for(int i=0; i<chans; i++)
      { len += WritePackedBits(surface, i, 0, pos+2+i*surface.Height*2);
        if(len>thresh) { len=-1; break; }
      }

      if(len==-1)
      { stream.Position=pos;
        stream.SetLength(pos);
        for(int i=0; i<chans; i++) WriteRawBits(surface, i, false);
      }
    }
    finally { surface.Unlock(); }
  }

  unsafe int WritePackedBits(Surface surface, int channel, int thresh, int table)
  { ushort[] lengths = new ushort[surface.Height];
    int length = surface.Height*2;

    if(table==0)
    { IOH.WriteBE2(stream, 1); // packbits compression (not included in 'length')
      table = (int)stream.Position;
      stream.Position += length;
    }

    byte[] data = new byte[128];
    byte* src = (byte*)surface.Data;
    int xinc=surface.Depth/8, yinc=surface.Pitch-surface.Width*xinc;
    switch(channel+(surface.UsingAlpha?0:1))
    { case 0: src += MaskToOffset(surface.Format.AlphaMask); break;
      case 1: src += MaskToOffset(surface.Format.RedMask); break;
      case 2: src += MaskToOffset(surface.Format.GreenMask); break;
      case 3: src += MaskToOffset(surface.Format.BlueMask); break;
    }

    fixed(byte* ddata=data)
      for(int y=0; y<surface.Height; y++)
      { byte* usrc=src;
        int   x=0, width=surface.Width, urun=0, prun, linelen=0;
        byte  v;

        do
        { if(urun>=128)
          { byte *dest=ddata;
            int olen=urun, len;
            if(urun>128) urun=128;
            len=urun;
            do { *dest++=*usrc; usrc+=xinc; } while(--urun!=0);
            stream.WriteByte((byte)(len-1));
            stream.Write(data, 0, len);
            linelen += len+1;
            urun = olen>128 ? olen-128 : 0;
            usrc = src;
          }

          v = *src; src+=xinc; prun = 1;
          while(++x<width && prun<128 && *src==v) { src+=xinc; prun++; }
          if(prun>2)
          { byte *dest=ddata;
            int len;
            if(urun>0)
            { len=urun;
              do { *dest++=*usrc; usrc+=xinc; } while(--urun!=0);
              stream.WriteByte((byte)(len-1));
              stream.Write(data, 0, len);
              linelen += len+1;
              dest=ddata;
            }
            len=prun;
            stream.WriteByte((byte)-(prun-1));
            stream.WriteByte(v);
            linelen += 2;
            usrc = src;
          }
          else urun += prun;
        } while(x<width);

        while(urun>0)
        { byte *dest=ddata;
          int olen=urun, len;
          if(urun>128) urun=128;
          len=urun;
          do { *dest++=*usrc; usrc+=xinc; } while(--urun!=0);
          stream.WriteByte((byte)(len-1));
          stream.Write(data, 0, len);
          linelen += len+1;
          urun = olen-128;
        }

        lengths[y] = (ushort)linelen;
        length += linelen;
        if(thresh>0 && length>thresh) return -1;

        src += yinc;
      }
    
    int pos = (int)stream.Position;
    stream.Position = table;
    for(int y=0; y<surface.Height; y++) IOH.WriteBE2U(stream, lengths[y]);
    stream.Position = pos;
    return length;
  }

  unsafe void WriteRawBits(Surface surface, int channel, bool layer)
  { byte[] data = new byte[1024];
    if(layer || channel==0) IOH.WriteBE2(stream, 0); // no compression
    int xinc=surface.Depth/8, yinc=surface.Pitch-surface.Width*xinc, blen=0;
    byte* src = (byte*)surface.Data;
    switch(channel+(surface.UsingAlpha?0:1))
    { case 0: src += MaskToOffset(surface.Format.AlphaMask); break;
      case 1: src += MaskToOffset(surface.Format.RedMask); break;
      case 2: src += MaskToOffset(surface.Format.GreenMask); break;
      case 3: src += MaskToOffset(surface.Format.BlueMask); break;
    }
    fixed(byte* ddata=data)
    { byte* dest=ddata;
      for(int y=0; y<surface.Height; y++)
      { int length=surface.Width;
        do
        { *dest++ = *src;
          src += xinc;
          if(++blen==1024)
          { stream.Write(data, 0, 1024);
            blen = 0;
            dest = ddata;
          }
        } while(--length != 0);
        src += yinc;
      }
    }
    if(blen>0) stream.Write(data, 0, blen);
  }

  enum State { Nothing, Header, Layers, Flattened };
  Stream   stream;
  PSDImage image;
  int      layer, savedPos;
  State    state;
  bool     autoClose, reading;

  static int MaskToOffset(uint mask) // TODO: make this big-endian safe
  { int i=0;
    while(mask!=255)
    { if(mask==0) throw new InvalidOperationException("Invalid color mask.");
      mask>>=8;
      i++;
    }
    return i;
  }
}
#endregion

} // namespace GameLib.Video