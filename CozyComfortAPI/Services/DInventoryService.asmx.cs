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
    public class DInventoryService : WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["CozyComfort"].ConnectionString;

        [WebMethod]
        public string AddDInventory(DInventory inv)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                   
                    SqlCommand cmdCheck = new SqlCommand(
                        "SELECT 1 FROM distributor WHERE did = @DistributorID",
                        con);
                    cmdCheck.Parameters.AddWithValue("@DistributorID", inv.DistributorID);

                    con.Open();
                    var exists = cmdCheck.ExecuteScalar();

                    if (exists == null)
                    {
                        return "error: Distributor account not found";
                    }

                    
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO DInventory (DistributorID, DBlanketID, DQuantity) " +
                        "VALUES (@DistributorID, @DBlanketID, @DQuantity)",
                        con);

                    cmd.Parameters.AddWithValue("@DistributorID", inv.DistributorID);
                    cmd.Parameters.AddWithValue("@DBlanketID", inv.DBlanketID);
                    cmd.Parameters.AddWithValue("@DQuantity", inv.DQuantity);

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
        public int GetDistributorIdByAppUserId(int appUserId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT did FROM distributor WHERE appuser_id = @AppUserId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@AppUserId", appUserId);

                con.Open();
                object result = cmd.ExecuteScalar();

                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        [WebMethod]
        public string UpdateDInventory(DInventory inv)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE DInventory SET DQuantity = @DQuantity, DLastUpdated = GETDATE() WHERE DInventoryID = @DInventoryID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@DInventoryID", inv.DInventoryID);
                    cmd.Parameters.AddWithValue("@DQuantity", inv.DQuantity);

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
        public string DeleteDInventory(int dInventoryId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM DInventory WHERE DInventoryID = @DInventoryID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@DInventoryID", dInventoryId);

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
        public string ReduceInventory(int blanketId, int quantity, int distributorId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                  
                    string getQuery = "SELECT DQuantity FROM DInventory WHERE DBlanketID = @BlanketID AND DistributorID = @DistributorID";
                    SqlCommand getCmd = new SqlCommand(getQuery, con);
                    getCmd.Parameters.AddWithValue("@BlanketID", blanketId);
                    getCmd.Parameters.AddWithValue("@DistributorID", distributorId);

                    con.Open();
                    int currentQty = Convert.ToInt32(getCmd.ExecuteScalar());

                    if (currentQty < quantity)
                    {
                        return $"error: Not enough inventory (Available: {currentQty}, Requested: {quantity})";
                    }

                 
                    string updateQuery = @"UPDATE DInventory 
                                 SET DQuantity = DQuantity - @Quantity,
                                 DLastUpdated = GETDATE()
                                 WHERE DBlanketID = @BlanketID AND DistributorID = @DistributorID";
                    SqlCommand updateCmd = new SqlCommand(updateQuery, con);
                    updateCmd.Parameters.AddWithValue("@Quantity", quantity);
                    updateCmd.Parameters.AddWithValue("@BlanketID", blanketId);
                    updateCmd.Parameters.AddWithValue("@DistributorID", distributorId);

                    int rows = updateCmd.ExecuteNonQuery();
                    return rows > 0 ? "success" : "error: Inventory not updated";
                }
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
        }

        [WebMethod]
        private string AddToDistributorInventory(int distributorId, int blanketId, int quantity)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string checkQuery = "SELECT DInventoryID FROM DInventory WHERE DistributorID = @DistributorID AND DBlanketID = @BlanketID";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@DistributorID", distributorId);
                    checkCmd.Parameters.AddWithValue("@BlanketID", blanketId);

                    con.Open();
                    object existingId = checkCmd.ExecuteScalar();

                    if (existingId != null)
                    {
                      
                        string updateQuery = @"UPDATE DInventory 
                                       SET DQuantity = DQuantity + @Quantity, 
                                           DLastUpdated = GETDATE()
                                       WHERE DistributorID = @DistributorID AND DBlanketID = @BlanketID";
                        SqlCommand updateCmd = new SqlCommand(updateQuery, con);
                        updateCmd.Parameters.AddWithValue("@DistributorID", distributorId);
                        updateCmd.Parameters.AddWithValue("@BlanketID", blanketId);
                        updateCmd.Parameters.AddWithValue("@Quantity", quantity);
                        updateCmd.ExecuteNonQuery();
                    }
                    else
                    {
                     
                        string insertQuery = @"INSERT INTO DInventory (DistributorID, DBlanketID, DQuantity, DLastUpdated)
                                       VALUES (@DistributorID, @BlanketID, @Quantity, GETDATE())";
                        SqlCommand insertCmd = new SqlCommand(insertQuery, con);
                        insertCmd.Parameters.AddWithValue("@DistributorID", distributorId);
                        insertCmd.Parameters.AddWithValue("@BlanketID", blanketId);
                        insertCmd.Parameters.AddWithValue("@Quantity", quantity);
                        insertCmd.ExecuteNonQuery();
                    }

                    return "success";
                }
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
        }


        [WebMethod]
        public List<DInventory> GetDInventoriesByDistributor(int distributorId)
        {
            List<DInventory> list = new List<DInventory>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM DInventory WHERE DistributorID = @DistributorID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@DistributorID", distributorId);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        list.Add(new DInventory
                        {
                            DInventoryID = Convert.ToInt32(reader["DInventoryID"]),
                            DistributorID = Convert.ToInt32(reader["DistributorID"]),
                            DBlanketID = Convert.ToInt32(reader["DBlanketID"]),
                            DQuantity = Convert.ToInt32(reader["DQuantity"]),
                            DLastUpdated = Convert.ToDateTime(reader["DLastUpdated"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                list.Add(new DInventory { Error = ex.Message });
            }

            return list;
        }
    }
}
