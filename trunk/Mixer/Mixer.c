/*
GameLib is a library for developing games and other multimedia applications.
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

#include "Mixer.h"
#include <stdlib.h>
#include <string.h>
#include <malloc.h>

#define BITS(fmt) ((fmt)&0xFF)
#define BYTES(fmt) (BITS(fmt)>>3)
#define DIVISIBLE(n, d) ((n)/(d)*(d)==(n))
#define SIGNED(fmt) ((fmt)&0x8000)
#define SWAPEND(v) (((v)<<8)|((v)>>8))
#define MAXALLOCA 20000

#if SDL_BYTEORDER == SDL_LIL_ENDIAN
  #define OPPEND(fmt) ((fmt)&0x1000)
  #define MAKESE(fmt) (fmt&~0x1000)
#else
  #define OPPEND(fmt) (!((fmt)&0x1000))
  #define MAKESE(fmt) (fmt|0x1000)
#endif

static SDL_AudioSpec mixFormat;
static MixCallback   mixCallback;
static Sint32       *mixAcc;
static Sint32        mixAccSize;
static int           initCount, mixVolume=256;

static void GLM_callback(void *userdata, Uint8 *stream, int bytes)
{ int samples, frames;
  if(!mixCallback) return;

  if(mixVolume>0)
  { samples = bytes/BYTES(mixFormat.format);
    frames  = samples/mixFormat.channels;
    memset(mixAcc, 0, samples*sizeof(Sint32)); /* zero the accumulator */
    mixCallback(mixAcc, frames, userdata);  /* call the user callback to mix in the audio */
    if(mixVolume<256) GLM_VolumeScale(mixAcc, samples, mixVolume, mixVolume);
    GLM_ConvertAcc(stream, mixAcc, samples, mixFormat.format);
  }
  else if(SIGNED(mixFormat.format)) memset(stream, 0, bytes);
  else if(BITS(mixFormat.format)==8) memset(stream, 128, bytes);
  else
  { Uint16 *buf = (Uint16*)stream;
    int     i, len = bytes/2;
    for(i=0; i<len; i++) buf[i]=32768;
  }
}

static void StereoToMono(GLM_AudioCVT *cvt)
{ int i, sfmt=cvt->srcFormat;
  if(BITS(sfmt)==8)
  { i=cvt->len/2;
    if(SIGNED(sfmt)) /* 8bit signed */
    { Sint8 *src = cvt->buf, *dest=src;
      for(; i; src+=2,i--) *dest++ = (src[0]+src[1])/2;
    }
    else /* 8bit unsigned */
    { Uint8 *src = cvt->buf, *dest=src;
      for(; i; src+=2,i--) *dest++ = (src[0]+src[1])/2;
    }
  }
  else
  { i=cvt->len/4;
    if(OPPEND(sfmt)) /* opposite endianness */
    { Uint16 *src = (Uint16*)cvt->buf;
      Uint32 dv;
      if(SIGNED(sfmt)) /* 16bit signed OE */
      { Sint16 *dest = (Sint16*)cvt->buf;
        for(; i; src+=2,i--)
        { dv = (Uint32)(((Sint32)SWAPEND(src[0])+(Sint32)SWAPEND(src[1]))/2);
          *dest++ = SWAPEND(dv);
        }
      }
      else /* 16bit unsigned OE */
      { Uint16 *dest = (Uint16*)cvt->buf;
        for(; i; src+=2,i--)
        { dv = (SWAPEND(src[0])+SWAPEND(src[1]))/2;
          *dest++ = SWAPEND(dv);
        }
      }
    }
    else /* same endianness */
    { if(SIGNED(sfmt)) /* 16bit signed SE */
      { Sint16 *src = (Sint16*)cvt->buf, *dest = src;
        for(; i; src+=2,i--) *dest++ = (src[0]+src[1])/2;
      }
      else /* 16bit unsigned SE */
      { Uint16 *src = (Uint16*)cvt->buf, *dest = src;
        for(; i; src+=2,i--) *dest++ = (src[0]+src[1])/2;
      }
    }
  }
  cvt->len/=2; cvt->srcChans=1;
}

static void MonoToStereo(GLM_AudioCVT *cvt)
{ int i;
  if(BITS(cvt->srcFormat)==8) /* 8bit */
  { Uint8 *src = cvt->buf+cvt->len-1, *dest = cvt->buf+cvt->len*2-2;
    for(i=cvt->len; i; dest-=2,i--) dest[0]=dest[1]=*src--;
  }
  else /* 16bit */
  { Uint16 *src = (Uint16*)(cvt->buf+cvt->len)-1, *dest = (Uint16*)(cvt->buf+cvt->len*2)-2;
    for(i=cvt->len/2; i; dest-=2,i--) dest[0]=dest[1]=*src--;
  }
  cvt->len*=2; cvt->srcChans=2;
}

static void EightToSixteen(GLM_AudioCVT *cvt)
{ int i = cvt->len, sfmt = cvt->srcFormat, dfmt = cvt->destFormat;
  if(SIGNED(sfmt)) /* 8bit signed to 16bit */
  { if(SIGNED(dfmt)) /* 8bit signed to 16bit signed */
    { Sint8  *src  = (Sint8*)(cvt->buf+cvt->len-1);
      Sint16 *dest = (Sint16*)(cvt->buf+cvt->len*2-2);
      Sint8   dv;
      if(OPPEND(dfmt)) /* 8bit signed to 16bit OE */
        for(; i; i--)
        { dv = (*src--)<<8;
          *dest-- = (Sint16)SWAPEND(dv);
        }
      else for(; i; i--) *dest-- = *src--<<8; /* 8bit signed to 16bit SE */
    }
    else /* 8bit signed to 16bit unsigned */
    { Uint8  *src  = cvt->buf+cvt->len-1;
      Uint16 *dest = (Uint16*)(cvt->buf+cvt->len*2-2), dv;
      if(OPPEND(dfmt)) /* 8bit signed to 16bit unsigned OE */
        for(; i; i--)
        { dv=(*src-- + 128)<<8;
          *dest-- = SWAPEND(dv);
        }
      else for(; i; i--) *dest-- = (*src-- + 128)<<8; /* 8bit signed to 16bit unsigned SE */
    }
  }
  else /* 8bit unsigned to 16bit */
  { Uint8 *src = cvt->buf+cvt->len-1;
    if(SIGNED(dfmt)) /* 8bit unsigned to 16bit signed */
    { Sint16 *dest = (Sint16*)(cvt->buf+cvt->len*2-2), dv;
      if(OPPEND(dfmt)) /* 8bit unsigned to 16bit signed OE */
        for(; i; i--)
        { dv = (*src-- - 128)<<8;
          *dest-- = SWAPEND(dv);
        }
      else for(; i; i--) *dest-- = (*src-- - 128)<<8; /* 8bit unsigned to 16bit signed SE */
    }
    else /* 8bit unsigned to 16bit unsigned */
    { Uint16 *dest = (Uint16*)(cvt->buf+cvt->len*2-2);
      Uint8   dv;
      if(OPPEND(dfmt)) /* 8bit unsigned to 16bit unsigned OE */
        for(; i; i--)
        { dv = *src--<<8;
          *dest-- = SWAPEND(dv);
        }
      else for(; i; i--) *dest-- = *src--<<8; /* 8bit unsigned to 16bit unsigned SE */
    }
  }
  cvt->len*=2;
  cvt->srcFormat = (cvt->srcFormat&~0x80FF)|(cvt->destFormat&0x80FF);
}

static void SixteenToEight(GLM_AudioCVT *cvt)
{ int i = cvt->len/2;
  if(SIGNED(cvt->srcFormat)) /* 16bit signed to 8bit */
  { if(SIGNED(cvt->destFormat)) /* 16bit signed to 8bit signed */
    { Sint16 *src  = (Sint16*)cvt->buf;
      Sint8  *dest = (Sint8*)cvt->buf;
      if(OPPEND(cvt->srcFormat)) /* 16bit signed OE to 8bit signed */
        for(; i; src++,i--) *dest++ = (Sint16)SWAPEND((Uint16)*src)>>8;
      else for(; i; i--) *dest++ = *src++>>8;
    }
    else /* 16bit signed to 8bit unsigned */
    { Uint16 *src  = (Uint16*)cvt->buf;
      Uint8  *dest = cvt->buf;
      if(OPPEND(cvt->srcFormat))
        for(; i; src++,i--) *dest++ = (Uint8)((SWAPEND(*src)+32768)>>8); /* 16bit signed OE to 8bit unsigned */
      else for(; i; i--) *dest++ = (Uint8)((*src++ + 32768)>>8); /* 16bit signed SE to 8bit unsigned */
    }
  }
  else /* 16bit unsigned to 8bit */
  { Uint16 *src = (Uint16*)cvt->buf;
    if(SIGNED(cvt->destFormat)) /* 16bit unsigned to 8bit signed */
    { Sint8 *dest = (Sint8*)cvt->buf;
      if(OPPEND(cvt->srcFormat)) /* 16bit unsigned OE to 8bit signed */
        for(; i; src++,i--) *dest++ = (Sint8)((SWAPEND(*src)-32768)>>8);
      else for(; i; i--) *dest++ = (Sint8)((*src++ - 32768)>>8); /* 16bit unsigned SE to 8bit signed */
    }
    else /* 16bit unsigned to 8bit unsigned */
    { Uint8 *dest = cvt->buf;
      if(OPPEND(cvt->srcFormat)) for(; i; src++,i--) *dest++ = SWAPEND(*src)>>8; /* 16bit unsigned OE to 8bit unsigned */
      else for(; i; i--) *dest++ = *src++>>8; /* 16bit unsigned SE to 8bit unsigned */
    }
  }
  cvt->len/=2;
  cvt->srcFormat = (cvt->srcFormat&~0x80FF)|(cvt->destFormat&0x80FF);
}

static void ConvertRate(GLM_AudioCVT *cvt, int destLen)
{ int i, srate=cvt->srcRate, drate=cvt->destRate;
  if(cvt->len<=BYTES(cvt->srcFormat)) return; /* no conversion if <=1 sample */

  if(srate/drate==2) /* halving the rate */
  { 
    #define HALVE for(; i; src+=2,i--) *dest++ = (src[0]+src[1])/2;
    if(BITS(cvt->srcFormat)==16) /* 16bit */
    { i = destLen/2;
      if(SIGNED(cvt->srcFormat)) /* 16bit signed */
      { Sint16 *src = (Sint16*)cvt->buf, *dest = src;
        HALVE
      }
      else /* 16bit unsigned */
      { Uint16 *src = (Uint16*)cvt->buf, *dest = src;
        HALVE
      }
    }
    else /* 8bit */
    { i = destLen;
      if(SIGNED(cvt->srcFormat)) /* 8bit signed */
      { Sint8 *src = (Sint8*)cvt->buf, *dest = src;
        HALVE
      }
      else /* 8bit unsigned */
      { Uint8 *src = cvt->buf, *dest = src;
        HALVE
      }
    }
    #undef HALVE
    cvt->len/=2;
  }
  else /* any rate, length must be >1 sample. */
  { 
#define SE(s) SWAPEND((Uint16)s)
#define CVTRATE_SE                            \
  { s0=dest[0]=src[0], s1=src[1], diff=s1-s0; \
    for(; i<dlen; i++)                        \
    { sic += sinc;                            \
      if(sic>sid)                             \
      { do si++, sic-=sid; while(sic>=sid);   \
        s0=src[si-1], s1=(si>=slen?s0:src[si]), diff=s1-s0; \
      }                                       \
      dest[i] = s0+diff*sic/sid;              \
    }                                         \
  }
#define CVTRATE_OE                          \
  { dest[0]=src[0], s0=SE(src[0]), s1=SE(src[1]), diff=s1-s0; \
    for(; i<dlen; i++)                      \
    { sic += sinc;                          \
      if(sic>sid)                           \
      { do si++, sic-=sid; while(sic>=sid); \
        s0=SE(src[si-1]), s1=(si>=slen?s0:SE(src[si])), diff=s1-s0; \
      }                                     \
      t = s0+diff*sic/sid;                  \
      dest[i] = SE(t);                      \
    }                                       \
  }
#define CVTRATE2_SE                            \
  { s0=dest[0]=src[0], s1=dest[1]=src[1], s2=src[2], s3=src[3], diff=s2-s0, diff2=s3-s1; \
    for(; i<dlen; i+=2)                        \
    { sic += sinc;                             \
      if(sic>sid)                              \
      { do si+=2, sic-=sid; while(sic>=sid);   \
        s0=src[si-2], s2=(si>=slen?s0:src[si]),   diff=s2-s0; \
        s1=src[si-1], s3=(si>=slen?s1:src[si+1]), diff=s3-s1; \
      }                                        \
      dest[i] = s0+diff*sic/sid;               \
      dest[i+1] = s1+diff2*sic/sid;            \
    }                                          \
  }
#define CVTRATE2_OE                            \
  { dest[0]=src[0], s0=SE(src[0]), dest[1]=src[1], s1=SE(src[1]), s2=SE(src[2]), s3=SE(src[3]), diff=s2-s0, diff2=s3-s1; \
    for(; i<dlen; i+=2)                        \
    { sic += sinc;                             \
      if(sic>sid)                              \
      { do si+=2, sic-=sid; while(sic>=sid);   \
        s0=SE(src[si-2]), s2=(si>=slen?s0:SE(src[si])),   diff=s2-s0; \
        s1=SE(src[si-1]), s3=(si>=slen?s1:SE(src[si+1])), diff=s3-s1; \
      }                                        \
      t = s1+diff2*sic/sid;                    \
      dest[i] = SE(t);                         \
      t = s0+diff*sic/sid;                     \
      dest[i+1] = SE(t);                       \
    }                                          \
  }

    int sic=0, sid, sinc, si, slen=cvt->len, dlen=destLen, s0, s1, diff, s2, s3, diff2, t;
    void *dbuf = srate>drate ? cvt->buf : destLen>MAXALLOCA ? malloc(destLen) : alloca(destLen);
    cvt->len = dlen;
    if(BITS(cvt->srcFormat)==16) /* 16bit */
    { slen/=2, dlen/=2;
      sinc=slen, sid=dlen; while(sid>32768) { sid>>=1, sinc>>=1; } /* prevent integer overflow */
      if(cvt->srcChans==2)
      { i=si=2;
        if(SIGNED(cvt->srcFormat))
        { Sint16 *src = (Sint16*)cvt->buf, *dest=(Sint16*)dbuf;
          if(OPPEND(cvt->srcFormat)) CVTRATE2_OE /* 16bit signed OE */
          else CVTRATE2_SE /* 16 bit signed SE */
        }
        else
        { Uint16 *src = (Uint16*)cvt->buf, *dest=(Uint16*)dbuf;
          if(OPPEND(cvt->srcFormat)) CVTRATE2_OE /* 16bit unsigned OE */
          else CVTRATE2_SE
        }
      }
      else
      { i=si=1;
        if(SIGNED(cvt->srcFormat))
        { Sint16 *src = (Sint16*)cvt->buf, *dest=(Sint16*)dbuf;
          if(OPPEND(cvt->srcFormat)) CVTRATE_OE /* 16bit signed OE */
          else CVTRATE_SE /* 16 bit signed SE */
        }
        else
        { Uint16 *src = (Uint16*)cvt->buf, *dest=(Uint16*)dbuf;
          if(OPPEND(cvt->srcFormat)) CVTRATE_OE /* 16bit unsigned OE */
          else CVTRATE_SE
        }
      }
    }
    else /* 8bit */
    { sinc=slen, sid=dlen; while(sid>8388608) { sid>>=1, sinc>>=1; } /* prevent integer overflow */
      if(cvt->srcChans==2)
      { i=si=2;
        if(SIGNED(cvt->srcFormat))
        { Sint8 *src = (Sint8*)cvt->buf, *dest = (Sint8*)dbuf;
          CVTRATE2_SE
        }
        else
        { Uint8 *src = cvt->buf, *dest = (Uint8*)dbuf;
          CVTRATE2_SE
        }
      }
      else
      { i=si=1;
        if(SIGNED(cvt->srcFormat))
        { Sint8 *src = (Sint8*)cvt->buf, *dest = (Sint8*)dbuf;
          CVTRATE_SE
        }
        else
        { Uint8 *src = cvt->buf, *dest = (Uint8*)dbuf;
          CVTRATE_SE
        }
      }
    }
    if(dbuf!=cvt->buf)
    { memcpy(cvt->buf, dbuf, destLen);
      if(destLen>MAXALLOCA) free(dbuf);
    }
    #undef SE
    #undef CVTRATE_SE
    #undef CVTRATE_OE
    #undef CVTRATE2_SE
    #undef CVTRATE2_OE
  }
}

static void ConvertMixMono(Sint32 *dest, void* data, Uint32 samples, Uint16 srcFormat, int vol)
{ register Uint32 i=0;
  if(BITS(srcFormat)==8) /* 8bit */
  { if(SIGNED(srcFormat))  /* 8bit signed */
    { Sint8 *src = (Sint8*)data;
      if(vol>=256) for(; i<samples; i++) dest[i]+=src[i]<<8;
      else for(; i<samples; i++) dest[i]+=src[i]*vol;
    }
    else /* 8bit unsigned */
    { Uint8 *src = (Uint8*)data;
      if(vol>=256) for(; i<samples; i++) dest[i]+=(Sint8)(src[i]-128)<<8;
      else for(; i<samples; i++) dest[i]+=(Sint8)(src[i]-128)*vol;
    }
  }
  else /* 16bit */
  { if(OPPEND(srcFormat)) /* opposite endianness */
    { Uint16 *src = (Uint16*)data;
      if(SIGNED(srcFormat)) /* 16bit signed OE */
      { if(vol>=256) for(; i<samples; i++) dest[i]+=(Sint16)SWAPEND(src[i]);
        else for(; i<samples; i++) dest[i]+=((Sint16)SWAPEND(src[i])*vol)>>8;
      }
      else /* 16bit unsigned OE */
      { if(vol>=256) for(; i<samples; i++) dest[i]+=(Sint16)SWAPEND(src[i])-32768;
        else for(; i<samples; i++) dest[i]+=(((Sint16)SWAPEND(src[i])-32768)*vol)>>8;
      }
    }
    else /* same endianness */
    { if(SIGNED(srcFormat)) /* 16bit signed SE */
      { Sint16 *src = (Sint16*)data;
        if(vol>=256) for(; i<samples; i++) dest[i]+=src[i];
        else for(; i<samples; i++) dest[i]+=(src[i]*vol)>>8;
      }
      else /* 16bit unsigned SE */
      { Uint16 *src = (Uint16*)data;
        if(vol>=256) for(; i<samples; i++) dest[i]+=src[i]-32768;
        else for(; i<samples; i++) dest[i]+=((Sint16)(src[i]-32768)*vol)>>8;
      }
    }
  }
}

static void ConvertMixStereo(Sint32 *dest, void* data, Uint32 samples, Uint16 srcFormat, int left, int right)
{ register Uint32 i=0;
  if(BITS(srcFormat)==8) /* 8bit */
  { if(SIGNED(srcFormat))  /* 8bit signed */
    { Sint8 *src = (Sint8*)data;
      if(left>=256 && right>=256) for(; i<samples; i++) dest[i]+=src[i]<<8;
      else
        for(; i<samples; )
        { dest[i]+=src[i]*left;  i++;
          dest[i]+=src[i]*right; i++;
        }
    }
    else /* 8bit unsigned */
    { Uint8 *src = (Uint8*)data;
      if(left>=256 && right>=256) for(; i<samples; i++) dest[i]+=((Sint8)src[i]-128)<<8;
      else
        for(; i<samples;)
        { dest[i]+=(Sint8)(src[i]-128)*left;  i++;
          dest[i]+=(Sint8)(src[i]-128)*right; i++;
        }
    }
  }
  else /* 16bit */
  { if(OPPEND(srcFormat)) /* opposite endianness */
    { Uint16 *src = (Uint16*)data;
      if(SIGNED(srcFormat)) /* 16bit signed OE */
      { if(left>=256 && right>=256) for(; i<samples; i++) dest[i]+=(Sint16)SWAPEND(src[i]);
        else for(; i<samples;)
        { dest[i]+=((Sint16)SWAPEND(src[i])*left )>>8; i++;
          dest[i]+=((Sint16)SWAPEND(src[i])*right)>>8; i++;
        }
      }
      else /* 16bit unsigned OE */
      { if(left>=256 && right>=256) for(; i<samples; i++) dest[i]+=(Sint16)SWAPEND(src[i])-32768;
        else for(; i<samples;)
        { dest[i]+=(((Sint16)SWAPEND(src[i])-32768)*left )>>8; i++;
          dest[i]+=(((Sint16)SWAPEND(src[i])-32768)*right)>>8; i++;
        }
      }
    }
    else /* same endianness */
    { if(SIGNED(srcFormat)) /* 16bit signed SE */
      { Sint16 *src = (Sint16*)data;
        if(left>=256 && right>=256) for(; i<samples; i++) dest[i]+=src[i];
        else for(; i<samples;)
        { dest[i]+=(src[i]*left )>>8; i++;
          dest[i]+=(src[i]*right)>>8; i++;
        }
      }
      else /* 16bit unsigned SE */
      { Uint16 *src = (Uint16*)data;
        if(left>=256 && right>=256) for(; i<samples; i++) dest[i]+=src[i]-32768;
        else for(; i<samples;)
        { dest[i]+=((Sint16)(src[i]-32768)*left )>>8; i++;
          dest[i]+=((Sint16)(src[i]-32768)*right)>>8; i++;
        }
      }
    }
  }
}

int GLM_Init(Uint32 freq, Uint16 format, Uint8 channels, Uint32 bufferMs, MixCallback callback, void *context)
{ SDL_AudioSpec spec;
  int samples = freq*bufferMs/1000;
  if(initCount>0) { initCount++; return 0; }
  
  spec.freq     = freq;
  spec.format   = format;
  spec.channels = channels;
  spec.samples  = samples>65535 ? 65535 : samples;
  spec.callback = GLM_callback;
  spec.userdata = context;

  mixCallback = callback;
  SDL_PauseAudio(1);
  if(SDL_OpenAudio(&spec, &mixFormat)<0) return -1;
  mixAccSize = mixFormat.samples*mixFormat.channels;
  mixAcc = malloc(sizeof(Sint32)*mixAccSize);

  initCount++;
  return 0;
}

int GLM_GetFormat(Uint32 *freq, Uint16 *format, Uint8 *channels, Uint32 *bufferBytes)
{ if(!initCount)
  { SDL_SetError("Audio not initialized");
    return -1;
  }
  if(freq) *freq = mixFormat.freq;
  if(format) *format = mixFormat.format;
  if(channels) *channels = mixFormat.channels;
  if(bufferBytes) *bufferBytes = mixFormat.size;
  return 0;
}

void GLM_Quit()
{ if(initCount==0) return;
  if(--initCount==0)
  { SDL_LockAudio();
    SDL_PauseAudio(1);
    SDL_UnlockAudio();
    SDL_CloseAudio();
    free(mixAcc);
    mixCallback=NULL;
    mixAcc=NULL;
  }
}

Uint16 GLM_GetMixVolume()
{ return (Uint16)mixVolume;
}

void GLM_SetMixVolume(Uint16 volume)
{ mixVolume = volume>256 ? 256 : volume;
}

int GLM_ConvertAcc(void *dest, Sint32 *src, Uint32 samples, Uint16 destFormat)
{ Uint32 i=0;
  if(!dest || !src)
  { SDL_SetError("NULL pointer passed");
    return -1;
  }

  for(; i<samples; i++) if(src[i]<-32768) src[i]=-32768; else if(src[i]>32767) src[i]=32767;
  if(BITS(destFormat)==8) /* 8bit */  /* convert the accumulator into the proper format for the audio stream */
  { Uint8 *dbuf = (Uint8*)dest;
    i=0;
    if(SIGNED(destFormat)) for(; i<samples; i++) dbuf[i] = (Uint8)(src[i]>>8);
    else for(; i<samples; i++) dbuf[i] = (Uint8)(((Uint32)src[i]+32768)>>8);
  }
  else /* 16 bit */
  { Uint32 *sbuf = (Uint32*)src;
    i=0;
    if(OPPEND(destFormat)) /* opposite endianness */
    { Uint16 *dbuf = (Uint16*)dest;
      if(SIGNED(destFormat)) /* 16bit signed OE */
        for(; i<samples; i++) dbuf[i] = SWAPEND(sbuf[i]);
      else /* 16bit unsigned OE */
        for(; i<samples; i++) dbuf[i] = (Uint16)(SWAPEND(sbuf[i])+32768);
    }
    else /* same endianness */
    { Sint16 *dbuf = (Sint16*)dest;
      if(SIGNED(destFormat)) for(; i<samples; i++) dbuf[i] = src[i]; /* 16bit signed SE */
      else for(; i<samples; i++) dbuf[i] = (Sint16)(sbuf[i]+32768); /* 16bit unsigned SE */
    }
  }
  return 0;
}

int GLM_SetupCVT(GLM_AudioCVT *cvt)
{ if(!cvt) 
  { SDL_SetError("NULL pointer passed");
    return -1;
  }
  cvt->len_mul = cvt->destChans*BYTES(cvt->destFormat)*cvt->destRate;
  cvt->len_div = cvt->srcChans*BYTES(cvt->srcFormat)*cvt->srcRate;

  /* poor man's LCD (doesn't need to be perfect) */
  if(DIVISIBLE(cvt->len_mul, 441) && DIVISIBLE(cvt->len_div, 441)) /* most bitrates are divisible by 441 */
  { cvt->len_mul/=441, cvt->len_div/=441;
    while(DIVISIBLE(cvt->len_mul, 25) && DIVISIBLE(cvt->len_div, 25)) cvt->len_mul/=25, cvt->len_div/=25;
  }
  while(DIVISIBLE(cvt->len_mul, 2) && DIVISIBLE(cvt->len_div, 2)) cvt->len_mul/=2, cvt->len_div/=2;

  cvt->len_cvt = (int)((__int64)cvt->len*cvt->len_mul/cvt->len_div);
  return 0;
}

int GLM_Convert(GLM_AudioCVT *cvt)
{ int i, sfmt, dfmt, olen;
  if(!cvt || !cvt->buf)
  { SDL_SetError("NULL pointer passed");
    return -1;
  }
  if(cvt->len==0) return 0;

  sfmt=cvt->srcFormat, dfmt=cvt->destFormat, olen=cvt->len;

  if(cvt->srcChans<1 || cvt->srcChans>2 || cvt->destChans<1 || cvt->destChans>2)
  { SDL_SetError("Unsupported number of channels");
    return -1;
  }

  if(BITS(sfmt)==BITS(dfmt) && OPPEND(sfmt)!=OPPEND(dfmt))
  { if(BITS(sfmt)==16)
    { Uint16 *buf = (Uint16*)cvt->buf;
      for(i=cvt->len/2; i; buf++,i--) *buf=SWAPEND(*buf);
    }
    sfmt = cvt->srcFormat = (sfmt&~0x1000)|(dfmt&0x1000);
  }

  if(cvt->srcChans>cvt->destChans) StereoToMono(cvt);

  if(BITS(sfmt)<BITS(dfmt)) EightToSixteen(cvt);
  else if(BITS(sfmt)>BITS(dfmt)) SixteenToEight(cvt);
  else if(SIGNED(sfmt)!=SIGNED(dfmt)) /* if bit size is different, then sign conversion has already been done */
  { int len=cvt->len;
    i=0;
    if(BITS(sfmt)==8) /* 8bit */
    { Sint8 *buf = cvt->buf;
      for(; i<len; i++) buf[i] ^= 0x80; /* handles both */
    }
    else /* 16bit */
    { Uint8 *buf = cvt->buf;
      if(!OPPEND(sfmt)) i++;
      for(; i<len; i+=2) buf[i] ^= 0x80; /* handles both, assuming 'i' has been set properly */
    }
    cvt->srcFormat ^= 0x8000;
  }

  if(cvt->srcRate!=cvt->destRate) ConvertRate(cvt, cvt->srcChans<cvt->destChans ? cvt->len_cvt/2 : cvt->len_cvt);
  
  if(cvt->srcChans<cvt->destChans) MonoToStereo(cvt);
  cvt->len = olen;
  return 0;
}

int GLM_Copy(Sint32 *dest, Sint32 *src, Uint32 samples)
{ if(!dest || !src)
  { SDL_SetError("NULL pointer passed");
    return -1;
  }
  memcpy(dest, src, sizeof(Sint32)*samples);
  return 0;
}

int GLM_VolumeScale(Sint32 *stream, Uint32 samples, Uint16 leftVolume, Uint16 rightVolume)
{ register Uint32 i=0;
  int left=leftVolume, right=rightVolume;
  if(!stream)
  { SDL_SetError("NULL pointer passed");
    return -1;
  }
  if(left>=256 && right>=256) return 0;
  if(left==0 && right==0)
  { memset(stream, 0, samples*sizeof(int));
    return 0;
  }
  if(mixFormat.channels==1)
  { left = (left+right)>>1;
    for(; i<samples; i++) stream[i]=(stream[i]*left)>>8;
  }
  else if(left>=256 || right>=256)
  { if(left>=256) left=right,i=1;
    for(; i<samples; i+=2) stream[i]=(stream[i]*left)>>8;
  }
  else
    for(; i<samples;)
    { stream[i]=(stream[i]*left)>>8; i++;
      stream[i]=(stream[i]*right)>>8; i++;
    }
  return 0;
}

int GLM_Mix(Sint32 *dest, Sint32 *src, Uint32 samples, Uint16 leftVolume, Uint16 rightVolume)
{ register Uint32 i=0;
  int left=leftVolume, right=rightVolume;
  if(!dest || !src)
  { SDL_SetError("NULL pointer passed");
    return -1;
  }
  if(left==0 && right==0) return 0;
  if(mixFormat.channels==1)
  { left = (left+right)>>1;
    if(left>=256) for(; i<samples; i++) dest[i]+=src[i];
    else for(; i<samples; i++) dest[i]+=(src[i]*left)>>8;
  }
  else if(left>=256 && right>=256) for(; i<samples; i++) dest[i]+=src[i];
  else if(left>=256)
    for(; i<samples;)
    { dest[i] += src[i]; i++;
      dest[i] += (src[i]*right)>>8; i++;
    }
  else if(right>=256)
    for(; i<samples;)
    { dest[i] += (src[i]*left)>>8; i++;
      dest[i] += src[i]; i++;
    }
  else
   for(; i<samples;)
   { dest[i]+=(src[i]*left)>>8; i++;
     dest[i]+=(src[i]*right)>>8; i++;
   }
  return 0;
}

int GLM_ConvertMix(Sint32 *dest, void* data, Uint32 samples, Uint16 srcFormat, Uint16 leftVolume, Uint16 rightVolume)
{ if(!dest || !data)
  { SDL_SetError("NULL pointer passed");
    return -1;
  }
  if(mixFormat.channels==1) ConvertMixMono(dest, data, samples, srcFormat, ((int)leftVolume+(int)rightVolume)>>1);
  else ConvertMixStereo(dest, data, samples, srcFormat, leftVolume, rightVolume);
  return 0;
}

int GLM_DivideAccumulator(Sint32 divisor)
{ int i=0, len=mixAccSize;
  if(divisor<2) return 0;
  switch(divisor) /* i wonder if this switch is worthwhile? */
  { case 256: for(; i<len; i++) mixAcc[i]>>=8; break;
    case 128: for(; i<len; i++) mixAcc[i]>>=7; break;
    case  64: for(; i<len; i++) mixAcc[i]>>=6; break;
    case  32: for(; i<len; i++) mixAcc[i]>>=5; break;
    case  16: for(; i<len; i++) mixAcc[i]>>=4; break;
    case   8: for(; i<len; i++) mixAcc[i]>>=3; break;
    case   4: for(; i<len; i++) mixAcc[i]>>=2; break;
    case   2: for(; i<len; i++) mixAcc[i]>>=1; break;
    default:  for(; i<len; i++) mixAcc[i]/=divisor; break;
  }
  return 0;
}