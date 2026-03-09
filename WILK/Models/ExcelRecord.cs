namespace WILK.Models
{
    /// <summary>
    /// Represents a single row from Excel import with parsed helpers
    /// </summary>
    public class ExcelRow
    {
        public string? Name { get; set; }
        public string? Id { get; set; }
        public string? Quantity { get; set; }
        // optional parsed helpers
        public int? RId => int.TryParse(Id, out var v) ? v : (int?)null;
        public int QuantityInt => int.TryParse(Quantity, out var q) ? q : 0;
    }
}
