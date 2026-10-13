using System.Text.Encodings.Web;
using System.Text.Json;
using WARP.OpenShotParser.Models;

namespace WARP.OpenShotParser.Services;

public class OpenShotTimelineParserService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Parses an OpenShot project (.osp) file and converts it into a streamlined TimelineOutput DTO.
    /// </summary>
    public TimelineOutput ParseOspFile(string ospFilePath)
    {
        if (!File.Exists(ospFilePath))
        {
            throw new FileNotFoundException($"OpenShot project file not found: {ospFilePath}");
        }

        string jsonContent = File.ReadAllText(ospFilePath);
        var rawProject = JsonSerializer.Deserialize<OpenShotProject>(jsonContent, JsonOptions)
            ?? throw new JsonException("Failed to deserialize OpenShot project file.");

        string projectName = Path.GetFileNameWithoutExtension(ospFilePath);
        double fps = rawProject.Fps?.FramesPerSecond ?? 30.0;

        var events = new List<TimelineEventDto>();

        // 1. Process Clips
        if (rawProject.Clips != null)
        {
            foreach (var clip in rawProject.Clips)
            {
                double durationSec = clip.Duration > 0 ? clip.Duration : Math.Max(0, clip.End - clip.Start);
                double startSec = Math.Round(clip.Position, 3);
                double endSec = Math.Round(startSec + durationSec, 3);

                string mediaPath = clip.Reader?.Path ?? string.Empty;
                string mediaName = !string.IsNullOrEmpty(clip.Title) 
                    ? clip.Title 
                    : (!string.IsNullOrEmpty(mediaPath) ? Path.GetFileName(mediaPath) : $"Clip_{clip.Id}");

                var clipEvent = new TimelineEventDto
                {
                    Id = clip.Id,
                    Name = mediaName,
                    Type = "clip",
                    Layer = clip.Layer,
                    StartSec = startSec,
                    EndSec = endSec,
                    DurationSec = Math.Round(durationSec, 3),
                    StartMs = (long)Math.Round(startSec * 1000),
                    EndMs = (long)Math.Round(endSec * 1000),
                    DurationMs = (long)Math.Round(durationSec * 1000),
                    Action = "PLAY_MEDIA",
                    Payload = new Dictionary<string, string>
                    {
                        { "mediaPath", mediaPath },
                        { "fileId", clip.FileId }
                    }
                };

                events.Add(clipEvent);
            }
        }

        // 2. Process Effects & Transitions
        if (rawProject.Effects != null)
        {
            foreach (var effect in rawProject.Effects)
            {
                bool isTransition = string.Equals(effect.Title, "Transition", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(effect.Type, "Mask", StringComparison.OrdinalIgnoreCase) ||
                                     (effect.Reader?.Path?.Contains("@transitions") ?? false);

                if (!isTransition) continue;

                double durationSec = Math.Max(0, effect.End - effect.Start);
                double startSec = Math.Round(effect.Position, 3);
                double endSec = Math.Round(startSec + durationSec, 3);

                string effectPath = effect.Reader?.Path ?? string.Empty;
                string effectName = !string.IsNullOrEmpty(effectPath) ? Path.GetFileName(effectPath) : "Transition";
                string cleanEffectType = Path.GetFileNameWithoutExtension(effectName);

                var transitionEvent = new TimelineEventDto
                {
                    Id = effect.Id,
                    Name = effectName,
                    Type = "transition",
                    Layer = effect.Layer,
                    StartSec = startSec,
                    EndSec = endSec,
                    DurationSec = Math.Round(durationSec, 3),
                    StartMs = (long)Math.Round(startSec * 1000),
                    EndMs = (long)Math.Round(endSec * 1000),
                    DurationMs = (long)Math.Round(durationSec * 1000),
                    Action = "TRIGGER_TRANSITION",
                    Payload = new Dictionary<string, string>
                    {
                        { "effectType", cleanEffectType },
                        { "path", effectPath }
                    }
                };

                events.Add(transitionEvent);
            }
        }

        // Sort events by StartSec ascending, then by Layer
        events = events.OrderBy(e => e.StartSec).ThenBy(e => e.Layer).ToList();

        double totalDurationSec = events.Count > 0 ? events.Max(e => e.EndSec) : 0.0;

        return new TimelineOutput
        {
            Metadata = new TimelineMetadataDto
            {
                ProjectName = projectName,
                Fps = fps,
                TotalDurationSec = Math.Round(totalDurationSec, 3),
                TotalEvents = events.Count,
                GeneratedAt = DateTime.UtcNow
            },
            Events = events
        };
    }

    /// <summary>
    /// Serializes TimelineOutput to a formatted JSON file.
    /// </summary>
    public string ExportToJsonFile(TimelineOutput output, string outputJsonPath)
    {
        string directory = Path.GetDirectoryName(outputJsonPath)!;
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string jsonString = JsonSerializer.Serialize(output, JsonOptions);
        File.WriteAllText(outputJsonPath, jsonString);
        return outputJsonPath;
    }
}