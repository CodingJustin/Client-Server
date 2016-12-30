using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class ClientControl
    {
        public string ip;
        public int port;
        public Socket client;

        public ClientControl() { }
        public ClientControl(string IP,int Port)
        {
            ip = IP;
            port = Port;
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect()
        {
            if (client != null)
            {
                client.Connect(ip, port);
                Console.WriteLine("客户端连接 ------");
            }
            //
            Thread sendThread = new Thread(ReadAndSend);//作为主线程
            sendThread.IsBackground = false;
            sendThread.Start();
            //
            Thread receThread = new Thread(Receive);
            receThread.IsBackground = true;
            receThread.Start();
        }

        public void Send()
        {
            
            //while (msg != "quit")
            while(true)
            {
                try
                {
                    string msg = Console.ReadLine();
                    byte[] bytes = new byte[1024];
                    bytes = Encoding.UTF8.GetBytes(msg);
                    client.Send(bytes);
                }
                catch
                {
                    Console.WriteLine("服务器拒绝连接");
                    break;
                }
            }
        }

        public void ReadAndSend()
        {
            string msg = Console.ReadLine();
            while(msg!="quit")
            {
                client.Send(Encoding.UTF8.GetBytes(msg));
                msg = Console.ReadLine();
            }

        }

        public void Receive() 
        {
            while(true)
            {
                try
                {
                    byte[] msg = new byte[1024];
                    int msgLen = client.Receive(msg);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine( "server : " + Encoding.UTF8.GetString(msg,0,msgLen));
                    msg = null;
                    Console.ResetColor();
                }
                catch
                {
                    Console.WriteLine("服务端拒绝连接");
                    break;
                }
            }
        }

    }
}
