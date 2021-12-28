using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberScope.Data.Tests
{
    public class Column
    {
        #region PROPS

        public string COLUMN_NAME { get; set; }
        public string TABLE_NAME { get; set; }
        public string DATA_TYPE { get; set; }

        #endregion
    }
    public class Table
    { 
        #region PROPS

        public Table()
        {
            List<Column> Columns = new List<Column>();
        }
        public string TABLE_NAME { get; set; }
        public List<Column> Columns { get; set; }

        #endregion 
    }
    public static class INFORMATION_SCHEMA
    {
        public static IEnumerable<Table> Get(Func<Table, bool> predicate)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CAClientConnectionString"].ConnectionString))
            {
                const string sql = @"
	                SELECT T.TABLE_NAME, C.COLUMN_NAME, C.TABLE_NAME 
	                FROM INFORMATION_SCHEMA.TABLES T
	                INNER JOIN INFORMATION_SCHEMA.COLUMNS C ON C.TABLE_NAME=T.TABLE_NAME
                ";
                var dict = new Dictionary<string, Table>();
                conn.Open();
                var result = conn.Query<Table, Column, Table>(
                        sql,
                        (table, column) =>
                        {
                            Table tableEntry;
                            if (!dict.TryGetValue(table.TABLE_NAME, out tableEntry))
                            {
                                tableEntry = table;
                                tableEntry.Columns = new List<Column>();
                                dict.Add(tableEntry.TABLE_NAME, tableEntry);
                            }
                            tableEntry.Columns.Add(column);
                            return tableEntry;
                        },  splitOn: "TABLE_NAME").Distinct().ToList();
                return result.Where(predicate).AsEnumerable();
            }
        }
    }
}
