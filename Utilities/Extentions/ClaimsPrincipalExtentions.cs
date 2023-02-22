using System;
using System.Security.Claims;

namespace TimesheetBE.Utilities.Extentions
{
    public static class ClaimsPrincipalExtentions
    {
        /// <summary>
        /// Get Logged In User Id Using Generic Method With Type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static T GetLoggedInUserId<T>(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            var loggedInUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(loggedInUserId, typeof(T));
            }
            else if (typeof(T) == typeof(int) || typeof(T) == typeof(long))
            {
                return loggedInUserId != null ? (T)Convert.ChangeType(loggedInUserId, typeof(T)) : (T)Convert.ChangeType(0, typeof(T));
            }
            else if (typeof(T) == typeof(Guid))
            {
                return loggedInUserId != null ? (T)Convert.ChangeType(new Guid(loggedInUserId.ToString()), typeof(T)) : (T)Convert.ChangeType(Guid.Empty, typeof(T));
            }
            else
            {
                throw new Exception("Invalid type provided");
            }
        }

        /// <summary>
        /// Get Current Logged On User Name, this username could be an email
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static string GetLoggedInUserName(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirstValue(ClaimTypes.Name);
        }

        /// <summary>
        /// This Method Returns The Current Logged On User Email
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static string GetLoggedInUserEmail(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirstValue(ClaimTypes.Email);
        }

        /// <summary>
        /// This Method Returns The Current Logged On User Role
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static string GetLoggedInUserRole(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirstValue(ClaimTypes.Role);
        }






    }
}