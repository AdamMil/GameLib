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
using System.Drawing.Imaging;
using GameLib;
using GameLib.Events;
using GameLib.Input;
using GameLib.Video;
using GameLib.Interop.OpenGL;
using GameLib.Mathematics.ThreeD;

namespace OpenGLTest
{

class App
{ 
  #if DEBUG
  static string dataPath = "../../../"; 
  #else
  static string dataPath = "./data/"; 
  #endif

  const int NUM_PARTICLES=500;
  const float ZOOM=-15f;

  struct Particle
  { public float  Life, Decay, R, G, B, Angle, AngleVel;
    public Point  Pos;
    public Vector Vel;
  }

  static void Main()
  { Initialize();
    WM.WindowTitle = "OpenGL Example";

    Events.Initialize();
    Input.Initialize();

    R=Rand.Next(256)/256f; G=Rand.Next(256)/256f; B=Rand.Next(256)/256f;
    Ri=Rand.Next(64)/256f+0.01f; Gi=Rand.Next(64)/256f+0.01f; Bi=Rand.Next(64)/256f+0.01f;

    float lastTime = (float)Timing.Seconds;
    while(true)
    { Event e;
      while((e=Events.NextEvent(0)) != null)
      { Input.ProcessEvent(e);
        if(e is KeyboardEvent)
        { KeyboardEvent ke = (KeyboardEvent)e;
          if(ke.Down && ke.Key==GameLib.Input.Key.Escape) goto done;
        }
        else if(e is QuitEvent) goto done;
      }
      float time = (float)Timing.Seconds;
      UpdateParticles(time - lastTime);
      lastTime = time;
      Draw();
    }

    done:
    Video.Deinitialize();
  }
  
  static void Initialize()
  { Video.Initialize();
    Video.SetGLMode(640, 480, 32); //SurfaceFlag.Fullscreen); 

    GL.glShadeModel(GL.GL_SMOOTH);
    GL.glClearColor(0.0f, 0.0f, 0.0f, 0.5f);
    GL.glClearDepth(1.0f);
    GL.glEnable(GL.GL_BLEND);
    GL.glEnable(GL.GL_TEXTURE_2D);
    GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE);
    GL.glDepthFunc(GL.GL_LEQUAL);
    GL.glHint(GL.GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_NICEST);

    GL.glGenTextures(textures);
    GL.glBindTexture(GL.GL_TEXTURE_2D, textures[0]);
    OpenGL.TexImage2D(new Surface(dataPath+"particle.png"));
    GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_LINEAR);
    GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR);

    GL.glMatrixMode(GL.GL_PROJECTION);
    GL.glLoadIdentity();
    GLU.gluPerspective(45.0f, (double)Video.Width/Video.Height, 0.1f, 100.0f);
    GL.glMatrixMode(GL.GL_MODELVIEW);
    GL.glLoadIdentity();
  }
  
  static void Draw()
  { GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);

    foreach(Particle p in Particles)
    { GL.glPushMatrix();
      GL.glTranslatef(p.Pos.X, p.Pos.Y, p.Pos.Z+ZOOM);
      GL.glRotatef(p.Angle, 0, 0, 1);
      GL.glBegin(GL.GL_QUADS);
      GL.glColor4f(p.R, p.G, p.B, p.Life);
      GL.glTexCoord2f(0, 1); GL.glVertex3f(-0.5f, -0.5f, 0);
      GL.glTexCoord2f(1, 1); GL.glVertex3f( 0.5f, -0.5f, 0);
      GL.glTexCoord2f(1, 0); GL.glVertex3f( 0.5f,  0.5f, 0);
      GL.glTexCoord2f(0, 0); GL.glVertex3f(-0.5f,  0.5f, 0);
      GL.glEnd();
      GL.glPopMatrix();
    }

    Video.Flip();
  }
  
  static void UpdateParticles(float timeDelta)
  { Vector gravity = Gravity*timeDelta;
    R+=Ri*timeDelta; G+=Gi*timeDelta; B+=Bi*timeDelta;
    if(R<0.3f || R>1f)
    { if(R<0.3f) { R=0.3f; Ri=Rand.Next(32)/256f+0.01f; }
      else if(R>1f) { R=1f; Ri=-Rand.Next(32)/256f-0.01f; }
    }
    if(G<0.3f || G>1f)
    { if(G<0.3f) { G=0.3f; Gi=Rand.Next(32)/256f+0.01f; }
      else if(G>1f) { G=1f; Gi=-Rand.Next(32)/256f-0.01f; }
    }
    if(B<0.3f || B>1f)
    { if(B<0.3f) { B=0.3f; Bi=Rand.Next(32)/256f+0.01f; }
      else if(B>1f) { B=1f; Bi=-Rand.Next(32)/256f-0.01f; }
    }

    float angle;
    float length;
    { Vector dir;
      double[] model=new double[16], proj=new double[16];
      int[] view=new int[4];
      double x, y, z;
      GL.glGetDoublev(GL.GL_MODELVIEW_MATRIX, model);
      GL.glGetDoublev(GL.GL_PROJECTION_MATRIX, proj);
      GL.glGetIntegerv(GL.GL_VIEWPORT, view);
      GLU.gluUnProject(Mouse.X, view[3]-Mouse.Y, 1, model, proj, view, out x, out y, out z);
      dir = new Vector((float)x/2, (float)y/2, 0);
      length = dir.Length;
      angle  = (float)Math.Acos(dir.DotProduct(new Vector(1, 0, 0))/length);
      if(y>0)
      { if(x>0) angle = (float)(Math.PI*2-angle);
        else angle += (float)((Math.PI-angle)*2);
      }
    }

    for(int i=0; i<Particles.Length; i++)
    { Particles[i].Life -= Particles[i].Decay*timeDelta;
      if(Particles[i].Life>0)
      { Particles[i].Pos += Particles[i].Vel*timeDelta;
        Particles[i].Vel += gravity;
        Particles[i].Angle += Particles[i].AngleVel*timeDelta;
        if(Particles[i].Angle<0) Particles[i].Angle += 360;
        else if(Particles[i].Angle>360) Particles[i].Angle -= 360;
      }
      else
      { Particles[i].Vel = new Vector(Rand.Next(100)/250f, (Rand.Next(100)-50)/2500f, 0).RotatedZ(angle) * length;
        Particles[i].Vel.Y = -Particles[i].Vel.Y;
        Particles[i].Decay = Rand.Next(1000)/25f+0.0025f;
        Particles[i].Life  = 1f;
        Particles[i].Pos   = new Point(0, 0, 0);
        Particles[i].R=R; Particles[i].G=G; Particles[i].B=B;
        Particles[i].Angle = 0;
        Particles[i].AngleVel = (Rand.Next(360)-180)*Particles[i].Vel.LengthSqr/10;
      }
    }
  }

  static Particle[] Particles = new Particle[NUM_PARTICLES];
  static Vector Gravity = new Vector(0, -1, 0);
  static Random Rand = new Random();
  static float R, G, B, Ri, Gi, Bi;

  static uint[] textures = new uint[1];
}

} // namespace OpenGLTest