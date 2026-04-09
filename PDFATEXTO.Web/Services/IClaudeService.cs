namespace PDFATEXTO.Web.Services;

public interface IClaudeService
{
    Task<string> ExtractTextAsync(byte[] pdfBytes, string fileName, CancellationToken cancellationToken);
}
