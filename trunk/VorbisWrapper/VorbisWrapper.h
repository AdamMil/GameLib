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

#ifndef GAMELIB_VORBIS_H
#define GAMELIB_VORBIS_H

#include <vorbis/vorbisfile.h>
#include "SDL_types.h"
#include "begin_code.h"

#ifdef __cplusplus
extern "C" {
#endif

enum { ALLSECTIONS=-1 };

typedef struct
{ Sint32 (SDLCALL *Read)(void *dest, Sint32 size, Sint32 maxnum);
  Sint32 (SDLCALL *Seek)(Sint32 offset, int whence);
  Sint32 (SDLCALL *Tell)();
  void   (SDLCALL *Close)();
  void  *Context;
} VW_Callbacks;

extern DECLSPEC int  SDLCALL VW_Open(OggVorbis_File **vf, VW_Callbacks calls);
extern DECLSPEC void SDLCALL VW_Close(OggVorbis_File *vf);

extern DECLSPEC Sint32 SDLCALL VW_PcmLength(OggVorbis_File *vf, int section);
extern DECLSPEC Sint32 SDLCALL VW_PcmTell(OggVorbis_File *vf);
extern DECLSPEC Sint32 SDLCALL VW_PcmSeek(OggVorbis_File *vf, Sint32 frames);
extern DECLSPEC double SDLCALL VW_TimeLength(OggVorbis_File *vf, int section);
extern DECLSPEC double SDLCALL VW_TimeTell(OggVorbis_File *vf);
extern DECLSPEC Sint32 SDLCALL VW_TimeSeek(OggVorbis_File *vf, double seconds);

extern DECLSPEC Sint32 SDLCALL VW_Read(OggVorbis_File *vf, Uint8 *buf, Sint32 length, int bigEndian, int word, int sgned, int *section);

extern DECLSPEC vorbis_info * SDLCALL VW_Info(OggVorbis_File *vf, int section);

#ifdef __cplusplus
}
#endif

#include "close_code.h"

#endif /* GAMELIB_VORBIS_H */