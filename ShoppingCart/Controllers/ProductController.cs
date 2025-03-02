using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace ShoppingCart.Controllers;

[Route("[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    [HttpPost]
    [Route("Add")]
    public async Task<IActionResult> AddProduct([FromQuery] string name, double price)
    {
        using (var connection = new SqliteConnection("Data Source=../Database/Banco.db"))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = $"INSERT INTO Product(Name, Price) VALUES (@Name, @Price)";
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@Price", price);

            await command.ExecuteNonQueryAsync();
        }

        return Ok($"Produto {name} de preço {price} adicionado com sucesso!");
    }
}