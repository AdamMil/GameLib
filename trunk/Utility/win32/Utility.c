#include "../Utility.h"
#include <windows.h>

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
    return (double)(cur.QuadPart-hrStart.QuadPart)/hrFreq.QuadPart;
  }
  else return (GetTickCount()-msStart)/1000.0;
}
