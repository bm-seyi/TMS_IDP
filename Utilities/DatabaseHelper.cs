using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace  TMS_API.Utilities;

public class DatabaseActions
{
    private static readonly string connectionString;

    static DatabaseActions()
    {
        connectionString = Environment.GetEnvironmentVariable("DB_Connection") ?? throw new ArgumentException("Database connection string cannot be null or empty.", nameof(connectionString));
    }


    public static void UserRegistration(string email, byte[] pwdhash, byte[] salt)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        { 
            string sql_query = "INSERT INTO [dbo].[hashed] ([email], [pwdhash], [salt]) VALUES (@Value1, @Value2, @Value3)";
            connection.Open();

            using (SqlCommand command = new SqlCommand(sql_query, connection)) 
            {
                command.Parameters.AddWithValue("@Value1", email);
                command.Parameters.AddWithValue("@Value2", pwdhash);
                command.Parameters.AddWithValue("@Value3", salt);

                int rowsAffected = command.ExecuteNonQuery();
            }
            
            connection.Close();
        }
    }

    public static (byte[] pwdhash, byte[] salt) UserAuthetication(string email)
    {
        if(string.IsNullOrEmpty(email))
        {
            throw new ArgumentException("Email must not be null or empty", nameof(email));
        }

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            const string sql_query = "SELECT [pwdhash], [salt] FROM [dbo].[hashed] WHERE [email] = @Value1";
            connection.Open();

            using (SqlCommand command = new SqlCommand(sql_query, connection))
            {
                command.Parameters.AddWithValue("@Value1", email);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                   if (reader.Read())
                   {
                        byte[] hashedpwd = reader["pwdhash"] as byte[] ?? throw new ArgumentNullException();
                        byte[] salt = reader["salt"] as byte[] ?? throw new ArgumentNullException();
                        return (hashedpwd, salt);
                   } else
                   {
                     throw new InvalidOperationException("No user found with the specified email.");
                   }
                }
            }
        }
    }

}