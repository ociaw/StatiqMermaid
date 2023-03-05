using System.Runtime.InteropServices;
using Statiq.Common;

namespace Ociaw.StatiqMermaid;

/// <summary>
/// Base class for modules that render mermaid-js definitions to SVG files.
/// </summary>
public abstract class RenderMermaidModule : TplParallelModule
{
    /// <summary>
    /// Creates a new <see cref="RenderMermaidModule"/> module with the default settings.
    /// </summary>
    protected RenderMermaidModule()
    { }

    /// <summary>
    /// Creates a new <see cref="RenderMermaidModule"/> module with the specified executable path.
    /// </summary>
    /// <param name="executable">The executable path.</param>
    protected RenderMermaidModule(Config<String> executable) => Executable = executable;

    /// <summary>
    /// Creates a new <see cref="RenderMermaidModule"/> module with the specified settings.
    /// </summary>
    /// <param name="executable">The executable path.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of concurrent mermaid executions allowed.</param>
    /// <param name="mermaidTimeout">The length of time mermaid CLI can execute before being cancelled.</param>
    protected RenderMermaidModule(Config<String> executable, Config<TimeSpan> mermaidTimeout, Config<Int32> maxDegreeOfParallelism)
    {
        Executable = executable;
        Timeout = mermaidTimeout;
        MaxDegreeOfParallelism = maxDegreeOfParallelism;
    }

    /// <summary>
    /// The path to the mermaid CLI executable.
    /// </summary>
    /// <remarks>Defaults to <c>mmdc</c> or <c>mmdc.cmd</c> on Windows.</remarks>
    public Config<String> Executable { get; init; } = Config.FromSetting(
        nameof(StatiqMermaidKeys.MermaidExecutable),
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "mmdc.cmd" : "mmdc"
    );

    /// <summary>
    /// The length of time an individual mermaid CLI command can execute before being cancelled.
    /// </summary>
    /// <remarks>Defaults to 120 seconds.</remarks>
    public Config<TimeSpan> Timeout { get; init; } = Config.FromSettings(s => TimeSpan.FromSeconds(s.GetInt(nameof(StatiqMermaidKeys.MermaidTimeoutSec), 120)));

    /// <summary>
    /// Renders the provided diagram definition to an SVG using <see cref="MermaidCliExecutor"/>.
    /// </summary>
    protected async Task<String?> RenderSvgAsync(IDocument document, IExecutionContext context, Stream diagramDef)
    {
        try
        {
            var executable = await Executable.GetValueAsync(document, context);
            var timeout = await Timeout.GetValueAsync(document, context);
            var executor = new MermaidCliExecutor(executable, timeout);
            return await executor.RenderSvgAsync(diagramDef, context.CancellationToken);
        }
        catch (Exception innerEx)
        {
            context.LogError(document, $"Exception while generating SVG from mermaid definition: {innerEx.Message}");
            return null;
        }
    }
}