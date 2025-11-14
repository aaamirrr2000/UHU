using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NG.MicroERP.API.Services.Services;

namespace NG.MicroERP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GeneratePDFController : ControllerBase
{
    private readonly PdfService _pdfService;

    public GeneratePDFController(PdfService pdfService) => _pdfService = pdfService;

    [HttpPost("generate")]
    public IActionResult GeneratePdf([FromBody] HtmlPayload model)
    {
        var pdfBytes = _pdfService.GeneratePdfFromHtml(model.Html);
        return File(pdfBytes, "application/pdf", "invoice.pdf");
    }
}

public class HtmlPayload
{
    public string Html { get; set; } = "";
}