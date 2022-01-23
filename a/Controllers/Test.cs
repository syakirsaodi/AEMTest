
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static a.Model.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace a.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Test : Controller
    {
        private IConfiguration Configuration;
        public Test(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> CallWebAPI(string UserName, string Password)
        {
            var msg = "";
            var login = new Login()
            {
                username = UserName,
                password = Password
            };

            var json = JsonConvert.SerializeObject(login);
            var client = new HttpClient();

            string url = "http://test-demo.aemenersol.com/api/Account/Login";



            var byteArray = Encoding.UTF8.GetBytes(json);

            var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            client.DefaultRequestHeaders.Authorization = header;

            var response = await client.PostAsync(url, login, new JsonMediaTypeFormatter());
            if (response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    msg = await response.Content.ReadAsStringAsync();

                }
                else
                {
                    return StatusCode(400, "Unauthorize");
                }

            }
            return Ok(msg);
        }




        [Route("GetPlatformWellActual")]
        [HttpGet]
        public async Task<IActionResult> GetPlatform(string AuthToken)
        {

            if (AuthToken != null)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                       
                        var streamTask = client.GetStreamAsync("http://test-demo.aemenersol.com/api/PlatformWell/GetPlatformWellActual");

                        var repositories = await JsonSerializer.DeserializeAsync<List<Platform>>(await streamTask);


                        return Ok(repositories);


                    }
                }
                catch
                {
                    return StatusCode(404, "Wrong Token");
                }
            }
            else
            {
                return StatusCode(400, "No Token Found");
            }
        }

        [Route("GetPlatformWellDummy")]
        [HttpGet]
        public async Task<IActionResult> GetPlatformDummy(string AuthToken)
        {

            if (AuthToken != null)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var streamTask = client.GetStreamAsync("http://test-demo.aemenersol.com/api/PlatformWell/GetPlatformWellDummy");//Dummy to retrieve different key.

                        var repositories = await JsonSerializer.DeserializeAsync<List<Platform>>(await streamTask);


                        return Ok(repositories);


                    }
                }
                catch
                {
                    return StatusCode(404, "Wrong Token");
                }
            }
            else
            {
                return StatusCode(400, "No Token Found");
            }
        }

        [Route("PostPlatformWellActual")]
        [HttpPost]
        public async Task<IActionResult> Insert(string AuthToken)
        {
            if (AuthToken != null)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var streamTask = client.GetStreamAsync("http://test-demo.aemenersol.com/api/PlatformWell/GetPlatformWellActual");

                        var repositories = await JsonSerializer.DeserializeAsync<List<Platform>>(await streamTask);
                        try
                        {
                            string connString = this.Configuration.GetConnectionString("DefaultConnection");
                            foreach (Platform p in repositories)
                            {

                                using (SqlConnection connection = new SqlConnection(connString))
                                {
                                    // Open your connection
                                    connection.Open();

                                    //Change the table name here
                                    SqlCommand check_User_Name = new SqlCommand("SELECT COUNT(*) FROM Platform WHERE (id = @id)", connection);
                                    check_User_Name.Parameters.AddWithValue("@id", p.id);
                                    int IdExist = (int)check_User_Name.ExecuteScalar();

                                    if (IdExist > 0)
                                    {
                                        string upd = "UPDATE Platform  SET uniqueName = @uniqueName,latitude = @latitude, longitude = @longitude,createdAt = @createdAt,updatedAt = @updatedAt WHERE (id = @id)";
                                        using (SqlCommand cmd = new SqlCommand(upd, connection))
                                        {
                                            //Loop through the and get of parameter values

                                            cmd.CommandType = System.Data.CommandType.Text;

                                            cmd.Parameters.AddWithValue("@id", p.id);
                                            cmd.Parameters.AddWithValue("@uniqueName", p.uniqueName);
                                            cmd.Parameters.AddWithValue("@latitude", p.latitude);
                                            cmd.Parameters.AddWithValue("@longitude", p.longitude);
                                            if (p.createdAt == null)
                                            {
                                                cmd.Parameters.AddWithValue("@createdAt", DBNull.Value);
                                            }
                                            else
                                                cmd.Parameters.AddWithValue("@createdAt", p.createdAt);
                                            if (p.updatedAt == null)
                                            {
                                                cmd.Parameters.AddWithValue("@updatedAt", DBNull.Value);
                                            }
                                            else
                                                cmd.Parameters.AddWithValue("@updatedAt", p.updatedAt);
                                            //Execute the query
                                            cmd.ExecuteNonQuery();
                                        }

                                        foreach (Well well in p.well)
                                        {
                                            SqlCommand check_well = new SqlCommand("SELECT COUNT(*) FROM Well WHERE (id = @id)", connection);
                                            check_well.Parameters.AddWithValue("@id", well.id);
                                            int wellExist = (int)check_well.ExecuteScalar();

                                            if (wellExist > 0)
                                            {
                                                string sql1 = "UPDATE Well SET  platformId =  @platformId, uniqueName = @uniqueName, latitude =@latitude,longitude = @longitude,createdAt = @createdAt, updatedAt = @updatedAt WHERE (id = @id)";

                                                using (SqlCommand cmd = new SqlCommand(sql1, connection))
                                                {
                                                    //Loop through the and get of parameter values

                                                    cmd.CommandType = System.Data.CommandType.Text;


                                                    cmd.Parameters.AddWithValue("@id", well.id);
                                                    cmd.Parameters.AddWithValue("@platformId", well.platformId);
                                                    if (String.IsNullOrEmpty(well.uniqueName))
                                                    {
                                                        cmd.Parameters.AddWithValue("@uniqueName", DBNull.Value);
                                                    }
                                                    else
                                                        cmd.Parameters.AddWithValue("@uniqueName", well.uniqueName);

                                                    cmd.Parameters.AddWithValue("@latitude", well.latitude);


                                                    cmd.Parameters.AddWithValue("@longitude", well.longitude);
                                                    if (well.createdAt == null)
                                                    {
                                                        cmd.Parameters.AddWithValue("@createdAt", DBNull.Value);
                                                    }
                                                    else
                                                        cmd.Parameters.AddWithValue("@createdAt", well.createdAt);
                                                    if (well.updatedAt == null)
                                                    {
                                                        cmd.Parameters.AddWithValue("@updatedAt", DBNull.Value);
                                                    }
                                                    else
                                                        cmd.Parameters.AddWithValue("@updatedAt", well.updatedAt);
                                                    //Execute the query
                                                    cmd.ExecuteNonQuery();
                                                }
                                            }
                                            else
                                            {
                                                string sql1 = "INSERT INTO Well (id, platformId, uniqueName, latitude, longitude, createdAt, updatedAt) VALUES (@id, @platformId, @uniqueName, @latitude, @longitude, @createdAt, @updatedAt)";

                                                using (SqlCommand cmd = new SqlCommand(sql1, connection))
                                                {
                                                    //Loop through the and get of parameter values

                                                    cmd.CommandType = System.Data.CommandType.Text;


                                                    cmd.Parameters.AddWithValue("@id", well.id);
                                                    cmd.Parameters.AddWithValue("@platformId", well.platformId);
                                                    if (String.IsNullOrEmpty(well.uniqueName))
                                                    {
                                                        cmd.Parameters.AddWithValue("@uniqueName", DBNull.Value);
                                                    }
                                                    else
                                                        cmd.Parameters.AddWithValue("@uniqueName", well.uniqueName);

                                                    cmd.Parameters.AddWithValue("@latitude", well.latitude);


                                                    cmd.Parameters.AddWithValue("@longitude", well.longitude);
                                                    if (well.createdAt == null)
                                                    {
                                                        cmd.Parameters.AddWithValue("@createdAt", DBNull.Value);
                                                    }
                                                    else
                                                        cmd.Parameters.AddWithValue("@createdAt", well.createdAt);
                                                    if (well.updatedAt == null)
                                                    {
                                                        cmd.Parameters.AddWithValue("@updatedAt", DBNull.Value);
                                                    }
                                                    else
                                                        cmd.Parameters.AddWithValue("@updatedAt", well.updatedAt);
                                                    //Execute the query
                                                    cmd.ExecuteNonQuery();

                                                }
                                            }
                                        }
                                        return StatusCode(200, "Data Updated");
                                    }



                                    else
                                    {

                                        string sql = "INSERT INTO Platform (id, uniqueName, latitude, longitude, createdAt, updatedAt) VALUES (@id, @uniqueName, @latitude, @longitude, @createdAt, @updatedAt)";
                                        // Create the Command and Parameter objects.
                                        using (SqlCommand cmd = new SqlCommand(sql, connection))
                                        {
                                            //Loop through the and get of parameter values

                                            cmd.CommandType = System.Data.CommandType.Text;
                                            cmd.Parameters.AddWithValue("@id", p.id);
                                            cmd.Parameters.AddWithValue("@uniqueName", p.uniqueName);
                                            cmd.Parameters.AddWithValue("@latitude", p.latitude);
                                            cmd.Parameters.AddWithValue("@longitude", p.longitude);
                                            if (p.createdAt == null)
                                            {
                                                cmd.Parameters.AddWithValue("@createdAt", DBNull.Value);
                                            }
                                            else
                                                cmd.Parameters.AddWithValue("@createdAt", p.createdAt);
                                            if (p.createdAt == null)
                                            {
                                                cmd.Parameters.AddWithValue("@updatedAt", DBNull.Value);
                                            }
                                            else
                                                cmd.Parameters.AddWithValue("@updatedAt", p.updatedAt);
                                            //Execute the query
                                            cmd.ExecuteNonQuery();
                                        }
                                        foreach (Well well in p.well)
                                        {
                                            string sql1 = "INSERT INTO Well (id, platformId, uniqueName, latitude, longitude, createdAt, updatedAt) VALUES (@id, @platformId, @uniqueName, @latitude, @longitude, @createdAt, @updatedAt)";

                                            using (SqlCommand cmd = new SqlCommand(sql1, connection))
                                            {
                                                //Loop through the and get of parameter values

                                                cmd.CommandType = System.Data.CommandType.Text;


                                                cmd.Parameters.AddWithValue("@id", well.id);
                                                cmd.Parameters.AddWithValue("@platformId", well.platformId);
                                                if (String.IsNullOrEmpty(well.uniqueName))
                                                {
                                                    cmd.Parameters.AddWithValue("@uniqueName", DBNull.Value);
                                                }
                                                else
                                                    cmd.Parameters.AddWithValue("@uniqueName", well.uniqueName);

                                                cmd.Parameters.AddWithValue("@latitude", well.latitude);


                                                cmd.Parameters.AddWithValue("@longitude", well.longitude);
                                                if (well.createdAt == null)
                                                {
                                                    cmd.Parameters.AddWithValue("@createdAt", DBNull.Value);
                                                }
                                                else
                                                    cmd.Parameters.AddWithValue("@createdAt", well.createdAt);
                                                if (well.updatedAt == null)
                                                {
                                                    cmd.Parameters.AddWithValue("@updatedAt", DBNull.Value);
                                                }
                                                else
                                                    cmd.Parameters.AddWithValue("@updatedAt", well.updatedAt);
                                                //Execute the query
                                                cmd.ExecuteNonQuery();
                                            }
                                        }


                                    }
                                }
                            }
                            return StatusCode(200, "Data Inserted");


                        }
                        catch (Exception e)
                        {
                            return StatusCode(500, e);
                        }

                    }
                }
                catch
                {
                    return StatusCode(404, "Wrong Token");
                }
            }
            else
            {
                return StatusCode(400, "No Token Found");
            }
            return Ok();

        }
        [Route("PostPlatformWellDummy")]
        [HttpPost]
        public async Task<IActionResult> DummyInsert(string AuthToken)
        {
            if (AuthToken != null)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                        var streamTask = client.GetStreamAsync("http://test-demo.aemenersol.com/api/PlatformWell/GetPlatformWellDummy");

                        var repositories = await JsonSerializer.DeserializeAsync<List<Platform>>(await streamTask);
                        try
                        {
                            string connString = this.Configuration.GetConnectionString("DefaultConnection");
                            foreach (Platform p in repositories)
                            {

                                using (SqlConnection connection = new SqlConnection(connString))
                                {
                                    // Open your connection
                                    connection.Open();

                                    //Change the table name here
                                    SqlCommand check_User_Name = new SqlCommand("SELECT COUNT(*) FROM Platform WHERE (id = @id)", connection);
                                    check_User_Name.Parameters.AddWithValue("@id", p.id);
                                    int IdExist = (int)check_User_Name.ExecuteScalar();

                                    if (IdExist > 0)
                                    {
                                        string upd = "UPDATE Platform  SET uniqueName = @uniqueName,latitude = @latitude, longitude = @longitude,createdAt = @createdAt,updatedAt = @updatedAt WHERE (id = @id)";
                                        using (SqlCommand cmd = new SqlCommand(upd, connection))
                                        {
                                            //Loop through the and get of parameter values

                                            cmd.CommandType = System.Data.CommandType.Text;

                                            cmd.Parameters.AddWithValue("@id", p.id);
                                            cmd.Parameters.AddWithValue("@uniqueName", p.uniqueName);
                                            cmd.Parameters.AddWithValue("@latitude", p.latitude);
                                            cmd.Parameters.AddWithValue("@longitude", p.longitude);
                                            if (p.createdAt == null)
                                            {
                                                cmd.Parameters.AddWithValue("@createdAt", DBNull.Value);
                                            }
                                            else
                                                cmd.Parameters.AddWithValue("@createdAt", p.createdAt);
                                            if (p.updatedAt == null)
                                            {
                                                cmd.Parameters.AddWithValue("@updatedAt", DBNull.Value);
                                            }
                                            else
                                                cmd.Parameters.AddWithValue("@updatedAt", p.updatedAt);
                                            //Execute the query
                                            cmd.ExecuteNonQuery();
                                        }

                                        foreach (Well well in p.well)
                                        {
                                            SqlCommand check_well = new SqlCommand("SELECT COUNT(*) FROM Well WHERE (id = @id)", connection);
                                            check_well.Parameters.AddWithValue("@id", well.id);
                                            int wellExist = (int)check_well.ExecuteScalar();

                                            if (wellExist > 0)
                                            {
                                                string sql1 = "UPDATE Well SET  platformId =  @platformId, uniqueName = @uniqueName, latitude =@latitude,longitude = @longitude,createdAt = @createdAt, updatedAt = @updatedAt WHERE (id = @id)";

                                                using (SqlCommand cmd = new SqlCommand(sql1, connection))
                                                {
                                                    //Loop through the and get of parameter values

                                                    cmd.CommandType = System.Data.CommandType.Text;


                                                    cmd.Parameters.AddWithValue("@id", well.id);
                                                    cmd.Parameters.AddWithValue("@platformId", well.platformId);
                                                    if (String.IsNullOrEmpty(well.uniqueName))
                                                    {
                                                        cmd.Parameters.AddWithValue("@uniqueName", DBNull.Value);
                                                    }
                                                    else
                                                        cmd.Parameters.AddWithValue("@uniqueName", well.uniqueName);

                                                    cmd.Parameters.AddWithValue("@latitude", well.latitude);


                                                    cmd.Parameters.AddWithValue("@longitude", well.longitude);
                                                    if (well.createdAt == null)
                                                    {
                                                        cmd.Parameters.AddWithValue("@createdAt", DBNull.Value);
                                                    }
                                                    else
                                                        cmd.Parameters.AddWithValue("@createdAt", well.createdAt);
                                                    if (well.updatedAt == null)
                                                    {
                                                        cmd.Parameters.AddWithValue("@updatedAt", DBNull.Value);
                                                    }
                                                    else
                                                        cmd.Parameters.AddWithValue("@updatedAt", well.updatedAt);
                                                    //Execute the query
                                                    cmd.ExecuteNonQuery();
                                                }
                                            }
                                            else
                                            {
                                                string sql1 = "INSERT INTO Well (id, platformId, uniqueName, latitude, longitude, createdAt, updatedAt) VALUES (@id, @platformId, @uniqueName, @latitude, @longitude, @createdAt, @updatedAt)";

                                                using (SqlCommand cmd = new SqlCommand(sql1, connection))
                                                {
                                                    //Loop through the and get of parameter values

                                                    cmd.CommandType = System.Data.CommandType.Text;


                                                    cmd.Parameters.AddWithValue("@id", well.id);
                                                    cmd.Parameters.AddWithValue("@platformId", well.platformId);
                                                    if (String.IsNullOrEmpty(well.uniqueName))
                                                    {
                                                        cmd.Parameters.AddWithValue("@uniqueName", DBNull.Value);
                                                    }
                                                    else
                                                        cmd.Parameters.AddWithValue("@uniqueName", well.uniqueName);

                                                    cmd.Parameters.AddWithValue("@latitude", well.latitude);


                                                    cmd.Parameters.AddWithValue("@longitude", well.longitude);
                                                    if (well.createdAt == null)
                                                    {
                                                        cmd.Parameters.AddWithValue("@createdAt", DBNull.Value);
                                                    }
                                                    else
                                                        cmd.Parameters.AddWithValue("@createdAt", well.createdAt);
                                                    if (well.updatedAt == null)
                                                    {
                                                        cmd.Parameters.AddWithValue("@updatedAt", DBNull.Value);
                                                    }
                                                    else
                                                        cmd.Parameters.AddWithValue("@updatedAt", well.updatedAt);
                                                    //Execute the query
                                                    cmd.ExecuteNonQuery();

                                                }
                                            }
                                        }
                                        return StatusCode(200, "Data Updated");
                                    }



                                    else
                                    {

                                        string sql = "INSERT INTO Platform (id, uniqueName, latitude, longitude, createdAt, updatedAt) VALUES (@id, @uniqueName, @latitude, @longitude, @createdAt, @updatedAt)";
                                        // Create the Command and Parameter objects.
                                        using (SqlCommand cmd = new SqlCommand(sql, connection))
                                        {
                                            //Loop through the and get of parameter values

                                            cmd.CommandType = System.Data.CommandType.Text;
                                            cmd.Parameters.AddWithValue("@id", p.id);
                                            cmd.Parameters.AddWithValue("@uniqueName", p.uniqueName);
                                            cmd.Parameters.AddWithValue("@latitude", p.latitude);
                                            cmd.Parameters.AddWithValue("@longitude", p.longitude);
                                            if (p.createdAt == null)
                                            {
                                                cmd.Parameters.AddWithValue("@createdAt", DBNull.Value);
                                            }
                                            else
                                                cmd.Parameters.AddWithValue("@createdAt", p.createdAt);
                                            if (p.createdAt == null)
                                            {
                                                cmd.Parameters.AddWithValue("@updatedAt", DBNull.Value);
                                            }
                                            else
                                                cmd.Parameters.AddWithValue("@updatedAt", p.updatedAt);
                                            //Execute the query
                                            cmd.ExecuteNonQuery();
                                        }
                                        foreach (Well well in p.well)
                                        {
                                            string sql1 = "INSERT INTO Well (id, platformId, uniqueName, latitude, longitude, createdAt, updatedAt) VALUES (@id, @platformId, @uniqueName, @latitude, @longitude, @createdAt, @updatedAt)";

                                            using (SqlCommand cmd = new SqlCommand(sql1, connection))
                                            {
                                                //Loop through the and get of parameter values

                                                cmd.CommandType = System.Data.CommandType.Text;


                                                cmd.Parameters.AddWithValue("@id", well.id);
                                                cmd.Parameters.AddWithValue("@platformId", well.platformId);
                                                if (String.IsNullOrEmpty(well.uniqueName))
                                                {
                                                    cmd.Parameters.AddWithValue("@uniqueName", DBNull.Value);
                                                }
                                                else
                                                    cmd.Parameters.AddWithValue("@uniqueName", well.uniqueName);

                                                cmd.Parameters.AddWithValue("@latitude", well.latitude);


                                                cmd.Parameters.AddWithValue("@longitude", well.longitude);
                                                if (well.createdAt == null)
                                                {
                                                    cmd.Parameters.AddWithValue("@createdAt", DBNull.Value);
                                                }
                                                else
                                                    cmd.Parameters.AddWithValue("@createdAt", well.createdAt);
                                                if (well.updatedAt == null)
                                                {
                                                    cmd.Parameters.AddWithValue("@updatedAt", DBNull.Value);
                                                }
                                                else
                                                    cmd.Parameters.AddWithValue("@updatedAt", well.updatedAt);
                                                //Execute the query
                                                cmd.ExecuteNonQuery();
                                            }
                                        }


                                    }
                                }
                            }
                            return StatusCode(200, "Data Inserted");


                        }
                        catch (Exception e)
                        {
                            return StatusCode(500, e);
                        }

                    }
                }
                catch
                {
                    return StatusCode(404, "Wrong Token");
                }
            }
            else
            {
                return StatusCode(400, "No Token Found");
            }
            return Ok();

        }
    }
}
