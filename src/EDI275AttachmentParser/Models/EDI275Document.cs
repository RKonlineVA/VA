//csharp src\EDI275AttachmentParser\Models\EDI275Document.Validation.cs
using System.Collections.Generic;

namespace EDI275AttachmentParser.Models
{
    public partial class EDI275Document
    {
        // Collect SNIP validation messages during parsing.
        // These will be persisted as parse-log rows by the persistence service.
       // public List<string> SnipValidationMessages { get; set; } = new();
    }
}