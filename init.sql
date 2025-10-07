CREATE TABLE IF NOT EXISTS StudentInfo
(
    id
        TEXT
        PRIMARY
            KEY,
    name
        TEXT
        NOT
            NULL,
    class
        TEXT,
    college
        TEXT,
    major
        TEXT,
    grade
        TEXT
);

-- 记录导入历史，用于下次启动直接展示旧数据并保留历史
CREATE TABLE IF NOT EXISTS ImportHistory
(
    id             INTEGER PRIMARY KEY AUTOINCREMENT,
    file_path      TEXT    NOT NULL,
    file_name      TEXT    NOT NULL,
    file_hash      TEXT,
    table_name     TEXT,
    imported_at    TEXT    NOT NULL,
    student_count  INTEGER NOT NULL,
    note           TEXT
);