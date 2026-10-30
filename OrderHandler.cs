public class OrderHandler
{
    public void ApplyDiscount(string orderId, string couponCode)
    {
        try
        {
            var repository = ServiceLocator.Get<IOrderRepository>("orders");
            if (!OrderCache.Orders.TryGetValue(orderId, out var order))
            {
                order = repository.FindOrder(orderId);
                if (order == null)
                {
                    Console.WriteLine("Order not found!");
                    return;
                }

                OrderCache.Orders[orderId] = order;
            }

            var coupon = repository.FindCoupon(couponCode);
            order.TotalPrice = DiscountCalculator.Calculate(order, coupon);
            repository.Save(order);
            Console.WriteLine($"Order {orderId} processed with discount");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
