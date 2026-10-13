using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;
using WARP.OpenShotParser.Models;
using WARP.OpenShotParser.Services;

namespace WARP.OpenShotParser.Tests;

public class OpenShotTimelineParserTests
{
    private readonly OpenShotTimelineParserService _service = new();

    private static string GetTestAssetsPath(string fileName)
    {
        string baseDir = AppContext.BaseDirectory;
        string sampleDataPath = Path.Combine(baseDir, "..", "..", "..", "..", "SampleData", fileName);

        if (!File.Exists(sampleDataPath))
        {
            sampleDataPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "SampleData", fileName);
        }

        return Path.GetFullPath(sampleDataPath);
    }

    private static string GetTestOutputDir()
    {
        string baseDir = AppContext.BaseDirectory;
        string outputDir = Path.Combine(baseDir, "..", "..", "..", "..", "SampleData", "Output_JSON");

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        return Path.GetFullPath(outputDir);
    }

    [Fact]
    public void ParseOspFile_Test01_ReturnsValidMetadataAndExportsJson()
    {
        string ospFilePath = GetTestAssetsPath("Test_01_for_App.osp");
        Assert.True(File.Exists(ospFilePath), $"Test asset missing at: {ospFilePath}");

        string outputJsonPath = Path.Combine(GetTestOutputDir(), "Test_01_timeline_output.json");

        TimelineOutput result = _service.ParseOspFile(ospFilePath);
        string exportedFile = _service.ExportToJsonFile(result, outputJsonPath);

        Assert.NotNull(result);
        Assert.NotNull(result.Metadata);
        Assert.True(File.Exists(exportedFile));
    }

    [Fact]
    public void ParseOspFile_Test02_ReturnsValidMetadataAndExportsJson()
    {
        string ospFilePath = GetTestAssetsPath("Test_02_for_App.osp");
        Assert.True(File.Exists(ospFilePath), $"Test asset missing at: {ospFilePath}");

        string outputJsonPath = Path.Combine(GetTestOutputDir(), "Test_02_timeline_output.json");

        TimelineOutput result = _service.ParseOspFile(ospFilePath);
        string exportedFile = _service.ExportToJsonFile(result, outputJsonPath);

        Assert.NotNull(result);
        Assert.NotNull(result.Metadata);
        Assert.True(File.Exists(exportedFile));
    }

    [Fact]
    public void ParseOspFile_Test03_ReturnsValidMetadataAndExportsJson()
    {
        string ospFilePath = GetTestAssetsPath("Test_03_for_App.osp");
        Assert.True(File.Exists(ospFilePath), $"Test asset missing at: {ospFilePath}");

        string outputJsonPath = Path.Combine(GetTestOutputDir(), "Test_03_timeline_output.json");

        TimelineOutput result = _service.ParseOspFile(ospFilePath);
        string exportedFile = _service.ExportToJsonFile(result, outputJsonPath);

        Assert.NotNull(result);
        Assert.NotNull(result.Metadata);
        Assert.True(File.Exists(exportedFile));
    }

    [Fact]
    public void ParseOspFile_NonExistentFile_ThrowsFileNotFoundException()
    {
        string nonExistentPath = "D:/InvalidFolder/NonExistent.osp";
        Assert.Throws<FileNotFoundException>(() => _service.ParseOspFile(nonExistentPath));
    }
}