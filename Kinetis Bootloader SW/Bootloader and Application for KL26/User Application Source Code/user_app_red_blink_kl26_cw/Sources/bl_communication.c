
#include "derivative.h" /* include peripheral declarations */
#include "bl_communication.h"



Byte sci_buffer[8] = {0};  // save the information PC send
Byte frame_length = 0xFF;  // SCI received frame length, initialized as 0xFF
Byte data_length = 0;	

//#define STATION_NUM   0x01  // satation number 
// reset system
#define NVIC_SystemReset()      SCB_AIRCR = SCB_AIRCR_VECTKEY(0x5FA)|\
											SCB_AIRCR_SYSRESETREQ_MASK;

#define AppIDC *(LWord *)(0x20003000 - 8)		
const Byte station_number = 1;
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
	UART0_C2 |= 0x20;    // Enable UART receiver interrupt
}

void UpdateAPP()
{
	
	Byte data_checked = 0;  // check frame end 0xAA 0x55. 1: correct frame ; 0: wrong frame

	if( sci_buffer[0] == '$') 			//check frame header: whether it is '$'
	{
		frame_length = 4 + sci_buffer[3] + 2;  // frame_length = frame head + data + frame end 
	}   	 
	if((sci_buffer[1] == station_number )&& (sci_buffer[frame_length-2] == 0xAA) && (sci_buffer[frame_length-1] == 0x55))
	{
		data_checked = 1; //  Correct frame with its own station number  received.
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

//-----------------------------------------------------------------------------
// FUNCTION:   UART_IRQHandler
// SCOPE:       Applicaiton function
// DESCRIPTION: UART interrupt
//
// PARAMETERS:  none
//
// RETURNS:     none
//-----------------------------------------------------------------------------
void UART0_IRQHandler()
{
	 unsigned char temp;
	       
	 temp = UART_D_REG(UART0_BASE_PTR);
	 if(temp == '$')
	 {
	     buff_index = 0;
	     sci_buffer[buff_index++] = temp;
	 }
	 else
	 {
	      sci_buffer[buff_index++] = temp;
	 }
}



