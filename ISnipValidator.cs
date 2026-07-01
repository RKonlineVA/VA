csharp src\EDI275AttachmentParser\Services\ISnipValidator.cs
namespace EDI275AttachmentParser.Services
{
    public interface ISnipValidator
    {
        bool IsValid(string? snip);
        string Normalize(string? snip);
    }
}