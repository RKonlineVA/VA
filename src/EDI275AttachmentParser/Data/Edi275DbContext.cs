using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EDI275AttachmentParser.Models;
using Microsoft.EntityFrameworkCore;

namespace EDI275AttachmentParser.Data
{
    public class Edi275DbContext : DbContext
    {
        public Edi275DbContext(DbContextOptions<Edi275DbContext> options) : base(options)
        {
        }

        public DbSet<Edi275ImportEntity> Edi275Imports => Set<Edi275ImportEntity>();
        public DbSet<Edi275PatientReportEntity> Edi275PatientReports => Set<Edi275PatientReportEntity>();
        public DbSet<Edi275AttachmentEntity> Edi275Attachments => Set<Edi275AttachmentEntity>();
        public DbSet<Edi275ParseLogEntity> Edi275ParseLogs => Set<Edi275ParseLogEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Edi275ImportEntity>(entity =>
            {
                entity.ToTable("Edi275Imports");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ImportedAtUtc).HasColumnType("datetime2(3)");
                entity.Property(x => x.SourceFilePath).HasMaxLength(500);
                entity.Property(x => x.InterchangeControlNumber).HasMaxLength(50);
                entity.Property(x => x.GroupControlNumber).HasMaxLength(50);
                entity.Property(x => x.TransactionSetControlNumber).HasMaxLength(50);
                entity.Property(x => x.TransactionSetType).HasMaxLength(20);
                entity.Property(x => x.ImplementationVersion).HasMaxLength(50);
                entity.Property(x => x.SenderId).HasMaxLength(100);
                entity.Property(x => x.ReceiverId).HasMaxLength(100);
                entity.Property(x => x.ClaimNumber).HasMaxLength(100);
                entity.Property(x => x.PatientControlNumber).HasMaxLength(100);
                entity.Property(x => x.MedicalRecordNumber).HasMaxLength(100);
                entity.Property(x => x.PayerClaimControlNumber).HasMaxLength(100);
                entity.Property(x => x.PurposeCode).HasMaxLength(20);
                entity.Property(x => x.ReferenceId).HasMaxLength(100);
                entity.Property(x => x.TraceTypeCode).HasMaxLength(20);
                entity.Property(x => x.AttachmentControlNumber).HasMaxLength(100);
                entity.Property(x => x.ReportTypeCode).HasMaxLength(20);
                entity.Property(x => x.AttachmentFormatCode).HasMaxLength(50);
                entity.Property(x => x.SecurityLevelCode).HasMaxLength(50);
                entity.Property(x => x.FileName).HasMaxLength(260);
            });

            modelBuilder.Entity<Edi275PatientReportEntity>(entity =>
            {
                entity.ToTable("Edi275PatientReports");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.EntityIdentifierCode).HasMaxLength(20);
                entity.Property(x => x.EntityTypeQualifier).HasMaxLength(20);
                entity.Property(x => x.PatientId).HasMaxLength(100);
                entity.Property(x => x.PatientName).HasMaxLength(250);
                entity.Property(x => x.LastName).HasMaxLength(100);
                entity.Property(x => x.FirstName).HasMaxLength(100);
                entity.Property(x => x.MiddleName).HasMaxLength(100);
                entity.Property(x => x.NamePrefix).HasMaxLength(50);
                entity.Property(x => x.NameSuffix).HasMaxLength(50);
                entity.Property(x => x.IdentificationCodeQualifier).HasMaxLength(20);
                entity.Property(x => x.IdentificationCode).HasMaxLength(100);

                entity.HasOne(x => x.Edi275Import)
                    .WithMany(x => x.PatientReports)
                    .HasForeignKey(x => x.Edi275ImportEntityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Edi275AttachmentEntity>(entity =>
            {
                entity.ToTable("Edi275Attachments");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.AttachmentTypeCode).HasMaxLength(20);
                entity.Property(x => x.TransmissionCode).HasMaxLength(20);
                entity.Property(x => x.AttachmentControlNumber).HasMaxLength(100);
                entity.Property(x => x.Description).HasMaxLength(500);
                entity.Property(x => x.FileName).HasMaxLength(260);
                entity.Property(x => x.AttachmentFormatCode).HasMaxLength(50);
                entity.Property(x => x.SecurityLevelCode).HasMaxLength(50);
                entity.Property(x => x.Base64Data).HasColumnType("nvarchar(max)");
                entity.Property(x => x.TextData).HasColumnType("nvarchar(max)");
                entity.Property(x => x.BinaryData).HasColumnType("varbinary(max)");

                entity.HasOne(x => x.Edi275Import)
                    .WithMany(x => x.Attachments)
                    .HasForeignKey(x => x.Edi275ImportEntityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Edi275ParseLogEntity>(entity =>
            {
                entity.ToTable("Edi275ParseLogs");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.LoggedAtUtc).HasColumnType("datetime2(3)");
                entity.Property(x => x.Level).HasMaxLength(20);
                entity.Property(x => x.Message).HasColumnType("nvarchar(max)");
                entity.Property(x => x.Exception).HasColumnType("nvarchar(max)");

                entity.HasOne(x => x.Edi275Import)
                    .WithMany(x => x.ParseLogs)
                    .HasForeignKey(x => x.Edi275ImportEntityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
