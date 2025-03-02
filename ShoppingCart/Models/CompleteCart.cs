public class CompleteCart
{
    public int CartId { get; set; }
    public string UserName { get; set; }
    public Coupon Coupon { get; set; }
    public List<Product> Products { get; set; } = new List<Product>();
    public double Subtotal { get; set; }
    public double Total { get; set; }
}