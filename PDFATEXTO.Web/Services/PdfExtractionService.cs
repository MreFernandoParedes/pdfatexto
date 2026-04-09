using Microsoft.AspNetCore.Http;
using PDFATEXTO.Web.Models;

namespace PDFATEXTO.Web.Services;

public sealed class PdfExtractionService : IPdfExtractionService
{
    private readonly IClaudeService _claudeService;

    public PdfExtractionService(IClaudeService claudeService)
    {
        _claudeService = claudeService;
    }

    public async Task<PdfExtractionResponse> ProcessAsync(IFormFile pdfFile, CancellationToken cancellationToken)
    {
        if (pdfFile.Length == 0)
        {
            return new PdfExtractionResponse
            {
                Success = false,
                Status = "No se pudo procesar el documento",
                ErrorMessage = "No se encontró contenido procesable en el archivo enviado."
            };
        }

        await using var stream = pdfFile.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);

        var content = await _claudeService.ExtractTextAsync(memoryStream.ToArray(), pdfFile.FileName, cancellationToken);

        return new PdfExtractionResponse
        {
            Success = true,
            Status = "Listo",
            Content = content
        };
    }
}
