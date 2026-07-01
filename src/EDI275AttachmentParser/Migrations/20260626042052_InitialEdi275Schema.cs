using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDI275AttachmentParser.Migrations
{
    /// <inheritdoc />
    public partial class InitialEdi275Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Edi275Imports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportedAtUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    SourceFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    InterchangeControlNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GroupControlNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransactionSetControlNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransactionSetType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ImplementationVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SenderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReceiverId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClaimNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PatientControlNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MedicalRecordNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PayerClaimControlNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PurposeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TraceTypeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AttachmentControlNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReportTypeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AttachmentFormatCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SecurityLevelCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    ClaimServiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClaimServiceEndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Edi275Imports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Edi275Attachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Edi275ImportEntityId = table.Column<int>(type: "int", nullable: false),
                    AttachmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttachmentTypeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TransmissionCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AttachmentControlNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    AttachmentFormatCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SecurityLevelCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Base64Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BinaryData = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Edi275Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Edi275Attachments_Edi275Imports_Edi275ImportEntityId",
                        column: x => x.Edi275ImportEntityId,
                        principalTable: "Edi275Imports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Edi275ParseLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Edi275ImportEntityId = table.Column<int>(type: "int", nullable: false),
                    LoggedAtUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Edi275ParseLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Edi275ParseLogs_Edi275Imports_Edi275ImportEntityId",
                        column: x => x.Edi275ImportEntityId,
                        principalTable: "Edi275Imports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Edi275PatientReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Edi275ImportEntityId = table.Column<int>(type: "int", nullable: false),
                    EntityIdentifierCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EntityTypeQualifier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PatientId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PatientName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NamePrefix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NameSuffix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdentificationCodeQualifier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IdentificationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Edi275PatientReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Edi275PatientReports_Edi275Imports_Edi275ImportEntityId",
                        column: x => x.Edi275ImportEntityId,
                        principalTable: "Edi275Imports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Edi275Attachments_Edi275ImportEntityId",
                table: "Edi275Attachments",
                column: "Edi275ImportEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Edi275ParseLogs_Edi275ImportEntityId",
                table: "Edi275ParseLogs",
                column: "Edi275ImportEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Edi275PatientReports_Edi275ImportEntityId",
                table: "Edi275PatientReports",
                column: "Edi275ImportEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Edi275Attachments");

            migrationBuilder.DropTable(
                name: "Edi275ParseLogs");

            migrationBuilder.DropTable(
                name: "Edi275PatientReports");

            migrationBuilder.DropTable(
                name: "Edi275Imports");
        }
    }
}
