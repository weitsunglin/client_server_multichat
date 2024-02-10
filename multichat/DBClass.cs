
using System.Data;
using System.Data.SqlClient; //NuGet 管理器安装
using System;
using System.Runtime.CompilerServices;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Media;

namespace DBManager
{
    public class DBClass
	{

        public static bool CheckAccount(string account, string password){
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;
                AttachDbFilename=C:\Users\wilso\Desktop\multichat\multichat\Database1.mdf;Integrated Security=True";
            string query = "SELECT COUNT(*) FROM [Table] WHERE account = @Account AND password = @Password";

            using (SqlConnection connection = new SqlConnection(connectionString)){
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Account", account);
                    command.Parameters.AddWithValue("@Password", password);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public static bool Register(string name, string account, string password)
        {
             int defaultisLogin = 0;

            // 在这里编写操作B的代码逻辑
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;
                AttachDbFilename=C:\Users\wilso\Desktop\multichat\multichat\Database1.mdf;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO [Table] (name, account, password, lastlogintime,isLogin) " +
                        "VALUES (@Name, @Account, @Password, @LastLoginTime,@IsLogin)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // 使用参数化查询来避免 SQL 注入攻击
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Account", account);
                        command.Parameters.AddWithValue("@Password", password);
                        command.Parameters.AddWithValue("@LastLoginTime", DateTime.Now);
                        command.Parameters.AddWithValue("@IsLogin", defaultisLogin);

                        // 执行插入操作
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("成功註冊帳號密碼");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("註冊帳號密碼失敗");
                            return false;
                        }
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("註冊帳號密碼出現異常：" + ex.Message);
                    return false;
                }
            }

        }

        public static string GetUsdrName( string _account )
        {
            // 定义连接字符串 
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;
                AttachDbFilename=C:\Users\wilso\Desktop\multichat\multichat\Database1.mdf;Integrated Security=True";

            string name = "";

            // 创建SqlConnection对象
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try{
                    // 打开数据库连接
                    connection.Open();

                    string query = "SELECT * FROM [Table] WHERE account = @_account";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // 使用参数化查询来避免 SQL 注入攻击
                        command.Parameters.AddWithValue("@_account", _account );

                        // 执行查询并获取结果
                        SqlDataReader reader = command.ExecuteReader();

                        // 遍历结果并输出
                        while (reader.Read()){
                            name = reader["name"].ToString();
                        }
                        reader.Close();
                    }

                    // 关闭数据库连接
                    connection.Close();

                    return name;

                }
                catch (Exception ex)
                {
                    // 处理连接过程中的异常
                    Console.WriteLine("資料庫連線出現異常：" + ex.Message);
                }
            }

            return name;
        }


        public static void UpdateLastLoginTime(string name )
        {
            string connectString = @"Data Source=(LocalDB)\MSSQLLocalDB;
                AttachDbFilename=C:\Users\wilso\Desktop\multichat\multichat\Database1.mdf;Integrated Security=True";


            using (SqlConnection connection = new SqlConnection(connectString))
            {
                connection.Open();

                string query = "UPDATE [Table] SET lastlogintime = @LastLoginTime WHERE name = @Name";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // 使用参数化查询来避免 SQL 注入攻击
                    command.Parameters.AddWithValue("@LastLoginTime", DateTime.Now);
                    command.Parameters.AddWithValue("@Name", name);

                    // 执行查询
                    command.ExecuteNonQuery();
                }
            }
        }


        ///-----------------------------------------------------------------------

        public static void UpdateUserLoginState(string name, int islogin)
        {
            //isLogin 0 = 沒上，1 = 有上
            string connectString = @"Data Source=(LocalDB)\MSSQLLocalDB;
                AttachDbFilename=C:\Users\wilso\Desktop\multichat\multichat\Database1.mdf;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectString)){
                connection.Open();

                string query = "UPDATE [Table] SET isLogin = @islogin WHERE name = @Name";


                using (SqlCommand command = new SqlCommand(query, connection)){
                    command.Parameters.AddWithValue("@isLogin", islogin);
                    command.Parameters.AddWithValue("@Name", name);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        //Console.WriteLine("Update successful!");
                        
                            SoundPlayer player = new SoundPlayer();
                            player.SoundLocation = @"C:\Users\wilso\Desktop\multichat\Sound\Notify.wav"; // 指定系統通知音效的路徑
                            player.Play();
                    }
                    else
                    {
                        //Console.WriteLine("No matching records found.");
                    }
                }

                    connection.Close();
            }
        }


        public static bool CheckisOnline( string _name )
        {
            string isLogin = "0";

            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;
                AttachDbFilename=C:\Users\wilso\Desktop\multichat\multichat\Database1.mdf;Integrated Security=True";

            string query = "SELECT isLogin  FROM [Table] WHERE name = @Name";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // 使用参数化查询来避免 SQL 注入攻击
                    command.Parameters.AddWithValue("@Name", _name);
                    // 执行查询并获取结果
                    SqlDataReader reader = command.ExecuteReader();
                     // 遍历结果并输出
                    while (reader.Read()){
                    isLogin = reader["isLogin"].ToString();
                    }
                    reader.Close();
                }
                // 关闭数据库连接
                connection.Close();

                if (isLogin == "0")
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
        }
    }
}