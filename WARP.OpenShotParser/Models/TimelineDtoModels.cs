using System.Text.Json.Serialization;

namespace WARP.OpenShotParser.Models;

/// <summary>
/// Streamlined timeline DTO format suitable for PC video players and microcontrollers (ESP32).
/// </summary>
public class TimelineOutput
{
    [JsonPropertyName("metadata")]
    public TimelineMetadataDto Metadata { get; set; } = new();

    [JsonPropertyName("events")]
    public List<TimelineEventDto> Events { get; set; } = new();
}

public class TimelineMetadataDto
{
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("fps")]
    public double Fps { get; set; } = 30.0;

    [JsonPropertyName("totalDurationSec")]
    public double TotalDurationSec { get; set; }

    [JsonPropertyName("totalEvents")]
    public int TotalEvents { get; set; }

    [JsonPropertyName("generatedAt")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class TimelineEventDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Event type: "clip" or "transition"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("layer")]
    public int Layer { get; set; }

    [JsonPropertyName("startSec")]
    public double StartSec { get; set; }

    [JsonPropertyName("endSec")]
    public double EndSec { get; set; }

    [JsonPropertyName("durationSec")]
    public double DurationSec { get; set; }

    [JsonPropertyName("startMs")]
    public long StartMs { get; set; }

    [JsonPropertyName("endMs")]
    public long EndMs { get; set; }

    [JsonPropertyName("durationMs")]
    public long DurationMs { get; set; }

    /// <summary>
    /// Action command e.g. "PLAY_MEDIA", "TRIGGER_TRANSITION"
    /// </summary>
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public Dictionary<string, string> Payload { get; set; } = new();
}
    public class StudentLogModel
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("studentName")]
        public string StudentName { get; set; } = string.Empty;

        [JsonPropertyName("userRole")]
        public string UserRole { get; set; } = "student";

        [JsonPropertyName("lessonName")]
        public string LessonName { get; set; } = string.Empty;

        [JsonPropertyName("eventId")]
        public int EventId { get; set; }

        [JsonPropertyName("eventName")]
        public string EventName { get; set; } = string.Empty;

        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("selectedOption")]
        public string SelectedOption { get; set; } = string.Empty;

        [JsonPropertyName("optionText")]
        public string OptionText { get; set; } = string.Empty;

        [JsonPropertyName("correctOption")]
        public string CorrectOption { get; set; } = string.Empty;

        [JsonPropertyName("isCorrect")]
        public bool? IsCorrect { get; set; }

        [JsonPropertyName("esp32Signal")]
        public string Esp32Signal { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }

    public class Esp32PayloadRequest
    {
        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; } = string.Empty;

        [JsonPropertyName("signalCode")]
        public string SignalCode { get; set; } = string.Empty;

        [JsonPropertyName("studentName")]
        public string StudentName { get; set; } = string.Empty;
    }