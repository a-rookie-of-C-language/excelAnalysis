using System.Collections.Generic;
using System.Linq;
using excel.DAO;
using excel.mvvm.model;
using Microsoft.Data.Sqlite;

namespace excel.Services;

public static class StudentService {
    public static void Initialize() => SqliteDb.EnsureDatabase();

    public static void BulkUpsertStudents(IEnumerable<StudentInfo> students) {
        using var conn = SqliteDb.Create();
        conn.Open();
        using var tran = conn.BeginTransaction();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT OR REPLACE INTO StudentInfo (Id,Name,Clazz,Course,Score,Grade,Major,TeacherId) VALUES ($Id,$Name,$Clazz,$Course,$Score,$Grade,$Major,$TeacherId)";

        var pId = cmd.Parameters.Add("$Id", SqliteType.Text);
        var pName = cmd.Parameters.Add("$Name", SqliteType.Text);
        var pClazz = cmd.Parameters.Add("$Clazz", SqliteType.Text);
        var pCourse = cmd.Parameters.Add("$Course", SqliteType.Text);
        var pScore = cmd.Parameters.Add("$Score", SqliteType.Text);
        var pGrade = cmd.Parameters.Add("$Grade", SqliteType.Text);
        var pMajor = cmd.Parameters.Add("$Major", SqliteType.Text);
        var pTeacherId = cmd.Parameters.Add("$TeacherId", SqliteType.Text);

        foreach (var s in students) {
            pId.Value = s.Id ?? string.Empty;
            pName.Value = s.Name ?? string.Empty;
            pClazz.Value = s.Clazz ?? string.Empty;
            pCourse.Value = s.Course ?? string.Empty;
            pScore.Value = s.Score ?? string.Empty;
            pGrade.Value = s.Grade ?? string.Empty;
            pMajor.Value = s.Major ?? string.Empty;
            pTeacherId.Value = s.TeacherId ?? string.Empty;
            cmd.ExecuteNonQuery();
        }

        tran.Commit();
    }

    public static List<StudentInfo> GetStudents() {
        using var conn = SqliteDb.Create();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id,Name,Clazz,Course,Score,Grade,Major,TeacherId FROM StudentInfo";
        var list = new List<StudentInfo>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            var s = new StudentInfo {
                Id = reader.IsDBNull(0) ? null : reader.GetString(0),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                Clazz = reader.IsDBNull(2) ? null : reader.GetString(2),
                Course = reader.IsDBNull(3) ? null : reader.GetString(3),
                Score = reader.IsDBNull(4) ? null : reader.GetString(4),
                Grade = reader.IsDBNull(5) ? null : reader.GetString(5),
                Major = reader.IsDBNull(6) ? null : reader.GetString(6),
                TeacherId = reader.IsDBNull(7) ? null : reader.GetString(7)
            };
            list.Add(s);
        }
        return list;
    }
}