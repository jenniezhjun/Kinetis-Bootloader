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
  \file     	Form1.cs
  \brief    	Bootloader GUI
  \author   	R66120
  \version      1.0
  \date     	26/Sep/2011
 
  \version      1.1
  \date     	04/Jan/2011
 
  \version      1.2
  \date     	12/Jan/2012 
 
  \version      1.2.1
  \date     	04/June/2012 
  \support address beyond 0xFFFF
 
*********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using s19_handler;
using Global_Var;
using System.Threading;


namespace Boot_Code
{    
    public partial class Form1 : Form
    {
        public string program_status = "";

        public Form1()
        {
            int i,NumberOfCOM;
            string[] ports;
          
            InitializeComponent();
            Form1.CheckForIllegalCrossThreadCalls = false;
            MyVar.Init();

            ports = System.IO.Ports.SerialPort.GetPortNames();

            NumberOfCOM = ports.GetLength(0);

            for (i = 0; i < NumberOfCOM;i++ )
            {
                comboBox1.Items.Add(ports[i]);
            }
           
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (program_status == "" || program_status == "verifying")
            {
                program_status = "verifying";

                if (serialPort1.IsOpen)
                {
                    if (button1.Text == "Verify")
                    {
                        if (MyVar.image.status == "OK")
                        {
                            button1.Text = "Terminate";
                            Monitor.Enter(serialPort1);
                            verify_func();
                            Monitor.Exit(serialPort1);
                        }
                        else
                        {
                            label12.Text = MyVar.image.status;
                        }
                    }
                    else if (button1.Text == "Terminate")
                    {
                        button1.Text = "Verify";
                        KillFlag = true;
                        try
                        {
                            threadVerify.Join();
                        }
                        catch (System.Exception ex)
                        {
                            label12.Text = ex.Message;
                        }
                        program_status = "";
                    }
                }
                else
                {
                    label12.Text = "Serial port is not open!";
                    program_status = "";

                }
            }
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            if (program_status == "" || program_status == "programming")
            {
                program_status = "programming";

                if (serialPort1.IsOpen)
                {
                    if (button2.Text == "Program")
                    {
                        if (MyVar.image.status == "OK")
                        {
                            button2.Text = "Terminate";
                            Monitor.Enter(serialPort1);
                            boot_func();
                            Monitor.Exit(serialPort1);
                        }
                        else
                        {
                            label8.Text = MyVar.image.status;
                        }
                    }
                    else if (button2.Text == "Terminate")
                    {
                        button2.Text = "Program";
                        KillFlag = true;
                        try
                        {
                            threadProg.Join();
                        }
                        catch (System.Exception ex)
                        {
                            label8.Text = ex.Message;

                        }
                        program_status = "";
                    }
                }
                else
                {
                    label8.Text = "Serial port is not open!";
                    program_status = "";
                }
        
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            s19.ReadImage( );
            if(MyVar.image.status == "OK")
                label2.Text = MyVar.image.filename;
            else
                label2.Text = MyVar.image.status;
        }

       
        private void label1_Click(object sender, EventArgs e)
        {
           
        }

    
        private void button4_Click(object sender, EventArgs e)
        {
            
         }

        private void button5_Click(object sender, EventArgs e)
        {
            KillFlag = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            byte[] buffer = new byte[64];

            if (comboBox1.Text != "")
            {
                try
                {
                    serialPort1.Close();
                }
                catch (System.Exception ex)
                {
                    label6.Text = ex.Message;

                }
                serialPort1.PortName = comboBox1.Text;
                //serialPort1.BaudRate = 115200;
                serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                //serialPort1.Encoding = Encoding.GetEncoding("iso-8859-1");
                try
                {
                    serialPort1.Open();

                }
                catch (System.Exception ex)
                {
                    label6.Text = ex.Message;

                }

                if (serialPort1.IsOpen)
                    label6.Text = "Opened";
                else
                    label6.Text = "Closed";
            }
            else 
            {
                if(comboBox1.Text == "")
                    label6.Text = "Closed";
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            KillFlag = true;
            try
            {
                serialPort1.Close();

            }
            catch (System.Exception ex)
            {
                label6.Text = ex.Message;

            }
            if(serialPort1.IsOpen)
                label6.Text = "Opened";
            else
                label6.Text = "Closed";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int i, NumberOfCOM;
            string[] ports;

            ports = System.IO.Ports.SerialPort.GetPortNames();
            NumberOfCOM = ports.GetLength(0);

            comboBox1.Items.Clear();
            for (i = 0; i < NumberOfCOM; i++)
            {
                comboBox1.Items.Add(ports[i]);
            }
        }

       public void set_label8(string mystring)
       {
           label8.Text = mystring;
       }

       private void Form1_Closing(object sender, FormClosingEventArgs e)
       {
           KillFlag = true;
           try
           {
               threadProg.Join();
           }
           catch (System.Exception ex)
           {
               label8.Text = ex.Message;

           }
       }

       private void SCI_Receive(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
       {

       }

       private void timer1_Tick(object sender, EventArgs e)
       {
           Form1.mytimer++;
       }

       private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
       {

       }

       private void Form1_Load(object sender, EventArgs e)
       {

       }

       private void label4_Click(object sender, EventArgs e)
       {

       } 


    }
}


