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

// FIXME: HACK: change this to SDLCALL when releasing
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