using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BathComplex.DataBase
{
    internal class UserSession
    {
        public static int UserID { get; set; }
        public static string FullName { get; set; }
        public static string UserRole { get; set; }
        public static Users CurrentUser { get; set; }
        public static bool IsAuthenticated { get; set; }

        public static void SetUser(Users user, string role)
        {
            CurrentUser = user;
            UserID = user.UserID;
            FullName = user.FullName;
            UserRole = role;
            IsAuthenticated = true;
        }

        public static void Clear()
        {
            CurrentUser = null;
            UserID = 0;
            FullName = null;
            UserRole = null;
            IsAuthenticated = false;
        }
    }
}
