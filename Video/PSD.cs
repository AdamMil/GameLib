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
using System.Drawing;
using System.IO;
using AdamMil.IO;

namespace GameLib.Video
{

enum PSDChannel { Alpha=-1, Red=0, Green=1, Blue=2 };

#region PSDLayer
public sealed class PSDLayer
{ 
  /// <summary>Initializes this layer as being empty, with zero bounds.</summary>
  public PSDLayer() : this(null, new Rectangle(), null) { }
  /// <summary>Initializes this layer with the specified bounds.</summary>
  /// <param name="bounds">A <see cref="Rectangle"/> specifying the size and location of the layer in the PSD image.</param>
  public PSDLayer(Rectangle bounds) : this(null, bounds, null) { }
  /// <summary>Initializes this layer with the specified bounds and name.</summary>
  /// <param name="bounds">A <see cref="Rectangle"/> specifying the size and location of the layer in the PSD image.</param>
  /// <param name="name">The name of the layer, or null if the layer name should be auto-generated during writing.</param>
  public PSDLayer(Rectangle bounds, string name) : this(null, bounds, name) { }
  /// <summary>Initializes this layer with the specified surface data, using the
  /// <see cref="GameLib.Video.Surface.Bounds"/> property for the layer boundaries.
  /// </summary>
  /// <param name="surface">The <see cref="GameLib.Video.Surface"/> containing the image data for this layer.</param>
  public PSDLayer(Surface surface) : this(surface, surface.Bounds, null) { }
  /// <summary>Initializes this layer with the specified surface data and name, using the
  /// <see cref="GameLib.Video.Surface.Bounds"/> property for the layer boundaries.
  /// </summary>
  /// <param name="surface">The <see cref="GameLib.Video.Surface"/> containing the image data for this layer.</param>
  /// <param name="name">The name of the layer, or null if the layer name should be auto-generated during writing.</param>
  public PSDLayer(Surface surface, string name) : this(surface, surface.Bounds, name) { }
  /// <summary>Initializes this layer with the specified surface data, using the <paramref name="bounds"/> parameter
  /// to set the layer boundaries.
  /// </summary>
  /// <param name="surface">The <see cref="GameLib.Video.Surface"/> containing the image data for this layer.</param>
  /// <param name="bounds">A <see cref="Rectangle"/> specifying the size and location of the layer in the PSD image.</param>
  public PSDLayer(Surface surface, Rectangle bounds) : this(surface, bounds, null) { }
  /// <summary>Initializes this layer with the specified surface data and name, using the <paramref name="bounds"/>
  /// parameter to set the layer boundaries.
  /// </summary>
  /// <param name="surface">The <see cref="GameLib.Video.Surface"/> containing the image data for this layer.</param>
  /// <param name="bounds">A <see cref="Rectangle"/> specifying the size and location of the layer in the PSD image.</param>
  /// <param name="name">The name of the layer, or null if the layer name should be auto-generated during writing.</param>
  public PSDLayer(Surface surface, Rectangle bounds, string name)
  { Surface=surface; Bounds=bounds; Name=name; Channels = surface==null ? 0 : surface.Format.AlphaMask==0 ? 3 : 4;
  }

  /// <summary>Gets or sets the width of the layer.</summary>
  /// <remarks>This property gets/sets the <see cref="Rectangle.Width"/> property of <see cref="Bounds"/>.</remarks>
  public int Width { get { return Bounds.Width; } set { Bounds.Width=value; } }
  /// <summary>Gets or sets the height of the layer.</summary>
  /// <remarks>This property gets/sets the <see cref="Rectangle.Height"/> property of <see cref="Bounds"/>.</remarks>
  public int Height { get { return Bounds.Height; } set { Bounds.Height=value; } }
  /// <summary>Gets or sets the location of the top-left corner of the layer within the PSD image.</summary>
  /// <remarks>This property gets/sets the <see cref="Rectangle.Location"/> property of <see cref="Bounds"/>.</remarks>
  public Point Location { get { return Bounds.Location; } set { Bounds.Location=value; } }
  /// <summary>Gets or sets the size of the layer.</summary>
  /// <remarks>This property gets/sets the <see cref="Rectangle.Size"/> property of <see cref="Bounds"/>.</remarks>
  public Size Size { get { return Bounds.Size; } set { Bounds.Size=value; } }

  /// <summary>The bounds of the layer within the PSD image.</summary>
  /// <value>A <see cref="Rectangle"/> holding the size and location of the layer within the PSD image.</value>
  /// <remarks>Layers aren't always the same size as the PSD image. In fact, they are generally the smallest they can
  /// be while still including all non-transparent pixels.
  /// </remarks>
  public Rectangle Bounds;
  /// <summary>The image data for this layer.</summary>
  /// <value>A <see cref="GameLib.Video.Surface"/> containing the image data for this layer, or null if the layer is
  /// empty.
  /// </value>
  /// <remarks>Layers can be empty, having no data. If a layer is empty, this field should be null, and
  /// <see cref="Bounds"/> should specify an empty rectangle.
  /// </remarks>
  public Surface Surface;
  /// <summary>The name of the layer.</summary>
  /// <value>A string holding the name of the layer, or null if the name should be auto-generated during writing.</value>
  /// <remarks>When reading a layer, this value will contain the layer's name. When writing a layer, you can set this
  /// field to null and the layer's name will be auto-generated.
  /// </remarks>
  public string Name;
  /// <summary>The blend mode of the layer.</summary>
  /// <value>A string holding the layer's blending mode.</value>
  /// <remarks>The layer blending modes are not all documented, and are currently ignored (but preseved) by the
  /// <see cref="PSDCodec"/> class. The default is "norm", which means that no special blending mode is applied.
  /// </remarks>
  public string Blend="norm";
  /// <summary>The number of channels in the layer's image data.</summary>
  /// <value>This should be 3 or 4, depending on whether the layer has an alpha channel, and should be set even if
  /// the layer is empty (has a width and height of zero).
  /// </value>
  public int Channels;
  /// <summary>The opacity of the layer.</summary>
  /// <value>A value, ranging from 0 (transparent) to 255 (opaque), applied in addition to the layer's alpha channel.</value>
  /// <remarks>The layer opacity is currently ignored (but preseved) by the <see cref="PSDCodec"/> class. The default
  /// is 255 (opaque).
  /// </remarks>
  public byte Opacity=255;
  /// <summary>The clipping value of the layer.</summary>
  /// <remarks>The clipping values are not documented, and are currently ignored (but preseved) by the
  /// <see cref="PSDCodec"/> class. The default is zero.
  /// </remarks>
  public byte Clipping;
  /// <summary>The flags for the layer.</summary>
  /// <remarks>The layer flags are not documented, and are currently ignored (but preseved) by the
  /// <see cref="PSDCodec"/> class. The default is zero.
  /// </remarks>
  public byte Flags;

  internal PSDLayer(Stream stream)
  {
    int y=stream.ReadBE4(), x=stream.ReadBE4(), y2=stream.ReadBE4(), x2=stream.ReadBE4(), chans;
    Bounds = new Rectangle(x, y, x2-x, y2-y);

    chans = stream.ReadBE2();
    channels = new PSDChannel[chans];
    for(int i=0; i<channels.Length; i++)
    {
      channels[i] = (PSDChannel)stream.ReadBE2();
      dataLength += stream.ReadBE4();
    }
    Channels = channels.Length;

    if(stream.ReadString(4)!="8BIM") throw new ArgumentException("Unknown blend signature");
    Blend    = stream.ReadString(4);
    Opacity  = stream.ReadByteOrThrow();
    Clipping = stream.ReadByteOrThrow();
    Flags    = stream.ReadByteOrThrow();
    stream.Skip(1); // reserved

    int extraBytes = stream.ReadBE4(); // extra layer data
    int bytes = stream.ReadBE4(); // layer mask size
    stream.Skip(bytes); extraBytes -= bytes+4;
    bytes = stream.ReadBE4(); // layer blending size
    stream.Skip(bytes); extraBytes -= bytes+4;

    bytes = stream.ReadByteOrThrow(); extraBytes -= bytes+1;
    Name = bytes==0 ? "" : stream.ReadString(bytes);
    stream.Skip(extraBytes);
  }

  internal PSDChannel[] channels;
  internal int dataLength, savedPos;
}
#endregion

#region PSDImage
/// <summary>This class represents a PSD format image.</summary>
public sealed class PSDImage
{ 
  /// <summary>Gets or sets the size of the flattened image.</summary>
  /// <remarks>This property gets/sets the <see cref="Width"/> and <see cref="Height"/> properties.</remarks>
  public Size Size { get { return new Size(Width, Height); } set { Width=value.Width; Height=value.Height; } }

  /// <summary>This field holds the layers for this PSD image.</summary>
  /// <value>A <see cref="PSDLayer"/> array, or possibly null if the image has no layers.</value>
  public PSDLayer[] Layers;
  /// <summary>The flattened image for this PSD image.</summary>
  /// <value>A <see cref="Surface"/> holding the data for the flattened image, or null.</value>
  public Surface Flattened;
  /// <summary>The width of the flattened image, in pixels.</summary>
  /// <remarks>This may seem redundant with the <see cref="Flattened"/> field, but it allows the width to be
  /// specified without having a surface of that width taking up memory.
  /// </remarks>
  public int Width;
  /// <summary>The height of the flattened image, in pixels.</summary>
  /// <remarks>This may seem redundant with the <see cref="Flattened"/> field, but it allows the height to be
  /// specified without having a surface of that height taking up memory.
  /// </remarks>
  public int Height;
  /// <summary>The number of channels in the flattened image.</summary>
  /// <value>This should be 3 or 4, depending on whether the flattened image has an alpha channel.</value>
  public int Channels;
}
#endregion

#region PSDCodec
/// <summary>This class provides a decoder/encoder for Photoshop (.PSD) format files.</summary>
/// <remarks>
/// <para>This codec was developed using information from the most recent publically-released information regarding
/// the PSD file format that was available at the time, and has a number of limitations. It only supports 8-bit
/// channel depths, 3 or 4 channel image data, and RGB color mode. Also, when loading a PSD image into a
/// <see cref="Surface"/>, blend modes, layer masks, and other fancy (and undocumented or poorly-documented) features
/// are ignored.
/// </para>
/// <para>There are a couple ways to read/write PSD files with this codec. The simplest way is to call the
/// <see cref="ReadPSD"/> or <see cref="WritePSD"/> functions, which read/write a <see cref="PSDImage"/> class
/// containing an entire image with all its layers. PSD files can also be read/written layer by layer (with the
/// ability to skip layers when reading) for higher performance and/or lower memory requirements.
/// <see cref="ReadComposite"/> can be called if you just want to read the flattened (composite) image from a PSD file.
/// </para>
/// <para>When reading or writing, a <see cref="PSDImage"/> object is held internally, and it is referenced by all
/// stages. The same <see cref="PSDImage"/> object and its layer information is returned by read functions at each
/// step of the way. The codec works as a state machine, requiring certain steps to be completed before other steps
/// can be taken. For reading, the following steps must be taken:
/// <list type="number">
///  <item><description>Initialize the codec with <see cref="StartReading"/>.</description></item>
///  <item><description>For each layer in the <see cref="PSDImage"/>, read or skip the corresponding layer with
///   <see cref="ReadLayer"/> or <see cref="SkipLayer"/>. You can read or skip all remaining layers with
///   <see cref="ReadLayers"/> or <see cref="SkipLayers"/>.
///  </description></item>
///  <item><description>Read the flattened image with <see cref="ReadFlattened"/>.</description></item>
///  <item><description>Finish reading with <see cref="FinishReading"/>.</description></item>
/// </list>
/// </para>
/// <para><see cref="Read"/> or <see cref="ReadPSD"/> can be used to do all of the above steps with a
/// single function call. Also, <see cref="ReadComposite"/> can be used to quickly get just the flattened image from a
/// PSD. Finally, <see cref="FinishReading"/> can actually be called at any point after <see cref="StartReading"/> to
/// end the reading process (without reading the rest of the data).
/// </para>
/// <para>For writing, the following steps must be taken:
/// <list type="number">
///  <item><description>Create a <see cref="PSDImage"/> class and populate all fields, including the layers. The
///   actual image data (the <see cref="PSDLayer.Surface"/> and <see cref="PSDImage.Flattened"/>fields) is not
///   required, however.
///  </description></item>
///  <item><description>Call <see cref="StartWriting"/> with the <see cref="PSDImage"/> class to write the header
///   and layer headers.
///  </description></item>
///  <item><description>For each layer, call <see cref="WriteLayer"/> to write the layer data.</description></item>
///  <item><description>Call <see cref="WriteFlattened"/> to write the composite image.</description></item>
///  <item><description>Call <see cref="FinishWriting"/> to finish writing the PSD file.</description></item>
/// </list>
/// </para>
/// <para><see cref="Write"/> or <see cref="WritePSD"/> can be used to do the above steps with a single function call.
/// </para>
/// </remarks>
public sealed class PSDCodec
{ 
  /// <summary>Determines whether the codec is currently in a reading mode.</summary>
  /// <remarks>The codec is in a reading mode if <see cref="StartReading"/> has been called to begin reading, and
  /// <see cref="FinishReading"/> has not yet been called to end reading.
  /// </remarks>
  public bool Reading { get { return state!=State.Nothing && reading; } }
  /// <summary>Determines whether the codec is currently in a writing mode.</summary>
  /// <remarks>The codec is in a writing mode if <see cref="StartWriting"/> has been called to begin writing, and
  /// <see cref="FinishWriting"/> has not yet been called to end writing.
  /// </remarks>
  public bool Writing { get { return state!=State.Nothing && !reading; } }

  /// <summary>Ends the reading mode and returns the <see cref="PSDImage"/> containing the PSD image data.</summary>
  /// <returns>The <see cref="PSDImage"/> containing the PSD image data.</returns>
  /// <remarks>This method does not finish reading image data from the PSD file. It ends the reading process at
  /// whatever stage it happens to be at and returns the <see cref="PSDImage"/> with whatever data it happens to have
  /// in it.
  /// </remarks>
  /// <exception cref="InvalidOperationException">Thrown if the codec is not in a reading mode.</exception>
  public PSDImage FinishReading()
  { AssertReading();
    PSDImage img = image;
    Abort(); // completes the process
    return img;
  }

  /// <summary>Finishes writing the PSD file and ends the writing mode.</summary>
  /// <remarks>This method requires the flattened image to have been written. This can be done with
  /// <see cref="WriteFlattened"/>.
  /// </remarks>
  /// <exception cref="InvalidOperationException">
  /// Thrown if
  /// <para>The codec is not in a writing mode.</para>
  /// -or-
  /// <para>The flattened image has not been written yet.</para>
  /// </exception>
  public void FinishWriting()
  { AssertWriting();
    if(state!=State.Flattened) throw new InvalidOperationException("The flattened image has not been written yet.");
    Abort(); // completes the process
  }

  /// <param name="filename">The path to a PSD image file.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Read/*"/>
  public PSDImage Read(string filename)
  { StartReading(filename);
    ReadLayers();
    ReadFlattened();
    return FinishReading();
  }
  /// <param name="stream">A stream containing PSD image data. The stream does not need to be seekable.
  /// The stream will be closed automatically.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Read/*"/>
  public PSDImage Read(Stream stream)
  { StartReading(stream);
    ReadLayers();
    ReadFlattened();
    return FinishReading();
  }
  /// <param name="stream">A stream containing PSD image data. The stream does not need to be seekable.</param>
  /// <param name="autoClose">If true, <paramref name="stream"/> will be closed when the reading process finishes.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Read/*"/>
  public PSDImage Read(Stream stream, bool autoClose)
  { StartReading(stream, autoClose);
    ReadLayers();
    ReadFlattened();
    return FinishReading();
  }

  /// <summary>Reads the current layer and advances to the next layer, or to the flattened image.</summary>
  /// <returns>The <see cref="PSDLayer"/> in <see cref="PSDImage.Layers"/> that contains the data for this layer.
  /// </returns>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/LayerReading/*"/>
  public PSDLayer ReadLayer()
  { AssertLayerRead();
    try
    { PSDLayer layer = image.Layers[this.layer];
      if(layer.Width==0 || layer.Height==0) SkipLayer(); // SkipLayer increments 'this.layer'
      else
      { int end = (int)stream.Position + layer.dataLength;
        layer.Surface = ReadImageData(layer.Width, layer.Height, layer.channels, true);
        stream.Skip((int)stream.Position-end); // skip over any padding or extra data after the bitmap
        this.layer++;
      }
      return layer;
    }
    catch { Abort(); throw; }
  }

  /// <summary>Reads the flattened image.</summary>
  /// <returns>The <see cref="PSDImage.Flattened"/> field of the <see cref="PSDImage"/>, which contains the image data
  /// for the composite image.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown if
  /// <para>The codec is not in a reading mode.</para>
  /// -or-
  /// <para>Not all layers have been read yet.</para>
  /// </exception>
  public Surface ReadFlattened()
  { AssertReading();
    if(state!=State.Header) throw new InvalidOperationException("Invalid codec state");
    if(image.Layers!=null && layer<image.Layers.Length)
      throw new InvalidOperationException("Not all the layers have been read yet!");
    stream.Skip(savedPos-(int)stream.Position);
    ValidateChannels(image.Channels);
    try { return image.Flattened = ReadImageData(); }
    catch { Abort(); throw; }
  }

  /// <summary>Reads all remaining layers from the PSD file.</summary>
  /// <returns>The <see cref="PSDImage.Layers"/> field of the <see cref="PSDImage"/>, which contains all the layer
  /// information.
  /// </returns>
  /// <exception cref="InvalidOperationException">Thrown if the codec is not in a reading mode.</exception>
  public PSDLayer[] ReadLayers()
  { AssertReading();
    if(image.Layers!=null) while(layer<image.Layers.Length) ReadLayer();
    return image.Layers;
  }

  /// <summary>Skips the current layer.</summary>
  /// <returns>The <see cref="PSDLayer"/> in <see cref="PSDImage.Layers"/> that contains the layer information for
  /// this layer. The layer's <see cref="PSDLayer.Surface"/> field will be null after calling this.
  /// </returns>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/LayerReading/*"/>
  public PSDLayer SkipLayer()
  { AssertLayerRead();
    try
    { PSDLayer layer = image.Layers[this.layer++];
      layer.Surface=null;
      stream.Skip(layer.dataLength);
      return layer;
    }
    catch { Abort(); throw; }
  }

  /// <summary>Skips all remaining layers in the PSD file.</summary>
  /// <returns>The <see cref="PSDImage.Layers"/> field of the <see cref="PSDImage"/>, which contains all the layer
  /// information.
  /// </returns>
  /// <exception cref="InvalidOperationException">Thrown if the codec is not in a reading mode.</exception>
  public PSDLayer[] SkipLayers()
  { AssertReading();
    if(image.Layers!=null) while(layer<image.Layers.Length) SkipLayer();
    return image.Layers;
  }

  /// <param name="filename">The path to a PSD image file.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/StartReading/*"/>
  public PSDImage StartReading(string filename)
  { return StartReading(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
  }
  /// <param name="stream">A stream containing PSD image data. The stream does not need to be seekable.
  /// The stream will be closed automatically.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/StartReading/*"/>
  public PSDImage StartReading(Stream stream) { return StartReading(stream, true); }
  /// <param name="stream">A stream containing PSD image data. The stream does not need to be seekable.</param>
  /// <param name="autoClose">If true, <paramref name="stream"/> will be closed when the reading process finishes.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/StartReading/*"/>
  public PSDImage StartReading(Stream stream, bool autoClose)
  { PSDImage img = new PSDImage();
    try
    { AssertNothing();
      if(stream.ReadString(4) != "8BPS") throw new ArgumentException("Not a photoshop file");
      int value = stream.ReadBE2U();
      if(value != 1) throw new NotSupportedException("Unsupported PSD version number: "+value);
      stream.Skip(6);
      img.Channels = stream.ReadBE2U();
      img.Height = stream.ReadBE4();
      img.Width  = stream.ReadBE4();
      value = stream.ReadBE2U();
      if(value != 8) throw new NotSupportedException("Unsupported channel depth: "+value);
      value = stream.ReadBE2U();
      if(value != 3) throw new NotSupportedException("Unsupported color mode: "+value);

      stream.Skip(stream.ReadBE4()); // skip color block
      stream.Skip(stream.ReadBE4()); // skip image resources

      int miscLen = stream.ReadBE4();
      savedPos = (int)stream.Position + miscLen;
      if(miscLen!=0) // length of miscellaneous info section
      {
        stream.Skip(4);
        int numLayers = Math.Abs(stream.ReadBE2());
        if(numLayers==0) { stream.Skip(miscLen-6); goto done; }

        img.Layers = new PSDLayer[numLayers];
        for(int i=0; i<numLayers; i++) img.Layers[i] = new PSDLayer(stream);
      }
    }
    catch { if(autoClose) stream.Close(); throw; }

    done:
    this.stream = stream;
    this.autoClose = autoClose;
    image = img;
    reading = true;
    layer = 0;
    state = State.Header;
    return image;
  }

  /// <param name="filename">A path to the file into which the PSD data will be written.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/StartWriting/*"/>
  public void StartWriting(PSDImage image, string filename)
  { StartWriting(image, File.Open(filename, FileMode.Create), true);
  }
  /// <param name="stream">A stream into which PSD image data will be written. The stream must be seekable, with its
  /// entire range devoted to the PSD data for this image. The stream will be closed automatically.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/StartWriting/*"/>
  public void StartWriting(PSDImage image, Stream stream) { StartWriting(image, stream, true); }
  /// <param name="stream">A stream into which PSD image data will be written. The stream must be seekable, with its
  /// entire range devoted to the PSD data for this image.
  /// </param>
  /// <param name="autoClose">If true, <paramref name="stream"/> will be closed when the writing process finishes.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/StartWriting/*"/>
  public void StartWriting(PSDImage image, Stream stream, bool autoClose)
  { try
    { AssertNothing();
      if(image==null || stream==null) throw new ArgumentNullException();
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

      stream.WriteString("8BPS"); // signature
      stream.WriteBE2(1); // version
      for(int i=0; i<6; i++) stream.WriteByte(0); // reserved
      stream.WriteBE2((short)image.Channels); // channels
      stream.WriteBE4(image.Height);
      stream.WriteBE4(image.Width);
      stream.WriteBE2(8); // bit depth (per channel)
      stream.WriteBE2(3); // color mode (3=RGB)

      stream.WriteBE4(0); // color data section
      stream.WriteBE4(0); // psd resources section

      if(image.Layers!=null && image.Layers.Length>0)
      {
        savedPos = (int)stream.Position;
        stream.WriteBE4(0); // size of the miscellaneous info section (to be filled later)
        stream.WriteBE4(0); // size of the layer section (to be filled later)
        stream.WriteBE2((short)image.Layers.Length); // number of layers

        for(int i=0; i<image.Layers.Length; i++)
        {
          PSDLayer layer = image.Layers[i];
          stream.WriteBE4(layer.Bounds.Y); // layer top
          stream.WriteBE4(layer.Bounds.X); // layer left
          stream.WriteBE4(layer.Bounds.Bottom); // layer bottom
          stream.WriteBE4(layer.Bounds.Right);  // layer right
          stream.WriteBE2((short)layer.Channels); // number of channels
          layer.savedPos = (int)stream.Position+2;
          for(short id=(short)(layer.Channels==4 ? -1 : 0); id<3; id++) // channel information
          {
            stream.WriteBE2(id); // channel ID
            stream.WriteBE4(0);  // data length (to be filled later)
          }
          stream.WriteString("8BIM"); // blend mode signature
          stream.WriteString(layer.Blend); // blend mode
          stream.WriteByte(layer.Opacity);  // opacity (255=opaque)
          stream.WriteByte(layer.Clipping); // clipping (0=base)
          stream.WriteByte(layer.Flags);    // flags
          stream.WriteByte(0); // reserved
          string name = layer.Name==null ? "Layer "+(i+1) : layer.Name;
          if(name.Length>255) name = name.Substring(0, 255);
          int extraLen = 8 + (name.Length+4)/4*4;
          stream.WriteBE4(extraLen); // size of the extra layer infomation
          stream.WriteBE4(0); // layer mask
          stream.WriteBE4(0); // layer blending size
          stream.WriteByte((byte)name.Length);
          stream.WriteString(name); // layer name
          if(((name.Length+1)&3) != 0)
          {
            for(int j=4-((name.Length+1)&3); j>0; j--) stream.WriteByte(0); // name padding
          }
        }
      }
      else
      {
        stream.WriteBE4(0); // misc info section
      }
    }
    catch { if(autoClose) stream.Close(); throw; }

    this.autoClose = autoClose;
    this.stream = stream;
    this.image = image;
    this.layer = 0;
    reading = false;
    state = image.Layers==null || image.Layers.Length==0 ? State.Layers : State.Header;
  }

  /// <param name="filename">A path to the file into which the PSD data will be written.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Write/*"/>
  public void Write(PSDImage image, string filename) { Write(image, File.Open(filename, FileMode.Create)); }
  /// <param name="stream">A stream into which PSD image data will be written. The stream must be seekable, with its
  /// entire range devoted to the PSD data for this image. The stream will be closed automatically.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Write/*"/>
  public void Write(PSDImage image, Stream stream) { Write(image, stream, true); }
  /// <param name="stream">A stream into which PSD image data will be written. The stream must be seekable, with its
  /// entire range devoted to the PSD data for this image.
  /// </param>
  /// <param name="autoClose">If true, <paramref name="stream"/> will be closed automatically.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Write/*"/>
  public void Write(PSDImage image, Stream stream, bool autoClose)
  { StartWriting(image, stream, autoClose);
    if(image.Layers!=null) foreach(PSDLayer layer in image.Layers) WriteLayer(layer.Surface);
    WriteFlattened(image.Flattened);
    FinishWriting();
  }

  /// <param name="filename">A path to the file into which the PSD data will be written.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Write_Surface/*"/>
  public void Write(Surface surface, string filename) { Write(surface, File.Open(filename, FileMode.Create)); }
  /// <param name="stream">A stream into which PSD image data will be written. The stream must be seekable, with its
  /// entire range devoted to the PSD data for this image. The stream will be closed automatically.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Write_Surface/*"/>
  public void Write(Surface surface, Stream stream) { Write(surface, stream, true); }
  /// <param name="stream">A stream into which PSD image data will be written. The stream must be seekable, with its
  /// entire range devoted to the PSD data for this image.
  /// </param>
  /// <param name="autoClose">If true, <paramref name="stream"/> will be closed automatically.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Write_Surface/*"/>
  public void Write(Surface surface, Stream stream, bool autoClose)
  { if(surface==null || stream==null) throw new ArgumentNullException();
    PSDImage image = new PSDImage();
    image.Width = surface.Width;
    image.Height = surface.Height;
    image.Channels = surface.Format.AlphaMask==0 ? 3 : 4;
    image.Flattened = surface;
    Write(image, stream, autoClose);
  }

  /// <summary>Writes the flattened image.</summary>
  /// <param name="surface">A <see cref="Surface"/> containing the flattened image to write.</param>
  /// <remarks>This method requires all layers to have been written. This can be done with <see cref="WriteLayer"/>.</remarks>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="surface"/> is null.</exception>
  /// <exception cref="InvalidOperationException">Thrown if
  /// <para>The codec is not in a writing mode.</para>
  /// -or-
  /// <para>Not all layers have been written yet.</para>
  /// </exception>
  public void WriteFlattened(Surface surface)
  { AssertWriting();
    if(surface==null) throw new ArgumentNullException("surface");
    if(state!=State.Layers) throw new InvalidOperationException("Not all the layers have been written yet!");
    if(surface.Depth<24)
      surface = surface.Clone(new PixelFormat(surface.Format.AlphaMask==0 ? 24 : 32, surface.Format.AlphaMask!=0));
    try { WriteImageData(surface); state=State.Flattened; }
    catch { Abort(); throw; }
  }

  /// <summary>Writes the next layer.</summary>
  /// <param name="surface">A <see cref="Surface"/> containing the image data for the layer, or null if the layer is
  /// empty (has a width and height of zero pixels).
  /// </param>
  /// <exception cref="ArgumentException">The surface size doesn't match the layer size.</exception>
  /// <exception cref="InvalidOperationException">Thrown if
  /// <para>The codec is not in a writing mode.</para>
  /// -or-
  /// <para>All layers have been written already.</para>
  /// </exception>
  public void WriteLayer(Surface surface)
  { AssertLayerWrite();
    try
    { PSDLayer layer = image.Layers[this.layer++];
      int sw = surface==null ? 0 : surface.Width, sh = surface==null ? 0 : surface.Height;
      if(sw!=layer.Width || sh!=layer.Height) throw new ArgumentException("Surface size doesn't match layer size!");
      if(surface==null)
      {
        for(int i=0; i<layer.Channels; i++) stream.WriteBE2(0);
        int endPos = (int)stream.Position;
        stream.Position = layer.savedPos;
        for(int i=0; i<layer.Channels; i++)
        {
          stream.WriteBE4(2);
          stream.Position += 2;
        }
        stream.Position = endPos;
      }
      else
      { int[] sizes = new int[layer.Channels];
        if(surface.Depth<24) surface = surface.Clone(new PixelFormat(surface.Format.AlphaMask==0 ? 24 : 32,
                                                                     surface.Format.AlphaMask!=0));
        for(int i=0; i<layer.Channels; i++) sizes[i] = WriteChannelData(layer.Surface, i);
        int endPos = (int)stream.Position;
        stream.Position = layer.savedPos;
        for(int i=0; i<layer.Channels; i++)
        {
          stream.WriteBE4(sizes[i]);
          stream.Position += 2;
        }
        stream.Position = endPos;
      }

      if(this.layer==image.Layers.Length)
      { int endPos=(int)stream.Position, dist=endPos-savedPos-4;
        stream.Position = savedPos;
        stream.WriteBE4(dist);   // miscellaneous info section size
        stream.WriteBE4(dist-4); // layer section size
        stream.Position = endPos;

        state = State.Layers;
      }
    }
    catch { Abort(); throw; }
  }

  /// <param name="filename">A path to the file to check.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/IsPSD/*"/>
  public static bool IsPSD(string filename)
  { return IsPSD(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read), true);
  }
  /// <param name="stream">A seekable stream positioned at the beginning of the data to check. The stream's position
  /// is unchanged (actually, changed and reset) by this function. The stream will not be closed automatically.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/IsPSD/*"/>
  public static bool IsPSD(Stream stream) { return IsPSD(stream, false); }
  /// <param name="stream">A seekable stream positioned at the beginning of the data to check. The stream's position
  /// is unchanged (actually, changed and reset) by this function.
  /// </param>
  /// <param name="autoClose">If true, the stream will be closed by this function.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/IsPSD/*"/>
  public static bool IsPSD(Stream stream, bool autoClose)
  { long position = autoClose ? 0 : stream.Position;
    if(stream.Length-stream.Position<6) return false;
    bool ret=true;
    if(stream.ReadString(4)!="8BPS" || stream.ReadBE2U()!=1) ret=false;
    if(autoClose) stream.Close();
    else stream.Position = position;
    return ret;
  }

  /// <param name="filename">The path to a PSD image file.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Read/*"/>
  public static PSDImage ReadPSD(string filename) { return new PSDCodec().Read(filename); }
  /// <param name="stream">A stream containing PSD image data. The stream does not need to be seekable.
  /// The stream will be closed automatically.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Read/*"/>
  public static PSDImage ReadPSD(Stream stream) { return new PSDCodec().Read(stream); }
  /// <param name="stream">A stream containing PSD image data. The stream does not need to be seekable.</param>
  /// <param name="autoClose">If true, <paramref name="stream"/> will be closed when the reading process finishes.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Read/*"/>
  public static PSDImage ReadPSD(Stream stream, bool autoClose) { return new PSDCodec().Read(stream, autoClose); }

  /// <param name="filename">The path to a PSD image file.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/ReadComposite/*"/>
  public static Surface ReadComposite(string filename)
  { PSDCodec codec = new PSDCodec();
    codec.StartReading(filename);
    return ReadComposite(codec);
  }
  /// <param name="stream">A stream containing PSD image data. The stream does not need to be seekable.
  /// The stream will be closed automatically.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/ReadComposite/*"/>
  public static Surface ReadComposite(Stream stream)
  { PSDCodec codec = new PSDCodec();
    codec.StartReading(stream);
    return ReadComposite(codec);
  }
  /// <param name="stream">A stream containing PSD image data. The stream does not need to be seekable.</param>
  /// <param name="autoClose">If true, <paramref name="stream"/> will be closed when the reading process finishes.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/ReadComposite/*"/>
  public static Surface ReadComposite(Stream stream, bool autoClose)
  { PSDCodec codec = new PSDCodec();
    codec.StartReading(stream, autoClose);
    return ReadComposite(codec);
  }

  /// <param name="filename">A path to the file into which the PSD data will be written.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Write/*"/>
  public static void WritePSD(PSDImage image, string filename) { new PSDCodec().Write(image, filename); }
  /// <param name="stream">A stream into which PSD image data will be written. The stream must be seekable, with its
  /// entire range devoted to the PSD data for this image. The stream will be closed automatically.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Write/*"/>
  public static void WritePSD(PSDImage image, Stream stream) { new PSDCodec().Write(image, stream); }
  /// <param name="stream">A stream into which PSD image data will be written. The stream must be seekable, with its
  /// entire range devoted to the PSD data for this image.
  /// </param>
  /// <param name="autoClose">If true, <paramref name="stream"/> will be closed automatically.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Write/*"/>
  public static void WritePSD(PSDImage image, Stream stream, bool autoClose)
  { new PSDCodec().Write(image, stream, autoClose);
  }

  /// <param name="filename">A path to the file into which the PSD data will be written.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Write_Surface/*"/>
  public static void WritePSD(Surface surface, string filename) { new PSDCodec().Write(surface, filename); }
  /// <param name="stream">A stream into which PSD image data will be written. The stream must be seekable, with its
  /// entire range devoted to the PSD data for this image. The stream will be closed automatically.
  /// </param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Write_Surface/*"/>
  public static void WritePSD(Surface surface, Stream stream) { new PSDCodec().Write(surface, stream); }
  /// <param name="stream">A stream into which PSD image data will be written. The stream must be seekable, with its
  /// entire range devoted to the PSD data for this image.
  /// </param>
  /// <param name="autoClose">If true, <paramref name="stream"/> will be closed automatically.</param>
  /// <include file="../documentation.xml" path="//Video/PSDCodec/Write_Surface/*"/>
  public static void WritePSD(Surface surface, Stream stream, bool autoClose)
  { new PSDCodec().Write(surface, stream, autoClose);
  }

  static Surface ReadComposite(PSDCodec codec)
  { codec.SkipLayers();
    Surface composite = codec.ReadFlattened();
    codec.FinishReading();
    return composite;
  }

  void Abort()
  { if(autoClose) stream.Close();

    image   = null;
    stream  = null;
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

  void AssertReading() { if(!Reading) throw new InvalidOperationException("A read operation is not in progress."); }
  void AssertWriting() { if(!Writing) throw new InvalidOperationException("A write operation is not in progress."); }

  Surface ReadImageData()
  { PSDChannel[] channels = new PSDChannel[image.Channels];
    for(int i=0,id=channels.Length==4 ? -1 : 0; id<3; i++,id++) channels[i] = (PSDChannel)id;
    return ReadImageData(image.Width, image.Height, channels, false);
  }

  unsafe Surface ReadImageData(int width, int height, PSDChannel[] chans, bool layer)
  { Surface surface = new Surface(width, height, chans.Length==3 ? 24 : 32,
                                  chans.Length==3 ? SurfaceFlag.None : SurfaceFlag.SourceAlpha);

    byte[] linebuf=null;
    int[]  lengths=null;
    bool   compressed=false;
    int    maxlen, yi=0, xinc=chans.Length, yinc = (int)surface.Pitch - width*xinc;

    surface.Lock();
    try
    { for(int chan=0; chan<chans.Length; chan++)
      { bool recognized = true;
        if(layer || chan==0)
        { int value = stream.ReadBE2();
          if(value!=0 && value!=1) throw new NotSupportedException("Unsupported compression type: "+value);
          compressed = value==1;
        }

        byte* dest = (byte*)surface.Data;
        switch(chans[chan])
        { case PSDChannel.Alpha: dest += MaskToOffset(surface.Format.AlphaMask); break;
          case PSDChannel.Red:   dest += MaskToOffset(surface.Format.RedMask); break;
          case PSDChannel.Green: dest += MaskToOffset(surface.Format.GreenMask); break;
          case PSDChannel.Blue:  dest += MaskToOffset(surface.Format.BlueMask); break;
          default: recognized=false; break;
        }

        if(compressed)
        { if(layer || chan==0)
          { if(lengths==null) lengths = new int[layer ? height : height*chans.Length];
            maxlen = 0;
            for(int y=0; y<lengths.Length; y++)
            { lengths[y] = stream.ReadBE2U();
              if(lengths[y]>maxlen) maxlen=lengths[y];
            }
            linebuf = new byte[maxlen];
          }
          if(!recognized)
          { int skip=0;
            for(int yend=yi+height; yi<yend; yi++) skip += lengths[yi];
            stream.Skip(skip);
            continue;
          }
          fixed(byte* lbptr=linebuf)
            for(int yend=yi+height; yi<yend; yi++)
            { byte* src = lbptr;
              int  f;
              byte b;
              stream.ReadOrThrow(linebuf, 0, lengths[yi]);
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
        {
          int length = width*height;
          if(!recognized) stream.Skip(length);
          else if(length<=65536)
          { 
            byte[] data = new byte[length];
            stream.ReadOrThrow(data, 0, data.Length);
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
          {
            byte[] data = new byte[width];
            fixed(byte* sdata=data)
            {
              for(int y=0; y<height; y++)
              {
                int xlen = width;
                stream.ReadOrThrow(data, 0, data.Length);
                byte* src=sdata;
                do { *dest=*src++; dest+=xinc; } while(--xlen != 0);
                dest += yinc;
              }
            }
          }
        }
      }
    }
    finally { surface.Unlock(); }
    return surface;
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
    { int chans=surface.Format.AlphaMask==0 ? 3 : 4, thresh = surface.Width*surface.Height*chans;
      int pos = (int)stream.Position, len = surface.Height*2*chans;

      stream.WriteBE2(1); // packbits compression
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
    {
      stream.WriteBE2(1); // packbits compression (not included in 'length')
      table = (int)stream.Position;
      stream.Position += length;
    }

    byte[] data = new byte[128];
    byte* src = (byte*)surface.Data;
    int xinc=surface.Depth/8, yinc=surface.Pitch-surface.Width*xinc;
    switch(channel+(surface.Format.AlphaMask==0 ? 1 : 0))
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
    for(int y=0; y<surface.Height; y++) stream.WriteBE2U(lengths[y]);
    stream.Position = pos;
    return length;
  }

  unsafe void WriteRawBits(Surface surface, int channel, bool layer)
  { 
    byte[] data = new byte[1024];
    if(layer || channel==0) stream.WriteBE2(0); // no compression
    int xinc=surface.Depth/8, yinc=surface.Pitch-surface.Width*xinc, blen=0;
    byte* src = (byte*)surface.Data;
    switch(channel+(surface.Format.AlphaMask==0 ? 1 : 0))
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

  static int MaskToOffset(uint mask) // TODO: make this big-endian safe?
  { int i=0;
    while(mask!=255)
    { if(mask==0) throw new InvalidOperationException("Invalid color mask.");
      mask>>=8;
      i++;
    }
    return i;
  }

  static void ValidateChannels(int channels)
  { if(channels<3 || channels>4) throw new NotSupportedException("Unsupported number of channels: "+channels);
  }
}
#endregion

} // namespace GameLib.Video