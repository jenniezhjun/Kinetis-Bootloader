20150714
can continues send erase and program command
return 'I'
24012B044B4C3236AA55

return 'E' and 'W'
24012B00AA55


20150715
complete 'G'


20150717
ident.bl_version = BL_KL26;
ident.bl_rcs = true;
ident.erblk = 1024;
ident.wrblk = 64;


20150726
complete 'V' , 'R'


20150727
change all instruction data length from 2 byte to 1byte: cmd_buffer[3] 

for 'W' 'R', change number of bytes to be programmed to 1byte:
cmd_buffer[8] = (byte)len;                


20150730
1. add resp_buffer[4 + rcv_datalength] == 0xAA && resp_buffer[4 + rcv_datalength +1] == 0x55
when receive a frame

2. remove 3.5T delay in   modbus(cmd_buffer, resp_buffer)

3. all address display from X6 to X8 , for example
label8.Text = "prg_area: 0x" + start.ToString("X8") + "- 0x" + end.ToString("X8");          