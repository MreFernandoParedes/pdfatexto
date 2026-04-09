using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PDFATEXTO.Web.Models;

public sealed class PdfUploadViewModel
{
    [Required(ErrorMessage = "Debe seleccionar un archivo PDF para continuar.")]
    public IFormFile? ArchivoPdf { get; set; }
}
