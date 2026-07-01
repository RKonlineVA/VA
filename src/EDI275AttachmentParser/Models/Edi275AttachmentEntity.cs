using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI275AttachmentParser.Models
{
    public class Edi275AttachmentEntity
    {
        public int Id { get; set; }
        public int Edi275ImportEntityId { get; set; }
        public Edi275ImportEntity Edi275Import { get; set; } = null!;

        public DateTime? AttachmentDate { get; set; }
        public string? AttachmentTypeCode { get; set; }
        public string? TransmissionCode { get; set; }
        public string? AttachmentControlNumber { get; set; }
        public string? Description { get; set; }
        public long? FileSize { get; set; }
        public string? FileName { get; set; }
        public string? AttachmentFormatCode { get; set; }
        public string? SecurityLevelCode { get; set; }

        public string? Base64Data { get; set; }
        public string? TextData { get; set; }
        public byte[]? BinaryData { get; set; }
    }
}
