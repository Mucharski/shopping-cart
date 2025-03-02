using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace ShoppingCart.Controllers;

[Route("[controller]")]
[ApiController]
public class CouponController : ControllerBase
{
    [HttpPost]
    [Route("Add")]
    public async Task<IActionResult> AddCoupon([FromQuery] int availableQuantity, int discountPercentage)
    {
        using (var connection = new SqliteConnection("Data Source=../Database/Banco.db"))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = $"INSERT INTO Coupon(AvailableQuantity, DiscountPercentage) VALUES (@AvailableQuantity, @DiscountPercentage)";
            command.Parameters.AddWithValue("@AvailableQuantity", availableQuantity);
            command.Parameters.AddWithValue("@DiscountPercentage", discountPercentage);

            await command.ExecuteNonQueryAsync();
        }

        return Ok($"Cupom de {discountPercentage}% adicionado. Quantidade disponível {availableQuantity}");
    }
}