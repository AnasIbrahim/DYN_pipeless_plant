import socket
TCP_IP = '192.168.0.130'
TCP_PORT=8000
BUFFER_SIZE=64
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((TCP_IP, TCP_PORT))
b=bytearray(64)

# bytes reserved for the valves of color vessel valves
b[1]=1              # yellow color
b[2]=10                 # Timer_yellow vessel's valve
b[3]=0                 # black_valve
b[4]=0                 # Timer_black_valve
b[5]=0                 # red_color_valve
b[6]=0                 # Timer_red_color
b[7]=1                 # blue_color_valve
b[8]=0                 # Timer_blue_color
b[19]=0

#  bytes
#b[18]=0
#b[19]=0
b[20]=0
b[21]=0

# unknown bytes
#b[22]=0
#b[23]=0

# unknown bytes
#b[24]=0
#b[25]=0
#b[26]=0
#b[27]=0

# Mixer bytes
#b[47]=0
#b[48]=0
#b[49]=0
#b[50]=0
#b[55]=0
#b[56]=0
#b[57]=0
#b[58]=0

# unknown bytes
b[28]=0
b[29]=0
b[30]=0
b[31]=0
b[32]=0
b[33]=0
b[44]=0
b[45]=0

# byte for storage station
b[34]=0
b[35]=0
b[36]=0
b[37]=0
b[38]=0
b[38]=0
b[39]=0
b[40]=0
b[41]=1
b[42]=0
b[43]=0

#b=bytearray(64)

s.send(b)
