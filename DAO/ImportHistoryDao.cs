using System;
using System.Collections.Generic;
using excel.entity;
using Microsoft.Data.Sqlite;

namespace excel.DAO;

public class ImportHistoryDao {
    public void Insert(ImportHistory history) {
        using var conn = Connection.CreateConnection();
        conn.Open();
        const string sql = @"INSERT INTO ImportHistory (file_path, file_name, file_hash, table_name, imported_at, student_count, note)
                             VALUES ($file_path, $file_name, $file_hash, $table_name, $imported_at, $student_count, $note);";
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("$file_path", history.FilePath);
        cmd.Parameters.AddWithValue("$file_name", history.FileName);
        cmd.Parameters.AddWithValue("$file_hash", string.IsNullOrEmpty(history.FileHash) ? (object)DBNull.Value : history.FileHash);
        cmd.Parameters.AddWithValue("$table_name", string.IsNullOrEmpty(history.TableName) ? (object)DBNull.Value : history.TableName);
        cmd.Parameters.AddWithValue("$imported_at", history.ImportedAt);
        cmd.Parameters.AddWithValue("$student_count", history.StudentCount);
        cmd.Parameters.AddWithValue("$note", string.IsNullOrEmpty(history.Note) ? (object)DBNull.Value : history.Note);
        cmd.ExecuteNonQuery();
        conn.Close();
    }

    public ImportHistory? GetLatest() {
        using var conn = Connection.CreateConnection();
        conn.Open();
        const string sql = @"SELECT id, file_path, file_name, file_hash, table_name, imported_at, student_count, note
                             FROM ImportHistory ORDER BY imported_at DESC LIMIT 1";
        using var cmd = new SqliteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        ImportHistory? result = null;
        if (reader.Read()) {
            result = new ImportHistory {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                FilePath = reader.GetString(reader.GetOrdinal("file_path")),
                FileName = reader.GetString(reader.GetOrdinal("file_name")),
                FileHash = reader.IsDBNull(reader.GetOrdinal("file_hash")) ? string.Empty : reader.GetString(reader.GetOrdinal("file_hash")),
                TableName = reader.IsDBNull(reader.GetOrdinal("table_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("table_name")),
                ImportedAt = reader.GetString(reader.GetOrdinal("imported_at")),
                StudentCount = reader.GetInt32(reader.GetOrdinal("student_count")),
                Note = reader.IsDBNull(reader.GetOrdinal("note")) ? string.Empty : reader.GetString(reader.GetOrdinal("note"))
            };
        }
        conn.Close();
        return result;
    }

    public List<ImportHistory> GetAll() {
        var list = new List<ImportHistory>();
        using var conn = Connection.CreateConnection();
        conn.Open();
        const string sql = @"SELECT id, file_path, file_name, file_hash, table_name, imported_at, student_count, note
                             FROM ImportHistory ORDER BY imported_at DESC";
        using var cmd = new SqliteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            list.Add(new ImportHistory {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                FilePath = reader.GetString(reader.GetOrdinal("file_path")),
                FileName = reader.GetString(reader.GetOrdinal("file_name")),
                FileHash = reader.IsDBNull(reader.GetOrdinal("file_hash")) ? string.Empty : reader.GetString(reader.GetOrdinal("file_hash")),
                TableName = reader.IsDBNull(reader.GetOrdinal("table_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("table_name")),
                ImportedAt = reader.GetString(reader.GetOrdinal("imported_at")),
                StudentCount = reader.GetInt32(reader.GetOrdinal("student_count")),
                Note = reader.IsDBNull(reader.GetOrdinal("note")) ? string.Empty : reader.GetString(reader.GetOrdinal("note"))
            });
        }
        conn.Close();
        return list;
    }
}