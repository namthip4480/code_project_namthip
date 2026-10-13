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