/*************************************************************************
 * DISCLAIMER                                                            *
 * Services performed by FREESCALE in this matter are performed          *
 * AS IS and without any warranty. CUSTOMER retains the final decision   *
 * relative to the total design and functionality of the end product.    *
 * FREESCALE neither guarantees nor will be held liable by CUSTOMER      *
 * for the success of this project. FREESCALE disclaims all warranties,  *
 * express, implied or statutory including, but not limited to,          *
 * implied warranty of merchantability or fitness for a particular       *
 * purpose on any hardware, software ore advise supplied to the project  *
 * by FREESCALE, and or any product resulting from FREESCALE services.   *
 * In no event shall FREESCALE be liable for incidental or consequential *
 * damages arising out of this agreement. CUSTOMER agrees to hold        *
 * FREESCALE harmless against any and all claims demands or actions      *
 * by anyone on account of any damage, or injury, whether commercial,    *
 * contractual, or tortuous, rising directly or indirectly as a result   *
 * of the advise or assistance supplied CUSTOMER in connection with      *
 * product, services or goods supplied under this Agreement.             *
 *************************************************************************/
/*******************************************************************
  Copyright (c) 2011 Freescale Semiconductor
  \file     	GlobalVars.cs
  \brief    	Variables shared by the project
  \author   	R66120
  \version      1.0
  \date     	26/Sep/2011

  \version      1.2.2
  \date     	04/June/2012
 
  \version      1.2.3
  \date     	05/Aug/2012
*********************************************************************/

namespace Global_Var
{
    public struct IDENT_DATA
    {
        //these variables are not used in this version of bootloader
        //public ulong[] mem_start;	                                 // start of usable flash
        //public ulong[] mem_end;	                                 // end of usable flash
        //public uint int_vect_tbl;	                                 // start of hard-wired interrupt vectors
        //public byte[] priv_data;	                                 // 8 bytes of private info
        //public uint num_blocks;	                                 // number of flash memory blocks (BL protocol version 2 and up)
        //public uint sdid;			                                 // SDID number   (BL protocol version 2 and up)
        //public uint sdidrev;		                                 // SDID revision (BL protocol version 2 and up)

        public string targ_name;	                                 // target name
        public uint erblk;			                                 // erase block size
        public uint wrblk;			                                 // write block size
        public uint bl_tbl;		                                     // start of boot-loader table (private data and jump table)
        public uint bl_version;
        public bool bl_rcs;                                          // reading support
        public ulong addr_limit;
        public ulong boot_addr_start;
        public ulong boot_addr_end;
      //public ulong verify_addr_limit;
        public ulong dontcare_addrl;                                 // when do verification, do not compare the contents from
        public ulong dontcare_addrh;                                 // dontcare_addrl to dontcare_addrh
    }

    public struct BOARD_MEM
    {
        public string status;
        public string filename;
        public byte[] d;                                             // data
        public byte[] f;                                             // valid flag 0=empty; 1=usercode; 2=systemcode
    }

    public static class MyVar
    {
        public static string identifier;
        public static ulong AddrLimit = 0x1000000;

        //public const int MAX_NUM_BLOCKS = 16;
        //public const int MAX_SDID = 32; 		                     // max. SDID, if modified, complete the SDID table in prog.c
        //public const int SDID_UNDEF = 0xFF;		                 // HC08 (ver. 3) will report this (0xFF)
        
        public const int MAX_ADDRESS = 0x1000000;
        public const int MAX_LENGTH = 0x1000;

    

        public const int FAMILY_HC08 = (1 << 0);
        public const int FAMILY_HCS08 = (1 << 1);
        public const int FAMILY_NEXT = (1 << 2);

        public const int BL_M0 = 1; //M0+ core
        public const int BL_M4 = 2; //M4 core

        public const int BL_HC08 = (1 << (FAMILY_HC08 - 1));
        public const int BL_HC08_LARGE = (BL_HC08 | 0x1 << FAMILY_HC08);

        public const int BL_HCS08 = (1 << (FAMILY_HCS08 - 1));
        public const int BL_HCS08_LONG = (BL_HCS08 | 0x1 << FAMILY_HCS08);
        public const int BL_HCS08_LARGE = (BL_HCS08 | 0x2 << FAMILY_HCS08);

        public const uint BL_UNKNOWN = 0;

        public static IDENT_DATA ident;
        public static BOARD_MEM image;

        public static void Init()
        {
            ident.targ_name = "";                                              // target name
            ident.bl_version = BL_UNKNOWN;
            
            image.filename = "";
            image.status = "File not specified!";
            image.d = new byte[MAX_ADDRESS];                                   // data
            image.f = new byte[MAX_ADDRESS];                                   // data
        }

        public static void Config()
        {
            switch (ident.targ_name)
            {

                case "MKL26Z128":
                    {
                        ident.bl_version = BL_M0;
                        ident.bl_rcs = true;
                        ident.erblk = 1024;
                        ident.wrblk = 64;
                        ident.addr_limit = 0x00020000;                            // The end address + 1 of the MCU
                        ident.boot_addr_start = 0x00000000;                        // Section contains the boot code
                        ident.boot_addr_end = 0x00000FFF;                          //
                        ident.dontcare_addrl = 0x000003FC;                         // 0x03FC to 0x03FF are nonvolatile registers
                        ident.dontcare_addrh = 0x000003FF;

                        break;
                    }
                case "MC9S08AC16":
                case "MC9S08AC32":
                    {
                        ident.bl_version = BL_HCS08;
                        ident.bl_rcs = true;
                        ident.erblk = 512;
                        ident.wrblk = 64;
                        ident.addr_limit = 0x10000;                            // The end address + 1 of the MCU
                        ident.boot_addr_start = 0xFC00;
                        ident.boot_addr_end = 0xFFFF;
                        ident.dontcare_addrl = 0xFFB0;                         // 0xFFB0 to 0xFFBF are nonvolatile registers
                        ident.dontcare_addrh = 0xFFBF; 

                        break;
                    }
                case "MC9S08LL64":
                case "MC9S08LH36":
                    {
                        ident.bl_version = BL_HCS08_LARGE;
                        ident.bl_rcs = true;
                        ident.erblk = 512;
                        ident.wrblk = 64;
                        ident.addr_limit = 0x03C000;                           // The end address + 1 of the MCU
                        ident.boot_addr_start = 0xFC00;                        // Section contains the boot code
                        ident.boot_addr_end = 0xFFFF;                          //
                        ident.dontcare_addrl = 0xFFAC;                         // 0xFFAC to 0xFFBF are nonvolatile registers
                        ident.dontcare_addrh = 0xFFBF;

                        break;
                    }

                case "MC9S08PT60":
                    {
                        ident.bl_version = BL_HCS08;
                        ident.bl_rcs = true;
                        ident.erblk = 512;
                        ident.wrblk = 64;
                        ident.addr_limit = 0x10000;                            // The end address + 1 of the MCU
                        ident.boot_addr_start = 0xF800;                        // Section contains the boot code
                        ident.boot_addr_end = 0xFFFF;                          //
                        ident.dontcare_addrl = 0xFF70;                         // 0xFF70 to 0xFF7F are nonvolatile registers
                        ident.dontcare_addrh = 0xFF7F;

                        break;
                    }


                default:
                    break;

            }
        }
    }
}