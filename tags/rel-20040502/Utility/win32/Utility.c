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
#include <windows.h>
#include <wchar.h>

static LARGE_INTEGER hrFreq, hrStart;
static DWORD  msStart;
static Uint8  hiRes;
static Uint32 initCount;

int GLU_Init()
{ if(initCount++==0)
  { if(QueryPerformanceFrequency(&hrFreq))
    { QueryPerformanceCounter(&hrStart);
      hiRes=1;
    }
    else msStart = GetTickCount();
  }
  return 0;
}

void GLU_Quit()
{ if(initCount>0) initCount--;
}

char* GLU_GetError()
{ return NULL;
}

Uint32 GLU_GetMilliseconds()
{ if(hiRes)
  { LARGE_INTEGER cur;
    QueryPerformanceCounter(&cur);
    return (Uint32)((cur.QuadPart-hrStart.QuadPart)*1000/hrFreq.QuadPart);
  }
  else return GetTickCount()-msStart;
}

Uint64 GLU_GetTimerFrequency()
{ return hiRes ? hrFreq.QuadPart : 1000;
}

Uint64 GLU_GetTimerCounter()
{ if(hiRes)
  { LARGE_INTEGER cur;
    QueryPerformanceCounter(&cur);
    return cur.QuadPart-hrStart.QuadPart;
  }
  else return GetTickCount()-msStart;
}

double GLU_GetSeconds()
{ if(hiRes)
  { LARGE_INTEGER cur;
    QueryPerformanceCounter(&cur);
    return (double)(cur.QuadPart-hrStart.QuadPart)/(double)hrFreq.QuadPart;
  }
  else return (GetTickCount()-msStart)/1000.0;
}

void GLU_ResetTimer()
{ if(hiRes) QueryPerformanceCounter(&hrStart);
  else msStart = GetTickCount();
}

wchar_t GLU_Getch()
{ return _getwch();
}

wchar_t GLU_Getche()
{ return _getwche();
}
