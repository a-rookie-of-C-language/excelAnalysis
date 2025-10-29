using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace excel.DAO;

// 简单的 SQLite 数据库帮助类
public static class SqliteDb {
    private static string GetDatabasePath() {
        var baseDir = AppContext.BaseDirectory;
        var dbDir = Path.Combine(baseDir, "sqlite");
        Directory.CreateDirectory(dbDir);
        return Path.Combine(dbDir, "sqlite.db");
    }

    public static SqliteConnection Create() {
        var dataSource = GetDatabasePath();
        return new SqliteConnection($"Data Source={dataSource}");
    }

    // 预期的 StudentInfo 表结构（列名 -> SQLite 类型）
    private static readonly Dictionary<string, string> StudentInfoSchema = new() {
        ["Id"] = "TEXT",
        ["Name"] = "TEXT",
        ["Clazz"] = "TEXT",
        ["Course"] = "TEXT",
        ["Score"] = "TEXT",
        ["Grade"] = "TEXT",
        ["Major"] = "TEXT",
        ["TeacherId"] = "TEXT"
    };

    public static void EnsureDatabase() {
        using var conn = Create();
        conn.Open();
        EnsureTable(conn);
        EnsureColumns(conn);
    }

    private static void EnsureTable(SqliteConnection conn) {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS StudentInfo (
    Id TEXT PRIMARY KEY,
    Name TEXT,
    Clazz TEXT,
    Course TEXT,
    Score TEXT,
    Grade TEXT,
    Major TEXT,
    TeacherId TEXT
);";
        cmd.ExecuteNonQuery();
    }

    // 如果旧库缺少某些列（例如 Clazz），通过 ALTER TABLE 动态补齐
    private static void EnsureColumns(SqliteConnection conn) {
        // 获取现有列
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = conn.CreateCommand()) {
            cmd.CommandText = "PRAGMA table_info(StudentInfo);";
            using var reader = cmd.ExecuteReader();
            // PRAGMA table_info 返回列：cid,name,type,notnull,dflt_value,pk
            while (reader.Read()) {
                var name = reader.GetString(1);
                existing.Add(name);
            }
        }

        // 逐列检查并补齐
        foreach (var kvp in StudentInfoSchema) {
            if (!existing.Contains(kvp.Key)) {
                using var alter = conn.CreateCommand();
                alter.CommandText = $"ALTER TABLE StudentInfo ADD COLUMN {kvp.Key} {kvp.Value};";
                alter.ExecuteNonQuery();
            }
        }
    }
}