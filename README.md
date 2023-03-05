# StatiqMermaid

[![StatiqMermaid on NuGet](https://img.shields.io/nuget/v/Ociaw.StatiqMermaid)](https://www.nuget.org/packages/Ociaw.StatiqMermaid/)

Provides [Statiq](https://www.statiq.dev/) modules and a pipeline to generate
diagrams from code. Depends upon
[Mermaid CLI](https://github.com/mermaid-js/mermaid-cli).

Modules added:
- BuildMermaidDefinition - builds a mermaid definition from API pipeline results
- RenderMermaidSvg - renders an SVG from mermaid definition (.mmd) documents
- RenderMermaidSvgInsideHtml - renders and embeds SVGs from HTML documents
- RenderMermadModule - base class for mermaid modules, useful if building your own module
- TplParallelModule - base class for modules that can run in parallel, but need to limit the degree

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
    .AddPipeline("Mermaid", new Mermaid())
    .RunAsync();
```

[Mermaid CLI](https://github.com/mermaid-js/mermaid-cli) must be installed; StatiqMermaid has been built against Version 10.0.0.

### Settings
The following settings are used to configure the behavior of StatiqMermaid:

- MermaidExecutable: `string` - the path to the mermaid CLI executable; defaults to "mmdc" or "mmdc.cmd" on Windows.
- MermaidTimeoutSec: `int` - the number of seconds allowed for Mermaid CLI to render a single diagram before cancelling; defaults to 120
- MaxDegreeOfParallelism: `int` - the maximum number of CLI executions to run in parallel; defaults to `Environment.ProcessorCount`
