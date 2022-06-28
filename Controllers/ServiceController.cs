using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Task.UI.Common.Interfaces;
using Task.UI.Data;
using ReflectionIT.Mvc.Paging;

namespace Task.UI.Controllers;
public class ServiceController : Controller
{
    private readonly IUnitOfWork unitOfWork;

    public ServiceController(IUnitOfWork _unitOfWork)
    {
        unitOfWork = _unitOfWork;
    }

    [Obsolete]
    public IActionResult Index(int page = 1, string? search = "")
    {
        var services = unitOfWork.CiscoPSSServices.GetAll();
        if (!string.IsNullOrEmpty(search))
            services = services.Where(p => p.ItemDescription.Contains(search) ||
            p.Manufacturer.ToLower().Contains(search) || p.CategoryCode.ToLower().Contains(search)).ToList();
        var query = PagingList.Create<CiscoPSSServices>(services, 10, page);
        return View(query);
    }
    public IActionResult ExportToExcel()
    {
        // Getting the information from our mimic db
        var result = ExportData();
        return result;
    }
    [HttpGet]
    public IActionResult BatchUpload()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = 409715200)]
    [RequestSizeLimit(409715200)]
    public async Task<IActionResult> BatchUpload(IFormFile batchservices, [FromServices] IWebHostEnvironment webHost)
    {
        if (!ModelState.IsValid)
        {
            return View(ModelState);
        }
        if (batchservices?.Length <= 0)
        {
            ModelState.AddModelError("Length", "length is zero");
            return View(ModelState);
        }
        string path = $"{webHost.WebRootPath}\\Uploads";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        string fileName = $"{path}\\{batchservices?.FileName}";
        if (!System.IO.File.Exists(fileName))
        {
            using (FileStream stream = System.IO.File.Create(fileName))
            {
                await batchservices!.CopyToAsync(stream);
                var result = await ImportData(batchservices!);
                await stream.FlushAsync();
                return result;
            }
        }
        using (FileStream stream = System.IO.File.OpenRead(fileName))
        {
            var result = await ImportData(batchservices!);
            await stream.FlushAsync();
            return result;
        }
    }
    private async Task<IActionResult> ImportData(IFormFile file)
    {
        var stream = file?.OpenReadStream();
        var services = unitOfWork.CiscoPSSServices.GetAll();
        var newServe = new List<CiscoPSSServices>();
        try
        {
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[1];
                var rowCount = worksheet.Dimension.Rows;

                for (var row = 4; row <= rowCount; row++)
                {
                    try
                    {
                        var band = 0;
                        var categoryCode = worksheet.Cells[row, 2].Value?.ToString();
                        var manufacturer = worksheet.Cells[row, 3].Value?.ToString();
                        var itemDescription = worksheet.Cells[row, 4].Value?.ToString();
                        var partSKU = worksheet.Cells[row, 5].Value?.ToString();
                        var listPrice = Decimal.Parse(worksheet.Cells[row, 6].Value?.ToString()!);
                        var minDiscount = Decimal.Parse(worksheet.Cells[row, 7].Value?.ToString()!);
                        var discountPrice = Decimal.Parse(worksheet.Cells[row, 8].Value?.ToString()!);


                        var service = new CiscoPSSServices
                        {
                            Band = band,
                            CategoryCode = categoryCode!,
                            Manufacturer = manufacturer!,
                            ItemDescription = itemDescription!,
                            PartSKU = partSKU!,
                            ListPrice = listPrice,
                            MinDiscount = minDiscount,
                            DiscountPrice = discountPrice
                        };

                        newServe.Add(service);
                        Console.WriteLine("Count is : " + newServe.Count);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            foreach (var service in services)
                newServe.Add(service);

            var services1 = await unitOfWork.CiscoPSSServices.AddOrUpdateAllAsync(newServe);
            Console.WriteLine(services1.Count());
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return View(ex.Message);
        }
    }
    private IActionResult ExportData()
    {
        var services = unitOfWork.CiscoPSSServices.GetAll();

        if (services is null)
            return View("Index");
        // Start exporting to Excel
        var stream = new MemoryStream();

        using (ExcelPackage xlPackage = new ExcelPackage(stream))
        {
            // Define a worksheet
            var worksheet = xlPackage.Workbook.Worksheets.Add("services");

            // Styling
            var customStyle = xlPackage.Workbook.Styles.CreateNamedStyle("CustomStyle");
            customStyle.Style.Font.UnderLine = true;
            customStyle.Style.Font.Color.SetColor(Color.Red);

            // First row
            var startRow = 5;
            var row = startRow;

            worksheet.Cells["A1"].Value = "Cisco Service Export";
            worksheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.Azure);
            worksheet.Cells["A1"].Style.Font.Bold = true;
            using (var r = worksheet.Cells["A1:H1"])
            {
                r.Merge = true;
                r.Style.Font.Color.SetColor(Color.Green);
                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(23, 55, 93));
            }

            worksheet.Cells["A4"].Value = "Band";
            worksheet.Cells["B4"].Value = "CategoryCode";
            worksheet.Cells["C4"].Value = "Manufacturer";
            worksheet.Cells["D4"].Value = "ItemDescription";
            worksheet.Cells["E4"].Value = "PartSKU";
            worksheet.Cells["F4"].Value = "ListPrice";
            worksheet.Cells["G4"].Value = "MinDiscount";
            worksheet.Cells["H4"].Value = "DiscountPrice";
            worksheet.Cells["A4:H4"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells["A4:H4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous;
            worksheet.Cells["A4:H4"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            worksheet.Cells["A4:H4"].Style.Fill.BackgroundColor.SetColor(Color.SkyBlue);

            row = 5;
            foreach (var service in services)
            {
                worksheet.Cells[row, 1].Value = service.Band;
                worksheet.Cells[row, 2].Value = service.CategoryCode;
                worksheet.Cells[row, 3].Value = service.Manufacturer;
                worksheet.Cells[row, 4].Value = service.ItemDescription;
                worksheet.Cells[row, 5].Value = service.PartSKU;
                worksheet.Cells[row, 6].Value = service.ListPrice;
                worksheet.Cells[row, 7].Value = service.MinDiscount;
                worksheet.Cells[row, 8].Value = service.DiscountPrice;

                row++; // row = row + 1;
            }

            xlPackage.Workbook.Properties.Title = "Services list";
            xlPackage.Workbook.Properties.Author = "Alaa";

            xlPackage.Save();
        }

        stream.Position = 0;
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "services.xlsx", true);
    }
}