using MongoDB.Driver;

public class OrderHandler
{
    public void ApplyDiscount(string orderId, string couponCode)
    {
        var client = new MongoClient("mongodb://localhost:27017");
        var database = client.GetDatabase("orderdb");
        var orders = database.GetCollection<Order>("orders");
        
        var order = orders.Find(o => o.Id == orderId).FirstOrDefault();
        if (order == null)
        {
            Console.WriteLine("Order not found!");
            return;
        }

        var dayOfWeek = DateTime.Now.DayOfWeek;
        if (dayOfWeek == DayOfWeek.Monday && order.User.IsPremiumMember)
        {
            order.Discount = 0.1;
        }

        if (couponCode.StartsWith("SUMMER"))
        {
            foreach (var item in order.Items)
            {
                if (item.Product.Category == "Swimwear")
                {
                    item.Price -= item.Price * 0.5;
                }
            }
        }

        var coupons = database.GetCollection<Coupon>("coupons");
        var coupon = coupons.Find(c => c.Code == couponCode).FirstOrDefault();

        if (coupon != null && coupon.IsValid)
        {
            order.Discount += coupon.DiscountPercentage;
        }

        double discountedPrice = 0.0;
        foreach (var item in order.Items)
        {
            discountedPrice += item.Price * (1 - order.Discount);
        }
        order.TotalPrice = discountedPrice;

        Console.WriteLine("Order processed with discount");
        
        var filter = Builders<Order>.Filter.Eq("_id", order.Id);
        var update = Builders<Order>.Update.Set("TotalPrice", order.TotalPrice);
        orders.UpdateOne(filter, update);
    }
}

public class Order
{
    public string Id { get; set; }
    public User User { get; set; }
    public List<Item> Items { get; set; }
    public double TotalPrice { get; set; }
    public double Discount { get; set; }
}

public class User
{
    public bool IsPremiumMember { get; set; }
}

public class Item
{
    public Product Product { get; set; }
    public double Price { get; set; }
}

public class Product
{
    public string Category { get; set; }
}

public class Coupon
{
    public string Code { get; set; }
    public double DiscountPercentage { get; set; }
    public bool IsValid { get; set; }
}
