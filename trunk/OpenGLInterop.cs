/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2006 Adam Milazzo

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

using System;
using System.Runtime.InteropServices;

namespace GameLib.Interop.OpenGL
{

#region OpenGL
/// <summary>This class provides access to the native, low-level OpenGL API. See the official OpenGL documentation for
/// information regarding these methods.
/// </summary>
/// <remarks>This class currently supports OpenGL through version 1.3.</remarks>
[System.Security.SuppressUnmanagedCodeSecurity()]
public sealed class GL
{ private GL() { }

  #region OpenGL 1.1
  #region Flags, Enums, Defines, etc
  #region AccumOp
  public const uint GL_ACCUM                          =0x0100;
  public const uint GL_LOAD                           =0x0101;
  public const uint GL_RETURN                         =0x0102;
  public const uint GL_MULT                           =0x0103;
  public const uint GL_ADD                            =0x0104;
  #endregion

  #region AlphaFunction
  public const uint GL_NEVER                          =0x0200;
  public const uint GL_LESS                           =0x0201;
  public const uint GL_EQUAL                          =0x0202;
  public const uint GL_LEQUAL                         =0x0203;
  public const uint GL_GREATER                        =0x0204;
  public const uint GL_NOTEQUAL                       =0x0205;
  public const uint GL_GEQUAL                         =0x0206;
  public const uint GL_ALWAYS                         =0x0207;
  #endregion

  #region AttribMask
  public const uint GL_CURRENT_BIT                    =0x00000001;
  public const uint GL_POINT_BIT                      =0x00000002;
  public const uint GL_LINE_BIT                       =0x00000004;
  public const uint GL_POLYGON_BIT                    =0x00000008;
  public const uint GL_POLYGON_STIPPLE_BIT            =0x00000010;
  public const uint GL_PIXEL_MODE_BIT                 =0x00000020;
  public const uint GL_LIGHTING_BIT                   =0x00000040;
  public const uint GL_FOG_BIT                        =0x00000080;
  public const uint GL_DEPTH_BUFFER_BIT               =0x00000100;
  public const uint GL_ACCUM_BUFFER_BIT               =0x00000200;
  public const uint GL_STENCIL_BUFFER_BIT             =0x00000400;
  public const uint GL_VIEWPORT_BIT                   =0x00000800;
  public const uint GL_TRANSFORM_BIT                  =0x00001000;
  public const uint GL_ENABLE_BIT                     =0x00002000;
  public const uint GL_COLOR_BUFFER_BIT               =0x00004000;
  public const uint GL_HINT_BIT                       =0x00008000;
  public const uint GL_EVAL_BIT                       =0x00010000;
  public const uint GL_LIST_BIT                       =0x00020000;
  public const uint GL_TEXTURE_BIT                    =0x00040000;
  public const uint GL_SCISSOR_BIT                    =0x00080000;
  public const uint GL_ALL_ATTRIB_BITS                =0x000fffff;
  #endregion

  #region BeginMode
  public const uint GL_POINTS                         =0x0000;
  public const uint GL_LINES                          =0x0001;
  public const uint GL_LINE_LOOP                      =0x0002;
  public const uint GL_LINE_STRIP                     =0x0003;
  public const uint GL_TRIANGLES                      =0x0004;
  public const uint GL_TRIANGLE_STRIP                 =0x0005;
  public const uint GL_TRIANGLE_FAN                   =0x0006;
  public const uint GL_QUADS                          =0x0007;
  public const uint GL_QUAD_STRIP                     =0x0008;
  public const uint GL_POLYGON                        =0x0009;
  #endregion

  #region BlendingFactorDest
  public const uint GL_ZERO                           =0;
  public const uint GL_ONE                            =1;
  public const uint GL_SRC_COLOR                      =0x0300;
  public const uint GL_ONE_MINUS_SRC_COLOR            =0x0301;
  public const uint GL_SRC_ALPHA                      =0x0302;
  public const uint GL_ONE_MINUS_SRC_ALPHA            =0x0303;
  public const uint GL_DST_ALPHA                      =0x0304;
  public const uint GL_ONE_MINUS_DST_ALPHA            =0x0305;
  #endregion

  #region BlendingFactorSrc
  /*      GL_ZERO */
  /*      GL_ONE */
  public const uint GL_DST_COLOR                      =0x0306;
  public const uint GL_ONE_MINUS_DST_COLOR            =0x0307;
  public const uint GL_SRC_ALPHA_SATURATE             =0x0308;
  /*      GL_SRC_ALPHA */
  /*      GL_ONE_MINUS_SRC_ALPHA */
  /*      GL_DST_ALPHA */
  /*      GL_ONE_MINUS_DST_ALPHA */
  #endregion

  #region Boolean
  public const uint GL_TRUE                           =1;
  public const uint GL_FALSE                          =0;
  #endregion

  #region ClearBufferMask
  /*      GL_COLOR_BUFFER_BIT */
  /*      GL_ACCUM_BUFFER_BIT */
  /*      GL_STENCIL_BUFFER_BIT */
  /*      GL_DEPTH_BUFFER_BIT */
  #endregion

  #region ClientArrayType
  /*      GL_VERTEX_ARRAY */
  /*      GL_NORMAL_ARRAY */
  /*      GL_COLOR_ARRAY */
  /*      GL_INDEX_ARRAY */
  /*      GL_TEXTURE_COORD_ARRAY */
  /*      GL_EDGE_FLAG_ARRAY */
  #endregion

  #region ClipPlaneName
  public const uint GL_CLIP_PLANE0                    =0x3000;
  public const uint GL_CLIP_PLANE1                    =0x3001;
  public const uint GL_CLIP_PLANE2                    =0x3002;
  public const uint GL_CLIP_PLANE3                    =0x3003;
  public const uint GL_CLIP_PLANE4                    =0x3004;
  public const uint GL_CLIP_PLANE5                    =0x3005;
  #endregion

  #region ColorMaterialFace
  /*      GL_FRONT */
  /*      GL_BACK */
  /*      GL_FRONT_AND_BACK */
  #endregion

  #region ColorMaterialParameter
  /*      GL_AMBIENT */
  /*      GL_DIFFUSE */
  /*      GL_SPECULAR */
  /*      GL_EMISSION */
  /*      GL_AMBIENT_AND_DIFFUSE */
  #endregion

  #region ColorPointerType
  /*      GL_BYTE */
  /*      GL_UNSIGNED_BYTE */
  /*      GL_SHORT */
  /*      GL_UNSIGNED_SHORT */
  /*      GL_INT */
  /*      GL_UNSIGNED_INT */
  /*      GL_FLOAT */
  /*      GL_DOUBLE */
  #endregion

  #region CullFaceMode
  /*      GL_FRONT */
  /*      GL_BACK */
  /*      GL_FRONT_AND_BACK */
  #endregion

  #region DataType
  public const uint GL_BYTE                           =0x1400;
  public const uint GL_UNSIGNED_BYTE                  =0x1401;
  public const uint GL_SHORT                          =0x1402;
  public const uint GL_UNSIGNED_SHORT                 =0x1403;
  public const uint GL_INT                            =0x1404;
  public const uint GL_UNSIGNED_INT                   =0x1405;
  public const uint GL_FLOAT                          =0x1406;
  public const uint GL_2_BYTES                        =0x1407;
  public const uint GL_3_BYTES                        =0x1408;
  public const uint GL_4_BYTES                        =0x1409;
  public const uint GL_DOUBLE                         =0x140A;
  #endregion

  #region DepthFunction
  /*      GL_NEVER */
  /*      GL_LESS */
  /*      GL_EQUAL */
  /*      GL_LEQUAL */
  /*      GL_GREATER */
  /*      GL_NOTEQUAL */
  /*      GL_GEQUAL */
  /*      GL_ALWAYS */
  #endregion

  #region DrawBufferMode
  public const uint GL_NONE                           =0;
  public const uint GL_FRONT_LEFT                     =0x0400;
  public const uint GL_FRONT_RIGHT                    =0x0401;
  public const uint GL_BACK_LEFT                      =0x0402;
  public const uint GL_BACK_RIGHT                     =0x0403;
  public const uint GL_FRONT                          =0x0404;
  public const uint GL_BACK                           =0x0405;
  public const uint GL_LEFT                           =0x0406;
  public const uint GL_RIGHT                          =0x0407;
  public const uint GL_FRONT_AND_BACK                 =0x0408;
  public const uint GL_AUX0                           =0x0409;
  public const uint GL_AUX1                           =0x040A;
  public const uint GL_AUX2                           =0x040B;
  public const uint GL_AUX3                           =0x040C;
  #endregion

  #region Enable
  /*      GL_FOG */
  /*      GL_LIGHTING */
  /*      GL_TEXTURE_1D */
  /*      GL_TEXTURE_2D */
  /*      GL_LINE_STIPPLE */
  /*      GL_POLYGON_STIPPLE */
  /*      GL_CULL_FACE */
  /*      GL_ALPHA_TEST */
  /*      GL_BLEND */
  /*      GL_INDEX_LOGIC_OP */
  /*      GL_COLOR_LOGIC_OP */
  /*      GL_DITHER */
  /*      GL_STENCIL_TEST */
  /*      GL_DEPTH_TEST */
  /*      GL_CLIP_PLANE0 */
  /*      GL_CLIP_PLANE1 */
  /*      GL_CLIP_PLANE2 */
  /*      GL_CLIP_PLANE3 */
  /*      GL_CLIP_PLANE4 */
  /*      GL_CLIP_PLANE5 */
  /*      GL_LIGHT0 */
  /*      GL_LIGHT1 */
  /*      GL_LIGHT2 */
  /*      GL_LIGHT3 */
  /*      GL_LIGHT4 */
  /*      GL_LIGHT5 */
  /*      GL_LIGHT6 */
  /*      GL_LIGHT7 */
  /*      GL_TEXTURE_GEN_S */
  /*      GL_TEXTURE_GEN_T */
  /*      GL_TEXTURE_GEN_R */
  /*      GL_TEXTURE_GEN_Q */
  /*      GL_MAP1_VERTEX_3 */
  /*      GL_MAP1_VERTEX_4 */
  /*      GL_MAP1_COLOR_4 */
  /*      GL_MAP1_INDEX */
  /*      GL_MAP1_NORMAL */
  /*      GL_MAP1_TEXTURE_COORD_1 */
  /*      GL_MAP1_TEXTURE_COORD_2 */
  /*      GL_MAP1_TEXTURE_COORD_3 */
  /*      GL_MAP1_TEXTURE_COORD_4 */
  /*      GL_MAP2_VERTEX_3 */
  /*      GL_MAP2_VERTEX_4 */
  /*      GL_MAP2_COLOR_4 */
  /*      GL_MAP2_INDEX */
  /*      GL_MAP2_NORMAL */
  /*      GL_MAP2_TEXTURE_COORD_1 */
  /*      GL_MAP2_TEXTURE_COORD_2 */
  /*      GL_MAP2_TEXTURE_COORD_3 */
  /*      GL_MAP2_TEXTURE_COORD_4 */
  /*      GL_POINT_SMOOTH */
  /*      GL_LINE_SMOOTH */
  /*      GL_POLYGON_SMOOTH */
  /*      GL_SCISSOR_TEST */
  /*      GL_COLOR_MATERIAL */
  /*      GL_NORMALIZE */
  /*      GL_AUTO_NORMAL */
  /*      GL_VERTEX_ARRAY */
  /*      GL_NORMAL_ARRAY */
  /*      GL_COLOR_ARRAY */
  /*      GL_INDEX_ARRAY */
  /*      GL_TEXTURE_COORD_ARRAY */
  /*      GL_EDGE_FLAG_ARRAY */
  /*      GL_POLYGON_OFFSET_POINT */
  /*      GL_POLYGON_OFFSET_LINE */
  /*      GL_POLYGON_OFFSET_FILL */
  #endregion

  #region ErrorCode
  public const uint GL_NO_ERROR                       =0;
  public const uint GL_INVALID_ENUM                   =0x0500;
  public const uint GL_INVALID_VALUE                  =0x0501;
  public const uint GL_INVALID_OPERATION              =0x0502;
  public const uint GL_STACK_OVERFLOW                 =0x0503;
  public const uint GL_STACK_UNDERFLOW                =0x0504;
  public const uint GL_OUT_OF_MEMORY                  =0x0505;
  #endregion

  #region FeedBackMode
  public const uint GL_2D                             =0x0600;
  public const uint GL_3D                             =0x0601;
  public const uint GL_3D_COLOR                       =0x0602;
  public const uint GL_3D_COLOR_TEXTURE               =0x0603;
  public const uint GL_4D_COLOR_TEXTURE               =0x0604;
  #endregion

  #region FeedBackToken
  public const uint GL_PASS_THROUGH_TOKEN             =0x0700;
  public const uint GL_POINT_TOKEN                    =0x0701;
  public const uint GL_LINE_TOKEN                     =0x0702;
  public const uint GL_POLYGON_TOKEN                  =0x0703;
  public const uint GL_BITMAP_TOKEN                   =0x0704;
  public const uint GL_DRAW_PIXEL_TOKEN               =0x0705;
  public const uint GL_COPY_PIXEL_TOKEN               =0x0706;
  public const uint GL_LINE_RESET_TOKEN               =0x0707;
  #endregion

  #region FogMode
  /*      GL_LINEAR */
  public const uint GL_EXP                            =0x0800;
  public const uint GL_EXP2                           =0x0801;
  #endregion

  #region FogParameter
  /*      GL_FOG_COLOR */
  /*      GL_FOG_DENSITY */
  /*      GL_FOG_END */
  /*      GL_FOG_INDEX */
  /*      GL_FOG_MODE */
  /*      GL_FOG_START */
  #endregion

  #region FrontFaceDirection
  public const uint GL_CW                             =0x0900;
  public const uint GL_CCW                            =0x0901;
  #endregion

  #region GetMapTarget
  public const uint GL_COEFF                          =0x0A00;
  public const uint GL_ORDER                          =0x0A01;
  public const uint GL_DOMAIN                         =0x0A02;
  #endregion

  #region GetPixelMap
  /*      GL_PIXEL_MAP_I_TO_I */
  /*      GL_PIXEL_MAP_S_TO_S */
  /*      GL_PIXEL_MAP_I_TO_R */
  /*      GL_PIXEL_MAP_I_TO_G */
  /*      GL_PIXEL_MAP_I_TO_B */
  /*      GL_PIXEL_MAP_I_TO_A */
  /*      GL_PIXEL_MAP_R_TO_R */
  /*      GL_PIXEL_MAP_G_TO_G */
  /*      GL_PIXEL_MAP_B_TO_B */
  /*      GL_PIXEL_MAP_A_TO_A */
  #endregion

  #region GetPointerTarget
  /*      GL_VERTEX_ARRAY_POINTER */
  /*      GL_NORMAL_ARRAY_POINTER */
  /*      GL_COLOR_ARRAY_POINTER */
  /*      GL_INDEX_ARRAY_POINTER */
  /*      GL_TEXTURE_COORD_ARRAY_POINTER */
  /*      GL_EDGE_FLAG_ARRAY_POINTER */
  #endregion

  #region GetTarget
  public const uint GL_CURRENT_COLOR                  =0x0B00;
  public const uint GL_CURRENT_INDEX                  =0x0B01;
  public const uint GL_CURRENT_NORMAL                 =0x0B02;
  public const uint GL_CURRENT_TEXTURE_COORDS         =0x0B03;
  public const uint GL_CURRENT_RASTER_COLOR           =0x0B04;
  public const uint GL_CURRENT_RASTER_INDEX           =0x0B05;
  public const uint GL_CURRENT_RASTER_TEXTURE_COORDS  =0x0B06;
  public const uint GL_CURRENT_RASTER_POSITION        =0x0B07;
  public const uint GL_CURRENT_RASTER_POSITION_VALID  =0x0B08;
  public const uint GL_CURRENT_RASTER_DISTANCE        =0x0B09;
  public const uint GL_POINT_SMOOTH                   =0x0B10;
  public const uint GL_POINT_SIZE                     =0x0B11;
  public const uint GL_POINT_SIZE_RANGE               =0x0B12;
  public const uint GL_POINT_SIZE_GRANULARITY         =0x0B13;
  public const uint GL_LINE_SMOOTH                    =0x0B20;
  public const uint GL_LINE_WIDTH                     =0x0B21;
  public const uint GL_LINE_WIDTH_RANGE               =0x0B22;
  public const uint GL_LINE_WIDTH_GRANULARITY         =0x0B23;
  public const uint GL_LINE_STIPPLE                   =0x0B24;
  public const uint GL_LINE_STIPPLE_PATTERN           =0x0B25;
  public const uint GL_LINE_STIPPLE_REPEAT            =0x0B26;
  public const uint GL_LIST_MODE                      =0x0B30;
  public const uint GL_MAX_LIST_NESTING               =0x0B31;
  public const uint GL_LIST_BASE                      =0x0B32;
  public const uint GL_LIST_INDEX                     =0x0B33;
  public const uint GL_POLYGON_MODE                   =0x0B40;
  public const uint GL_POLYGON_SMOOTH                 =0x0B41;
  public const uint GL_POLYGON_STIPPLE                =0x0B42;
  public const uint GL_EDGE_FLAG                      =0x0B43;
  public const uint GL_CULL_FACE                      =0x0B44;
  public const uint GL_CULL_FACE_MODE                 =0x0B45;
  public const uint GL_FRONT_FACE                     =0x0B46;
  public const uint GL_LIGHTING                       =0x0B50;
  public const uint GL_LIGHT_MODEL_LOCAL_VIEWER       =0x0B51;
  public const uint GL_LIGHT_MODEL_TWO_SIDE           =0x0B52;
  public const uint GL_LIGHT_MODEL_AMBIENT            =0x0B53;
  public const uint GL_SHADE_MODEL                    =0x0B54;
  public const uint GL_COLOR_MATERIAL_FACE            =0x0B55;
  public const uint GL_COLOR_MATERIAL_PARAMETER       =0x0B56;
  public const uint GL_COLOR_MATERIAL                 =0x0B57;
  public const uint GL_FOG                            =0x0B60;
  public const uint GL_FOG_INDEX                      =0x0B61;
  public const uint GL_FOG_DENSITY                    =0x0B62;
  public const uint GL_FOG_START                      =0x0B63;
  public const uint GL_FOG_END                        =0x0B64;
  public const uint GL_FOG_MODE                       =0x0B65;
  public const uint GL_FOG_COLOR                      =0x0B66;
  public const uint GL_DEPTH_RANGE                    =0x0B70;
  public const uint GL_DEPTH_TEST                     =0x0B71;
  public const uint GL_DEPTH_WRITEMASK                =0x0B72;
  public const uint GL_DEPTH_CLEAR_VALUE              =0x0B73;
  public const uint GL_DEPTH_FUNC                     =0x0B74;
  public const uint GL_ACCUM_CLEAR_VALUE              =0x0B80;
  public const uint GL_STENCIL_TEST                   =0x0B90;
  public const uint GL_STENCIL_CLEAR_VALUE            =0x0B91;
  public const uint GL_STENCIL_FUNC                   =0x0B92;
  public const uint GL_STENCIL_VALUE_MASK             =0x0B93;
  public const uint GL_STENCIL_FAIL                   =0x0B94;
  public const uint GL_STENCIL_PASS_DEPTH_FAIL        =0x0B95;
  public const uint GL_STENCIL_PASS_DEPTH_PASS        =0x0B96;
  public const uint GL_STENCIL_REF                    =0x0B97;
  public const uint GL_STENCIL_WRITEMASK              =0x0B98;
  public const uint GL_MATRIX_MODE                    =0x0BA0;
  public const uint GL_NORMALIZE                      =0x0BA1;
  public const uint GL_VIEWPORT                       =0x0BA2;
  public const uint GL_MODELVIEW_STACK_DEPTH          =0x0BA3;
  public const uint GL_PROJECTION_STACK_DEPTH         =0x0BA4;
  public const uint GL_TEXTURE_STACK_DEPTH            =0x0BA5;
  public const uint GL_MODELVIEW_MATRIX               =0x0BA6;
  public const uint GL_PROJECTION_MATRIX              =0x0BA7;
  public const uint GL_TEXTURE_MATRIX                 =0x0BA8;
  public const uint GL_ATTRIB_STACK_DEPTH             =0x0BB0;
  public const uint GL_CLIENT_ATTRIB_STACK_DEPTH      =0x0BB1;
  public const uint GL_ALPHA_TEST                     =0x0BC0;
  public const uint GL_ALPHA_TEST_FUNC                =0x0BC1;
  public const uint GL_ALPHA_TEST_REF                 =0x0BC2;
  public const uint GL_DITHER                         =0x0BD0;
  public const uint GL_BLEND_DST                      =0x0BE0;
  public const uint GL_BLEND_SRC                      =0x0BE1;
  public const uint GL_BLEND                          =0x0BE2;
  public const uint GL_LOGIC_OP_MODE                  =0x0BF0;
  public const uint GL_INDEX_LOGIC_OP                 =0x0BF1;
  public const uint GL_COLOR_LOGIC_OP                 =0x0BF2;
  public const uint GL_AUX_BUFFERS                    =0x0C00;
  public const uint GL_DRAW_BUFFER                    =0x0C01;
  public const uint GL_READ_BUFFER                    =0x0C02;
  public const uint GL_SCISSOR_BOX                    =0x0C10;
  public const uint GL_SCISSOR_TEST                   =0x0C11;
  public const uint GL_INDEX_CLEAR_VALUE              =0x0C20;
  public const uint GL_INDEX_WRITEMASK                =0x0C21;
  public const uint GL_COLOR_CLEAR_VALUE              =0x0C22;
  public const uint GL_COLOR_WRITEMASK                =0x0C23;
  public const uint GL_INDEX_MODE                     =0x0C30;
  public const uint GL_RGBA_MODE                      =0x0C31;
  public const uint GL_DOUBLEBUFFER                   =0x0C32;
  public const uint GL_STEREO                         =0x0C33;
  public const uint GL_RENDER_MODE                    =0x0C40;
  public const uint GL_PERSPECTIVE_CORRECTION_HINT    =0x0C50;
  public const uint GL_POINT_SMOOTH_HINT              =0x0C51;
  public const uint GL_LINE_SMOOTH_HINT               =0x0C52;
  public const uint GL_POLYGON_SMOOTH_HINT            =0x0C53;
  public const uint GL_FOG_HINT                       =0x0C54;
  public const uint GL_TEXTURE_GEN_S                  =0x0C60;
  public const uint GL_TEXTURE_GEN_T                  =0x0C61;
  public const uint GL_TEXTURE_GEN_R                  =0x0C62;
  public const uint GL_TEXTURE_GEN_Q                  =0x0C63;
  public const uint GL_PIXEL_MAP_I_TO_I               =0x0C70;
  public const uint GL_PIXEL_MAP_S_TO_S               =0x0C71;
  public const uint GL_PIXEL_MAP_I_TO_R               =0x0C72;
  public const uint GL_PIXEL_MAP_I_TO_G               =0x0C73;
  public const uint GL_PIXEL_MAP_I_TO_B               =0x0C74;
  public const uint GL_PIXEL_MAP_I_TO_A               =0x0C75;
  public const uint GL_PIXEL_MAP_R_TO_R               =0x0C76;
  public const uint GL_PIXEL_MAP_G_TO_G               =0x0C77;
  public const uint GL_PIXEL_MAP_B_TO_B               =0x0C78;
  public const uint GL_PIXEL_MAP_A_TO_A               =0x0C79;
  public const uint GL_PIXEL_MAP_I_TO_I_SIZE          =0x0CB0;
  public const uint GL_PIXEL_MAP_S_TO_S_SIZE          =0x0CB1;
  public const uint GL_PIXEL_MAP_I_TO_R_SIZE          =0x0CB2;
  public const uint GL_PIXEL_MAP_I_TO_G_SIZE          =0x0CB3;
  public const uint GL_PIXEL_MAP_I_TO_B_SIZE          =0x0CB4;
  public const uint GL_PIXEL_MAP_I_TO_A_SIZE          =0x0CB5;
  public const uint GL_PIXEL_MAP_R_TO_R_SIZE          =0x0CB6;
  public const uint GL_PIXEL_MAP_G_TO_G_SIZE          =0x0CB7;
  public const uint GL_PIXEL_MAP_B_TO_B_SIZE          =0x0CB8;
  public const uint GL_PIXEL_MAP_A_TO_A_SIZE          =0x0CB9;
  public const uint GL_UNPACK_SWAP_BYTES              =0x0CF0;
  public const uint GL_UNPACK_LSB_FIRST               =0x0CF1;
  public const uint GL_UNPACK_ROW_LENGTH              =0x0CF2;
  public const uint GL_UNPACK_SKIP_ROWS               =0x0CF3;
  public const uint GL_UNPACK_SKIP_PIXELS             =0x0CF4;
  public const uint GL_UNPACK_ALIGNMENT               =0x0CF5;
  public const uint GL_PACK_SWAP_BYTES                =0x0D00;
  public const uint GL_PACK_LSB_FIRST                 =0x0D01;
  public const uint GL_PACK_ROW_LENGTH                =0x0D02;
  public const uint GL_PACK_SKIP_ROWS                 =0x0D03;
  public const uint GL_PACK_SKIP_PIXELS               =0x0D04;
  public const uint GL_PACK_ALIGNMENT                 =0x0D05;
  public const uint GL_MAP_COLOR                      =0x0D10;
  public const uint GL_MAP_STENCIL                    =0x0D11;
  public const uint GL_INDEX_SHIFT                    =0x0D12;
  public const uint GL_INDEX_OFFSET                   =0x0D13;
  public const uint GL_RED_SCALE                      =0x0D14;
  public const uint GL_RED_BIAS                       =0x0D15;
  public const uint GL_ZOOM_X                         =0x0D16;
  public const uint GL_ZOOM_Y                         =0x0D17;
  public const uint GL_GREEN_SCALE                    =0x0D18;
  public const uint GL_GREEN_BIAS                     =0x0D19;
  public const uint GL_BLUE_SCALE                     =0x0D1A;
  public const uint GL_BLUE_BIAS                      =0x0D1B;
  public const uint GL_ALPHA_SCALE                    =0x0D1C;
  public const uint GL_ALPHA_BIAS                     =0x0D1D;
  public const uint GL_DEPTH_SCALE                    =0x0D1E;
  public const uint GL_DEPTH_BIAS                     =0x0D1F;
  public const uint GL_MAX_EVAL_ORDER                 =0x0D30;
  public const uint GL_MAX_LIGHTS                     =0x0D31;
  public const uint GL_MAX_CLIP_PLANES                =0x0D32;
  public const uint GL_MAX_TEXTURE_SIZE               =0x0D33;
  public const uint GL_MAX_PIXEL_MAP_TABLE            =0x0D34;
  public const uint GL_MAX_ATTRIB_STACK_DEPTH         =0x0D35;
  public const uint GL_MAX_MODELVIEW_STACK_DEPTH      =0x0D36;
  public const uint GL_MAX_NAME_STACK_DEPTH           =0x0D37;
  public const uint GL_MAX_PROJECTION_STACK_DEPTH     =0x0D38;
  public const uint GL_MAX_TEXTURE_STACK_DEPTH        =0x0D39;
  public const uint GL_MAX_VIEWPORT_DIMS              =0x0D3A;
  public const uint GL_MAX_CLIENT_ATTRIB_STACK_DEPTH  =0x0D3B;
  public const uint GL_SUBPIXEL_BITS                  =0x0D50;
  public const uint GL_INDEX_BITS                     =0x0D51;
  public const uint GL_RED_BITS                       =0x0D52;
  public const uint GL_GREEN_BITS                     =0x0D53;
  public const uint GL_BLUE_BITS                      =0x0D54;
  public const uint GL_ALPHA_BITS                     =0x0D55;
  public const uint GL_DEPTH_BITS                     =0x0D56;
  public const uint GL_STENCIL_BITS                   =0x0D57;
  public const uint GL_ACCUM_RED_BITS                 =0x0D58;
  public const uint GL_ACCUM_GREEN_BITS               =0x0D59;
  public const uint GL_ACCUM_BLUE_BITS                =0x0D5A;
  public const uint GL_ACCUM_ALPHA_BITS               =0x0D5B;
  public const uint GL_NAME_STACK_DEPTH               =0x0D70;
  public const uint GL_AUTO_NORMAL                    =0x0D80;
  public const uint GL_MAP1_COLOR_4                   =0x0D90;
  public const uint GL_MAP1_INDEX                     =0x0D91;
  public const uint GL_MAP1_NORMAL                    =0x0D92;
  public const uint GL_MAP1_TEXTURE_COORD_1           =0x0D93;
  public const uint GL_MAP1_TEXTURE_COORD_2           =0x0D94;
  public const uint GL_MAP1_TEXTURE_COORD_3           =0x0D95;
  public const uint GL_MAP1_TEXTURE_COORD_4           =0x0D96;
  public const uint GL_MAP1_VERTEX_3                  =0x0D97;
  public const uint GL_MAP1_VERTEX_4                  =0x0D98;
  public const uint GL_MAP2_COLOR_4                   =0x0DB0;
  public const uint GL_MAP2_INDEX                     =0x0DB1;
  public const uint GL_MAP2_NORMAL                    =0x0DB2;
  public const uint GL_MAP2_TEXTURE_COORD_1           =0x0DB3;
  public const uint GL_MAP2_TEXTURE_COORD_2           =0x0DB4;
  public const uint GL_MAP2_TEXTURE_COORD_3           =0x0DB5;
  public const uint GL_MAP2_TEXTURE_COORD_4           =0x0DB6;
  public const uint GL_MAP2_VERTEX_3                  =0x0DB7;
  public const uint GL_MAP2_VERTEX_4                  =0x0DB8;
  public const uint GL_MAP1_GRID_DOMAIN               =0x0DD0;
  public const uint GL_MAP1_GRID_SEGMENTS             =0x0DD1;
  public const uint GL_MAP2_GRID_DOMAIN               =0x0DD2;
  public const uint GL_MAP2_GRID_SEGMENTS             =0x0DD3;
  public const uint GL_TEXTURE_1D                     =0x0DE0;
  public const uint GL_TEXTURE_2D                     =0x0DE1;
  public const uint GL_FEEDBACK_BUFFER_POINTER        =0x0DF0;
  public const uint GL_FEEDBACK_BUFFER_SIZE           =0x0DF1;
  public const uint GL_FEEDBACK_BUFFER_TYPE           =0x0DF2;
  public const uint GL_SELECTION_BUFFER_POINTER       =0x0DF3;
  public const uint GL_SELECTION_BUFFER_SIZE          =0x0DF4;
  /*      GL_TEXTURE_BINDING_1D */
  /*      GL_TEXTURE_BINDING_2D */
  /*      GL_VERTEX_ARRAY */
  /*      GL_NORMAL_ARRAY */
  /*      GL_COLOR_ARRAY */
  /*      GL_INDEX_ARRAY */
  /*      GL_TEXTURE_COORD_ARRAY */
  /*      GL_EDGE_FLAG_ARRAY */
  /*      GL_VERTEX_ARRAY_SIZE */
  /*      GL_VERTEX_ARRAY_TYPE */
  /*      GL_VERTEX_ARRAY_STRIDE */
  /*      GL_NORMAL_ARRAY_TYPE */
  /*      GL_NORMAL_ARRAY_STRIDE */
  /*      GL_COLOR_ARRAY_SIZE */
  /*      GL_COLOR_ARRAY_TYPE */
  /*      GL_COLOR_ARRAY_STRIDE */
  /*      GL_INDEX_ARRAY_TYPE */
  /*      GL_INDEX_ARRAY_STRIDE */
  /*      GL_TEXTURE_COORD_ARRAY_SIZE */
  /*      GL_TEXTURE_COORD_ARRAY_TYPE */
  /*      GL_TEXTURE_COORD_ARRAY_STRIDE */
  /*      GL_EDGE_FLAG_ARRAY_STRIDE */
  /*      GL_POLYGON_OFFSET_FACTOR */
  /*      GL_POLYGON_OFFSET_UNITS */
  #endregion

  #region GetTextureParameter
  /*      GL_TEXTURE_MAG_FILTER */
  /*      GL_TEXTURE_MIN_FILTER */
  /*      GL_TEXTURE_WRAP_S */
  /*      GL_TEXTURE_WRAP_T */
  public const uint GL_TEXTURE_WIDTH                  =0x1000;
  public const uint GL_TEXTURE_HEIGHT                 =0x1001;
  public const uint GL_TEXTURE_INTERNAL_FORMAT        =0x1003;
  public const uint GL_TEXTURE_BORDER_COLOR           =0x1004;
  public const uint GL_TEXTURE_BORDER                 =0x1005;
  /*      GL_TEXTURE_RED_SIZE */
  /*      GL_TEXTURE_GREEN_SIZE */
  /*      GL_TEXTURE_BLUE_SIZE */
  /*      GL_TEXTURE_ALPHA_SIZE */
  /*      GL_TEXTURE_LUMINANCE_SIZE */
  /*      GL_TEXTURE_INTENSITY_SIZE */
  /*      GL_TEXTURE_PRIORITY */
  /*      GL_TEXTURE_RESIDENT */
  #endregion

  #region HintMode
  public const uint GL_DONT_CARE                      =0x1100;
  public const uint GL_FASTEST                        =0x1101;
  public const uint GL_NICEST                         =0x1102;
  #endregion

  #region HintTarget
  /*      GL_PERSPECTIVE_CORRECTION_HINT */
  /*      GL_POINT_SMOOTH_HINT */
  /*      GL_LINE_SMOOTH_HINT */
  /*      GL_POLYGON_SMOOTH_HINT */
  /*      GL_FOG_HINT */
  /*      GL_PHONG_HINT */
  #endregion

  #region IndexPointerType
  /*      GL_SHORT */
  /*      GL_INT */
  /*      GL_FLOAT */
  /*      GL_DOUBLE */
  #endregion

  #region LightModelParameter
  /*      GL_LIGHT_MODEL_AMBIENT */
  /*      GL_LIGHT_MODEL_LOCAL_VIEWER */
  /*      GL_LIGHT_MODEL_TWO_SIDE */
  #endregion

  #region LightName
  public const uint GL_LIGHT0                         =0x4000;
  public const uint GL_LIGHT1                         =0x4001;
  public const uint GL_LIGHT2                         =0x4002;
  public const uint GL_LIGHT3                         =0x4003;
  public const uint GL_LIGHT4                         =0x4004;
  public const uint GL_LIGHT5                         =0x4005;
  public const uint GL_LIGHT6                         =0x4006;
  public const uint GL_LIGHT7                         =0x4007;
  #endregion

  #region LightParameter
  public const uint GL_AMBIENT                        =0x1200;
  public const uint GL_DIFFUSE                        =0x1201;
  public const uint GL_SPECULAR                       =0x1202;
  public const uint GL_POSITION                       =0x1203;
  public const uint GL_SPOT_DIRECTION                 =0x1204;
  public const uint GL_SPOT_EXPONENT                  =0x1205;
  public const uint GL_SPOT_CUTOFF                    =0x1206;
  public const uint GL_CONSTANT_ATTENUATION           =0x1207;
  public const uint GL_LINEAR_ATTENUATION             =0x1208;
  public const uint GL_QUADRATIC_ATTENUATION          =0x1209;
  #endregion

  #region InterleavedArrays
  /*      GL_V2F */
  /*      GL_V3F */
  /*      GL_C4UB_V2F */
  /*      GL_C4UB_V3F */
  /*      GL_C3F_V3F */
  /*      GL_N3F_V3F */
  /*      GL_C4F_N3F_V3F */
  /*      GL_T2F_V3F */
  /*      GL_T4F_V4F */
  /*      GL_T2F_C4UB_V3F */
  /*      GL_T2F_C3F_V3F */
  /*      GL_T2F_N3F_V3F */
  /*      GL_T2F_C4F_N3F_V3F */
  /*      GL_T4F_C4F_N3F_V4F */
  #endregion

  #region ListMode
  public const uint GL_COMPILE                        =0x1300;
  public const uint GL_COMPILE_AND_EXECUTE            =0x1301;
  #endregion

  #region ListNameType
  /*      GL_BYTE */
  /*      GL_UNSIGNED_BYTE */
  /*      GL_SHORT */
  /*      GL_UNSIGNED_SHORT */
  /*      GL_INT */
  /*      GL_UNSIGNED_INT */
  /*      GL_FLOAT */
  /*      GL_2_BYTES */
  /*      GL_3_BYTES */
  /*      GL_4_BYTES */
  #endregion

  #region LogicOp
  public const uint GL_CLEAR                          =0x1500;
  public const uint GL_AND                            =0x1501;
  public const uint GL_AND_REVERSE                    =0x1502;
  public const uint GL_COPY                           =0x1503;
  public const uint GL_AND_INVERTED                   =0x1504;
  public const uint GL_NOOP                           =0x1505;
  public const uint GL_XOR                            =0x1506;
  public const uint GL_OR                             =0x1507;
  public const uint GL_NOR                            =0x1508;
  public const uint GL_EQUIV                          =0x1509;
  public const uint GL_INVERT                         =0x150A;
  public const uint GL_OR_REVERSE                     =0x150B;
  public const uint GL_COPY_INVERTED                  =0x150C;
  public const uint GL_OR_INVERTED                    =0x150D;
  public const uint GL_NAND                           =0x150E;
  public const uint GL_SET                            =0x150F;
  #endregion

  #region MapTarget
  /*      GL_MAP1_COLOR_4 */
  /*      GL_MAP1_INDEX */
  /*      GL_MAP1_NORMAL */
  /*      GL_MAP1_TEXTURE_COORD_1 */
  /*      GL_MAP1_TEXTURE_COORD_2 */
  /*      GL_MAP1_TEXTURE_COORD_3 */
  /*      GL_MAP1_TEXTURE_COORD_4 */
  /*      GL_MAP1_VERTEX_3 */
  /*      GL_MAP1_VERTEX_4 */
  /*      GL_MAP2_COLOR_4 */
  /*      GL_MAP2_INDEX */
  /*      GL_MAP2_NORMAL */
  /*      GL_MAP2_TEXTURE_COORD_1 */
  /*      GL_MAP2_TEXTURE_COORD_2 */
  /*      GL_MAP2_TEXTURE_COORD_3 */
  /*      GL_MAP2_TEXTURE_COORD_4 */
  /*      GL_MAP2_VERTEX_3 */
  /*      GL_MAP2_VERTEX_4 */
  #endregion

  #region MaterialFace
  /*      GL_FRONT */
  /*      GL_BACK */
  /*      GL_FRONT_AND_BACK */
  #endregion

  #region MaterialParameter
  public const uint GL_EMISSION                       =0x1600;
  public const uint GL_SHININESS                      =0x1601;
  public const uint GL_AMBIENT_AND_DIFFUSE            =0x1602;
  public const uint GL_COLOR_INDEXES                  =0x1603;
  /*      GL_AMBIENT */
  /*      GL_DIFFUSE */
  /*      GL_SPECULAR */
  #endregion

  #region MatrixMode
  public const uint GL_MODELVIEW                      =0x1700;
  public const uint GL_PROJECTION                     =0x1701;
  public const uint GL_TEXTURE                        =0x1702;
  #endregion

  #region MeshMode1
  /*      GL_POINT */
  /*      GL_LINE */
  #endregion

  #region MeshMode2
  /*      GL_POINT */
  /*      GL_LINE */
  /*      GL_FILL */
  #endregion

  #region NormalPointerType
  /*      GL_BYTE */
  /*      GL_SHORT */
  /*      GL_INT */
  /*      GL_FLOAT */
  /*      GL_DOUBLE */
  #endregion

  #region PixelCopyType
  public const uint GL_COLOR                          =0x1800;
  public const uint GL_DEPTH                          =0x1801;
  public const uint GL_STENCIL                        =0x1802;
  #endregion

  #region PixelFormat
  public const uint GL_COLOR_INDEX                    =0x1900;
  public const uint GL_STENCIL_INDEX                  =0x1901;
  public const uint GL_DEPTH_COMPONENT                =0x1902;
  public const uint GL_RED                            =0x1903;
  public const uint GL_GREEN                          =0x1904;
  public const uint GL_BLUE                           =0x1905;
  public const uint GL_ALPHA                          =0x1906;
  public const uint GL_RGB                            =0x1907;
  public const uint GL_RGBA                           =0x1908;
  public const uint GL_LUMINANCE                      =0x1909;
  public const uint GL_LUMINANCE_ALPHA                =0x190A;
  #endregion

  #region PixelMap
  /*      GL_PIXEL_MAP_I_TO_I */
  /*      GL_PIXEL_MAP_S_TO_S */
  /*      GL_PIXEL_MAP_I_TO_R */
  /*      GL_PIXEL_MAP_I_TO_G */
  /*      GL_PIXEL_MAP_I_TO_B */
  /*      GL_PIXEL_MAP_I_TO_A */
  /*      GL_PIXEL_MAP_R_TO_R */
  /*      GL_PIXEL_MAP_G_TO_G */
  /*      GL_PIXEL_MAP_B_TO_B */
  /*      GL_PIXEL_MAP_A_TO_A */
  #endregion

  #region PixelStore
  /*      GL_UNPACK_SWAP_BYTES */
  /*      GL_UNPACK_LSB_FIRST */
  /*      GL_UNPACK_ROW_LENGTH */
  /*      GL_UNPACK_SKIP_ROWS */
  /*      GL_UNPACK_SKIP_PIXELS */
  /*      GL_UNPACK_ALIGNMENT */
  /*      GL_PACK_SWAP_BYTES */
  /*      GL_PACK_LSB_FIRST */
  /*      GL_PACK_ROW_LENGTH */
  /*      GL_PACK_SKIP_ROWS */
  /*      GL_PACK_SKIP_PIXELS */
  /*      GL_PACK_ALIGNMENT */
  #endregion

  #region PixelTransfer
  /*      GL_MAP_COLOR */
  /*      GL_MAP_STENCIL */
  /*      GL_INDEX_SHIFT */
  /*      GL_INDEX_OFFSET */
  /*      GL_RED_SCALE */
  /*      GL_RED_BIAS */
  /*      GL_GREEN_SCALE */
  /*      GL_GREEN_BIAS */
  /*      GL_BLUE_SCALE */
  /*      GL_BLUE_BIAS */
  /*      GL_ALPHA_SCALE */
  /*      GL_ALPHA_BIAS */
  /*      GL_DEPTH_SCALE */
  /*      GL_DEPTH_BIAS */
  #endregion

  #region PixelType
  public const uint GL_BITMAP                         =0x1A00;
  /*      GL_BYTE */
  /*      GL_UNSIGNED_BYTE */
  /*      GL_SHORT */
  /*      GL_UNSIGNED_SHORT */
  /*      GL_INT */
  /*      GL_UNSIGNED_INT */
  /*      GL_FLOAT */
  #endregion

  #region PolygonMode
  public const uint GL_POINT                          =0x1B00;
  public const uint GL_LINE                           =0x1B01;
  public const uint GL_FILL                           =0x1B02;
  #endregion

  #region ReadBufferMode
  /*      GL_FRONT_LEFT */
  /*      GL_FRONT_RIGHT */
  /*      GL_BACK_LEFT */
  /*      GL_BACK_RIGHT */
  /*      GL_FRONT */
  /*      GL_BACK */
  /*      GL_LEFT */
  /*      GL_RIGHT */
  /*      GL_AUX0 */
  /*      GL_AUX1 */
  /*      GL_AUX2 */
  /*      GL_AUX3 */
  #endregion

  #region RenderingMode
  public const uint GL_RENDER                         =0x1C00;
  public const uint GL_FEEDBACK                       =0x1C01;
  public const uint GL_SELECT                         =0x1C02;
  #endregion

  #region ShadingModel
  public const uint GL_FLAT                           =0x1D00;
  public const uint GL_SMOOTH                         =0x1D01;
  #endregion

  #region StencilFunction
  /*      GL_NEVER */
  /*      GL_LESS */
  /*      GL_EQUAL */
  /*      GL_LEQUAL */
  /*      GL_GREATER */
  /*      GL_NOTEQUAL */
  /*      GL_GEQUAL */
  /*      GL_ALWAYS */
  #endregion

  #region StencilOp
  /*      GL_ZERO */
  public const uint GL_KEEP                           =0x1E00;
  public const uint GL_REPLACE                        =0x1E01;
  public const uint GL_INCR                           =0x1E02;
  public const uint GL_DECR                           =0x1E03;
  /*      GL_INVERT */
  #endregion

  #region StringName
  public const uint GL_VENDOR                         =0x1F00;
  public const uint GL_RENDERER                       =0x1F01;
  public const uint GL_VERSION                        =0x1F02;
  public const uint GL_EXTENSIONS                     =0x1F03;
  #endregion

  #region TextureCoordName
  public const uint GL_S                              =0x2000;
  public const uint GL_T                              =0x2001;
  public const uint GL_R                              =0x2002;
  public const uint GL_Q                              =0x2003;
  #endregion

  #region TexCoordPointerType
  /*      GL_SHORT */
  /*      GL_INT */
  /*      GL_FLOAT */
  /*      GL_DOUBLE */
  #endregion

  #region TextureEnvMode
  public const uint GL_MODULATE                       =0x2100;
  public const uint GL_DECAL                          =0x2101;
  /*      GL_BLEND */
  /*      GL_REPLACE */
  #endregion

  #region TextureEnvParameter
  public const uint GL_TEXTURE_ENV_MODE               =0x2200;
  public const uint GL_TEXTURE_ENV_COLOR              =0x2201;
  #endregion

  #region TextureEnvTarget
  public const uint GL_TEXTURE_ENV                    =0x2300;
  #endregion

  #region TextureGenMode
  public const uint GL_EYE_LINEAR                     =0x2400;
  public const uint GL_OBJECT_LINEAR                  =0x2401;
  public const uint GL_SPHERE_MAP                     =0x2402;
  #endregion

  #region TextureGenParameter
  public const uint GL_TEXTURE_GEN_MODE               =0x2500;
  public const uint GL_OBJECT_PLANE                   =0x2501;
  public const uint GL_EYE_PLANE                      =0x2502;
  #endregion

  #region TextureMagFilter
  public const uint GL_NEAREST                        =0x2600;
  public const uint GL_LINEAR                         =0x2601;
  #endregion

  #region TextureMinFilter
  /*      GL_NEAREST */
  /*      GL_LINEAR */
  public const uint GL_NEAREST_MIPMAP_NEAREST         =0x2700;
  public const uint GL_LINEAR_MIPMAP_NEAREST          =0x2701;
  public const uint GL_NEAREST_MIPMAP_LINEAR          =0x2702;
  public const uint GL_LINEAR_MIPMAP_LINEAR           =0x2703;
  #endregion

  #region TextureParameterName
  public const uint GL_TEXTURE_MAG_FILTER             =0x2800;
  public const uint GL_TEXTURE_MIN_FILTER             =0x2801;
  public const uint GL_TEXTURE_WRAP_S                 =0x2802;
  public const uint GL_TEXTURE_WRAP_T                 =0x2803;
  /*      GL_TEXTURE_BORDER_COLOR */
  /*      GL_TEXTURE_PRIORITY */
  #endregion

  #region TextureTarget
  /*      GL_TEXTURE_1D */
  /*      GL_TEXTURE_2D */
  /*      GL_PROXY_TEXTURE_1D */
  /*      GL_PROXY_TEXTURE_2D */
  #endregion

  #region TextureWrapMode
  public const uint GL_CLAMP  =0x2900;
  public const uint GL_REPEAT =0x2901;
  #endregion

  #region VertexPointerType
  /*      GL_SHORT */
  /*      GL_INT */
  /*      GL_FLOAT */
  /*      GL_DOUBLE */
  #endregion

  #region ClientAttribMask
  public const uint GL_CLIENT_PIXEL_STORE_BIT         =0x00000001;
  public const uint GL_CLIENT_VERTEX_ARRAY_BIT        =0x00000002;
  public const uint GL_CLIENT_ALL_ATTRIB_BITS         =0xffffffff;
  #endregion

  #region polygon_offset
  public const uint GL_POLYGON_OFFSET_FACTOR          =0x8038;
  public const uint GL_POLYGON_OFFSET_UNITS           =0x2A00;
  public const uint GL_POLYGON_OFFSET_POINT           =0x2A01;
  public const uint GL_POLYGON_OFFSET_LINE            =0x2A02;
  public const uint GL_POLYGON_OFFSET_FILL            =0x8037;
  #endregion

  #region texture
  public const uint GL_ALPHA4                         =0x803B;
  public const uint GL_ALPHA8                         =0x803C;
  public const uint GL_ALPHA12                        =0x803D;
  public const uint GL_ALPHA16                        =0x803E;
  public const uint GL_LUMINANCE4                     =0x803F;
  public const uint GL_LUMINANCE8                     =0x8040;
  public const uint GL_LUMINANCE12                    =0x8041;
  public const uint GL_LUMINANCE16                    =0x8042;
  public const uint GL_LUMINANCE4_ALPHA4              =0x8043;
  public const uint GL_LUMINANCE6_ALPHA2              =0x8044;
  public const uint GL_LUMINANCE8_ALPHA8              =0x8045;
  public const uint GL_LUMINANCE12_ALPHA4             =0x8046;
  public const uint GL_LUMINANCE12_ALPHA12            =0x8047;
  public const uint GL_LUMINANCE16_ALPHA16            =0x8048;
  public const uint GL_INTENSITY                      =0x8049;
  public const uint GL_INTENSITY4                     =0x804A;
  public const uint GL_INTENSITY8                     =0x804B;
  public const uint GL_INTENSITY12                    =0x804C;
  public const uint GL_INTENSITY16                    =0x804D;
  public const uint GL_R3_G3_B2                       =0x2A10;
  public const uint GL_RGB4                           =0x804F;
  public const uint GL_RGB5                           =0x8050;
  public const uint GL_RGB8                           =0x8051;
  public const uint GL_RGB10                          =0x8052;
  public const uint GL_RGB12                          =0x8053;
  public const uint GL_RGB16                          =0x8054;
  public const uint GL_RGBA2                          =0x8055;
  public const uint GL_RGBA4                          =0x8056;
  public const uint GL_RGB5_A1                        =0x8057;
  public const uint GL_RGBA8                          =0x8058;
  public const uint GL_RGB10_A2                       =0x8059;
  public const uint GL_RGBA12                         =0x805A;
  public const uint GL_RGBA16                         =0x805B;
  public const uint GL_TEXTURE_RED_SIZE               =0x805C;
  public const uint GL_TEXTURE_GREEN_SIZE             =0x805D;
  public const uint GL_TEXTURE_BLUE_SIZE              =0x805E;
  public const uint GL_TEXTURE_ALPHA_SIZE             =0x805F;
  public const uint GL_TEXTURE_LUMINANCE_SIZE         =0x8060;
  public const uint GL_TEXTURE_INTENSITY_SIZE         =0x8061;
  public const uint GL_PROXY_TEXTURE_1D               =0x8063;
  public const uint GL_PROXY_TEXTURE_2D               =0x8064;
  #endregion

  #region texture_object
  public const uint GL_TEXTURE_PRIORITY               =0x8066;
  public const uint GL_TEXTURE_RESIDENT               =0x8067;
  public const uint GL_TEXTURE_BINDING_1D             =0x8068;
  public const uint GL_TEXTURE_BINDING_2D             =0x8069;
  #endregion

  #region vertex_array
  public const uint GL_VERTEX_ARRAY                   =0x8074;
  public const uint GL_NORMAL_ARRAY                   =0x8075;
  public const uint GL_COLOR_ARRAY                    =0x8076;
  public const uint GL_INDEX_ARRAY                    =0x8077;
  public const uint GL_TEXTURE_COORD_ARRAY            =0x8078;
  public const uint GL_EDGE_FLAG_ARRAY                =0x8079;
  public const uint GL_VERTEX_ARRAY_SIZE              =0x807A;
  public const uint GL_VERTEX_ARRAY_TYPE              =0x807B;
  public const uint GL_VERTEX_ARRAY_STRIDE            =0x807C;
  public const uint GL_NORMAL_ARRAY_TYPE              =0x807E;
  public const uint GL_NORMAL_ARRAY_STRIDE            =0x807F;
  public const uint GL_COLOR_ARRAY_SIZE               =0x8081;
  public const uint GL_COLOR_ARRAY_TYPE               =0x8082;
  public const uint GL_COLOR_ARRAY_STRIDE             =0x8083;
  public const uint GL_INDEX_ARRAY_TYPE               =0x8085;
  public const uint GL_INDEX_ARRAY_STRIDE             =0x8086;
  public const uint GL_TEXTURE_COORD_ARRAY_SIZE       =0x8088;
  public const uint GL_TEXTURE_COORD_ARRAY_TYPE       =0x8089;
  public const uint GL_TEXTURE_COORD_ARRAY_STRIDE     =0x808A;
  public const uint GL_EDGE_FLAG_ARRAY_STRIDE         =0x808C;
  public const uint GL_VERTEX_ARRAY_POINTER           =0x808E;
  public const uint GL_NORMAL_ARRAY_POINTER           =0x808F;
  public const uint GL_COLOR_ARRAY_POINTER            =0x8090;
  public const uint GL_INDEX_ARRAY_POINTER            =0x8091;
  public const uint GL_TEXTURE_COORD_ARRAY_POINTER    =0x8092;
  public const uint GL_EDGE_FLAG_ARRAY_POINTER        =0x8093;
  public const uint GL_V2F                            =0x2A20;
  public const uint GL_V3F                            =0x2A21;
  public const uint GL_C4UB_V2F                       =0x2A22;
  public const uint GL_C4UB_V3F                       =0x2A23;
  public const uint GL_C3F_V3F                        =0x2A24;
  public const uint GL_N3F_V3F                        =0x2A25;
  public const uint GL_C4F_N3F_V3F                    =0x2A26;
  public const uint GL_T2F_V3F                        =0x2A27;
  public const uint GL_T4F_V4F                        =0x2A28;
  public const uint GL_T2F_C4UB_V3F                   =0x2A29;
  public const uint GL_T2F_C3F_V3F                    =0x2A2A;
  public const uint GL_T2F_N3F_V3F                    =0x2A2B;
  public const uint GL_T2F_C4F_N3F_V3F                =0x2A2C;
  public const uint GL_T4F_C4F_N3F_V4F                =0x2A2D;
  #endregion

  #region Extensions
  public const uint GL_EXT_vertex_array               =1;
  public const uint GL_EXT_bgra                       =1;
  public const uint GL_EXT_paletted_texture           =1;
  public const uint GL_WIN_swap_hint                  =1;
  public const uint GL_WIN_draw_range_elements        =1;
  // public const uint GL_WIN_phong_shading              1
  // public const uint GL_WIN_specular_fog               1
  #endregion

  #region EXT_vertex_array
  public const uint GL_VERTEX_ARRAY_EXT               =0x8074;
  public const uint GL_NORMAL_ARRAY_EXT               =0x8075;
  public const uint GL_COLOR_ARRAY_EXT                =0x8076;
  public const uint GL_INDEX_ARRAY_EXT                =0x8077;
  public const uint GL_TEXTURE_COORD_ARRAY_EXT        =0x8078;
  public const uint GL_EDGE_FLAG_ARRAY_EXT            =0x8079;
  public const uint GL_VERTEX_ARRAY_SIZE_EXT          =0x807A;
  public const uint GL_VERTEX_ARRAY_TYPE_EXT          =0x807B;
  public const uint GL_VERTEX_ARRAY_STRIDE_EXT        =0x807C;
  public const uint GL_VERTEX_ARRAY_COUNT_EXT         =0x807D;
  public const uint GL_NORMAL_ARRAY_TYPE_EXT          =0x807E;
  public const uint GL_NORMAL_ARRAY_STRIDE_EXT        =0x807F;
  public const uint GL_NORMAL_ARRAY_COUNT_EXT         =0x8080;
  public const uint GL_COLOR_ARRAY_SIZE_EXT           =0x8081;
  public const uint GL_COLOR_ARRAY_TYPE_EXT           =0x8082;
  public const uint GL_COLOR_ARRAY_STRIDE_EXT         =0x8083;
  public const uint GL_COLOR_ARRAY_COUNT_EXT          =0x8084;
  public const uint GL_INDEX_ARRAY_TYPE_EXT           =0x8085;
  public const uint GL_INDEX_ARRAY_STRIDE_EXT         =0x8086;
  public const uint GL_INDEX_ARRAY_COUNT_EXT          =0x8087;
  public const uint GL_TEXTURE_COORD_ARRAY_SIZE_EXT   =0x8088;
  public const uint GL_TEXTURE_COORD_ARRAY_TYPE_EXT   =0x8089;
  public const uint GL_TEXTURE_COORD_ARRAY_STRIDE_EXT =0x808A;
  public const uint GL_TEXTURE_COORD_ARRAY_COUNT_EXT  =0x808B;
  public const uint GL_EDGE_FLAG_ARRAY_STRIDE_EXT     =0x808C;
  public const uint GL_EDGE_FLAG_ARRAY_COUNT_EXT      =0x808D;
  public const uint GL_VERTEX_ARRAY_POINTER_EXT       =0x808E;
  public const uint GL_NORMAL_ARRAY_POINTER_EXT       =0x808F;
  public const uint GL_COLOR_ARRAY_POINTER_EXT        =0x8090;
  public const uint GL_INDEX_ARRAY_POINTER_EXT        =0x8091;
  public const uint GL_TEXTURE_COORD_ARRAY_POINTER_EXT =0x8092;
  public const uint GL_EDGE_FLAG_ARRAY_POINTER_EXT    =0x8093;
  public const uint GL_DOUBLE_EXT                     =GL_DOUBLE;
  #endregion

  #region EXT_bgra
  public const uint GL_BGR_EXT                        =0x80E0;
  public const uint GL_BGRA_EXT                       =0x80E1;
  #endregion

  #region EXT_paletted_texture
  public const uint GL_COLOR_TABLE_FORMAT_EXT         =0x80D8;
  public const uint GL_COLOR_TABLE_WIDTH_EXT          =0x80D9;
  public const uint GL_COLOR_TABLE_RED_SIZE_EXT       =0x80DA;
  public const uint GL_COLOR_TABLE_GREEN_SIZE_EXT     =0x80DB;
  public const uint GL_COLOR_TABLE_BLUE_SIZE_EXT      =0x80DC;
  public const uint GL_COLOR_TABLE_ALPHA_SIZE_EXT     =0x80DD;
  public const uint GL_COLOR_TABLE_LUMINANCE_SIZE_EXT =0x80DE;
  public const uint GL_COLOR_TABLE_INTENSITY_SIZE_EXT =0x80DF;

  public const uint GL_COLOR_INDEX1_EXT               =0x80E2;
  public const uint GL_COLOR_INDEX2_EXT               =0x80E3;
  public const uint GL_COLOR_INDEX4_EXT               =0x80E4;
  public const uint GL_COLOR_INDEX8_EXT               =0x80E5;
  public const uint GL_COLOR_INDEX12_EXT              =0x80E6;
  public const uint GL_COLOR_INDEX16_EXT              =0x80E7;
  #endregion

  #region WIN_draw_range_elements
  public const uint GL_MAX_ELEMENTS_VERTICES_WIN      =0x80E8;
  public const uint GL_MAX_ELEMENTS_INDICES_WIN       =0x80E9;
  #endregion

  #region WIN_phong_shading
  public const uint GL_PHONG_WIN                      =0x80EA;
  public const uint GL_PHONG_HINT_WIN                 =0x80EB;
  #endregion

  #region WIN_specular_fog
  public const uint GL_FOG_SPECULAR_TEXTURE_WIN       =0x80EC;
  #endregion

  #region For compatibility with OpenGL v1.0
  public const uint GL_LOGIC_OP =GL_INDEX_LOGIC_OP;
  public const uint GL_TEXTURE_COMPONENTS =GL_TEXTURE_INTERNAL_FORMAT;
  #endregion
  #endregion

  #region Imports
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glAccum(uint op, float value);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glAlphaFunc(uint func, float refval);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern byte glAreTexturesResident(int n, /*const*/ uint *textures, byte *residences);
  public unsafe static byte glAreTexturesResident(uint[] textures, byte[] residences)
  { fixed(uint* t=textures) fixed(byte* r=residences) return glAreTexturesResident(textures.Length, t, r);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glArrayElement(int i);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glBegin(uint mode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glBindTexture(uint target, uint texture);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glBitmap(int width, int height, float xorig, float yorig, float xmove, float ymove, /*const*/ byte *bitmap);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glBlendFunc(uint sfactor, uint dfactor);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glCallList(uint list);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glCallLists(int n, uint type, /*const*/ void* lists);
  public unsafe static void glCallLists(int n, sbyte *lists)  { glCallLists(n, GL_BYTE, lists); }
  public unsafe static void glCallLists(int n, byte *lists)   { glCallLists(n, GL_UNSIGNED_BYTE, lists); }
  public unsafe static void glCallLists(int n, short *lists)  { glCallLists(n, GL_SHORT, lists); }
  public unsafe static void glCallLists(int n, ushort *lists) { glCallLists(n, GL_UNSIGNED_SHORT, lists); }
  public unsafe static void glCallLists(int n, int *lists)    { glCallLists(n, GL_INT, lists); }
  public unsafe static void glCallLists(int n, uint *lists)   { glCallLists(n, GL_UNSIGNED_INT, lists); }
  public unsafe static void glCallLists(int n, float *lists)   { glCallLists(n, GL_FLOAT, lists); }
  public unsafe static void glCallLists(sbyte[] lists)  { fixed(sbyte* p=lists) glCallLists(lists.Length, GL_BYTE, p); }
  public unsafe static void glCallLists(byte[] lists)   { fixed(byte* p=lists) glCallLists(lists.Length, GL_UNSIGNED_BYTE, p); }
  public unsafe static void glCallLists(short[] lists)  { fixed(short* p=lists) glCallLists(lists.Length, GL_SHORT, p); }
  public unsafe static void glCallLists(ushort[] lists) { fixed(ushort* p=lists) glCallLists(lists.Length, GL_UNSIGNED_SHORT, p); }
  public unsafe static void glCallLists(int[] lists)    { fixed(int* p=lists) glCallLists(lists.Length, GL_INT, p); }
  public unsafe static void glCallLists(uint[] lists)   { fixed(uint* p=lists) glCallLists(lists.Length, GL_UNSIGNED_INT, p); }
  public unsafe static void glCallLists(float[] lists)  { fixed(float* p=lists) glCallLists(lists.Length, GL_FLOAT, p); }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glClear(uint mask);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glClearAccum(float red, float green, float blue, float alpha);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glClearColor(float red, float green, float blue, float alpha);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glClearDepth(double depth);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glClearIndex(float c);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glClearStencil(int s);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glClipPlane(uint plane, /*const*/ double *equation);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glClipPlane(uint plane, /*const*/ double[] equation);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3b(sbyte red, sbyte green, sbyte blue);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor3bv(/*const*/ sbyte *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3bv([In] sbyte[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3d(double red, double green, double blue);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor3dv(/*const*/ double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3dv([In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3f(float red, float green, float blue);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor3fv(/*const*/ float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3fv([In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3i(int red, int green, int blue);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor3iv(/*const*/ int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3iv([In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3s(short red, short green, short blue);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor3sv(/*const*/ short *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3sv([In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3ub(byte red, byte green, byte blue);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor3ubv(/*const*/ byte *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3ubv([In] byte[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3ui(uint red, uint green, uint blue);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor3uiv(/*const*/ uint *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3uiv([In] uint[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3us(ushort red, ushort green, ushort blue);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor3usv(/*const*/ ushort *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor3usv([In] ushort[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4b(sbyte red, sbyte green, sbyte blue, sbyte alpha);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor4bv(/*const*/ sbyte *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4bv([In] sbyte[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4d(double red, double green, double blue, double alpha);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor4dv(/*const*/ double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4dv([In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4f(float red, float green, float blue, float alpha);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor4fv(/*const*/ float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4fv([In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4i(int red, int green, int blue, int alpha);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor4iv(/*const*/ int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4iv([In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4s(short red, short green, short blue, short alpha);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor4sv(/*const*/ short *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4sv([In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4ub(byte red, byte green, byte blue, byte alpha);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor4ubv(/*const*/ byte *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4ubv([In] byte[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4ui(uint red, uint green, uint blue, uint alpha);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor4uiv(/*const*/ uint *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4uiv([In] uint[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4us(ushort red, ushort green, ushort blue, ushort alpha);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColor4usv(/*const*/ ushort *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColor4usv([In] ushort[] v);
  public static void glColor(System.Drawing.Color c) { glColor4ub(c.R, c.G, c.B, c.A); }
  public static void glColor(byte alpha, System.Drawing.Color c) { glColor4ub(c.R, c.G, c.B, alpha); }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColorMask(byte red, byte green, byte blue, byte alpha);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glColorMaterial(uint face, uint mode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColorPointer(int size, uint type, int stride, /*const*/ void* pointer);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glCopyPixels(int x, int y, int width, int height, uint type);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glCopyTexImage1D(uint target, int level, uint internalformat, int x, int y, int width, int border);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glCopyTexImage2D(uint target, int level, uint internalformat, int x, int y, int width, int height, int border);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glCopyTexSubImage1D(uint target, int level, int xoffset, int x, int y, int width);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glCopyTexSubImage2D(uint target, int level, int xoffset, int yoffset, int x, int y, int width, int height);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glCullFace(uint mode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glDeleteLists(uint list, int range);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glDeleteTextures(int n, /*const*/ uint *textures);
  public unsafe static void glDeleteTextures([In] uint[] textures)
  { fixed(uint* p=textures) glDeleteTextures(textures.Length, p);
  }
  public unsafe static void glDeleteTexture(uint texture) { glDeleteTextures(1, &texture); }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glDepthFunc(uint func);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glDepthMask(byte flag);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glDepthRange(double zNear, double zFar);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glDisable(uint cap);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glDisableClientState(uint array);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glDrawArrays(uint mode, int first, int count);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glDrawBuffer(uint mode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glDrawElements(uint mode, int count, uint type, /*const*/ void* indices);
  public unsafe static void glDrawElements(uint mode, int count, /*const*/ byte *indices)
  { glDrawElements(mode, count, GL_UNSIGNED_BYTE, indices);
  }
  public unsafe static void glDrawElements(uint mode, int count, /*const*/ ushort *indices)
  { glDrawElements(mode, count, GL_UNSIGNED_SHORT, indices);
  }
  public unsafe static void glDrawElements(uint mode, int count, /*const*/ uint *indices)
  { glDrawElements(mode, count, GL_UNSIGNED_INT, indices);
  }
  public unsafe static void glDrawElements(uint mode, byte[] indices)
  { fixed(byte* p=indices) glDrawElements(mode, indices.Length, GL_UNSIGNED_BYTE, p);
  }
  public unsafe static void glDrawElements(uint mode, ushort[] indices)
  { fixed(ushort* p=indices) glDrawElements(mode, indices.Length, GL_UNSIGNED_SHORT, p);
  }
  public unsafe static void glDrawElements(uint mode, uint[] indices)
  { fixed(uint* p=indices) glDrawElements(mode, indices.Length, GL_UNSIGNED_INT, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glDrawPixels(int width, int height, uint format, uint type, /*const*/ void* pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glDrawPixels(int width, int height, uint format, uint type, /*const*/ IntPtr pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glDrawPixels(int width, int height, uint format, uint type, /*const*/ byte[] pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEdgeFlag(byte flag);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glEdgeFlagPointer(int stride, /*const*/ byte* pointer);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEdgeFlagPointer(int stride, [In] byte[] pointer);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glEdgeFlagv(/*const*/ byte *flag);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEnable(uint cap);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEnableClientState(uint array);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEnd();
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEndList();
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEvalCoord1d(double u);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glEvalCoord1dv(/*const*/ double *u);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEvalCoord1f(float u);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glEvalCoord1fv(/*const*/ float *u);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEvalCoord1fv([In] ref float u);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEvalCoord2d(double u, double v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glEvalCoord2dv(/*const*/ double *u);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEvalCoord2dv([In] double[] u);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEvalCoord2f(float u, float v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glEvalCoord2fv(/*const*/ float *u);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEvalCoord2fv([In] float[] u);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEvalMesh1(uint mode, int i1, int i2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEvalMesh2(uint mode, int i1, int i2, int j1, int j2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEvalPoint1(int i);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glEvalPoint2(int i, int j);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glFeedbackBuffer(int size, uint type, float *buffer);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glFinish();
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glFlush();
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glFogf(uint pname, float param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glFogfv(uint pname, /*const*/ float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glFogfv(uint pname, [In] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glFogi(uint pname, int param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glFogiv(uint pname, /*const*/ int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glFogiv(uint pname, [In] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glFrontFace(uint mode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glFrustum(double left, double right, double bottom, double top, double zNear, double zFar);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern uint glGenLists(int range);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGenTextures(int n, uint *textures);
  public unsafe static void glGenTextures(uint[] textures)
  { fixed(uint* p=textures) glGenTextures(textures.Length, p);
  }
  public unsafe static void glGenTextures(uint[] textures, int length)
  { fixed(uint* p=textures) glGenTextures(length, p);
  }
  public unsafe static void glGenTextures(uint[] textures, int index, int length)
  { fixed(uint* p=textures) glGenTextures(length, p+index);
  }
  public unsafe static void glGenTexture(out uint texture) { fixed(uint* tp=&texture) glGenTextures(1, tp); }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetBooleanv(uint pname, byte* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetBooleanv(uint pname, [In] [Out] byte[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetBooleanv(uint pname, out byte parm);
  public static byte glGetBooleanv(uint pname) { unsafe { byte v; glGetBooleanv(pname, &v); return v; } }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetClipPlane(uint plane, double *equation);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetClipPlane(uint plane, [In] [Out] double[] equation);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetDoublev(uint pname, double* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetDoublev(uint pname, [In] [Out] double[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetDoublev(uint pname, out double parm);
  public static double glGetDoublev(uint pname) { unsafe { double v; glGetDoublev(pname, &v); return v; } }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern uint glGetError();
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetFloatv(uint pname, float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetFloatv(uint pname, [In] [Out] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetFloatv(uint pname, out float parm);
  public static float glGetFloatv(uint pname) { unsafe { float v; glGetFloatv(pname, &v); return v; } }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetIntegerv(uint pname, int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetIntegerv(uint pname, [In] [Out] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetIntegerv(uint pname, out int parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetIntegerv(uint pname, uint* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetIntegerv(uint pname, [In] [Out] uint[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetIntegerv(uint pname, out uint parm);
  public static int glGetIntegerv(uint pname) { unsafe { int v; glGetIntegerv(pname, &v); return v; } }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetLightfv(uint light, uint pname, float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetLightfv(uint light, uint pname, [In] [Out] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetLightfv(uint light, uint pname, out float parm);
  public static float glGetLightfv(uint light, uint pname) { unsafe { float v; glGetLightfv(light, pname, &v); return v; } }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetLightiv(uint light, uint pname, int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetLightiv(uint light, uint pname, [In] [Out] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetLightiv(uint light, uint pname, out int parm);
  public static int glGetLightiv(uint light, uint pname)
  { unsafe { int v; glGetLightiv(light, pname, &v); return v; }
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMapdv(uint target, uint query, double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetMapdv(uint target, uint query, [In] [Out] double[] v);
  public static double glGetMapdv(uint target, uint query)
  { unsafe { double v; glGetMapdv(target, query, &v); return v; }
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMapfv(uint target, uint query, float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetMapfv(uint target, uint query, [In] [Out] float[] v);
  public static float glGetMapfv(uint target, uint query, uint pname)
  { unsafe { float v; glGetMapfv(target, query, &v); return v; }
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMapiv(uint target, uint query, int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetMapiv(uint target, uint query, [In] [Out] int[] v);
  public static int glGetMapiv(uint target, uint query)
  { unsafe { int v; glGetMapiv(target, query, &v); return v; }
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMaterialfv(uint face, uint pname, float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetMaterialfv(uint face, uint pname, [In] [Out] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetMaterialfv(uint face, uint pname, out float parm);
  public static float glGetMaterialfv(uint face, uint pname)
  { unsafe { float v; glGetMaterialfv(face, pname, &v); return v; } }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMaterialiv(uint face, uint pname, int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetMaterialiv(uint face, uint pname, [In] [Out] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetMaterialiv(uint face, uint pname, out int parm);
  public static int glGetMaterialiv(uint face, uint pname)
  { unsafe { int v; glGetMaterialiv(face, pname, &v); return v; }
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetPixelMapfv(uint map, float *values);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetPixelMapfv(uint map, [In] [Out] float[] values);
  public static float glGetPixelMapfv(uint map) { unsafe { float v; glGetPixelMapfv(map, &v); return v; } }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetPixelMapuiv(uint map, uint *values);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetPixelMapuiv(uint map, [In] [Out] uint[] values);
  public static uint glGetPixelMapuiv(uint map) { unsafe { uint v; glGetPixelMapuiv(map, &v); return v; } }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetPixelMapusv(uint map, ushort *values);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetPixelMapusv(uint map, [In] [Out] ushort[] values);
  public static uint glGetPixelMapusv(uint map) { unsafe { ushort v; glGetPixelMapusv(map, &v); return v; } }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetPointerv(uint pname, void** parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetPointerv(uint pname, [In] [Out] void*[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetPointerv(uint pname, out void* parms);
  public unsafe static void* glGetPointerv(uint pname) { void* v; glGetPointerv(pname, &v); return v; }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetPolygonStipple(byte *mask);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetPolygonStipple([In] [Out] byte[] mask);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern string glGetString(uint name);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetTexEnvfv(uint target, uint pname, float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexEnvfv(uint target, uint pname, [In] [Out] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetTexEnvfv(uint target, uint pname, out float parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetTexEnviv(uint target, uint pname, int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexEnviv(uint target, uint pname, [In] [Out] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexEnviv(uint target, uint pname, out int parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetTexGendv(uint coord, uint pname, double* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexGendv(uint coord, uint pname, [In] [Out] double[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexGendv(uint coord, uint pname, out double parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetTexGenfv(uint coord, uint pname, float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexGenfv(uint coord, uint pname, [In] [Out] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexGenfv(uint coord, uint pname, out float parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetTexGeniv(uint coord, uint pname, int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexGeniv(uint coord, uint pname, [In] [Out] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexGeniv(uint coord, uint pname, out int parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetTexImage(uint target, int level, uint format, uint type, void* pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetTexImage(uint target, int level, uint format, uint type, IntPtr pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetTexImage(uint target, int level, uint format, uint type, [In] [Out] byte[] pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetTexLevelParameterfv(uint target, int level, uint pname, float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexLevelParameterfv(uint target, int level, uint pname, [In] [Out] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexLevelParameterfv(uint target, int level, uint pname, out float parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetTexLevelParameteriv(uint target, int level, uint pname, int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexLevelParameteriv(uint target, int level, uint pname, [In] [Out] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexLevelParameteriv(uint target, int level, uint pname, out int parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetTexParameterfv(uint target, uint pname, float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexParameterfv(uint target, uint pname, [In] [Out] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexParameterfv(uint target, uint pname, out float parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetTexParameteriv(uint target, uint pname, int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexParameteriv(uint target, uint pname, [In] [Out] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glGetTexParameteriv(uint target, uint pname, out int parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glHint(uint target, uint mode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glIndexMask(uint mask);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glIndexPointer(uint type, int stride, /*const*/ void* pointer);
  public unsafe static void glIndexPointer(int stride, /*const*/ short* pointer)
  { glIndexPointer(GL_SHORT, stride, pointer);
  }
  public unsafe static void glIndexPointer(int stride, /*const*/ int* pointer)
  { glIndexPointer(GL_INT, stride, pointer);
  }
  public unsafe static void glIndexPointer(int stride, /*const*/ float* pointer)
  { glIndexPointer(GL_FLOAT, stride, pointer);
  }
  public unsafe static void glIndexPointer(int stride, /*const*/ double* pointer)
  { glIndexPointer(GL_DOUBLE, stride, pointer);
  }
  public unsafe static void glIndexPointer(int stride, /*const*/ short[] pointer)
  { fixed(short* p=pointer) glIndexPointer(GL_SHORT, stride, p);
  }
  public unsafe static void glIndexPointer(int stride, /*const*/ int[] pointer)
  { fixed(int* p=pointer) glIndexPointer(GL_INT, stride, p);
  }
  public unsafe static void glIndexPointer(int stride, /*const*/ float[] pointer)
  { fixed(float* p=pointer) glIndexPointer(GL_FLOAT, stride, p);
  }
  public unsafe static void glIndexPointer(int stride, /*const*/ double[] pointer)
  { fixed(double* p=pointer) glIndexPointer(GL_DOUBLE, stride, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glIndexd(double c);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glIndexdv(/*const*/ double *c);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glIndexf(float c);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glIndexfv(/*const*/ float *c);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glIndexi(int c);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glIndexiv(/*const*/ int *c);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glIndexs(short c);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glIndexsv(/*const*/ short *c);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glIndexub(byte c);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glIndexubv(/*const*/ byte *c);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glInitNames();
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void interleavedArrays(uint format, int stride, /*const*/ void* pointer);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern byte glIsEnabled(uint cap);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern byte glIsList(uint list);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern byte glIsTexture(uint texture);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLightModelf(uint pname, float param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glLightModelfv(uint pname, /*const*/ float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLightModelfv(uint pname, [In] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLightModeli(uint pname, int param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLightModeli(uint pname, uint param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glLightModeliv(uint pname, /*const*/ int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLightModeliv(uint pname, int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLightf(uint light, uint pname, float param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glLightfv(uint light, uint pname, /*const*/ float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLightfv(uint light, uint pname, [In] float[] parms);
  public unsafe static void glLightPosition(uint light, float x, float y, float z, float w)
  { float* p = stackalloc float[4];
    p[0] = x;
    p[1] = y;
    p[2] = z;
    p[3] = w;
    glLightfv(light, GL_POSITION, p);
  }
  public unsafe static void glLightPosition(uint light, Mathematics.ThreeD.Point pt, float w)
  { float* p = stackalloc float[4];
    p[0] = (float)pt.X;
    p[1] = (float)pt.Y;
    p[2] = (float)pt.Z;
    p[3] = w;
    glLightfv(light, GL_POSITION, p);
  }
  public unsafe static void glLightColor(uint light, uint pname, System.Drawing.Color c)
  { float* p = stackalloc float[4];
    p[0] = c.R/255f;
    p[1] = c.G/255f;
    p[2] = c.B/255f;
    p[3] = c.A/255f;
    glLightfv(light, pname, p);
  }
  public unsafe static void glLightColor(uint light, uint pname, float r, float g, float b, float a)
  { float* p = stackalloc float[4];
    p[0] = r;
    p[1] = g;
    p[2] = b;
    p[3] = a;
    glLightfv(light, pname, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLighti(uint light, uint pname, int param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glLightiv(uint light, uint pname, /*const*/ int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLightiv(uint light, uint pname, [In] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLineStipple(int factor, ushort pattern);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLineWidth(float width);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glListBase(uint offset);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLoadIdentity();
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glLoadMatrixd(/*const*/ double *m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLoadMatrixd([In] double[] m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glLoadMatrixf(/*const*/ float *m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLoadMatrixf([In] float[] m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLoadName(uint name);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glLogicOp(uint opcode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glMap1d(uint target, double u1, double u2, int stride, int order, /*const*/ double* points);
  public unsafe static void glMap1d(uint target, double u1, double u2, int stride, double[] points)
  { fixed(double* p=points) glMap1d(target, u1, u2, stride, points.Length, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glMap1f(uint target, float u1, float u2, int stride, int order, /*const*/ float* points);
  public unsafe static void glMap1f(uint target, float u1, float u2, int stride, float[] points)
  { fixed(float* p=points) glMap1f(target, u1, u2, stride, points.Length, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glMap2d(uint target, double u1, double u2, int ustride, int uorder, double v1, double v2, int vstride, int vorder, /*const*/ double* points);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMap2d(uint target, double u1, double u2, int ustride, int uorder, double v1, double v2, int vstride, int vorder, double[] points);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glMap2f(uint target, float u1, float u2, int ustride, int uorder, float v1, float v2, int vstride, int vorder, /*const*/ float* points);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMap2f(uint target, float u1, float u2, int ustride, int uorder, float v1, float v2, int vstride, int vorder, float[] points);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMapGrid1d(int un, double u1, double u2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMapGrid1f(int un, float u1, float u2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMapGrid2d(int un, double u1, double u2, int vn, double v1, double v2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMapGrid2f(int un, float u1, float u2, int vn, float v1, float v2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMaterialf(uint face, uint pname, float param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glMaterialfv(uint face, uint pname, /*const*/ float* parms);
  public unsafe static void glMaterialColor(uint face, uint pname, System.Drawing.Color c)
  { float* p = stackalloc float[4];
    p[0] = c.R/255f;
    p[1] = c.G/255f;
    p[2] = c.B/255f;
    p[3] = c.A/255f;
    glMaterialfv(face, pname, p);
  }
  public unsafe static void glMaterialColor(uint face, uint pname, float r, float g, float b, float a)
  { float* p = stackalloc float[4];
    p[0] = r;
    p[1] = g;
    p[2] = b;
    p[3] = a;
    glMaterialfv(face, pname, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMaterialfv(uint face, uint pname, [In] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMateriali(uint face, uint pname, int param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glMaterialiv(uint face, uint pname, /*const*/ int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMaterialiv(uint face, uint pname, [In] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMatrixMode(uint mode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glMultMatrixd(/*const*/ double *m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMultMatrixd([In] double[] m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glMultMatrixf(/*const*/ float *m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMultMatrixf([In] float[] m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glNewList(uint list, uint mode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glNormal3b(sbyte nx, sbyte ny, sbyte nz);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glNormal3bv(/*const*/ sbyte *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glNormal3bv([In] sbyte[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glNormal3d(double nx, double ny, double nz);
  public static void glNormal3d(Mathematics.ThreeD.Vector v) { glNormal3d(v.X, v.Y, v.Z); }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glNormal3dv(/*const*/ double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glNormal3dv([In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glNormal3f(float nx, float ny, float nz);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glNormal3fv(/*const*/ float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glNormal3fv([In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glNormal3i(int nx, int ny, int nz);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glNormal3iv(/*const*/ int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glNormal3iv([In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glNormal3s(short nx, short ny, short nz);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glNormal3sv(/*const*/ short *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glNormal3sv([In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glNormalPointer(uint type, int stride, /*const*/ void* pointer);
  public unsafe static void glNormalPointer(int stride, /*const*/ byte* pointer)
  { glNormalPointer(GL_BYTE, stride, pointer);
  }
  public unsafe static void glNormalPointer(int stride, /*const*/ short* pointer)
  { glNormalPointer(GL_SHORT, stride, pointer);
  }
  public unsafe static void glNormalPointer(int stride, /*const*/ int* pointer)
  { glNormalPointer(GL_INT, stride, pointer);
  }
  public unsafe static void glNormalPointer(int stride, /*const*/ float* pointer)
  { glNormalPointer(GL_FLOAT, stride, pointer);
  }
  public unsafe static void glNormalPointer(int stride, /*const*/ double* pointer)
  { glNormalPointer(GL_DOUBLE, stride, pointer);
  }
  public unsafe static void glNormalPointer(int stride, byte[] pointer)
  { fixed(byte* p=pointer) glNormalPointer(GL_BYTE, stride, p);
  }
  public unsafe static void glNormalPointer(int stride, short[] pointer)
  { fixed(short* p=pointer) glNormalPointer(GL_SHORT, stride, p);
  }
  public unsafe static void glNormalPointer(int stride, int[] pointer)
  { fixed(int* p=pointer) glNormalPointer(GL_INT, stride, p);
  }
  public unsafe static void glNormalPointer(int stride, float[] pointer)
  { fixed(float* p=pointer) glNormalPointer(GL_FLOAT, stride, p);
  }
  public unsafe static void glNormalPointer(int stride, double[] pointer)
  { fixed(double* p=pointer) glNormalPointer(GL_DOUBLE, stride, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glOrtho(double left, double right, double bottom, double top, double zNear, double zFar);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPassThrough(float token);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glPixelMapfv(uint map, int mapsize, /*const*/ float *values);
  public unsafe static void glPixelMapfv(uint map, float[] values)
  { fixed(float* p=values) glPixelMapfv(map, values.Length, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glPixelMapuiv(uint map, int mapsize, /*const*/ uint *values);
  public unsafe static void glPixelMapuiv(uint map, uint[] values)
  { fixed(uint* p=values) glPixelMapuiv(map, values.Length, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glPixelMapusv(uint map, int mapsize, /*const*/ ushort *values);
  public unsafe static void glPixelMapusv(uint map, ushort[] values)
  { fixed(ushort* p=values) glPixelMapusv(map, values.Length, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPixelStoref(uint pname, float param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPixelStorei(uint pname, int param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPixelTransferf(uint pname, float param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPixelTransferi(uint pname, int param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPixelZoom(float xfactor, float yfactor);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPointSize(float size);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPolygonMode(uint face, uint mode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPolygonOffset(float factor, float units);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glPolygonStipple(/*const*/ byte *mask);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPolygonStipple([In] byte[] mask);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPopAttrib();
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPopClientAttrib();
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPopMatrix();
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPopName();
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glPrioritizeTextures(int n, /*const*/ uint *textures, /*const*/ float* priorities);
  public unsafe static void glPrioritizeTextures(uint[] textures, float[] priorities)
  { fixed(uint* t=textures) fixed(float* p=priorities) glPrioritizeTextures(textures.Length, t, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPushAttrib(uint mask);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPushClientAttrib(uint mask);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPushMatrix();
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glPushName(uint name);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos2d(double x, double y);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glRasterPos2dv(/*const*/ double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos2dv([In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos2f(float x, float y);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static unsafe extern void glRasterPos2fv(/*const*/ float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos2fv([In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos2i(int x, int y);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static unsafe extern void glRasterPos2iv(/*const*/ int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos2iv([In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos2s(short x, short y);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static unsafe extern void glRasterPos2sv(/*const*/ short *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos2sv([In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos3d(double x, double y, double z);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static unsafe extern void glRasterPos3dv(/*const*/ double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos3dv([In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos3f(float x, float y, float z);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static unsafe extern void glRasterPos3fv(/*const*/ float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos3fv([In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos3i(int x, int y, int z);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static unsafe extern void glRasterPos3iv(/*const*/ int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos3iv([In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos3s(short x, short y, short z);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static unsafe extern void glRasterPos3sv(/*const*/ short *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos3sv([In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos4d(double x, double y, double z, double w);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static unsafe extern void glRasterPos4dv(/*const*/ double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos4dv([In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos4f(float x, float y, float z, float w);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static unsafe extern void glRasterPos4fv(/*const*/ float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos4fv([In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos4i(int x, int y, int z, int w);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static unsafe extern void glRasterPos4iv(/*const*/ int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos4iv([In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos4s(short x, short y, short z, short w);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static unsafe extern void glRasterPos4sv(/*const*/ short *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRasterPos4sv([In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glReadBuffer(uint mode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, void* pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, IntPtr pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, [In] [Out] byte[] pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRectd(double x1, double y1, double x2, double y2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glRectdv(/*const*/ double *v1, /*const*/ double *v2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRectdv([In] double[] v1, [In] double[] v2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRectf(float x1, float y1, float x2, float y2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glRectfv(/*const*/ float *v1, /*const*/ float *v2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRectfv([In] float[] v1, [In] float[] v2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRecti(int x1, int y1, int x2, int y2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glRectiv(/*const*/ int *v1, /*const*/ int *v2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRectiv([In] int[] v1, [In] int[] v2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRects(short x1, short y1, short x2, short y2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glRectsv(/*const*/ short *v1, /*const*/ short *v2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRectsv([In] short[] v1, [In] short[] v2);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern int glRenderMode(uint mode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRotated(double angle, double x, double y, double z);
  public static void glRotated(double angle, Mathematics.ThreeD.Vector axis)
  { glRotated(angle, axis.X, axis.Y, axis.Z);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glRotatef(float angle, float x, float y, float z);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glScaled(double x, double y, double z);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glScalef(float x, float y, float z);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glScissor(int x, int y, int width, int height);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glSelectBuffer(int size, uint *buffer);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glSelectBuffer(int size, [In] [Out] uint[] buffer);
  public unsafe static void glSelectBuffer(uint[] buffer)
  { fixed(uint* p=buffer) glSelectBuffer(buffer.Length, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glShadeModel(uint mode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glStencilFunc(uint func, int refval, uint mask);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glStencilMask(uint mask);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glStencilOp(uint fail, uint zfail, uint zpass);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord1d(double s);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord1dv(/*const*/ double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord1dv([In] ref double v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord1f(float s);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord1fv(/*const*/ float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord1fv([In] ref float v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord1i(int s);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord1iv(/*const*/ int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord1iv([In] ref int v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord1s(short s);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord1sv(/*const*/ short *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord1sv([In] ref short v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord2d(double s, double t);
  public static void glTexCoord2d(Mathematics.TwoD.Point pt) { glTexCoord2d(pt.X, pt.Y); }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord2dv(/*const*/ double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord2dv([In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord2f(float s, float t);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord2fv(/*const*/ float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord2fv([In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord2i(int s, int t);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord2iv(/*const*/ int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord2iv([In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord2s(short s, short t);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord2sv(/*const*/ short *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord2sv([In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord3d(double s, double t, double r);
  public static void glTexCoord3d(Mathematics.ThreeD.Point pt) { glTexCoord3d(pt.X, pt.Y, pt.Z); }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord3dv(/*const*/ double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord3dv([In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord3f(float s, float t, float r);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord3fv(/*const*/ float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord3fv([In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord3i(int s, int t, int r);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord3iv(/*const*/ int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord3iv([In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord3s(short s, short t, short r);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord3sv(/*const*/ short *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord3sv([In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord4d(double s, double t, double r, double q);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord4dv(/*const*/ double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord4dv([In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord4f(float s, float t, float r, float q);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord4fv(/*const*/ float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord4fv([In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord4i(int s, int t, int r, int q);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord4iv(/*const*/ int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord4iv([In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord4s(short s, short t, short r, short q);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoord4sv(/*const*/ short *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexCoord4sv([In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexCoordPointer(int size, uint type, int stride, /*const*/ void* pointer);
  public unsafe static void glTexCoordPointer(int size, int stride, /*const*/ short* pointer)
  { glTexCoordPointer(size, GL_SHORT, stride, pointer);
  }
  public unsafe static void glTexCoordPointer(int size, int stride, /*const*/ int* pointer)
  { glTexCoordPointer(size, GL_INT, stride, pointer);
  }
  public unsafe static void glTexCoordPointer(int size, int stride, /*const*/ float* pointer)
  { glTexCoordPointer(size, GL_FLOAT, stride, pointer);
  }
  public unsafe static void glTexCoordPointer(int size, int stride, /*const*/ double* pointer)
  { glTexCoordPointer(size, GL_DOUBLE, stride, pointer);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexEnvf(uint target, uint pname, float param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexEnvfv(uint target, uint pname, /*const*/ float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexEnvfv(uint target, uint pname, [In] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexEnvi(uint target, uint pname, int param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexEnviv(uint target, uint pname, /*const*/ int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexEnviv(uint target, uint pname, [In] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexGend(uint coord, uint pname, double param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexGendv(uint coord, uint pname, /*const*/ double* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexGendv(uint coord, uint pname, [In] double[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexGenf(uint coord, uint pname, float param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexGenfv(uint coord, uint pname, /*const*/ float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexGenfv(uint coord, uint pname, [In] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexGeni(uint coord, uint pname, int param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexGeniv(uint coord, uint pname, /*const*/ int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexGeniv(uint coord, uint pname, [In] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexImage1D(uint target, int level, uint internalformat, int width, int border, uint format, uint type, /*const*/ void* pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexImage1D(uint target, int level, uint internalformat, int width, int border, uint format, uint type, IntPtr pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexImage1D(uint target, int level, uint internalformat, int width, int border, uint format, uint type, byte[] pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, /*const*/ void* pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, IntPtr pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, byte[] pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexParameterf(uint target, uint pname, float param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexParameterfv(uint target, uint pname, /*const*/ float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexParameterfv(uint target, uint pname, [In] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexParameteri(uint target, uint pname, uint param);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexParameteriv(uint target, uint pname, /*const*/ int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTexParameteriv(uint target, uint pname, [In] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexSubImage1D(uint target, int level, int xoffset, int width, uint format, uint type, /*const*/ void* pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexSubImage1D(uint target, int level, int xoffset, int width, uint format, uint type, IntPtr pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexSubImage1D(uint target, int level, int xoffset, int width, uint format, uint type, [In] byte[] pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexSubImage2D(uint target, int level, int xoffset, int yoffset, int width, int height, uint format, uint type, /*const*/ void* pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexSubImage2D(uint target, int level, int xoffset, int yoffset, int width, int height, uint format, uint type, IntPtr pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexSubImage2D(uint target, int level, int xoffset, int yoffset, int width, int height, uint format, uint type, [In] byte[] pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTranslated(double x, double y, double z);
  public static void glTranslated(Mathematics.ThreeD.Vector v) { glTranslated(v.X, v.Y, v.Z); }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glTranslatef(float x, float y, float z);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex2d(double x, double y);
  public static void glVertex2d(Mathematics.TwoD.Point pt) { glVertex2d(pt.X, pt.Y); }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glVertex2dv(/*const*/ double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex2dv([In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex2f(float x, float y);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glVertex2fv(/*const*/ float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex2fv([In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex2i(int x, int y);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glVertex2iv(/*const*/ int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex2iv([In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex2s(short x, short y);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glVertex2sv(/*const*/ short *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex2sv([In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex3d(double x, double y, double z);
  public static void glVertex3d(Mathematics.ThreeD.Point pt) { glVertex3d(pt.X, pt.Y, pt.Z); }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glVertex3dv(/*const*/ double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex3dv([In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex3f(float x, float y, float z);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glVertex3fv(/*const*/ float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex3fv([In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex3i(int x, int y, int z);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glVertex3iv(/*const*/ int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex3iv([In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex3s(short x, short y, short z);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glVertex3sv(/*const*/ short *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex3sv([In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex4d(double x, double y, double z, double w);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glVertex4dv(/*const*/ double *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex4dv([In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex4f(float x, float y, float z, float w);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glVertex4fv(/*const*/ float *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex4fv([In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex4i(int x, int y, int z, int w);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glVertex4iv(/*const*/ int *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex4iv([In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex4s(short x, short y, short z, short w);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glVertex4sv(/*const*/ short *v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glVertex4sv([In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glVertexPointer(int size, uint type, int stride, /*const*/ void* pointer);
  public unsafe static void glVertexPointer(int size, int stride, /*const*/ short* pointer)
  { glVertexPointer(size, GL_SHORT, stride, pointer);
  }
  public unsafe static void glVertexPointer(int size, int stride, /*const*/ int* pointer)
  { glVertexPointer(size, GL_INT, stride, pointer);
  }
  public unsafe static void glVertexPointer(int size, int stride, /*const*/ float* pointer)
  { glVertexPointer(size, GL_FLOAT, stride, pointer);
  }
  public unsafe static void glVertexPointer(int size, int stride, /*const*/ double* pointer)
  { glVertexPointer(size, GL_DOUBLE, stride, pointer);
  }
  public unsafe static void glVertexPointer(int size, int stride, short[] pointer)
  { fixed(short* p=pointer) glVertexPointer(size, GL_SHORT, stride, p);
  }
  public unsafe static void glVertexPointer(int size, int stride, int[] pointer)
  { fixed(int* p=pointer) glVertexPointer(size, GL_INT, stride, p);
  }
  public unsafe static void glVertexPointer(int size, int stride, float[] pointer)
  { fixed(float* p=pointer) glVertexPointer(size, GL_FLOAT, stride, p);
  }
  public unsafe static void glVertexPointer(int size, int stride, double[] pointer)
  { fixed(double* p=pointer) glVertexPointer(size, GL_DOUBLE, stride, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glViewport(int x, int y, int width, int height);
  #endregion
  #endregion

  #region OpenGL 1.2
  #region Flags, Enums, Defines, etc
  public const int GL_CONSTANT_COLOR    = 0x8001;
  public const int GL_ONE_MINUS_CONSTANT_COLOR    = 0x8002;
  public const int GL_CONSTANT_ALPHA    = 0x8003;
  public const int GL_ONE_MINUS_CONSTANT_ALPHA    = 0x8004;
  public const int GL_BLEND_COLOR    = 0x8005;
  public const int GL_FUNC_ADD    = 0x8006;
  public const int GL_MIN    = 0x8007;
  public const int GL_MAX    = 0x8008;
  public const int GL_BLEND_EQUATION    = 0x8009;
  public const int GL_FUNC_SUBTRACT    = 0x800A;
  public const int GL_FUNC_REVERSE_SUBTRACT    = 0x800B;
  public const int GL_CONVOLUTION_1D    = 0x8010;
  public const int GL_CONVOLUTION_2D    = 0x8011;
  public const int GL_SEPARABLE_2D    = 0x8012;
  public const int GL_CONVOLUTION_BORDER_MODE    = 0x8013;
  public const int GL_CONVOLUTION_FILTER_SCALE    = 0x8014;
  public const int GL_CONVOLUTION_FILTER_BIAS    = 0x8015;
  public const int GL_REDUCE    = 0x8016;
  public const int GL_CONVOLUTION_FORMAT     = 0x8017;
  public const int GL_CONVOLUTION_WIDTH    = 0x8018;
  public const int GL_CONVOLUTION_HEIGHT     = 0x8019;
  public const int GL_MAX_CONVOLUTION_WIDTH    = 0x801A;
  public const int GL_MAX_CONVOLUTION_HEIGHT    = 0x801B;
  public const int GL_POST_CONVOLUTION_RED_SCALE     = 0x801C;
  public const int GL_POST_CONVOLUTION_GREEN_SCALE   = 0x801D;
  public const int GL_POST_CONVOLUTION_BLUE_SCALE    = 0x801E;
  public const int GL_POST_CONVOLUTION_ALPHA_SCALE   = 0x801F;
  public const int GL_POST_CONVOLUTION_RED_BIAS    = 0x8020;
  public const int GL_POST_CONVOLUTION_GREEN_BIAS    = 0x8021;
  public const int GL_POST_CONVOLUTION_BLUE_BIAS     = 0x8022;
  public const int GL_POST_CONVOLUTION_ALPHA_BIAS    = 0x8023;
  public const int GL_HISTOGRAM    = 0x8024;
  public const int GL_PROXY_HISTOGRAM    = 0x8025;
  public const int GL_HISTOGRAM_WIDTH    = 0x8026;
  public const int GL_HISTOGRAM_FORMAT    = 0x8027;
  public const int GL_HISTOGRAM_RED_SIZE     = 0x8028;
  public const int GL_HISTOGRAM_GREEN_SIZE    = 0x8029;
  public const int GL_HISTOGRAM_BLUE_SIZE    = 0x802A;
  public const int GL_HISTOGRAM_ALPHA_SIZE    = 0x802B;
  public const int GL_HISTOGRAM_LUMINANCE_SIZE    = 0x802C;
  public const int GL_HISTOGRAM_SINK    = 0x802D;
  public const int GL_MINMAX    = 0x802E;
  public const int GL_MINMAX_FORMAT    = 0x802F;
  public const int GL_MINMAX_SINK    = 0x8030;
  public const int GL_TABLE_TOO_LARGE    = 0x8031;
  public const int GL_UNSIGNED_BYTE_3_3_2    = 0x8032;
  public const int GL_UNSIGNED_SHORT_4_4_4_4    = 0x8033;
  public const int GL_UNSIGNED_SHORT_5_5_5_1    = 0x8034;
  public const int GL_UNSIGNED_INT_8_8_8_8    = 0x8035;
  public const int GL_UNSIGNED_INT_10_10_10_2    = 0x8036;
  public const int GL_RESCALE_NORMAL    = 0x803A;
  public const int GL_UNSIGNED_BYTE_2_3_3_REV    = 0x8362;
  public const int GL_UNSIGNED_SHORT_5_6_5    = 0x8363;
  public const int GL_UNSIGNED_SHORT_5_6_5_REV    = 0x8364;
  public const int GL_UNSIGNED_SHORT_4_4_4_4_REV     = 0x8365;
  public const int GL_UNSIGNED_SHORT_1_5_5_5_REV     = 0x8366;
  public const int GL_UNSIGNED_INT_8_8_8_8_REV    = 0x8367;
  public const int GL_UNSIGNED_INT_2_10_10_10_REV    = 0x8368;
  public const int GL_COLOR_MATRIX    = 0x80B1;
  public const int GL_COLOR_MATRIX_STACK_DEPTH    = 0x80B2;
  public const int GL_MAX_COLOR_MATRIX_STACK_DEPTH   = 0x80B3;
  public const int GL_POST_COLOR_MATRIX_RED_SCALE    = 0x80B4;
  public const int GL_POST_COLOR_MATRIX_GREEN_SCALE  = 0x80B5;
  public const int GL_POST_COLOR_MATRIX_BLUE_SCALE   = 0x80B6;
  public const int GL_POST_COLOR_MATRIX_ALPHA_SCALE  = 0x80B7;
  public const int GL_POST_COLOR_MATRIX_RED_BIAS     = 0x80B8;
  public const int GL_POST_COLOR_MATRIX_GREEN_BIAS   = 0x80B9;
  public const int GL_POST_COLOR_MATRIX_BLUE_BIAS    = 0x80BA;
  public const int GL_COLOR_TABLE    = 0x80D0;
  public const int GL_POST_CONVOLUTION_COLOR_TABLE   = 0x80D1;
  public const int GL_POST_COLOR_MATRIX_COLOR_TABLE  = 0x80D2;
  public const int GL_PROXY_COLOR_TABLE    = 0x80D3;
  public const int GL_PROXY_POST_CONVOLUTION_COLOR_TABLE = 0x80D4;
  public const int GL_PROXY_POST_COLOR_MATRIX_COLOR_TABLE = 0x80D5;
  public const int GL_COLOR_TABLE_SCALE    = 0x80D6;
  public const int GL_COLOR_TABLE_BIAS    = 0x80D7;
  public const int GL_COLOR_TABLE_FORMAT     = 0x80D8;
  public const int GL_COLOR_TABLE_WIDTH    = 0x80D9;
  public const int GL_COLOR_TABLE_RED_SIZE    = 0x80DA;
  public const int GL_COLOR_TABLE_GREEN_SIZE    = 0x80DB;
  public const int GL_COLOR_TABLE_BLUE_SIZE    = 0x80DC;
  public const int GL_COLOR_TABLE_ALPHA_SIZE    = 0x80DD;
  public const int GL_COLOR_TABLE_LUMINANCE_SIZE     = 0x80DE;
  public const int GL_COLOR_TABLE_INTENSITY_SIZE     = 0x80DF;
  public const int GL_CLAMP_TO_EDGE    = 0x812F;
  public const int GL_TEXTURE_MIN_LOD    = 0x813A;
  public const int GL_TEXTURE_MAX_LOD    = 0x813B;
  public const int GL_TEXTURE_BASE_LEVEL     = 0x813C;
  public const int GL_TEXTURE_MAX_LEVEL    = 0x813D;
  public const int GL_LIGHT_MODEL_COLOR_CONTROL      = 0x81F8;
  public const int GL_SINGLE_COLOR                   = 0x81F9;
  public const int GL_SEPARATE_SPECULAR_COLOR        = 0x81FA;
  public const int GL_MAX_ELEMENTS_VERTICES          = 0xF0E8;
  public const int GL_MAX_ELEMENTS_INDICES           = 0xF0E9;
  public const int GL_POST_COLOR_MATRIX_ALPHA_BIAS   = 0x80BB;
  public const int GL_BGR                            = 0x80E0;
  public const int GL_BGRA                           = 0x80E1;
  #endregion

  #region Imports
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glDrawRangeElements(uint mode, int start, int end, int count, uint type, void* indices);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glDrawRangeElements(uint mode, int start, int end, int count, uint type, IntPtr indices);
  public unsafe static void glDrawRangeElements(uint mode, int start, int end, int count, byte* indices)
  { glDrawRangeElements(mode, start, end, count, GL_UNSIGNED_BYTE, indices);
  }
  public unsafe static void glDrawRangeElements(uint mode, int start, int end, int count, ushort* indices)
  { glDrawRangeElements(mode, start, end, count, GL_UNSIGNED_SHORT, indices);
  }
  public unsafe static void glDrawRangeElements(uint mode, int start, int end, int count, uint* indices)
  { glDrawRangeElements(mode, start, end, count, GL_UNSIGNED_INT, indices);
  }
  public unsafe static void glDrawRangeElements(uint mode, int start, int end, int count, byte[] indices)
  { fixed(byte* p=indices) glDrawRangeElements(mode, start, end, count, GL_UNSIGNED_BYTE, p);
  }
  public unsafe static void glDrawRangeElements(uint mode, int start, int end, int count, ushort[] indices)
  { fixed(ushort* p=indices) glDrawRangeElements(mode, start, end, count, GL_UNSIGNED_SHORT, p);
  }
  public unsafe static void glDrawRangeElements(uint mode, int start, int end, int count, uint[] indices)
  { fixed(uint* p=indices) glDrawRangeElements(mode, start, end, count, GL_UNSIGNED_INT, p);
  }
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexImage3D(uint target, int level, int internalFormat, int width, int height, int depth, int border, uint format, uint type, void* pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexImage3D(uint target, int level, int internalFormat, int width, int height, int depth, int border, uint format, uint type, IntPtr pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexImage3D(uint target, int level, int internalFormat, int width, int height, int depth, int border, uint format, uint type, [In] byte[] pixels);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexSubImage3D(uint target, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, uint format, uint type, void* pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexSubImage3D(uint target, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, uint format, uint type, IntPtr pixels);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glTexSubImage3D(uint target, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, uint format, uint type, [In] byte[] pixels);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glCopyTexSubImage3D(uint target, int level, int xoffset, int yoffset, int zoffset, int x, int y, int width, int height);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColorTable(uint target, uint internalformat, int width, uint format, uint type, void* table);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColorTable(uint target, uint internalformat, int width, uint format, uint type, IntPtr table);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColorTable(uint target, uint internalformat, int width, uint format, uint type, [In] byte[] table);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColorSubTable(uint target, int start, int count, uint format, uint type, void* data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColorSubTable(uint target, int start, int count, uint format, uint type, IntPtr data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColorSubTable(uint target, int start, int count, uint format, uint type, [In] byte[] data);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColorTableParameteriv(uint target, uint pname, int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColorTableParameteriv(uint target, uint pname, [In] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColorTableParameterfv(uint target, uint pname, float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glColorTableParameterfv(uint target, uint pname, [In] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glCopyColorSubTable(uint target, int start, int x, int y, int width);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glCopyColorTable(uint target, uint internalformat, int x, int y, int width);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetColorTable(uint target, uint format, uint type, void* table);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetColorTable(uint target, uint format, uint type, IntPtr table);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetColorTable(uint target, uint format, uint type, [In] byte[] table);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetColorTableParameterfv(uint target, uint pname, float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetColorTableParameterfv(uint target, uint pname, out float parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetColorTableParameterfv(uint target, uint pname, [In] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetColorTableParameteriv(uint target, uint pname, int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetColorTableParameteriv(uint target, uint pname, out int parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetColorTableParameteriv(uint target, uint pname, [In] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glBlendEquation(uint mode);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glBlendColor(float red, float green, float blue, float alpha);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glHistogram(uint target, int width, uint internalformat, byte sink);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glResetHistogram(uint target);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]

  public unsafe static extern void glGetHistogram(uint target, byte reset, uint format, uint type, void* values);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetHistogram(uint target, byte reset, uint format, uint type, IntPtr values);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetHistogram(uint target, byte reset, uint format, uint type, [In] [Out] byte[] values);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetHistogramParameterfv(uint target, uint pname, float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetHistogramParameterfv(uint target, uint pname, out float parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetHistogramParameterfv(uint target, uint pname, [In] [Out] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetHistogramParameteriv(uint target, uint pname, int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetHistogramParameteriv(uint target, uint pname, out int parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetHistogramParameteriv(uint target, uint pname, [In] [Out] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glMinmax(uint target, uint internalformat, byte sink);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glResetMinmax(uint target); 

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMinmax(uint target, byte reset, uint format, uint types, void* values);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMinmax(uint target, byte reset, uint format, uint types, IntPtr values);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMinmax(uint target, byte reset, uint format, uint types, [In] [Out] byte[] values);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMinmaxParameterfv(uint target, uint pname, float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMinmaxParameterfv(uint target, uint pname, out float parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMinmaxParameterfv(uint target, uint pname, [In] [Out] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMinmaxParameteriv(uint target, uint pname, int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMinmaxParameteriv(uint target, uint pname, out int parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetMinmaxParameteriv(uint target, uint pname, [In] [Out] int[] parms);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glConvolutionFilter1D(uint target, uint internalformat, int width, uint format, uint type, void* image);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glConvolutionFilter1D(uint target, uint internalformat, int width, uint format, uint type, IntPtr image);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glConvolutionFilter1D(uint target, uint internalformat, int width, uint format, uint type, [In] byte[] image);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glConvolutionFilter2D(uint target, uint internalformat, int width, int height, uint format, uint type, void* image);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glConvolutionFilter2D(uint target, uint internalformat, int width, int height, uint format, uint type, IntPtr image);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glConvolutionFilter2D(uint target, uint internalformat, int width, int height, uint format, uint type, [In] byte[] image);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glConvolutionParameterf(uint target, uint pname, float parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glConvolutionParameterfv(uint target, uint pname, float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glConvolutionParameterfv(uint target, uint pname, [In] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glConvolutionParameteri(uint target, uint pname, int parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glConvolutionParameteriv(uint target, uint pname, int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glConvolutionParameteriv(uint target, uint pname, [In] int[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glCopyConvolutionFilter1D(uint target, uint internalformat, int x, int y, int width);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public static extern void glCopyConvolutionFilter2D(uint target, uint internalformat, int x, int y, int width, int height);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetConvolutionFilter(uint target, uint format, uint type, void* image);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetConvolutionFilter(uint target, uint format, uint type, IntPtr image);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetConvolutionFilter(uint target, uint format, uint type, [In] [Out] byte[] image);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetConvolutionParameterfv(uint target, uint pname, float* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetConvolutionParameterfv(uint target, uint pname, out float parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetConvolutionParameterfv(uint target, uint pname, [In] [Out] float[] parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetConvolutionParameteriv(uint target, uint pname, int* parms);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetConvolutionParameteriv(uint target, uint pname, out int parm);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetConvolutionParameteriv(uint target, uint pname, [In] [Out] int[] parms);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glSeparableFilter2D(uint target, uint internalformat, int width, int height, uint format, uint type, void* row, void* column);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glSeparableFilter2D(uint target, uint internalformat, int width, int height, uint format, uint type, IntPtr row, IntPtr column);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glSeparableFilter2D(uint target, uint internalformat, int width, int height, uint format, uint type, [In] byte[] row, [In] byte[] column);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetSeparableFilter(uint target, uint format, uint type, void* row, void* column, void* span);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetSeparableFilter(uint target, uint format, uint type, void* row, IntPtr column, IntPtr span);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void glGetSeparableFilter(uint target, uint format, uint type, void* row, [In] [Out] byte[] column, [In] [Out] byte[] span);
  #endregion
  #endregion

  #region OpenGL 1.3
  #region Flags, Enums, Defines, etc
  public const int GL_TEXTURE0 = 0x84C0;
  public const int GL_TEXTURE1 = 0x84C1;
  public const int GL_TEXTURE2 = 0x84C2;
  public const int GL_TEXTURE3 = 0x84C3;
  public const int GL_TEXTURE4 = 0x84C4;
  public const int GL_TEXTURE5 = 0x84C5;
  public const int GL_TEXTURE6 = 0x84C6;
  public const int GL_TEXTURE7 = 0x84C7;
  public const int GL_TEXTURE8 = 0x84C8;
  public const int GL_TEXTURE9 = 0x84C9;
  public const int GL_TEXTURE10 = 0x84CA;
  public const int GL_TEXTURE11 = 0x84CB;
  public const int GL_TEXTURE12 = 0x84CC;
  public const int GL_TEXTURE13 = 0x84CD;
  public const int GL_TEXTURE14 = 0x84CE;
  public const int GL_TEXTURE15 = 0x84CF;
  public const int GL_TEXTURE16 = 0x84D0;
  public const int GL_TEXTURE17 = 0x84D1;
  public const int GL_TEXTURE18 = 0x84D2;
  public const int GL_TEXTURE19 = 0x84D3;
  public const int GL_TEXTURE20 = 0x84D4;
  public const int GL_TEXTURE21 = 0x84D5;
  public const int GL_TEXTURE22 = 0x84D6;
  public const int GL_TEXTURE23 = 0x84D7;
  public const int GL_TEXTURE24 = 0x84D8;
  public const int GL_TEXTURE25 = 0x84D9;
  public const int GL_TEXTURE26 = 0x84DA;
  public const int GL_TEXTURE27 = 0x84DB;
  public const int GL_TEXTURE28 = 0x84DC;
  public const int GL_TEXTURE29 = 0x84DD;
  public const int GL_TEXTURE30 = 0x84DE;
  public const int GL_TEXTURE31 = 0x84DF;
  public const int GL_ACTIVE_TEXTURE = 0x84E0;
  public const int GL_CLIENT_ACTIVE_TEXTURE = 0x84E1;
  public const int GL_MAX_TEXTURE_UNITS = 0x84E2;
  public const int GL_NORMAL_MAP = 0x8511;
  public const int GL_REFLECTION_MAP = 0x8512;
  public const int GL_TEXTURE_CUBE_MAP = 0x8513;
  public const int GL_TEXTURE_BINDING_CUBE_MAP = 0x8514;
  public const int GL_TEXTURE_CUBE_MAP_POSITIVE_X = 0x8515;
  public const int GL_TEXTURE_CUBE_MAP_NEGATIVE_X = 0x8516;
  public const int GL_TEXTURE_CUBE_MAP_POSITIVE_Y = 0x8517;
  public const int GL_TEXTURE_CUBE_MAP_NEGATIVE_Y = 0x8518;
  public const int GL_TEXTURE_CUBE_MAP_POSITIVE_Z = 0x8519;
  public const int GL_TEXTURE_CUBE_MAP_NEGATIVE_Z = 0x851A;
  public const int GL_PROXY_TEXTURE_CUBE_MAP = 0x851B;
  public const int GL_MAX_CUBE_MAP_TEXTURE_SIZE = 0x851C;
  public const int GL_COMPRESSED_ALPHA = 0x84E9;
  public const int GL_COMPRESSED_LUMINANCE = 0x84EA;
  public const int GL_COMPRESSED_LUMINANCE_ALPHA = 0x84EB;
  public const int GL_COMPRESSED_INTENSITY = 0x84EC;
  public const int GL_COMPRESSED_RGB = 0x84ED;
  public const int GL_COMPRESSED_RGBA = 0x84EE;
  public const int GL_TEXTURE_COMPRESSION_HINT = 0x84EF;
  public const int GL_TEXTURE_COMPRESSED_IMAGE_SIZE= 0x86A0;
  public const int GL_TEXTURE_COMPRESSED = 0x86A1;
  public const int GL_NUM_COMPRESSED_TEXTURE_FORMATS = 0x86A2;
  public const int GL_COMPRESSED_TEXTURE_FORMATS = 0x86A3;
  public const int GL_MULTISAMPLE = 0x809D;
  public const int GL_SAMPLE_ALPHA_TO_COVERAGE = 0x809E;
  public const int GL_SAMPLE_ALPHA_TO_ONE = 0x809F;
  public const int GL_SAMPLE_COVERAGE = 0x80A0;
  public const int GL_SAMPLE_BUFFERS = 0x80A8;
  public const int GL_SAMPLES = 0x80A9;
  public const int GL_SAMPLE_COVERAGE_VALUE = 0x80AA;
  public const int GL_SAMPLE_COVERAGE_INVERT = 0x80AB;
  public const int GL_MULTISAMPLE_BIT = 0x20000000;
  public const int GL_TRANSPOSE_MODELVIEW_MATRIX = 0x84E3;
  public const int GL_TRANSPOSE_PROJECTION_MATRIX = 0x84E4;
  public const int GL_TRANSPOSE_TEXTURE_MATRIX = 0x84E5;
  public const int GL_TRANSPOSE_COLOR_MATRIX = 0x84E6;
  public const int GL_COMBINE = 0x8570;
  public const int GL_COMBINE_RGB = 0x8571;
  public const int GL_COMBINE_ALPHA = 0x8572;
  public const int GL_SOURCE0_RGB = 0x8580;
  public const int GL_SOURCE1_RGB = 0x8581;
  public const int GL_SOURCE2_RGB = 0x8582;
  public const int GL_SOURCE0_ALPHA = 0x8588;
  public const int GL_SOURCE1_ALPHA = 0x8589;
  public const int GL_SOURCE2_ALPHA = 0x858A;
  public const int GL_OPERAND0_RGB = 0x8590;
  public const int GL_OPERAND1_RGB = 0x8591;
  public const int GL_OPERAND2_RGB = 0x8592;
  public const int GL_OPERAND0_ALPHA = 0x8598;
  public const int GL_OPERAND1_ALPHA = 0x8599;
  public const int GL_OPERAND2_ALPHA = 0x859A;
  public const int GL_RGB_SCALE = 0x8573;
  public const int GL_ADD_SIGNED = 0x8574;
  public const int GL_INTERPOLATE = 0x8575;
  public const int GL_SUBTRACT = 0x84E7;
  public const int GL_CONSTANT = 0x8576;
  public const int GL_PRIMARY_COLOR = 0x8577;
  public const int GL_PREVIOUS = 0x8578;
  public const int GL_DOT3_RGB = 0x86AE;
  public const int GL_DOT3_RGBA = 0x86AF;
  public const int GL_CLAMP_TO_BORDER = 0x812D;
  public const int GL_TEXTURE0_ARB = 0x84C0;
  public const int GL_TEXTURE1_ARB = 0x84C1;
  public const int GL_TEXTURE2_ARB = 0x84C2;
  public const int GL_TEXTURE3_ARB = 0x84C3;
  public const int GL_TEXTURE4_ARB = 0x84C4;
  public const int GL_TEXTURE5_ARB = 0x84C5;
  public const int GL_TEXTURE6_ARB = 0x84C6;
  public const int GL_TEXTURE7_ARB = 0x84C7;
  public const int GL_TEXTURE8_ARB = 0x84C8;
  public const int GL_TEXTURE9_ARB = 0x84C9;
  public const int GL_TEXTURE10_ARB = 0x84CA;
  public const int GL_TEXTURE11_ARB = 0x84CB;
  public const int GL_TEXTURE12_ARB = 0x84CC;
  public const int GL_TEXTURE13_ARB = 0x84CD;
  public const int GL_TEXTURE14_ARB = 0x84CE;
  public const int GL_TEXTURE15_ARB = 0x84CF;
  public const int GL_TEXTURE16_ARB = 0x84D0;
  public const int GL_TEXTURE17_ARB = 0x84D1;
  public const int GL_TEXTURE18_ARB = 0x84D2;
  public const int GL_TEXTURE19_ARB = 0x84D3;
  public const int GL_TEXTURE20_ARB = 0x84D4;
  public const int GL_TEXTURE21_ARB = 0x84D5;
  public const int GL_TEXTURE22_ARB = 0x84D6;
  public const int GL_TEXTURE23_ARB = 0x84D7;
  public const int GL_TEXTURE24_ARB = 0x84D8;
  public const int GL_TEXTURE25_ARB = 0x84D9;
  public const int GL_TEXTURE26_ARB = 0x84DA;
  public const int GL_TEXTURE27_ARB = 0x84DB;
  public const int GL_TEXTURE28_ARB = 0x84DC;
  public const int GL_TEXTURE29_ARB = 0x84DD;
  public const int GL_TEXTURE30_ARB = 0x84DE;
  public const int GL_TEXTURE31_ARB = 0x84DF;
  public const int GL_ACTIVE_TEXTURE_ARB = 0x84E0;
  public const int GL_CLIENT_ACTIVE_TEXTURE_ARB = 0x84E1;
  public const int GL_MAX_TEXTURE_UNITS_ARB = 0x84E2;
  #endregion

  #region Imports
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glActiveTexture(uint texture);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glClientActiveTexture(uint texture);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexImage1D(uint target, int level, uint internalformat, int width, int border, int imageSize, void* data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexImage1D(uint target, int level, uint internalformat, int width, int border, int imageSize, IntPtr data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexImage1D(uint target, int level, uint internalformat, int width, int border, int imageSize, [In] byte[] data);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, int imageSize, void* data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, int imageSize, IntPtr data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, int imageSize, [In] byte[] data);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexImage3D(uint target, int level, uint internalformat, int width, int height, int depth, int border, int imageSize, void* data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexImage3D(uint target, int level, uint internalformat, int width, int height, int depth, int border, int imageSize, IntPtr data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexImage3D(uint target, int level, uint internalformat, int width, int height, int depth, int border, int imageSize, [In] byte[] data);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexSubImage1D(uint target, int level, int xoffset, int width, uint format, int imageSize, void* data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexSubImage1D(uint target, int level, int xoffset, int width, uint format, int imageSize, IntPtr data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexSubImage1D(uint target, int level, int xoffset, int width, uint format, int imageSize, [In] byte[] data);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexSubImage2D(uint target, int level, int xoffset, int yoffset, int width, int height, uint format, int imageSize, void* data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexSubImage2D(uint target, int level, int xoffset, int yoffset, int width, int height, uint format, int imageSize, IntPtr data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexSubImage2D(uint target, int level, int xoffset, int yoffset, int width, int height, uint format, int imageSize, [In] byte[] data);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexSubImage3D(uint target, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, uint format, int imageSize, void* data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexSubImage3D(uint target, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, uint format, int imageSize, IntPtr data);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glCompressedTexSubImage3D(uint target, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, uint format, int imageSize, [In] byte[] data);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glGetCompressedTexImage(uint target, int lod, void* img);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glGetCompressedTexImage(uint target, int lod, IntPtr img);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glGetCompressedTexImage(uint target, int lod, [In] [Out] byte[] img);

  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord1d(uint target, double s);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord1dv(uint target, double* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord1dv(uint target, [In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord1f(uint target, float s);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord1fv(uint target, float* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord1fv(uint target, [In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord1i(uint target, int s);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord1iv(uint target, int* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord1iv(uint target, [In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord1s(uint target, short s);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord1sv(uint target, short* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord1sv(uint target, [In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord2d(uint target, double s, double t);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord2dv(uint target, double* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord2dv(uint target, [In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord2f(uint target, float s, float t);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord2fv(uint target, float* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord2fv(uint target, [In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord2i(uint target, int s, int t);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord2iv(uint target, int* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord2iv(uint target, [In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord2s(uint target, short s, short t);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord2sv(uint target, short* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord2sv(uint target, [In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord3d(uint target, double s, double t, double r);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord3dv(uint target, double* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord3dv(uint target, [In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord3f(uint target, float s, float t, float r);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord3fv(uint target, float* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord3fv(uint target, [In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord3i(uint target, int s, int t, int r);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord3iv(uint target, int* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord3iv(uint target, [In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord3s(uint target, short s, short t, short r);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord3sv(uint target, short* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord3sv(uint target, [In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord4d(uint target, double s, double t, double r, double q);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord4dv(uint target, double* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord4dv(uint target, [In] double[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord4f(uint target, float s, float t, float r, float q);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord4fv(uint target, float* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord4fv(uint target, [In] float[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord4i(uint target, int s, int t, int r, int q);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord4iv(uint target, int* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord4iv(uint target, [In] int[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glMultiTexCoord4s(uint target, short s, short t, short r, short q);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord4sv(uint target, short* v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultiTexCoord4sv(uint target, [In] short[] v);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glLoadTransposeMatrixd(double* m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glLoadTransposeMatrixd([In] double[] m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glLoadTransposeMatrixf(float* m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glLoadTransposeMatrixf([In] float[] m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultTransposeMatrixd(double* m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultTransposeMatrixd([In] double[] m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultTransposeMatrixf(float* m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static unsafe extern void glMultTransposeMatrixf([In] float[] m);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glSampleCoverage(float value, byte invert);
  [DllImport(Config.OpenGLImportPath, ExactSpelling=true, CallingConvention=CallingConvention.Winapi)]
  static extern void glSamplePass(uint pass);
  #endregion
  #endregion
}
#endregion

#region GLU
/// <summary>This class provides access to the native, low-level GLU API. See the official GLU documentation for
/// information regarding these methods.
/// </summary>
[System.Security.SuppressUnmanagedCodeSecurity()]
public sealed class GLU
{ private GLU() { }
  // TODO: add GLU objects (nurbs, etc)

  #region Enums & Constants
  public const uint GLU_VERSION    = 100800;
  public const uint GLU_EXTENSIONS = 100801;

  public const uint GLU_TRUE       = GL.GL_TRUE;
  public const uint GLU_FALSE      = GL.GL_FALSE;
  #endregion

  #region Imports
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public static extern string gluErrorString(int error);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public static extern string gluGetString(uint str);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public static extern void gluOrtho2D(double left, double right, double bottom, double top);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public static extern void gluPerspective(double fovy, double aspect, double zNear, double zFar);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern void gluPickMatrix(double x, double y, double width, double height, int* viewport);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public static extern void gluPickMatrix(double x, double y, double width, double height, [In] int[] viewport);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public static extern void gluLookAt(double eyex, double eyey, double eyez, double centerx, double centery, double centerz, double upx, double upy, double upz);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern int gluProject(double objx, double objy, double objz, /*const*/ double* modelMatrix, /*const*/ double* projMatrix, /*const*/ int* viewport, double* winx, double* winy, double* winz);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public static extern int gluProject(double objx, double objy, double objz, double[] modelMatrix, double[] projMatrix, int[] viewport, out double winx, out double winy, out double winz);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern int gluUnProject(double winx, double winy, double winz, /*const*/ double* modelMatrix, /*const*/ double* projMatrix, /*const*/ int* viewport, double *objx, double *objy, double *objz);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public static extern int gluUnProject(double winx, double winy, double winz, double[] modelMatrix, double[] projMatrix, int[] viewport, out double objx, out double objy, out double objz);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern int gluScaleImage(uint format, int widthin, int heightin, uint typein, /*const*/ void* datain, int widthout, int heightout, uint typeout, void* dataout);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern int gluScaleImage(uint format, int widthin, int heightin, uint typein, IntPtr datain, int widthout, int heightout, uint typeout, IntPtr dataout);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern int gluScaleImage(uint format, int widthin, int heightin, uint typein, [In] byte[] datain, int widthout, int heightout, uint typeout, [Out] byte[] dataout);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern int gluBuild1DMipmaps(uint target, int components, int width, uint format, uint type, /*const*/ void* data);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern int gluBuild1DMipmaps(uint target, int components, int width, uint format, uint type, IntPtr data);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern int gluBuild1DMipmaps(uint target, int components, int width, uint format, uint type, [In] byte[] data);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern int gluBuild2DMipmaps(uint target, int components, int width, int height, uint format, uint type, /*const*/ void* data);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern int gluBuild2DMipmaps(uint target, int components, int width, int height, uint format, uint type, IntPtr data);
  [DllImport(Config.GLUImportPath, CallingConvention=CallingConvention.Winapi)]
  public unsafe static extern int gluBuild2DMipmaps(uint target, int components, int width, int height, uint format, uint type, [In] byte[] data);
  #endregion
}
#endregion

} // namespace GameLib.Interop.OpenGL
