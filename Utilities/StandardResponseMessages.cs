namespace TimesheetBE.Utilities
{
    public class StandardResponseMessages
    {
        public static string OK => SUCCESSFUL;
        public static string PASSWORD_RESET_EMAIL_SENT = "An email has been sent to you with instructions and next steps";
        public static string ACTIVATE_RESET_EMAIL_SENT = "An email has been sent to you with instructions and next steps";
        public static string SUCCESSFUL = "Successful";
        public static string USER_NOT_PERMITTED = "Sorry you are not permitted to log in as an administrator";
        public static string UNSUCCESSFUL = "Unsuccessful";
        public static string ERROR_OCCURRED = "An Error Occurred, please try again later";
        public static string EMAIL_VERIFIED = "Email verification successful";
        public static string ALREADY_ACTIVATED = "Email already verified";
        public static string EMAIL_VERIFICATION_FAILED =
            "The password reset link you used has expired. Please contact your Admin for assistance";
        public static string USER_NOT_FOUND = "User with this email does not exist";
        public static string PASSWORD_RESET_FAILED =
            "The password reset link you used has expired. Please contact your Admin for assistance";
        public static string PASSWORD_RESET_COMPLETE = "Your password has been reset successfully";
        public static string USER_ALREADY_EXISTS = "A user with this email already exists.";
        public static string CLIENT_ALREADY_HAS_SUPERVISOR = "This client already has a supervisor.";
        internal static string DEELETED;
        public static string NOT_FOUND = "The resource you requested for was not found.";
        public static string SERVICE_ALREADY_EXISTS = "A service with this name already exists";
    }
}
