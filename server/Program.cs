using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DBManager;

namespace MainServer
{
    class Program
    {
        public static int PROTOCOL = 5000;

        static void Main(string[] args)
        {
            InitTcpListener(PROTOCOL);
        }

        private static string username = "";
        private static TcpListener tcpListener;
        private static List<TcpClient> clients = new List<TcpClient>();
        private static Dictionary<TcpClient, string> hashMap = new Dictionary<TcpClient, string>();

        public static void InitTcpListener(int protocol)
        {
            tcpListener = new TcpListener(IPAddress.Any, protocol);
            tcpListener.Start();
            Console.WriteLine("Main server已啟動");
            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                if (client != null)
                {
                    StreamReader reader = new StreamReader(client.GetStream());
                    username = reader.ReadLine();
                    hashMap.Add(client, username);
                    Console.WriteLine("新的用戶已連線: " + username);
                    clients.Add(client);
                    Thread thread = new Thread(ClientListener);
                    thread.Start(client);
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
                    Console.WriteLine(message + " 客戶端發送的新訊息");
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("客戶端以斷線: " + ex.Message);
                DBClass.UpdateUserLoginState(hashMap[client], 0);
            }
        }

        public static void BroadCast(string msg, TcpClient excludeClient)
        {
            foreach (TcpClient client in clients)
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
