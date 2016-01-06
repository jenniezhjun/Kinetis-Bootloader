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
#define INIT_CLOCKS_TO_MODULES    SIM_SOPT2 = SIM_SOPT2_UART0SRC(1);\
                                  SIM_SCGC4 |= (SIM_SCGC4_UART0_MASK | SIM_SCGC4_UART1_MASK | SIM_SCGC4_UART2_MASK); \
                                  SIM_SCGC5 |= 0xffffffff; \
                                  SIM_SCGC6 |= SIM_SCGC6_FTF_MASK;
#define PIN_INIT_AS_UART	PORT_PCR_REG(PORTA_BASE_PTR, 1) = PORT_PCR_MUX(2);\
							PORT_PCR_REG(PORTA_BASE_PTR, 2) = PORT_PCR_MUX(2);		  
							 
#define UART_IsChar() (UART_S1_REG(UART0_BASE_PTR) & UART_S1_RDRF_MASK)

#define PIN_INIT_AS_UART0	PORT_PCR_REG(PORTE_BASE_PTR, 20) = PORT_PCR_MUX(4);\
							PORT_PCR_REG(PORTE_BASE_PTR, 21) = PORT_PCR_MUX(4);	
// end UART

extern Byte sci_buffer[];  // save the information PC send

// functions of updateapp.c
void UART_Initialization(void);
unsigned char UART_GetChar(void);
void UART_PutChar(unsigned char data);
void UpdateAPP();
//
// interrupt
#define EnableInterrupts asm(" CPSIE i");
#define DisableInterrupts asm(" CPSID i");
extern Byte buff_index ;   // receive buffer index for sci_buffer[]
//
#endif /* UPDATEAPP_H_ */
