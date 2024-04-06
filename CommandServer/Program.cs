using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using DBManager;

namespace CommandServer
{
    class Program
    {
        private const int serverPort = 6000;
        private static TcpListener listener;
        private static NetworkStream stream;
        private static byte[] dataBytes = new byte[4096];

        static void Main(string[] args)
        {
            StartServer();
            while (true)
            {
                Console.WriteLine("等待客戶端連線");
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("客戶端已連線");
                HandleClient(client);
            }
        }

        private static void StartServer()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, serverPort);
                listener.Start();
                Console.WriteLine("Command Server started. Waiting for connections...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to start command server: " + ex.Message);
            }
        }

        private static void HandleClient(TcpClient client)
        {
            try
            {
                stream = client.GetStream();
                Array.Clear(dataBytes, 0, dataBytes.Length);

                int bytesRead = stream.Read(dataBytes, 0, dataBytes.Length);
                string jsonData = Encoding.ASCII.GetString(dataBytes, 0, bytesRead);

                CommandData data = JsonConvert.DeserializeObject<CommandData>(jsonData);
                Console.WriteLine("Received command: " + data.Command);

                string response;
                if (data.Command == "COMMAND1")
                {
                    if (DBClass.CheckAccount(data.Account, data.Password))
                    {
                        string userName = DBClass.GetUsdrName(data.Account);
                        response = "true," + userName;
                    }
                    else
                    {
                        response = "false,";
                    }

                    byte[] responseData = Encoding.ASCII.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error handling client: " + ex.Message);
            }
        }

        private static void StopServer()
        {
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }

        public class CommandData
        {
            public string Command { get; set; }
            public string Account { get; set; }
            public string Password { get; set; }
        }
    }
}
