using EDI275AttachmentParser.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EDI275AttachmentParser
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Host building and configuration code...

            // Update to include SNIP Validator
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services, context.HostingEnvironment.IsDevelopment());
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build()
                .Run();
        }

        // --- rest of Program class unchanged ---
    }

    public class EDI275Parser
    {
        private readonly ILogger<EDI275Parser> _logger;
        private readonly ISnipValidator _snipValidator;

        public EDI275Parser(ILogger<EDI275Parser> logger, ISnipValidator snipValidator)
        {
            _logger = logger;
            _snipValidator = snipValidator;
        }

        public void ParseNameSegment(Report report)
        {
            // existing code...

            // Normalize & validate SNIP (PatientId) using ISnipValidator
            try
            {
                if (_snipValidator != null && !string.IsNullOrWhiteSpace(report.PatientId))
                {
                    var normalized = _snipValidator.Normalize(report.PatientId);
                    if (!string.IsNullOrWhiteSpace(normalized))
                    {
                        if (_snipValidator.IsValid(normalized))
                        {
                            report.PatientId = normalized;
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Invalid SNIP for patient {PatientName}. Original: {OriginalId} Normalized: {NormalizedId}",
                                report.PatientName, report.IdentificationCode, normalized);
                            report.PatientId = string.Empty;
                        }
                    }
                    else
                    {
                        report.PatientId = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating SNIP for patient {PatientName}", report.PatientName);
                // keep original behavior if validator fails
            }

            // --- rest of method unchanged ---
        }
    }

    // --- rest of file unchanged ---
}