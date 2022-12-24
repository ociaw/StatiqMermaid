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
    /// Creates a new instance of <see cref="Mermaid"/> with default settings.
    /// </summary>
    public Mermaid() : this("mmdc")
    { }

    /// <summary>
    /// Creates a new <see cref="Mermaid"/> with the specified Mermaid executable.
    /// </summary>
    public Mermaid(String mermaidExecutable)
    {
        Dependencies.Add("Api");
        DependencyOf.Add("Content");

        ProcessModules = new ModuleList(
            new ConcatDocuments("Api"),
            new FilterDocuments(Config.FromDocument(doc => doc.GetString(CodeAnalysisKeys.Kind) == SymbolKind.NamedType.ToString())),   
            new CacheDocuments(
                new BuildMermaidDefinition(),
                new RenderMermaidSvg(mermaidExecutable),
                new WriteFiles()
            )
        );
    }
}
