using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Task.UI.Common.Interfaces;
using Task.UI.Data;
using ReflectionIT.Mvc.Paging;

namespace Task.UI.Controllers;
public class ProductController : Controller
{
    private readonly IUnitOfWork unitOfWork;

    public ProductController(IUnitOfWork _unitOfWork)
    {
        unitOfWork = _unitOfWork;
    }

    [Obsolete]
    public IActionResult Index(int page = 1, string? search = "")
    {
        var products = unitOfWork.CiscoPSSProducts.GetAll();
        if (!string.IsNullOrEmpty(search))
            products = products.Where(p => p.ItemDescription.Contains(search) ||
            p.Manufacturer.ToLower().Contains(search) || p.CategoryCode.ToLower().Contains(search)).ToList();
        var query = PagingList.Create<CiscoPSSProducts>(products, 10, page);
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
    public async Task<IActionResult> BatchUpload(IFormFile batchproducts)
    {
        if (!ModelState.IsValid)
        {
            return View(ModelState);
        }
        if (batchproducts?.Length <= 0)
        {
            ModelState.AddModelError("Length", "length is zero");
            return View(ModelState);
        }
        var result = await ImportData(batchproducts!);
        return result;
    }
    private async Task<IActionResult> ImportData(IFormFile file)
    {
        var stream = file?.OpenReadStream();
        var products = unitOfWork.CiscoPSSProducts.GetAll();
        var newProds = new List<CiscoPSSProducts>();
        try
        {
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.First();
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


                        var product = new CiscoPSSProducts
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

                        newProds.Add(product);
                        Console.WriteLine("Count is : " + newProds.Count);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            foreach (var product in products)
                newProds.Add(product);

            var products1 = await unitOfWork.CiscoPSSProducts.AddAllAsync(newProds);
            Console.WriteLine(products1.Count());
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
        var products = unitOfWork.CiscoPSSProducts.GetAll();

        if (products is null)
            return View("Index");
        // Start exporting to Excel
        var stream = new MemoryStream();

        using (ExcelPackage xlPackage = new ExcelPackage(stream))
        {
            // Define a worksheet
            var worksheet = xlPackage.Workbook.Worksheets.Add("products");

            // Styling
            var customStyle = xlPackage.Workbook.Styles.CreateNamedStyle("CustomStyle");
            customStyle.Style.Font.UnderLine = true;
            customStyle.Style.Font.Color.SetColor(Color.Red);

            // First row
            var startRow = 5;
            var row = startRow;

            worksheet.Cells["A1"].Value = "Cisco Product Export";
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
            foreach (var product in products)
            {
                worksheet.Cells[row, 1].Value = product.Band;
                worksheet.Cells[row, 2].Value = product.CategoryCode;
                worksheet.Cells[row, 3].Value = product.Manufacturer;
                worksheet.Cells[row, 4].Value = product.ItemDescription;
                worksheet.Cells[row, 5].Value = product.PartSKU;
                worksheet.Cells[row, 6].Value = product.ListPrice;
                worksheet.Cells[row, 7].Value = product.MinDiscount;
                worksheet.Cells[row, 8].Value = product.DiscountPrice;

                row++; // row = row + 1;
            }

            xlPackage.Workbook.Properties.Title = "Product list";
            xlPackage.Workbook.Properties.Author = "Alaa";

            xlPackage.Save();
        }

        stream.Position = 0;
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products.xlsx", true);
    }
}