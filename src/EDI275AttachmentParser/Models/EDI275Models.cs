using System.Text;

namespace EDI275AttachmentParser.Models
{
    /// <summary>
    /// Represents a parsed EDI 275 document with attachments
    /// </summary>
    public partial class EDI275Document
    {
        internal string TransactionSetType = string.Empty;
        internal string ImplementationVersion = string.Empty;

        public string InterchangeControlNumber { get; set; } = string.Empty;
        public string GroupControlNumber { get; set; } = string.Empty;
        public string TransactionSetControlNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public List<PatientReport> PatientReports { get; set; } = new();
        public List<Attachment> Attachments { get; set; } = new();


        public string PurposeCode { get; set; } = "";           // BGN01
        public string ReferenceId { get; set; } = "";           // BGN02 / TRN02 depending on flow
        public DateTime BGNdate { get; set; }                   // BGN01 == "11"
        public DateTime BGNtime { get; set; }                 // BGN01 == "02"

        public string PatientControlNumber { get; set; } = "";  // REF*D9
        public string MedicalRecordNumber { get; set; } = "";   // REF*EA or payer-specific
        public DateTime? ClaimServiceDate { get; set; }         // DTP if present

        public DateTime? ClaimServiceEndDate { get; set; }

        public string TraceTypeCode { get; set; } = string.Empty;
        public string AttachmentControlNumber { get; set; } = string.Empty;

        public string ClaimNumber { get; set; } = string.Empty;
  
        public string PayerClaimControlNumber { get; set; } = string.Empty;
        public Dictionary<string, string> ReferenceValues { get; set; } = new();

        public string SecurityLevelCode { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;

        public string ReportTypeCode { get; set; } = string.Empty;
        public string AttachmentFormatCode { get; set; } = string.Empty;

        public string HierarchicalStructureCode { get; set; } = string.Empty;
        public string BhtPurposeCode { get; set; } = string.Empty;
        public string BhtReferenceId { get; set; } = string.Empty;
        public string TransactionTypeCode { get; set; } = string.Empty;
        public DateTime? BhtCreatedDateTime { get; set; }

        public string FunctionalIdentifierCode { get; set; } = string.Empty;
        public string ApplicationSenderCode { get; set; } = string.Empty;
        public string ApplicationReceiverCode { get; set; } = string.Empty;
        public string ResponsibleAgencyCode { get; set; } = string.Empty;
        public string GroupVersion { get; set; } = string.Empty;
        public DateTime? GroupCreatedDateTime { get; set; }

        // New: collect SNIP validation messages during parsing
        public List<string> SnipValidationMessages { get; set; } = new();

        public PatientReport PatientReport { get; set; } = new PatientReport();



    }
    //public class Attachment
    //{
    //    public DateTime AttachmentDate { get; set; }
    //    public string AttachmentTypeCode { get; set; } = string.Empty;      // PWK01
    //    public string TransmissionCode { get; set; } = string.Empty;        // PWK02
    //    public string AttachmentControlNumber { get; set; } = string.Empty; // PWK05/06 or TRN02
    //    public string Description { get; set; } = string.Empty;             // PWK06/07 depending on guide

    //    public string AttachmentFormatCode { get; set; } = string.Empty;    // CAT02 (HL/TX/MB)
    //    public string SecurityLevelCode { get; set; } = string.Empty;       // EFI01
    //    public long FileSize { get; set; }                        // BIN01
    //    public string FileName { get; set; } = string.Empty;
    //    public string RawPayload { get; set; } = string.Empty;              // BIN02 as-is

    //    public string? Base64Data { get; set; }
    //    public byte[]? BinaryData { get; set; }
    //    public string? TextData { get; set; }
    //    }
    /// <summary>
    /// Represents a patient report in the 275 transaction
    /// </summary>
    //public class PatientReport
    // {
    //     public string PatientId { get; set; } = string.Empty;
    //     public string PatientName { get; set; } = string.Empty;
    //     public DateTime? DateOfBirth { get; set; }
    //     public string Gender { get; set; } = string.Empty;
    //     public string ReportTypeCode { get; set; } = string.Empty;
    //     public string ReportTransmissionCode { get; set; } = string.Empty;
    //     public List<string> ReferenceCodes { get; set; } = new();
    // }

    /// <summary>
    /// Represents an attachment in the EDI 275
    /// </summary>
    /// 

    public class PatientReport
    {
        // Core patient identity
        public string EntityIdentifierCode { get; set; } = string.Empty;   // e.g. IL, QC, 74
        public string EntityTypeQualifier { get; set; } = string.Empty;    // 1 = Person, 2 = Non-Person
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string NamePrefix { get; set; } = string.Empty;
        public string NameSuffix { get; set; } = string.Empty;


        

        // Patient/member identifiers
        public string IdentificationCodeQualifier { get; set; } = string.Empty;
        public string IdentificationCode { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string PatientControlNumber { get; set; } = string.Empty;
        public string ClaimNumber { get; set; } = string.Empty;

        // Demographics
        public DateTime? DateOfBirth { get; set; }
        public string GenderCode { get; set; } = string.Empty;

        // Address
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;

        // Contact
        public string ContactName { get; set; } = string.Empty;
        public string ContactFunctionCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string FaxNumber { get; set; } = string.Empty;

        // Transaction/attachment context
        public string AttachmentControlNumber { get; set; } = string.Empty;
        public string AttachmentTypeCode { get; set; } = string.Empty;
        public string AttachmentFormatCode { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ReportTypeCode { get; set; } = string.Empty;
        public string SecurityLevelCode { get; set; } = string.Empty;

        // Dates associated with the report/claim
        public DateTime? ServiceDate { get; set; }
        public DateTime? ServiceEndDate { get; set; }
        public DateTime? SubmittedDate { get; set; }

        // Free-form or additional identifiers
        public Dictionary<string, string> ReferenceValues { get; set; } = new();

        // Optional notes/description
        public string Description { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;
 

        public override string ToString()
        {
            return $"{PatientName} ({IdentificationCode})";
        }
    }


}

