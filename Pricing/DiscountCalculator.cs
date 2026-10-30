public static class DiscountCalculator
{
    public static double PremiumDiscount = 0.1;

    public static double Calculate(Order order, Coupon? coupon)
    {
        if (order.User.IsPremiumMember)
        {
            order.Discount += PremiumDiscount;
        }

        if (coupon != null && coupon.IsValid)
        {
            order.Discount += coupon.DiscountPercentage;
        }

        return order.Items.Sum(item => item.Price) * (1 - order.Discount);
    }
}
