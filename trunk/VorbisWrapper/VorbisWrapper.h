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
{ Sint32 (__stdcall *Read)(void *context, void *dest, Sint32 size, Sint32 maxnum); // TODO: change to SDLCALL
  Sint32 (__stdcall *Seek)(void *context, Sint32 offset, int whence);
  Sint32 (__stdcall *Tell)(void *context);
  void   (__stdcall *Close)(void *context);
  void  *Context;
} VW_Callbacks;

extern DECLSPEC int  SDLCALL VW_Open(OggVorbis_File *vf, VW_Callbacks *calls);
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