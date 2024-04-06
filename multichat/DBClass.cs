using System;
using System.Data.SqlClient;
using System.Media;

namespace DBManager
{
    public class DBClass
    {
        public static bool CheckAccount(string account, string password)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\wilso\Desktop\multichat\multichat\Database1.mdf;Integrated Security=True";
            string query = "SELECT COUNT(*) FROM [Table] WHERE account = @Account AND password = @Password";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
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
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\wilso\Desktop\multichat\multichat\Database1.mdf;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO [Table] (name, account, password, lastlogintime,isLogin) VALUES (@Name, @Account, @Password, @LastLoginTime, @IsLogin)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Account", account);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@LastLoginTime", DateTime.Now);
                    command.Parameters.AddWithValue("@IsLogin", defaultisLogin);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public static string GetUsdrName(string account)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\wilso\Desktop\multichat\multichat\Database1.mdf;Integrated Security=True";
            string name = "";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM [Table] WHERE account = @Account";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Account", account);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            name = reader["name"].ToString();
                        }
                    }
                }
            }
            return name;
        }

        public static void UpdateLastLoginTime(string name)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\wilso\Desktop\multichat\multichat\Database1.mdf;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE [Table] SET lastlogintime = @LastLoginTime WHERE name = @Name";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LastLoginTime", DateTime.Now);
                    command.Parameters.AddWithValue("@Name", name);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateUserLoginState(string name, int isLogin)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\wilso\Desktop\multichat\multichat\Database1.mdf;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE [Table] SET isLogin = @isLogin WHERE name = @Name";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@isLogin", isLogin);
                    command.Parameters.AddWithValue("@Name", name);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        SoundPlayer player = new SoundPlayer(@"C:\Users\wilso\Desktop\multichat\Sound\Notify.wav");
                        player.Play();
                    }
                }
            }
        }

        public static bool CheckisOnline(string name)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\wilso\Desktop\multichat\multichat\Database1.mdf;Integrated Security=True";
            string query = "SELECT isLogin FROM [Table] WHERE name = @Name";
            string isLogin = "0";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            isLogin = reader["isLogin"].ToString();
                        }
                    }
                }
            }
            return isLogin == "1";
        }
    }
}
