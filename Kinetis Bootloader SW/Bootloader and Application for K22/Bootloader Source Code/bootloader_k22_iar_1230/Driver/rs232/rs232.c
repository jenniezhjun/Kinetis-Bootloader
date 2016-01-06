/*****************************************************************************
 * (c) Copyright 2011, Freescale Semiconductor Inc.
 * ALL RIGHTS RESERVED.
 ***************************************************************************//*!
 * @file      rs232.c
 *       
 ******************************************************************************/

//#include "kinetis_params.h"
#include "FRDM_K22F_cfg.h"
#include "rs232.h"
#include "bootloader.h"


/**************************************************************//*!
* UART Initialization  
* UART0 clock source(baud clock) is decided by SIM_SOPT2[PLLFLLSEL] SIM_SOPT2[UART0SRC]
* UART0 baudrate = baud clock/((OSR+1) �BR). Here baud clock source is MCGFLLCLK or MCGPLLCLK/2
* UART1, UART2 clock source is Bus clock
******************************************************************/
void UART_Initialization(void)
{
	  UART_BDH_REG(BOOT_UART_BASE)     = ((UART_SBR>>8)&0x1f);
	  UART_BDL_REG(BOOT_UART_BASE)     = (UART_SBR&0xff);

	#if (defined(KINETIS_K) || defined(KINETIS_V))
	  UART_C4_REG(BOOT_UART_BASE)      = (UART_BRFA&0x1f);
	#endif
	  UART_C2_REG(BOOT_UART_BASE)      = UART_C2_TE_MASK|UART_C2_RE_MASK;

	  while(UART_IsChar())
	    (void)UART_GetChar();
}   

/**************************************************************//*!
* Function for sending one character   
******************************************************************/
void UART_PutChar(unsigned char data)
{
  while((UART_S1_REG(BOOT_UART_BASE)&UART_S1_TC_MASK) == 0){};
  UART_D_REG(BOOT_UART_BASE) = data;
}

/**************************************************************//*!
* Function for receiving of one character  
******************************************************************/
unsigned char UART_GetChar(void)
{
  unsigned char ret = 0;
  volatile unsigned long timeout_counter = BOOT_CORE_CLOCK/100;

  while(UART_IsChar() == 0 && timeout_counter --)
  {

#if BOOTLOADER_INT_WATCHDOG == 1
    WDG_Refresh(); /* feeds the dog */
#endif
		
  };
  ret = UART_D_REG(BOOT_UART_BASE);
  return ret;
}

