using System.Diagnostics;
using Statiq.Common;

namespace  Ociaw.StatiqMermaid;

/// <summary>
/// Executes the Mermaid CLI to render SVGs from definitions.
/// </summary>
public sealed class MermaidCliExecutor : IDisposable
{
    // TODO: Make this configurable
    private readonly SemaphoreSlim _semaphore = new(16);

    /// <summary>
    /// Sets the file path to a custom mermaid CLI executable. If not set the embedded version will be used.
    /// </summary>
    /// <param name="executable">The file path.</param>
    /// <returns>The current instance.</returns>
    public MermaidCliExecutor(String executable) => Executable = executable;

    /// <summary>
    /// The length of time to wait for mmdc to execute. Defaults to 60 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// The path to the mermaid CLI executable.
    /// </summary>
    public String Executable { get; set; }

    /// <summary>
    /// Generates
    /// </summary>
    public async Task<String> RenderSvgAsync(Stream inputStream, CancellationToken token)
    {
        var timer = new CancellationTokenSource(Timeout);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(token, timer.Token);
        String? outputFile = null;
        await _semaphore.WaitAsync(cts.Token);
        try
        {
            var outputTempFile = Path.GetTempFileName();
            outputFile = Path.ChangeExtension(outputTempFile, "svg");
            File.Move(outputTempFile, outputFile);

            return await BuildSvg(inputStream, outputFile, cts.Token);
        }
        finally
        {
            _semaphore.Release();
            if (outputFile is not null)
                File.Delete(outputFile);
        }
    }

    private async Task<String> BuildSvg(Stream inputStream, String outputFile, CancellationToken token)
    {
        var args = new ProcessStartInfo(Executable)
        {
            RedirectStandardInput = true
        };
        args.ArgumentList.AddRange("--quiet", "--i", "-", "--outputFormat", "svg", "--output", outputFile);

        using (var proc = Process.Start(args))
        {
            if (proc is null)
                throw new ExecutionException("Failed to start mermaid process.");

            await inputStream.CopyToAsync(proc.StandardInput);
            proc.StandardInput.Close();

            await proc.WaitForExitAsync(token);
            if (proc.ExitCode != 0)
                throw new ExecutionException($"Mermaid returned non-zero exit code: {proc.ExitCode}");
        }

        return await File.ReadAllTextAsync(outputFile, token);
    }

    /// <inheritdoc />
    public void Dispose() => _semaphore.Dispose();
}
