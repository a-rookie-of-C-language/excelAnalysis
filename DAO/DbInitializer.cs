using System;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace excel.DAO;

public static class DbInitializer {
    private static readonly string ProjectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
    private static readonly string InitSqlPath = Path.Combine(ProjectRoot, "init.sql");

    public static void Initialize() {
        if (!File.Exists(InitSqlPath)) return;
        using var conn = Connection.CreateConnection();
        conn.Open();
        var script = File.ReadAllText(InitSqlPath);
        var statements = script
            .Split(';')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s));
        foreach (var stmt in statements) {
            using var cmd = new SqliteCommand(stmt + ";", conn);
            cmd.ExecuteNonQuery();
        }

        // 迁移 ImportHistory，确保存在 table_name 列
        using (var checkCmd = new SqliteCommand("PRAGMA table_info(ImportHistory);", conn)) {
            using var reader = checkCmd.ExecuteReader();
            var hasTableName = false;
            while (reader.Read()) {
                var colName = reader.GetString(reader.GetOrdinal("name"));
                if (string.Equals(colName, "table_name", StringComparison.OrdinalIgnoreCase)) {
                    hasTableName = true;
                    break;
                }
            }
            reader.Close();
            if (!hasTableName) {
                using var alter = new SqliteCommand("ALTER TABLE ImportHistory ADD COLUMN table_name TEXT;", conn);
                alter.ExecuteNonQuery();
            }
        }
        conn.Close();
    }
}