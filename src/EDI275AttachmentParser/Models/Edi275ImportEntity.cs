using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI275AttachmentParser.Models
{
    public class Edi275ImportEntity
    {
        public int Id { get; set; }
        public DateTime ImportedAtUtc { get; set; }
        public string SourceFilePath { get; set; } = string.Empty;

        public string? InterchangeControlNumber { get; set; }
        public string? GroupControlNumber { get; set; }
        public string? TransactionSetControlNumber { get; set; }
        public string? TransactionSetType { get; set; }
        public string? ImplementationVersion { get; set; }

        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public DateTime? TransactionDate { get; set; }

        public string? ClaimNumber { get; set; }
        public string? PatientControlNumber { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public string? PayerClaimControlNumber { get; set; }

        public string? PurposeCode { get; set; }
        public string? ReferenceId { get; set; }
        public string? TraceTypeCode { get; set; }
        public string? AttachmentControlNumber { get; set; }
        public string? ReportTypeCode { get; set; }
        public string? AttachmentFormatCode { get; set; }
        public string? SecurityLevelCode { get; set; }
        public string? FileName { get; set; }

        public DateTime? ClaimServiceDate { get; set; }
        public DateTime? ClaimServiceEndDate { get; set; }

        public ICollection<Edi275PatientReportEntity> PatientReports { get; set; } = new List<Edi275PatientReportEntity>();
        public ICollection<Edi275AttachmentEntity> Attachments { get; set; } = new List<Edi275AttachmentEntity>();
        public ICollection<Edi275ParseLogEntity> ParseLogs { get; set; } = new List<Edi275ParseLogEntity>();
    }
}