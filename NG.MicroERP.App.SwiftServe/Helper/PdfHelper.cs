using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using Syncfusion.Drawing;
using System.Reflection;
using System.ComponentModel.DataAnnotations;



using Microsoft.Maui.Graphics;

using System;
using System.Collections.Generic;

using System.Linq;

using System.Text;
using System.Threading.Tasks;
using NG.MicroERP.Shared.Helper;

namespace NG.MicroERP.App.SwiftServe.Helper;

public static class PdfHelper
{
    //public static async Task GeneratePdfFromList<T>(
    //  IEnumerable<T>? data,
    //  string title,
    //  string fileName,
    //  List<string>? includedProperties = null,
    //  Dictionary<string, string>? summaryTotals = null)
    //{
    //    using PdfDocument document = new PdfDocument();
    //    PdfPage page = document.Pages.Add();

    //    float yOffset = 0;

    //    // 🏢 Draw Company Info
    //    if (Globals.Organization != null)
    //    {
    //        PdfFont headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold);
    //        PdfBrush headerBrush = new PdfSolidBrush(Syncfusion.Drawing.Color.Black);

    //        string headerText = $"{Globals.Organization.Name ?? ""}, {Globals.Organization.Address ?? ""} - {Globals.Organization.Phone ?? ""}";
    //        page.Graphics.DrawString(headerText, headerFont, headerBrush, new Syncfusion.Drawing.PointF(0, yOffset));
    //        yOffset += 20;
    //    }

    //    // 📄 Draw Title
    //    PdfFont titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 14, PdfFontStyle.Bold);
    //    PdfBrush titleBrush = new PdfSolidBrush(Syncfusion.Drawing.Color.DarkBlue);
    //    page.Graphics.DrawString(title, titleFont, titleBrush, new Syncfusion.Drawing.PointF(0, yOffset));
    //    yOffset += 30;

    //    // 📊 Draw Table if data exists
    //    if (data != null && data.Any())
    //    {
    //        PdfGrid grid = new PdfGrid();
    //        grid.Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 11);
    //        grid.ApplyBuiltinStyle(PdfGridBuiltinStyle.ListTable4Accent1);

    //        var type = typeof(T);
    //        var allProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    //        var selectedProps = includedProperties != null && includedProperties.Any()
    //            ? allProps.Where(p => includedProperties.Contains(p.Name)).ToList()
    //            : allProps.ToList();

    //        var extraProps = includedProperties?
    //            .Where(p => selectedProps.All(sp => sp.Name != p))
    //            .ToList();

    //        grid.Columns.Add(selectedProps.Count + (extraProps?.Count ?? 0));
    //        grid.Headers.Add(1);
    //        PdfGridRow header = grid.Headers[0];

    //        int colIndex = 0;
    //        foreach (var prop in selectedProps)
    //        {
    //            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
    //            var name = displayAttr?.Name ?? prop.Name;
    //            header.Cells[colIndex++].Value = name;
    //        }

    //        if (extraProps != null)
    //        {
    //            foreach (var extra in extraProps)
    //            {
    //                header.Cells[colIndex++].Value = extra;
    //            }
    //        }

    //        foreach (var item in data)
    //        {
    //            PdfGridRow row = grid.Rows.Add();
    //            int i = 0;
    //            foreach (var prop in selectedProps)
    //            {
    //                var val = prop.GetValue(item);
    //                row.Cells[i++].Value = val?.ToString() ?? "";
    //            }

    //            if (extraProps != null)
    //            {
    //                foreach (var extra in extraProps)
    //                {
    //                    var extraValue = type.GetProperty(extra)?.GetValue(item)
    //                                   ?? type.GetMethod(extra)?.Invoke(item, null);
    //                    row.Cells[i++].Value = extraValue?.ToString() ?? "";
    //                }
    //            }
    //        }

    //        grid.Draw(page, new Syncfusion.Drawing.PointF(0, yOffset));
    //        yOffset += grid.Rows.Count * 20 + 60;
    //    }

    //    // 🧾 Draw Summary Totals
    //    if (summaryTotals != null && summaryTotals.Any())
    //    {
    //        PdfGrid summaryGrid = new PdfGrid();
    //        summaryGrid.Columns.Add(2); 
    //        summaryGrid.Headers.Add(1);
    //        summaryGrid.Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 11);
    //        summaryGrid.Headers[0].Cells[0].Value = "Desc.";
    //        summaryGrid.Headers[0].Cells[1].Value = "Detail";

    //        foreach (var kv in summaryTotals)
    //        {
    //            PdfGridRow row = summaryGrid.Rows.Add();
    //            row.Cells[0].Value = kv.Key;
    //            row.Cells[1].Value = kv.Value;
    //        }

    //        summaryGrid.Draw(page, new Syncfusion.Drawing.PointF(0, yOffset));
    //    }

    //    // 💾 Save & Launch PDF
    //    using MemoryStream stream = new MemoryStream();
    //    document.Save(stream);
    //    stream.Position = 0;

    //    var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
    //    using (var fileStream = File.Create(filePath))
    //        stream.CopyTo(fileStream);

    //    await Task.Delay(200);
    //    await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(filePath) });
    //}

    public static async Task GenerateBill()
    {
        using PdfDocument document = new PdfDocument();
        PdfPage page = document.Pages.Add();

        float yOffset = 0;

        // 🏢 Draw Company Info
        if (Globals.Organization != null)
        {
            PdfFont headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold);
            PdfBrush headerBrush = new PdfSolidBrush(Syncfusion.Drawing.Color.Black);

            string headerText = $"{Globals.Organization.Name ?? ""}, {Globals.Organization.Address ?? ""} - {Globals.Organization.Phone ?? ""}";
            page.Graphics.DrawString(headerText, headerFont, headerBrush, new Syncfusion.Drawing.PointF(0, yOffset));
            yOffset += 20;
        }

   

        // 💾 Save & Launch PDF
        using MemoryStream stream = new MemoryStream();
        document.Save(stream);
        stream.Position = 0;

        var filePath = Path.Combine(FileSystem.AppDataDirectory, "test");
        using (var fileStream = File.Create(filePath))
            stream.CopyTo(fileStream);

        await Task.Delay(200);
        await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(filePath) });
    }
}
