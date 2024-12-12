namespace TMS_IDP.Utilities
{
    public static class ApiMessages
    {
        // Log Messages
        public const string InvalidModelStateLog = "Invalid model state: {ModelStateErrors}";
        public const string AuthenticationFailedLog = "Authentication failed. Invalid email or password.";
        public const string StoreRefreshTokenFailedLog = "Failed to store refresh token";
        public const string ErrorMessageLog = "Error Message, {message}";
        public const string InternalErrorMessageLog  = "Inner Exception: {message}";
        public const string UserCreationFailedLog = "Unable to create User";

        // HTTP Status Messages
        public const string InvalidModelStateMessage = "Invalid model state.";
        public const string AuthenticationFailedMessage = "Authentication failed.";
        public const string InternalServerErrorMessage = "An internal server error occurred.";
    }
}
