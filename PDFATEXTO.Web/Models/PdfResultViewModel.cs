namespace PDFATEXTO.Web.Models;

public sealed class PdfResultViewModel
{
    public PdfUploadViewModel Upload { get; set; } = new();

    public string? NombreArchivo { get; set; }

    public string? ContenidoExtraido { get; set; }

    public string Estado { get; set; } = "Listo";

    public string? ErrorMessage { get; set; }

    public bool TieneResultado => !string.IsNullOrWhiteSpace(ContenidoExtraido);
}
