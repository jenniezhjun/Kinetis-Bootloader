/****************************************************************************
* (c) Copyright 2011, Freescale Semiconductor Inc.
 * ALL RIGHTS RESERVED.
 * rs232.h
 *         
 ******************************************************************************/
#ifndef _RS232_H
	#define _RS232_H

//#include "kinetis_params.h"

void UART_Initialization(void);
#define UART_Deinitialization() UART_C2_REG(BOOT_UART_BASE) = 0;
void UART_PutChar(unsigned char data);
unsigned char UART_GetChar(void);

#define UART_IsChar() (UART_S1_REG(BOOT_UART_BASE) & UART_S1_RDRF_MASK)
        
#endif
