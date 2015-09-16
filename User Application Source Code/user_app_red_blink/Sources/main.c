/*
 * main implementation: use this 'C' sample to create your own application
 *
 */

/* blink green light*/
#include "derivative.h" /* include peripheral declarations */
#include "bl_communication.h"

#define EnableInterrupts  asm(" CPSIE i");
#define DisableInterrupts asm(" CPSID i");

// enable GPIO clock
#define INIT_ALL_GPIO_CLK    SIM_SCGC5 = SIM_SCGC5_PORTA_MASK |\
                                         SIM_SCGC5_PORTB_MASK | \
                                         SIM_SCGC5_PORTC_MASK | \
                                         SIM_SCGC5_PORTD_MASK | \
                                         SIM_SCGC5_PORTE_MASK   
// init PTE31 to gpio 
#define APP_LED_INIT         PORTE_PCR29 = PORT_PCR_MUX(0x1); \
							 GPIOE_PDDR = 0x020000000; \
							 GPIOE_PCOR = 0x020000000 

void enable_irq (Word irq);
void init_PIT(void);
void flash_protect();

__attribute__((section(".appIndicator"))) LWord AppIDC; //-- need add to APP of uers - 1 

int main(void)
{   	
	EnableInterrupts;			// enable interrupt 
	
	INIT_ALL_GPIO_CLK;
	
	flash_protect();           // protect the entire flash 
	
	INIT_CLOCKS_TO_MODULES;    // init clock module	-- need add to APP of uers - 2 
	UART_Initialization();     // init UART module -- need add to APP of uers - 3 

	APP_LED_INIT;      
	init_PIT();				   // init timer 
	enable_irq(22);            // enable timer interrupt 

	while(1)
	{
		UpdateAPP();         // update user's application -- need add to the app of user's - 4
	}
}

//-----------------------------------------------------------------------------
// FUNCTION:    init_PIT
// SCOPE:       Applicaiton function
// DESCRIPTION: Initialize the PIT  module 
//              
// PARAMETERS:  none
//              
// RETURNS:     none
//-----------------------------------------------------------------------------  
void init_PIT(void)
{
	SIM_SCGC6=SIM_SCGC6|0x00800000; //enable PIT clock 
	PIT_MCR = 0x00;                 // turn on PIT
	PIT_LDVAL1 = 0x003FFFFF;        // setup timer so that the LED have enough time to flash
	PIT_TCTRL1 = PIT_TCTRL1|0x02;   // enable Timer 1 interrupts
	PIT_TCTRL1 |= 0x01;             // start Timer 1
}

//-----------------------------------------------------------------------------
// FUNCTION:    PIT_IRQHandler
// SCOPE:       Applicaiton function
// DESCRIPTION: PIT interrupt  
//              
// PARAMETERS:  none
//              
// RETURNS:     none
//-----------------------------------------------------------------------------  
void PIT_IRQHandler()
{
	GPIOE_PTOR = 0x020000000;
	PIT_TFLG1=0x01;
	PIT_TCTRL1; //dummy read the PIT1 control reg to enable another interput
}

//-----------------------------------------------------------------------------
// FUNCTION:    enable_irq
// SCOPE:       Applicaiton function
// DESCRIPTION: enalbe interrupt 
//              
// PARAMETERS:  irq number
//              
// RETURNS:     none
//-----------------------------------------------------------------------------  
void enable_irq (Word irq)
{   
    /* Make sure that the IRQ is an allowable number. Up to 32 is 
     * used.
     *
     * NOTE: If you are using the interrupt definitions from the header
     * file, you MUST SUBTRACT 16!!!
     */
    if (irq > 32)
       asm("nop");
    else
    {
      /* Set the ICPR and ISER registers accordingly */
      NVIC_ICPR |= 1 << (irq%32);
      NVIC_ISER |= 1 << (irq%32);
    }
}
//-----------------------------------------------------------------------------
// FUNCTION:    flash_protect
// SCOPE:       Applicaiton function
// DESCRIPTION: protected flash regions from program and erase operations 
//              
// PARAMETERS:  none
//              
// RETURNS:     none
//----------------------------------------------------------------------------- 
void flash_protect()
{
	FTFA_FPROT0 = 0x00;
	FTFA_FPROT1 = 0x00;
	FTFA_FPROT2 = 0x00;
	FTFA_FPROT3 = 0x00;

}
