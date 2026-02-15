using Dapper;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Data;

namespace MicroERP.API.Helper
{
    public class DapperFunctions
    {
        private readonly Config cfg = new();

        private static string GetConnectionString(string type, Config cfg)
        {
            string connectionString = string.Empty;
            
            switch (type.ToLower())
            {
                case "default":
                    connectionString = cfg.DefaultDB() ?? string.Empty;
                    break;
                case "global":
                    connectionString = cfg.GlobalDB() ?? string.Empty;
                    break;

                default:
                    connectionString = cfg.DefaultDB() ?? string.Empty;
                    break;
            }

            // Ensure pooling and reasonable defaults
            if (!string.IsNullOrEmpty(connectionString) && !connectionString.Contains("Pooling"))
            {
                connectionString += ";Pooling=true;Min Pool Size=5;Max Pool Size=50;";
            }

            return connectionString;
        }

        private static async Task<SqlConnection> GetConnectionAsync(string connectionType, Config cfg)
        {
            var connectionString = GetConnectionString(connectionType, cfg);
            var cnn = new SqlConnection(connectionString);
            await cnn.OpenAsync();
            return cnn;
        }

        public async Task<T?> SearchByID<T>(string tableName, int id = 0, string connection = "Default")
        {
            try
            {
                string sql = $"SELECT * FROM {tableName}" + (id != 0 ? " WHERE Id = @Id" : "");
                await using var cnn = await GetConnectionAsync(connection, cfg);
                var result = await cnn.QueryAsync<T>(sql, new { Id = id });
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SearchByID Error for table {Table}", tableName);
                return default;
            }
        }

        public async Task<List<T>?> SearchByQuery<T>(string sql, string connection = "Default", object? parameters = null)
        {
            try
            {
                await using var cnn = await GetConnectionAsync(connection, cfg);
                var result = parameters != null 
                    ? await cnn.QueryAsync<T>(sql, parameters)
                    : await cnn.QueryAsync<T>(sql);
                return result.ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SearchByQuery Error for query: {SQL}", sql);
                return null;
            }
        }

        public async Task<(bool, string)> ExecuteQuery(string sql, string connection = "Default")
        {
            try
            {
                await using var cnn = await GetConnectionAsync(connection, cfg);
                await cnn.ExecuteAsync(sql);
                return (true, "OK");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ExecuteQuery Error for query: {SQL}", sql);
                return (false, ex.Message);
            }
        }

        public async Task<(bool, int, string?)> Insert(string sqlInsert, string sqlDuplicate = "", string connection = "Default")
        {
            try
            {
                if (!string.IsNullOrEmpty(sqlDuplicate))
                {
                    if (await Duplicate(sqlDuplicate, connection))
                        return (false, -1, "Duplicate Record Found.");
                }

                await using var cnn = await GetConnectionAsync(connection, cfg);
                await using var tran = await cnn.BeginTransactionAsync();

                await cnn.ExecuteAsync(sqlInsert.TrimEnd(';'), transaction: tran);
                int insertedId = Convert.ToInt32(await cnn.ExecuteScalarAsync("SELECT SCOPE_IDENTITY();", transaction: tran));

                await tran.CommitAsync();
                return (true, insertedId, null);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Insert Error for query: {SQLInsert}", sqlInsert);
                return (false, 0, ex.Message);
            }
        }

        public async Task<(bool, string?)> Update(string sqlUpdate, string sqlDuplicate = "", string connection = "Default")
        {
            try
            {
                if (!string.IsNullOrEmpty(sqlDuplicate))
                {
                    if (await Duplicate(sqlDuplicate, connection))
                        return (false, "Duplicate Record Found.");
                }

                await using var cnn = await GetConnectionAsync(connection, cfg);
                int affected = await cnn.ExecuteAsync(sqlUpdate);
                return affected > 0 ? (true, null) : (false, "Update operation completed but no records were affected. The record may not exist, no changes were detected, or the WHERE clause did not match any rows.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Update Error for query: {SQLUpdate}", sqlUpdate);
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> Delete(string table, int id, string connection = "Default")
        {
            try
            {
                await using var cnn = await GetConnectionAsync(connection, cfg);
                string sql = $"DELETE FROM {table} WHERE Id = @Id";
                int affected = await cnn.ExecuteAsync(sql, new { Id = id });
                return affected > 0 ? (true, "OK") : (false, "Record already in use.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Delete Error for table {Table}", table);
                return (false, ex.Message);
            }
        }

        public async Task<bool> Duplicate(string sql, string connection = "Default")
        {
            try
            {
                await using var cnn = await GetConnectionAsync(connection, cfg);
                var result = await cnn.QueryAsync(sql);
                return result.Any();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Duplicate Check Error for query: {SQL}", sql);
                return false;
            }
        }

        public string? GetCode(string prefix, string tableName, string field = "Code", int codeLength = 6, string connection = "Default")
        {
            string format = new('0', codeLength);
            string sql = string.IsNullOrEmpty(prefix)
                ? $@"SELECT MAX(CAST({field} AS INT)) AS SEQNO FROM {tableName} WHERE ISNUMERIC({field}) = 1"
                : $@"SELECT MAX(CAST(SUBSTRING({field}, {prefix.Length + 1}, LEN({field}) - {prefix.Length}) AS INT)) AS SEQNO
                     FROM {tableName} WHERE LEFT({field}, {prefix.Length}) = '{prefix}'";

            try
            {
                using var cnn = new SqlConnection(GetConnectionString(connection, cfg));
                cnn.Open();
                var result = cnn.QueryFirstOrDefault<int?>(sql);

                int next = (result ?? 0) + 1;
                return string.IsNullOrEmpty(prefix) ? next.ToString(format) : prefix + next.ToString(format);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetCode Error for table {TableName}", tableName);
                return null;
            }
        }
    }
}

