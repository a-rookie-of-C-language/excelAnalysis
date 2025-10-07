using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;

namespace excel.DAO;

public static class StudentTableDao {
    private static readonly Regex SafeName = new Regex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    public static void EnsureTableExists(string tableName) {
        if (!SafeName.IsMatch(tableName)) throw new ArgumentException("非法表名", nameof(tableName));
        using var conn = Connection.CreateConnection();
        conn.Open();
        var sql = $@"CREATE TABLE IF NOT EXISTS {tableName}
                    (
                        id     TEXT PRIMARY KEY,
                        name   TEXT NOT NULL,
                        class  TEXT,
                        college TEXT,
                        major  TEXT,
                        grade  TEXT
                    );";
        using var cmd = new SqliteCommand(sql, conn);
        cmd.ExecuteNonQuery();
        conn.Close();
    }
}