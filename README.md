# StatiqMermaid

[![StatiqMermaid on NuGet](https://img.shields.io/nuget/v/Ociaw.StatiqMermaid)](https://www.nuget.org/packages/Ociaw.StatiqMermaid/)

Provides [Statiq](https://www.statiq.dev/) modules and a pipeline to generate
diagrams from code. Depends upon
[Mermaid CLI](https://github.com/mermaid-js/mermaid-cli).

Modules added:
- BuildMermaidDefinition - builds a mermaid definition from API pipeline results
- RenderMermaidSvg - renders an SVG from mermaid definition (.mmd) documents
- RenderMermaidSvgFromHtml - renders and embeds SVGs from HTML documents

The Mermaid Pipeline inserts itself to execute after the "API" pipeline, but
before the "Content" pipeline. It pulls documents API that have the Kind of
NamedType, then feeds them to BuildMermaidDefinition and RenderMermaidSvg,
respectively, before writing the resulting SVGs to the file system.

## Usage

Adding the pipeline:

```csharp
using Ociaw.StatiqMermaid;

await Bootstrapper.Factory
    .CreateDocs(args)
    .AddSourceFiles(sourceGlob)
    .AddPipeline("Mermaid", settings =>
    {
        var executable = settings.GetString("MermaidExecutable");
        return executable is null ? new Mermaid() : new Mermaid(executable);
    })
    .RunAsync();
```

The pipeline assumes that the Mermaid CLI executable is `mmdc` - if that is not
the case, pass in a custom executable path.
