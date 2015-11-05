/****************************************************************************
* (c) Copyright 2011, Freescale Semiconductor Inc.
 * ALL RIGHTS RESERVED.
 ***************************************************************************//*!
 * @file      flash_kinetis.h
 * @author    R20253
 * @version   1.0.8.0
 * @date      Dec-13-2012
 * @brief     Flash programming driver header file
 * @par       
 * @include   
 * @par       
 * @include         
 ******************************************************************************/
#ifndef _FLASH_KINETIS_H
#define _FLASH_KINETIS_H
#include "bootloader.h"
//  Flash hardware algorithm operation commands 
#define FLASH_PROGRAM_LONGWORD    0x06
#define FLASH_ERASE_SECTOR        0x09

#define FCCOB_REGS  12
#define FLASH_OK     0
#define FLASH_FAIL   1

#define FLASH_BURST_RAM_ADDR (LWord*)0x14000000
#define FLASH_BURST_RAM_SIZE	64


#define ERASE_BLOCK_INDEX       4
#define PROGRAM_LONGWORD_INDEX  8

//  FCOOB register structure
typedef union 
{
  Byte all[FCCOB_REGS];
  struct
  {
    Byte fccob3;
    Byte fccob2;
    Byte fccob1;
    Byte fccob0;
    Byte fccob7;
    Byte fccob6;
    Byte fccob5;
    Byte fccob4;
    Byte fccobB;
    Byte fccobA;
    Byte fccob9;
    Byte fccob8;
  }regs;
  
  struct
  {
    LWord fccob3210;
    LWord fccob7654;
    LWord fccobBA98;
  }regsLong;
}FCC0B_STR;

//  API FUNCTION FOR KINETIS FLASH DRIVER
void  FLASH_Initialization(void);
LWord FLASH_EraseSector(LWord destination);
LWord FLASH_ProgramLongWord(LWord destination, LWord data);
LWord FLASH_ProgramSectionByLongs(LWord destination, LWord* pSource, LWord size);
LWord FLASH_FlashCommandSequenceStart(Byte index);


#endif



