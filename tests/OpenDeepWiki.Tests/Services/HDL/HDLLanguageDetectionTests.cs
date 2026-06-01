using System.Reflection;
using OpenDeepWiki.Services.Repositories;
using OpenDeepWiki.Services.Wiki;
using Xunit;

namespace OpenDeepWiki.Tests.Services.HDL;

/// <summary>
/// Tests for HDL (Verilog, SystemVerilog, PSL, VHDL) language and project type detection.
/// </summary>
public class HDLLanguageDetectionTests
{
    #region Language Extension Mapping Tests

    [Theory]
    [InlineData(".v", "Verilog")]
    [InlineData(".sv", "SystemVerilog")]
    [InlineData(".svh", "SystemVerilog")]
    [InlineData(".vh", "Verilog Header")]
    [InlineData(".vhd", "VHDL")]
    [InlineData(".vhdl", "VHDL")]
    [InlineData(".psl", "PSL")]
    [InlineData(".sdc", "SDC")]
    [InlineData(".tcl", "Tcl")]
    public void GetLanguageFromExtension_ShouldReturnCorrectHDLLanguage(string extension, string expectedLanguage)
    {
        // Use reflection to access the private static method
        var method = typeof(RepositoryAnalyzer).GetMethod(
            "GetLanguageFromExtension",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var result = method!.Invoke(null, new object[] { extension }) as string;

        Assert.Equal(expectedLanguage, result);
    }

    [Fact]
    public void GetLanguageFromExtension_ShouldReturnNullForUnknownExtension()
    {
        var method = typeof(RepositoryAnalyzer).GetMethod(
            "GetLanguageFromExtension",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { ".xyz" }) as string;

        Assert.Null(result);
    }

    #endregion

    #region Project Type Detection Tests

    [Fact]
    public void DetectProjectType_ShouldReturnHdlForVerilogFiles()
    {
        var tempDir = CreateTempDirectory();
        CreateFile(tempDir, "rtl/top.v", "module top(); endmodule");
        CreateFile(tempDir, "rtl/counter.v", "module counter(); endmodule");

        var method = typeof(WikiGenerator).GetMethod(
            "DetectProjectType",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var result = method!.Invoke(null, new object[] { new DirectoryInfo(tempDir) }) as string;

        Assert.Equal("hdl", result);
    }

    [Fact]
    public void DetectProjectType_ShouldReturnHdlForSystemVerilogFiles()
    {
        var tempDir = CreateTempDirectory();
        CreateFile(tempDir, "rtl/top.sv", "module top(); endmodule");
        CreateFile(tempDir, "rtl/defs.svh", "`define CLK 100");

        var method = typeof(WikiGenerator).GetMethod(
            "DetectProjectType",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { new DirectoryInfo(tempDir) }) as string;

        Assert.Equal("hdl", result);
    }

    [Fact]
    public void DetectProjectType_ShouldReturnHdlForVHDLFiles()
    {
        var tempDir = CreateTempDirectory();
        CreateFile(tempDir, "src/top.vhd", "entity top is end entity;");

        var method = typeof(WikiGenerator).GetMethod(
            "DetectProjectType",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { new DirectoryInfo(tempDir) }) as string;

        Assert.Equal("hdl", result);
    }

    [Fact]
    public void DetectProjectType_ShouldReturnFullstackForMixedHdlAndPython()
    {
        var tempDir = CreateTempDirectory();
        CreateFile(tempDir, "rtl/top.v", "module top(); endmodule");
        CreateFile(tempDir, "requirements.txt", "numpy");

        var method = typeof(WikiGenerator).GetMethod(
            "DetectProjectType",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { new DirectoryInfo(tempDir) }) as string;

        Assert.Contains("hdl", result);
        Assert.Contains("python", result);
    }

    #endregion

    #region Entry Point Detection Tests

    [Fact]
    public void IdentifyEntryPoints_ShouldFindTopLevelHdlFiles()
    {
        var tempDir = CreateTempDirectory();
        CreateFile(tempDir, "top.v", "module top(); endmodule");
        CreateFile(tempDir, "rtl/sub.v", "module sub(); endmodule");

        var method = typeof(WikiGenerator).GetMethod(
            "IdentifyEntryPoints",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var result = method!.Invoke(null, new object[] { new DirectoryInfo(tempDir), "hdl" }) as List<string>;

        Assert.NotNull(result);
        Assert.Contains("top.v", result);
    }

    [Fact]
    public void IdentifyEntryPoints_ShouldFindTestbenchFiles()
    {
        var tempDir = CreateTempDirectory();
        CreateFile(tempDir, "tb/tb_counter.v", "module tb_counter(); endmodule");
        CreateFile(tempDir, "rtl/counter.v", "module counter(); endmodule");

        var method = typeof(WikiGenerator).GetMethod(
            "IdentifyEntryPoints",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { new DirectoryInfo(tempDir), "hdl" }) as List<string>;

        Assert.NotNull(result);
        Assert.True(result.Any(r => r.Contains("tb_counter")));
    }

    [Fact]
    public void IdentifyEntryPoints_ShouldFindTclScripts()
    {
        var tempDir = CreateTempDirectory();
        CreateFile(tempDir, "build.tcl", "# build script");
        CreateFile(tempDir, "rtl/top.sv", "module top(); endmodule");

        var method = typeof(WikiGenerator).GetMethod(
            "IdentifyEntryPoints",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { new DirectoryInfo(tempDir), "hdl" }) as List<string>;

        Assert.NotNull(result);
        Assert.Contains("build.tcl", result);
    }

    [Fact]
    public void IdentifyEntryPoints_ShouldLimitTo10Entries()
    {
        var tempDir = CreateTempDirectory();
        // Create more than 10 potential entry
        CreateFile(tempDir, "top.v", "");
        CreateFile(tempDir, "top.sv", "");
        CreateFile(tempDir, "main.v", "");
        CreateFile(tempDir, "main.sv", "");
        CreateFile(tempDir, "Makefile", "");
        CreateFile(tempDir, "CMakeLists.txt", "");
        CreateFile(tempDir, "tb/tb1.v", "");
        CreateFile(tempDir, "tb/tb2.v", "");
        CreateFile(tempDir, "tb/tb3.v", "");
        CreateFile(tempDir, "build.tcl", "");
        CreateFile(tempDir, "synth.tcl", "");
        CreateFile(tempDir, "rtl/extra.v", "");

        var method = typeof(WikiGenerator).GetMethod(
            "IdentifyEntryPoints",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = method!.Invoke(null, new object[] { new DirectoryInfo(tempDir), "hdl" }) as List<string>;

        Assert.NotNull(result);
        Assert.True(result.Count <= 10);
    }

    #endregion

    #region Helpers

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "OpenDeepWiki.HDL.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void CreateFile(string rootDir, string relativePath, string content)
    {
        var fullPath = Path.Combine(rootDir, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
    }

    #endregion
}
