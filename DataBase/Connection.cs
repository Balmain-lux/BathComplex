using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BathComplex.DataBase
{
    internal class Connection
    {
        public static BathComplexDBEntities db = new BathComplexDBEntities();

        // Проверка подключения
        public static bool TestConnection()
        {
            try
            {
                db.Database.Connection.Open();
                db.Database.Connection.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
