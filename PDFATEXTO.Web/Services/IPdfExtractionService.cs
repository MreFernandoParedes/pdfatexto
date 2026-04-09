using Microsoft.AspNetCore.Http;
using PDFATEXTO.Web.Models;

namespace PDFATEXTO.Web.Services;

public interface IPdfExtractionService
{
    Task<PdfExtractionResponse> ProcessAsync(IFormFile pdfFile, CancellationToken cancellationToken);
}
