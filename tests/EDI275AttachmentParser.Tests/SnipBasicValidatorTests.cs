// C#
using System.Linq;
using Xunit;

namespace EDI275AttachmentParser.Tests.Validators
{
    public class SnipBasicValidatorTests
    {
        private readonly SnipBasicValidator _validator = new();

        [Fact]
        public void Validate_NullDocument_ReturnsError()
        {
            var results = _validator.Validate(null!).ToList();

            Assert.Single(results);
            Assert.Equal(ValidationLevel.Error, results[0].Level);
            Assert.Contains("Document is null", results[0].Message);
        }

        [Fact]
        public void Validate_MissingInterchangeAndClaim_ReturnsErrorAndWarning()
        {
            var doc = new Edi275Document
            {
                InterchangeControlNumber = null,
                ClaimNumber = null,
                Attachments = new System.Collections.Generic.List<Attachment>(),
                PatientReports = new System.Collections.Generic.List<Edi275PatientReport>()
            };

            var results = _validator.Validate(doc).ToList();

            Assert.Contains(results, r => r.Level == ValidationLevel.Error && r.Message.Contains("InterchangeControlNumber"));
            Assert.Contains(results, r => r.Level == ValidationLevel.Warning && r.Message.Contains("ClaimNumber"));
        }

        [Fact]
        public void Validate_AttachmentMissingFilename_ReturnsWarning()
        {
            var doc = new Edi275Document
            {
                InterchangeControlNumber = "ICN-1",
                ClaimNumber = "C-1",
                Attachments = new System.Collections.Generic.List<Attachment>
                {
                    new Attachment { FileName = string.Empty, FileSize = 10, AttachmentControlNumber = "A1" }
                },
                PatientReports = new System.Collections.Generic.List<Edi275PatientReport>()
            };

            var results = _validator.Validate(doc).ToList();

            Assert.Contains(results, r => r.Level == ValidationLevel.Warning && r.Message.Contains("missing filename"));
        }
    }
}