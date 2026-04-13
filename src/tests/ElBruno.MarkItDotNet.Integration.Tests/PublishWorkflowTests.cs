using System.Text.RegularExpressions;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Integration.Tests;

/// <summary>
/// Validates that the publish workflow includes all packable projects
/// so that NuGet consumers never hit missing dependency errors (see issue #7).
/// </summary>
public class PublishWorkflowTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string PublishWorkflowPath = Path.Combine(RepoRoot, ".github", "workflows", "publish.yml");
    private static readonly string SolutionPath = Path.Combine(RepoRoot, "ElBruno.MarkItDotNet.slnx");

    [Fact]
    public void PublishWorkflow_PacksAllPackableProjects()
    {
        var packableProjects = GetPackableProjectsFromSolution();
        var packedProjects = GetPackedProjectsFromWorkflow();

        packableProjects.Should().NotBeEmpty("there should be packable projects in the solution");

        foreach (var project in packableProjects)
        {
            packedProjects.Should().Contain(project,
                $"packable project '{project}' must be included in publish.yml pack step " +
                "to avoid missing NuGet dependency errors (issue #7)");
        }
    }

    [Fact]
    public void PublishWorkflow_PacksCoreModelBeforeDependents()
    {
        var workflowContent = File.ReadAllText(PublishWorkflowPath);
        var packLines = workflowContent
            .Split('\n')
            .Where(line => line.Contains("dotnet pack") && line.Contains(".csproj"))
            .ToList();

        var coreModelIndex = packLines.FindIndex(l => l.Contains("CoreModel"));
        var mainPackageIndex = packLines.FindIndex(l =>
            l.Contains("ElBruno.MarkItDotNet/ElBruno.MarkItDotNet.csproj"));

        coreModelIndex.Should().BeGreaterThanOrEqualTo(0,
            "CoreModel must be present in pack steps");
        mainPackageIndex.Should().BeGreaterThanOrEqualTo(0,
            "main package must be present in pack steps");

        coreModelIndex.Should().BeLessThan(mainPackageIndex,
            "CoreModel must be packed before the main package that depends on it");
    }

    [Fact]
    public void PublishWorkflow_AllProjectReferenceDependenciesArePacked()
    {
        var packedProjects = GetPackedProjectsFromWorkflow();
        var srcDir = Path.Combine(RepoRoot, "src");

        foreach (var projectName in packedProjects)
        {
            var csprojPath = Path.Combine(srcDir, projectName, $"{projectName}.csproj");
            if (!File.Exists(csprojPath))
                continue;

            var doc = XDocument.Load(csprojPath);
            var projectRefs = doc.Descendants("ProjectReference")
                .Select(pr => pr.Attribute("Include")?.Value)
                .Where(v => v != null)
                .Select(v => ExtractProjectNameFromPath(v!))
                .ToList();

            foreach (var dep in projectRefs)
            {
                var depCsprojPath = Path.Combine(srcDir, dep, $"{dep}.csproj");
                if (!File.Exists(depCsprojPath))
                    continue;

                var depDoc = XDocument.Load(depCsprojPath);
                var isPackable = IsProjectPackable(depDoc);

                if (isPackable)
                {
                    packedProjects.Should().Contain(dep,
                        $"'{projectName}' has a ProjectReference to '{dep}' which is packable. " +
                        $"'{dep}' must also be in publish.yml or NuGet consumers will get missing package errors.");
                }
            }
        }
    }

    [Fact]
    public void AllPackableProjects_HaveRequiredNuGetMetadata()
    {
        var packableProjects = GetPackableProjectsFromSolution();
        var srcDir = Path.Combine(RepoRoot, "src");

        foreach (var projectName in packableProjects)
        {
            var csprojPath = Path.Combine(srcDir, projectName, $"{projectName}.csproj");
            if (!File.Exists(csprojPath))
                continue;

            var doc = XDocument.Load(csprojPath);
            var props = doc.Descendants("PropertyGroup").Elements();

            var packageId = props.FirstOrDefault(e => e.Name.LocalName == "PackageId")?.Value;
            var version = props.FirstOrDefault(e => e.Name.LocalName == "Version")?.Value;
            var description = props.FirstOrDefault(e => e.Name.LocalName == "Description")?.Value;

            packageId.Should().NotBeNullOrEmpty($"'{projectName}' must have a PackageId");
            version.Should().NotBeNullOrEmpty($"'{projectName}' must have a Version");
            description.Should().NotBeNullOrEmpty($"'{projectName}' must have a Description");
        }
    }

    private static List<string> GetPackableProjectsFromSolution()
    {
        var doc = XDocument.Load(SolutionPath);
        var allProjects = doc.Descendants("Project")
            .Select(p => p.Attribute("Path")?.Value)
            .Where(p => p != null)
            .Select(p => p!)
            .ToList();

        var srcDir = Path.Combine(RepoRoot, "src");
        var packable = new List<string>();

        foreach (var relativePath in allProjects)
        {
            var fullPath = Path.Combine(RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
                continue;

            // Skip test and sample projects
            if (relativePath.Contains("/tests/") || relativePath.Contains("/samples/"))
                continue;

            var csproj = XDocument.Load(fullPath);
            if (IsProjectPackable(csproj))
            {
                var projectName = Path.GetFileNameWithoutExtension(fullPath);
                packable.Add(projectName);
            }
        }

        return packable;
    }

    private static List<string> GetPackedProjectsFromWorkflow()
    {
        var content = File.ReadAllText(PublishWorkflowPath);
        var matches = Regex.Matches(content, @"dotnet pack src/([^/]+)/[^ ]+\.csproj");
        return matches.Select(m => m.Groups[1].Value).Distinct().ToList();
    }

    private static bool IsProjectPackable(XDocument csproj)
    {
        var props = csproj.Descendants("PropertyGroup").Elements();
        var isPackable = props.FirstOrDefault(e => e.Name.LocalName == "IsPackable")?.Value;
        if (isPackable?.Equals("false", StringComparison.OrdinalIgnoreCase) == true)
            return false;

        var isTestProject = props.FirstOrDefault(e => e.Name.LocalName == "IsTestProject")?.Value;
        if (isTestProject?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
            return false;

        var packageId = props.FirstOrDefault(e => e.Name.LocalName == "PackageId")?.Value;
        return !string.IsNullOrEmpty(packageId);
    }

    private static string ExtractProjectNameFromPath(string projectRefPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(projectRefPath);
        return fileName;
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "ElBruno.MarkItDotNet.slnx")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException(
            "Could not find repository root (looking for ElBruno.MarkItDotNet.slnx)");
    }
}
