using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDI275AttachmentParser.Data.Entities
{
    internal class EdiDBCore
    {
    }

    [Table("Edi275Documents")]
    public class Edi275DocumentEntity
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string? InterchangeControlNumber { get; set; }

        [MaxLength(50)]
        public string? GroupControlNumber { get; set; }

        [MaxLength(50)]
        public string? TransactionSetType { get; set; }

        [MaxLength(50)]
        public string? TransactionSetControlNumber { get; set; }

        [MaxLength(100)]
        public string? ImplementationVersion { get; set; }

        [MaxLength(50)]
        public string? SenderId { get; set; }

        [MaxLength(50)]
        public string? ReceiverId { get; set; }

        public DateTime? TransactionDate { get; set; }

        [MaxLength(10)]
        public string? PurposeCode { get; set; }

        [MaxLength(100)]
        public string? ReferenceId { get; set; }

        public bool? IsSolicited { get; set; }
        public bool? IsUnsolicited { get; set; }

        [MaxLength(50)]
        public string? TraceTypeCode { get; set; }

        [MaxLength(100)]
        public string? AttachmentControlNumber { get; set; }

        [MaxLength(100)]
        public string? ClaimNumber { get; set; }

        [MaxLength(100)]
        public string? PatientControlNumber { get; set; }

        [MaxLength(100)]
        public string? MedicalRecordNumber { get; set; }

        [MaxLength(100)]
        public string? PayerClaimControlNumber { get; set; }

        public DateTime? ClaimServiceDate { get; set; }
        public DateTime? ClaimServiceEndDate { get; set; }

        [MaxLength(25)]
        public string? ReportTypeCode { get; set; }

        [MaxLength(25)]
        public string? AttachmentFormatCode { get; set; }

        [MaxLength(25)]
        public string? SecurityLevelCode { get; set; }

        [MaxLength(255)]
        public string? FileName { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public virtual ICollection<PatientReportEntity> PatientReports { get; set; } = new List<PatientReportEntity>();
        public virtual ICollection<AttachmentEntity> Attachments { get; set; } = new List<AttachmentEntity>();
    }

    [Table("PatientReports")]
    public class PatientReportEntity
    {
        [Key]
        public int Id { get; set; }

        public int Edi275DocumentId { get; set; }

        [ForeignKey(nameof(Edi275DocumentId))]
        public Edi275DocumentEntity? Document { get; set; }

        [MaxLength(10)]
        public string? EntityIdentifierCode { get; set; }

        [MaxLength(10)]
        public string? EntityTypeQualifier { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? MiddleName { get; set; }

        [MaxLength(25)]
        public string? NamePrefix { get; set; }

        [MaxLength(25)]
        public string? NameSuffix { get; set; }

        [MaxLength(10)]
        public string? IdentificationCodeQualifier { get; set; }

        [MaxLength(100)]
        public string? IdentificationCode { get; set; }

        [MaxLength(250)]
        public string? PatientName { get; set; }

        [MaxLength(100)]
        public string? PatientId { get; set; }
    }

    [Table("Attachments")]
    public class AttachmentEntity
    {
        [Key]
        public int Id { get; set; }

        public int Edi275DocumentId { get; set; }

        [ForeignKey(nameof(Edi275DocumentId))]
        public Edi275DocumentEntity? Document { get; set; }

        [MaxLength(25)]
        public string? AttachmentTypeCode { get; set; }

        [MaxLength(25)]
        public string? TransmissionCode { get; set; }

        [MaxLength(100)]
        public string? AttachmentControlNumber { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(25)]
        public string? AttachmentFormatCode { get; set; }

        [MaxLength(25)]
        public string? SecurityLevelCode { get; set; }

        [MaxLength(255)]
        public string? FileName { get; set; }

        public long? FileSize { get; set; }

        public byte[]? BinaryData { get; set; }

        public string? Base64Data { get; set; }

        public string? TextData { get; set; }

        public DateTime? AttachmentDate { get; set; }
    }
}