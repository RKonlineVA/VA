using EDI275AttachmentParser.Data; // <-- added
// C#
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Linq;

namespace EDI275AttachmentParser.Tests
{
    public class ValidationRunnerTests
    {
        private Edi275DbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<Edi275DbContext>()
                //.UseInMemoryDatabase(dbName)
                .Options;
            return new Edi275DbContext(options);
        }

        //private class TestValidator : IValidator<Edi275Document>
        //{
        //    public IEnumerable<ValidationResult> Validate(Edi275Document target)
        //    {
        //        yield return new ValidationResult(ValidationLevel.Info, "Info result");
        //        yield return new ValidationResult(ValidationLevel.Error, "Error result");
        //    }
        //}

        [Fact]
        public async Task RunAndPersistAsync_PersistsLogsToDatabase()
        {
            var dbName = $"Edi275Test_{System.Guid.NewGuid()}";
            await using var db = CreateInMemoryContext(dbName);

            // Ensure DB created
            await db.Database.EnsureCreatedAsync();

           // var runner = new ValidationRunner(db);

            //var validators = new[] { new TestValidator() };
            //var doc = new Edi275Document
            //{
            //    InterchangeControlNumber = "ICN-TEST",
            //    Attachments = new System.Collections.Generic.List<Attachment>(),
            //    PatientReports = new System.Collections.Generic.List<Edi275PatientReport>()
            //};

            var importId = 123;

            //await runner.RunAndPersistAsync(validators, doc, importId);

            var logs = await db.Edi275ParseLogs.Where(l => l.Edi275ImportEntityId == importId).ToListAsync();

            Assert.Equal(2, logs.Count);
            Assert.Contains(logs, l => l.Message.Contains("Info result") && l.Level == "Info");
            Assert.Contains(logs, l => l.Message.Contains("Error result") && l.Level == "Error");
        }
    }
}