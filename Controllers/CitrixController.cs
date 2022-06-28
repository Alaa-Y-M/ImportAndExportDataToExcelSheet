using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Task.UI.Common.Interfaces;
using Task.UI.Data;
using ReflectionIT.Mvc.Paging;

namespace Task.UI.Controllers;
public class CitrixController : Controller
{
    private readonly IUnitOfWork unitOfWork;

    public CitrixController(IUnitOfWork _unitOfWork)
    {
        unitOfWork = _unitOfWork;
    }

    [Obsolete]
    public IActionResult Index(int page = 1, string? search = "")
    {
        var citrix = unitOfWork.Citrix3PPSS.GetAll();
        if (!string.IsNullOrEmpty(search))
            citrix = citrix.Where(p => p.ItemDescription.Contains(search) ||
            p.Manufacturer.ToLower().Contains(search) || p.CategoryCode.ToLower().Contains(search)).ToList();
        var query = PagingList.Create<Citrix3PPSS>(citrix, 10, page);
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
    public async Task<IActionResult> BatchUpload(IFormFile batchcitrix, [FromServices] IWebHostEnvironment webHost)
    {
        if (!ModelState.IsValid)
        {
            return View(ModelState);
        }
        if (batchcitrix?.Length <= 0)
        {
            ModelState.AddModelError("Length", "length is zero");
            return View(ModelState);
        }
        string path = $"{webHost.WebRootPath}\\Uploads";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        string fileName = $"{path}\\{batchcitrix?.FileName}";
        if (!System.IO.File.Exists(fileName))
        {
            using (FileStream stream = System.IO.File.Create(fileName))
            {
                await batchcitrix!.CopyToAsync(stream);
                var result = await ImportData(batchcitrix!);
                await stream.FlushAsync();
                return result;
            }
        }
        using (FileStream stream = System.IO.File.OpenRead(fileName))
        {
            var result = await ImportData(batchcitrix!);
            await stream.FlushAsync();
            return result;
        }
    }
    private async Task<IActionResult> ImportData(IFormFile file)
    {
        var stream = file?.OpenReadStream();
        var citrixs = unitOfWork.Citrix3PPSS.GetAll();
        var newProds = new List<Citrix3PPSS>();
        try
        {
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[2];
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


                        var citrix = new Citrix3PPSS
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

                        newProds.Add(citrix);
                        Console.WriteLine("Count is : " + newProds.Count);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            foreach (var citrix in citrixs)
                newProds.Add(citrix);

            var citrix1 = await unitOfWork.Citrix3PPSS.AddOrUpdateAllAsync(newProds);
            Console.WriteLine(citrix1.Count());
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
        var citrixs = unitOfWork.Citrix3PPSS.GetAll();

        if (citrixs is null)
            return View("Index");
        // Start exporting to Excel
        var stream = new MemoryStream();

        using (ExcelPackage xlPackage = new ExcelPackage(stream))
        {
            // Define a worksheet
            var worksheet = xlPackage.Workbook.Worksheets.Add("citrix");

            // Styling
            var customStyle = xlPackage.Workbook.Styles.CreateNamedStyle("CustomStyle");
            customStyle.Style.Font.UnderLine = true;
            customStyle.Style.Font.Color.SetColor(Color.Red);

            // First row
            var startRow = 5;
            var row = startRow;

            worksheet.Cells["A1"].Value = "Cisco Citrix Export";
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
            foreach (var citrix in citrixs)
            {
                worksheet.Cells[row, 1].Value = citrix.Band;
                worksheet.Cells[row, 2].Value = citrix.CategoryCode;
                worksheet.Cells[row, 3].Value = citrix.Manufacturer;
                worksheet.Cells[row, 4].Value = citrix.ItemDescription;
                worksheet.Cells[row, 5].Value = citrix.PartSKU;
                worksheet.Cells[row, 6].Value = citrix.ListPrice;
                worksheet.Cells[row, 7].Value = citrix.MinDiscount;
                worksheet.Cells[row, 8].Value = citrix.DiscountPrice;

                row++; // row = row + 1;
            }

            xlPackage.Workbook.Properties.Title = "Citrix list";
            xlPackage.Workbook.Properties.Author = "Alaa";

            xlPackage.Save();
        }

        stream.Position = 0;
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "citrix.xlsx", true);
    }
}