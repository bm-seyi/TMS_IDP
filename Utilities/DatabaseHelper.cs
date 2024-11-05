using Microsoft.Data.SqlClient;

namespace TMS_API.Utilities 
{
    public interface IDatabaseActions
    {   
        Task<bool> StoreRefreshTokenAsync(string clientID, byte[] RefreshToken, DateTime expiry, byte[] hashed);
    }

    public class DatabaseActions : IDatabaseActions
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseActions> _logger;
   
        public DatabaseActions(ILogger<DatabaseActions> logger, IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionStrings:Development"] ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> StoreRefreshTokenAsync(string clientID, byte[] RefreshToken, DateTime expiry, byte[] hashed)
        {
            if (string.IsNullOrWhiteSpace(clientID)) return false;

            try
            {
                const string query = "INSERT INTO [dbo].[Tokens] ([client_id], [encryptedToken], [expiry], [hashedToken]) VALUES (@Value1, @Value2, @Value3, @Value4)";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command =  new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Value1", System.Data.SqlDbType.NVarChar).Value = clientID;
                        command.Parameters.Add("@Value2", System.Data.SqlDbType.VarBinary).Value = RefreshToken;
                        command.Parameters.Add("@Value3", System.Data.SqlDbType.DateTime2).Value = expiry;
                        command.Parameters.Add("@Value4", System.Data.SqlDbType.VarBinary).Value = hashed;

                        _ = await command.ExecuteNonQueryAsync();
                        _logger.LogInformation("Query has been executed. New Token has been registered");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred when registering the refresh token. Error Message: {Message}", ex.Message);

                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {Message}", ex.InnerException.Message);
                }
                return false;
            }
        }
    }
}