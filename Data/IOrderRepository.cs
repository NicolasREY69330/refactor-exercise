public interface IOrderRepository
{
    Order? FindOrder(string id);
    Coupon? FindCoupon(string code);
    void Save(Order order);
}
