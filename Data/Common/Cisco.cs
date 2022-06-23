using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task.UI.Data.Common;
public class Cisco
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Band { get; set; }
    [Display(Name = "Category code")]
    public string CategoryCode { get; set; } = null!;
    public string Manufacturer { get; set; } = null!;
    [Display(Name = "Part SKU")]
    public string PartSKU { get; set; } = null!;
    [Display(Name = "Item description")]
    public string ItemDescription { get; set; } = null!;
    [Display(Name = "List price"), DataType(DataType.Currency)]
    public double ListPrice { get; set; }
    [Display(Name = "Min Discount")]
    public double MinDiscount { get; set; }
    [Display(Name = "Discount price"), DataType(DataType.Currency)]
    public double DiscountPrice { get; set; }


}