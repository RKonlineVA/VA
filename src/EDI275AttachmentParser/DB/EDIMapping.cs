using EDI275AttachmentParser.Data.Entities;
using EDI275AttachmentParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI275AttachmentParser.DB
{
    internal class EDIMapping
    {
    }



namespace EDI275AttachmentParser.Data.Mapping
    {
        public static class Edi275Mapper
        {
            public static Edi275DocumentEntity ToEntity(EDI275Document source)
            {
                var entity = new Edi275DocumentEntity
                {
                    InterchangeControlNumber = source.InterchangeControlNumber,
                    GroupControlNumber = source.GroupControlNumber,
                    TransactionSetType = source.TransactionSetType,
                    TransactionSetControlNumber = source.TransactionSetControlNumber,
                    ImplementationVersion = source.ImplementationVersion,
                    SenderId = source.SenderId,
                    ReceiverId = source.ReceiverId,
                    TransactionDate = source.TransactionDate,
                    PurposeCode = source.PurposeCode,
                    ReferenceId = source.ReferenceId,
                    //IsSolicited = source.IsSolicited,
                    //IsUnsolicited = source.IsUnsolicited,
                    TraceTypeCode = source.TraceTypeCode,
                    AttachmentControlNumber = source.AttachmentControlNumber,
                    ClaimNumber = source.ClaimNumber,
                    PatientControlNumber = source.PatientControlNumber,
                    MedicalRecordNumber = source.MedicalRecordNumber,
                    PayerClaimControlNumber = source.PayerClaimControlNumber,
                    ClaimServiceDate = source.ClaimServiceDate,
                    ClaimServiceEndDate = source.ClaimServiceEndDate,
                    ReportTypeCode = source.ReportTypeCode,
                    AttachmentFormatCode = source.AttachmentFormatCode,
                    SecurityLevelCode = source.SecurityLevelCode,
                    FileName = source.FileName
                };

                if (source.PatientReports != null)
                {
                    foreach (var p in source.PatientReports)
                    {
                        entity.PatientReports.Add(new PatientReportEntity
                        {
                            EntityIdentifierCode = p.EntityIdentifierCode,
                            EntityTypeQualifier = p.EntityTypeQualifier,
                            LastName = p.LastName,
                            FirstName = p.FirstName,
                            MiddleName = p.MiddleName,
                            NamePrefix = p.NamePrefix,
                            NameSuffix = p.NameSuffix,
                            IdentificationCodeQualifier = p.IdentificationCodeQualifier,
                            IdentificationCode = p.IdentificationCode,
                            PatientName = p.PatientName,
                            PatientId = p.PatientId
                        });
                    }
                }

                if (source.Attachments != null)
                {
                    foreach (var a in source.Attachments)
                    {
                        entity.Attachments.Add(new AttachmentEntity
                        {
                            AttachmentTypeCode = a.AttachmentTypeCode,
                            TransmissionCode = a.TransmissionCode,
                            AttachmentControlNumber = a.AttachmentControlNumber,
                            Description = a.Description,
                            AttachmentFormatCode = a.AttachmentFormatCode,
                            SecurityLevelCode = a.SecurityLevelCode,
                            FileName = a.FileName,
                            FileSize = a.FileSize,
                            BinaryData = a.BinaryData,
                            Base64Data = a.Base64Data,
                            TextData = a.TextData,
                            AttachmentDate = a.AttachmentDate
                        });
                    }
                }

                return entity;
            }
        }
    }
}
