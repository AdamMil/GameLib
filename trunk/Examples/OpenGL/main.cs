/*
This is an example application for the GameLib multimedia/gaming library.
http:
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
using System.Drawing;
using System.Drawing.Imaging;
using GameLib;
using GameLib.Events;
using GameLib.Video;
using GameLib.Interop.OpenGL;

namespace OpenGLTest
{

class App
{ 
  #if DEBUG
  static string dataPath = "../../../"; 
  #else
  static string dataPath = "./data/"; 
  #endif

  static void Main()
  { Initialize();
    WM.WindowTitle = "OpenGL Example";

    Events.Initialize();

    double timeBase = Timing.Seconds, lastTime = timeBase;
    while(true)
    { Event e;
      while((e=Events.NextEvent(0)) != null)
      { if(e is KeyboardEvent)
        { KeyboardEvent ke = (KeyboardEvent)e;
          if(ke.Down && ke.Key==GameLib.Input.Key.Escape) goto done;
        }
        else if(e is QuitEvent) goto done;
      }
      Draw();

      double time = Timing.Seconds, timeDelta = time - lastTime;
      lastTime = time;
      xrot += 60*timeDelta; // rotate 60, 40, and 80 degrees per second,
      yrot += 40*timeDelta; // respectively, regardless of the framerate
      zrot += 80*timeDelta;
    }

    done:
    Video.Deinitialize();
  }
  
  static void Initialize()
  { Video.Initialize();
    Video.SetGLMode(640, 480, 16); 

    GL.glShadeModel(GL.GL_SMOOTH);                
    GL.glClearColor(0.0f, 0.0f, 0.0f, 0.5f);      
    GL.glClearDepth(1.0f);                        
    GL.glEnable(GL.GL_DEPTH_TEST);             
    GL.glDepthFunc(GL.GL_LEQUAL);                
    GL.glHint(GL.GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_NICEST);  
    GL.glEnable(GL.GL_TEXTURE_2D);

    Surface smiley = new Surface(dataPath+"smiley.png");
    Primitives.Box(smiley, smiley.Bounds, Color.FromArgb(0, 96, 48));
    Bitmap image = smiley.ToBitmap();
    BitmapData data = image.LockBits(new Rectangle(new Point(0,0), image.Size), ImageLockMode.ReadOnly,
                                     System.Drawing.Imaging.PixelFormat.Format24bppRgb);
    GL.glGenTextures(textures);
    GL.glBindTexture(GL.GL_TEXTURE_2D, textures[0]);
    GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGB8, image.Width, image.Height,
                    0, GL.GL_BGR_EXT, GL.GL_UNSIGNED_BYTE, data.Scan0);
    GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_LINEAR);
    GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR);

    image.UnlockBits(data);

    GL.glMatrixMode(GL.GL_PROJECTION);
    GL.glLoadIdentity();
    GLU.gluPerspective(45.0f, (double)Video.Width/Video.Height, 0.1f, 100.0f);
  }
  
  static void Draw()
  { GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
    GL.glMatrixMode(GL.GL_MODELVIEW);

    GL.glLoadIdentity();
    GL.glTranslatef(0.0f, 0.0f, -5.0f);
    GL.glRotated(xrot, 1.0f, 0.0f, 0.0f);
    GL.glRotated(yrot, 0.0f, 1.0f, 0.0f);
    GL.glRotated(zrot, 0.0f, 0.0f, 1.0f);

    GL.glBegin(GL.GL_QUADS);

    // this is an ugly, verbose way of doing it, but it's just test code
    GL.glTexCoord2f(1.0f,1.0f);
    GL.glVertex3f(1.0f,1.0f,1.0f);
    GL.glTexCoord2f(0.0f,1.0f);
    GL.glVertex3f(-1.0f,1.0f,1.0f);
    GL.glTexCoord2f(0.0f,0.0f);
    GL.glVertex3f(-1.0f,-1.0f,1.0f);
    GL.glTexCoord2f(1.0f,0.0f);
    GL.glVertex3f(1.0f,-1.0f,1.0f);

    GL.glTexCoord2f(1.0f,1.0f);
    GL.glVertex3f(-1.0f,1.0f,-1.0f);
    GL.glTexCoord2f(0.0f,1.0f); 
    GL.glVertex3f(1.0f,1.0f,-1.0f);
    GL.glTexCoord2f(0.0f,0.0f);  
    GL.glVertex3f(1.0f,-1.0f,-1.0f);
    GL.glTexCoord2f(1.0f,0.0f);
    GL.glVertex3f(-1.0f,-1.0f,-1.0f);

    GL.glTexCoord2f(1.0f,1.0f);
    GL.glVertex3f(1.0f,1.0f,-1.0f);    
    GL.glTexCoord2f(0.0f,1.0f);      
    GL.glVertex3f(-1.0f,1.0f,-1.0f);  
    GL.glTexCoord2f(0.0f,0.0f);      
    GL.glVertex3f(-1.0f,1.0f,1.0f);    
    GL.glTexCoord2f(1.0f,0.0f);      
    GL.glVertex3f(1.0f,1.0f,1.0f);    

    GL.glTexCoord2f(1.0f,1.0f);      
    GL.glVertex3f(1.0f,-1.0f,1.0f);    
    GL.glTexCoord2f(0.0f,1.0f);      
    GL.glVertex3f(-1.0f,-1.0f,1.0f);  
    GL.glTexCoord2f(0.0f,0.0f);      
    GL.glVertex3f(-1.0f,-1.0f,-1.0f);  
    GL.glTexCoord2f(1.0f,0.0f);      
    GL.glVertex3f(1.0f,-1.0f,-1.0f);  

    GL.glTexCoord2f(1.0f,1.0f);      
    GL.glVertex3f(1.0f,1.0f,-1.0f);    
    GL.glTexCoord2f(0.0f,1.0f);      
    GL.glVertex3f(1.0f,1.0f,1.0f);    
    GL.glTexCoord2f(0.0f,0.0f);      
    GL.glVertex3f(1.0f,-1.0f,1.0f);    
    GL.glTexCoord2f(1.0f,0.0f);      
    GL.glVertex3f(1.0f,-1.0f,-1.0f);  
    
    GL.glTexCoord2f(1.0f,1.0f);      
    GL.glVertex3f(-1.0f,1.0f,1.0f);    
    GL.glTexCoord2f(0.0f,1.0f);      
    GL.glVertex3f(-1.0f,1.0f,-1.0f);  
    GL.glTexCoord2f(0.0f,0.0f);      
    GL.glVertex3f(-1.0f,-1.0f,-1.0f);  
    GL.glTexCoord2f(1.0f,0.0f);      
    GL.glVertex3f(-1.0f,-1.0f,1.0f);  
    GL.glEnd();

    Video.Flip();
  }

  static uint[] textures = new uint[1];
  static double xrot, yrot, zrot;
}

} // namespace OpenGLTest