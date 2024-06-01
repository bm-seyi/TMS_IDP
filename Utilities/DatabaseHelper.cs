using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace TMS_API.Utilities
{
    public interface IDatabaseActions
    {   
        Task<int> UserRegistration(string email, byte[] pwdhash, byte[] salt);
        Task<(byte[]? pwdhash, byte[]? salt)> UserAuthentication(string email);
    }

    public class DatabaseActions : IDatabaseActions
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseActions> _logger;

        public DatabaseActions(ILogger<DatabaseActions> logger)
        {
            _connectionString = Environment.GetEnvironmentVariable("DB_Connection") ?? throw new ArgumentException("Database connection string cannot be null or empty.", nameof(_connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<int> UserRegistration(string email, byte[] pwdhash, byte[] salt)
        {
            try
            { 
                const string sql_query = "INSERT INTO [dbo].[hashed] ([email], [pwdhash], [salt]) VALUES (@Value1, @Value2, @Value3)";
                await using (SqlConnection connection = new SqlConnection(_connectionString))
                { 
                    await connection.OpenAsync();

                    await using (SqlCommand command = new SqlCommand(sql_query, connection)) 
                    {
                        command.Parameters.Add("@Value1", System.Data.SqlDbType.NVarChar).Value = email;
                        command.Parameters.Add("@Value2", System.Data.SqlDbType.VarBinary).Value = pwdhash;
                        command.Parameters.Add("@Value3", System.Data.SqlDbType.VarBinary).Value = salt;

                        _ = await command.ExecuteNonQueryAsync();
                        _logger.LogInformation("Query has been executed. User has now been entered into the database.");
                    }
                }
                return 1;
            } 
            catch (Exception ex)
            {
                _logger.LogError("An error occurred when registering the user. Error Message: {Message}", ex.Message);

                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {Message}", ex.InnerException.Message);
                }
                return -1;
            }
        }

        public async Task<(byte[]? pwdhash, byte[]? salt)> UserAuthentication(string email)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(email))
                {
                    throw new ArgumentException("Email must not be null or empty", nameof(email));
                }

                const string sql_query = "SELECT [pwdhash], [salt] FROM [dbo].[hashed] WHERE [email] = @Value1";

                await using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    await using (SqlCommand command = new SqlCommand(sql_query, connection))
                    {
                        command.Parameters.Add("@Value1", System.Data.SqlDbType.NVarChar).Value = email;
                        await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                    byte[] hashedpwd = (byte[])reader["pwdhash"];
                                    byte[] salt = (byte[])reader["salt"];
                                    _logger.LogInformation("User has been found: {Email}", email);
                                    return (hashedpwd, salt);
                            } else
                            {
                                _logger.LogWarning("No user found with the specified email: {Email}", email);
                                return (null, null);
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                _logger.LogError("An error occurred when trying to authenticating the user: {Message}", ex.Message);
                
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {Message}", ex.InnerException.Message);
                }
                return (null, null);
            }
        }
    }
}