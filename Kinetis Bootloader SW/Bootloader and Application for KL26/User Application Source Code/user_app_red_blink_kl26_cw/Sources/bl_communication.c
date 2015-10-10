
#include "derivative.h" /* include peripheral declarations */
#include "bl_communication.h"



Byte sci_buffer[8] = {0};  // save the information PC send
Byte frame_length = 0xFF;  // SCI received frame length, initialized as 0xFF
Byte data_length = 0;	

#define STATION_NUM   0x01  // satation number 
// reset system
#define NVIC_SystemReset()      SCB_AIRCR = SCB_AIRCR_VECTKEY(0x5FA)|\
											SCB_AIRCR_SYSRESETREQ_MASK;

#define AppIDC *(LWord *)(0x20003000 - 8)							
/**************************************************************//*!
* UART Initialization  
******************************************************************/
void UART_Initialization(void)
{
	PIN_INIT_AS_UART; 
	
	UART_BDH_REG(UART0_BASE_PTR)     = ((UART_SBR>>8)&0x1f);
	UART_BDL_REG(UART0_BASE_PTR)     = (UART_SBR&0xff);
							   
	#if (defined(KINETIS_K) || defined(KINETIS_V))
	UART_C4_REG(UART0_BASE_PTR)      = (UART_BRFA&0x1f);  
	#endif
	UART_C2_REG(UART0_BASE_PTR)      = UART_C2_TE_MASK|UART_C2_RE_MASK;

	while(UART_IsChar())
		(void)UART_GetChar();
}
/**************************************************************//*!
* Function for receiving of one character  
******************************************************************/
Byte UART_GetChar(void)
{
  Byte ret = 0;
  while(UART_IsChar() == 0){
		};
  ret = UART_D_REG(UART0_BASE_PTR);
  return ret;
}
/**************************************************************//*!
* Function for sending one character   
******************************************************************/
void UART_PutChar(Byte data)
{
  while((UART_S1_REG(UART0_BASE_PTR)&UART_S1_TC_MASK) == 0){};
  UART_D_REG(UART0_BASE_PTR) = data;
}

void UpdateAPP()
{
	Byte frame_start_flag  = 0;  // if frame header is received. 1: receive $, start to receive frame;  0: no $ received 	
	Byte data_checked = 0;  // check frame end 0xAA 0x55. 1: correct frame ; 0: wrong frame
	Byte buff_index = 1;   	   // receive buffer index for sci_buffer[]

	sci_buffer[0] = UART_GetChar();
	if( sci_buffer[0] == '$') 			//check frame header: whether it is '$'
	{
		frame_start_flag  = 1;
	}   	 
	if(frame_start_flag == 1)
	{
		sci_buffer[1] = UART_GetChar();     // sci_buffer[1] is the station number
		if(sci_buffer[1] != STATION_NUM)    // if not the STATION_NUM, continue the APP, do not update 
		{
			return;		
		}
		UART_GetChar();   					// sci_buffer[2] is reserved
		sci_buffer[3] = UART_GetChar();     // sci_buffer[3] is the Data length
		data_length = sci_buffer[3];
		frame_length = 4 + sci_buffer[3] + 2;  // frame_length = frame head + data + frame end 
		buff_index = 4;
		while(data_length --)
			sci_buffer[buff_index ++] = UART_GetChar();
		sci_buffer[buff_index ++] = UART_GetChar();  // frame end. It should be 0xAA
		sci_buffer[buff_index ++] = UART_GetChar();  // frame end. It should be 0x55
		frame_start_flag = 0;
	}	
	if((sci_buffer[frame_length-2] == 0xAA) && (sci_buffer[frame_length-1] == 0x55))
	{
		data_checked = 1; //  Correct frame received. 
	}
	//all the data in frame was correctly received (data_checked = 1) above, now perform frame analysis below.
	if(data_checked == 1)
	{
		if((sci_buffer[4] == 'B')||(sci_buffer[4] == 'V')) //if get 'B' or 'V'command, reset system 
			
		{	
			DisableInterrupts;		//disable interrupt 	
			if(sci_buffer[4] == 'B')
				AppIDC = 0x0000000B;    // appIndicator: app->bootloader
			if(sci_buffer[4] == 'V')
				AppIDC = 0x0000000A;    // appIndicator: app->bootloader
			NVIC_SystemReset();    // System reset   
		}
			
	}
}





