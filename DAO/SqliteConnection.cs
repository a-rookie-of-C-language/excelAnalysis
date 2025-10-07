using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace excel.DAO;


public class Connection{
    private static readonly string ProjectRoot = Path.GetFullPath(Path
        .Combine(AppContext.BaseDirectory, "..", "..", ".."));
    private static readonly string DbPath = Path.Combine(ProjectRoot, "" + "sqlite", "sqlite.db");
    private static readonly string connectionString = new SqliteConnectionStringBuilder { DataSource = DbPath }
        .ToString();
    private static SqliteConnection sqliteConnection = new SqliteConnection(connectionString);

    public static SqliteConnection GetConnection() {
        var dir = Path.GetDirectoryName(DbPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return sqliteConnection;
    }

    // 为多线程批量操作提供新的连接实例，确保线程安全
    public static SqliteConnection CreateConnection() {
        var dir = Path.GetDirectoryName(DbPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return new SqliteConnection(connectionString);
    }
}