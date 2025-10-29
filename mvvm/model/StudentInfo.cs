namespace excel.mvvm.model;

public class StudentInfo {
    // 基础信息（来自 Excel 对应列）
    public string? Id { get; set; }            // 学号
    public string? Name { get; set; }          // 姓名
    public string? Clazz { get; set; }         // 班级
    public string? Course { get; set; }        // 课程名称
    public string? Score { get; set; }         // 成绩
    public string? Grade { get; set; }         // 年级
    public string? Major { get; set; }         // 专业
    public string? TeacherId { get; set; }     // 任课教师（可用作教师标识）

    // 扩展信息（非 Excel 字段，可后续计算填充）
    public int FailNumber { get; set; } // 挂科数量
    public int TotalNumber { get; set; } // 总学习课程数量
    public string[]? FailIds { get; set; }
    public string[]? TotalIds { get; set; }
}