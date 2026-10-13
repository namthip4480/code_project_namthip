using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using System.Text.Json;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

// ต่อฐานข้อมูล SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=warp_app.db"));

var app = builder.Build();

// สร้าง DB และ User Admin อัตโนมัติ
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Users.Any())
    {
        db.Users.Add(new User { Username = "Admin", Password = "1234" });
        db.SaveChanges();
    }
}

// ตั้งค่า Content-Type
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".mp4"] = "video/mp4";
provider.Mappings[".json"] = "application/json";

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

// ==========================================
// API ENDPOINTS
// ==========================================

// 1. API ดึงข้อมูลไทม์ไลน์
app.MapGet("/api/timeline/{fileName}", (string fileName) =>
{
    var cleanName = fileName.Replace(".json", "", StringComparison.OrdinalIgnoreCase);

    var possiblePaths = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), "SampleData", "Output_JSON", $"{cleanName}.json"),
        Path.Combine(Directory.GetCurrentDirectory(), "SampleData", $"{cleanName}.json"),
        Path.Combine(Directory.GetCurrentDirectory(), "..", "SampleData", "Output_JSON", $"{cleanName}.json")
    };

    string? targetPath = possiblePaths.FirstOrDefault(File.Exists);
    if (targetPath == null) return Results.NotFound(new { error = $"ไม่พบไฟล์ JSON ที่: {cleanName}" });

    var jsonContent = File.ReadAllText(targetPath);
    var jsonNode = JsonNode.Parse(jsonContent);

    if (jsonNode?["events"] != null && jsonNode?["metadata"] != null)
    {
        return Results.Content(jsonContent, "application/json");
    }

    // แปลงโครงสร้างพื้นฐาน
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
                double dur = item["durationSec"]?.GetValue<double>() ?? item["duration"]?.GetValue<double>() ?? (end - start);
                string name = item["name"]?.GetValue<string>() ?? item["event_name"]?.GetValue<string>() ?? $"Event_{eventId}";
                
                string type = item["type"]?.GetValue<string>() ?? (name.ToLower().Contains("transition") || name.ToLower().Contains("fade") ? "transition" : "clip");
                string action = item["action"]?.GetValue<string>() ?? (type == "transition" || name.ToLower().Contains("fade") ? "TRIGGER_TRANSITION" : "PLAY_MEDIA");

                if (end > maxDuration) maxDuration = end;

                eventsList.Add(new
                {
                    id = eventId++,
                    type = type,
                    name = name,
                    startSec = start,
                    endSec = end,
                    durationSec = dur,
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
});

// 2. API บันทึกและส่งออกแพ็กเกจบทเรียนฉบับสมบูรณ์ (Save & Export Package)
app.MapPost("/api/save-lesson", async (SaveLessonRequest request, AppDbContext db) =>
{
    try
    {
        var outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "SampleData", "Output_JSON");
        if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

        var savePath = Path.Combine(outputFolder, $"{request.FileName}.json");
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        
        string updatedJson = JsonSerializer.Serialize(request.LessonData, jsonOptions);
        await File.WriteAllTextAsync(savePath, updatedJson);

        // บันทึกลงฐานข้อมูล SQLite
        db.LessonPackages.Add(new LessonPackage
        {
            PackageName = request.FileName,
            JsonPath = savePath,
            CreatedDate = DateTime.Now
        });
        await db.SaveChangesAsync();

        Console.WriteLine($"[EXPORT SUCCESS] บันทึกแพ็กเกจบทเรียนสมบูรณ์ที่: {savePath}");
        return Results.Ok(new { status = "Package Exported Successfully", path = savePath });
    }
    catch (Exception ex)
    {
        return Results.Problem($"เกิดข้อผิดพลาดในการส่งออก: {ex.Message}");
    }
});

app.Run();

// ==========================================
// MODELS
// ==========================================
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SaveLessonRequest
{
    public string FileName { get; set; } = string.Empty;
    public object LessonData { get; set; } = new();
}

public class LessonPackage
{
    public int Id { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string JsonPath { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
    public DbSet<LessonPackage> LessonPackages => Set<LessonPackage>();
}