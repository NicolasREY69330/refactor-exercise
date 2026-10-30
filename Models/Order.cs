public class Order
{
    public string Id { get; set; } = "";
    public User User { get; set; } = new();
    public List<Item> Items { get; set; } = new();
    public double TotalPrice { get; set; }
    public double Discount { get; set; }
}

public class User
{
    public bool IsPremiumMember { get; set; }
}

public class Item
{
    public Product Product { get; set; } = new();
    public double Price { get; set; }
}

public class Product
{
    public string Category { get; set; } = "";
}

public class Coupon
{
    public string Code { get; set; } = "";
    public double DiscountPercentage { get; set; }
    public bool IsValid { get; set; }
}
