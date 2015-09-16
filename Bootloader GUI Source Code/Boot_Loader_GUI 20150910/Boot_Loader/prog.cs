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
  \file     	Prog.cs
  \brief    	Program functions ported from AN2295SW
  \author   	R66120
  \version      1.0
  \date     	27/Sep/2011
 
  \version      1.1
  \date     	04/Jan/2011
 
  \version      1.2  modified function modbus()
  \date     	12/Jan/2012 
 
  \version      1.2.1  
  \date     	2/June/2012 
  \support address beyond 0xFFFF. Fixed bug in read_blk()
 
  \version      1.2.2  
  \date     	12/Dec/2012 
  \support recovery from temporary communication broken

*********************************************************************/


using System;
using System.Text;
using System.IO;
using System.Windows.Forms;
using s19_handler;
using Global_Var;
using System.Threading;

  
namespace Boot_Code
{    
    public partial class Form1 : Form
    { 
        public volatile bool KillFlag = false;
        public volatile int ThreadProgStarted = 0, ThreadVerifyStarted = 0;
        public int StationBeginAddr = 0, StationEndAddr = 0, StationAddr = 0;
        public static uint mytimer = 0;

        System.Threading.Thread threadProg, threadVerify;

        public void verify_func()
        {
            System.Threading.ThreadStart ts = new System.Threading.ThreadStart(verify);
            threadVerify = new System.Threading.Thread(ts);
            threadVerify.IsBackground = true;
            threadVerify.Priority = ThreadPriority.Highest;
            threadVerify.Start();
        }

        public void verify()
        {
            byte[] TxdBuffer = new byte[10];
            TxdBuffer[0] = 0;
            KillFlag = false;

            try
            {
                StationBeginAddr = Convert.ToInt32(textBox1.Text);
                StationEndAddr = Convert.ToInt32(textBox2.Text);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            ////////////  open log file
            try
            {
                //DateTime now = DateTime.Now;
                string filename = "boot log.txt";
                FileStream aFile = new FileStream(filename, FileMode.Append);
                StreamWriter sr = new StreamWriter(aFile);

                sr.WriteLine("======================================================================");
                sr.WriteLine("Current time is " + DateTime.Now.ToString());
                sr.WriteLine("The s19 file is: " + MyVar.image.filename);

                if (StationBeginAddr >= 1 && StationEndAddr <= 32)
                {
                    StationAddr = StationBeginAddr;

                    while ((!KillFlag) && StationAddr <= StationEndAddr)
                    {
                        uint counter = 0;

                        label9.Text = "Station " + StationAddr.ToString() + " is trying to enter Verify mode ";
                        label12.Text = "";
                        label5.Text = "";
                        sr.WriteLine(label9.Text);
                        progressBar3.Value = 0;

                        while ((!KillFlag) && counter++ < 3)                   // try 3 times to enter boot mode
                        {
                            if (enter_boot(2) < 0)
                            {
                                label12.Text = "Error " + counter.ToString() + " times!";
                                sr.WriteLine(label12.Text);
                                Thread.Sleep(2000);
                            }
                            else
                            {

                                label9.Text = "Verifying Station " + StationAddr.ToString();
                                sr.WriteLine(label9.Text);
                                if (verify_mem(sr) < 0)
                                {
                                    label12.Text = "Error!";
                                    sr.WriteLine(label12.Text);
                                    Thread.Sleep(2000);
                                    target_go();
                                }
                                else
                                {
                                    if (!KillFlag)
                                    {
                                        label12.Text = "Success!";
                                        sr.WriteLine(label12.Text);
                                    }
                                    target_go();
                                }
                                break;
                            }
                        }
                        StationAddr++;
                    }
                    label12.Text = "Verified!";
                }
                else
                {
                    label12.Text = ("Invalid station address!");
                    sr.WriteLine(label12.Text);
                }
                if (KillFlag)
                {
                    label12.Text = ("Terminated!");
                    sr.WriteLine(label12.Text);
                }
                ThreadVerifyStarted = 0;
                button1.Text = "Verify";

                sr.Close();
                aFile.Close();
                program_status = "";

            }  ///////////// close log file
            catch (IOException ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }

        public int verify_mem(StreamWriter sr)
        {
            ulong s, e;
           
            // find first and last valid byte
            for (s = 0; s < MyVar.ident.addr_limit; s++)
                if (MyVar.image.f[s] != 0) break;
            for (e = MyVar.ident.addr_limit; e > s; e--)
                if (MyVar.image.f[e - 1] != 0) break;

            if (verify_area(s, e, sr) < 0)
                return -1;

            return 0;
        }

        public int verify_area(ulong start, ulong end, StreamWriter sr)
        {
            ulong i, er, wr, er_next, wr_end, wr_next;
            ulong wr_one, verified = 0, total = 0;
            ulong MAX_ADDRESS = MyVar.MAX_ADDRESS;
            uint bl_version = MyVar.ident.bl_version;
           // ulong BL_HCS08_LARGE = MyVar.BL_HCS08_LARGE, BL_HCS08_LONG = MyVar.BL_HCS08_LONG;
            bool bl_rcs = true;  // reading support
            ulong failed_flag = 0;             // set it to 1 if there is any error in the process
            ulong failed_counter = 0;
        
            IDENT_DATA ident = MyVar.ident;
            BOARD_MEM image = MyVar.image;

            label12.Text = "verify_area: 0x" + start.ToString("X8") + "- 0x" + end.ToString("X8");
            sr.WriteLine(label12.Text);

            if (start >= MAX_ADDRESS || end >= MAX_ADDRESS)
                return -1;

            // count valid bytes
            for (i = start; i < end; i++)
                if (image.f[i] != 0) total++;


            // take start address as it is, but do further steps to erblk boundaries
            //for (er = start; er < end; er = er_next)
            for (er = start; er < end;         )
            {
                failed_flag = 0;

                if (KillFlag) break;
                // start of next erase block
                er_next = FLASHMODULO(er, ident.erblk);

                // anything to verify in this erase block ?
                wr = wr_end = er;
                for (i = er; i < er_next; i++)
                {
                    // valid byte
                    if (image.f[i] != 0)
                    {
                        if (image.f[wr] == 0) wr = i;
                        wr_end = i + 1;
                    }
                }

                // never pass after end
                if (wr_end > end)
                    wr_end = end;

                // wr is now pointing to first valid byte (within current erase block)
                // wr_end is now pointing after last valid byte (within current erase block)
                if (wr < wr_end)
                {
                    if (bl_version == MyVar.BL_M0)
                    {
                        label12.Text = "Memory verifying: 0x" + wr.ToString("X8");
                        progressBar3.Value = progressBar3.Maximum * (int)(verified) / (int)(total);
                    }
                    else
                    {
                        label12.Text = "Memory verifying: 0x" + wr.ToString("X4");
                        progressBar3.Value = progressBar3.Maximum * (int)(verified) / (int)(total);
                    }

                    for (/* original wr */; wr < wr_end; wr = wr_next)
                    {
                        if (KillFlag) break;
                        // start of next write block
                        wr_next = FLASHMODULO(wr, ident.wrblk);

                        if (bl_version == MyVar.BL_M0)
                        {
                            label12.Text = "Memory verifying: 0x" + wr.ToString("X8");
                            progressBar3.Value = progressBar3.Maximum * (int)(verified) / (int)(total);
                        }
                        else
                        {
                            label12.Text = "Memory verifying: 0x" + wr.ToString("X4");
                            progressBar3.Value = progressBar3.Maximum * (int)(verified) / (int)(total);
                        }
                        
                        wr_one = umin(wr_end, wr_next) - wr;
                        
                        if ((bl_rcs))	// read command implemented!
                        {
                            byte[] rbuff = new byte[256];
                            ulong adr;

                            if (bl_version == MyVar.BL_M0)
                                label12.Text = "Memory reading:     R 0x" + wr.ToString("X8");
                            else
                                label12.Text = "Memory reading:     R 0x" + wr.ToString("X4");

                            if (read_blk(wr, (int)(wr_one), rbuff) < 0)
                            {
                                if (!KillFlag)
                                {

                                    if (bl_version == MyVar.BL_M0)
                                    {
                                        label12.Text = "Can't read block at address 0x" + wr.ToString("X8");
                                        sr.WriteLine(label12.Text);
                                    }
                                    else
                                    {
                                        label12.Text = "Can't read block at address 0x" + wr.ToString("X4");
                                        sr.WriteLine(label12.Text);
                                    }
                                    //return -1;
                                    failed_flag = 1;
                                }
                            }
                            if (failed_flag == 1) break;

                            for (adr = wr; adr < wr + wr_one; adr++)
                            {
                                if (image.d[adr] != rbuff[adr - wr])
                                {
                                    if (!(adr >= ident.dontcare_addrl && adr <= ident.dontcare_addrh))
                                    {
                                        if (bl_version == MyVar.BL_M0)
                                        {
                                            label12.Text = "Verification failed at address 0x" + adr.ToString("X8") + ", image: 0x" + image.d[adr].ToString("X2") + ", MCU: 0x" + rbuff[adr - wr].ToString("X2");
                                            sr.WriteLine(label12.Text);
                                        }
                                        else
                                        {
                                            label12.Text = "Verification failed at address 0x" + adr.ToString("X4") + ", image: 0x" + image.d[adr].ToString("X2") + ", MCU: 0x" + rbuff[adr - wr].ToString("X2");
                                            sr.WriteLine(label12.Text);
                                        }
                                        //return -1;
                                        failed_flag = 1;
                                    }
                                }
                                if (failed_flag == 1) break;
                            }
                        }

                        if (failed_flag == 1) break;
                        // the percentage-counting algorithm is not perfect, in some cases there might 
                        // be more than 100% achieved (if S19 file has holes within erblks = rare case)
                        if ((verified += wr_one) > total)
                            verified = total;
                        progressBar3.Value = progressBar3.Maximum * (int)(verified) / (int)(total);
                        // Thread.Sleep(1000);                       // just display the result for customer to see it
                    }
                }
                if (failed_flag == 0)               // if there is no error in the process then go to next flash block
                {
                    er = er_next;
                    failed_counter = 0;
                }
                else
                {
                    failed_counter++;
                    if (failed_counter >= 5)
                        return -1;
                }
            }
            return 0;
        }

        public void boot_func( )
        {
            System.Threading.ThreadStart ts = new System.Threading.ThreadStart(boot);
            threadProg = new System.Threading.Thread(ts);
            threadProg.IsBackground = true;
            threadProg.Priority = ThreadPriority.Highest;
            threadProg.Start();
        }

        public void boot()
        {
            byte[] TxdBuffer = new byte[10];
            TxdBuffer[0] = 0;
            KillFlag = false;

            try
            {
                StationBeginAddr = Convert.ToInt32(textBox1.Text);
                StationEndAddr = Convert.ToInt32(textBox2.Text);
            }
            catch (System.Exception ex)
            {
                label8.Text = ex.Message;

            }

            ////////////  open log file
            try
            {
                //DateTime now = DateTime.Now;
                string filename = "boot log.txt";
                FileStream aFile = new FileStream(filename, FileMode.Append);
                StreamWriter sr = new StreamWriter(aFile);

                sr.WriteLine("======================================================================");
                sr.WriteLine("Current time is " + DateTime.Now.ToString());
                sr.WriteLine("The s19 file is: " + MyVar.image.filename);

                if (StationBeginAddr >= 1 && StationEndAddr <= 32)
                {
                    StationAddr = StationBeginAddr;

                    while ((!KillFlag) && StationAddr <= StationEndAddr)
                    {
                        uint counter = 0;

                        label7.Text = "Station " + StationAddr.ToString() + " is trying to enter Boot mode ";
                        label8.Text = "";
                        label5.Text = "";
                        sr.WriteLine(label7.Text);
                        progressBar2.Value = 0;

                        while ((!KillFlag) && counter++ < 3)                   // try 3 times to enter boot mode
                        {
                            if (enter_boot(1) < 0)
                            {
                                label8.Text = "Error " + counter.ToString() + " times!";
                                sr.WriteLine(label8.Text);
                                Thread.Sleep(1000);
                            }
                            else
                            {

                                label7.Text = "Programming Station " + StationAddr.ToString();    //Jennie Boot_Loader_GUI 20150706
                                sr.WriteLine(label7.Text);
                                if (prg_mem(sr) < 0)
                                {
                                    label8.Text = "Error!";
                                    sr.WriteLine(label8.Text);
                                    Thread.Sleep(2000);
                                }
                                else
                                {
                                    if (!KillFlag)                  //if Click terminate button, don't excute below three line of code
                                    {
                                        label8.Text = "Success!";
                                        sr.WriteLine(label8.Text);
                                        target_go();
                                    }
                                }
                                break;
                            }
                        }
                        StationAddr++;
                    }
                    label8.Text = "Programmed!";
                    //sr.WriteLine(label8.Text);
                }
                else
                {
                    label8.Text = ("Invalid station address!");
                    sr.WriteLine(label8.Text);
                }
                if (KillFlag)
                {
                    label8.Text = ("Terminated!");
                    sr.WriteLine(label8.Text);
                }

                ThreadProgStarted = 0;
                button2.Text = "Program";

                sr.Close();
                aFile.Close();
                program_status = "";

            }  ///////////// close log file
            catch (IOException ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }

        public int target_go()
        {
            int ret;
            byte[] cmd_buffer = new byte[256];
            byte[] resp_buffer = new byte[256];

            // issue command

            if (MyVar.ident.bl_version == MyVar.BL_M0)
            {

                cmd_buffer[0] = (byte)'$';
                cmd_buffer[1] = (byte)StationAddr;
                cmd_buffer[2] = 0x00;
                cmd_buffer[3] = 0x01; //data length
                cmd_buffer[4] = (byte)'G'; // run the code
                
            }
            else
            {
                cmd_buffer[0] = (byte)StationAddr;
                cmd_buffer[1] = 0x00;
                cmd_buffer[2] = 0x01;                                //data length
                cmd_buffer[3] = (byte)'G';                           // run the code
            }

            modbus(cmd_buffer, resp_buffer);

            ret = 1;

            return ret;
        }

        static int catcherror = 0;
        //////////////////////////////////////////////////////////////
        // read memory block, return number of bytes read or -1

        public int read_blk(ulong a, int len, byte[] dest)
        {
            uint bl_version = MyVar.ident.bl_version;//, BL_HCS08_LARGE = MyVar.BL_HCS08_LARGE, BL_HCS08_LONG = MyVar.BL_HCS08_LONG;
            byte[] cmd_buffer = new byte[256];
            byte[] resp_buffer = new byte[256];
            int i = 0;
            int rcv_datalength;

            catcherror++;

            if(len < 0 || len >= MyVar.MAX_LENGTH || a >= MyVar.MAX_ADDRESS)
		        return -1;

	        // issue command
	       
	       // if ((bl_version == BL_HCS08_LARGE) || (bl_version == BL_HCS08_LONG))
            if (bl_version == MyVar.BL_M0)                    //Jennie 0726
            {

                cmd_buffer[0] = (byte)'$';
                cmd_buffer[1] = (byte)StationAddr;//1
                cmd_buffer[2] = 0x00;
                cmd_buffer[3] = 0x05; //data length
                cmd_buffer[4] = (byte)'R';
                cmd_buffer[5] = (byte)((a >> 16) & 0x0000ff);       // 24 bit address 00
                cmd_buffer[6] = (byte)((a >> 8) & 0x0000ff);        //                20           
                cmd_buffer[7] = (byte)(a & 0x0000ff);               //                00
                cmd_buffer[8] = (byte)len;                           // number of bytes to be read

            }
            else
            {
                cmd_buffer[0] = (byte)StationAddr;
                cmd_buffer[1] = 0x00;
                cmd_buffer[2] = 0x04;                                // data length
                cmd_buffer[3] = (byte)'R';
                cmd_buffer[4] = (byte)((a >> 8) & 0x0000ff);         // 16 bit address
                cmd_buffer[5] = (byte)(a & 0x0000ff);
                cmd_buffer[6] = (byte)len;                           // number of bytes to be read
            }

            modbus(cmd_buffer, resp_buffer);

            rcv_datalength = resp_buffer[3];
            if (resp_buffer[0] == '$' && resp_buffer[1] == StationAddr && resp_buffer[3] == len && resp_buffer[4 + rcv_datalength] == 0xAA && resp_buffer[4 + rcv_datalength + 1] == 0x55)
            {
                if (bl_version == MyVar.BL_M0)
                {
                    for (i = 0; i < len; i++)
                        dest[i] = resp_buffer[i + 0x04];
                }
                else
                {
                    for (i = 0; i < len; i++)
                        dest[i] = resp_buffer[i + 0x03];
                }
            }
            else
                len = -1;

            return len;    //for 'R'
	        // return number of bytes read (==requested)
        }

        // mode = 1 enter program mode
        // mode = 2 enter verify mode
        public int enter_boot(int mode)
        {
            int ret;
            byte[] cmd_buffer = new byte[256];
            byte[] resp_buffer = new byte[256];
            int rcv_datalength;

            // issue command

            cmd_buffer[0] = (byte)'$';
            cmd_buffer[1] = (byte)StationAddr;
            cmd_buffer[2] = 0x00;
            cmd_buffer[3] = 0x01; //data length
            if(mode == 1)
                cmd_buffer[4] = (byte)'B';                           // enter program mode
            else if(mode == 2)
                cmd_buffer[4] = (byte)'V';                           // enter verify mode

            modbus(cmd_buffer, resp_buffer);

            if (!KillFlag)
                Thread.Sleep(500);                                   // wait target to enter boot mode
            else
                return -1;

            cmd_buffer[0] = (byte)'$';
            cmd_buffer[1] = (byte)StationAddr;
            cmd_buffer[2] = 0x00;
            cmd_buffer[3] = 0x01; //data length
            cmd_buffer[4] = (byte)'I';
   
            modbus(cmd_buffer, resp_buffer);

            rcv_datalength = resp_buffer[3];

            if (resp_buffer[0] == '$' && resp_buffer[1] == StationAddr && resp_buffer[4 + rcv_datalength] == 0xAA && resp_buffer[4 + rcv_datalength +1] == 0x55)
            {
                int i = 0;
                string tempstr = "";
                for (i = 0; i < resp_buffer[3]; i++)
                    tempstr += (char)resp_buffer[i+4];
                label5.Text = tempstr;
                ret = 1;
                MyVar.ident.targ_name = tempstr;
                MyVar.Config();
            }
            else
                ret = -1;

            if (MyVar.ident.bl_version == MyVar.BL_UNKNOWN)
                ret = -1;

            return ret;
        }

        //////////////////////////////////////////////////////////////
        // erase single block around given address

        public int erase_blk(ulong a)
        {
            int ret;
	        byte [] cmd_buffer = new byte[256];
            byte [] resp_buffer = new byte[256];
            int rcv_datalength;

	        label8.Text = "erase_blk: 0x" + a.ToString("X8");
	        
            if(a >= MyVar.ident.addr_limit)
		        return -1;

	        // issue command
	                    
           // if ((MyVar.ident.bl_version == MyVar.BL_HCS08_LARGE) || (MyVar.ident.bl_version == MyVar.BL_HCS08_LONG))
            if (MyVar.ident.bl_version == MyVar.BL_M0)
            {
                cmd_buffer[0] = (byte)'$';
                cmd_buffer[1] = (byte)StationAddr;
                cmd_buffer[2] = 0x00;
                cmd_buffer[3] = 0x04; //data length
                cmd_buffer[4] = (byte)'E';
                cmd_buffer[5] = (byte)((a >> 16) & 0x0000ff);       // 24 bit address
                cmd_buffer[6] = (byte)((a >> 8) & 0x0000ff);
                cmd_buffer[7] = (byte)(a & 0x0000ff);
            }
            else
            {
                cmd_buffer[0] = (byte)'$';
                cmd_buffer[1] = (byte)StationAddr;
                cmd_buffer[2] = 0x00;
                cmd_buffer[3] = 0x03; //data length
                cmd_buffer[4] = (byte)'E';
                cmd_buffer[5] = (byte)((a >> 8) & 0x0000ff);        // 16 bit address
                cmd_buffer[6] = (byte)(a & 0x0000ff);

            }

            modbus(cmd_buffer, resp_buffer);

            rcv_datalength = resp_buffer[3];

            if (resp_buffer[0] == '$' && resp_buffer[1] == StationAddr && resp_buffer[3] == 0x00 && resp_buffer[4 + rcv_datalength] == 0xAA && resp_buffer[4 + rcv_datalength + 1] == 0x55)
                ret = 1;            //Jennie 050709
            else
                ret = -1;

            return ret;    // for 'E'
        }

        //////////////////////////////////////////////////////////////
        // program single block

        public int prg_blk(ulong a, ulong len)
        {
            uint MAX_LENGTH = MyVar.MAX_LENGTH, MAX_ADDRESS = MyVar.MAX_ADDRESS;
            uint bl_version = MyVar.ident.bl_version;//, BL_HCS08_LARGE = MyVar.BL_HCS08_LARGE, BL_HCS08_LONG = MyVar.BL_HCS08_LONG;
            BOARD_MEM image = MyVar.image;
            byte [] cmd_buffer = new byte[256];  //>0x400
            byte [] resp_buffer = new byte[256];
            int ret;
	        ulong i;
            int rcv_datalength;


            label8.Text = "prg_blk: 0x" + a.ToString("X8") + "-0x" +  (a+len-1).ToString("X8");

	        if(len < 0 || len >= MAX_LENGTH || a >= MAX_ADDRESS) 
		        return -1;

	        // issue command
	      
	       // if ((bl_version == BL_HCS08_LARGE) || (bl_version == BL_HCS08_LONG))
            if (MyVar.ident.bl_version == MyVar.BL_M0)
            {
                cmd_buffer[0] = (byte)'$';
                cmd_buffer[1] = (byte)StationAddr;
                cmd_buffer[2] = 0x00;
                cmd_buffer[3] = (byte)(0x05 + len); // data length
                cmd_buffer[4] = (byte)'W';
                cmd_buffer[5] = (byte)((a >> 16) & 0x0000ff);       // 24 bit address
                cmd_buffer[6] = (byte)((a >> 8) & 0x0000ff);
                cmd_buffer[7] = (byte)(a & 0x0000ff);
                cmd_buffer[8] = (byte)len;         // number of bytes to be programmed

                for (i = 0; i < len; i++)
                { 
                  cmd_buffer[9 + i] = image.d[a+i];
                }
            }
            else
            {
                cmd_buffer[0] = (byte)'$';
                cmd_buffer[1] = (byte)StationAddr;
                cmd_buffer[2] = 0x00;
                cmd_buffer[3] = (byte)(0x04 + len); // data length
                cmd_buffer[4] = (byte)'W';
                cmd_buffer[5] = (byte)((a >> 8) & 0x0000ff);        // 16 bit address
                cmd_buffer[6] = (byte)(a & 0x0000ff);
                cmd_buffer[7] = (byte)len;         // number of bytes to be programmed
                for (i = 0; i < len; i++)
                {
                    cmd_buffer[8 + i] = image.d[a + i];
                }
            }

            modbus(cmd_buffer, resp_buffer);

            rcv_datalength = resp_buffer[3];

            if ( resp_buffer[0] == '$' && resp_buffer[1] == StationAddr && resp_buffer[3] == 0x00 && resp_buffer[4 + rcv_datalength] == 0xAA && resp_buffer[4 + rcv_datalength +1] == 0x55)
                ret = 1;     //Jennie 0710
            else
                ret = -1;

            return ret;   // for 'W'
        }

        /////////////////////////////////////////////////////////////////
        // helper compare of unsigned 

        public static ulong umin(ulong a, ulong b)
        {
            return a < b ? a : b;
        }

        public ulong FLASHMODULO(ulong x,ulong y)
        {
            return ((ulong)(MyVar.MAX_ADDRESS - (y) *(1 + ((MyVar.MAX_ADDRESS-(ulong)((x)+(y))-1) / (y)))));
        }

        public int prg_area(ulong start, ulong end, StreamWriter sr)
        {
            ulong i, er, wr, er_next, wr_end, wr_next;
	        ulong wr_one, written=0, total=0;
            ulong MAX_ADDRESS = MyVar.MAX_ADDRESS;
            ulong bl_version = MyVar.ident.bl_version;
          //  ulong BL_HCS08_LARGE = MyVar.BL_HCS08_LARGE, BL_HCS08_LONG = MyVar.BL_HCS08_LONG;
            bool bl_rcs = MyVar.ident.bl_rcs;  // reading support
            bool verify = checkBox1.Checked;
            ulong failed_flag = 0;             // set it to 1 if there is any error in the process
            ulong failed_counter = 0;

            IDENT_DATA ident = MyVar.ident;
            BOARD_MEM image = MyVar.image;


            label8.Text = "prg_area: 0x" + start.ToString("X8") + "- 0x" + end.ToString("X8");
            sr.WriteLine(label8.Text);

            if(start >= MAX_ADDRESS || end >= MAX_ADDRESS) 
                return -1;

            // count valid bytes
            for (i = start; i < end; i++)
            {
                if (i > ident.boot_addr_start && i < ident.boot_addr_end)
                {                                                   //if an address is in the boot code section, don't count it           
                }
                else 
                {
                    if (image.f[i] != 0) total++; 
                }
            }


	        // take start address as it is, but do further steps to erblk boundaries
            //for(er = start; er < end; er = er_next)
            for (er = start; er < end;      )
            {
                failed_flag = 0;
                if (KillFlag) break;
                // start of next erase block
                er_next = FLASHMODULO(er, ident.erblk);

                if (er >= ident.boot_addr_start && er <= ident.boot_addr_end)                    // if the address is within the boot code section
                {
                    er = ident.boot_addr_end + 1;                   // skip the boot code section
                    er_next = FLASHMODULO(er, ident.erblk);
                }

                // anything to program in this erase block ?
                wr = wr_end = er;
                for (i = er; i < er_next; i++)
                {
                    // valid byte
                    if (image.f[i] != 0)
                    {
                        if (image.f[wr] == 0) wr = i;
                        wr_end = i + 1;
                    }
                }

                // never pass after end
                if (wr_end > end)
                    wr_end = end;

                // wr is now pointing to first valid byte (within current erase block)
                // wr_end is now pointing after last valid byte (within current erase block)
                if (wr < wr_end)
                {
                    if (bl_version == MyVar.BL_M0)
                    {
                        label8.Text = "Memory programming: 0x" + wr.ToString("X8");
                        progressBar2.Value = progressBar2.Maximum * (int)(written) / (int)(total);
                    }
                    else
                    {
                        label8.Text = "Memory programming: 0x" + wr.ToString("X4");
                        progressBar2.Value = progressBar2.Maximum * (int)(written) / (int)(total);
                    }


                    //fflush(stdout); DBG("\n");

                    // use the first valid-byte address
                    if (erase_blk(wr) < 0)
                    {
                        if (bl_version == MyVar.BL_M0)
                        {
                            label8.Text = "Can't erase block at address 0x" + wr.ToString("X8");
                            sr.WriteLine(label8.Text);
                        }
                        else
                        {
                            label8.Text = "Can't erase block at address 0x" + wr.ToString("X4");
                            sr.WriteLine(label8.Text);
                        }
                        //return -1;
                        failed_flag = 1;
                    }

                    if (failed_flag == 0)
                    {

                        for (/* original wr */; wr < wr_end; wr = wr_next)
                        {
                            if (KillFlag) break;
                            // start of next write block
                            wr_next = FLASHMODULO(wr, ident.wrblk);

                            if (bl_version == MyVar.BL_M0)
                            {
                                label8.Text = "Memory programming: 0x" + wr.ToString("X8");
                                progressBar2.Value = progressBar2.Maximum * (int)(written) / (int)(total);
                            }
                            else
                            {
                                label8.Text = "Memory programming: 0x" + wr.ToString("X4");
                                progressBar2.Value = progressBar2.Maximum * (int)(written) / (int)(total);
                            }
                            //fflush(stdout); DBG("\n");

                            wr_one = umin(wr_end, wr_next) - wr;
                            if (prg_blk(wr, wr_one) < 0)
                            {
                                if (bl_version == MyVar.BL_M0)
                                {
                                    label8.Text = "Can't program block at address 0x" + wr.ToString("X8");
                                    sr.WriteLine(label8.Text);
                                }
                                else
                                {
                                    label8.Text = "Can't program block at address 0x" + wr.ToString("X4");
                                    sr.WriteLine(label8.Text);
                                }
                                //return -1;
                                failed_flag = 1;
                            }
                            if (failed_flag == 1) break;

                            if ((bl_rcs) && (verify))	// read command implemented!
                            {
                                byte[] rbuff = new byte[256];
                                ulong adr;

                                if (bl_version == MyVar.BL_M0)
                                    label8.Text = "Memory reading:     R 0x" + wr.ToString("X8");
                                else
                                    label8.Text = "Memory reading:     R 0x" + wr.ToString("X4");

                                //fflush(stdout); DBG("\n");
                                if (read_blk(wr, (int)(wr_one), rbuff) < 0)
                                {
                                    if (bl_version == MyVar.BL_M0)
                                    {
                                        label8.Text = "Can't read block at address 0x" + wr.ToString("X8");
                                        sr.WriteLine(label8.Text);
                                    }
                                    else
                                    {
                                        label8.Text = "Can't read block at address 0x" + wr.ToString("X4");
                                        sr.WriteLine(label8.Text);
                                    }
                                    //return -1;
                                    failed_flag = 1;

                                }
                                if (failed_flag == 1) break;

                                for (adr = wr; adr < wr + wr_one; adr++)
                                {
                                    if (image.d[adr] != rbuff[adr - wr])
                                    {
                                        if (bl_version == MyVar.BL_M0)
                                        {
                                            label8.Text = "Verification failed at address 0x" + adr.ToString("X8") + ", image: 0x" + image.d[adr].ToString("X2") + ", MCU: 0x" + rbuff[adr - wr].ToString("X2");
                                            sr.WriteLine(label8.Text);
                                        }
                                        else
                                        {
                                            label8.Text = "Verification failed at address 0x" + adr.ToString("X4") + ", image: 0x" + image.d[adr].ToString("X2") + ", MCU: 0x" + rbuff[adr - wr].ToString("X2");
                                            sr.WriteLine(label8.Text);
                                        }
                                        //return -1;
                                        failed_flag = 1;
                                    }
                                    if (failed_flag == 1) break;
                                }
                            }

                            if (failed_flag == 1) break;
                            // the percentage-counting algorithm is not perfect, in some cases there might 
                            // be more than 100% achieved (if S19 file has holes within erblks = rare case)
                            if ((written += wr_one) > total)
                                written = total;
                            progressBar2.Value = progressBar2.Maximum * (int)(written) / (int)(total);
                            // Thread.Sleep(1000);       // just display the result for customer to see it
                        }
                    }
                }

                if (failed_flag == 0)               // if there is no error in the process then go to next flash block
                {
                    er = er_next;
                    failed_counter = 0;
                }
                else 
                {
                    failed_counter++;
                    if (failed_counter >= 5)
                        return -1;
                }
            }
            return 0;            
        }

        // program all
        public int prg_mem(StreamWriter sr)
        {
	        ulong s,e;
       
            // find first and last valid byte
            for(s=0; s<MyVar.ident.addr_limit; s++)
                if(MyVar.image.f[s] != 0) break;
            for (e = MyVar.ident.addr_limit; e > s; e--)
                if(MyVar.image.f[e-1] != 0) break;

	        if(prg_area(s, e, sr) < 0)
		        return -1;

            return 0;
        }

        public void modbus(byte[] cmd_buffer, byte[] resp_buffer)
        {
            //calculate the CRCH CRCL send command and wait for response
            int pointer, datalength, counter = 0, max_wait_time = 100;
           // double T_modbus;
            byte temp, CRCH = 0xAA, CRCL = 0x55;
            int availablebytes;

            pointer = cmd_buffer[3];
            pointer = pointer + 3;
            cmd_buffer[++pointer] = CRCH;                            // should fill CRCH if CRC is implemented
            cmd_buffer[++pointer] = CRCL;                            // should fill CRCH if CRC is implemented
            datalength = pointer + 1;

            //serialPort1.BaudRate = 4800;
           
           // T_modbus = 11.0 * 1000.0 / serialPort1.BaudRate;

           // double tempt;
           // tempt = 3.5 * T_modbus;
           // tempt = 4 * T_modbus;

           // Thread.Sleep((int)(Math.Ceiling(4 * T_modbus)));
            try
            {
                serialPort1.DiscardInBuffer();
                serialPort1.DiscardOutBuffer();
            }
            catch (System.Exception ex)
            {
                label8.Text = ex.Message;
            }

            try
            {
                serialPort1.Write(cmd_buffer, 0, datalength);
            }
            catch (System.Exception ex)
            {
                label8.Text = ex.Message;
            }

            //serialPort1.ReadTimeout = (int)(3.5 * T_modbus);
            serialPort1.ReadTimeout = 20;

            pointer = 0;
            counter = 0;
            if (cmd_buffer[4] == 'B' || cmd_buffer[4] == 'V' || cmd_buffer[4] == 'G')              // target node does not send back any informaiton for command 'B', 'V' and 'G'
            {
                max_wait_time = 1;
                Thread.Sleep(500);                                                                 // wait code start up
            }
            else
            {
                //Thread.Sleep(100);

                mytimer = 0;
                timer1.Start();

                //while (!KillFlag && pointer == 0 && mytimer < 500)                                 // we may adjust mytimer to adapt sci speed
                while (!KillFlag && pointer == 0 && mytimer < 200)                                 // we may adjust mytimer to adapt sci speed
                {
                    try
                    {
                        availablebytes = serialPort1.BytesToRead;
                        temp = (byte)(serialPort1.ReadByte());
                        resp_buffer[pointer] = temp;
                        pointer++;
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.Message == "The operation has timed out." || ex.Message == "操作已超时。")
                        {
                            if (pointer > 0)
                                break;
                        }
                    }
                }
            }

            mytimer = 0;

            if (pointer > 0)
            {
                timer1.Start();

                //while (!KillFlag && pointer <= 200 && mytimer < 20)
                while (!KillFlag && mytimer < 4)                                                  // we may adjust mytimer for different baudrates. 8 for 4800, 4 for 9600 or higher baudrates, 
                {
                    try
                    {
                        availablebytes = serialPort1.BytesToRead;
                        temp = (byte)(serialPort1.ReadByte());
                        resp_buffer[pointer] = temp;
                        pointer++;
                        mytimer = 0;
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.Message == "The operation has timed out." || ex.Message == "操作已超时。")
                        {
                            //if (pointer > 0)
                            //    break;
                        }
                    }
                }

            }
        }
	}
}
