using CozyComfortAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Services;

namespace CozyComfortAPI.Services
{
    /// <summary>
    /// BlanketService handles CRUD operations for Blanket entity.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class BlanketService : System.Web.Services.WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["CozyComfort"].ConnectionString;

        [WebMethod]
        public string AddBlanket(string model, string material, int productionCapacity)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Blanket (Model, Material, ProductionCapacity) VALUES (@Model, @Material, @ProductionCapacity)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Model", model);
                    cmd.Parameters.AddWithValue("@Material", material);
                    cmd.Parameters.AddWithValue("@ProductionCapacity", productionCapacity);

                    con.Open();
                    int result = cmd.ExecuteNonQuery();

                    return result > 0 ? "success" : "error: Could not add blanket";
                }
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
        }

        [WebMethod]
        public string UpdateBlanket(int blanketId, string model, string material, int productionCapacity)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Blanket SET Model = @Model, Material = @Material, ProductionCapacity = @ProductionCapacity WHERE BlanketID = @BlanketID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Model", model);
                    cmd.Parameters.AddWithValue("@Material", material);
                    cmd.Parameters.AddWithValue("@ProductionCapacity", productionCapacity);
                    cmd.Parameters.AddWithValue("@BlanketID", blanketId);

                    con.Open();
                    int result = cmd.ExecuteNonQuery();

                    return result > 0 ? "success" : "error: Blanket not found or not updated";
                }
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
        }

        [WebMethod]
        public string DeleteBlanket(int blanketId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Blanket WHERE BlanketID = @BlanketID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@BlanketID", blanketId);

                    con.Open();
                    int result = cmd.ExecuteNonQuery();

                    return result > 0 ? "success" : "error: Blanket not found or not deleted";
                }
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
        }

        [WebMethod]
        public Blanket GetBlanket(int blanketId)
        {
            Blanket blanket = null;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Blanket WHERE BlanketID = @BlanketID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@BlanketID", blanketId);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        blanket = new Blanket
                        {
                            BlanketID = (int)reader["BlanketID"],
                            Model = reader["Model"].ToString(),
                            Material = reader["Material"].ToString(),
                            ProductionCapacity = reader["ProductionCapacity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ProductionCapacity"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving blanket: " + ex.Message);
            }

            return blanket;
        }

        [WebMethod]
        public Blanket[] GetBlanketsByManufacturer(int manufacturerId)
        {
            var blankets = new List<Blanket>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"
                SELECT b.BlanketID, b.Model, b.Material, b.ProductionCapacity
                FROM Blanket b
                INNER JOIN MInventory m ON b.BlanketID = m.MBlanketID
                WHERE m.ManufacturerID = @ManufacturerID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ManufacturerID", manufacturerId);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            blankets.Add(new Blanket
                            {
                                BlanketID = (int)reader["BlanketID"],
                                Model = reader["Model"].ToString(),
                                Material = reader["Material"].ToString(),
                                ProductionCapacity = Convert.ToInt32(reader["ProductionCapacity"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving blankets: " + ex.Message);
            }

            return blankets.ToArray();
        }


        [WebMethod]
        public string GetBlanketModelName(int blanketId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT Model FROM Blanket WHERE BlanketID = @BlanketID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@BlanketID", blanketId);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "Unknown Model";
                }
            }
            catch
            {
                return "Error";
            }
        }


        [WebMethod]
        public Blanket[] GetAllBlankets()
        {
            var blankets = new System.Collections.Generic.List<Blanket>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Blanket";
                    SqlCommand cmd = new SqlCommand(query, con);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        blankets.Add(new Blanket
                        {
                            BlanketID = (int)reader["BlanketID"],
                            Model = reader["Model"].ToString(),
                            Material = reader["Material"].ToString(),
                            ProductionCapacity = reader["ProductionCapacity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ProductionCapacity"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching blankets: " + ex.Message);
            }

            return blankets.ToArray();
        }
    }
}
