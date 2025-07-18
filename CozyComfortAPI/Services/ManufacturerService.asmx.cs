using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Services;
using System.Diagnostics;

namespace CozyComfortAPI.Services
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class ManufacturerService : WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["CozyComfort"].ConnectionString;

        [WebMethod]
        public ManufacturerDTO GetManufacturerDetails(int manufacturerId)
        {
            if (manufacturerId <= 0)
            {
                throw new ArgumentException("Manufacturer ID must be a positive integer", nameof(manufacturerId));
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT mid, company_name FROM manufacturer WHERE mid = @mid";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@mid", manufacturerId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ManufacturerDTO
                            {
                                ManufacturerId = Convert.ToInt32(reader["mid"]),
                                CompanyName = reader["company_name"].ToString()
                            };
                        }
                    }
                }
                return null; 
            }
            catch (SqlException sqlEx)
            {
                
                LogError($"Database error in GetManufacturerDetails: {sqlEx.Message}");
                throw new ApplicationException("A database error occurred while retrieving manufacturer details.");
            }
            catch (Exception ex)
            {
                
                LogError($"Error in GetManufacturerDetails: {ex.Message}");
                throw new ApplicationException("An error occurred while retrieving manufacturer details.");
            }
        }

        [WebMethod]
        public List<ManufacturerDTO> GetAllManufacturers()
        {
            List<ManufacturerDTO> manufacturers = new List<ManufacturerDTO>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT mid, company_name FROM manufacturer";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            manufacturers.Add(new ManufacturerDTO
                            {
                                ManufacturerId = Convert.ToInt32(reader["mid"]),
                                CompanyName = reader["company_name"].ToString()
                            });
                        }
                    }
                }
                return manufacturers;
            }
            catch (SqlException sqlEx)
            {
                LogError($"Database error in GetAllManufacturers: {sqlEx.Message}");
                throw new ApplicationException("A database error occurred while retrieving all manufacturers.");
            }
            catch (Exception ex)
            {
                LogError($"Error in GetAllManufacturers: {ex.Message}");
                throw new ApplicationException("An error occurred while retrieving all manufacturers.");
            }
        }

        private void LogError(string errorMessage)
        {
            
            Debug.WriteLine($"[ERROR] {DateTime.UtcNow}: {errorMessage}");

            
        }
    }

    public class ManufacturerDTO
    {
        public int ManufacturerId { get; set; }
        public string CompanyName { get; set; }
    }
}