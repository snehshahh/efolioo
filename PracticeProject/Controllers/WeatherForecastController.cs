using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Text;

namespace APILink.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public WeatherForecastController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Login([FromQuery] string username, [FromQuery] string password)
        {
            // Create a new SqlConnection and open it
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // Prepare a SQL query to retrieve the user with the given username and password
                string query = "SELECT Email FROM [dbo].[LoginAuth] WHERE Email = @email AND Password = @password";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@email", username);
                    command.Parameters.AddWithValue("@password", password);

                    // Execute the query and read the results
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            // User was found, retrieve link and return it
                            reader.Read();
                            string userEmail = reader.GetString(0);
                            reader.Close();
                            string link = await RetrieveLink(connection, userEmail);
                            if (Uri.TryCreate(link, UriKind.Absolute, out Uri uri))
                            {
                                return Ok(new { uri });
                            }
                            return Ok();
                            
                        }
                        else
                        {
                            // User was not found, construct link and return it
                            string link = ConstructLink(username);
                            await InsertLink(username, link);
                            if (Uri.TryCreate(link, UriKind.Absolute, out Uri uri))
                            {
                                return Ok(new { uri });
                            }
                            return Ok();
                        }
                    }
                }
            }
        }

        private async Task<string> RetrieveLink(SqlConnection connection, string userEmail)
        {
            // Prepare a SQL query to retrieve the link for the user with the given email
            string query = "SELECT Link FROM [dbo].[Link] WHERE Email = @userEmail";
            using (SqlCommand command2 = new SqlCommand(query, connection))
            {
                command2.Parameters.AddWithValue("@userEmail", userEmail);

                // Execute the query and read the result
                using (SqlDataReader reader2 = await command2.ExecuteReaderAsync())
                {
                    if (reader2.HasRows)
                    {
                        reader2.Read();
                        return reader2.GetString(0);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        private string ConstructLink(string username)
        {
            // Construct a link with the username as a query parameter
            StringBuilder sb = new StringBuilder();
            sb.Append(" ://example.com/?email=");
            sb.Append(username);
            return sb.ToString();
        }
        private async Task<IActionResult> InsertLink(string userEmail, string link)
        {
            // Create a new SqlConnection and open it
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // Prepare a SQL query to insert the email and link into the table
                string query = "INSERT INTO [dbo].[Link] (Email, Link) VALUES (@email, @link)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@email", userEmail);
                    command.Parameters.AddWithValue("@link", link);

                    // Execute the query
                    await command.ExecuteNonQueryAsync();
                }
                return Ok ();
            }
        }
        // hello

    }
}