using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace CyberBalance.CS.Core 
{
    public class BatchInserter<ItemType>
    {
        #region BUILDER 

        public BatchInserter<ItemType> SetImportTable(string ImportTableName)
        {
            this.ImportTableName = ImportTableName;
            return this;
        }
        public BatchInserter<ItemType> SetImportSproc(string ImportSprocName)
        {
            this.ImportSprocName = ImportSprocName;
            return this;
        }
        public BatchInserter<ItemType> WithEvents()
        {
            this.OnPreInsert += (sender, e) => ExecuteEvent("OnPreBatchInsert", e.cmd);
            this.OnInsertComplete += (sender, e) => ExecuteEvent("OnBatchInsertComplete", e.cmd);
            return this;
        }

        #endregion

        #region PROPS  
        public string ImportTableName { get; set; }
        public string ImportSprocName { get; set; }
        public int BatchSize { get; set; } = 250;
        public int Rows { get; set; } = 0;
        #endregion

        #region CTOR  
        private PropertyInfo[] Props { get; set; }
        private object ItemTypeInstance { get; set; }
        private string ItemTypeName { get; set; }
        private string sql_cols;
        private StringBuilder sql_rows = new StringBuilder();
        public BatchInserter()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(assm => assm.GetTypes())
                        .Where(t => t.Name == typeof(ItemType).Name)
                        .FirstOrDefault();
            this.ItemTypeInstance = Activator.CreateInstance(type);
            this.ItemTypeName = typeof(ItemType).Name;

            this.ImportTableName = $"{this.ItemTypeName}";
            this.ImportSprocName = $"{this.ItemTypeName}_CRUD";
            this.Props = this.ItemTypeInstance.GetType().GetProperties();

        }
        #endregion

        #region EVENTS  

        public class InsertEventArgs : EventArgs
        {
            public SqlConnection conn { get; set; }
            public SqlCommand cmd { get; set; }
            public InsertEventArgs(SqlConnection conn, ref SqlCommand cmd)
            {
                this.conn = conn;
                this.cmd = cmd;
            }
        }
        public event EventHandler<InsertEventArgs> OnPreInsert;
        protected virtual void PreInsert(InsertEventArgs e)
        {
            var cmd = e.cmd;
            OnPreInsert?.Invoke(this, e);
        }
        public event EventHandler<InsertEventArgs> OnInsertComplete;
        protected virtual void InsertComplete(InsertEventArgs e)
        {
            var cmd = e.cmd;
            OnInsertComplete?.Invoke(this, e);
        }
        #endregion

        #region METHODS

        public void BatchInsert(IEnumerable<ItemType> Items)
        {
            IEnumerable<ItemType> feedItems = Items;
            sql_cols = string.Join(",", Props.Select(p => $"{p.Name}").ToArray()).TrimEnd(',');
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CAClientConnectionString"].ConnectionString);
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    try
                    {
                        PreInsert(new InsertEventArgs(conn, ref cmd));
                        cmd = conn.CreateCommand();
                        foreach (var item in feedItems)
                        {
                            Rows++;
                            var sql_row = new StringBuilder();
                            foreach (var prop in Props)
                            {
                                var value = this.ItemTypeInstance.GetType().GetProperty(prop.Name).GetValue(item);
                                cmd.Parameters.AddWithValue($"@{prop.Name}{Rows}", value);
                                sql_row.Append($"@{prop.Name}{Rows},");
                            }
                            sql_rows.Append($"({sql_row.ToString().TrimEnd(',')}),");
                            if (Rows % BatchSize == 0)
                            {
                                InsertBatch(ref conn, ref cmd);
                            }
                        }
                        if (sql_rows.Length >= sql_cols.Length)
                        {
                            InsertBatch(ref conn, ref cmd);
                        }
                        InsertComplete(new InsertEventArgs(conn, ref cmd));
                        cmd = conn.CreateCommand();
                    }
                    finally
                    {
                        cmd.Dispose();
                        conn.Close();
                    }
                    scope.Complete();
                }
            }
            catch (TransactionAbortedException ex)
            {
                Console.WriteLine("TransactionAbortedException Message: {0}", ex.Message);
                throw;
            }
        }
        protected void ExecuteEvent(string Event, SqlCommand cmd)
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"{this.ImportSprocName}";
            cmd.Parameters.AddWithValue($"@MODE", $"{Event}");
            cmd.ExecuteNonQuery();
        }
        private void InsertBatch(ref SqlConnection conn, ref SqlCommand cmd)
        {
            var sql = $"INSERT INTO {this.ImportTableName} ({sql_cols})  VALUES  " + sql_rows.ToString().TrimEnd(',');
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            sql_rows = new StringBuilder();
            cmd = conn.CreateCommand();
        }
        #endregion
    }
}
