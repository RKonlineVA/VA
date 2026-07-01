using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EDI275AttachmentParser.Data
{
    public class Edi275DbContextFactory : IDesignTimeDbContextFactory<Edi275DbContext>
    {
        public Edi275DbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<Edi275DbContext>();

            var connectionString =
                Environment.GetEnvironmentVariable("EDI275_DB_CONNECTION")
                ?? "Server=localhost;Database=X12EDI75IngestionDb;Trusted_Connection=True;TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString);

            return new Edi275DbContext(optionsBuilder.Options);
        }
    }
}
