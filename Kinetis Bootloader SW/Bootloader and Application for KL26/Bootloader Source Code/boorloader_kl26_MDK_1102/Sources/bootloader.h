/*****************************************************************************
* Copyright (c) 2010 Freescale Semiconductor;
* All Rights Reserved                       
*
*******************************************************************************
*
* THIS SOFTWARE IS PROVIDED BY FREESCALE "AS IS" AND ANY EXPRESSED OR 
* IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
* OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  
* IN NO EVENT SHALL FREESCALE OR ITS CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
* INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
* SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
* HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
* STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
* IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
* THE POSSIBILITY OF SUCH DAMAGE.
*
***************************************************************************//*!
*
* @file      bootloader.h
*
* @author    b01119
* 
* @version   0.0.13.0
* 
* @date      Dec-11-2012
* 
* @brief     Bootloader state machine file header file
*
******************************************************************************/
#ifndef   _BOOTLOADER_H
#define   _BOOTLOADER_H


/******************************************************************************
* Includes
******************************************************************************/


/******************************************************************************
* Types
******************************************************************************/
// typedef definitions
typedef unsigned char   	Byte;  		//1 byte
typedef unsigned short  	Word;  		//2 byte
typedef unsigned int	   	LWord; 		//4 byte
typedef unsigned long long 	DLWord; 	//8 Byte


typedef union _FlashConfig_t
{
  unsigned int Data[4];
  struct {
    unsigned int BackDoorKey[2];
    unsigned int Protection;
    unsigned int Config;
  }nem;
} FlashConfig_t;

  typedef unsigned long addrType;
 
  typedef unsigned char BootloaderProtocolType;
  


#endif





