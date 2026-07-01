using Amazon.S3;
using DotNetEnv;
using EDI275AttachmentParser.Data;
using EDI275AttachmentParser.Services;
using EDI275AttachmentParser.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.CommandLine;
using System.IO;
using System.Reflection;
using X12EDI837.Ingestion.Infrastructure.FileSource;

namespace EDI275AttachmentParser
{
    public sealed class PlainConsoleFormatter : ConsoleFormatter
    {
        public PlainConsoleFormatter() : base("plain") { }

        public override void Write<TState>(
            in LogEntry<TState> logEntry,
            IExternalScopeProvider? scopeProvider,
            TextWriter textWriter)
        {
            var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);

            if (string.IsNullOrWhiteSpace(message) && logEntry.Exception is null)
                return;

            if (!string.IsNullOrWhiteSpace(message))
                textWriter.WriteLine(message);

            if (logEntry.Exception is not null)
                textWriter.WriteLine(logEntry.Exception);
        }
    }

    public class Program
    {
        static Program()
        {
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                Console.WriteLine("✓ Loaded configuration from .env file");
            }

            var licenseKey = Environment.GetEnvironmentVariable("TRIAL_EDIFABRIC_LICENSE");

            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                Console.WriteLine("Warning: TRIAL_EDIFABRIC_LICENSE environment variable not set.");
                Console.WriteLine("EdiFabric parser will not work without a valid license.");
                Console.WriteLine("Set the environment variable or create a .env file with TRIAL_EDIFABRIC_LICENSE.");
                return;
            }

            try
            {
                var ediFabricAssembly = Assembly.Load("EdiFabric");
                var serialKeyType = ediFabricAssembly.GetType("EdiFabric.SerialKey");

                if (serialKeyType != null)
                {
                    var setMethod = serialKeyType.GetMethod(
                        "Set",
                        BindingFlags.Public | BindingFlags.Static);

                    if (setMethod != null)
                    {
                        var parameters = setMethod.GetParameters();
                        if (parameters.Length == 2 && parameters[1].ParameterType == typeof(bool))
                        {
                            setMethod.Invoke(null, new object[] { licenseKey, true });
                            Console.WriteLine("✓ EdiFabric license key set successfully");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not set EdiFabric license: {ex.Message}");
            }
        }

        public static async Task<int> Main(string[] args)
        {


            var rootCommand = new RootCommand("EDI 275 Attachment Parser - Parse EDI 275 files and extract attachments");

            var fileOption = new Option<FileInfo>(
                name: "--file",
                description: "Path to the EDI 275 file to parse")
            {
                IsRequired = true
            };
            fileOption.AddAlias("-f");

            var outputOption = new Option<DirectoryInfo>(
                name: "--output",
                description: "Directory to save extracted attachments",
                getDefaultValue: () => new DirectoryInfo("./attachments"));
            outputOption.AddAlias("-o");

            var verboseOption = new Option<bool>(
                name: "--verbose",
                description: "Enable verbose logging");
            verboseOption.AddAlias("-v");

            var jsonOption = new Option<bool>(
                name: "--json",
                description: "Export parsed data as JSON");
            jsonOption.AddAlias("-j");

            rootCommand.AddOption(fileOption);
            rootCommand.AddOption(outputOption);
            rootCommand.AddOption(verboseOption);
            rootCommand.AddOption(jsonOption);

            rootCommand.SetHandler((file, output, verbose, exportJson) =>
            {
                var services = new ServiceCollection();
                ConfigureServices(services, verbose);

                using var serviceProvider = services.BuildServiceProvider();

                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                // Apply migrations in a dedicated scope
                using (var migrationScope = serviceProvider.CreateScope())
                {
                    var db = migrationScope.ServiceProvider.GetRequiredService<Edi275DbContext>();
                    db.Database.Migrate();
                }

                try
                {
                    // Create a scope for the entire parsing and persistence operation
                    using (var operationScope = serviceProvider.CreateScope())
                    {
                        var parser = operationScope.ServiceProvider.GetRequiredService<EDI275Parser>();
                        var persistenceService = operationScope.ServiceProvider.GetRequiredService<Edi275PersistenceService>();

                        string s3Url = "True";


                        Console.WriteLine();
                        logger.LogInformation("Starting EDI 275 parsing...");
                        logger.LogInformation("Input file: {File}", file.FullName);
                        logger.LogInformation("Output directory: {Dir}", output.FullName);

                        // Parse the EDI file
                        if (s3Url.Equals("True", StringComparison.OrdinalIgnoreCase))
                        {
                            // Wire real (or moto-mocked) S3 client
                            services.AddSingleton<IAmazonS3>(sp =>
                            {
                                FileSourceOptions opts =
                                    sp.GetRequiredService<IOptions<FileSourceOptions>>().Value;

                                AmazonS3Config cfg = new AmazonS3Config
                                {
                                    ServiceURL = opts.S3ServiceUrl,
                                    ForcePathStyle = true,
                                };

                                return new AmazonS3Client("test", "test", cfg);
                            });

                            services.AddSingleton<IFileSource, S3FileSource>();
                        }

                       
                       var document = parser.ParseFile(file.FullName);
                       
                            // PERSIST TO DATABASE
                        int importId = persistenceService.SaveDocument(file.FullName, document);
                        logger.LogInformation("✓ Document saved to database with Import ID: {ImportId}", importId);

                        Console.WriteLine();
                        Console.WriteLine(new string('=', 60));
                        Console.WriteLine("EDI 275 PARSING SUMMARY");
                        Console.WriteLine(new string('=', 60));
                        Console.WriteLine($"Interchange Control Number: {document.InterchangeControlNumber}");
                        Console.WriteLine($"Sender ID: {document.SenderId}");
                        Console.WriteLine($"Receiver ID: {document.ReceiverId}");
                        Console.WriteLine($"Transaction Date: {document.TransactionDate}");
                        Console.WriteLine($"Patient Reports: {document.PatientReports.Count}");
                        Console.WriteLine($"Attachments Found: {document.Attachments.Count}");
                        Console.WriteLine($"Database Import ID: {importId}");
                        Console.WriteLine(new string('=', 60));

                        if (document.PatientReports.Count > 0)
                        {
                            Console.WriteLine();
                            Console.WriteLine("PATIENT INFORMATION:");
                            foreach (var patient in document.PatientReports)
                            {
                                Console.WriteLine($"  - {patient.PatientName} (ID: {patient.PatientId}) ");
                            }
                        }

                        if (document.Attachments.Count > 0)
                        {
                            Console.WriteLine();
                            Console.WriteLine("ATTACHMENTS:");
                            for (int i = 0; i < document.Attachments.Count; i++)
                            {

                                var att = document.Attachments[i];

                                var controlNumber = att.AttachmentControlNumber;

                                if (string.IsNullOrEmpty(controlNumber))
                                {
                                    controlNumber = document.InterchangeControlNumber;
                                }

                                Console.WriteLine($"  [{i + 1}] Control#: {controlNumber}");
                                Console.WriteLine($"                Type: {att.AttachmentTypeCode}");
                                Console.WriteLine($"         Description: {att.Description}");
                                Console.WriteLine($"                Size: {att.FileSize} bytes");
                                Console.WriteLine($"                File: {att.FileName}");
                            }

                            Console.WriteLine();
                            Console.WriteLine($"Extracting attachments to: {output.FullName}");
                            parser.ExtractAttachments(document, output.FullName);
                            Console.WriteLine("✓ Attachments extracted successfully!");
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("No attachments found in the EDI file.");
                        }

                        if (exportJson)
                        {
                            if (!output.Exists)
                                output.Create();

                            var jsonPath = Path.Combine(output.FullName, "parsed_data.json");
                            var json = JsonConvert.SerializeObject(document, Formatting.Indented);
                            File.WriteAllText(jsonPath, json);
                            Console.WriteLine();
                            Console.WriteLine($"✓ JSON export saved to: {jsonPath}");
                        }

                        Console.WriteLine(new string('=', 60));
                        logger.LogInformation("EDI 275 parsing completed successfully");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error parsing EDI 275 file");
                    Console.WriteLine();
                    Console.WriteLine($"✗ ERROR: {ex.Message}");

                    if (verbose)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Stack Trace:{Environment.NewLine}{ex.StackTrace}");
                    }

                    Environment.ExitCode = 1;
                }
            }, fileOption, outputOption, verboseOption, jsonOption);

            return await rootCommand.InvokeAsync(args);
        }

        static void ConfigureServices(IServiceCollection services, bool verbose)
        {
            var connectionString = Environment.GetEnvironmentVariable("EDI275_DB_CONNECTION")
                ?? "Server=localhost;Database=X12EDI75IngestionDb;Trusted_Connection=True;TrustServerCertificate=True;";

         
            services.AddDbContext<Edi275DbContext>(options => options.UseSqlServer(connectionString));

            services.AddLogging(configure =>
            {
                configure.ClearProviders();
                configure.AddConsole(options =>
                {
                    options.FormatterName = verbose ? "simple" : "plain";
                });
                configure.AddConsoleFormatter<PlainConsoleFormatter, SimpleConsoleFormatterOptions>();
                configure.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Critical);
            });

            services.AddTransient<EDI275Parser>();
            services.AddTransient<ManualEDI275Parser>();
            services.AddTransient<Edi275PersistenceService>();
        }
    }
}