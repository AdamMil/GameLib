#include "Mixer.h"
#include <stdlib.h>
#include <string.h>

static SDL_AudioSpec mixFormat;
static MixCallback   mixCallback;
static Sint32       *mixAcc;
static Sint32        mixAccSize;
static int           initCount, mixVolume=256;

static void GLM_callback(void *userdata, Uint8 *stream, int bytes)
{ int samples;
  if(!mixCallback) return;

  if(mixVolume>0)
  { samples = bytes/((mixFormat.format&0xFF)>>3);
    memset(mixAcc, 0, samples*sizeof(Sint32)); /* zero the accumulator */
    mixCallback(mixAcc, samples, userdata);  /* call the user callback to mix in the audio */
    GLM_VolumeScale(mixAcc, samples, mixVolume);
    GLM_ConvertAcc(stream, mixAcc, samples, mixFormat.format);
  }
  else
  { if(mixFormat.format&0x8000) memset(stream, 0, bytes);
    else if((mixFormat.format&0xFF)==8) memset(stream, 128, bytes);
    else
    { Uint16 *buf = (Uint16*)stream;
      int     i, len = bytes/2;
      for(i=0; i<len; i++) buf[i]=32768;
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

int GLM_Copy(Sint32 *dest, Sint32 *src, Uint32 samples)
{ if(!dest || !src) return -1;
  memcpy(dest, src, sizeof(Sint32)*samples);
  return 0;
}

int GLM_VolumeScale(Sint32 *stream, Uint32 samples, Uint16 volume)
{ Uint32 i;
  int vol=volume;
  if(!stream) return -1;
  for(i=0; i<samples; i++) stream[i]=(stream[i]*vol)>>8;
  return 0;
}

int GLM_Mix(Sint32 *dest, Sint32 *src, Uint32 samples, Uint16 srcVolume)
{ Uint32 i=0;
  int volume = srcVolume;
  if(!dest || !src) return -1;
  if(volume>=256) for(; i<samples; i++) dest[i]+=src[i];
  else for(; i<samples; i++) dest[i]+=(src[i]*volume)>>8;
  return 0;
}

int GLM_ConvertAcc(void *dest, Sint32 *src, Uint32 samples, Uint16 destFormat)
{ Uint32 i=0;
  if(!dest || !src) return -1;

  if((destFormat&0xFF)==8) /* 8bit */  /* convert the accumulator into the proper format for the audio stream */
  { Uint8 *dbuf = (Uint8*)dest;
    if(destFormat&0x8000) /* 8bit signed */
      for(; i<samples; i++) dbuf[i] = (Uint8)src[i];
    else /* 8bit unsigned */
      for(; i<samples; i++) dbuf[i] = (Uint8)(src[i]+128);
  }
  else /* 16 bit */
  {
    #if SDL_BYTEORDER == SDL_LIL_ENDIAN
    if(destFormat&0x1000) /* opposite endianness */
    #else
    if(!(destFormat&0x1000)) /* opposite endianness */
    #endif
    { Uint32 *sbuf = (Uint32*)src;
      Uint16 *dbuf = (Uint16*)dest;
      if(destFormat&0x8000) /* 16bit signed OE */
        for(; i<samples; i++) dbuf[i] = (Uint16)(((sbuf[i]&0xFF)<<8)|(sbuf[i]>>8));
      else
        for(; i<samples; i++) dbuf[i] = (Uint16)((((sbuf[i]&0xFF)<<8)|(sbuf[i]>>8))+32768);
    }
    else /* same endianness */
    { Sint16 *dbuf = (Sint16*)dest;
      if(destFormat&0x8000) /* 16bit signed SE */
        for(; i<samples; i++) dbuf[i] = (Sint16)src[i];
      else
        for(; i<samples; i++) dbuf[i] = (Sint16)(src[i]+32768);
    }
  }
  return 0;
}

int GLM_ConvertMix(Sint32 *dest, void* data, Uint32 samples, Uint16 srcFormat, Uint16 srcVolume)
{ Uint32 i=0;
  int vol=srcVolume;

  if(!dest || !data) return -1;

  if((srcFormat&0xFF)==8) /* 8bit */
  { if(srcFormat&0x8000)  /* 8bit signed */
    { Sint8 *src = (Sint8*)data;
      if(vol>=256) for(; i<samples; i++) dest[i]+=src[i];
      else for(; i<samples; i++) dest[i]+=(src[i]*vol)>>8;
    }
    else /* 8bit unsigned */
    { Uint8 *src = (Uint8*)data;
      if(vol>=256) for(; i<samples; i++) dest[i]+=(Sint8)(src[i]-128);
      else for(; i<samples; i++) dest[i]+=((Sint8)(src[i]-128)*vol)>>8;
    }
  }
  else /* 16bit */
  { 
    #if SDL_BYTEORDER == SDL_LIL_ENDIAN
    if(srcFormat&0x1000) /* opposite endianness */
    #else
    if(!(srcFormat&0x1000)) /* opposite endianness */
    #endif
    { Uint16 *src = (Uint16*)data;
      if(srcFormat&0x8000) /* 16bit signed OE */
      { if(vol>=256) for(; i<samples; i++) dest[i]+=(Sint16)(((src[i]&0xFF)<<8)|(src[i]>>8));
        else for(; i<samples; i++) dest[i]+=(Sint16)(((((src[i]&0xFF)<<8)|(src[i]>>8))*vol)>>8);
      }
      else /* 16bit unsigned OE */
      { if(vol>=256) for(; i<samples; i++) dest[i]+=(Sint16)((((src[i]&0xFF)<<8)|(src[i]>>8))-32768);
        else for(; i<samples; i++) dest[i]+=((Sint16)((((src[i]&0xFF)<<8)|(src[i]>>8))-32768)*vol)>>8;
      }
    }
    else /* same endianness */
    { if(srcFormat&0x8000) /* 16bit signed SE */
      { Sint16 *src = (Sint16*)data;
        if(vol>=256) for(; i<samples; i++) dest[i]+=src[i];
        else for(; i<samples; i++) dest[i]+=(src[i]*vol)>>8;
      }
      else /* 16bit unsigned SE */
      { Uint16 *src = (Uint16*)data;
        if(vol>=256) for(; i<samples; i++) dest[i]+=(Sint16)(src[i]-32768);
        else for(; i<samples; i++) dest[i]+=((Sint16)(src[i]-32768)*vol)>>8;
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