#ifndef GAMELIB_MIXER_H
#define GAMELIB_MIXER_H

#include "SDL_types.h"
#include "SDL_audio.h"
#include "begin_code.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct
{ Uint8  *buf;
  Sint32 len;
  Sint32 srcRate, destRate;
  Sint32 len_cvt;
  Sint32 len_mul, len_div;
  Uint16 srcFormat, destFormat;
  Uint8  srcChans,  destChans;
} GLM_AudioCVT;

typedef void (__stdcall *MixCallback)(Sint32 *stream, Uint32 frames, void *context);

extern DECLSPEC int  SDLCALL GLM_Init(Uint32 freq, Uint16 format, Uint8 channels, Uint32 bufferMs,
                                      MixCallback callback, void *context);
extern DECLSPEC int  SDLCALL GLM_GetFormat(Uint32 *freq, Uint16 *format, Uint8 *channels, Uint32 *bufferBytes);
extern DECLSPEC void SDLCALL GLM_Quit();

extern DECLSPEC Uint16 SDLCALL GLM_GetMixVolume();
extern DECLSPEC void   SDLCALL GLM_SetMixVolume(Uint16 volume);

extern DECLSPEC int SDLCALL GLM_ConvertAcc(void *dest, Sint32 *src, Uint32 samples, Uint16 destFormat);
extern DECLSPEC int SDLCALL GLM_SetupCVT(GLM_AudioCVT *cvt);
extern DECLSPEC int SDLCALL GLM_Convert(GLM_AudioCVT *cvt);

extern DECLSPEC int SDLCALL GLM_Copy(Sint32 *dest, Sint32 *src, Uint32 samples);
extern DECLSPEC int SDLCALL GLM_VolumeScale(Sint32 *stream, Uint32 samples, Uint16 leftVolume, Uint16 rightVolume);
extern DECLSPEC int SDLCALL GLM_Mix(Sint32 *dest, Sint32 *src, Uint32 samples, Uint16 leftVolume, Uint16 rightVolume);
extern DECLSPEC int SDLCALL GLM_ConvertMix(Sint32 *dest, void *src, Uint32 samples, Uint16 srcFormat,
                                           Uint16 leftVolume, Uint16 rightVolume);
extern DECLSPEC int SDLCALL GLM_DivideAccumulator(Sint32 divisor);

#ifdef __cplusplus
}
#endif

#include "close_code.h"

#endif /* GAMELIB_MIXER_H */