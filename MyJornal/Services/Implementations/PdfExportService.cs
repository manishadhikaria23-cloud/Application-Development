using JournalApp.Models.Entities;
using JournalApp.Services.Interfaces;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace JournalApp.Services.Implementations
{
    public class PdfExportService : IPdfExportService
    {
        public Task<string> ExportAsync(List<JournalEntry> entries, string fileName)
        {
            var safeName = string.IsNullOrWhiteSpace(fileName) ? "journals.pdf" : fileName;
            if (!safeName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                safeName += ".pdf";

            var path = Path.Combine(FileSystem.AppDataDirectory, safeName);

            var doc = new PdfDocument();

            var titleFont = new XFont("Verdana", 16, XFontStyle.Bold);
            var hFont = new XFont("Verdana", 11, XFontStyle.Bold);
            var pFont = new XFont("Verdana", 10, XFontStyle.Regular);

            foreach (var e in entries.OrderBy(x => x.EntryDate))
            {
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                double y = 30;

                gfx.DrawString($"Journal Entry - {e.EntryDate:yyyy-MM-dd}", titleFont, XBrushes.Black, new XPoint(40, y));
                y += 30;

                gfx.DrawString("Title:", hFont, XBrushes.Black, new XPoint(40, y));
                gfx.DrawString(e.Title ?? "", pFont, XBrushes.Black, new XPoint(110, y));
                y += 18;

                gfx.DrawString("Category:", hFont, XBrushes.Black, new XPoint(40, y));
                gfx.DrawString(e.Category ?? "", pFont, XBrushes.Black, new XPoint(110, y));
                y += 18;

                gfx.DrawString("Primary Mood:", hFont, XBrushes.Black, new XPoint(40, y));
                gfx.DrawString(e.PrimaryMood.ToString(), pFont, XBrushes.Black, new XPoint(140, y));
                y += 18;

                gfx.DrawString("Secondary:", hFont, XBrushes.Black, new XPoint(40, y));
                gfx.DrawString(e.SecondaryMoods ?? "", pFont, XBrushes.Black, new XPoint(120, y));
                y += 18;

                gfx.DrawString("Tags:", hFont, XBrushes.Black, new XPoint(40, y));
                gfx.DrawString(e.Tags ?? "", pFont, XBrushes.Black, new XPoint(110, y));
                y += 24;

                gfx.DrawString("Content:", hFont, XBrushes.Black, new XPoint(40, y));
                y += 16;

                // basic text wrapping
                var content = e.Content ?? "";
                var lines = SplitLines(content, 90);

                foreach (var line in lines)
                {
                    if (y > page.Height - 40)
                    {
                        page = doc.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        y = 40;
                    }

                    gfx.DrawString(line, pFont, XBrushes.Black, new XPoint(40, y));
                    y += 14;
                }

                y += 10;
                gfx.DrawString($"Created: {e.CreatedAt}   Updated: {e.UpdatedAt}", pFont, XBrushes.Black, new XPoint(40, page.Height - 25));
            }

            using (var stream = File.Create(path))
            {
                doc.Save(stream);
            }

            return Task.FromResult(path);
        }

        private static List<string> SplitLines(string text, int maxChars)
        {
            var result = new List<string>();
            foreach (var raw in text.Replace("\r\n", "\n").Split('\n'))
            {
                var line = raw;
                while (line.Length > maxChars)
                {
                    result.Add(line.Substring(0, maxChars));
                    line = line.Substring(maxChars);
                }
                result.Add(line);
            }
            return result;
        }
    }
}
