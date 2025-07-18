using CozyComfortAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Services;

namespace CozyComfortAPI.Services
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class MInventoryService : WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["CozyComfort"].ConnectionString;

        [WebMethod]
        public string AddMInventory(MInventory inv)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO MInventory (ManufacturerID, MBlanketID, MQuantity) VALUES (@ManufacturerID, @MBlanketID, @MQuantity)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ManufacturerID", inv.ManufacturerID);
                    cmd.Parameters.AddWithValue("@MBlanketID", inv.MBlanketID);
                    cmd.Parameters.AddWithValue("@MQuantity", inv.MQuantity);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "success";
                }
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
        }

        [WebMethod]
        public string UpdateMInventory(MInventory inv)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE MInventory SET MQuantity = @MQuantity, MLastUpdated = GETDATE() WHERE MInventoryID = @MInventoryID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@MInventoryID", inv.MInventoryID);
                    cmd.Parameters.AddWithValue("@MQuantity", inv.MQuantity);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0 ? "success" : "error: not found";
                }
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
        }

        [WebMethod]
        public string DeleteMInventory(int mInventoryId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM MInventory WHERE MInventoryID = @MInventoryID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@MInventoryID", mInventoryId);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0 ? "success" : "error: not found";
                }
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
        }

        [WebMethod]
        public List<MInventory> GetMInventoriesByManufacturer(int manufacturerId)
        {
            List<MInventory> list = new List<MInventory>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM MInventory WHERE ManufacturerID = @ManufacturerID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ManufacturerID", manufacturerId);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        list.Add(new MInventory
                        {
                            MInventoryID = Convert.ToInt32(reader["MInventoryID"]),
                            ManufacturerID = Convert.ToInt32(reader["ManufacturerID"]),
                            MBlanketID = Convert.ToInt32(reader["MBlanketID"]),
                            MQuantity = Convert.ToInt32(reader["MQuantity"]),
                            MLastUpdated = Convert.ToDateTime(reader["MLastUpdated"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                list.Add(new MInventory { Error = ex.Message });
            }

            return list;
        }
    }
}
