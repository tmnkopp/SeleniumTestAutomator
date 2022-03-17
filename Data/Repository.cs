
using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberBalance.CS.Core
{
    public static class Repository<T> where T : class, new()
    {
        public static IEnumerable<T> Get(Func<T, bool> predicate)
        {
            return GetAll().Where(predicate).AsEnumerable<T>();
        }
        public static IEnumerable<T> GetAll()
        {
            IEnumerable<T> results;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CAClientConnectionString"].ConnectionString))
            {
                var meta = typeof(T).GetCustomAttributes(typeof(ORMEntityMap), true).FirstOrDefault() as ORMEntityMap;
                results = conn.Query<T>($"SELECT * FROM {meta.TableName}").AsEnumerable<T>();
            }
            return results;
        }
    }
}
