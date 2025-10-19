using MySql.Data.MySqlClient;
using Dapper;
using Serilog;
using System.Data;

namespace NG.MicroERP.API.Helper
{
    public class DapperFunctions
    {
        public static string DBConnection = string.Empty;
        public Config cfg = new Config();

        public async Task<T?> SearchByID<T>(string TableName, int id = 0, string Connection = "Default")
        {
            DBConnection = Connection == "Default" ? cfg.DefaultDB()! : cfg.GlobalDB()!;
            try
            {
                string SQL = "SELECT * FROM " + TableName;
                if (id != 0)
                    SQL += " WHERE ID = @Id";

                using IDbConnection connection = new MySqlConnection(DBConnection);
                IEnumerable<T> result = await connection.QueryAsync<T>(SQL, new { Id = id });
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return default;
            }
        }

        public async Task<List<T>?> SearchByQuery<T>(string SQL, string Connection = "Default")
        {
            DBConnection = Connection == "Default" ? cfg.DefaultDB()! : cfg.GlobalDB()!;
            try
            {
                using IDbConnection cnn = new MySqlConnection(DBConnection);
                IEnumerable<T> result = await cnn.QueryAsync<T>(SQL, new DynamicParameters());
                return result.ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return null;
            }
        }

        public async Task<(bool, string)> ExecuteQuery(string SQL, string Connection = "Default")
        {
            DBConnection = Connection == "Default" ? cfg.DefaultDB()! : cfg.GlobalDB()!;
            try
            {
                using IDbConnection cnn = new MySqlConnection(DBConnection);
                _ = await cnn.ExecuteAsync(SQL);
                return (true, "OK");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool, int, string?)> Insert(string SQLInsert, string SQLDuplicate = "", string Connection = "Default")
        {
            DBConnection = Connection == "Default" ? cfg.DefaultDB()! : cfg.GlobalDB()!;

            try
            {
                if (!string.IsNullOrEmpty(SQLDuplicate))
                {
                    if (await Duplicate(SQLDuplicate))
                        return (false, -1, "Duplicate Record Found.");
                }

                using var cnn = new MySqlConnection(DBConnection);
                await cnn.OpenAsync();

                using var tran = cnn.BeginTransaction();

                // 1️⃣ Execute the insert
                await cnn.ExecuteAsync(SQLInsert.TrimEnd(';'), transaction: tran);

                // 2️⃣ Then get the new ID from same connection/transaction
                int insertedId = await cnn.ExecuteScalarAsync<int>("SELECT LAST_INSERT_ID();", transaction: tran);

                tran.Commit();

                return (true, insertedId, null);
            }
            catch (Exception ex)
            {
                return (false, 0, ex.Message);
            }
        }


        public async Task<(bool, string?)> Update(string SQLUpdate, string SQLDuplicate = "", string Connection = "Default")
        {
            DBConnection = Connection == "Default" ? cfg.DefaultDB()! : cfg.GlobalDB()!;
            try
            {
                if (!string.IsNullOrEmpty(SQLDuplicate))
                {
                    if (await Duplicate(SQLDuplicate) == true)
                        return (false, "Duplicate Record Found.");
                }

                using IDbConnection cnn = new MySqlConnection(DBConnection);
                int affectedRows = await cnn.ExecuteAsync(SQLUpdate);
                return affectedRows > 0 ? (true, null) : (false, "Record Not Saved.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> Delete(string Table, int id, string Connection = "Default")
        {
            DBConnection = Connection == "Default" ? cfg.DefaultDB()! : cfg.GlobalDB()!;
            try
            {
                using IDbConnection cnn = new MySqlConnection(DBConnection);
                string SQLDelete = $"DELETE FROM {Table} WHERE Id = @Id";
                int affectedRows = await cnn.ExecuteAsync(SQLDelete, new { Id = id });
                return affectedRows == 0 ? (false, "Record already in Use.") : (true, "OK");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<bool> Duplicate(string SQL, string Connection = "Default")
        {
            DBConnection = Connection == "Default" ? cfg.DefaultDB()! : cfg.GlobalDB()!;
            try
            {
                using IDbConnection cnn = new MySqlConnection(DBConnection);
                IEnumerable<dynamic> result = await cnn.QueryAsync(SQL);
                return result.Any();
            }
            catch
            {
                return false;
            }
        }

        public string? GetCode(string Prefix, string TableName, string? Field = "Code", int CodeLength = 6, string? Connection = "Default")
        {
            DBConnection = Connection == "Default" ? cfg.DefaultDB()! : cfg.GlobalDB()!;
            string SQL;
            string format = new('0', CodeLength);

            if (!string.IsNullOrEmpty(Prefix))
            {
                // MySQL SUBSTRING syntax: SUBSTRING(str, pos, len)
                SQL = $@"SELECT MAX(CAST(SUBSTRING({Field}, {Prefix.Length + 1}, CHAR_LENGTH({Field}) - {Prefix.Length}) AS UNSIGNED)) AS SEQNO
                         FROM {TableName} 
                         WHERE LEFT({Field}, {Prefix.Length}) = '{Prefix}'";
            }
            else
            {
                SQL = $@"SELECT MAX(CAST({Field} AS UNSIGNED)) AS SEQNO 
                         FROM {TableName} 
                         WHERE {Field} REGEXP '^[0-9]+$'";
            }

            try
            {
                using IDbConnection cnn = new MySqlConnection(DBConnection);
                var result = cnn.QueryFirstOrDefault<long?>(SQL);
                if (!string.IsNullOrEmpty(Prefix))
                {
                    return result == null ? Prefix + 1.ToString(format) : Prefix + (result.Value + 1).ToString(format);
                }
                else
                {
                    return result == null ? 1.ToString(format) : (result.Value + 1).ToString(format);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return null;
            }
        }
    }
}
