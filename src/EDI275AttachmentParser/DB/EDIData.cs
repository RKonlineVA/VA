using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDI275AttachmentParser.Data.Entities;

namespace DI275AttachmentParser.Data
{
    public class Edi275DbContext : DbContext
    {
        public Edi275DbContext(DbContextOptions<Edi275DbContext> options)
            : base(options)
        {
        }

        public DbSet<Edi275DocumentEntity> Edi275Documents => Set<Edi275DocumentEntity>();
        public DbSet<PatientReportEntity> PatientReports => Set<PatientReportEntity>();
        public DbSet<AttachmentEntity> Attachments => Set<AttachmentEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Edi275DocumentEntity>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.InterchangeControlNumber);
                entity.HasIndex(x => x.TransactionSetControlNumber);
                entity.HasIndex(x => x.ClaimNumber);
                entity.HasIndex(x => x.PatientControlNumber);

                entity.Property(x => x.CreatedUtc)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<PatientReportEntity>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Document)
                      .WithMany(x => x.PatientReports)
                      .HasForeignKey(x => x.Edi275DocumentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AttachmentEntity>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Document)
                      .WithMany(x => x.Attachments)
                      .HasForeignKey(x => x.Edi275DocumentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(x => x.BinaryData)
                      .HasColumnType("varbinary(max)");

                entity.Property(x => x.Base64Data)
                      .HasColumnType("nvarchar(max)");

                entity.Property(x => x.TextData)
                      .HasColumnType("nvarchar(max)");

                entity.Property(x => x.Description)
                      .HasColumnType("nvarchar(500)");
            });
        }
    }
}