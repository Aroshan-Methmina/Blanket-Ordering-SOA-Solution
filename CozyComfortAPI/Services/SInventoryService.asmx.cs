using CozyComfortAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;

namespace CozyComfortAPI.Services
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class SInventoryService : WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["CozyComfort"].ConnectionString;

        [WebMethod]
        public InventoryOperationResult AddSInventory(SInventory inv)
        {
            var result = new InventoryOperationResult();

            try
            {
               
                if (inv.SellerID <= 0 || inv.SBlanketID <= 0 || inv.SQuantity < 0)
                {
                    result.Success = false;
                    result.Message = "Invalid inventory data";
                    return result;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO SInventory (SellerID, SBlanketID, SQuantity) 
                                    VALUES (@SellerID, @SBlanketID, @SQuantity);
                                    SELECT SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SellerID", inv.SellerID);
                    cmd.Parameters.AddWithValue("@SBlanketID", inv.SBlanketID);
                    cmd.Parameters.AddWithValue("@SQuantity", inv.SQuantity);

                    con.Open();
                    var newId = cmd.ExecuteScalar();

                    result.Success = true;
                    result.Message = "Inventory added successfully";
                    result.InventoryId = Convert.ToInt32(newId);
                }
            }
            catch (SqlException sqlEx)
            {
                result.Success = false;
                result.Message = HandleSqlException(sqlEx);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error: " + ex.Message;
            }

            return result;
        }

        [WebMethod]
        public InventoryOperationResult UpdateSInventory(SInventory inv)
        {
            var result = new InventoryOperationResult();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE SInventory SET 
                                    SQuantity = @SQuantity, 
                                    SLastUpdated = GETDATE() 
                                    WHERE SInventoryID = @SInventoryID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SInventoryID", inv.SInventoryID);
                    cmd.Parameters.AddWithValue("@SQuantity", inv.SQuantity);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        result.Success = true;
                        result.Message = "Inventory updated successfully";
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = "Inventory record not found";
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                result.Success = false;
                result.Message = HandleSqlException(sqlEx);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error: " + ex.Message;
            }

            return result;
        }

        [WebMethod]
        public InventoryOperationResult DeleteSInventory(int sInventoryId)
        {
            var result = new InventoryOperationResult();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM SInventory WHERE SInventoryID = @SInventoryID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SInventoryID", sInventoryId);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        result.Success = true;
                        result.Message = "Inventory deleted successfully";
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = "Inventory record not found";
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                result.Success = false;
                result.Message = HandleSqlException(sqlEx);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error: " + ex.Message;
            }

            return result;
        }

        [WebMethod]
        public List<SInventory> GetSInventoriesBySeller(int sellerId)
        {
            var inventoryList = new List<SInventory>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT si.*, b.Model, b.Material 
                                   FROM SInventory si
                                   JOIN Blanket b ON si.SBlanketID = b.BlanketID
                                   WHERE si.SellerID = @SellerID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SellerID", sellerId);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var inventory = new SInventory
                            {
                                SInventoryID = Convert.ToInt32(reader["SInventoryID"]),
                                SellerID = Convert.ToInt32(reader["SellerID"]),
                                SBlanketID = Convert.ToInt32(reader["SBlanketID"]),
                                SQuantity = Convert.ToInt32(reader["SQuantity"]),
                                SLastUpdated = Convert.ToDateTime(reader["SLastUpdated"]),
                                Model = reader["Model"].ToString(),
                                Material = reader["Material"].ToString()
                            };
                            inventoryList.Add(inventory);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                inventoryList.Add(new SInventory { Error = ex.Message });
            }

            return inventoryList;
        }

        [WebMethod]
        public SInventory GetSInventoryById(int inventoryId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT si.*, b.Model, b.Material 
                                   FROM SInventory si
                                   JOIN Blanket b ON si.SBlanketID = b.BlanketID
                                   WHERE si.SInventoryID = @InventoryID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@InventoryID", inventoryId);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SInventory
                            {
                                SInventoryID = Convert.ToInt32(reader["SInventoryID"]),
                                SellerID = Convert.ToInt32(reader["SellerID"]),
                                SBlanketID = Convert.ToInt32(reader["SBlanketID"]),
                                SQuantity = Convert.ToInt32(reader["SQuantity"]),
                                SLastUpdated = Convert.ToDateTime(reader["SLastUpdated"]),
                                Model = reader["Model"].ToString(),
                                Material = reader["Material"].ToString()
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new SInventory { Error = ex.Message };
            }

            return new SInventory { Error = "Inventory not found" };
        }

        private string HandleSqlException(SqlException ex)
        {
            switch (ex.Number)
            {
                case 547: 
                    return "Error: Related record not found. Please check the seller or blanket ID.";
                case 2627: 
                    return "Error: Duplicate inventory entry.";
                case 2601: 
                    return "Error: This inventory record already exists.";
                default:
                    return "Database error: " + ex.Message;
            }
        }
    }

    public class InventoryOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int InventoryId { get; set; }
    }
}