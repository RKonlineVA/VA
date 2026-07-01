// C#
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDI275AttachmentParser
{
    public enum ValidationLevel { Info, Warning, Error }

    public record ValidationResult(ValidationLevel Level, string Message, string? Exception = null);

    public interface IValidator<T>
    {
        IEnumerable<ValidationResult> Validate(T target);
    }

    public class SnipBasicValidator : IValidator<Edi275Document>
    {
        public IEnumerable<ValidationResult> Validate(Edi275Document doc)
        {
            if (doc == null)
            {
                yield return new ValidationResult(ValidationLevel.Error, "Document is null");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(doc.InterchangeControlNumber))
                yield return new ValidationResult(ValidationLevel.Error, "Missing InterchangeControlNumber");

            if (string.IsNullOrWhiteSpace(doc.ClaimNumber))
                yield return new ValidationResult(ValidationLevel.Warning, "ClaimNumber is empty");

            if (doc.Attachments == null || doc.Attachments.Count == 0)
                yield return new ValidationResult(ValidationLevel.Info, "No attachments found");

            if (doc.Attachments != null)
            {
                foreach (var att in doc.Attachments)
                {
                    if (string.IsNullOrWhiteSpace(att.FileName))
                        yield return new ValidationResult(ValidationLevel.Warning, $"Attachment missing filename (Control#: {att.AttachmentControlNumber ?? "n/a"})");

                    if (att.FileSize < 0)
                        yield return new ValidationResult(ValidationLevel.Error, $"Attachment has invalid size: {att.FileSize}");
                }
            }

            foreach (var patient in doc.PatientReports ?? Enumerable.Empty<Edi275PatientReport>())
            {
                if (string.IsNullOrWhiteSpace(patient.PatientId))
                    yield return new ValidationResult(ValidationLevel.Warning, $"Patient missing ID: {patient.PatientName}");
            }
        }
    }
}