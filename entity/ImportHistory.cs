namespace excel.entity;

public class ImportHistory {
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string ImportedAt { get; set; } = string.Empty; // ISO8601 字符串
    public int StudentCount { get; set; }
    public string Note { get; set; } = string.Empty;
}