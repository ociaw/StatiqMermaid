using Microsoft.CodeAnalysis;
using Statiq.CodeAnalysis;
using Statiq.Common;
using Statiq.Core;

namespace Ociaw.StatiqMermaid;

/// <summary>
/// Generates type-hierarchy diagrams with mermaid.
/// </summary>
public sealed class Mermaid : Pipeline
{
    /// <summary>
    /// Creates a new <see cref="Mermaid"/> pipeline with the default configuration functions.
    /// </summary>
    public Mermaid()
    {
        Dependencies.Add("Api");
        DependencyOf.Add("Content");

        ProcessModules = new ModuleList(
            new ConcatDocuments("Api"),
            new FilterDocuments(Config.FromDocument(doc => doc.GetString(CodeAnalysisKeys.Kind) == SymbolKind.NamedType.ToString())),   
            new CacheDocuments(
                new BuildMermaidDefinition(),
                new RenderMermaidSvg(),
                new WriteFiles()
            )
        );
    }

    /// <summary>
    /// Creates a new <see cref="Mermaid"/> pipeline with the specified configuration functions.
    /// </summary>
    /// <param name="mermaidExecutable">The executable path.</param>
    /// <param name="maxConcurrency">The maximum number of concurrent mermaid executions allowed.</param>
    /// <param name="mermaidTimeout">The length of time mermaid CLI can execute before being cancelled.</param>
    public Mermaid(Config<String> mermaidExecutable, Config<TimeSpan> mermaidTimeout, Config<Int32> maxConcurrency)
    {
        Dependencies.Add("Api");
        DependencyOf.Add("Content");

        ProcessModules = new ModuleList(
            new ConcatDocuments("Api"),
            new FilterDocuments(Config.FromDocument(doc => doc.GetString(CodeAnalysisKeys.Kind) == SymbolKind.NamedType.ToString())),   
            new CacheDocuments(
                new BuildMermaidDefinition(),
                new RenderMermaidSvg(mermaidExecutable, mermaidTimeout, maxConcurrency),
                new WriteFiles()
            )
        );
    }
}
