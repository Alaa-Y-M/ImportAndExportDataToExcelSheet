using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Task.UI.Common.Interfaces;
using Task.UI.Data;

namespace Task.UI.Controllers;
public class ProductController : Controller
{
    private readonly IUnitOfWork unitOfWork;

    public ProductController(IUnitOfWork _unitOfWork)
    {
        unitOfWork = _unitOfWork;
    }
    public IActionResult Index()
    {
        var products = unitOfWork.CiscoPSSProducts.GetAll();
        return View(products);
    }
    public IActionResult ExportToExcel()
    {
        // Getting the information from our mimic db
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
    [HttpGet]
    public IActionResult BatchUpload()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BatchUpload(IFormFile batchproducts)
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
        // convert to a stream
        var stream = batchproducts?.OpenReadStream();
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
                        var band = Int32.Parse(worksheet.Cells[row, 1].Value?.ToString() ?? null!);
                        var categoryCode = worksheet.Cells[row, 2].Value?.ToString();
                        var manufacturer = worksheet.Cells[row, 3].Value?.ToString();
                        var itemDescription = worksheet.Cells[row, 4].Value?.ToString();
                        var partSKU = worksheet.Cells[row, 5].Value?.ToString();
                        var listPrice = Double.Parse(worksheet.Cells[row, 6].Value?.ToString()!);
                        var minDiscount = Double.Parse(worksheet.Cells[row, 7].Value?.ToString()!);
                        var discountPrice = Double.Parse(worksheet.Cells[row, 8].Value?.ToString()!);


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
                        Console.WriteLine(product.Band + " " + product.DiscountPrice + " " + product.PartSKU);
                        // await unitOfWork.CiscoPSSProducts.AddOneAsync(product);
                        // await unitOfWork.CompleteAsync();
                        newProds.Add(product);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            var products1 = new List<CiscoPSSProducts>();
            foreach (var product in products)
                newProds.Add(product);

            for (int i = 0; i < newProds.Count; i++)
            {
                products1.Add(unitOfWork.CiscoPSSProducts.AddOne(newProds[i]));
            }

            Console.WriteLine(products1.Count);
            //unitOfWork.Complete();
            // var pSSProducts = unitOfWork.CiscoPSSProducts.GetAll();
            //Console.WriteLine(pSSProducts.Count());
            return View("Index", products1);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return View(ex.Message);
        }

    }
    // [HttpGet("{searchWord}")]
    // public async Task<IActionResult> Search(string searchWord)
    // {
    //     if (!string.IsNullOrEmpty(searchWord))
    //         return View(ModelState);

    //     var query = await unitOfWork.CiscoPSSProducts.FindAllAsync(p => p.PartSKU.Contains(searchWord));
    //     if (query is null)
    //         return View(ModelState);
    //     return View("index", query);
    // }
}