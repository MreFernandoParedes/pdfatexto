using Microsoft.AspNetCore.Mvc;
using PDFATEXTO.Web.Models;
using PDFATEXTO.Web.Services;

namespace PDFATEXTO.Web.Controllers;

public sealed class HomeController : Controller
{
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

        return View(model);
    }

    [HttpGet]
    public IActionResult Error()
    {
        return View();
    }
}
