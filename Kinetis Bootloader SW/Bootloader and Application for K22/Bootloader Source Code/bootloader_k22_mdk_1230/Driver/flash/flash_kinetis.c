/*****************************************************************************
 * (c) Copyright 2011, Freescale Semiconductor Inc.
 * ALL RIGHTS RESERVED.
 ***************************************************************************//*!
 * @file      flash_kinetis.c
 * @author    R20253
 * @version   1.0.17.0
 * @date      Feb-15-2013
 * @brief     Flash programming driver
 * @par       
 * @include   
 * @par       
 * @include         
 ******************************************************************************/

#include "bootloader.h"
#include "flash_kinetis.h"
#include "FRDM_K22F_cfg.h"
Byte buffer[200]={0};
FCC0B_STR CommandObj;

#define FLASH_FlashCommandSequence ((LWord (*)(Byte))&buffer[1])

extern LWord FLASH_FlashCommandSequenceStart(Byte index);

/********************************************************
* Init Function 
*
********************************************************/
void FLASH_Initialization(void)
{
  LWord i;
  for(i=0;i<200;i++)
     buffer[i] = ((Byte*)FLASH_FlashCommandSequenceStart)[i-1];
}


/********************************************************
* Function for Programming of one Long Word 
*
********************************************************/
LWord FLASH_ProgramLongWord(LWord destination, LWord data32b)
{
  /* preparing passing parameter to program the flash block */
  
  CommandObj.regsLong.fccob3210 = destination;
  CommandObj.regs.fccob0 = FLASH_PROGRAM_LONGWORD;
  CommandObj.regsLong.fccob7654 = data32b; 

#if defined(KINETIS_E)
	return FLASH_FlashCommandSequenceStart(PROGRAM_LONGWORD_INDEX);
#else
	return FLASH_FlashCommandSequence(PROGRAM_LONGWORD_INDEX);
#endif

  
}

/********************************************************
* Function for Programming of section by simple longs
*
********************************************************/
LWord FLASH_ProgramSectionByLongs(LWord destination, LWord* pSource, LWord size)
{ 
  while(size--)
  {
    if(FLASH_ProgramLongWord(destination, *pSource++) != FLASH_OK)
      return FLASH_FAIL;
    destination += 4;
  }
  return FLASH_OK;
}

/********************************************************
* Function for erasing of flash memory sector (0x800)
*
********************************************************/
LWord FLASH_EraseSector(LWord destination)
{  
  CommandObj.regsLong.fccob3210 = destination;
  CommandObj.regs.fccob0 = FLASH_ERASE_SECTOR;

  #if defined(KINETIS_E)
		return FLASH_FlashCommandSequenceStart(ERASE_BLOCK_INDEX);
  #else
		return FLASH_FlashCommandSequence(ERASE_BLOCK_INDEX);
  #endif
  
  
}

LWord FLASH_FlashCommandSequenceStart(Byte index)
{
  Byte* ptrFccobReg = (Byte*)&FLASH_BASE_PTR->FCCOB3;
  Byte* ptrCommandObj = (Byte*)&CommandObj;

  /* wait till CCIF bit is set */
  while(!(FLASH_FSTAT & FLASH_FSTAT_CCIF_MASK)){};
  /* clear RDCOLERR & ACCERR & FPVIOL flag in flash status register */
  FLASH_FSTAT = FLASH_FSTAT_ACCERR_MASK | FLASH_FSTAT_FPVIOL_MASK | FLASH_FSTAT_RDCOLERR_MASK;

  /* load FCCOB registers */
  while(index--)
    *ptrFccobReg++ = *ptrCommandObj++;

  //  launch a command
  FLASH_FSTAT |= FLASH_FSTAT_CCIF_MASK;
  //  waiting for the finishing of the command
  while(!(FLASH_FSTAT & FLASH_FSTAT_CCIF_MASK)){};

   /* Check error bits */
  /* Get flash status register value */
  return (FLASH_FSTAT & (FLASH_FSTAT_ACCERR_MASK | FLASH_FSTAT_FPVIOL_MASK | FLASH_FSTAT_MGSTAT0_MASK));
}



