using Npgsql;

public class PostgresOrderRepository : IOrderRepository
{
    public static string ConnectionString = "Host=localhost;Port=5432;Database=orders;Username=orders;Password=orders";

    public Order? FindOrder(string id)
    {
        using var connection = OpenConnection();
        using var command = new NpgsqlCommand("SELECT * FROM orders WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", id);

        Order order;
        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read())
            {
                return null;
            }

            order = new Order
            {
                Id = reader.GetString(reader.GetOrdinal("id")),
                User = new User { IsPremiumMember = reader.GetBoolean(reader.GetOrdinal("is_premium_member")) },
                TotalPrice = reader.GetDouble(reader.GetOrdinal("total_price")),
                Discount = reader.GetDouble(reader.GetOrdinal("discount"))
            };
        }

        using var itemsCommand = new NpgsqlCommand("SELECT * FROM order_items WHERE order_id = @id", connection);
        itemsCommand.Parameters.AddWithValue("id", id);
        using var items = itemsCommand.ExecuteReader();
        while (items.Read())
        {
            order.Items.Add(new Item
            {
                Price = items.GetDouble(items.GetOrdinal("price")),
                Product = new Product { Category = items.GetString(items.GetOrdinal("category")) }
            });
        }

        return order;
    }

    public Coupon? FindCoupon(string code)
    {
        using var connection = OpenConnection();
        using var command = new NpgsqlCommand("SELECT * FROM coupons WHERE code = @code", connection);
        command.Parameters.AddWithValue("code", code);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new Coupon
        {
            Code = reader.GetString(reader.GetOrdinal("code")),
            DiscountPercentage = reader.GetDouble(reader.GetOrdinal("discount_percentage")),
            IsValid = reader.GetBoolean(reader.GetOrdinal("is_valid"))
        };
    }

    public void Save(Order order)
    {
        using var connection = OpenConnection();
        using var command = new NpgsqlCommand(
            "UPDATE orders SET total_price = @total, discount = @discount WHERE id = @id", connection);
        command.Parameters.AddWithValue("total", order.TotalPrice);
        command.Parameters.AddWithValue("discount", order.Discount);
        command.Parameters.AddWithValue("id", order.Id);
        command.ExecuteNonQuery();

        using var audit = new NpgsqlCommand(
            "INSERT INTO order_events (order_id, message, created_at) VALUES (@id, @message, @created)", connection);
        audit.Parameters.AddWithValue("id", order.Id);
        audit.Parameters.AddWithValue("message", $"Discount applied: {order.Discount}");
        audit.Parameters.AddWithValue("created", DateTime.Now);
        audit.ExecuteNonQuery();
    }

    private NpgsqlConnection OpenConnection()
    {
        var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }
}
