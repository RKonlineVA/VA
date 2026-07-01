using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI275AttachmentParser.Models
{
    public class Edi275PatientReportEntity
    {
        public int Id { get; set; }
        public int Edi275ImportEntityId { get; set; }
        public Edi275ImportEntity Edi275Import { get; set; } = null!;

        public string? EntityIdentifierCode { get; set; }
        public string? EntityTypeQualifier { get; set; }
        public string? PatientId { get; set; }
        public string? PatientName { get; set; }

        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? NamePrefix { get; set; }
        public string? NameSuffix { get; set; }

        public string? IdentificationCodeQualifier { get; set; }
        public string? IdentificationCode { get; set; }
    }
}
