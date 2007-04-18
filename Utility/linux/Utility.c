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

#include "../Utility.h"
#include <sys/time.h>
#include <termios.h>
#include <stdio.h>
#include <string.h>

static struct timeval hrStart;
static Uint32 initCount;

static void timediff(struct timeval *pt)
{ gettimeofday(pt, NULL);
  pt->tv_sec  -= hrStart.tv_sec;
  pt->tv_usec -= hrStart.tv_usec;
  if(pt->tv_usec < hrStart.tv_usec)
  { pt->tv_sec--;
    pt->tv_usec += 1000000;
  }
}

int GLU_Init()
{ if(initCount++==0) gettimeofday(&hrStart, NULL);
  return 0;
}

void GLU_Quit() { if(initCount>0) initCount--; }
char* GLU_GetError() { return NULL; }

Uint32 GLU_GetMilliseconds()
{ struct timeval now;
  timediff(&now);
  return (Uint32)(now.tv_sec*1000 + now.tv_usec/1000);
}

Uint64 GLU_GetTimerFrequency() { return 1000; }

Uint64 GLU_GetTimerCounter()
{ struct timeval now;
  timediff(&now);
  return (Uint64)(now.tv_sec*1000 + now.tv_usec/1000);
}

double GLU_GetSeconds()
{ struct timeval now;
  timediff(&now);
  return now.tv_sec + now.tv_usec/1000000.0;
}

void GLU_ResetTimer() { gettimeofday(&hrStart, NULL); }

void GLU_MemCopy(void *src, void *dest, int length) { memcpy(dest, src, length); }
void GLU_MemFill(void *dest, Uint8 value, int length) { memset(dest, value, length); }
void GLU_MemMove(void *src, void *dest, int length) { memmove(dest, src, length); }