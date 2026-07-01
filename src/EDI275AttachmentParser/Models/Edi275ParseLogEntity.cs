using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI275AttachmentParser.Models
{
    public class Edi275ParseLogEntity
    {
        public int Id { get; set; }
        public int Edi275ImportEntityId { get; set; }
        public Edi275ImportEntity Edi275Import { get; set; } = null!;

        public DateTime LoggedAtUtc { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
    }
}
