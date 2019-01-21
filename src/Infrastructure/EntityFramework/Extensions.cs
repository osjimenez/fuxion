using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.EntityFramework
{
	public static class Extensions
	{
		public static void ClearData(this DbContext me, bool usePartialExcludedTableName = false, bool autoExludeMigrationHistory = true, params string[] excludedTable)
		{
			using (SqlConnection connection = new SqlConnection(me.Database.Connection.ConnectionString))
			{
				// Open connection
				connection.Open();
				// Get tables
				List<string> tables = (from DataRow row in connection.GetSchema("Tables").Rows select row[2].ToString()).ToList();
				// Compute exlusions
				if (autoExludeMigrationHistory && !excludedTable.Contains("__MigrationHistory"))
				{
					List<string> list = excludedTable.ToList();
					list.Add("__MigrationHistory");
					excludedTable = list.ToArray();
				}
				IEnumerable<string> exclusions = usePartialExcludedTableName ?
					tables.Where(t => excludedTable.Any(t.Contains)) :
					tables.Where(excludedTable.Contains);
				// Remove exlusions
				tables.RemoveAll(exclusions.Contains);
				// Deactivate db consistency check
				foreach (string table in tables)
				{
					SqlCommand com = connection.CreateCommand();
					com.CommandText = $"ALTER TABLE [{table}] NOCHECK CONSTRAINT ALL";
					com.ExecuteNonQuery();
				}
				// Remove all data from tables
				foreach (string table in tables)
				{
					SqlCommand com = connection.CreateCommand();
					com.CommandText = $"DELETE FROM [{table}]";
					com.ExecuteNonQuery();
				}
				// Activate db consistency check
				foreach (string table in tables)
				{
					SqlCommand com = connection.CreateCommand();
					com.CommandText = $"ALTER TABLE [{table}] CHECK CONSTRAINT ALL";
					com.ExecuteNonQuery();
				}
				// Close connection
				connection.Close();
			}
		}
		#region Sequences
		public static int CreateSequence(this DbContext me, string name, int startWith = 1, int increment = 1, int minValue = 1, int maxValue = int.MaxValue, bool cycle = false)
		{
			SqlConnection con = new SqlConnection(me.Database.Connection.ConnectionString);
			con.Open();
			SqlCommand com = new SqlCommand($@"CREATE SEQUENCE [{name}]
				AS int
				START WITH {startWith}
				INCREMENT BY {increment}
				MINVALUE {minValue}
				MAXVALUE {maxValue}
				{(cycle ? " CYCLE" : "")}", con);
			int res = Convert.ToInt32(com.ExecuteScalar());
			con.Close();
			return res;
		}
		public static int DeleteSequence(this DbContext me, string name)
		{
			SqlConnection con = new SqlConnection(me.Database.Connection.ConnectionString);
			con.Open();
			SqlCommand com = new SqlCommand($"DROP SEQUENCE [{name}]", con);
			int res = Convert.ToInt32(com.ExecuteScalar());
			con.Close();
			return res;
		}
		public static int GetSequenceValue(this DbContext me, string name, bool increment = true)
		{
			SqlConnection con = new SqlConnection(me.Database.Connection.ConnectionString);
			con.Open();
			SqlCommand com;
			if (increment)
			{
				com = new SqlCommand($"SELECT NEXT VALUE FOR [{name}]", con);
			}
			else
			{
				com = new SqlCommand($"SELECT current_value FROM sys.sequences WHERE name = '{name}'", con);
			}

			int res = Convert.ToInt32(com.ExecuteScalar());
			con.Close();
			return res;
		}
		public static void SetSequenceValue(this DbContext me, string name, int value)
		{
			SqlConnection con = new SqlConnection(me.Database.Connection.ConnectionString);
			con.Open();
			SqlCommand com = new SqlCommand($"ALTER SEQUENCE [{name}] RESTART WITH {value}", con);
			com.ExecuteNonQuery();
			con.Close();
		}
		#endregion
	}
}