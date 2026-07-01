namespace X12EDI837.Ingestion.Infrastructure.FileSource;

/// <summary>
/// Abstraction over a file source (local disk or S3).
/// Swapping providers is a one-line change in appsettings.json — no code changes needed.
/// </summary>
public interface IFileSource
{
    /// <summary>
    /// Returns all EDI file names available in the configured source.
    /// </summary>
    Task<IEnumerable<string>> ListFilesAsync(CancellationToken ct = default);

    /// <summary>
    /// Opens a readable stream for the given file name.
    /// </summary>
    Task<Stream> OpenReadAsync(string fileName, CancellationToken ct = default);
}
