namespace PDFATEXTO.Web.Models;

public sealed class PdfExtractionResponse
{
    public bool Success { get; set; }

    public string Status { get; set; } = "Listo";

    public string Content { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}
