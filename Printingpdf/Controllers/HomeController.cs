using Microsoft.AspNetCore.Mvc;
using Printingpdf.Models;
using System;
using System.Drawing.Printing;
using System.IO;
using Pdfium.Net;
using Pdfium.NET;

namespace Printingpdf.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadFile(UploadFile model)
        {
            try
            {
                if (model.File != null && model.File.Length > 0)
                {
                    string defaultPrinterName = GetDefaultPrinter();

                    bool status = PrintPDF(defaultPrinterName, model.File);

                    if (status)
                    {
                        return Content($"File printed: {model.File.FileName}");
                    }
                    else
                    {
                        return Content("Failed to print the file.");
                    }
                }

                return Content("No file selected");
            }
            catch (Exception ex)
            {
                // Handle exceptions and log them
                _logger.LogError($"Error during file upload and printing: {ex.Message}");
                return Content("An error occurred during file upload and printing.");
            }
        }

        private bool PrintPDF(string printerName, IFormFile file)
        {
            try
            {
                using (PrintDocument printDocument = new PrintDocument())
                {
                    printDocument.PrinterSettings.PrinterName = printerName;
                    printDocument.PrintPage += (sender, e) =>
                    {
                        using (Stream fileStream = file.OpenReadStream())
                        {
                            using (PdfDocument document = PdfDocument.Load(fileStream))
                            {
                                int pageNumber = e.PageSettings.PrinterSettings.FromPage;
                                using (var pdfRenderer = new PdfRenderer(document))
                                {
                                    var image = pdfRenderer.Render(pageNumber, 96, 96, true);
                                    e.Graphics.DrawImage(image, e.MarginBounds);
                                }
                            }
                        }
                    };

                    printDocument.Print();

                    return true;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and log them
                _logger.LogError($"Error during PDF printing: {ex.Message}");
                return false;
            }
        }

        private static string GetDefaultPrinter()
        {
            PrinterSettings settings = new PrinterSettings();
            string defaultPrinter = null;

            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                settings.PrinterName = printer;
                if (settings.IsDefaultPrinter)
                {
                    defaultPrinter = printer;
                    break;
                }
            }

            return defaultPrinter;
        }
    }
}
