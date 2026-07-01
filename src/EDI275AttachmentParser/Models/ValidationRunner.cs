// C#
public class ValidationRunner
{
    private readonly Edi275DbContext _db;

    public ValidationRunner(Edi275DbContext db) => _db = db;

    /// <summary>
    /// Run all registered validators and persist results to Edi275ParseLogs linked to the importId.
    /// </summary>
    public void RunAndPersist(IEnumerable<IValidator<Edi275Document>> validators, Edi275Document doc, int importId)
    {
        var now = DateTime.UtcNow;
        var results = validators.SelectMany(v => SafeInvoke(v, doc)).ToList();

        foreach (var r in results)
        {
            var log = new Edi275ParseLogEntity
            {
                Edi275ImportEntityId = importId,
                LoggedAtUtc = now,
                Level = r.Level.ToString(),
                Message = r.Message,
                Exception = r.Exception
            };
            _db.Edi275ParseLogs.Add(log);
        }

        _db.SaveChanges();
    }

    private IEnumerable<ValidationResult> SafeInvoke(IValidator<Edi275Document> v, Edi275Document d)
    {
        try
        {
            return v.Validate(d) ?? Enumerable.Empty<ValidationResult>();
        }
        catch (Exception ex)
        {
            return new[] { new ValidationResult(ValidationLevel.Error, $"Validator {v.GetType().Name} threw: {ex.Message}", ex.ToString()) };
        }
    }
}