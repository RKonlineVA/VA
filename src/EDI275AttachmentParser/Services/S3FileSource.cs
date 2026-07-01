using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace X12EDI837.Ingestion.Infrastructure.FileSource;

/// <summary>
/// Reads EDI files from an S3 bucket (or a moto / localstack mock endpoint).
/// Switching from local → S3 is a one-line change: "Provider": "s3" in appsettings.json.
/// </summary>
public sealed class S3FileSource : IFileSource
{
    private readonly IAmazonS3 _s3;
    private readonly FileSourceOptions _opts;
    private readonly ILogger<S3FileSource> _logger;

    public S3FileSource(IAmazonS3 s3, IOptions<FileSourceOptions> opts, ILogger<S3FileSource> logger)
    {
        _s3 = s3;
        _opts = opts.Value;
        _logger = logger;
    }

  
    public async Task<IEnumerable<string>> ListFilesAsync(CancellationToken ct = default)
    {
        // If a specific filename is configured, return just that key — same behaviour as LocalFileSource.
        if (!string.IsNullOrWhiteSpace(_opts.FileName))
        {
            var singleKey = $"{_opts.S3Prefix.TrimEnd('/')}/{_opts.FileName}";
            _logger.LogInformation("S3FileSource: using configured file s3://{Bucket}/{Key}",
                _opts.S3BucketName, singleKey);
            return [singleKey];
        }

        // Otherwise list ALL .edi files under the prefix.
        var request = new ListObjectsV2Request
        {
            BucketName = _opts.S3BucketName,
            Prefix = _opts.S3Prefix
        };

        var response = await _s3.ListObjectsV2Async(request, ct);

        var keys = response.S3Objects
            .Where(o => o.Key.EndsWith(".edi", StringComparison.OrdinalIgnoreCase))
            .Select(o => o.Key)
            .ToList();

        _logger.LogInformation("S3FileSource: found {Count} file(s) in s3://{Bucket}/{Prefix}",
            keys.Count, _opts.S3BucketName, _opts.S3Prefix);

        return keys;
    }

 
    public async Task<Stream> OpenReadAsync(string fileName, CancellationToken ct = default)
    {
        _logger.LogInformation("S3FileSource: downloading s3://{Bucket}/{Key}",
            _opts.S3BucketName, fileName);

        var response = await _s3.GetObjectAsync(_opts.S3BucketName, fileName, ct);

        // Copy to a MemoryStream so the caller owns the lifetime (S3 stream auto-closes).
        var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, ct);
        ms.Position = 0;
        return ms;
    }
}
