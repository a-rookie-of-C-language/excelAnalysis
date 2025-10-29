using System.IO;
using Microsoft.Extensions.Logging;

namespace excel.AI;

public class BitNetService{
    private BitNetHelper? bitNetHelper;
    private bool isModelLoaded = false;
    private string modelStatus = "";
    private readonly ILogger<BitNetHelper> logger;

    public BitNetService() {
        // 创建日志记录器
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        logger = loggerFactory.CreateLogger<BitNetHelper>();

        // 异步初始化模型
        _ = InitializeModelAsync();
    }

    private async Task InitializeModelAsync() {
        try {
            var modelPath = Path.Combine("..", "..", "..", "AI", "resources", "models",
                "qwen2.5-1.5b-instruct-q4_k_m.gguf");

            if (!File.Exists(modelPath)) {
                modelStatus = $"模型文件不存在: {Path.GetFullPath(modelPath)}";
                System.Windows.MessageBox.Show(modelStatus, "模型状态");
                return;
            }

            // 创建BitNetHelper实例
            bitNetHelper = new BitNetHelper(logger);

            // 加载模型
            bool loadSuccess = await bitNetHelper.LoadModelAsync(modelPath, contextSize: 2048);

            if (loadSuccess) {
                isModelLoaded = true;
                modelStatus = "Qwen2.5-1.5B-Instruct模型加载成功！\n使用Q4_K_M量化格式，兼容性良好。";
                System.Windows.MessageBox.Show($"BitNet服务已启动\n\n{modelStatus}", "服务状态");
            }
            else {
                modelStatus = "模型加载失败，请检查日志文件获取详细信息。";
                System.Windows.MessageBox.Show(modelStatus, "错误");
            }
        }
        catch (Exception ex) {
            modelStatus = $"初始化失败: {ex.Message}";
            System.Windows.MessageBox.Show(modelStatus, "错误");
            isModelLoaded = false;
        }
    }

    public async void chat() {
        if (!isModelLoaded || bitNetHelper == null) {
            var response = System.Windows.MessageBox.Show(
                $"模型未加载或正在加载中。\n\n{modelStatus}\n\n是否要查看模拟对话示例？",
                "BitNet聊天",
                System.Windows.MessageBoxButton.YesNo);

            if (response == System.Windows.MessageBoxResult.Yes) {
                ShowMockChat();
            }

            return;
        }

        // 实际的聊天逻辑
        await StartRealChat();
    }

    /// <summary>
    /// 聊天方法（带参数）
    /// </summary>
    /// <param name="userMessage">用户消息</param>
    /// <returns>AI回复</returns>
    public async Task<string> chat(string userMessage) {
        if (!isModelLoaded || bitNetHelper == null) {
            return $"模型未加载。状态：{modelStatus}";
        }

        try {
            // 构建对话消息
            var messages = new List<(string role, string content)> {
                ("system", "你是一个专业的教育数据分析助手，专门帮助分析学生成绩数据。请用中文回答，保持简洁专业。"),
                ("user", userMessage)
            };

            string prompt = BitNetHelper.BuildChatPrompt(messages);
            prompt += "助手: ";

            string response = await bitNetHelper.GenerateAsync(
                prompt,
                maxTokens: 200,
                temperature: 0.7f,
                antiPrompts: new List<string> { "用户:", "User:" }
            );

            return response;
        }
        catch (Exception ex) {
            return $"聊天过程中发生错误：{ex.Message}";
        }
    }

    private async Task StartRealChat() {
        try {
            // 构建对话消息
            var messages = new List<(string role, string content)> {
                ("system", "你是一个专业的教育数据分析助手，专门帮助分析学生成绩数据。请用中文回答，保持简洁专业。"),
                ("user", "你好！我是一名教师，想要了解如何使用AI来分析学生成绩数据。你能帮助我吗？")
            };

            string prompt = BitNetHelper.BuildChatPrompt(messages);
            prompt += "助手: ";

            // 显示正在生成的提示
            var loadingWindow =
                System.Windows.MessageBox.Show("AI正在思考中，请稍候...", "生成中", System.Windows.MessageBoxButton.OK);

            // 生成回复
            string aiResponse = await bitNetHelper!.GenerateAsync(
                prompt,
                maxTokens: 200,
                temperature: 0.7f,
                antiPrompts: new List<string> { "用户:", "User:", "系统:" }
            );

            // 显示AI回复
            System.Windows.MessageBox.Show($"AI助手回复：\n\n{aiResponse}", "BitNet聊天", System.Windows.MessageBoxButton.OK);
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show($"聊天过程中发生错误：{ex.Message}", "错误", System.Windows.MessageBoxButton.OK);
        }
    }

    private void ShowMockChat() {
        string mockConversation = "=== 模拟对话示例 ===\n\n" +
                                  "用户: 你好，请帮我分析一下学生成绩数据\n" +
                                  "BitNet: 您好！我可以帮您分析学生成绩数据。请上传Excel文件，我将为您提供：\n" +
                                  "• 成绩分布统计\n" +
                                  "• 学科表现分析\n" +
                                  "• 学习建议\n\n" +
                                  "用户: 这个班级的数学成绩怎么样？\n" +
                                  "BitNet: 根据数据分析，该班级数学成绩呈现以下特点：\n" +
                                  "• 平均分：78.5分\n" +
                                  "• 优秀率：25%\n" +
                                  "• 及格率：85%\n" +
                                  "建议加强基础知识训练。\n\n" +
                                  "注：这是模拟对话，实际功能需要兼容的模型文件。";

        System.Windows.MessageBox.Show(mockConversation, "模拟对话", System.Windows.MessageBoxButton.OK);
    }

    public string GetModelStatus() {
        return modelStatus;
    }

    public bool IsModelLoaded() {
        return isModelLoaded;
    }

    /// <summary>
    /// 分析学生成绩数据
    /// </summary>
    /// <param name="gradeData">成绩数据描述</param>
    /// <returns>分析结果</returns>
    public async Task<string> AnalyzeGradeDataAsync(string gradeData) {
        if (!isModelLoaded || bitNetHelper == null) {
            return "模型未加载，无法进行分析。";
        }

        try {
            var messages = new List<(string role, string content)> {
                ("system", "你是一个专业的教育数据分析师。请分析提供的学生成绩数据，给出专业的分析报告，包括成绩分布、学习建议等。"),
                ("user", $"请分析以下学生成绩数据：\n{gradeData}")
            };

            string prompt = BitNetHelper.BuildChatPrompt(messages);
            prompt += "助手: ";

            string analysis = await bitNetHelper.GenerateAsync(
                prompt,
                maxTokens: 300,
                temperature: 0.6f,
                antiPrompts: new List<string> { "用户:", "User:" }
            );

            return analysis;
        }
        catch (Exception ex) {
            return $"分析过程中发生错误：{ex.Message}";
        }
    }

    /// <summary>
    /// 生成学习建议
    /// </summary>
    /// <param name="studentInfo">学生信息</param>
    /// <returns>学习建议</returns>
    public async Task<string> GenerateStudyAdviceAsync(string studentInfo) {
        if (!isModelLoaded || bitNetHelper == null) {
            return "模型未加载，无法生成建议。";
        }

        try {
            var messages = new List<(string role, string content)> {
                ("system", "你是一个经验丰富的教育顾问。根据学生的情况，提供个性化的学习建议和改进方案。"),
                ("user", $"请为以下学生提供学习建议：\n{studentInfo}")
            };

            string prompt = BitNetHelper.BuildChatPrompt(messages);
            prompt += "助手: ";

            string advice = await bitNetHelper.GenerateAsync(
                prompt,
                maxTokens: 250,
                temperature: 0.7f,
                antiPrompts: new List<string> { "用户:", "User:" }
            );

            return advice;
        }
        catch (Exception ex) {
            return $"生成建议时发生错误：{ex.Message}";
        }
    }

    /// <summary>
    /// 获取模型信息
    /// </summary>
    /// <returns>模型信息</returns>
    public Dictionary<string, object> GetModelInfo() {
        if (bitNetHelper != null && isModelLoaded) {
            return bitNetHelper.GetModelInfo();
        }

        return new Dictionary<string, object> {
            ["ModelType"] = "未加载",
            ["Status"] = modelStatus,
            ["IsLoaded"] = false
        };
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose() {
        bitNetHelper?.Dispose();
    }
}