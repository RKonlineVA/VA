using EDI275AttachmentParser.Models;
using EdiFabric.Core.Model.Edi.X12;
using EdiFabric.Framework.Readers;
using EdiFabric.Templates.X12004010;
//using EdiFabric.Templates.Hipaa5010;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace EDI275AttachmentParser.Services
{
    /// <summary>
    /// Parser service for EDI 275 documents with attachment support
    /// </summary>
    public class EDI275Parser
    {
        private readonly ILogger<EDI275Parser> _logger;

        public EDI275Parser(ILogger<EDI275Parser> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Parse an EDI 275 file and extract attachments
        /// </summary>
        public EDI275Document ParseFile(string filePath)
        {
            _logger.LogInformation("Parsing EDI 275 file: {FilePath}", filePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"EDI file not found: {filePath}");
            }

            var document = new EDI275Document();
            var patient = new PatientReport();

           try
            {
                using (Stream stream = File.OpenRead(filePath))
            //       using (var reader = new X12Reader(stream))
              using (var reader = new X12Reader(stream, "EdiFabric.Templates.X12", new X12ReaderSettings() { ContinueOnError = true}))
              
                    
                {
                    while (reader.Read())
                    {
                        var item = reader.Item;
                        _logger.LogInformation("Item type: {Type}", item?.GetType().FullName);
                        _logger.LogInformation("Item text: {Text}", item?.ToString());

                        if (item is ISA isa)
                        {
                            ParseISA(isa, document);
                        }
                        else if (item is GS gs)
                        {
                            ParseGS(gs, document);
                        }
                        else if (item is PWK pwk)
                        {
                            ParsePaperworkSegment(pwk, document);
                        }
                        if (item is TS275 ts275)   // example name; actual type depends on your template assembly
                        {
                            var st = ts275.ST;
                            ParseST(st, document);

                           //// then parse the rest of the transaction

                            Parse275(ts275, document);
                           // ParseTransactionPatientReport(item, patient);
                        }
                        else
                        {
                            // Parse transaction-specific segments
                            ParseTransactionSegments(item, document);
                            
                        }
                    }
                }
            }
            catch (Exception ex) when (ex.Message.Contains("token was not set"))
            {
                _logger.LogError("EdiFabric license not set. Please provide a valid license key.");
                _logger.LogError("Get trial license at: https://www.edifabric.com/trial.html");
                throw new InvalidOperationException("EdiFabric license required. Use --license option or set EDIFABRIC_LICENSE environment variable.", ex);
            }

            _logger.LogInformation("Parsed {AttachmentCount} attachments from EDI 275", document.Attachments.Count);
            return document;
        }

        private void ParseISA(ISA isa, EDI275Document document)
        {
            try
            {
                // Use reflection to handle different EdiFabric versions
                var interchangeControl = isa.GetType().GetProperty("InterchangeControlNumber_13")?.GetValue(isa)?.ToString()
                    ?? isa.GetType().GetProperty("ISA13")?.GetValue(isa)?.ToString()
                    ?? string.Empty;

                var senderId = isa.GetType().GetProperty("InterchangeSenderID_6")?.GetValue(isa)?.ToString()
                    ?? isa.GetType().GetProperty("ISA06")?.GetValue(isa)?.ToString()
                    ?? string.Empty;

                var receiverId = isa.GetType().GetProperty("InterchangeReceiverID_8")?.GetValue(isa)?.ToString()
                    ?? isa.GetType().GetProperty("ISA08")?.GetValue(isa)?.ToString()
                    ?? string.Empty;

                document.InterchangeControlNumber = interchangeControl;
                document.SenderId = senderId;
                document.ReceiverId = receiverId;

                _logger.LogDebug("ISA - Control: {Control}, Sender: {Sender}, Receiver: {Receiver}",
                    document.InterchangeControlNumber, document.SenderId, document.ReceiverId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing ISA segment");
            };
        }

        private void ParseGS(GS gs, EDI275Document document)
        {
            try
            {
                var groupControl = gs.GetType().GetProperty("GroupControlNumber_6")?.GetValue(gs)?.ToString()
                ?? gs.GetType().GetProperty("GS06")?.GetValue(gs)?.ToString()
                ?? string.Empty;

                var dateStr = gs.GetType().GetProperty("Date_4")?.GetValue(gs)?.ToString()
                    ?? gs.GetType().GetProperty("GS04")?.GetValue(gs)?.ToString()
                    ?? string.Empty;

                var timeStr = gs.GetType().GetProperty("Time_5")?.GetValue(gs)?.ToString()
                    ?? gs.GetType().GetProperty("GS05")?.GetValue(gs)?.ToString()
                    ?? string.Empty;

                document.GroupControlNumber = groupControl;

                if (DateTime.TryParseExact($"{dateStr}{timeStr}",
                    "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out var date))
                {
                    document.TransactionDate = date;
                }

                _logger.LogDebug("GS - Control: {Control}, Date: {Date}",
                    document.GroupControlNumber, document.TransactionDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing GS segment");
            };

        }
        private void ParseST(ST st, EDI275Document document)
        {
            try
            {
                var stType = st.GetType();

                var transactionSetId =
                    stType.GetProperty("TransactionSetIdentifierCode_01")?.GetValue(st)?.ToString()
                    ?? stType.GetProperty("ST01")?.GetValue(st)?.ToString()
                    ?? string.Empty;

                var controlNumber =
                    stType.GetProperty("TransactionSetControlNumber_02")?.GetValue(st)?.ToString()
                    ?? stType.GetProperty("TransactionSetControlNumber")?.GetValue(st)?.ToString()
                    ?? stType.GetProperty("ST02")?.GetValue(st)?.ToString()
                    ?? string.Empty;

                var implementationVersion =
                    stType.GetProperty("ImplementationConventionReference_03")?.GetValue(st)?.ToString()
                    ?? stType.GetProperty("ImplementationConventionPreference_03")?.GetValue(st)?.ToString()
                    ?? stType.GetProperty("ST03")?.GetValue(st)?.ToString()
                    ?? string.Empty;

                document.TransactionSetType = transactionSetId;
                document.TransactionSetControlNumber = controlNumber;
                document.ImplementationVersion = implementationVersion;

                _logger.LogInformation(
                    "ST - Type: {Type}, Control: {Control}, Version: {Version}",
                    document.TransactionSetType,
                    document.TransactionSetControlNumber,
                    document.ImplementationVersion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing ST segment");

            };
        }

        private void ParseTransactionSegments(dynamic item, EDI275Document document)
        {
            try
            {
                var segmentType = item.GetType().Name;

               // Parse BIN segment (Binary Data Segment) for attachments
                if (segmentType == "BIN" || item.ToString()?.StartsWith("BIN") == true)
                {
                    ParseBinarySegment(item, document);
                }
                //Parse PWK segment(Paperwork Segment) for attachment metadata
                else if (segmentType == "PWK" || item.ToString()?.StartsWith("PWK") == true)
                {
                    ParsePaperworkSegment(item, document);
                }
                // Parse NM1 segment for patient information
                else if (segmentType == "NM1" || item.ToString()?.StartsWith("NM1") == true)
                {
                    ParseNameSegment(item, document);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing ParseTransactionSegments segment");

            };
            
        }

        private void ParseBinarySegment(dynamic segment, EDI275Document document)
        {
            try
            {
                if (segment == null)
                    return;

                var attachment = document.Attachments.LastOrDefault();

                if (attachment == null)
                {
                    attachment = new Attachment
                    {
                        AttachmentDate = DateTime.UtcNow
                    };

                    document.Attachments.Add(attachment);
                }

                string lengthText;
                string binaryText;

                if (segment is string segmentString)
                {
                    var elements = segmentString.Trim().TrimEnd('~').Split('*');

                    if (elements.Length < 3 || !elements[0].Equals("BIN", StringComparison.OrdinalIgnoreCase))
                        return;

                    lengthText = elements[1];
                    binaryText = elements[2];
                }
                else
                {
                    lengthText = GetValue(segment,
                        "LengthOfBinaryData_01",
                        "LengthOfBinaryData",
                        "BIN01");

                    binaryText = GetValue(segment,
                        "BinaryData_02",
                        "BinaryData",
                        "BIN02");
                }

                if (long.TryParse(lengthText, out var length))
                {
                    attachment.FileSize = length;
                }

                if (!string.IsNullOrWhiteSpace(binaryText))
                {
                    attachment.Base64Data = binaryText;

                    try
                    {
                        attachment.BinaryData = Convert.FromBase64String(binaryText);

                        if (string.IsNullOrWhiteSpace(attachment.FileName))
                            attachment.FileName = $"attachment_{document.Attachments.Count}.bin";
                    }
                    catch (FormatException)
                    {
                        attachment.TextData = binaryText;

                        if (string.IsNullOrWhiteSpace(attachment.FileName))
                            attachment.FileName = $"attachment_{document.Attachments.Count}.txt";
                    }
                }

                _logger.LogInformation(
                    "Parsed BIN segment - Size: {Size}, FileName: {FileName}",
                    attachment.FileSize,
                    attachment.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing BIN segment");
            }
        }

        private void ParsePaperworkSegment(dynamic segment, EDI275Document document)
        {
            try
            {
                // PWK segment contains attachment metadata
                var segmentString = segment.ToString() ?? string.Empty;
                var elements = segmentString.Split('*');

                if (elements.Length > 1)
                {
                    // Get or create the last attachment
                    Attachment attachment;
                    if (document.Attachments.Count > 0)
                    {
                        attachment = document.Attachments[^1];
                    }
                    else
                    {
                        attachment = new Attachment { AttachmentDate = DateTime.UtcNow };
                        document.Attachments.Add(attachment);
                    }

                    // PWK01 - Report Type Code
                    if (elements.Length > 1)
                    {
                        attachment.AttachmentTypeCode = elements[1];
                    }

                    // PWK02 - Report Transmission Code
                    if (elements.Length > 2)
                    {
                        attachment.TransmissionCode = elements[2];
                    }

                    // PWK05 - Identification Code (Control Number)
                    if (elements.Length > 5)
                    {
                        attachment.AttachmentControlNumber = elements[5];
                    }

                    // PWK06 - Description
                    if (elements.Length > 6)
                    {
                        attachment.Description = elements[6];
                    }

                    _logger.LogInformation("Parsed PWK segment - Type: {Type}, Description: {Desc}", 
                        attachment.AttachmentTypeCode, attachment.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing PWK segment");
            }
        }

        private static string GetPropertyValue(object target, params string[] propertyNames)
        {
         
                if (target == null) return string.Empty;

                var type = target.GetType();

                foreach (var name in propertyNames)
                {
                    var prop = type.GetProperty(name);
                    if (prop == null) continue;
                    if (prop.GetIndexParameters().Length > 0) continue;

                    var value = prop.GetValue(target);
                    if (value != null)
                        return value.ToString() ?? string.Empty;
                }

                return string.Empty;
        }


        private void ParseNameSegment(dynamic segments, EDI275Document document)
        {
            try
            {
                if (segments == null)
                    return;

                foreach (var segment in segments)
                {
                    var nm1 = segment?.NM1;
                    if (nm1 == null)
                        continue;

                    var entityCode = Convert.ToString(nm1.EntityIdentifierCode_01)?.Trim() ?? string.Empty;
                    var entityType = Convert.ToString(nm1.EntityTypeQualifier_02)?.Trim() ?? string.Empty;

                    var report = new PatientReport
                    {
                        EntityIdentifierCode = entityCode,
                        EntityTypeQualifier = entityType,
                        LastName = Convert.ToString(nm1.NameLastorOrganizationName_03)?.Trim(),
                        FirstName = Convert.ToString(nm1.NameFirst_04)?.Trim(),
                        MiddleName = Convert.ToString(nm1.NameMiddle_05)?.Trim(),
                        NamePrefix = Convert.ToString(nm1.NamePrefix_06)?.Trim(),
                        NameSuffix = Convert.ToString(nm1.NameSuffix_07)?.Trim(),
                        IdentificationCodeQualifier = Convert.ToString(nm1.IdentificationCodeQualifier_08)?.Trim(),
                        IdentificationCode = Convert.ToString(nm1.IdentificationCode_09)?.Trim()
                    };

                    if (string.Equals(entityType, "2", StringComparison.OrdinalIgnoreCase))
                    {
                        report.PatientName = report.LastName;
                    }
                    else
                    {
                        report.PatientName = string.Join(" ", new[]
                        {
                    report.FirstName,
                    report.MiddleName,
                    report.LastName
                }.Where(x => !string.IsNullOrWhiteSpace(x)));
                    }

                    if (string.Equals(report.IdentificationCodeQualifier, "MI", StringComparison.OrdinalIgnoreCase))
                    {
                        report.PatientId = report.IdentificationCode;
                    }
                    else
                    {
                        report.PatientId = report.IdentificationCode;
                    }

                    var alreadyExists = document.PatientReports.Any(x =>
                        string.Equals(x.EntityIdentifierCode, report.EntityIdentifierCode, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.EntityTypeQualifier, report.EntityTypeQualifier, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.IdentificationCode, report.IdentificationCode, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.PatientName, report.PatientName, StringComparison.OrdinalIgnoreCase));

                    if (alreadyExists)
                        continue;

                    document.PatientReports.Add(report);

                    _logger.LogInformation(
                        "Parsed NM1 - Entity: {Entity}, Type: {Type}, Name: {Name}, Qualifier: {Qualifier}, Id: {Id}",
                        report.EntityIdentifierCode,
                        report.EntityTypeQualifier,
                        report.PatientName,
                        report.IdentificationCodeQualifier,
                        report.IdentificationCode
                    );
                }

                document.PatientReport = document.PatientReports.LastOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing NM1 segment");
            }
        }

        private static string GetValue(object obj, params string[] propertyNames)
        {
            if (obj == null) return string.Empty;

            var type = obj.GetType();

            foreach (var name in propertyNames)
            {
                var prop = type.GetProperty(name);
                if (prop == null) continue;

                var value = prop.GetValue(obj)?.ToString();

                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return string.Empty;
        }

        /// <summary>
        /// Extract and save all attachments to a directory
        /// </summary>
        public void ExtractAttachments(EDI275Document document, string outputDirectory)
        {
            try
            {
                _logger.LogInformation("Extracting {Count} attachments to {Dir}",
                    document.Attachments.Count, outputDirectory);

                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                foreach (var attachment in document.Attachments)
                {
                    try
                    {
                        object value = attachment.SaveToFile(outputDirectory);
                        _logger.LogInformation("Saved attachment: {FileName}", attachment.FileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving attachment: {FileName}", attachment.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting attachments");
            };
        }

        private void Parse275(dynamic ts275, EDI275Document document)
        {
            try
            {
                if (ts275 == null)
                {
                    _logger.LogWarning("TS275 is null.");
                    return;
                }

         
                foreach (var prop in ts275.GetType().GetProperties())
                {

                    string s =  prop.Name;
                       string ss = prop.PropertyType.FullName;
                }
             
                //
                // ST
                if (HasProperty(ts275, "ST") && ts275.ST != null)
                {
                    ParseST(ts275.ST, document);
                }

                // BGN
                if (HasProperty(ts275, "BGN") && ts275.BGN != null)
                {
                    ParseBgnSegment(ts275.BGN, document);
                }

                if (HasProperty(ts275, "NM1") && ts275.NM1 != null)
                {
                     ParseNameSegment(ts275.NM1LOOP.NM1, document);
   
                }

                if (HasProperty(ts275, "PWK") && ts275.PWK != null)
                {
                    ParsePaperworkSegment(ts275.PWK, document);
                }

                
                ParsePossibleCollection((object)ts275, "DTP", (Action<object>)(dtp => ParseDtpSegment(dtp, document)));
                ParsePossibleCollection((object)ts275, "TRN", (Action<object>)(trn => ParseTrnSegment(trn, document)));
                ParsePossibleCollection((object)ts275, "REF", (Action<object>)(rf => ParseRefSegment(rf, document)));

                // Top-level loops
                ParseNamedLoops(ts275, document);

                _logger.LogInformation(
                    "Parsed 275 transaction - Type: {Type}, Control: {Control}, Attachments: {Count}",
                    document.TransactionSetType,
                    document.TransactionSetControlNumber,
                    document.Attachments.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing TS275 transaction");
            }
        }
        private void ParseBgnSegment(dynamic segment, EDI275Document document)
        {
            try
            {
                var segmentString = segment.ToString() ?? string.Empty;
                var elements = segmentString.Split('*');

                document.PurposeCode = segment.BGN.TransactionSetPurposeCode_02;
                document.ReferenceId = segment.BGN.ReferenceIdentification_03;
                document.BGNdate = segment.BGN.Date_03;
                document.BGNtime = segment.BGN.Time_04;

    
                _logger.LogInformation(
                    "BGN - Purpose: {Purpose}, Reference: {Reference},  Date: {Date}, TimeCode: {TimeCode}",
                    document.PurposeCode,
                    document.ReferenceId,
                    document.BGNdate,
                    document.BGNtime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing BGN segment");
            }
        }


        public sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new();

            public new bool Equals(object x, object y) => ReferenceEquals(x, y);

            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }

        private readonly HashSet<object> _visited = new HashSet<object>(ReferenceEqualityComparer.Instance);


        private void ParseNamedLoops(dynamic node, EDI275Document document)
        {
            if (node == null) return;

            var type = node.GetType();
            var props = type.GetProperties();



            foreach (var prop in props)
            {
                var value = prop.GetValue(node);
                if (value == null) continue;

                var propName = prop.Name;

             //   Parse segment-like properties directly
                if (propName.StartsWith("NM1", StringComparison.OrdinalIgnoreCase) || propName == "NM1")
                {
            
                    ParseNameSegment(value, document);
                    ParsePaperworkSegmentFromString("PWK*OZ*EL****Sample File for EDI 275*Medical Records Attachment~", document);
                    ParseBinarySegmentFromString("BIN*1024*VGhpcyBpcyBhIHNhbXBsZSBhdHRhY2htZW50IGZpbGUgY29udGVudC4gVGhpcyB3b3VsZCBub3JtYWxseSBiZSBiaW5hcnkgZGF0YSBlbmNvZGVkIGluIEJhc2U2NC4gSW4gYSByZWFsIEVESSAyNzUsIHRoaXMgd291bGQgY29udGFpbiB0aGUgYWN0dWFsIG1lZGljYWwgcmVjb3JkcyBvciBvdGhlciBkb2N1bWVudHMgYXMgYmluYXJ5IGRhdGEu~", document);
                    SaveAttachments(document, @"C:\Randy-Kirksey_R\Attachments");
                }
                else
                if (propName.StartsWith("PWK", StringComparison.OrdinalIgnoreCase) || propName == "PWK")
                {
                    ParsePaperworkSegment(value, document);
                }
                else if (propName.StartsWith("CAT", StringComparison.OrdinalIgnoreCase) || propName == "CAT")
                {
                    ParseCatSegment(value, document);
                }
                else if (propName.StartsWith("EFI", StringComparison.OrdinalIgnoreCase) || propName == "EFI")
                {
                    ParseEfiSegment(value, document);
                }
                else if (propName.StartsWith("BIN", StringComparison.OrdinalIgnoreCase) || propName == "BIN")
                {
                    ParseBinarySegment(value, document);
                }
                //else if (propName.StartsWith("REF", StringComparison.OrdinalIgnoreCase) || propName == "REF")
                //{
                //    ParsePossibleCollectionOrSingle(value, x => ParseRefSegment(x, document));
                //}
                //else if (propName.StartsWith("DTP", StringComparison.OrdinalIgnoreCase) || propName == "DTP")
                //{
                //    ParsePossibleCollectionOrSingle(value, x => ParseDtpSegment(x, document));
                //}
                //else if (propName.StartsWith("TRN", StringComparison.OrdinalIgnoreCase) || propName == "TRN")
                //{
                //    ParsePossibleCollectionOrSingle(value, x => ParseTrnSegment(x, document));
                //}

                //else if (propName.StartsWith("BGN", StringComparison.OrdinalIgnoreCase) || propName == "BGN")
                //{
                //    ParsePossibleCollectionOrSingle(value, x => ParseTrnSegment(x, document));
                //}
                else
                {
                    // Recurse into loops and child objects
                    //if (!(value is string))
                    //{
                    //    ParsePossibleCollectionOrSingle(value, x => ParseNamedLoops(x, document));
                    //}
                }
            }
        }
        private void ParsePossibleCollection(object parent, string propertyName, Action<object> action)
        {
            var prop = parent.GetType().GetProperty(propertyName);
            if (prop == null) return;

            var value = prop.GetValue(parent);
            if (value == null) return;

            ParsePossibleCollectionOrSingle(value, action);
        }

        private void ParsePossibleCollectionOrSingle(object value, Action<object> action)
        {
            if (value == null) return;

            if (value is System.Collections.IEnumerable enumerable && value is not string)
            {
                foreach (var item in enumerable)
                {
                    if (item != null)
                        action(item);
                }
            }
            else
            {
                action(value);
            }
        }

        private void ParseTrnSegment(object segment, EDI275Document document)
        {
            try
            {
                var segmentString = segment.ToString() ?? string.Empty;
                var elements = segmentString.Split('*');

                if (elements.Length < 3)
                {
                    _logger.LogWarning("TRN segment is too short: {Segment}", segmentString);
                    return;
                }

                var traceTypeCode = elements.Length > 1 ? elements[1] : string.Empty;   // TRN01
                var referenceId = elements.Length > 2 ? elements[2] : string.Empty;      // TRN02
                var originatorId = elements.Length > 3 ? elements[3] : string.Empty;     // TRN03
                var referenceId2 = elements.Length > 4 ? elements[4] : string.Empty;     // TRN04

                document.TraceTypeCode = traceTypeCode;
                document.AttachmentControlNumber = referenceId;

                _logger.LogInformation(
                    "TRN - TraceType: {TraceType}, AttachmentControlNumber: {ACN}, Originator: {Originator}, Ref2: {Ref2}",
                    traceTypeCode,
                    document.AttachmentControlNumber,
                    originatorId,
                    referenceId2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing TRN segment");
            }
        }

        private void ParseRefSegment(object segment, EDI275Document document)
        {
            try
            {
                var segmentString = segment.ToString() ?? string.Empty;
                var elements = segmentString.Split('*');

                if (elements.Length < 3)
                {
                    _logger.LogWarning("REF segment is too short: {Segment}", segmentString);
                    return;
                }

                var qualifier = elements[1];
                var value = elements[2];

                switch (qualifier)
                {
                    case "D9": // Claim Number / Claim Identifier in many guides
                        document.ClaimNumber = value;
                        break;

                    case "EJ": // Patient Control Number in many healthcare contexts
                        document.PatientControlNumber = value;
                        break;

                    case "EA": // Medical Record Number / Medical Record ID in many contexts
                        document.MedicalRecordNumber = value;
                        break;

                    case "1K": // Payer Claim Number or similar in some guides
                        document.PayerClaimControlNumber = value;
                        break;

                    default:
                        document.ReferenceValues[qualifier] = value;
                        break;
                }

                _logger.LogInformation(
                    "REF - Qualifier: {Qualifier}, Value: {Value}",
                    qualifier,
                    value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing REF segment");
            }
        }

        private void ParseDtpSegment(object segment, EDI275Document document)
        {
            try
            {
                var segmentString = segment.ToString() ?? string.Empty;
                var elements = segmentString.Split('*');

                if (elements.Length < 4)
                {
                    _logger.LogWarning("DTP segment is too short: {Segment}", segmentString);
                    return;
                }

                var qualifier = elements[1];   // e.g. 472
                var format = elements[2];      // e.g. D8 or RD8
                var value = elements[3];       // e.g. 20240601 or 20240601-20240610

                if (format == "D8")
                {
                    if (DateTime.TryParseExact(
                            value,
                            "yyyyMMdd",
                            null,
                            System.Globalization.DateTimeStyles.None,
                            out var singleDate))
                    {
                        document.ClaimServiceDate = singleDate;
                        _logger.LogInformation(
                            "DTP - Qualifier: {Qualifier}, Date: {Date}",
                            qualifier,
                            document.ClaimServiceDate);
                    }
                    else
                    {
                        _logger.LogWarning("Could not parse DTP D8 date: {Value}", value);
                    }
                }
                else if (format == "RD8")
                {
                    var parts = value.Split('-');
                    if (parts.Length == 2 &&
                        DateTime.TryParseExact(parts[0], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var fromDate) &&
                        DateTime.TryParseExact(parts[1], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var toDate))
                    {
                        document.ClaimServiceDate = fromDate;
                        document.ClaimServiceEndDate = toDate;

                        _logger.LogInformation(
                            "DTP - Qualifier: {Qualifier}, From: {From}, To: {To}",
                            qualifier,
                            fromDate,
                            toDate);
                    }
                    else
                    {
                        _logger.LogWarning("Could not parse DTP RD8 date range: {Value}", value);
                    }
                }
                else
                {
                    _logger.LogDebug(
                        "DTP - Unhandled format {Format}, qualifier {Qualifier}, value {Value}",
                        format, qualifier, value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing DTP segment");
            }
        }
        

        private void ParseCatSegment(object segment, EDI275Document document)
        {
            try
            {
                var segmentString = segment.ToString() ?? string.Empty;
                var elements = segmentString.Split('*');

                if (elements.Length < 3)
                {
                    _logger.LogWarning("CAT segment is too short: {Segment}", segmentString);
                    return;
                }

                // CAT01 - Report Type Code, commonly AE for attachment
                var reportTypeCode = elements.Length > 1 ? elements[1] : string.Empty;

                // CAT02 - Report Transmission Code / Attachment Format Code
                var attachmentFormatCode = elements.Length > 2 ? elements[2] : string.Empty;

                document.ReportTypeCode = reportTypeCode;
                document.AttachmentFormatCode = attachmentFormatCode;

                // If you have a "current attachment" object, populate it too
                var currentAttachment = document.Attachments.LastOrDefault();
                if (currentAttachment != null)
                {
                    currentAttachment.AttachmentFormatCode = attachmentFormatCode;
                }

                _logger.LogInformation(
                    "CAT - ReportType: {ReportType}, AttachmentFormat: {Format}",
                    reportTypeCode,
                    attachmentFormatCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing CAT segment");
            }
        }

         // C#
        private void ParseBinarySegmentFromString(string segmentString, EDI275Document document)
        {
            var elements = segmentString.Trim().TrimEnd('~').Split('*');
            if (elements.Length < 3 || elements[0] != "BIN")
                return;

            var attachment = document.Attachments.LastOrDefault();
            if (attachment == null)
            {
                attachment = new Attachment { AttachmentDate = DateTime.UtcNow };
                document.Attachments.Add(attachment);
            }

            if (long.TryParse(elements[1], out var fileSize))
                attachment.FileSize = fileSize;

            var base64 = elements[2];
            attachment.Base64Data = base64;

            try
            {
                var bytes = Convert.FromBase64String(base64);
                attachment.BinaryData = bytes;

                // Try to get extension from other fields first (if you parse them elsewhere)
                var ext = GetExtensionFromFormatCode(attachment.AttachmentFormatCode)
                          ?? GetExtensionFromBytes(bytes);

                // If no filename set, create a safe name with detected extension
                if (string.IsNullOrWhiteSpace(attachment.FileName))
                {
                    var idx = document.Attachments.Count;
                    attachment.FileName = $"attachment_{idx}{(ext ?? ".bin")}";
                }
                else
                {
                    // ensure filename has an extension; add detected one if missing
                    if (string.IsNullOrWhiteSpace(Path.GetExtension(attachment.FileName)) && ext != null)
                        attachment.FileName = Path.ChangeExtension(attachment.FileName, ext);
                }
            }
            catch (FormatException)
            {
                // treat as text if base64 decode fails for some reason
                attachment.TextData = base64;
                if (string.IsNullOrWhiteSpace(attachment.FileName))
                    attachment.FileName = $"attachment_{document.Attachments.Count}.txt";
            }
        }

        public void SaveAttachments(EDI275Document document, string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);

            foreach (var attachment in document.Attachments)
            {
                var fileName = string.IsNullOrWhiteSpace(attachment.FileName)
                    ? $"attachment_{document.Attachments.IndexOf(attachment) + 1}.bin"
                    : SanitizeFileName(attachment.FileName);

                var path = Path.Combine(outputDirectory, fileName);

                if (attachment.BinaryData != null && attachment.BinaryData.Length > 0)
                {
                    File.WriteAllBytes(path, attachment.BinaryData);
                }
                else if (!string.IsNullOrWhiteSpace(attachment.Base64Data))
                {
                    var bytes = Convert.FromBase64String(attachment.Base64Data);
                    File.WriteAllBytes(path, bytes);
                }
                else if (!string.IsNullOrWhiteSpace(attachment.TextData))
                {
                    File.WriteAllText(path, attachment.TextData);
                }
            }
        }

        /* Helpers */
        private static string? GetExtensionFromFormatCode(string? formatCode)
        {
            if (string.IsNullOrWhiteSpace(formatCode)) return null;
            formatCode = formatCode.Trim().ToLowerInvariant();

            // Map your format codes to extensions (expand as needed)
            return formatCode switch
            {
                "pdf" or "application/pdf" => ".pdf",
                "png" or "image/png" => ".png",
                "jpg" or "jpeg" or "image/jpeg" => ".jpg",
                "gif" or "image/gif" => ".gif",
                "tif" or "tiff" => ".tiff",
                "txt" or "text/plain" => ".txt",
                "xml" => ".xml",
                "csv" => ".csv",
                "zip" => ".zip",
                "docx" => ".docx",
                _ => null
            };
        }

        private static string? GetExtensionFromBytes(byte[] data)
        {
            if (data.Length >= 4)
            {
                // PDF: %PDF
                if (data[0] == 0x25 && data[1] == 0x50 && data[2] == 0x44 && data[3] == 0x46) return ".pdf";
                // PNG
                if (data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47) return ".png";
                // JPG
                if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF) return ".jpg";
                // GIF: GIF87a or GIF89a
                if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46) return ".gif";
                // ZIP / OOXML (docx/xlsx/pptx)
                if (data[0] == 0x50 && data[1] == 0x4B && data[2] == 0x03 && data[3] == 0x04) return ".zip";
                // RTF { \rtf
                if (data[0] == 0x7B && data.Length > 4 && data[1] == 0x5C && data[2] == 0x72 && data[3] == 0x74) return ".rtf";
                // TIFF (II* or MM*)
                if ((data[0] == 0x49 && data[1] == 0x49 && data[2] == 0x2A) || (data[0] == 0x4D && data[1] == 0x4D && data[2] == 0x00)) return ".tiff";
            }

            // Text-ish detection (UTF-8 printable)
            var sampleLen = Math.Min(256, data.Length);
            var isText = true;
            for (int i = 0; i < sampleLen; i++)
            {
                var b = data[i];
                if (b == 0) { isText = false; break; } // NUL -> likely binary
                                                       // allow common whitespace and printable ranges
                if (b < 0x09) { isText = false; break; }
            }
            if (isText) return ".txt";

            return null;
        }

        private static string SanitizeFileName(string input)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                input = input.Replace(c, '_');
            return input;
        }

        // C#
        private void ParsePaperworkSegmentFromString(string segmentString, EDI275Document document)
        {
            var elements = segmentString.Trim().TrimEnd('~').Split('*');

            if (elements.Length < 2 || elements[0] != "PWK")
                return;

            var attachment = new Attachment
            {
                AttachmentDate = DateTime.UtcNow,
                AttachmentTypeCode = elements.Length > 1 ? elements[1] : string.Empty,
                TransmissionCode = elements.Length > 2 ? elements[2] : string.Empty,
                Description = elements.Length > 6 ? elements[6] : string.Empty
            };

            // Try to find an explicit filename candidate in the segment (some providers include it)
            string? explicitCandidate = elements.Skip(1)
                                               .FirstOrDefault(e => e.Contains('.') && e.Length <= 260);

            // Try to find a format code in typical positions (extend if needed)
            string? formatCode = null;
            if (elements.Length > 3) formatCode = elements[3];
            if (string.IsNullOrWhiteSpace(formatCode) && elements.Length > 4) formatCode = elements[4];
            attachment.AttachmentFormatCode = string.IsNullOrWhiteSpace(formatCode) ? null : formatCode;

            // Determine extension preference: 1) explicit candidate, 2) format code mapping, 3) description, 4) default
            string? ext =
                GetExtensionFromFilename(explicitCandidate) ??
                GetExtensionFromFormatCode(formatCode) ??
                GetExtensionFromFilename(attachment.Description) ??
                ".txt";

            // Build a base name (prefer explicit candidate name without extension, then description, else a numbered default)
            string baseName = !string.IsNullOrWhiteSpace(explicitCandidate)
                ? Path.GetFileNameWithoutExtension(explicitCandidate)
                : (!string.IsNullOrWhiteSpace(attachment.Description) ? attachment.Description : $"attachment_{document.Attachments.Count + 1}");

            baseName = SanitizeFileName(baseName);
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = $"attachment_{document.Attachments.Count + 1}";

            var candidateFileName = Path.ChangeExtension(baseName, ext);

            // Ensure unique filename within this document
            var uniqueName = candidateFileName;
            var idx = 1;
            while (document.Attachments.Any(a => string.Equals(a.FileName, uniqueName, StringComparison.OrdinalIgnoreCase)))
            {
                uniqueName = $"{Path.GetFileNameWithoutExtension(candidateFileName)}_{++idx}{Path.GetExtension(candidateFileName)}";
            }

            attachment.FileName = uniqueName;
            document.Attachments.Add(attachment);
        }

        private static string? GetExtensionFromFilename(string? candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate)) return null;
            try
            {
                var ext = Path.GetExtension(candidate).Trim();
                if (string.IsNullOrWhiteSpace(ext)) return null;
                // Normalize common no-dot values
                return ext.StartsWith(".") ? ext : "." + ext;
            }
            catch
            {
                return null;
            }
        }


        private void ParseEfiSegment(object segment, EDI275Document document)
        {
            try
            {
                var segmentString = segment.ToString() ?? string.Empty;
                var elements = segmentString.Split('*');

                if (elements.Length < 2)
                {
                    _logger.LogWarning("EFI segment is too short: {Segment}", segmentString);
                    return;
                }

                // EFI01 - Security Level Code
                var securityLevelCode = elements.Length > 1 ? elements[1] : string.Empty;

                // EFI11 - File Name (optional, position 11 in the segment)
                var fileName = elements.Length > 11 ? elements[11] : string.Empty;

                document.SecurityLevelCode = securityLevelCode;

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    document.FileName = fileName;
                }

                var currentAttachment = document.Attachments.LastOrDefault();
                if (currentAttachment != null)
                {
                    currentAttachment.SecurityLevelCode = securityLevelCode;

                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        currentAttachment.FileName = fileName;
                    }
                }

                _logger.LogInformation(
                    "EFI - SecurityLevel: {SecurityLevel}, FileName: {FileName}",
                    securityLevelCode,
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing EFI segment");
            }
        }

        private bool HasProperty(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }
    }



    }


