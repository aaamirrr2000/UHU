using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;

namespace NG.MicroERP.API.Helper
{
    public class DapperFunctions
    {
        private static readonly string DBConnection;

        // --- Static constructor initializes once, thread-safe ---
        static DapperFunctions()
        {
            DBConnection = Debugger.IsAttached
                ? "Server=localhost;Database=dbHikMiott;User ID=DevOps;Password=P.LOEA)A@3D1uNwl;Port=3306;SslMode=Preferred;Pooling=true;Min Pool Size=3;Max Pool Size=50;Connection Lifetime=60;Connection Timeout=15;"
                : "Server=172.16.1.8;Database=dbHikMiott;User ID=moitt;Password=HP0z*shmAYc7qmmm;Port=3306;SslMode=Preferred;Pooling=true;Min Pool Size=3;Max Pool Size=50;Connection Lifetime=60;Connection Timeout=15;";
        }

        // --- Helper method: Create connection (Dapper opens automatically) ---
        private static MySqlConnection GetConnection() => new(DBConnection);

        // =====================================================
        // ================  SELECT Operations =================
        // =====================================================

        public async Task<T?> SearchByID<T>(string tableName, int id = 0)
        {
            string sql = $"SELECT * FROM {tableName}" + (id != 0 ? " WHERE ID = @Id" : "");
            try
            {
                await using var cnn = GetConnection();
                var result = await cnn.QueryAsync<T>(sql, new { Id = id });
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SearchByID Error: {ex.Message}");
                return default;
            }
        }

        public async Task<List<T>?> SearchByQuery<T>(string sql, object? parameters = null)
        {
            try
            {
                await using var cnn = GetConnection();
                var result = await cnn.QueryAsync<T>(sql, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SearchByQuery Error: {ex.Message}");
                return null;
            }
        }

        // =====================================================
        // ================  INSERT / UPDATE / DELETE ===========
        // =====================================================

        public async Task<(bool Success, int InsertedId, string? Message)> Insert(string sqlInsert, string sqlDuplicate = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(sqlDuplicate))
                {
                    if (await Duplicate(sqlDuplicate))
                        return (false, -1, "Duplicate Record Found.");
                }

                await using var cnn = GetConnection();
                string sql = sqlInsert + "; SELECT LAST_INSERT_ID();";
                int insertedId = await cnn.ExecuteScalarAsync<int>(sql);
                return (true, insertedId, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Insert Error: {ex.Message}");
                return (false, 0, ex.Message);
            }
        }

        public async Task<(bool Success, string? Message)> Update(string sqlUpdate, string sqlDuplicate = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(sqlDuplicate))
                {
                    if (await Duplicate(sqlDuplicate))
                        return (false, "Duplicate Record Found.");
                }

                await using var cnn = GetConnection();
                int affected = await cnn.ExecuteAsync(sqlUpdate);
                return affected > 0 ? (true, null) : (false, "Record Not Saved.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update Error: {ex.Message}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> Delete(string table, int id)
        {
            try
            {
                await using var cnn = GetConnection();
                string sql = $"DELETE FROM {table} WHERE Id = @Id";
                int affected = await cnn.ExecuteAsync(sql, new { Id = id });
                return affected > 0 ? (true, "OK") : (false, "Record already in use.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Delete Error: {ex.Message}");
                return (false, ex.Message);
            }
        }

        // =====================================================
        // ================  UTILITY FUNCTIONS =================
        // =====================================================

        public async Task<bool> Duplicate(string sql)
        {
            try
            {
                await using var cnn = GetConnection();
                var result = await cnn.QueryAsync(sql);
                return result.Any();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Duplicate Error: {ex.Message}");
                return false;
            }
        }

        public async Task<(bool Success, string Message)> ExecuteQuery(string sql, object? parameters = null)
        {
            try
            {
                await using var cnn = GetConnection();
                await cnn.ExecuteAsync(sql, parameters);
                return (true, "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExecuteQuery Error: {ex.Message}");
                return (false, ex.Message);
            }
        }

        // =====================================================
        // ================  CODE GENERATION ===================
        // =====================================================

        public async Task<string?> GetCodeAsync(string prefix, string tableName, string field = "Code", int codeLength = 6)
        {
            string format = new('0', codeLength);

            string sql = string.IsNullOrEmpty(prefix)
                ? $@"SELECT MAX(CAST({field} AS UNSIGNED)) AS SEQNO 
                     FROM {tableName} WHERE {field} REGEXP '^[0-9]+$'"
                : $@"SELECT MAX(CAST(SUBSTRING({field}, {prefix.Length + 1}, 
                         CHAR_LENGTH({field}) - {prefix.Length}) AS UNSIGNED)) AS SEQNO
                     FROM {tableName} WHERE LEFT({field}, {prefix.Length}) = '{prefix}'";

            try
            {
                await using var cnn = GetConnection();
                var result = await cnn.QueryFirstOrDefaultAsync<long?>(sql);
                long next = (result ?? 0) + 1;

                return string.IsNullOrEmpty(prefix)
                    ? next.ToString(format)
                    : prefix + next.ToString(format);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetCodeAsync Error: {ex.Message}");
                return null;
            }
        }
    }
}
