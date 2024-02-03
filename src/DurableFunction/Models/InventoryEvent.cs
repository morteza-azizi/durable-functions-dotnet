namespace DurableFunction.Models
{
    public class InventoryEvent
    {
        public string TransactionNumber { get; set; } = default!;
        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public List<string> Items = new();
    }
}
