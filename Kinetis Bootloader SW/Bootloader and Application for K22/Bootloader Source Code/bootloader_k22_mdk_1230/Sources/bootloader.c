/*
 * main implementation: use this 'C' sample to create your own application
 *
 */


#include "MK22F51212.h" /* include peripheral declarations */
#include "core_cm4.h"
#include "bootloader.h"
#include "rs232.h" 
#include "flash_kinetis.h"
#include "FRDM_K22F_cfg.h"



const Byte str_app_ok[8]	= "APP_OK";
const Byte station_number = 1;
typedef union Address
 {
   unsigned long complete;
   struct
   {
     unsigned short low;
     unsigned short high;
   }Words;
   struct
   {
     unsigned char ll;
     unsigned char lh;
     unsigned char hl;
     unsigned char hh;
   }Bytes;
 }AddressType;
AddressType address;
 
typedef struct IDENT_DATA
{
		Byte targ_core;
		Byte targ_name[10];	                                 // target name
		Byte Boot_version[10];	                             // Boot_version
		Word erblk;			                                 // erase block size
		Word wrblk;			                                 // write block size
		AddressType addr_limit;								 // MCU maximal FLASH address
		AddressType relocated_vectors;						 // relocated_vectors address
		AddressType dontcare_addrl;                          // when do verification, do not compare the contents from
		AddressType dontcare_addrh;                          // dontcare_addrl to dontcare_addrh
}IdentType;
__attribute__((section(".init")))const IdentType MCU_Identification = {BL_M4, "MK22FN512","1.0" ,0x800, 0X80, 0x080000, 0x004000, 0x0003FC, 0x0003FF};


Byte sci_buffer[FRAME_BUFF_LENGTH] ;  // for both UART rx and tx buffer
 
 __attribute__((section(".cfmconfig"))) const FlashConfig_t Config __attribute__((used)) =
{
 {0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFE, 0xFFFFFBFE}
};

Byte Boot_StrCompare(const Byte *s1, const Byte *s2,  LWord len);
void Boot_Send(Byte tx_buffer[]);
void Boot_Button_Init(void);
void Boot_Init_Clock(void);
static void Boot_ReadAddress(void);
void JumpToUserApplication(LWord userSP, LWord userStartup);
int  main(void);

//-----------------------------------------------------------------------------
// FUNCTION:    WDG_Disable  WDG_Enable  WDG_Refresh
// SCOPE:       Bootloader application system function
// DESCRIPTION: Watchdog operation             
// RETURNS:     function never go back
//-----------------------------------------------------------------------------  
void WDG_Disable(void)
{
    WDOG->UNLOCK = WDOG_UNLOCK_WDOGUNLOCK(0xC520);
    WDOG->UNLOCK = WDOG_UNLOCK_WDOGUNLOCK(0xD928);
    WDOG->STCTRLH &= ~WDOG_STCTRLH_WDOGEN_MASK;         // Disable watchdog 
}

void WDG_Enable(void)
{
    WDOG->UNLOCK = WDOG_UNLOCK_WDOGUNLOCK(0xC520);
    WDOG->UNLOCK = WDOG_UNLOCK_WDOGUNLOCK(0xD928);
    WDOG->STCTRLH |= WDOG_STCTRLH_WDOGEN_MASK;         //enable watchdog
}

void WDG_Refresh(void)
{
    WDOG->REFRESH = WDOG_REFRESH_WDOGREFRESH(0xA602);
    WDOG->REFRESH = WDOG_REFRESH_WDOGREFRESH(0xB480);  // Feed watchdog
}

//-----------------------------------------------------------------------------
// FUNCTION:    BootButton_Init
// SCOPE:       Bootloader application system function
// DESCRIPTION: press boot button SW1 on FRDM_KL26Z board to enter boot mode             
// RETURNS:     function never go back
//-----------------------------------------------------------------------------  
void Boot_Button_Init(void)
{
    //init SW3 botton
    PORTB_PCR17=(0|PORT_PCR_MUX(1)|PORT_PCR_PE_MASK | PORT_PCR_PS_MASK);  //Pin Control Register
    GPIOB_PDDR &= ~GPIO_PDDR_PDD(GPIO_PIN(17)); //Port C Data Direction Register

}

//-----------------------------------------------------------------------------
// FUNCTION:    StrCompare
// SCOPE:       Bootloader application system function
// DESCRIPTION: compare if the two strings are same          
// RETURNS:     CHECK_OK - 1: two strings are  same
//				CHECK_FAIL - 0: two strings are  different
//-----------------------------------------------------------------------------
Byte Boot_StrCompare(const Byte *s1, const Byte *s2,  LWord len)
{
    while(len)
    {
        if(*s1++ ==*s2++ )
                len--;
        else
                return CHECK_FAIL ;
    }
    return CHECK_OK;
}


//-----------------------------------------------------------------------------
// FUNCTION:    JumpToUserApplication
// SCOPE:       Bootloader application system function
// DESCRIPTION: The function startup user application
//              
// PARAMETERS:  pointer on user vector table
//              
// RETURNS:     function never go back
//-----------------------------------------------------------------------------  
/*
void JumpToUserApplication(LWord userSP, LWord userStartup)
{
    volatile LWord avoid_optimization;
    avoid_optimization = userSP;       //In order to avoid optimization issue when -Os
    avoid_optimization = userStartup;  //In order to avoid optimization issue when -Os

          // set up stack pointer
    __asm("msr msp, r0");
    __asm("msr psp, r0");

    // Jump to PC (r1)
    __asm("mov pc, r1"); 
}
*/

//-----------------------------------------------------------------------------
// FUNCTION:    ReadAddress
// SCOPE:       Bootloader application system function  
// DESCRIPTION: The functin reads the 4 bytes from SCI and store it to global address variable
//              
// PARAMETERS:  none
//              
// RETURNS:     none
//----------------------------------------------------------------------------- 

static void Boot_ReadAddress(void)
{
    address.Bytes.hh = 0;
    address.Bytes.hl = sci_buffer[5];
    address.Bytes.lh = sci_buffer[6];
    address.Bytes.ll = sci_buffer[7];	 	 
}
//-----------------------------------------------------------------------------
// FUNCTION:    Boot_Send
// SCOPE:       Bootloader application system function  
// DESCRIPTION: send tx frame to PC
//              
// PARAMETERS:  send out buffer
//              
// RETURNS:     none
//----------------------------------------------------------------------------- 

void Boot_Send(Byte tx_buffer[])
{
    Byte data_length,frame_length, tx_index=0;

    data_length = tx_buffer[3];

    tx_buffer[data_length+4] = 0xAA;
    tx_buffer[data_length+5] = 0x55;

    frame_length = 4 + data_length + 2;

    while(frame_length)
    {
            UART_PutChar(tx_buffer[tx_index]);
            frame_length --;
            tx_index++;		
    }	
}
//-----------------------------------------------------------------------------
// FUNCTION:    boot_init_clock
// SCOPE:       Bootloader application system function
// DESCRIPTION: Init the sytem clock. Here it uses PEE with external 8M crystal, Core clock = 48MHz, Bus clock = 24MHz
//-----------------------------------------------------------------------------
void Boot_Init_Clock()
{
    SIM_CLKDIV1 = SIM_CLKDIV1_OUTDIV1(0x00) |
                  SIM_CLKDIV1_OUTDIV2(0x01) |
                  SIM_CLKDIV1_OUTDIV3(0x04) |
                  SIM_CLKDIV1_OUTDIV4(0x04); /* Set the system prescalers to safe value */
    SIM_SCGC5 |= (uint32_t)SIM_SCGC5_PORTA_MASK; /* Enable EXTAL/XTAL pins clock gate */
    PORTA_PCR18 &= (uint32_t)~(uint32_t)(
                PORT_PCR_ISF_MASK |
                PORT_PCR_MUX(0x07)
               );
    /* Is external crystal/resonator used in targeted clock configuration? */ /* If yes, initialize also XTAL pin routing */
    /* PORTA_PCR19: ISF=0,MUX=0 */
    PORTA_PCR19 &= (uint32_t)~(uint32_t)(
         PORT_PCR_ISF_MASK |
         PORT_PCR_MUX(0x07)
         );
    MCG_C2 = 0xa4;
    OSC_CR = 0x00; /* Set OSC_CR (OSCERCLK enable, oscillator capacitor load) */
    MCG_C7 = 0x00; /* Select MCG OSC clock source */
    MCG_C1 = 0x98; /* Set C1 (clock source selection, FLL ext. reference divider, int. reference enable etc.) */
    while((MCG_S & MCG_S_OSCINIT0_MASK) == 0x00U) { /* Check that the oscillator is running */
      }
    while((MCG_S & MCG_S_IREFST_MASK) != 0x00U) { /* Check that the source of the FLL reference clock is the external reference clock. */
    }
    MCG_C4 = 0x17; /* Set C4 (FLL output; trim values not changed) */
    MCG_C5 = 0x00; /* Set C5 (PLL settings, PLL reference divider etc.) */
    MCG_C6 = 0x00; /* Set C6 (PLL select, VCO divider etc.) */
    while((MCG_S & 0x0CU) != 0x08U) { /* Wait until external reference clock is selected as MCG output */
    }
    OSC_CR = 0x80; /* Set OSC_CR (OSCERCLK enable, oscillator capacitor load) */
    MCG_C7 = 0x00; /* Select MCG OSC clock source */
    MCG_C1 = 0x9a; /* Set C1 (clock source selection, FLL ext. reference divider, int. reference enable etc.) */
    MCG_C2 = 0x24; /* Set C2 (freq. range, ext. and int. reference selection etc.; trim values not changed) */
    MCG_C5 = 0x23; /* Set C5 (PLL settings, PLL reference divider etc.) */
    MCG_C6 = 0x40; /* Set C6 (PLL select, VCO divider etc.) */
    while((MCG_S & MCG_S_LOCK0_MASK) == 0x00U) { /* Wait until PLL is locked*/
    }
    while((MCG_S & 0x0CU) != 0x08U) { /* Wait until external reference clock is selected as MCG output */
    }
    OSC_CR = 0x80; /* Set OSC_CR (OSCERCLK enable, oscillator capacitor load) */
    MCG_C7 = 0x00; /* Select MCG OSC clock source */
    MCG_C1 = 0x1a;
    MCG_C5 = 0x23;
    MCG_C6 = 0x40;
    while((MCG_S & MCG_S_LOCK0_MASK) == 0x00U) { /* Wait until PLL is locked*/
    }
    while((MCG_S & 0x0CU) != 0x0CU) { /* Wait until output of the PLL is selected */
    }
    //while (NextMode != (TargetMode & CPU_MCG_MODE_INDEX_MASK)); /* Loop until the target MCG mode is set */
    //	SIM_CLKDIV1 = 0x11100000;
    //	SIM_SOPT1 = 0x800C9010;
    //	SIM_SOPT2 = 0x00011000;
}


//-----------------------------------------------------------------------------
// FUNCTION:    main
// SCOPE:       Bootloader application system function
// DESCRIPTION: bootloader main function             
// RETURNS:     0
//----------------------------------------------------------------------------- 
int main(void)
{	
    volatile unsigned long loop_counter = 0;    // counter of loop without received frame

    Byte enableBootMode = 0; 			//  0: enter APP, 1: enter BOOT mode, 2: enter verify mode
    //Byte s19buffer[WR_BLOCK_BYTE] = {1};        //extract app file burning code from received frame, store them to s19buffer[]

    DisableInterrupts;


    INIT_CLOCKS_TO_MODULES;			// init clock module
    Boot_Button_Init();  			// init SW3 button (PTC3 port) on FRDM_K22 board
    INIT_BOOT_LED;
    //  condition for entering boot:
    if(((*(unsigned long*)(RELOCATED_VECTORS + 8)) == 0xffffffff)                          //1. no valid code in APP vector section,
    || Boot_StrCompare(( Byte*)APPOK_START_ADDRESS, str_app_ok, APPOK_LENGTH) == CHECK_FAIL	   //2."APP_OK" is wrong in address APPOK_START_ADDRESS.
    || ((GPIO_PDIR_REG(BOOT_PIN_ENABLE_GPIO_BASE) & (1 << BOOT_PIN_ENABLE_NUM)) == 0)      //3. SW1 is pressed
    || (AppIDC == 0x0000000B))                                                             //4. App request boot
    {
        enableBootMode = 1; // enable boot
        BOOT_LED_ON;
        AppIDC = 0;
    }
    else if (AppIDC == 0x0000000A)   // App request verify
    {
        enableBootMode = 2; // enable verify mode
        BOOT_LED_ON;
        AppIDC = 0;
    }
    if(enableBootMode)
    {
    // if use external clock
#ifdef USE_EXTERNAL_CLOCK
        Boot_Init_Clock();
#endif
        PIN_INIT_AS_UART;   			// set PTEA1/UART1_RX and PTE0/UART1_TX pins as UART
        UART_Initialization();  		// init UART module
    }   
    ////////////////////////////////////////////////////////////////////////
    //BOOT Model
    ////////////////////////////////////////////////////////////////////////
    while(enableBootMode)  // enter boot or verify mode, execute all command from GUI
    {
        volatile Byte frame_start_flag  = 0;    // if frame header is received. 1: receive $, start to receive frame;  0: no $ received
        volatile Byte frame_length 	= 0xFF;    // SCI received frame length, initialized as 0xFF
        volatile Byte data_length = 0;
        volatile Byte buff_index = 1;   	   // receive buffer index for sci_buffer[]
        volatile Byte data_checked  = 0; 	   // check frame end 0xAA 0x55. 1: correct frame ; 0: wrong frame
				WDG_Refresh();
        FLASH_Initialization();

        sci_buffer[0] = UART_GetChar();
        if( sci_buffer[0] == '$') 			//check frame header: whether it is '$'
        {
            loop_counter = 0;
            frame_start_flag  = 1;
        }

        if(frame_start_flag == 1)
        {
            sci_buffer[1] = UART_GetChar();     // sci_buffer[1] is the station number
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

        if((sci_buffer[frame_length-2] == 0xAA) && (sci_buffer[frame_length-1] == 0x55)) //Check if Frame end is correct
        {
            data_checked = 1; //  Correct frame received.
        }

       // all the data in frame was correctly received (data_checked = 1) above, now perform frame analysis below.
       // sci_buffer[] now is switched to tx buffer
        if(data_checked == 1)
        {
            Byte i = 0;
            Byte j = 0;
            Byte burn_data_length = 0;
            Byte s19buffer[WR_BLOCK_BYTE]; //extract app file burning code from received frame, store them to s19buffer[]

            //WDG_Refresh();
            switch(sci_buffer[4])
            {
                case 'I':	// receive 'I' command, send  version information to UART
														i = 4;
														while((sci_buffer[i] = MCU_Identification.targ_name[i-4]) !='\0')	//assign chip part number info to sci_buffer[]
														{
															i++;
														}
														sci_buffer[i++] = '#';
														j = i;
														while((sci_buffer[i] = MCU_Identification.Boot_version[i-j]) !='\0')	//assign bootloader version info to sci_buffer[]
														{
															i++;
														}
														sci_buffer[i++] = '#';
														sci_buffer[i++] = (MCU_Identification.wrblk & 0xFF00)>>8;		//assign write block size
														sci_buffer[i++] = MCU_Identification.wrblk & 0x00FF;

														sci_buffer[i++] = (MCU_Identification.erblk & 0xFF00)>>8;		//assign erase block size
														sci_buffer[i++] = MCU_Identification.erblk & 0x00FF;

														sci_buffer[i++] = MCU_Identification.addr_limit.Bytes.hl;		 //assign MCU Flash end address
														sci_buffer[i++] = MCU_Identification.addr_limit.Bytes.lh;
														sci_buffer[i++] = MCU_Identification.addr_limit.Bytes.ll;

														sci_buffer[i++] = MCU_Identification.dontcare_addrl.Bytes.hl;		//assign bootloader dont care memory start adddress
														sci_buffer[i++] = MCU_Identification.dontcare_addrl.Bytes.lh;
														sci_buffer[i++] = MCU_Identification.dontcare_addrl.Bytes.ll;

														sci_buffer[i++] = MCU_Identification.dontcare_addrh.Bytes.hl;		//assign bootloader dont care memory end adddress
														sci_buffer[i++] = MCU_Identification.dontcare_addrh.Bytes.lh;
														sci_buffer[i++] = MCU_Identification.dontcare_addrh.Bytes.ll;

														sci_buffer[i++] = MCU_Identification.relocated_vectors.Bytes.hl;		//assign bootloader start adddress
														sci_buffer[i++] = MCU_Identification.relocated_vectors.Bytes.lh;
														sci_buffer[i++] = MCU_Identification.relocated_vectors.Bytes.ll;

														sci_buffer[i++] = MCU_Identification.targ_core; 	// target is M4 core
                            sci_buffer[3] = i - 4;   // tx data length in one frame 
                            Boot_Send(sci_buffer);
                            if(enableBootMode == 1)  // if in boot mode
                                FLASH_EraseSector((LWord)APPOK_START_ADDRESS);	//erase APP_OK
														WDG_Refresh();
                            break;

                case 'E':	// receive 'E' command, erase sector, then send confirm frame to UART
                            Boot_ReadAddress();
                            if(!( FLASH_EraseSector(address.complete)))
                            {
                                sci_buffer[3] = 00;
                                Boot_Send(sci_buffer);
                            }
														WDG_Refresh();
                            break;

                case'W':	// receive 'W' command, extract app  burning code,  program flash. then send confirm frame to UART
                            Boot_ReadAddress();
                            burn_data_length = sci_buffer[8];
                            for(j=0,i=9;j<burn_data_length;j++,i++) // extract the prepared writing data from sci_buff[] to S19buffer[]
                            {
                                s19buffer[j] = sci_buffer[i];
                            }
                            if(!(FLASH_ProgramSectionByLongs (address.complete, (LWord*)s19buffer, burn_data_length/4)))
                            {
                                sci_buffer[3] = 00;
                                Boot_Send(sci_buffer);
                                
                            }
														WDG_Refresh();
                            break;
                case'R':	// receive 'R' command, read out the memory data, then send the data in frame to UART
                            Boot_ReadAddress();
                            burn_data_length = sci_buffer[8];
                            for(i=0;i<burn_data_length;i++)
                            {
                                sci_buffer[i + 4] = ((Byte*)(address.complete))[i];
                            }
                            sci_buffer[3] = burn_data_length;
                            Boot_Send(sci_buffer);
														WDG_Refresh();
                            break;

                case'G':	// receive 'G' command, go to app.
                            if(enableBootMode == 1)
                            FLASH_ProgramSectionByLongs ((LWord)APPOK_START_ADDRESS, (LWord*)str_app_ok, APPOK_LENGTH/4);
                            BOOT_LED_OFF;
                            NVIC_SystemReset();
                            break;

                default :
                            break;
            }// end switch(sci_buffer[4])
            frame_start_flag  = 0;
            for (buff_index = 0; buff_index<FRAME_BUFF_LENGTH; buff_index++)	// clear sci_buffer[]
                sci_buffer[buff_index] = 0;

          }// end if(data_checked == 1)
          loop_counter ++;
          if(loop_counter >= 10 && enableBootMode == 2)                         // 100 * timeout of UART_GetChar();
          {
              NVIC_SystemReset();
          }
    }// end while(enableBootMode)

    // deinitialization of used modules
    UART_Deinitialization();
    DEINIT_BOOT_LED;
    DEINIT_CLOCKS_TO_MODULES;

    // relocate vector table
    SCB_VTOR = RELOCATED_VECTORS;
    AppIDC = 0;
    // Jump to user application
    JumpToUserApplication(*((unsigned long*)RELOCATED_VECTORS), *((unsigned long*)(RELOCATED_VECTORS+4)));
    return 0;
}

