using Dapper;

using Microsoft.Data.SqlClient;

using Serilog;

using System.Data;

namespace NG.MicroERP.API.Helper;

public class DapperFunctions
{
    public static string DBConnection = string.Empty;
    public Config cfg = new Config();

    public async Task<T?> SearchByID<T>(string TableName, int id = 0, string Connection = "Default")
    {
        DBConnection = Connection == "Default" ? cfg.DefaultDB()! : cfg.GlobalDB()!;

        try
        {
            string SQL = "Select * from " + TableName;
            if (id != 0)
            {
                SQL += " where ID = " + id;
            }

            using IDbConnection connection = new SqlConnection(DBConnection);
            IEnumerable<T> result = await connection.QueryAsync<T>(SQL, new DynamicParameters());
            //Log.Information("Command Executed: " + SQL);
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
            using IDbConnection cnn = new SqlConnection(DBConnection);
            IEnumerable<T> result = await cnn.QueryAsync<T>(SQL, new DynamicParameters());
            //Log.Information("Command Executed: " + SQL);
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
            using IDbConnection cnn = new SqlConnection(DBConnection);
            _ = await cnn.ExecuteAsync(SQL);
            //Log.Information("Command Executed: " + SQL);
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
                if (await Duplicate(SQLDuplicate) == true)
                {
                    return (false, -1, "Duplicate Record Found.");
                }
            }

            using IDbConnection cnn = new SqlConnection(DBConnection);
            string sql = SQLInsert + "SELECT CAST(SCOPE_IDENTITY() as int)";
            int insertedId = await cnn.ExecuteScalarAsync<int>(sql);
            //Log.Information("Command Executed: " + SQLInsert);
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
                {
                    return (false, "Duplicate Record Found.");
                }
            }

            using IDbConnection cnn = new SqlConnection(DBConnection);
            int affectedRows = await cnn.ExecuteAsync(SQLUpdate);
            ////Log.Information("Command Executed: " + SQLUpdate);
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
            using IDbConnection cnn = new SqlConnection(DBConnection);
            string SQLDelete = $"DELETE FROM {Table} WHERE Id = {id}";
            int affectedRows = await cnn.ExecuteAsync(SQLDelete);
            //Log.Information("Command Executed: " + SQLDelete);
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
            using IDbConnection cnn = new SqlConnection(DBConnection);
            IEnumerable<dynamic> result = await cnn.QueryAsync(SQL);
            return result.ToList().Count > 0;
        }
        catch
        {
            return false;
        }
    }

    public string? GetCode(string Prefix, string TableName, string? Field = "Code", int CodeLength = 6, string? Connection = "Default")
    {
        /*
            GetCode("INV", "Orders") → Looks for codes like "INV000001", "INV000002"
            GetCode("", "Orders") → Looks for numeric codes like "000001", "000002"
            GetCode("CUST", "Customers", "CustomerCode", 8) → Uses "CUST" prefix with 8 - digit sequence
        */

        DBConnection = Connection == "Default" ? cfg.DefaultDB()! : cfg.GlobalDB()!;

        string SQL;
        string format = new('0', CodeLength);

        if (!string.IsNullOrEmpty(Prefix))
        {
            // With prefix: extract the numeric part after the prefix
            SQL = $@"Select MAX(CAST(SUBSTRING({Field}, {Prefix.Length + 1}, LEN({Field}) - {Prefix.Length}) as INT)) as SEQNO 
                         from {TableName} 
                         Where LEFT({Field}, {Prefix.Length}) = '{Prefix}'";
        }
        else
        {
            // Without prefix: use the entire field as numeric
            SQL = $@"Select MAX(CAST({Field} as INT)) as SEQNO from {TableName} Where ISNUMERIC({Field}) = 1";
        }

        try
        {
            using IDbConnection cnn = new SqlConnection(DBConnection);
            var result = cnn.QueryFirstOrDefault<long?>(SQL, new DynamicParameters());

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