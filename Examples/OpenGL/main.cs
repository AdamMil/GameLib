/*
This is an example application for the GameLib multimedia/gaming library.
http:
Copyright (C) 2004-2010 Adam Milazzo

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
using System.Drawing.Imaging;
using GameLib;
using GameLib.Events;
using GameLib.Input;
using GameLib.Video;
using GameLib.Interop.OpenGL;
using AdamMil.Mathematics.Geometry;
using AdamMil.Mathematics.Geometry.TwoD;

namespace OpenGLTest
{

  class App
  {
#if DEBUG
    static string dataPath = "../../../";
#else
  static string dataPath = "./data/"; 
#endif

    const int NUM_PARTICLES=150;
    const double ZOOM=-15;

    struct Particle
    {
      public float Life, Decay, R, G, B, Angle, AngleVel;
      public Point Pos;
      public Vector Vel;
    }

    static void Main()
    {
      InitializeGL();
      WM.WindowTitle = "OpenGL Example";

      Events.Initialize();
      Input.Initialize();
      Mouse.Point = new System.Drawing.Point(Video.Width*6/10, Video.Height*4/10);

      for(int i=0; i<3; i++)
      {
        Color[i] = Rand.Next(256)/256f;
        Cinc[i] = Rand.Next(64)/256f+0.01f;
      }

      float lastTime = (float)Timing.Seconds;
      while(true)
      {
        Event e;
        while((e=Events.NextEvent(0)) != null)
        {
          Input.ProcessEvent(e);
          if(e.Type==EventType.Keyboard)
          {
            KeyboardEvent ke = (KeyboardEvent)e;
            if(ke.Down && ke.Key==GameLib.Input.Key.Escape) goto done;
          }
          else if(e.Type==EventType.Quit) goto done;
        }
        float time = (float)Timing.Seconds;
        UpdateParticles(time - lastTime);
        lastTime = time;
        Draw();
      }

      done:
      texture.Unload();
      Video.Deinitialize();
    }

    static void InitializeGL()
    {
      Video.Initialize();
      Video.SetGLMode(640, 480, 32);

      GL.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
      GL.glDisable(GL.GL_DITHER);
      GL.glEnable(GL.GL_BLEND);
      GL.glEnable(GL.GL_TEXTURE_2D);
      GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE);
      // perspective correction is not necessary because all objects are
      // parallel to the screen
      GL.glHint(GL.GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_FASTEST);
      GL.glShadeModel(GL.GL_FLAT); // smooth shading is also unnecessary

      texture.Load(dataPath+"particle.png");
      texture.Bind();
      GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_LINEAR);
      GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR);

      GL.glMatrixMode(GL.GL_PROJECTION);
      GL.glLoadIdentity();
      GLU.gluPerspective(45.0f, (double)Video.Width/Video.Height, 0.1f, 100.0f);
      GL.glMatrixMode(GL.GL_MODELVIEW);
      GL.glLoadIdentity();
    }

    static void Draw()
    {
      GL.glClear(GL.GL_COLOR_BUFFER_BIT);

      foreach(Particle p in Particles)
      {
        GL.glPushMatrix();
        GL.glTranslated(p.Pos.X, p.Pos.Y, ZOOM);
        GL.glRotatef(p.Angle, 0, 0, 1);
        GL.glBegin(GL.GL_QUADS);
        GL.glColor4f(p.R, p.G, p.B, p.Life);
        GL.glTexCoord2f(0, 1); GL.glVertex2f(-0.5f, -0.5f);
        GL.glTexCoord2f(1, 1); GL.glVertex2f(0.5f, -0.5f);
        GL.glTexCoord2f(1, 0); GL.glVertex2f(0.5f, 0.5f);
        GL.glTexCoord2f(0, 0); GL.glVertex2f(-0.5f, 0.5f);
        GL.glEnd();
        GL.glPopMatrix();
      }

      Video.Flip();
    }

    static void UpdateParticles(float timeDelta)
    {
      Vector gravity = Gravity*timeDelta; // gravity for this update

      for(int i=0; i<3; i++) // smooth, random color transitions
      {
        Color[i] += Cinc[i]*timeDelta;
        if(Color[i]<0.3f || Color[i]>1f)
        {
          if(Color[i]<0.3f) { Color[i]=0.3f; Cinc[i]=Rand.Next(1, 32)/256f; }
          else if(Color[i]>1) { Color[i]=1; Cinc[i]=-Rand.Next(1, 32)/256f; }
        }
      }

      double angle;  // angle between the spigot and the mouse (in radians)
      double length; // distance to the mouse from the spigot
      {
        double[] model=new double[16], proj=new double[16];
        int[] view=new int[4];
        double x, y, z;
        GL.glGetDoublev(GL.GL_MODELVIEW_MATRIX, model);
        GL.glGetDoublev(GL.GL_PROJECTION_MATRIX, proj);
        GL.glGetIntegerv(GL.GL_VIEWPORT, view);
        GLU.gluUnProject(Mouse.X, view[3]-Mouse.Y, 1, model, proj, view, out x, out y, out z);
        Vector dir = new Vector(x/2, y/2);
        length = dir.Length;
        angle  = dir.Angle;
      }

      for(int i=0; i<Particles.Length; i++)
      {
        Particles[i].Life -= Particles[i].Decay*timeDelta;
        if(Particles[i].Life>0)
        {
          Particles[i].Pos += Particles[i].Vel*timeDelta;
          Particles[i].Vel += gravity;
          Particles[i].Angle += Particles[i].AngleVel*timeDelta;
          if(Particles[i].Angle<0) Particles[i].Angle += 360;
          else if(Particles[i].Angle>360) Particles[i].Angle -= 360;
        }
        else
        { // make a velocity pointing at 0 degrees, then rotate by 'angle'
          Vector vel = new Vector((Rand.Next(200)+800)/2500f,
                                  (Rand.Next(100)-50)/1500f)
                        .Rotated(angle)*length; // and multiply by 'length'
          Particles[i].Vel   = vel;
          Particles[i].Decay = Math.Max((float)Rand.NextDouble(), 0.1f);
          Particles[i].Life  = 1f;
          Particles[i].Pos   = new Point(0, 0); // position of the spigot
          Particles[i].Angle = 0;
          // rotate faster for higher velocities
          Particles[i].AngleVel = (float)((Rand.Next(360)-180)*vel.LengthSqr/10);
          Particles[i].R=Color[0]; Particles[i].G=Color[1]; Particles[i].B=Color[2];
        }
      }
    }

    static Particle[] Particles = new Particle[NUM_PARTICLES];
    static Vector Gravity = new Vector(0, -1);
    static Random Rand = new Random();
    static GLTexture2D texture = new GLTexture2D();
    static float[] Color=new float[3], Cinc=new float[3];
  }

} // namespace OpenGLTest