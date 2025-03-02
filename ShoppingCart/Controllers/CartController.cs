using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace ShoppingCart.Controllers;

[Route("[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    [HttpPost]
    [Route("Add")]
    public async Task<IActionResult> AddCart([FromQuery] string userName)
    {
        using (var connection = new SqliteConnection("Data Source=../Database/Banco.db"))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = $"INSERT INTO Cart(UserName) VALUES (@Username)";
            command.Parameters.AddWithValue("@Username", userName);

            await command.ExecuteNonQueryAsync();
        }

        return Ok($"Usuário {userName} adicionado com sucesso!");
    }

    [HttpPost]
    [Route("AddItemToCart")]
    public async Task<IActionResult> AddItemToCart([FromQuery] int cartId, int productId)
    {
        using (var connection = new SqliteConnection("Data Source=../Database/Banco.db"))
        {
            connection.Open();

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = $"SELECT Quantity FROM Cart_Has_Product WHERE CartId = @CartId AND ProductId = @ProductId";
            selectCommand.Parameters.AddWithValue("@CartId", cartId);
            selectCommand.Parameters.AddWithValue("@ProductId", productId);

            var quantity = 1;

            using (var reader = await selectCommand.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    await reader.ReadAsync();

                    quantity = reader.GetInt32(0);

                    var updateCommand = connection.CreateCommand();
                    updateCommand.CommandText = $"UPDATE Cart_Has_Product SET Quantity = @Quantity WHERE CartId = @CartId AND ProductId = @ProductId";
                    updateCommand.Parameters.AddWithValue("@Quantity", quantity + 1);
                    updateCommand.Parameters.AddWithValue("@CartId", cartId);
                    updateCommand.Parameters.AddWithValue("@ProductId", productId);

                    await updateCommand.ExecuteNonQueryAsync();

                    return Ok("Quantidade atualizada com sucesso!");
                }
            }

            var command = connection.CreateCommand();
            command.CommandText = $"INSERT INTO Cart_Has_Product(CartId, ProductId, Quantity) VALUES (@CartId, @ProductId, @Quantity)";
            command.Parameters.AddWithValue("@Quantity", quantity);
            command.Parameters.AddWithValue("@CartId", cartId);
            command.Parameters.AddWithValue("@ProductId", productId);

            await command.ExecuteNonQueryAsync();
        }

        return Ok($"Produto adicionado com sucesso!");
    }

    [HttpDelete]
    [Route("DeleteItemFromCart")]
    public async Task<IActionResult> DeleteItemFromCart([FromQuery] int cartId, int productId)
    {
        using (var connection = new SqliteConnection("Data Source=../Database/Banco.db"))
        {
            connection.Open();

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = $"SELECT Quantity FROM Cart_Has_Product WHERE CartId = @CartId AND ProductId = @ProductId";
            selectCommand.Parameters.AddWithValue("@CartId", cartId);
            selectCommand.Parameters.AddWithValue("@ProductId", productId);

            using (var reader = await selectCommand.ExecuteReaderAsync())
            {
                if (!reader.HasRows)
                {
                    return BadRequest("O produto não existe nesse carrinho");
                }
            }

            var command = connection.CreateCommand();
            command.CommandText = $"DELETE FROM Cart_Has_Product WHERE CartId = @CartId AND ProductId = @ProductId";
            command.Parameters.AddWithValue("@CartId", cartId);
            command.Parameters.AddWithValue("@ProductId", productId);

            await command.ExecuteNonQueryAsync();
        }

        return Ok($"Produto deletado com sucesso!");
    }

    [HttpDelete]
    [Route("ClearCart")]
    public async Task<IActionResult> CleartCart([FromQuery] int cartId)
    {
        using (var connection = new SqliteConnection("Data Source=../Database/Banco.db"))
        {
            connection.Open();

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = $"SELECT Quantity FROM Cart_Has_Product WHERE CartId = @CartId";
            selectCommand.Parameters.AddWithValue("@CartId", cartId);

            using (var reader = await selectCommand.ExecuteReaderAsync())
            {
                if (!reader.HasRows)
                {
                    return BadRequest("O carrinho não existe");
                }
            }

            var command = connection.CreateCommand();
            command.CommandText = $"DELETE FROM Cart_Has_Product WHERE CartId = @CartId";
            command.Parameters.AddWithValue("@CartId", cartId);

            await command.ExecuteNonQueryAsync();
        }

        return Ok($"Carrinho deletado com sucesso!");
    }

    [HttpPut]
    [Route("AddCouponToCart")]
    public async Task<IActionResult> AddCouponToCart([FromQuery] int cartId, int couponId)
    {
        using (var connection = new SqliteConnection("Data Source=../Database/Banco.db"))
        {
            connection.Open();

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = $"SELECT AvailableQuantity FROM Coupon WHERE Id = @CouponId";
            selectCommand.Parameters.AddWithValue("@CouponId", couponId);

            using (var reader = await selectCommand.ExecuteReaderAsync())
            {
                if (!reader.HasRows)
                {
                    return BadRequest("O cupom não existe");
                }

                await reader.ReadAsync();

                var availableQuantity = reader.GetInt32(0);

                if (availableQuantity <= 0)
                {
                    return BadRequest("O cupom está esgotado");
                }

                var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = $"UPDATE Coupon SET AvailableQuantity = @AvailableQuantity WHERE Id = @CouponId";
                updateCommand.Parameters.AddWithValue("@CouponId", couponId);
                updateCommand.Parameters.AddWithValue("@AvailableQuantity", availableQuantity - 1);

                await updateCommand.ExecuteNonQueryAsync();
            }

            var command = connection.CreateCommand();
            command.CommandText = $"UPDATE Cart SET CouponId = @CouponId WHERE Id = @CartId";
            command.Parameters.AddWithValue("@CouponId", couponId);
            command.Parameters.AddWithValue("@CartId", cartId);

            await command.ExecuteNonQueryAsync();
        }

        return Ok($"Cupom vinculado com sucesso!");
    }

    [HttpGet]
    [Route("CompleteCart")]
    public async Task<IActionResult> GetCompleteCart([FromQuery] int cartId)
    {
        using (var connection = new SqliteConnection("Data Source=../Database/Banco.db"))
        {
            connection.Open();

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = @"SELECT c.Id, c.UserName, c.CouponId, p.Name, p.Price, cp.Quantity, coupon.DiscountPercentage FROM Cart_Has_Product cp
                INNER JOIN Cart c ON cp.CartId = c.Id
                INNER JOIN Product p ON cp.ProductId = p.Id
                INNER JOIN Coupon coupon ON c.CouponId = coupon.Id WHERE CartId = @CartId";
            selectCommand.Parameters.AddWithValue("@CartId", cartId);

            using (var reader = await selectCommand.ExecuteReaderAsync())
            {
                var completeCart = new CompleteCart();
                
                while (reader.Read())
                {
                    completeCart.CartId = reader.GetInt32(0);

                    completeCart.UserName = reader.GetString(1);

                    completeCart.Coupon = new Coupon()
                    {
                        DiscountPercentage = reader.GetInt32(6)
                    };
                    
                    var product = new Product()
                    {
                        Name = reader.GetString(3),
                        Price = reader.GetDouble(4),
                        Quantity = reader.GetInt32(5)
                    };

                    completeCart.Products.Add(product);
                }

                completeCart.Subtotal = completeCart.Products.Sum(x => x.Price * x.Quantity);
                completeCart.Total = completeCart.Subtotal - ((completeCart.Subtotal * completeCart.Coupon.DiscountPercentage) / 100);

                return Ok(completeCart);
            }
        }

    }
}