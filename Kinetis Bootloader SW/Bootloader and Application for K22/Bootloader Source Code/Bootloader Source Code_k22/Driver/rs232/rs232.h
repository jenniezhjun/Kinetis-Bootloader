/*****************************************************************************
 * (c) Copyright 2011, Freescale Semiconductor Inc.
 * ALL RIGHTS RESERVED.
 ***************************************************************************//*!
 * @file      rs232.h
 * @author    R20253
 * @version   1.0.9.0
 * @date      Dec-11-2012
 * @brief     RS232 driver header file
 * @par       
 * @include   
 * @par       
 * @include         
 ******************************************************************************/
#ifndef _RS232_H
	#define _RS232_H
#include "system_MK22F51212.h"

#include "bootloader.h"
	//#include "kinetis_params.h"
   
#ifdef LPUART_USED

	#define LPUART_SBR  (unsigned char)(BOOT_BUS_CLOCK / (16*BOOT_UART_BAUD_RATE))

	//  API
	void UART_Initialization(void);
	#define UART_Deinitialization() LPUART_CTRL_REG(BOOT_UART_BASE) = 0
	void UART_PutChar(unsigned char data);
	unsigned char UART_GetChar(void);
	#define UART_IsChar() (LPUART_STAT_REG(BOOT_UART_BASE) & LPUART_STAT_RDRF_MASK)   
   
#else

	//#define UART_SBR  (unsigned char)(BOOT_BUS_CLOCK / (16*BOOT_UART_BAUD_RATE))
	//#define UART_BRFA (unsigned char)((((BOOT_BUS_CLOCK/(16*BOOT_UART_BAUD_RATE))- \
                          UART_SBR)*32.0)+0.5)
//#ifdef USE_EXTERNAL_CLOCK
////#define UART_SBR  (BOOT_CORE_CLOCK / (16*BOOT_UART_BAUD_RATE))
//#define UART_BRFA  ((BOOT_CORE_CLOCK  /BOOT_UART_BAUD_RATE)-16*UART_SBR )*32

//#else

#define UART_SBR  (BOOT_CORE_CLOCK / (16*BOOT_UART_BAUD_RATE))
#define UART_BRFA  ((BOOT_CORE_CLOCK   /BOOT_UART_BAUD_RATE)-16*UART_SBR )*32
#endif
	//  API
	void UART_Initialization(void);
	#define UART_Deinitialization() UART_C2_REG(BOOT_UART_BASE) = 0
	void UART_PutChar(unsigned char data);
	unsigned char UART_GetChar(void);
	//unsigned char UART_GetChar1(void);
	//unsigned char UART_GetChar2(void);
	
	// 07.03.2015
	//unsigned char UART_GetBlock2(unsigned char* getpc, unsigned int size);
	////////////////////////////////////////////////////////////////////
	
	#define UART_IsChar() (UART_S1_REG(BOOT_UART_BASE) & UART_S1_RDRF_MASK)
#endif
