using System.Text.Json.Serialization;

namespace WARP.OpenShotParser.Models;

/// <summary>
/// Root model representing the OpenShot Project (.osp) JSON file structure.
/// </summary>
public class OpenShotProject
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("fps")]
    public OpenShotFps Fps { get; set; } = new();

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("sample_rate")]
    public int SampleRate { get; set; }

    [JsonPropertyName("channels")]
    public int Channels { get; set; }

    [JsonPropertyName("clips")]
    public List<OpenShotClip> Clips { get; set; } = new();

    [JsonPropertyName("effects")]
    public List<OpenShotEffect> Effects { get; set; } = new();

    [JsonPropertyName("files")]
    public List<OpenShotFile> Files { get; set; } = new();
}

public class OpenShotFps
{
    [JsonPropertyName("num")]
    public int Num { get; set; } = 30;

    [JsonPropertyName("den")]
    public int Den { get; set; } = 1;

    public double FramesPerSecond => Den == 0 ? 30.0 : (double)Num / Den;
}

public class OpenShotClip
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public double Position { get; set; }

    [JsonPropertyName("start")]
    public double Start { get; set; }

    [JsonPropertyName("end")]
    public double End { get; set; }

    [JsonPropertyName("duration")]
    public double Duration { get; set; }

    [JsonPropertyName("layer")]
    public int Layer { get; set; }

    [JsonPropertyName("file_id")]
    public string FileId { get; set; } = string.Empty;

    [JsonPropertyName("reader")]
    public OpenShotReader? Reader { get; set; }
}

public class OpenShotEffect
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public double Position { get; set; }

    [JsonPropertyName("start")]
    public double Start { get; set; }

    [JsonPropertyName("end")]
    public double End { get; set; }

    [JsonPropertyName("layer")]
    public int Layer { get; set; }

    [JsonPropertyName("reader")]
    public OpenShotReader? Reader { get; set; }
}

public class OpenShotFile
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}

public class OpenShotReader
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public double Duration { get; set; }

    [JsonPropertyName("fps")]
    public OpenShotFps? Fps { get; set; }
}