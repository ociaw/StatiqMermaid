namespace Ociaw.StatiqMermaid;

/// <summary>
/// Settings keys for StatiqMermaid.
/// </summary>
public static class StatiqMermaidKeys
{
    /// <inheritdoc cref="RenderMermaidModule.Executable"/>
    public static String MermaidExecutable { get; } = nameof(MermaidExecutable);

    /// <inheritdoc cref="RenderMermaidModule.Timeout"/>
    public static String MermaidTimeoutSec { get; } = nameof(MermaidTimeoutSec);

    /// <inheritdoc cref="TplParallelModule.MaxDegreeOfParallelism"/>
    public static String MaxDegreeOfParallelism { get; } = nameof(MaxDegreeOfParallelism);
}
