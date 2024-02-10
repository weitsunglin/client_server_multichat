using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json; //NuGet 管理器安装
using DBManager;

namespace CommandServer
{
    class Program
    {
        private const int serverPort = 6000; // 服务器端口号
        private static TcpListener listener;
        private static NetworkStream stream;
        private static byte[] dataBytes = new byte[4096];

        static void Main(string[] args)
        {
            StartServer();
            while (true){
                Console.WriteLine("等待客戶端連線");
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("客戶端已連線");
                HandleClient(client);
                //client.Close();
                //Console.WriteLine("客戶端斷線");
            }
        }

        private static void StartServer(){
            try{
                // 创建 TCP 服务器监听器
                listener = new TcpListener(IPAddress.Any, serverPort);
                // 启动服务器
                listener.Start();
                Console.WriteLine("command Server started. Waiting for connections...");
            }
            catch (Exception ex){
                Console.WriteLine("Failed to start command server: " + ex.Message);
            }
        }

        public class CommandData
        {
            public string Command { get; set; }
            public string Account { get; set; }
            public string Password { get; set; }
        }

       

        private static void HandleClient(TcpClient client)
        {
            Console.WriteLine("1111111111111111111111 ") ;
            try
            {
                Console.WriteLine("222222222222222222222 ");
                stream = client.GetStream();

                
                Console.WriteLine("222222222222222222222 ");
                Array.Clear(dataBytes, 0, dataBytes.Length);


                //if (stream.DataAvailable)
                //{
                    int bytesRead = stream.Read(dataBytes, 0, dataBytes.Length);
                    // 处理读取的数据
                    Console.WriteLine("222222222222222222222 ");
                    // 将接收到的字节数组转换为字符串
                    string jsonData = Encoding.ASCII.GetString(dataBytes, 0, bytesRead);
                    Console.WriteLine("222222222222222222222 ");
 
                //}


                // 将字符串解析为数据对象
                CommandData data = JsonConvert.DeserializeObject<CommandData>(jsonData);

                Console.WriteLine("Received command: " + data.Command);


                string user_name = "";

                // 执行命令
                if (data.Command == "COMMAND1") {
                    Console.WriteLine("3333333333333333333333 ");
                    string response;
                    // TODO: 执行相应的操作
                    if (DBClass.CheckAccount(data.Account, data.Password)==true) {
                        Console.WriteLine("444444444444444444444444444 ");
                        // Console.WriteLine("yyyyyyyyy: " + data.Account);

                        user_name = DBClass.GetUsdrName(data.Account);

                        //Console.WriteLine("xxxxxxx: " + user_name);

                        response = "true"+","+ user_name;
                    }
                    else {
                        Console.WriteLine("5555555555555555555555555555 ");
                        response = "false" + "," + user_name;
                    }
                    Console.WriteLine("6666666666666666666666666666 ");
                    byte[] responseData = Encoding.ASCII.GetBytes(response);

                    //Console.WriteLine("將回應資料寫入網路流: " + response);
                    // 將回應資料寫入網路流
                    stream.Write(responseData, 0, responseData.Length);
                    stream.Flush();
                }
                
            }
            catch (Exception ex){
                Console.WriteLine("Error handling client: " + ex.Message);
            }
        }


        private static void StopServer(){
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }
    }
}