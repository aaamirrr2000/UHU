using DinkToPdf;
using DinkToPdf.Contracts;

namespace NG.MicroERP.API.Services.Services;

public class PdfService
{
    private readonly IConverter _converter;
    public PdfService(IConverter converter) => _converter = converter;

    public byte[] GeneratePdfFromHtml(string html)
    {
        var doc = new HtmlToPdfDocument
        {
            GlobalSettings = { PaperSize = PaperKind.A4 },
            Objects = {
                new ObjectSettings {
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
        };

        return _converter.Convert(doc);
    }
}