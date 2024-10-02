namespace TMS_API.Utilities
{
    public static class ApiMessages
    {
        // Log Messages
        public const string InvalidModelStateLog = "Invalid model state: {ModelStateErrors}";
        public const string UserAuthenticationFailedLog = "User authentication failed: password hash or salt is null.";
        public const string PasswordMismatchLog = "Authentication failed: provided password does not match.";
        public const string CredentialsAuthenticationFailedLog = "Credentials authentication failed for client ID: {ClientId}";
        public const string RefreshTokenNotProvidedLog = "Refresh token was not provided.";
        public const string RefreshTokenAuthenticationFailedLog = "Refresh token authentication failed.";
        public const string RefreshTokenExpiredLog = "Refresh token has expired.";
        public const string InvalidRefreshTokenProvidedLog = "Invalid refresh token provided.";
        public const string StoreRefreshTokenFailedLog = "Failed to store refresh token";
        public const string ProcessingErrorLog = "An error occurred while processing the request.";
        public const string ErrorMessageLog = "Error Message, {message}";
        public const string InternalErrorMessageLog  = "Inner Exception: {message}";
        public const string MissingEncryptionLog = "Encryption key not found. The key was not found in either the application settings or environment variables.";

        // HTTP Status Messages
        public const string InvalidModelStateMessage = "Invalid model state.";
        public const string EmailOrPasswordNotProvidedMessage = "Email or password was not provided.";
        public const string AuthenticationFailedMessage = "Authentication failed.";
        public const string UnsupportedGrantTypeMessage = "The specified grant type is not supported.";
        public const string ClientCredentialsAuthenticationFailedMessage = "Client credentials authentication failed.";
        public const string RefreshTokenNotProvidedMessage = "Refresh token was not provided.";
        public const string RefreshTokenAuthenticationFailedMessage = "Invalid or expired refresh token.";
        public const string RefreshTokenExpiredMessage = "Refresh token has expired.";
        public const string InvalidRefreshTokenProvidedMessage = "Invalid refresh token provided.";
        public const string StoreRefreshTokenFailedMessage = "Failed to store refresh token.";
        public const string InternalServerErrorMessage = "An internal server error occurred.";
    }
}
