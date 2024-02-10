using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using DBManager;
using System.Reflection.Metadata.Ecma335;

namespace MainServer
{
    class Program{
        public static int PROTOCOL = 5000;
        static void Main(string[] args){

            InitTcpListener( PROTOCOL );
        }



        private static string username = "";

        private static TcpListener tcpListener;

        private static List<TcpClient> Clients = new List<TcpClient>();

        private static  Dictionary<TcpClient, string> hashMap = new Dictionary<TcpClient, string>();

        //private static int mapIndex = 0;

        public static void InitTcpListener(int PROTOCOL)
        {
            tcpListener = new TcpListener(IPAddress.Any, PROTOCOL);
            tcpListener.Start();
            Console.WriteLine("Main server已啟動");
            while (true)
            {


                TcpClient client = tcpListener.AcceptTcpClient();



                if (client != null)
                {
                    StreamReader reader = new StreamReader(client.GetStream());
                    username = reader.ReadLine();
                    /*mapIndex = mapIndex + 1;*/
                    hashMap.Add(client, username);
                    Console.WriteLine("新的用戶已連線:" + username);
                    Clients.Add(client);
                    Thread thread = new Thread(ClientListener);
                    thread.Start(client);
                    continue;
                }
            }

        }

        public static void ClientListener(object clientObj)
        {
            TcpClient client = (TcpClient)clientObj;

            try
            {
               
                StreamReader reader = new StreamReader(client.GetStream());
                while (true)
                {
                    string message = reader.ReadLine();
                    BroadCast(message, client);
                    Console.WriteLine(message, "客戶端發送的新訊息");
                }
            }
            catch (IOException ex)
            {
               
                Console.WriteLine("客戶端以斷線: " + ex.Message);

                DBClass.UpdateUserLoginState(hashMap[client], 0);
                return;
            }
        }


        public static void BroadCast(string msg, TcpClient excludeClient)
        {
            foreach (TcpClient client in Clients)
            {
                if (client.Connected && client != excludeClient)
                {
                    StreamWriter sWriter = new StreamWriter(client.GetStream());
                    sWriter.WriteLine(msg);
                    sWriter.Flush();
                    Console.WriteLine("成功廣播訊息給所有客戶端");
                }
            }
        }
    }
}