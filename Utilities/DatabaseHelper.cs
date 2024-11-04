using Microsoft.Data.SqlClient;

namespace TMS_API.Utilities 
{
    public interface IDatabaseActions
    {   
        Task<bool> UserRegistrationAsync(string email, byte[] pwdhash, byte[] salt);
        Task<bool> StoreRefreshTokenAsync(string clientID, byte[] RefreshToken, DateTime expiry, byte[] hashed);
        Task<(byte[] secret, byte[] iv)?> CredentialsAuthenticationAsync(string clientID);
        Task<(DateTime, byte[], byte[], byte[])?> RefreshTokenAuthenticationAsync(byte[] token);
        Task<(byte[] hashedpwd, byte[] salt, byte[] secret, byte[] IV)?> AuthenticationUserAndCredentialAsync(string email, string clientID);
        Task<(byte[] encryptedToken, byte[] IV)?> AuthenticateRefreshTokenandCredentials(string clientID, byte[] hashedToken);
       
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


        public async Task<bool> UserRegistrationAsync(string email, byte[] pwdhash, byte[] salt)
        {
            try
            { 
                const string sql_query = "INSERT INTO [dbo].[hashed] ([email], [pwdhash], [salt]) VALUES (@Value1, @Value2, @Value3)";
                using (SqlConnection connection = new SqlConnection(_connectionString))
                { 
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(sql_query, connection)) 
                    {
                        command.Parameters.Add("@Value1", System.Data.SqlDbType.NVarChar).Value = email;
                        command.Parameters.Add("@Value2", System.Data.SqlDbType.VarBinary).Value = pwdhash;
                        command.Parameters.Add("@Value3", System.Data.SqlDbType.VarBinary).Value = salt;

                        _ = await command.ExecuteNonQueryAsync();
                        _logger.LogInformation("Query has been executed. User has now been entered into the database.");
                    }
                }
                return true;
            } 
            catch (Exception ex)
            {
                _logger.LogError("An error occurred when registering the user. Error Message: {Message}", ex.Message);

                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {Message}", ex.InnerException.Message);
                }
                return false;
            }
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

        public async Task<(byte[] secret, byte[] iv)?> CredentialsAuthenticationAsync(string clientID)
        {
            if (string.IsNullOrWhiteSpace(clientID)) return null;

            try
            {
                const string query = "SELECT [secret], [iv] FROM [dbo].[Clients] WHERE [ID] = @Value1";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command =  new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Value1", System.Data.SqlDbType.UniqueIdentifier).Value = Guid.Parse(clientID);

                        await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                            {
                                _logger.LogWarning("No data can be found with the specified clientId {1}", clientID.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
                                return null;
                            }

                            byte[] clientSecret = (byte[])reader["secret"];
                            byte[] clientIV = (byte[])reader["iv"];
                            _logger.LogInformation("Client Secret Located");
                            return (clientSecret, clientIV);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred when authenticating credentials. Error Message: {Message}", ex.Message);

                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception:{1}", ex.InnerException.Message.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
                }

                return null;
            }
        }

        public async Task<(DateTime, byte[], byte[], byte[])?> RefreshTokenAuthenticationAsync(byte[] token)
        {
            try
            {
                const string query = "SELECT [expiry], [Key], [iv], [encryptedToken] FROM [dbo].[Tokens] WHERE [hashedToken] = @Value1";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Value1", System.Data.SqlDbType.VarBinary).Value = token;

                        await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                            {
                                _logger.LogWarning("No data can be found with the specified refresh token:{1}", token);
                                return null;
                            }

                            DateTime dateTime = (DateTime)reader["expiry"];
                            byte[] Key = (byte[])reader["Key"];
                            byte[] iv = (byte[])reader["iv"];
                            byte[] refreshToken = (byte[])reader["encryptedToken"];
                            _logger.LogInformation("Refresh Token Located");
                            return (dateTime, Key, iv, refreshToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred when authenticating the refresh token. Error Message: {Message}", ex.Message);

                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception:{1}", ex.InnerException.Message.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
                }

                return null;
            }
        }

        public async Task<(byte[] hashedpwd, byte[] salt, byte[] secret, byte[] IV)?> AuthenticationUserAndCredentialAsync(string email, string clientID)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
                {
                    await sqlConnection.OpenAsync();
                    using (SqlCommand command = new SqlCommand("sp_AuthenticateUserAndClient", sqlConnection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@ClientId", Guid.Parse(clientID));

                        await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                byte[] hashedpwd = (byte[])reader["pwdhash"];
                                byte[] salt = (byte[])reader["salt"];
                                byte[] secret = (byte[])reader["secret"];
                                byte[] IV = (byte[])reader["iv"];

                                return (hashedpwd, salt, secret, IV);
                            }
                        }
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred when authenticating the User and Credentials. Error Message: {Message}", ex.Message);

                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception:{1}", ex.InnerException.Message.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
                }

                return null;
            }
        }

        public async Task<(byte[] encryptedToken, byte[] IV)?> AuthenticateRefreshTokenandCredentials(string clientID, byte[] hashedToken)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
                {
                    await sqlConnection.OpenAsync();
                    using (SqlCommand command = new SqlCommand("sp_AuthenticateRefreshTokenandCredentials", sqlConnection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ClientId", clientID);
                        command.Parameters.AddWithValue("@hashedToken", hashedToken);
                        
                        await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                byte[] encryptedToken = (byte[])reader["encryptedToken"];
                                byte[] IV = (byte[])reader["IV"];

                                return (encryptedToken, IV);
                            }
                        }

                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred when authenticating the Refresh Token and Credentials. Error Message: {Message}", ex.Message);

                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception:{1}", ex.InnerException.Message.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
                }

                return null;
            }
        }
    }
}