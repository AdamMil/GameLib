#include "Mixer.h"
#include <stdlib.h>
#include <string.h>

#define BYTES(fmt) ((fmt)0xFF)
#define BYTES(fmt) (BYTES(fmt)>>3)
#define DIVISIBLE(n, d) ((n)/(d)*(d)==(n))
#define SIGNED(fmt) ((fmt)&0x8000)
#define SWAPEND(v) (((v)<<8)|((v)>>8))

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
{ int samples;
  if(!mixCallback) return;

  if(mixVolume>0)
  { samples = bytes/BYTES(mixFormat.format);
    memset(mixAcc, 0, samples*sizeof(Sint32)); /* zero the accumulator */
    mixCallback(mixAcc, samples, userdata);  /* call the user callback to mix in the audio */
    GLM_VolumeScale(mixAcc, samples, mixVolume);
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
  { if(SIGNED(sfmt)) /* 8bit signed */
    { Sint8 *src = cvt->buf+cvt->len-2, *dest=cvt->buf+cvt->len/2-1;
      for(i=cvt->len; i; src-=2,i--) *dest-- = (src[0]+src[1])/2;
    }
    else /* 8bit unsigned */
    { Uint8 *src = cvt->buf+cvt->len-2, *dest=cvt->buf+cvt->len/2-1;
      for(i=cvt->len; i; src-=2,i--) *dest-- = (src[0]+src[1])/2;
    }
  }
  else
  { if(OPPEND(sfmt)) /* opposite endianness */
    { Uint16 *src = cvt->buf+cvt->len/2-4;
      if(SIGNED(sfmt)) /* 16bit signed OE */
      { Sint16 *dest = cvt->buf+cvt->len/4-2;
        for(i=cvt->len/2; i; src-=2,i--) *dest-- = ((Sint32)SWAPEND(src[0])+(Sint32)SWAPEND(src[1]))/2;
      }
      else /* 16bit unsigned OE */
      { Uint16 *dest = cvt->buf+cvt->len/4-2;
        for(i=cvt->len/2; i; src-=2,i--) *dest-- = (SWAPEND(src[0])+SWAPEND(src[1]))/2;
      }
    }
    else /* same endianness */
    { if(SIGNED(sfmt)) /* 16bit signed SE */
      { SInt16 *src = cvt->buf+cvt->len/2-4, *dest = cvt->buf+cvt->len/4-2;
        for(i=cvt->len/2; i; src-=2,i--) *dest-- = (src[0]+src[1])/2;
      }
      else /* 16bit unsigned OE */
      { UInt16 *src = cvt->buf+cvt->len/2-4, *dest = cvt->buf+cvt->len/4-2;
        for(i=cvt->len/2; i; src-=2,i--) *dest-- = (src[0]+src[1])/2;
      }
    }
  }    
}

static void MonoToStereo(GLM_AudioCVT *cvt)
{ int i;
  if(BITS(cvt->srcFormat)==8)
  { Uint8 *src = cvt->buf, *dest = cvt->buf;
    for(i=cvt->len; i; src+=2,i--) src[0]=src[1]*dest++;
  }
  else
  { Uint16 *src = cvt->buf, *dest = cvt->buf;
    for(i=cvt->len/2; i; src+=2,i--) src[0]=src[1]*dest++;
  }
}

static void EightToSixteen(GLM_AudioCVT *cvt)
{ int i = cvt->len, sfmt = cvt->srcFormat, dfmt = cvt->destFormat;
  if(SIGNED(sfmt)) /* 8bit signed to 16bit */
  { if(SIGNED(dfmt)) /* 8bit signed to 16bit signed */
    { Sint8  *src  = cvt->buf+cvt->len-1;
      Sint16 *dest = cvt->buf+cvt->len*2-2;
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
      Uint16 *dest = cvt->buf+cvt->len*2-2, dv;
      if(OPPEND(dfmt)) /* 8bit signed to 16bit unsigned OE */
        for(; i; i--)
        { dv=-*src--<<8;
          *dest-- = SWAPEND(dv);
        }
      else for(; i; i--) *dest = -*src--<<8; /* 8bit signed to 16bit unsigned SE */
    }
  }
  else /* 8bit unsigned to 16bit */
  { Uint8 *src = cvt->buf+cvt->len-1;
    if(SIGNED(dfmt)) /* 8bit unsigned to 16bit signed */
    { Sint16 *dest = cvt->buf+cvt->len*2-2, dv;
      if(OPPEND(dfmt)) /* 8bit unsigned to 16bit signed OE */
        for(; i; i--)
        { dv = -*src--<<8;
          *dest-- = SWAPEND(dv);
        }
      else for(; i; i--) *dest-- = -*src--<<8; /* 8bit unsigned to 16bit signed SE */
    }
    else /* 8bit unsigned to 16bit unsigned */
    { Uint16 *dest = cvt->buf+cvt->len*2-2;
      Uint8   dv;
      if(OPPEND(dfmt)) /* 8bit unsigned to 16bit unsigned OE */
        for(; i; i--)
        { dv = *src--<<8;
          *dest-- = SWAPEND(sv);
        }
      else for(; i; i--) *dest-- = *src--<<8; /* 8bit unsigned to 16bit unsigned SE */
    }
  }
}

static void SixteenToEight(GLM_AudioCVT *cvt)
{ int i = cvt->len/2;
  if(SIGNED(cvt->srcFormat)) /* 16bit signed to 8bit */
  { if(SIGNED(cvt->destFormat) /* 16bit signed to 8bit signed */
    { Sint16 *src  = cvt->buf+cvt->len-2;
      Sint8  *dest = cvt->buf+cvt->len/2-1;
      if(OPPEND(cvt->srcFormat)) /* 16bit signed OE to 8bit signed */
        for(; i; src--,i--) *dest-- = (Sint16)SWAPEND((Uint16)*src);
      else for(; i; i--) *dest-- = *src--;
    }
    else /* 16bit signed to 8bit unsigned */
    { Uint16 *src  = cvt->buf+cvt->len-2;
      Uint8  *dest = cvt->buf+cvt->len/2-1;
      if(OPPEND(cvt->srcFormat))
        for(; i; src--,i--) *dest-- = (Uint8)-(Sint16)SWAPEND(*src); /* 16bit signed OE to 8bit unsigned */
      else for(; i; i--) *dest-- = (Uint8)-(Sint16)*src--; /* 16bit signed SE to 8bit unsigned */
    }
  }
  else /* 16bit unsigned to 8bit */
  { Uint16 *src = cvt->buf+cvt->len-2;
    if(SIGNED(cvt->destFormat)) /* 16bit unsigned to 8bit signed */
    { Sint8 *dest = cvt->buf+cvt->len/2-1;
      if(OPPEND(cvt->srcFormat)) /* 16bit unsigned OE to 8bit signed */
        for(; i; src--,i--) *dest-- = (Sint8)-(Sint16)SWAPEND(*src);
      else for(; i; i--) *dest-- = (Sint8)-(Sint16)*src--; /* 16bit unsigned SE to 8bit signed */
    }
    else /* 16bit unsigned to 8bit unsigned */
    { Uint8 *dest = cvt->buf+cvt->len/2-1;
      if(OPPEND(cvt->srcFormat)) for(; i; src--,i--) *dest-- = SWAPEND(*src); /* 16bit unsigned OE to 8bit unsigned */
      else for(; i; i--) *dest-- = *src--; /* 16bit unsigned SE to 8bit unsigned */
    }
  }
}

static void ConvertRate(GLM_AudioCVT *cvt)
{ int i, srate=cvt->srcRate, drate=cvt->destRate;
  if(srate/drate==2 || cvt->len<=BYTES(cvt->srcFormat)) /* halving rate or just 1 sample*/
  { if(BITS(cvt->srcFormat)==16) /* 16bit */
    { Uint16 *src = cvt->buf, *dest = src;
      i = cvt->len/2;
      else for(; i; src+=2,i--) *dest++ = *src;
    }
    else /* 8bit */
    { Uint8 *src = cvt->buf, *dest = src;
      i = cvt->len;
      else for(; i; src,i--) *dest++ = *src++;
    }
  }
  else /* any rate, length must be >1 sample */
  { int sic=0, dlen, s0, s1, diff, t;
    i=1;
    if(BITS(cvt->srcFormat)==16) /* 16bit */
    { Sint16 *buf = cvt->buf;
      dlen=cvt->len/2*drate/srate;
      if(OPPEND(cvt->srcFormat)) /* 16bit signed OE */
      { s0=SWAPEND(buf[0]), s1=SWAPEND(buf[1]), diff=s1-s0;
        for(; i<dlen; i++)
        { sic += srate;
          if(sic>drate)
          { s0=s1;
            do si++, sic-=drate; while(sic>drate);
            s1=SWAPEND(buf[si]), diff=s1-s0;
          }
          t = s0+diff*drate/sic;
          buf[i] = SWAPEND(t);
        }
      }
      else /* 16 bit signed SE */
      { s0=buf[0], s1=buf[1], diff=s1-s0;
        for(; i<dlen; i++)
        { sic += srate;
          if(sic>drate)
          { s0=s1;
            do si++, sic-=drate; while(sic>drate);
            s1=buf[si], diff=s1-s0;
          }
          buf[i] = s0+diff*drate/sic;
        }
      }
    }
    else /* 8bit */
    { Sint8 *buf = cvt->buf;
      dlen=cvt->len*drate/srate;
      s0=buf[0], s1=buf[1], diff=s1-s0;
      for(; i<dlen; i++)
      { sic += srate;
        if(sic>drate)
        { s0=s1;
          do si++, sic-=drate; while(sic>drate);
          s1=buf[si], diff=s1-s0;
        }
        buf[i] = s0+diff*drate/sic;
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

  if(BITS(destFormat)==8) /* 8bit */  /* convert the accumulator into the proper format for the audio stream */
  { Uint8 *dbuf = (Uint8*)dest;
    if(SIGNED(destFormat)) for(; i<samples; i++) dbuf[i] = (Uint8)src[i];
    else for(; i<samples; i++) dbuf[i] = (Uint8)-src[i];
  }
  else /* 16 bit */
  { if(OPPEND(destFormat)) /* opposite endianness */
    { Uint32 *sbuf = (Uint32*)src;
      Uint16 *dbuf = (Uint16*)dest;
      if(SIGNED(destFormat)) /* 16bit signed OE */
        for(; i<samples; i++) dbuf[i] = SWAPEND(sbuf[i]);
      else /* 16bit signed SE */
        for(; i<samples; i++) dbuf[i] = -SWAPEND(sbuf[i]);
    }
    else /* same endianness */
    { Sint16 *dbuf = (Sint16*)dest;
      if(SIGNED(destFormat)) for(; i<samples; i++) dbuf[i] = (Sint16)src[i]; /* 16bit signed SE */
      else for(; i<samples; i++) dbuf[i] = (Sint16)-src[i];
    }
  }
  return 0;
}

int GLM_SetupCVT(GLM_AudioCVT *cvt, int inputLen)
{ int i;
  if(!cvt) 
  { SDL_SetError("NULL pointer passed");
    return -1;
  }
  cvt->len_mul  = cvt->srcChannels,      cvt->len_div  = cvt->destChannels;
  cvt->len_mul *= BYTES(cvt->srcFormat), cvt->len_div *= BYTES(cvt->destFormat);
  cvt->len_mul *= cvt->srcRate,          cvt->len_div *= cvt->destRate;

  /* poor man's LCD (doesn't need to be perfect) */
  if(DIVISIBLE(cvt->len_mul, 441) && DIVISIBLE(cvt->len_div, 441))
  { cvt->len_mul/=441, cvt->len_div/=441;
    while(DIVISIBLE(cvt->len_mul, 25) && DIVISIBLE(cvt->len_div, 25)) cvt->len_mul/=25, cvt->len_div/=25;
  }
  while(DIVISIBLE(cvt->len_mul, 2) && DIVISIBLE(cvt->len_div, 2)) cvt->len_mul/=2, cvt->len_div/=2;

  cvt->len_cvt = inputLen*cvt->len_mul/cvt->len_div;
}

int GLM_Convert(GLM_AudioCVT *cvt)
{ int i, sfmt, dfmt;
  if(!cvt || !cvt->buf)
  { SDL_SetError("NULL pointer passed");
    return -1;
  }

  sfmt=cvt->srcFormat, dfmt=cvt->destFormat;

  if(BITS(sfmt)!=16 || BITS(dfmt)!=16 || cvt->srcChannels<1 || cvt->srcChannels>2 ||
     cvt->destChannels<1 || cvt->destChannels>2)
  { SDL_SetError("Unsupported audio format");
    return -1;
  }
     
  if(BITS(sfmt)==BITS(dfmt) && OPPEND(sfmt)!=OPPEND(dfmt))
  { if(BITS(sfmt)==16)
    { Uint16 *buf = (Uint16*)cvt->buf;
      for(i=cvt->len/2; i; buf++,i--) *buf=SWAPEND(*buf);
    }
    sfmt = cvt->srcFormat = (sfmt&~0x1000)|(dfmt&0x1000);
  }

  if(cvt->srcChannels>cvt->destChannels) StereoToMono(cvt);

  if(BITS(sfmt)<BITS(dfmt)) EightToSixteen(cvt);
  else if(BITS(sfmt)>BITS(dfmt)) SixteenToEight(cvt);
  else if(SIGNED(sfmt)!=SIGNED(dfmt)) /* if bit size is different, then sign conversion has already been done */
  { if(BITS(sfmt)==8) /* 8bit */
    { Sint8 *buf = cvt->buf;
      for(i=cvt->len; i; src++,i--) *buf = -*buf; /* handles both */
    }
    else /* 16bit */
    { Sint16 *buf = cvt->buf;
      for(i=cvt->len/2; i; src+=2,i--) *buf = -*buf; /* handles both */
    }
  }

  if(cvt->srcRate!=cvt->destRate) ConvertRate(cvt);
  
  if(cvt->srcChannels<cvt->destChannels) MonoToStereo(cvt);
}

int GLM_Copy(Sint32 *dest, Sint32 *src, Uint32 samples)
{ if(!dest || !src)
  { SDL_SetError("NULL pointer passed");
    return -1;
  }
  memcpy(dest, src, sizeof(Sint32)*samples);
  return 0;
}

int GLM_VolumeScale(Sint32 *stream, Uint32 samples, Uint16 volume)
{ Uint32 i;
  int vol=volume;
  if(vol==256) return 0;
  if(!stream)
  { SDL_SetError("NULL pointer passed");
    return -1;
  }
  for(i=0; i<samples; i++) stream[i]=(stream[i]*vol)>>8;
  return 0;
}

int GLM_Mix(Sint32 *dest, Sint32 *src, Uint32 samples, Uint16 srcVolume)
{ Uint32 i=0;
  int volume = srcVolume;
  if(!dest || !src)
  { SDL_SetError("NULL pointer passed");
    return -1;
  }
  if(volume>=256) for(; i<samples; i++) dest[i]+=src[i];
  else for(; i<samples; i++) dest[i]+=(src[i]*volume)>>8;
  return 0;
}

int GLM_ConvertMix(Sint32 *dest, void* data, Uint32 samples, Uint16 srcFormat, Uint16 srcVolume)
{ Uint32 i=0;
  int vol=srcVolume;

  if(!dest || !data)
  { SDL_SetError("NULL pointer passed");
    return -1;
  }

  if(BITS(srcFormat)==8) /* 8bit */
  { Sint8 *src = (Uint8*)data;
    if(SIGNED(srcFormat))  /* 8bit signed */
    { if(vol>=256) for(; i<samples; i++) dest[i]+=src[i];
      else for(; i<samples; i++) dest[i]+=(src[i]*vol)>>8;
    }
    else /* 8bit unsigned */
    { if(vol>=256) for(; i<samples; i++) dest[i]+=-src[i];
      else for(; i<samples; i++) dest[i]+=(-src[i]*vol)>>8;
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
      { if(vol>=256) for(; i<samples; i++) dest[i]+=-(Sint16)SWAPEND(src[i]);
        else for(; i<samples; i++) dest[i]+=(-(Sint16)SWAPEND(src[i])*vol)>>8;
      }
    }
    else /* same endianness */
    { Sint16 *src = (Sint16*)data;
      if(SIGNED(srcFormat)) /* 16bit signed SE */
      { if(vol>=256) for(; i<samples; i++) dest[i]+=src[i];
        else for(; i<samples; i++) dest[i]+=(src[i]*vol)>>8;
      }
      else /* 16bit unsigned SE */
      { if(vol>=256) for(; i<samples; i++) dest[i]+=-src[i];
        else for(; i<samples; i++) dest[i]+=(-src[i]*vol)>>8;
      }
    }
  }
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