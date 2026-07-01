namespace X12EDI837.Ingestion.Infrastructure.FileSource;

/// <summary>
/// Bound from the "FileSource" section in appsettings.json.
/// </summary>
public class FileSourceOptions
{
    public const string SectionName = "FileSource";

    /// <summary>"local" or "s3"</summary>
    public string Provider { get; set; } = "local";

    // --- Local ---
    public string LocalPath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;

    // --- S3 ---
    public string S3BucketName { get; set; } = string.Empty;
    public string S3Prefix { get; set; } = string.Empty;

    /// <summary>Override endpoint for moto / localstack mock testing.</summary>
    public string S3ServiceUrl { get; set; } = string.Empty;
}
