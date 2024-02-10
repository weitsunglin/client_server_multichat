using System;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text;
using DBManager;
using System.Security.Principal;
using Newtonsoft.Json; //NuGet 管理器安装
using System.Net.Http;


namespace Client
{
    class Program{

        static string serverIP = "192.168.1.101";
        static int MAIN_SESRVER_PROTOCOL = 5000;
        private const int COMMAND_SERVER = 6000; 
        private static TcpClient client_command;
        private static NetworkStream stream_command;
        private static string userNameFromServerDB = "";

        static void Main(string[] args){
            welcome();
            Console.ReadLine();
            DisconnectFromServer();
        }

        public static void welcome(){

            ConnectToServer();
            bool isSucLog = false;
            bool isSucReg = false;
            Console.WriteLine("歡迎來到多人聊天系統");
            Console.WriteLine("請問您有帳號密碼嗎，有請輸入輸入Y，沒則輸入N");
            string input = Console.ReadLine();
            if (input == "Y"){
                isSucLog = Login();
            }
            else if (input == "N"){
                isSucReg = Register();
            }
            else{
                Console.WriteLine("無效的選擇，請重新輸入");
            }
            if (isSucLog==true){
                Console.WriteLine("登入成功");
                DBClass.UpdateUserLoginState(userNameFromServerDB,1);
                DBClass.UpdateLastLoginTime(userNameFromServerDB);
                StartClient(MAIN_SESRVER_PROTOCOL);
            }
            else if(isSucReg==true){
                Console.WriteLine("註冊成功請重新登入。");
                client_command.Close();
                stream_command.Close();
                welcome();
            }
            else if (isSucLog == false){
                Console.WriteLine("登入失敗。");
                client_command.Close();
                stream_command.Close();
                welcome();
            }
            else if (isSucReg == false){
                Console.WriteLine("註冊失敗。");
            }
            else {
                
            }
            // 关闭连接
            //client_command.Close();
            //stream_command.Close();
            client_command.Close();
            stream_command.Close();
        }

        public static bool Register() {
            Console.WriteLine("請輸入名字");
            string name = Console.ReadLine();
            Console.WriteLine("請輸入帳號");
            string account = Console.ReadLine();
            Console.WriteLine("請輸入密碼");
            string password = Console.ReadLine();
            bool isSucReg = DBClass.Register(name, account, password);
            if (isSucReg)
            {
                Console.WriteLine("註冊成功");
                return true;
            }
            else
            {
                Console.WriteLine("註冊失敗");
                return false;
            }
        }

        public static bool Login(){
            // 在这里编写操作A的代码逻辑
            Console.WriteLine("請輸入帳號：");
            string account = Console.ReadLine();
            Console.WriteLine("請輸入密碼：");
            string password = Console.ReadLine();

            bool found = false;

            SendCommand("COMMAND1",account, password);

            string nameAndisFound = OnCommand();

            string[] nameAndisFoundarr = nameAndisFound.Split(',');
            string trueName = nameAndisFoundarr[0].Trim();
            userNameFromServerDB = nameAndisFoundarr[1].Trim();

            if (trueName == "true")
            {
                found = true;
                Console.WriteLine("成功找到這組帳號密碼");
            }
            else
            {
                found = false;
                Console.WriteLine("沒有這組帳號密碼哦。");
            }

            if (  DBClass.CheckisOnline(userNameFromServerDB) == true)
            {
                Console.WriteLine("該帳號在線哦....");
                return false;
            }

            return found;
        }



        //-------------------Client( 聊天用 )--------------------\\


        public static void StartClient(int PROTOCOL){
            //Random rnd = new Random();
            //int ID = rnd.Next(0, 999999);
            TcpClient tcpClient = new TcpClient( serverIP, PROTOCOL);
            StreamWriter sWriter = new StreamWriter(tcpClient.GetStream());
            if (tcpClient.Connected)
            {
                //Console.WriteLine("tcpClient" + ID.ToString() + "Connected to server");
                sWriter.WriteLine(userNameFromServerDB);
                sWriter.Flush();
            }
            Thread thread = new Thread(Read);
            thread.Start(tcpClient);
            try
            {

                bool isFirstChar = true;

                while (true)
                {
                    if (tcpClient.Connected)
                    {
                        // string input = userNameFromServerDB + Console.ReadLine();

                        StringBuilder inputBuilder = new StringBuilder(userNameFromServerDB + ": ");

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

               

                            // 在输入字符串后附加按键字符
                            inputBuilder.Append(keyInfo.KeyChar);

                            if (isFirstChar)
                            {
                                // 在光标位置后直接显示按键字符
                                Console.Write(userNameFromServerDB+keyInfo.KeyChar);
                                isFirstChar = false;
                            }
                            else
                            {
                                // 在光标位置后直接显示按键字符
                                Console.Write(keyInfo.KeyChar);
                            }

                            // 按下 Enter 键表示输入结束
                            if (keyInfo.Key == ConsoleKey.Enter)
                                break;

                        }


                        Console.WriteLine();
                        string input = inputBuilder.ToString();
                        Console.WriteLine();

                        isFirstChar = true;

                        /*if (userNameFromServerDB != "")
                        {
                            sWriter.WriteLine("clientID"  + "(" + userNameFromServerDB + ")" + ":" + input);
                        }
                        else
                        {
                            sWriter.WriteLine( input);
                        }*/
                        sWriter.WriteLine(input);
                        sWriter.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            Console.ReadKey();
        }

        public static void Read(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj; //轉型
            StreamReader sReader = new StreamReader(tcpClient.GetStream());
            while (true)
            {
                try
                {
                    string message = sReader.ReadLine();
                    Console.WriteLine(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
        }

        //-------------------Client( send and onCommand )--------------------\\


        private static void ConnectToServer(){

           
            try
            {
                // 创建 TCP 客户端 Socket
                client_command = new TcpClient();
                // 连接到服务器
                client_command.Connect(serverIP, COMMAND_SERVER);
                Console.WriteLine("連線到command server");
                // 获取网络流
                stream_command = client_command.GetStream();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to connect to 連線到command server: " + ex.Message);
            }
        }

        public class CommandData
        {
            public string Command { get; set; }
            public string Account { get; set; }
            public string Password { get; set; }
        }

        // 发送命令
        private static void SendCommand(string command, string account, string password)
        {
            try
            {
                // 创建要发送的数据对象
                CommandData data = new CommandData
                {
                    Command = command,
                    Account = account,
                    Password = password
                };

                
                // 将数据对象转换为字节数组
                byte[] dataBytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data));

                // 发送数据
                stream_command.Write(dataBytes, 0, dataBytes.Length);
                Console.WriteLine("Command sent: " + command);
                Array.Clear(dataBytes, 0, dataBytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send command: " + ex.Message);
            }
        }

        private static string OnCommand( )
        {
            // 接收服务器的响应
            byte[] responseData = new byte[1024];
            int bytesRead = stream_command.Read(responseData, 0, responseData.Length);
            string response = Encoding.ASCII.GetString(responseData, 0, bytesRead);
            Console.WriteLine("Response OnCommand: " + response);
            return response;
        }

        private static void DisconnectFromServer(){

            Console.WriteLine("系統結束");
            DBClass.UpdateUserLoginState(userNameFromServerDB, 0);
            client_command.Close();
            stream_command.Close();
        }
    }
}