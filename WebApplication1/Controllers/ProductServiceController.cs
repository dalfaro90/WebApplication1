using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using WebApplication1.Oms;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication1.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductServiceController : ControllerBase
    {
        private const string ConnectionString = "Server=localhost;Database=testDB;Trusted_Connection=true";

        // GET: api/<ProductServiceController>/Get
        //[HttpGet]
        //public string Get()
        //{
        //    return "ProductServiceController";
        //}

        // GET api/<ProductServiceController>/Get/x
        [HttpGet("{id}")]
        public async System.Threading.Tasks.Task<string> GetAsync(int id)
        {
            Product product = null;
            var price = "0";
            var commandString = "SELECT ProductId, Name from dbo.Product WHERE ProductId = @productId; ";

            await using var connection = new SqlConnection(ConnectionString);
            var command = new SqlCommand(commandString, connection);
            command.Parameters.AddWithValue("@productId", id);

            try
            {
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    product = new Product { ProductId = (int)reader[0], Name = reader[1].ToString() };
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            using var client = new HttpClient { BaseAddress = new Uri("https://localhost:44354/") };
            var response = await client.GetAsync($"api/PriceService/GetPrice/{id}");
            if (response.IsSuccessStatusCode)
            {
                price = await response.Content.ReadAsStringAsync();
            }

            return product != null ? $"Product name: {product.Name} & Price {price}" : "Not found";
        }

        // POST api/<ProductServiceController>/Post
        //{ "name" : "adidas", "price" : 10 }
        [HttpPost]
        public async System.Threading.Tasks.Task PostAsync([FromBody] JsonDocument json)
        {
            var name = json.RootElement.GetProperty("name").GetString();
            var price = json.RootElement.GetProperty("price").GetInt32();

            var commandString = "INSERT into dbo.Product (Name) output INSERTED.ProductId VALUES (@name); ";

            await using var connection = new SqlConnection(ConnectionString);
            var command = new SqlCommand(commandString, connection);
            command.Parameters.AddWithValue("@name", name);

            try
            {
                connection.Open();
                var productId = (int)command.ExecuteScalar();

                var payload = "{\"productId\": " + productId + ",\"value\": " + price + "}";

                using var client = new HttpClient { BaseAddress = new Uri("https://localhost:44354/") };
                var response = await client.PostAsJsonAsync("api/PriceService/Post/", JsonDocument.Parse(payload));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // DELETE api/<ProductServiceController>/Delete/x
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            var commandString = "DELETE from dbo.Product WHERE ProductId = @productId; ";

            using var connection = new SqlConnection(ConnectionString);
            var command = new SqlCommand(commandString, connection);
            command.Parameters.AddWithValue("@productId", id);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
