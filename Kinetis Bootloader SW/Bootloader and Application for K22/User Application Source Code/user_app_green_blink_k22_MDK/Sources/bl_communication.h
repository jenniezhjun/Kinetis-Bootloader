/*
 * updateapp.h
 *
 *  Created on: Aug 15, 2015
 *      Author: B51432
 */

#ifndef UPDATEAPP_H_
#define UPDATEAPP_H_



typedef unsigned char   	Byte;  		//1 byte
typedef unsigned short  	Word;  		//2 byte
typedef unsigned int	   	LWord; 		//4 byte
typedef unsigned long long 	DLWord; 	//8 Byte

// UART 
#define BUS_CLOCK        (32768*640)
#define UART_BAUD_RATE   9600 
#define UART_SBR  (unsigned char)(BUS_CLOCK / (16*UART_BAUD_RATE))

#define INIT_CLOCKS_TO_MODULES    SIM_SCGC4 |= SIM_SCGC4_UART1_MASK ; \
                                  SIM_SCGC5 |= 0xffffffff; \
                                  SIM_SCGC6 |= SIM_SCGC6_FTF_MASK;

							 
#define UART_IsChar() (UART_S1_REG(UART1_BASE_PTR) & UART_S1_RDRF_MASK)
#define PIN_INIT_AS_UART	PORT_PCR_REG(PORTE_BASE_PTR, 1) = PORT_PCR_MUX(3);\
							PORT_PCR_REG(PORTE_BASE_PTR, 0) = PORT_PCR_MUX(3);
// end UART

// functions of updateapp.c
void UART_Initialization(void);
void UpdateAPP(void);
//
// interrupt
#define EnableInterrupts __asm(" CPSIE i");
#define DisableInterrupts __asm(" CPSID i");
extern unsigned int AppIDC;
extern Byte sci_buffer[];
extern Byte get_uart;
extern Byte buff_index;
//
#endif /* UPDATEAPP_H_ */
