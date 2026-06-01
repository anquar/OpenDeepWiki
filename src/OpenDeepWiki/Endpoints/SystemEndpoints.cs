using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OpenDeepWiki.EFCore;

namespace OpenDeepWiki.Endpoints;

/// <summary>
/// 系统信息相关接口
/// </summary>
public static class SystemEndpoints
{
    public static void MapSystemEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/system")
            .WithTags("System");

        group.MapGet("/version", GetVersion)
            .WithSummary("获取系统版本信息")
            .WithDescription("返回当前系统的版本号和构建信息");

        group.MapGet("/branding", GetBranding)
            .WithSummary("获取网站品牌设置")
            .WithDescription("返回网站名称、LOGO路径和描述信息（无需认证）");
    }

    /// <summary>
    /// 获取系统版本信息
    /// </summary>
    private static IResult GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? version;

        return Results.Ok(new
        {
            success = true,
            data = new
            {
                version = informationalVersion,
                assemblyVersion = version,
                productName = "OpenDeepWiki"
            }
        });
    }

    /// <summary>
    /// 获取网站品牌设置（无需认证）
    /// </summary>
    private static async Task<IResult> GetBranding(IContext context)
    {
        var settings = await context.SystemSettings
            .Where(s => !s.IsDeleted && s.Category == "general")
            .ToListAsync();

        var dict = settings.ToDictionary(s => s.Key, s => s.Value ?? "");

        return Results.Ok(new
        {
            siteName = dict.GetValueOrDefault("SITE_NAME", "OpenDeepWiki"),
            logoUrl = dict.GetValueOrDefault("SITE_LOGO_URL", "/favicon.png"),
            siteDescription = dict.GetValueOrDefault("SITE_DESCRIPTION", "AI 驱动的代码知识库"),
        });
    }
}
