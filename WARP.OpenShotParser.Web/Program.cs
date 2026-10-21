using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using System.Text.Json;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

// ฐานข้อมูล SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=warp_app.db"));

var app = builder.Build();

// สร้าง DB และ User Admin อัตโนมัติ
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }
    catch (Exception dbEx)
    {
        Console.WriteLine($"[DB INIT WARN] {dbEx.Message}");
    }
}

// ตั้งค่า Content-Type รองรับ MP4 และ JSON
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".mp4"] = "video/mp4";
provider.Mappings[".json"] = "application/json";

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

// ==========================================
// API ENDPOINTS
// ==========================================

// 1. API ดึงข้อมูลไทม์ไลน์บทเรียน
app.MapGet("/api/timeline/{fileName}", (string fileName) =>
{
    var cleanName = fileName.Replace(".json", "", StringComparison.OrdinalIgnoreCase);

    var possiblePaths = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), "SampleData", "Output_JSON", $"{cleanName}.json"),
        Path.Combine(Directory.GetCurrentDirectory(), "SampleData", $"{cleanName}.json"),
        Path.Combine(Directory.GetCurrentDirectory(), "..", "SampleData", "Output_JSON", $"{cleanName}.json"),
        Path.Combine(Directory.GetCurrentDirectory(), "SampleData", "Output_JSON", $"{cleanName}_timeline_output.json")
    };

    string? targetPath = possiblePaths.FirstOrDefault(File.Exists);
    if (targetPath == null) return Results.NotFound(new { error = $"ไม่พบไฟล์ JSON สำหรับ: {cleanName}" });

    try
    {
        var jsonContent = File.ReadAllText(targetPath);
        var jsonNode = JsonNode.Parse(jsonContent);

        if (jsonNode?["events"] != null && jsonNode?["metadata"] != null)
        {
            return Results.Content(jsonContent, "application/json");
        }

        var eventsList = new List<object>();
        int eventId = 1;
        double maxDuration = 0;

        Action<JsonNode?> processClips = (clipsNode) =>
        {
            if (clipsNode is JsonArray array)
            {
                foreach (var item in array)
                {
                    if (item == null) continue;
                    
                    double start = item["startSec"]?.GetValue<double>() ?? item["start"]?.GetValue<double>() ?? 0;
                    double end = item["endSec"]?.GetValue<double>() ?? item["end"]?.GetValue<double>() ?? (start + 3.0);
                    string name = item["name"]?.GetValue<string>() ?? item["event_name"]?.GetValue<string>() ?? $"Event_{eventId}";
                    string type = (name.ToLower().Contains("transition") || name.ToLower().Contains("fade")) ? "transition" : "clip";
                    string action = type == "transition" ? "TRIGGER_TRANSITION" : "PLAY_MEDIA";

                    if (end > maxDuration) maxDuration = end;

                    eventsList.Add(new
                    {
                        id = eventId++,
                        type = type,
                        name = name,
                        startSec = start,
                        endSec = end,
                        durationSec = end - start,
                        action = action,
                        question = item["question"]?.GetValue<string>() ?? null,
                        options = item["options"] ?? null
                    });
                }
            }
        };

        if (jsonNode?["events"] != null) processClips(jsonNode["events"]);
        if (jsonNode?["clips"] != null) processClips(jsonNode["clips"]);

        var formattedResponse = new
        {
            metadata = new
            {
                projectName = jsonNode?["project_name"]?.GetValue<string>() ?? cleanName,
                fps = jsonNode?["fps"]?.GetValue<int>() ?? 30,
                totalDurationSec = Math.Round(maxDuration, 2),
                totalEvents = eventsList.Count
            },
            events = eventsList
        };

        return Results.Json(formattedResponse);
    }
    catch (Exception ex)
    {
        return Results.Problem($"เกิดข้อผิดพลาดในการประมวลผลไฟล์: {ex.Message}");
    }
});

// 2. API เซฟแพ็กเกจบทเรียนลง Output_JSON (ป้องกัน Database Crash 100%)
app.MapPost("/api/save-lesson", async (HttpContext context, AppDbContext db) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var bodyText = await reader.ReadToEndAsync();
        var jsonDoc = JsonNode.Parse(bodyText);

        string fileName = jsonDoc?["fileName"]?.GetValue<string>() ?? $"Package_{DateTime.Now:yyyyMMdd_HHmmss}";
        var lessonData = jsonDoc?["lessonData"];

        if (lessonData == null) return Results.BadRequest(new { error = "ข้อมูลเป็นค่าว่าง" });

        // สร้างโฟลเดอร์สำหรับเก็บไฟล์
        var outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "SampleData", "Output_JSON");
        if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

        var cleanFileName = fileName.Replace(".json", "", StringComparison.OrdinalIgnoreCase);
        var savePath = Path.Combine(outputFolder, $"{cleanFileName}.json");

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string updatedJson = JsonSerializer.Serialize(lessonData, jsonOptions);
        await File.WriteAllTextAsync(savePath, updatedJson);

        // เซฟลง SQLite แบบเบาบาง (ถ้าล้มเหลวก็จะไม่กระทบการเซฟไฟล์)
        try
        {
            var existing = await db.LessonPackages.FirstOrDefaultAsync(p => p.PackageName == cleanFileName);
            if (existing == null)
            {
                db.LessonPackages.Add(new LessonPackage { PackageName = cleanFileName, JsonPath = savePath, CreatedDate = DateTime.Now });
            }
            else
            {
                existing.JsonPath = savePath;
                existing.CreatedDate = DateTime.Now;
            }
            await db.SaveChangesAsync();
        }
        catch (Exception dbEx)
        {
            Console.WriteLine($"[DB WARN Ignored] บันทึกไฟล์ JSON เรียบร้อยแล้ว (DB log: {dbEx.Message})");
        }

        Console.WriteLine($"[EXPORT SUCCESS] บันทึกแพ็กเกจบทเรียนเรียบร้อยที่: {savePath}");
        return Results.Ok(new { status = "Success", path = savePath, fileName = cleanFileName });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[EXPORT ERROR] {ex.Message}");
        return Results.Problem($"เกิดข้อผิดพลาด: {ex.Message}");
    }
});

// 3. API ล็อกทางเลือกนักเรียน (เตรียมส่งสัญญาณ ESP32)
app.MapPost("/api/log-choice", async (UserChoiceLog log, AppDbContext db) =>
{
    try
    {
        log.Timestamp = DateTime.Now;
        db.UserChoiceLogs.Add(log);
        await db.SaveChangesAsync();
    }
    catch { }

    Console.WriteLine($"[ESP32 DISPATCH] Student Choice: {log.Choice} | Signal Payload: {log.SignalPayload}");
    return Results.Ok(new { status = "Dispatched", payload = log.SignalPayload });
});

app.Run();

// ==========================================
// MODELS
// ==========================================
public class LessonPackage
{
    public int Id { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string JsonPath { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}

public class UserChoiceLog
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string Choice { get; set; } = string.Empty;
    public string SignalPayload { get; set; } = string.Empty;
    public string VideoFile { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<LessonPackage> LessonPackages => Set<LessonPackage>();
    public DbSet<UserChoiceLog> UserChoiceLogs => Set<UserChoiceLog>();
}