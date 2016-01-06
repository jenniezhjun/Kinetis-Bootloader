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
  \file     	s19.cs
  \brief    	s19 file decorder ported from AN2295SW
  \author   	R66120
  \version      1.0
  \date     	26/Sep/2011
 
  \version      1.1
  \date     	04/Jan/2011
*********************************************************************/

using System;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Global_Var;
using Boot_Loader;


namespace s19_handler
{
    /// <summary>
    /// Summary description for s19.
    /// </summary>
    ///    
	public class s19
	{
        const int MAX_ADDRESS = 0x1000000;

        public s19()
		{
            //
			// TODO: Add constructor logic here
			//
		}

        public static int ReadImage( )
		{
            string filename;
            int i;
  	        char [] afmt= new char[7];
	        byte u, b, sum, len, alen, add_byte;
	        ulong addr = 0, total = 0, addr_lo = MyVar.MAX_ADDRESS, addr_hi = 0;
	        int line = 0, terminate = 0;
            byte pc = 0;
            ulong len1 = 0;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "S19 file(*.s19)|*.s19|Text Document(*.txt)|*.txt|hex file(*.hex)|*.hex|bin file(*.bin)|*.bin|srec file(*.srec)|*.srec|All files(*.*)|*.*";
            ofd.ShowDialog();
            filename = ofd.FileName;
            
            if (filename != "")
            {
                string fileExtension = System.IO.Path.GetExtension(filename);
                if(fileExtension == ".s19" | fileExtension == ".srec")
                {
                    for (i = 0; i < MyVar.MAX_ADDRESS; i++)
                    {
                        MyVar.image.d[i] = 0xff;
                        MyVar.image.f[i] = 0;
                    }

                    string srecord;
                    try
                    {
                        FileStream aFile = new FileStream(filename, FileMode.Open);
                        StreamReader sr = new StreamReader(aFile);
                        while (terminate == 0 && (srecord = sr.ReadLine()) != null)
                        {
                            pc = 0;
                            line++;

                            if (srecord[pc++] != 'S')
                                continue;
                            switch (srecord[pc++])
                            {
                                case '0':
                                    continue;
                                case '1':
                                    alen = 4;
                                    break;
                                case '2':
                                    alen = 6;
                                    break;
                                case '3':
                                    alen = 8;
                                    break;
                                case '9':
                                case '8':
                                case '7':
                                    terminate = 1;
                                    continue;
                                default:
                                    continue;
                            }

                            string hex = null;

                            hex = new String(new Char[] { srecord[pc++], srecord[pc++] });
                            len = HexToByte(hex);

                            hex = null;
                            addr = 0;
                            for (i = 0; i < alen; i++)
                            {
                                byte temp;
                                temp = (byte)srecord[pc++];
                                if (temp >= 48 && temp <= 57)       // '0' to '9'
                                    temp -= 48;
                                else if (temp >= 65 && temp <= 70)  // 'A' to 'F'
                                    temp -= 55;
                                addr += (ulong)(temp) << (byte)(alen - i - 1) * 4;
                            }
                            sum = len;
                            for (u = 0; u < 4; u++)
                                sum += (byte)((addr >> (byte)((u * 8))) & 0xff);

                            len -= (byte)(alen / 2 + 1);

                            for (u = 0; u < len; u++)
                            {
                                hex = new String(new Char[] { srecord[pc++], srecord[pc++] });
                                b = HexToByte(hex);
                                MyVar.image.d[addr + u] = b;
                                MyVar.image.f[addr + u] = 1;
                                sum += b;
                                total++;

                                if (addr + u < addr_lo)
                                    addr_lo = addr + u;
                                if (addr + u > addr_hi)
                                    addr_hi = addr + u;
                            }
                            add_byte = (byte)(4 - len % 4);
                            for (u = 0; u < add_byte; u++)
                            {
                                MyVar.image.d[addr + len + u] = 0xFF;
                                MyVar.image.f[addr + len + u] = 1;
                                if (addr + len + u < addr_lo)
                                    addr_lo = addr + len + u;
                                if (addr + len + u > addr_hi)
                                    addr_hi = addr + len + u;
                            }
                            hex = new String(new Char[] { srecord[pc++], srecord[pc++] });
                            b = HexToByte(hex);
                            if ((sum + b) != 0xff)
                            {
                                MyVar.image.status = "ERROR reading s19 file!";
                            }
                        }
                        sr.Close();
                        aFile.Close();
                    }
                    catch (IOException ee)
                    {
                        Console.WriteLine("An IO exception has been thrown!");
                        Console.WriteLine(ee.ToString());
                        Console.ReadKey();
                        MyVar.image.status = "ERROR reading s19 file!";
                        return -1;
                    }
                    MyVar.image.filename = filename;
                    MyVar.image.status = "OK";
                    return 0;
                }
                else if (fileExtension == ".hex")
                {
                    for (i = 0; i < MyVar.MAX_ADDRESS; i++)
                    {
                        MyVar.image.d[i] = 0xff;
                        MyVar.image.f[i] = 0;
                    }
                    string srecord;
                    try
                    {
                        FileStream aFile = new FileStream(filename, FileMode.Open);
                        StreamReader sr = new StreamReader(aFile);
                        while (terminate == 0 && (srecord = sr.ReadLine()) != null)
                        {
                            pc = 0;
                            line++;
                            byte temp1, temp2;
                            if (srecord[pc++] != ':')
                                continue;

                            string hex = null;
                            hex = null;
                            addr = 0;
                            temp1 = (byte)srecord[pc++];
                            temp2 = (byte)srecord[pc++];
                            if (temp1 >= 48 && temp1 <= 57)       // '0' to '9'
                                temp1 -= 48;
                            else if (temp1 >= 65 && temp1 <= 70)  // 'A' to 'F'
                                temp1 -= 55;
                            if (temp2 >= 48 && temp2 <= 57)       // '0' to '9'
                                temp2 -= 48;
                            else if (temp2 >= 65 && temp2 <= 70)  // 'A' to 'F'
                                temp2 -= 55;
                            len1 = (ulong)temp1 * 16 + (ulong)temp2;
                            len = (byte)len1;

                            for (i = 0; i < 4; i++)
                            {
                                byte temp;
                                temp = (byte)srecord[pc++];
                                if (temp >= 48 && temp <= 57)       // '0' to '9'
                                    temp -= 48;
                                else if (temp >= 65 && temp <= 70)  // 'A' to 'F'
                                    temp -= 55;
                                addr += (ulong)(temp) << (byte)(4 - i - 1) * 4;
                            }
                            sum = len;
                            for (u = 0; u < 4; u++)
                                sum += (byte)((addr >> (byte)((u * 8))) & 0xff);

                            pc++;
                            switch (srecord[pc++])
                            {
                                case '0':
                                    break;
                                case '1':
                                    terminate = 1;
                                    continue;
                                default:
                                    continue;
                            }

                            for (u = 0; u < len; u++)
                            {
                                hex = new String(new Char[] { srecord[pc++], srecord[pc++] });
                                b = HexToByte(hex);
                                MyVar.image.d[addr + u] = b;
                                MyVar.image.f[addr + u] = 1;
                                sum += b;
                                total++;

                                if (addr + u < addr_lo)
                                    addr_lo = addr + u;
                                if (addr + u > addr_hi)
                                    addr_hi = addr + u;
                            }
                            add_byte = (byte)(4 - len % 4);
                            for (u = 0; u < add_byte; u++)
                            {
                                MyVar.image.d[addr + len + u] = 0xFF;
                                MyVar.image.f[addr + len + u] = 1;
                                if (addr + len + u < addr_lo)
                                    addr_lo = addr + len + u;
                                if (addr + len + u > addr_hi)
                                    addr_hi = addr + len + u;
                            }

                            hex = new String(new Char[] { srecord[pc++], srecord[pc++] });
                            b = HexToByte(hex);
                        }
                        sr.Close();
                        aFile.Close();
                    }
                    catch (IOException ee)
                    {
                        Console.WriteLine("An IO exception has been thrown!");
                        Console.WriteLine(ee.ToString());
                        Console.ReadKey();
                        MyVar.image.status = "ERROR reading hex file!";
                        return -1;
                    }
                    MyVar.image.filename = filename;
                    MyVar.image.status = "OK";
                    return 0;
                }
                else if(fileExtension == ".bin")
                {
                    for (i = 0; i < MyVar.MAX_ADDRESS; i++)
                    {
                        MyVar.image.d[i] = 0xff;
                        MyVar.image.f[i] = 0;
                    }
                    MyVar.flag_bin = 1;
                    try
                    {
                        //string srecord;
                        long fileLength;
                        FileStream aFile = new FileStream(filename, FileMode.Open);
                        fileLength = aFile.Length;
                        byte[] fileContent = new byte[(aFile.Length & 0x03) > 0 ? (aFile.Length / 4 + 1) << 2 : aFile.Length];
                        aFile.Read(fileContent, 0, (int)fileLength);
                        string sr = System.Text.Encoding.Default.GetString(fileContent);
                        ulong j = 0;
                        for (j = 0; j < (ulong)fileLength; j++)
                        {

                            MyVar.image.d[j] = fileContent[j];
                            MyVar.image.f[j] = 1;
                            if (j < addr_lo)
                                addr_lo = j;
                            if (j > addr_hi)
                                addr_hi = j;
                        }

                        aFile.Close();
                    }
                    catch (IOException ee)
                    {
                        Console.WriteLine("An IO exception has been thrown!");
                        Console.WriteLine(ee.ToString());
                        Console.ReadKey();
                        MyVar.image.status = "ERROR reading bin file!";
                        return -1;
                    }
                    MyVar.image.filename = filename;
                    MyVar.image.status = "OK";
                    return 0;
                }
                else
                {
                    MyVar.image.status = "File not specified!";
                    return -1;
                }


            }
            else 
            {
                MyVar.image.status = "File not specified!";
                return -1; 
            }
		}

        private static byte HexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
                throw new ArgumentException("hex must be 1 or 2 characters in length");
            byte newByte = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return newByte;
        }

	}
    
}