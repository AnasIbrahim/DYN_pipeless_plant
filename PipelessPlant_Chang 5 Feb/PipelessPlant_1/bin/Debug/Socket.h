int createSocket(const char* IP_ADDRESS, int port);
int startUP();
void sendMessage(char* buf);
char* receiveMessage();
int cleanUp();