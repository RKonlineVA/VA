using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EDI275AttachmentParser.Data;
using EDI275AttachmentParser.Models;
using Microsoft.Extensions.Logging;

namespace EDI275AttachmentParser.Services
{
    public class Edi275PersistenceService
    {
        private readonly Edi275DbContext _dbContext;
        private readonly ILogger<Edi275PersistenceService> _logger;

        public Edi275PersistenceService(Edi275DbContext dbContext, ILogger<Edi275PersistenceService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public int SaveDocument(string filePath, EDI275Document document)
        {
            try
            {
                _logger.LogInformation("Starting to save EDI 275 document from file: {FilePath}", filePath);

                var entity = new Edi275ImportEntity
                {
                    ImportedAtUtc = DateTime.UtcNow,
                    SourceFilePath = filePath,
                    InterchangeControlNumber = document.InterchangeControlNumber,
                    GroupControlNumber = document.GroupControlNumber,
                    TransactionSetControlNumber = document.TransactionSetControlNumber,
                    TransactionSetType = document.TransactionSetType,
                    ImplementationVersion = document.ImplementationVersion,
                    SenderId = document.SenderId,
                    ReceiverId = document.ReceiverId,
                    TransactionDate = document.TransactionDate,
                    ClaimNumber = document.ClaimNumber,
                    PatientControlNumber = document.PatientControlNumber,
                    MedicalRecordNumber = document.MedicalRecordNumber,
                    PayerClaimControlNumber = document.PayerClaimControlNumber,
                    PurposeCode = document.PurposeCode,
                    ReferenceId = document.ReferenceId,
                    TraceTypeCode = document.TraceTypeCode,
                    AttachmentControlNumber = document.AttachmentControlNumber,
                    ReportTypeCode = document.ReportTypeCode,
                    AttachmentFormatCode = document.AttachmentFormatCode,
                    SecurityLevelCode = document.SecurityLevelCode,
                    FileName = document.FileName,
                    ClaimServiceDate = document.ClaimServiceDate,
                    ClaimServiceEndDate = document.ClaimServiceEndDate
                };

                if (document.PatientReports != null && document.PatientReports.Count > 0)
                {
                    _logger.LogInformation("Adding {Count} patient reports", document.PatientReports.Count);
                    foreach (var patient in document.PatientReports)
                    {
                        entity.PatientReports.Add(new Edi275PatientReportEntity
                        {
                            EntityIdentifierCode = patient.EntityIdentifierCode,
                            EntityTypeQualifier = patient.EntityTypeQualifier,
                            PatientId = patient.PatientId,
                            PatientName = patient.PatientName,
                            LastName = patient.LastName,
                            FirstName = patient.FirstName,
                            MiddleName = patient.MiddleName,
                            NamePrefix = patient.NamePrefix,
                            NameSuffix = patient.NameSuffix,
                            IdentificationCodeQualifier = patient.IdentificationCodeQualifier,
                            IdentificationCode = patient.IdentificationCode
                        });
                    }
                }

                if (document.Attachments != null && document.Attachments.Count > 0)
                {
                    _logger.LogInformation("Adding {Count} attachments", document.Attachments.Count);
                    foreach (var attachment in document.Attachments)
                    {
                        entity.Attachments.Add(new Edi275AttachmentEntity
                        {
                            AttachmentDate = attachment.AttachmentDate,
                            AttachmentTypeCode = attachment.AttachmentTypeCode,
                            TransmissionCode = attachment.TransmissionCode,
                            AttachmentControlNumber = attachment.AttachmentControlNumber,
                            Description = attachment.Description,
                            FileSize = attachment.FileSize,
                            FileName = attachment.FileName,
                            AttachmentFormatCode = attachment.AttachmentFormatCode,
                            SecurityLevelCode = attachment.SecurityLevelCode,
                            Base64Data = attachment.Base64Data,
                            TextData = attachment.TextData,
                            BinaryData = attachment.BinaryData
                        });
                    }
                }

                _dbContext.Edi275Imports.Add(entity);
                _logger.LogInformation("Entity added to context. Calling SaveChanges()...");

                _dbContext.SaveChanges();

                _logger.LogInformation("✓ Document saved successfully with ID: {ImportId}", entity.Id);
                _logger.LogInformation("SNIP validation messages count: {Count}", document?.SnipValidationMessages?.Count ?? 0);

                // Persist SNIP validation messages as parse logs (if any)
                if (document?.SnipValidationMessages != null && document.SnipValidationMessages.Count > 0)
                {
                    foreach (var msg in document.SnipValidationMessages)
                    {
                        try
                        {
                            // SaveParseLog creates a Edi275ParseLogEntity row and saves
                            SaveParseLog(entity.Id, "Warning", msg);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to save SNIP validation parse log for import {ImportId}", entity.Id);
                        }
                    }
                }

                return entity.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "✗ Error saving EDI 275 document");

                // Log inner exception if it exists
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception details");
                }

                throw;
            }
        }

        public void SaveParseLog(int importId, string level, string message, Exception? exception = null)
        {
            try
            {
                var row = new Edi275ParseLogEntity
                {
                    Edi275ImportEntityId = importId,
                    LoggedAtUtc = DateTime.UtcNow,
                    Level = level,
                    Message = message,
                    Exception = exception?.ToString()
                };

                _dbContext.Edi275ParseLogs.Add(row);
                _dbContext.SaveChanges();

                _logger.LogDebug("Parse log saved: Level={Level}, Message={Message}", level, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving parse log");
                throw;
            }
        }
    }
}
