using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Data
{
    public static class Extensions
    {
        public static void ClearData(this DbContext me, bool usePartialExcludedTableName = false, bool autoExludeMigrationHistory = true, params string[] excludedTable)
        {
            using (var connection = new SqlConnection(me.Database.Connection.ConnectionString))
            {
                // Open connection
                connection.Open();
                // Get tables
                var tables =  (from DataRow row in connection.GetSchema("Tables").Rows select row[2].ToString()).ToList();
                // Compute exlusions
                if (autoExludeMigrationHistory && !excludedTable.Contains("__MigrationHistory"))
                {
                    var list = excludedTable.ToList();
                    list.Add("__MigrationHistory");
                    excludedTable = list.ToArray();
                }
                var exclusions = usePartialExcludedTableName ?
                    tables.Where(t => excludedTable.Any(t.Contains)) :
                    tables.Where(excludedTable.Contains);
                // Remove exlusions
                tables.RemoveAll(exclusions.Contains);
                // Deactivate db consistency check
                foreach (var table in tables)
                {
                    var com = connection.CreateCommand();
                    com.CommandText = "ALTER TABLE [" + table + "] NOCHECK CONSTRAINT ALL";
                    com.ExecuteNonQuery();
                }
                // Remove all data from tables
                foreach (var table in tables)
                {
                    var com = connection.CreateCommand();
                    com.CommandText = "DELETE FROM [" + table + "]";
                    com.ExecuteNonQuery();
                }
                // Activate db consistency check
                foreach (var table in tables)
                {
                    var com = connection.CreateCommand();
                    com.CommandText = "ALTER TABLE [" + table + "] CHECK CONSTRAINT ALL";
                    com.ExecuteNonQuery();
                }
                // Close connection
                connection.Close();
            }
        }
    }
}
