using System;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Oms;
using System.Data.SqlClient;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication1.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PriceServiceController : ControllerBase
    {
        private const string ConnectionString = "Server=localhost;Database=testDB;Trusted_Connection=true";

        // GET: api/<PriceServiceController>/Get
        //[HttpGet]
        //public string Get()
        //{
        //    return "PriceServiceController";
        //}

        // GET api/<PriceServiceController>/Get/5
        [HttpGet("{id}")]
        public string GetPrice(int id)
        {
            Price price = null;

            var commandString = "SELECT ProductId, Value from dbo.Price WHERE ProductId = @productId; ";

            using var connection = new SqlConnection(ConnectionString);
            var command = new SqlCommand(commandString, connection);
            command.Parameters.AddWithValue("@productId", id);

            try
            {
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    price = new Price { ProductId = (int)reader[0], Value = (double)reader[1] };
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return price != null ? price.Value.ToString() : "Not found";
        }

        // POST api/<PriceServiceController>/Post
        //{ "productId" : 1, "value" : 10 }
        [HttpPost]
        public void Post([FromBody] JsonDocument json)
        {
            var productId = json.RootElement.GetProperty("productId").GetInt32();
            var value = json.RootElement.GetProperty("value").GetInt32();

            var commandString = "INSERT into dbo.Price (ProductId, Value) VALUES (@productId, @value); ";

            using var connection = new SqlConnection(ConnectionString);
            var command = new SqlCommand(commandString, connection);
            command.Parameters.AddWithValue("@productId", productId);
            command.Parameters.AddWithValue("@value", value);

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
