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

#ifndef GAMELIB_UTILITY_H
#define GAMELIB_UTILITY_H

#include <wchar.h>
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

extern DECLSPEC wchar_t SDLCALL GLU_Getch();
extern DECLSPEC wchar_t SDLCALL GLU_Getche();

extern DECLSPEC void SDLCALL GLU_MemCopy(void *src, void *dest, int length);
extern DECLSPEC void SDLCALL GLU_MemFill(void *dest, Uint8 value, int length);
extern DECLSPEC void SDLCALL GLU_MemMove(void *src, void *dest, int length);

#ifdef __cplusplus
}
#endif

#include "close_code.h"

#endif /* GAMELIB_UTILITY_H */