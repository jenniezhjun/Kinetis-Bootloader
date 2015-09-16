
/******************************************************************************
* Includes
******************************************************************************/

#include "FRDM_KL26Z_cfg.h"

/******************************************************************************
* Types
******************************************************************************/
// typedef definitions
typedef unsigned char   	Byte;  		//1 byte
typedef unsigned short  	Word;  		//2 byte
typedef unsigned int	   	LWord; 		//4 byte
typedef unsigned long long 	DLWord; 	//8 Byte
 

#ifndef KINETIS_PARAMS_H
#define KINETIS_PARAMS_H
typedef union _FlashConfig_t
{
  unsigned int Data[4];
  struct {
    unsigned int BackDoorKey[2];
    unsigned int Protection;
    unsigned int Config;
  };
} FlashConfig_t;

#endif //KINETIS_PARAMS_H




