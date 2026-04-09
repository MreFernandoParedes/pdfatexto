using Microsoft.AspNetCore.Mvc;
using PDFATEXTO.Web.Models;
using PDFATEXTO.Web.Services;

namespace PDFATEXTO.Web.Controllers;

public sealed class HomeController : Controller
{
    private const string ExtractedTextSessionKey = "ExtractedText";
    private const string ExtractedFileNameSessionKey = "ExtractedFileName";

    private readonly IPdfExtractionService _pdfExtractionService;

    public HomeController(IPdfExtractionService pdfExtractionService)
    {
        _pdfExtractionService = pdfExtractionService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new PdfResultViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(PdfResultViewModel model, CancellationToken cancellationToken)
    {
        if (model.Upload.ArchivoPdf is null)
        {
            model.Estado = "No se pudo procesar el documento";
            model.ErrorMessage = "Debe seleccionar un archivo PDF para continuar.";
            return View(model);
        }

        if (!string.Equals(model.Upload.ArchivoPdf.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) &&
            !model.Upload.ArchivoPdf.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            model.Estado = "No se pudo procesar el documento";
            model.ErrorMessage = "Solo se permite cargar archivos en formato PDF.";
            return View(model);
        }

        var result = await _pdfExtractionService.ProcessAsync(model.Upload.ArchivoPdf, cancellationToken);

        model.NombreArchivo = model.Upload.ArchivoPdf.FileName;
        model.Estado = result.Status;
        model.ContenidoExtraido = result.Content;
        model.ErrorMessage = result.ErrorMessage;

        if (!string.IsNullOrWhiteSpace(result.Content))
        {
            HttpContext.Session.SetString(ExtractedTextSessionKey, result.Content);
            HttpContext.Session.SetString(ExtractedFileNameSessionKey, model.DownloadFileName);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Download()
    {
        var extractedText = HttpContext.Session.GetString(ExtractedTextSessionKey);
        if (string.IsNullOrWhiteSpace(extractedText))
        {
            return RedirectToAction(nameof(Index));
        }

        var fileName = HttpContext.Session.GetString(ExtractedFileNameSessionKey);
        var downloadName = string.IsNullOrWhiteSpace(fileName) ? "texto_extraido.txt" : fileName;

        return File(Encoding.UTF8.GetBytes(extractedText), "text/plain; charset=utf-8", downloadName);
    }

    [HttpGet]
    public IActionResult Error()
    {
        return View();
    }
}
