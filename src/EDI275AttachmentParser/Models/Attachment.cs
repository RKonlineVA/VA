using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI275AttachmentParser.Models
{
 
    public class Attachment
    {
        public DateTime AttachmentDate { get; set; }
        public string AttachmentTypeCode { get; set; } = string.Empty;
        public string TransmissionCode { get; set; } = string.Empty;
        public string AttachmentControlNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string AttachmentFormatCode { get; set; } = string.Empty;
        public string SecurityLevelCode { get; set; } = string.Empty;

        public long FileSize { get; set; }
        public string FileName { get; set; } = string.Empty;

        public string RawPayload { get; set; } = string.Empty;
        public string? Base64Data { get; set; }
        public byte[]? BinaryData { get; set; }
        public string? TextData { get; set; }
        public string? MimeType { get; set; }


        public string SaveToFile(string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);

            var baseName = !string.IsNullOrWhiteSpace(FileName)
                ? Path.GetFileNameWithoutExtension(FileName)
                : $"attachment_{(!string.IsNullOrWhiteSpace(AttachmentControlNumber) ? AttachmentControlNumber : Guid.NewGuid().ToString("N"))}";

            // Save binary if available
            if (BinaryData != null && BinaryData.Length > 0)
            {
                var extension = GetSuggestedExtension();
                var fullPath = Path.Combine(outputDirectory, baseName + extension);
                File.WriteAllBytes(fullPath, BinaryData);
                return fullPath;
            }

            // Save text if available
            if (!string.IsNullOrWhiteSpace(TextData))
            {
                var fullPath = Path.Combine(outputDirectory, baseName + ".txt");
                File.WriteAllText(fullPath, TextData, Encoding.UTF8);
                return fullPath;
            }

            // Save raw payload as fallback
            if (!string.IsNullOrWhiteSpace(RawPayload))
            {
                var fullPath = Path.Combine(outputDirectory, baseName + ".raw.txt");
                File.WriteAllText(fullPath, RawPayload, Encoding.UTF8);
                return fullPath;
            }

            // Last fallback: save base64 if present
            if (!string.IsNullOrWhiteSpace(Base64Data))
            {
                var fullPath = Path.Combine(outputDirectory, baseName + ".b64.txt");
                File.WriteAllText(fullPath, Base64Data, Encoding.UTF8);
                return fullPath;
            }

            throw new InvalidOperationException("Attachment has no data to save.");
        }

        private string GetSuggestedExtension()
        {
            var text = $"{Description} {AttachmentTypeCode} {AttachmentFormatCode}".ToLowerInvariant();

            if (text.Contains("pdf")) return ".pdf";
            if (text.Contains("tiff")) return ".tiff";
            if (text.Contains("tif")) return ".tif";
            if (text.Contains("jpeg")) return ".jpeg";
            if (text.Contains("jpg")) return ".jpg";
            if (text.Contains("png")) return ".png";
            if (text.Contains("gif")) return ".gif";
            if (text.Contains("xml")) return ".xml";
            if (text.Contains("docx")) return ".docx";
            if (text.Contains("doc")) return ".doc";
            if (text.Contains("rtf")) return ".rtf";
            if (text.Contains("txt")) return ".txt";

            return ".bin";
        }
    }
}