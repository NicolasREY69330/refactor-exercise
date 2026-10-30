using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

public class OrderTests
{
    public OrderTests()
    {
        ServiceLocator.Services.Clear();
        OrderCache.Orders.Clear();
        DiscountCalculator.PremiumDiscount = 0.1;
    }

    [Theory]
    [InlineData(false, true, 90)]
    [InlineData(true, true, 80)]
    [InlineData(false, false, 100)]
    [InlineData(true, false, 90)]
    public void Calculate_applies_member_and_coupon_discounts(bool premium, bool valid, double expected)
    {
        var order = CreateOrder(premium);
        var coupon = new Coupon { Code = "SAVE10", DiscountPercentage = 0.1, IsValid = valid };

        var total = DiscountCalculator.Calculate(order, coupon);

        Assert.Equal(expected, total, 2);
    }

    [Fact]
    public void Calculate_returns_a_positive_number()
    {
        var total = DiscountCalculator.Calculate(CreateOrder(), new Coupon { IsValid = true });

        Assert.True(total > 0);
    }

    [Fact]
    public void Order_can_store_its_id()
    {
        var order = new Order { Id = "test-order" };

        Assert.Equal("test-order", order.Id);
    }

    [Fact]
    public void Repository_can_be_registered()
    {
        var repository = new StubOrderRepository(CreateOrder());
        ServiceLocator.Register("orders", repository);

        Assert.Same(repository, ServiceLocator.Get<IOrderRepository>("orders"));
    }

    [Fact]
    public void ApplyDiscount_saves_the_order()
    {
        var repository = new StubOrderRepository(CreateOrder());
        ServiceLocator.Register("orders", repository);

        new OrderHandler().ApplyDiscount("test-order", "SAVE10");

        Assert.Equal(1, repository.SaveCalls);
    }

    [Fact]
    public void ApplyDiscount_does_not_save_a_missing_order()
    {
        var repository = new StubOrderRepository(null);
        ServiceLocator.Register("orders", repository);

        new OrderHandler().ApplyDiscount("missing", "SAVE10");

        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public void ApplyDiscount_does_not_throw_when_database_is_unavailable()
    {
        ServiceLocator.Register("orders", new UnavailableOrderRepository());

        var error = Record.Exception(() => new OrderHandler().ApplyDiscount("test-order", "SAVE10"));

        Assert.Null(error);
    }

    private static Order CreateOrder(bool premium = false)
    {
        return new Order
        {
            Id = "test-order",
            User = new User { IsPremiumMember = premium },
            Items = new List<Item> { new() { Price = 40 }, new() { Price = 60 } }
        };
    }

    private class StubOrderRepository(Order? order) : IOrderRepository
    {
        public int SaveCalls;

        public Order? FindOrder(string id) => order;
        public Coupon? FindCoupon(string code) => new() { Code = code, IsValid = true, DiscountPercentage = 0.1 };
        public void Save(Order savedOrder) => SaveCalls++;
    }

    private class UnavailableOrderRepository : IOrderRepository
    {
        public Order? FindOrder(string id) => throw new InvalidOperationException("Database unavailable");
        public Coupon? FindCoupon(string code) => throw new InvalidOperationException("Database unavailable");
        public void Save(Order order) => throw new InvalidOperationException("Database unavailable");
    }
}
