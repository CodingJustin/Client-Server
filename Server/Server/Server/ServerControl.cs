using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class ServerControl
    {
        public string ip;
        public int port;
        public Socket server;
        public Dictionary<EndPoint, Socket> clientDic = new Dictionary<EndPoint, Socket>();

        public ServerControl() { }
        public ServerControl(string IP,int PORT)
        {
            ip = IP;
            port = PORT;
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Init()
        {
            if (server != null)
            {
                server.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                server.Listen(10);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("init server");
            }

            Thread listenThread = new Thread(ListenConnect);
            listenThread.IsBackground = false;
            listenThread.Start();
                       
        }

     
        public void ListenConnect()
        {
            while (true)
            {
                Socket client = server.Accept();
                Console.WriteLine("新连接 -- " + client.RemoteEndPoint.ToString());
                clientDic.Add(client.RemoteEndPoint, client);

                ///-------
                ///为每一个连接的客户端创建一个接收消息线程
                Thread receThread = new Thread(ReceiveData);
                receThread.IsBackground = true;
                receThread.Start(client);
            }
        }


        public void ReceiveData(object clientObject)
        {
            Socket client = (Socket)clientObject;
            
            while (true)
            {
                try
                {
                    byte[] bytes = new byte[1024];
                    int msgLen = client.Receive(bytes);

                    //这样写会导致会隔了几行
                    //Console.WriteLine(client.RemoteEndPoint.ToString() +"  "
                    //    + System.DateTime.Now + "  " + Encoding.UTF8.GetString(bytes));

                    Console.WriteLine(client.RemoteEndPoint.ToString() + "  " + System.DateTime.Now + "  "+ 
                        Encoding.UTF8.GetString(bytes, 0, msgLen));//这样对

                    Console.WriteLine("msgLen -- " + msgLen);

                    Broadcast(client,bytes,msgLen);//发送给其他客户端
                    //重发给发送此消息的客户端
      //              client.Send(Encoding.UTF8.GetBytes("repeat from server: " + Encoding.UTF8.GetString(bytes)));
                }
                catch
                {
                    Console.WriteLine(client.RemoteEndPoint.ToString() + "断开连接");
                    clientDic.Remove(client.RemoteEndPoint);//移除这一个断开连接的socket
                    //这个线程也应该要回收了才对，但是，怎么回收呢
                    break;
                }
            }
        }

        public void Broadcast(Socket clientThis, byte[] bytes,int length)
        {
            foreach (var item in clientDic.Values)
            {
                if (item != clientThis)
                {
                    string msg = 
                        item.RemoteEndPoint.ToString()+"  " 
                        + Encoding.UTF8.GetString(bytes,0,length);//注意这里也是三个参数，要不会隔了几行

                    item.Send(Encoding.UTF8.GetBytes(msg));
                }
            }
        }




    }
}
