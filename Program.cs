var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapPost("/applyDiscount/{orderId}/{couponCode}", async (HttpContext context, string orderId, string couponCode) =>
{
    var orderHandler = new OrderHandler();

    try
    {
        if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(couponCode))
        {
            return Results.BadRequest("Order ID and Coupon Code are required.");
        }

        await Task.Run(() => orderHandler.ApplyDiscount(orderId, couponCode));
        return Results.Ok("Discount applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
        return Results.Problem("An error occurred while applying the discount.");
    }
})
.WithName("GetWeatherForecast")
.WithOpenApi();
   
app.Run();