#ifndef GAMELIB_UTILITY_H
#define GAMELIB_UTILITY_H

#include "SDL_types.h"
#include "begin_code.h"

#ifdef __cplusplus
extern "C" {
#endif

extern DECLSPEC int   SDLCALL GLU_Init();
extern DECLSPEC void  SDLCALL GLU_Quit();
extern DECLSPEC char* SDLCALL GLU_GetError();

extern DECLSPEC Uint32 SDLCALL GLU_GetMilliseconds();
extern DECLSPEC Uint64 SDLCALL GLU_GetTimerFrequency();
extern DECLSPEC Uint64 SDLCALL GLU_GetTimerCounter();
extern DECLSPEC double SDLCALL GLU_GetSeconds();

#ifdef __cplusplus
}
#endif

#include "close_code.h"

#endif /* GAMELIB_UTILITY_H */