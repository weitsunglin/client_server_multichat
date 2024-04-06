using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DBManager;
using Newtonsoft.Json;

namespace Client
{
    class Program
    {
        static string serverIP = "192.168.1.101";
        static int MAIN_SESRVER_PROTOCOL = 5000;
        private const int COMMAND_SERVER = 6000;
        private static TcpClient client_command;
        private static NetworkStream stream_command;
        private static string userNameFromServerDB = "";

        static void Main(string[] args)
        {
            welcome();
            Console.ReadLine();
            DisconnectFromServer();
        }

        public static void welcome()
        {
            ConnectToServer();
            bool isSucLog = false;
            bool isSucReg = false;
            Console.WriteLine("歡迎來到多人聊天系統");
            Console.WriteLine("請問您有帳號密碼嗎，有請輸入輸入Y，沒則輸入N");
            string input = Console.ReadLine();
            if (input == "Y")
            {
                isSucLog = Login();
            }
            else if (input == "N")
            {
                isSucReg = Register();
            }
            else
            {
                Console.WriteLine("無效的選擇，請重新輸入");
            }

            if (isSucLog)
            {
                Console.WriteLine("登入成功");
                DBClass.UpdateUserLoginState(userNameFromServerDB, 1);
                DBClass.UpdateLastLoginTime(userNameFromServerDB);
                StartClient(MAIN_SESRVER_PROTOCOL);
            }
            else if (isSucReg)
            {
                Console.WriteLine("註冊成功請重新登入。");
                client_command.Close();
                stream_command.Close();
                welcome();
            }
            else if (!isSucLog)
            {
                Console.WriteLine("登入失敗。");
                client_command.Close();
                stream_command.Close();
                welcome();
            }
            else if (!isSucReg)
            {
                Console.WriteLine("註冊失敗。");
            }

            client_command.Close();
            stream_command.Close();
        }

        public static bool Register()
        {
            Console.WriteLine("請輸入名字");
            string name = Console.ReadLine();
            Console.WriteLine("請輸入帳號");
            string account = Console.ReadLine();
            Console.WriteLine("請輸入密碼");
            string password = Console.ReadLine();
            bool isSucReg = DBClass.Register(name, account, password);
            return isSucReg;
        }

        public static bool Login()
        {
            Console.WriteLine("請輸入帳號：");
            string account = Console.ReadLine();
            Console.WriteLine("請輸入密碼：");
            string password = Console.ReadLine();

            SendCommand("COMMAND1", account, password);

            string nameAndisFound = OnCommand();
            string[] nameAndisFoundarr = nameAndisFound.Split(',');
            bool found = nameAndisFoundarr[0].Trim() == "true";
            userNameFromServerDB = nameAndisFoundarr[1].Trim();

            if (!found)
            {
                Console.WriteLine("沒有這組帳號密碼哦。");
            }
            else if (DBClass.CheckisOnline(userNameFromServerDB))
            {
                Console.WriteLine("該帳號在線哦....");
                return false;
            }

            return found;
        }

        public static void StartClient(int PROTOCOL)
        {
            TcpClient tcpClient = new TcpClient(serverIP, PROTOCOL);
            StreamWriter sWriter = new StreamWriter(tcpClient.GetStream());
            if (tcpClient.Connected)
            {
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
                        StringBuilder inputBuilder = new StringBuilder(userNameFromServerDB + ": ");
                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                            inputBuilder.Append(keyInfo.KeyChar);

                            if (isFirstChar)
                            {
                                Console.Write(userNameFromServerDB + keyInfo.KeyChar);
                                isFirstChar = false;
                            }
                            else
                            {
                                Console.Write(keyInfo.KeyChar);
                            }

                            if (keyInfo.Key == ConsoleKey.Enter)
                                break;
                        }

                        Console.WriteLine();
                        string input = inputBuilder.ToString();
                        Console.WriteLine();
                        isFirstChar = true;

                        sWriter.WriteLine(input);
                        sWriter.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public static void Read(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
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

        private static void ConnectToServer()
        {
            try
            {
                client_command = new TcpClient();
                client_command.Connect(serverIP, COMMAND_SERVER);
                Console.WriteLine("連線到command server");
                stream_command = client_command.GetStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to connect to command server: " + ex.Message);
            }
        }

        private static void SendCommand(string command, string account, string password)
        {
            try
            {
                CommandData data = new CommandData { Command = command, Account = account, Password = password };
                byte[] dataBytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data));
                stream_command.Write(dataBytes, 0, dataBytes.Length);
                Console.WriteLine("Command sent: " + command);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send command: " + ex.Message);
            }
        }

        private static string OnCommand()
        {
            byte[] responseData = new byte[1024];
            int bytesRead = stream_command.Read(responseData, 0, responseData.Length);
            string response = Encoding.ASCII.GetString(responseData, 0, bytesRead);
            Console.WriteLine("Response OnCommand: " + response);
            return response;
        }

        private static void DisconnectFromServer()
        {
            Console.WriteLine("系統結束");
            DBClass.UpdateUserLoginState(userNameFromServerDB, 0);
            client_command.Close();
            stream_command.Close();
        }

        public class CommandData
        {
            public string Command { get; set; }
            public string Account { get; set; }
            public string Password { get; set; }
        }
    }
}
