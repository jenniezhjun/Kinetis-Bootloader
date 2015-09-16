/**************************************************/
/** USER SETTINGS OF KINETIS MCU */
/**  Kinetis ARM Cortex-M0 model */

#include "MKL26Z4.h"

/**************************************************/
#define RELOCATED_VECTORS          0x1000                            // Start address of relocated interrutp vector table
#define AppIDC                     *((LWord*)(0x20003000 - 8))		 // GUI requests program: User app  writes ‘B’ to AppIDC
																	 // GUI requests verify: User app  writes ‘A’ to AppIDC
#define APPOK_START_ADDRESS		   0x1FFF8 

#define APPOK_LENGTH			   8
#define FRAME_BUFF_LENGTH		   80
#define CHECK_OK     			   0
#define CHECK_FAIL   			   1
#define WR_BLOCK_BYTE			   0X40

/**************************************************/
//#define USE_EXTERNAL_CLOCK
#ifdef USE_EXTERNAL_CLOCK
// if use external clock
#define BOOT_CORE_CLOCK            (48000000)
#define BOOT_BUS_CLOCK             (24000000)
#define BOOT_UART_BAUD_RATE        9600                    // tested up to 115200
#else
//if use the default internal clock, FEI mode
#define BOOT_CORE_CLOCK            (32768*640)
#define BOOT_BUS_CLOCK             (32768*640)
#define BOOT_UART_BAUD_RATE        9600                    // up to 57600
#endif

/**************************************************/
#define INIT_CLOCKS_TO_MODULES     SIM_SOPT2 |= SIM_SOPT2_UART0SRC(1);\
                                   SIM_SCGC4 |= (SIM_SCGC4_UART0_MASK | SIM_SCGC4_UART1_MASK | SIM_SCGC4_UART2_MASK); \
                                   SIM_SCGC5 |= 0xffffffff; \
                                   SIM_SCGC6 |= SIM_SCGC6_FTF_MASK;

#define DEINIT_CLOCKS_TO_MODULES   SIM_SOPT2 &= ~SIM_SOPT2_UART0SRC(1);\
                                   SIM_SCGC4 &= ~(SIM_SCGC4_UART0_MASK | SIM_SCGC4_UART1_MASK | SIM_SCGC4_UART2_MASK); \
                                   SIM_SCGC5 &= 0x0; \
                                   // we should keep flash clock on

/**************************************************/
#define BOOT_UART_BASE UART0_BASE_PTR                                // UART used for bootloader. PTA1 is Rx (pullup enable), PTA2 is Tx
#define PIN_INIT_AS_UART	       PORT_PCR_REG(PORTA_BASE_PTR, 1) = PORT_PCR_MUX(2) | PORT_PCR_PE_MASK;\
							       PORT_PCR_REG(PORTA_BASE_PTR, 2) = PORT_PCR_MUX(2);

/**************************************************/
#define BOOT_PIN_ENABLE_PORT_BASE  PORTC_BASE_PTR                    // pin used to trigger bootloader 
#define BOOT_PIN_ENABLE_GPIO_BASE  PTC_BASE_PTR   
#define BOOT_PIN_ENABLE_NUM        3          

#define GPIO_PIN_MASK              0x1Fu
#define GPIO_PIN(x)                (((1)<<(x & GPIO_PIN_MASK)))
                                                                     // pin used for boot mode indicator
#define INIT_BOOT_LED              PORTD_PCR5 |= PORT_PCR_MUX(0x1); \
						           GPIOD_PDDR |= GPIO_PDDR_PDD(0x20); \
                                   GPIOD_PDOR |= 0x000000020

#define DEINIT_BOOT_LED            PORTD_PCR5 &= ~PORT_PCR_MUX(0x1); \
						           GPIOD_PDDR &= ~GPIO_PDDR_PDD(0x20); \
                                   GPIOD_PDOR &= ~0x000000020

#define BOOT_LED_ON		           GPIOD_PDOR &= ~0x000000020
#define BOOT_LED_OFF	           GPIOD_PDOR |= 0x000000020

// Normally we do not need to change below definitions when migrating to a new M0+ MCU
/**************************************************/
//Register
#define SRS_REG                    RCM_SRS0
#define SRS_POR_MASK               RCM_SRS0_POR_MASK
  
#define FLASH_INIT_FLASH_CLOCK     ;//SIM_CLKDIV1 |= SIM_CLKDIV1_OUTDIV4(2);
#define FLASH_BASE_PTR             FTFA_BASE_PTR
#define FLASH_FSTAT                FTFA_FSTAT                                  
#define FLASH_FSTAT_CCIF_MASK      FTFA_FSTAT_CCIF_MASK
#define FLASH_FSTAT_ACCERR_MASK    FTFA_FSTAT_ACCERR_MASK
#define FLASH_FSTAT_FPVIOL_MASK    FTFA_FSTAT_FPVIOL_MASK
#define FLASH_FSTAT_RDCOLERR_MASK  FTFA_FSTAT_RDCOLERR_MASK
#define FLASH_FSTAT_MGSTAT0_MASK   FTFA_FSTAT_MGSTAT0_MASK                            
  
/**************************************************/
#define WDG_ResetMCU();            SIM_SRVCOP = 0xAA; SIM_SRVCOP = 0x55;
#define WDG_Disable();             SIM_COPC = 0;                               // Disable watchdog 
#define WDG_Enable();              SIM_COPC = (0x0000000F & (1<<3));           // Enable watchdog 
#define WDG_Refresh();             SIM_SRVCOP = 0x55; SIM_SRVCOP = 0xAA;       // Feed watchdog


#define EnableInterrupts           asm(" CPSIE i");
#define DisableInterrupts          asm(" CPSID i");


