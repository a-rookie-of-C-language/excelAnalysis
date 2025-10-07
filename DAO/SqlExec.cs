using excel.entity;
using Microsoft.Data.Sqlite;
using System.Data;

namespace excel.DAO;

public class SqlExec{
    private SqliteConnection sqliteConnection = Connection.GetConnection();

    public int InsertStudent(Student s) {
        var sql = SplicingStudentSql(s);
        using var cmd = new SqliteCommand(sql, sqliteConnection);
        cmd.Parameters.AddWithValue("$id", s.Id);
        cmd.Parameters.AddWithValue("$name", s.Name);
        cmd.Parameters.AddWithValue("$class", s.Class);
        cmd.Parameters.AddWithValue("$college", s.College);
        cmd.Parameters.AddWithValue("$major", s.Major);
        cmd.Parameters.AddWithValue("$grade", s.Grade);
        if (sqliteConnection.State != ConnectionState.Open) sqliteConnection.Open();
        var affected = cmd.ExecuteNonQuery();
        sqliteConnection.Close();
        return affected;
    }

    private String SplicingStudentSql(Student s) {
        return @"INSERT INTO StudentInfo (id, name, class, college, major, grade)
                  VALUES ($id, $name, $class, $college, $major, $grade);";
    }

    public int UpdateStudent(Student s) {
        const string sql = @"UPDATE StudentInfo
                             SET name = $name,
                                 class = $class,
                                 college = $college,
                                 major = $major,
                                 grade = $grade
                             WHERE id = $id;";
        using var cmd = new SqliteCommand(sql, sqliteConnection);
        cmd.Parameters.AddWithValue("$id", s.Id);
        cmd.Parameters.AddWithValue("$name", s.Name);
        cmd.Parameters.AddWithValue("$class", s.Class);
        cmd.Parameters.AddWithValue("$college", s.College);
        cmd.Parameters.AddWithValue("$major", s.Major);
        cmd.Parameters.AddWithValue("$grade", s.Grade);
        if (sqliteConnection.State != ConnectionState.Open) sqliteConnection.Open();
        var affected = cmd.ExecuteNonQuery();
        sqliteConnection.Close();
        return affected;
    }

    public int DeleteStudent(string id) {
        const string sql = @"DELETE FROM StudentInfo WHERE id = $id";
        using var cmd = new SqliteCommand(sql, sqliteConnection);
        cmd.Parameters.AddWithValue("$id", id);
        if (sqliteConnection.State != ConnectionState.Open) sqliteConnection.Open();
        var affected = cmd.ExecuteNonQuery();
        sqliteConnection.Close();
        return affected;
    }

    public Student? GetStudentById(string id) {
        const string sql = @"SELECT id, name, class, college, major, grade
                             FROM StudentInfo WHERE id = $id";
        using var cmd = new SqliteCommand(sql, sqliteConnection);
        cmd.Parameters.AddWithValue("$id", id);
        if (sqliteConnection.State != ConnectionState.Open) sqliteConnection.Open();
        using var reader = cmd.ExecuteReader();
        Student? result = null;
        if (reader.Read()) {
            result = MapReaderToStudent(reader);
        }
        sqliteConnection.Close();
        return result;
    }

    public List<Student> GetAllStudents() {
        const string sql = @"SELECT id, name, class, college, major, grade FROM StudentInfo";
        using var cmd = new SqliteCommand(sql, sqliteConnection);
        if (sqliteConnection.State != ConnectionState.Open) sqliteConnection.Open();
        using var reader = cmd.ExecuteReader();
        var list = new List<Student>();
        while (reader.Read()) {
            list.Add(MapReaderToStudent(reader));
        }
        sqliteConnection.Close();
        return list;
    }

    private static Student MapReaderToStudent(SqliteDataReader reader) {
        return new Student {
            Id = reader.GetString(reader.GetOrdinal("id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Class = reader.IsDBNull(reader.GetOrdinal("class")) ? string.Empty : reader.GetString(reader.GetOrdinal("class")),
            College = reader.IsDBNull(reader.GetOrdinal("college")) ? string.Empty : reader.GetString(reader.GetOrdinal("college")),
            Major = reader.IsDBNull(reader.GetOrdinal("major")) ? string.Empty : reader.GetString(reader.GetOrdinal("major")),
            Grade = reader.IsDBNull(reader.GetOrdinal("grade")) ? string.Empty : reader.GetString(reader.GetOrdinal("grade"))
        };
    }
}

// 每线程使用独立连接/事务的执行器，避免共享连接导致的线程安全问题
    public class SqlExecPerConn {
        private readonly SqliteConnection _conn;
        private readonly SqliteTransaction? _tx;
        private readonly SqliteCommand _insertCmd;
        private readonly string _tableName;

        public SqlExecPerConn(SqliteConnection conn, SqliteTransaction? tx = null, string? tableName = null) {
            _conn = conn;
            _tx = tx;
            _tableName = string.IsNullOrWhiteSpace(tableName) ? "StudentInfo" : tableName;
            _insertCmd = new SqliteCommand($@"INSERT OR IGNORE INTO {_tableName} (id, name, class, college, major, grade)
                                         VALUES ($id, $name, $class, $college, $major, $grade);", _conn, _tx);
            _insertCmd.Parameters.Add("$id", SqliteType.Text);
            _insertCmd.Parameters.Add("$name", SqliteType.Text);
            _insertCmd.Parameters.Add("$class", SqliteType.Text);
            _insertCmd.Parameters.Add("$college", SqliteType.Text);
            _insertCmd.Parameters.Add("$major", SqliteType.Text);
            _insertCmd.Parameters.Add("$grade", SqliteType.Text);
            try { _insertCmd.Prepare(); } catch { /* ignore prepare issues */ }
        }

        public int InsertOrIgnore(Student s) {
            _insertCmd.Parameters["$id"].Value = s.Id;
            _insertCmd.Parameters["$name"].Value = s.Name;
            _insertCmd.Parameters["$class"].Value = s.Class ?? string.Empty;
            _insertCmd.Parameters["$college"].Value = s.College ?? string.Empty;
            _insertCmd.Parameters["$major"].Value = s.Major ?? string.Empty;
            _insertCmd.Parameters["$grade"].Value = s.Grade ?? string.Empty;
            return _insertCmd.ExecuteNonQuery();
        }
    }