using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EDI275AttachmentParser.Models;
using EDI275AttachmentParser.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EDI275AttachmentParser.Tests;

/// <summary>
/// Unit tests for EDI275Parser.ParseNameSegment method
/// Tests the parsing of NM1 segments containing patient information
/// </summary>
public class EDI275ParserParseNameSegmentTests
{
    private readonly Mock<ILogger<EDI275Parser>> _mockLogger;
    private readonly EDI275Parser _parser;

    public EDI275ParserParseNameSegmentTests()
    {
        _mockLogger = new Mock<ILogger<EDI275Parser>>();
        _parser = new EDI275Parser(_mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public void ParseNameSegment_WithValidSingleSegment_ParsesPatientInfoCorrectly()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Smith",
                NameFirst_04 = "John",
                NameMiddle_05 = "Michael",
                NamePrefix_06 = "Dr.",
                NameSuffix_07 = "Jr.",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "123456789"
            }
        };

        // Act
        _parser.ParseNameSegment(segment, document);

        // Assert
        Assert.NotEmpty(document.PatientReports);
        Assert.Single(document.PatientReports);

        var patientReport = document.PatientReports[0];
        Assert.Equal("IL", patientReport.EntityIdentifierCode);
        Assert.Equal("1", patientReport.EntityTypeQualifier);
        Assert.Equal("Smith", patientReport.LastName);
        Assert.Equal("John", patientReport.FirstName);
        Assert.Equal("Michael", patientReport.MiddleName);
        Assert.Equal("Dr.", patientReport.NamePrefix);
        Assert.Equal("Jr.", patientReport.NameSuffix);
        Assert.Equal("MI", patientReport.IdentificationCodeQualifier);
        Assert.Equal("123456789", patientReport.IdentificationCode);
        Assert.Equal("123456789", patientReport.PatientId); // MI sets PatientId
    }

    [Fact]
    public void ParseNameSegment_WithMultipleSegments_ParsesAllPatientReports()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic[] segments = new dynamic[]
        {
            new { NM1 = new {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Smith",
                NameFirst_04 = "John",
                NameMiddle_05 = "Michael",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "111111111"
            }},
            new { NM1 = new {
                EntityIdentifierCode_01 = "QC",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Doe",
                NameFirst_04 = "Jane",
                NameMiddle_05 = "Ann",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "222222222"
            }}
        };

        // Act
        _parser.ParseNameSegment(segments, document);

        // Assert
        Assert.Equal(2, document.PatientReports.Count);
        Assert.Equal("John Michael Smith", document.PatientReports[0].PatientName);
        Assert.Equal("Jane Ann Doe", document.PatientReports[1].PatientName);
    }

    [Fact]
    public void ParseNameSegment_WithOrganizationEntityType_UsesOnlyLastNameAsPatientName()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "2", // 2 = Non-Person/Organization
                NameLastorOrganizationName_03 = "Acme Hospital",
                NameFirst_04 = "",
                NameMiddle_05 = "",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "XX",
                IdentificationCode_09 = "987654321"
            }
        };

        // Act
        _parser.ParseNameSegment(segment, document);

        // Assert
        var patientReport = document.PatientReports[0];
        Assert.Equal("Acme Hospital", patientReport.PatientName);
        Assert.Equal("2", patientReport.EntityTypeQualifier);
    }

    [Fact]
    public void ParseNameSegment_WithMIIdentifier_SetsMedicalId()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Johnson",
                NameFirst_04 = "Bob",
                NameMiddle_05 = "",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "MED123456"
            }
        };

        // Act
        _parser.ParseNameSegment(segment, document);

        // Assert
        var patientReport = document.PatientReports[0];
        Assert.Equal("MED123456", patientReport.PatientId);
    }

    [Fact]
    public void ParseNameSegment_WithNonMIIdentifier_SetsPatientId()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Wilson",
                NameFirst_04 = "Alice",
                NameMiddle_05 = "",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "XX",
                IdentificationCode_09 = "OTHER123456"
            }
        };

        // Act
        _parser.ParseNameSegment(segment, document);

        // Assert
        var patientReport = document.PatientReports[0];
        Assert.Equal("OTHER123456", patientReport.PatientId);
    }

    [Fact]
    public void ParseNameSegment_WithPartialName_BuildsPatientNameFromAvailableFields()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Taylor",
                NameFirst_04 = "Emma",
                NameMiddle_05 = null, // No middle name
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "555555555"
            }
        };

        // Act
        _parser.ParseNameSegment(segment, document);

        // Assert
        var patientReport = document.PatientReports[0];
        Assert.Equal("Emma Taylor", patientReport.PatientName);
    }

    [Fact]
    public void ParseNameSegment_WithLastNameOnly_BuildsPatientName()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Brown",
                NameFirst_04 = null,
                NameMiddle_05 = null,
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "666666666"
            }
        };

        // Act
        _parser.ParseNameSegment(segment, document);

        // Assert
        var patientReport = document.PatientReports[0];
        Assert.Equal("Brown", patientReport.PatientName);
    }

    #endregion

    #region Duplicate Detection Tests

    [Fact]
    public void ParseNameSegment_WithDuplicatePatients_IgnoresDuplicates()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Martinez",
                NameFirst_04 = "Carlos",
                NameMiddle_05 = "",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "777777777"
            }
        };

        // Act - Parse same segment twice
        _parser.ParseNameSegment(segment, document);
        _parser.ParseNameSegment(segment, document);

        // Assert - Should only have one patient report due to duplicate detection
        Assert.Single(document.PatientReports);
    }

    [Fact]
    public void ParseNameSegment_WithDifferentIdentificationCodes_AddsAsSeperateReports()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment1 = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Anderson",
                NameFirst_04 = "David",
                NameMiddle_05 = "",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "888888888"
            }
        };

        dynamic segment2 = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Anderson",
                NameFirst_04 = "David",
                NameMiddle_05 = "",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "999999999" // Different ID
            }
        };

        // Act
        _parser.ParseNameSegment(segment1, document);
        _parser.ParseNameSegment(segment2, document);

        // Assert
        Assert.Equal(2, document.PatientReports.Count);
    }

    #endregion

    #region Edge Cases & Error Handling

    [Fact]
    public void ParseNameSegment_WithNullSegments_HandlesGracefully()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment = null;

        // Act & Assert - Should not throw
        _parser.ParseNameSegment(segment, document);
        Assert.Empty(document.PatientReports);
    }

    [Fact]
    public void ParseNameSegment_WithNullDocument_HandlesGracefully()
    {
        // Arrange
        dynamic segment = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Harris",
                NameFirst_04 = "Eve",
                NameMiddle_05 = "",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "333333333"
            }
        };

        // Act & Assert - Should not throw
        _parser.ParseNameSegment(segment, null);
    }

    [Fact]
    public void ParseNameSegment_WithWhitespaceNames_TrimsCorrectly()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "  Parker  ",
                NameFirst_04 = "  Peter  ",
                NameMiddle_05 = "  Paul  ",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "444444444"
            }
        };

        // Act
        _parser.ParseNameSegment(segment, document);

        // Assert
        var patientReport = document.PatientReports[0];
        Assert.Equal("Parker", patientReport.LastName);
        Assert.Equal("Peter", patientReport.FirstName);
        Assert.Equal("Paul", patientReport.MiddleName);
        Assert.Equal("Peter Paul Parker", patientReport.PatientName);
    }

    [Fact]
    public void ParseNameSegment_WithEmptyStrings_HandlesCorrectly()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Rogers",
                NameFirst_04 = "",
                NameMiddle_05 = "",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = ""
            }
        };

        // Act
        _parser.ParseNameSegment(segment, document);

        // Assert
        var patientReport = document.PatientReports[0];
        Assert.Equal("Rogers", patientReport.PatientName);
        Assert.Empty(patientReport.EntityIdentifierCode);
    }

    [Fact]
    public void ParseNameSegment_WithEmptySegmentCollection_DoesNotThrow()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic[] segments = new dynamic[] { };

        // Act & Assert
        _parser.ParseNameSegment(segments, document);
        Assert.Empty(document.PatientReports);
    }

    [Fact]
    public void ParseNameSegment_WithCaseInsensitiveQualifier_CorrectlyIdentifiesMI()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Crawford",
                NameFirst_04 = "Chris",
                NameMiddle_05 = "",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "mi", // lowercase
                IdentificationCode_09 = "555666777"
            }
        };

        // Act
        _parser.ParseNameSegment(segment, document);

        // Assert
        var patientReport = document.PatientReports[0];
        Assert.Equal("555666777", patientReport.PatientId);
    }

    #endregion

    #region Integration with Document State

    [Fact]
    public void ParseNameSegment_SetsLastPatientReportAsDocumentPatientReport()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic[] segments = new dynamic[]
        {
            new { NM1 = new {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "First",
                NameFirst_04 = "Patient",
                NameMiddle_05 = "",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "111"
            }},
            new { NM1 = new {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Second",
                NameFirst_04 = "Patient",
                NameMiddle_05 = "",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "222"
            }}
        };

        // Act
        _parser.ParseNameSegment(segments, document);

        // Assert
        Assert.Equal(2, document.PatientReports.Count);
        Assert.NotNull(document.PatientReport);
        Assert.Equal("Patient Second", document.PatientReport.PatientName);
    }

    [Fact]
    public void ParseNameSegment_LogsInformationForEachPatient()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = "1",
                NameLastorOrganizationName_03 = "Thompson",
                NameFirst_04 = "Tom",
                NameMiddle_05 = "T",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = "999888777"
            }
        };

        // Act
        _parser.ParseNameSegment(segment, document);

        // Assert - Verify logging was called
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Parsed NM1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Type Conversion Tests

    [Fact]
    public void ParseNameSegment_WithNumericStrings_ConvertsCorrectly()
    {
        // Arrange
        var document = new EDI275Document();
        dynamic segment = new
        {
            NM1 = new
            {
                EntityIdentifierCode_01 = "IL",
                EntityTypeQualifier_02 = 1, // int instead of string
                NameLastorOrganizationName_03 = "Davis",
                NameFirst_04 = "Dan",
                NameMiddle_05 = "",
                NamePrefix_06 = "",
                NameSuffix_07 = "",
                IdentificationCodeQualifier_08 = "MI",
                IdentificationCode_09 = 123456 // int instead of string
            }
        };

        // Act
        _parser.ParseNameSegment(segment, document);

        // Assert
        var patientReport = document.PatientReports[0];
        Assert.Equal("1", patientReport.EntityTypeQualifier);
        Assert.Equal("123456", patientReport.IdentificationCode);
    }

    #endregion
}
