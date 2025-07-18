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
    public class DOrderService : WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["CozyComfort"].ConnectionString;

        [WebMethod]
        public OrderOperationResult PlaceDistributorOrder(int distributorId, int manufacturerId, int blanketId, int quantity)
        {
            var result = new OrderOperationResult();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO DOrder (DistributorID, ManufacturerID, DBlanketID, DQuantity, DStatus, DOrderDate)
                                    VALUES (@DistributorID, @ManufacturerID, @BlanketID, @Quantity, 'Pending', GETDATE());
                                    SELECT SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@DistributorID", distributorId);
                    cmd.Parameters.AddWithValue("@ManufacturerID", manufacturerId);
                    cmd.Parameters.AddWithValue("@BlanketID", blanketId);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);

                    con.Open();
                    int newOrderId = Convert.ToInt32(cmd.ExecuteScalar());

                    result.Success = true;
                    result.Message = "Order placed successfully";
                    result.OrderId = newOrderId;
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
        public OrderOperationResult UpdateDistributorOrderStatus(int orderId, string newStatus)
        {
            var result = new OrderOperationResult();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE DOrder SET DStatus = @Status 
                                   WHERE DOrderID = @OrderID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        result.Success = true;
                        result.Message = "Order status updated successfully";

                       
                        if (newStatus == "Completed")
                        {
                            UpdateInventoryAfterOrderCompletion(orderId);
                        }
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = "Order not found";
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

        private void UpdateInventoryAfterOrderCompletion(int orderId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlTransaction transaction = con.BeginTransaction();

                    try
                    {
                        
                        string getOrderQuery = @"SELECT ManufacturerID, DistributorID, DBlanketID, DQuantity 
                                               FROM DOrder 
                                               WHERE DOrderID = @OrderID";

                        SqlCommand getOrderCmd = new SqlCommand(getOrderQuery, con, transaction);
                        getOrderCmd.Parameters.AddWithValue("@OrderID", orderId);

                        SqlDataReader reader = getOrderCmd.ExecuteReader();
                        if (reader.Read())
                        {
                            int manufacturerId = Convert.ToInt32(reader["ManufacturerID"]);
                            int distributorId = Convert.ToInt32(reader["DistributorID"]);
                            int blanketId = Convert.ToInt32(reader["DBlanketID"]);
                            int quantity = Convert.ToInt32(reader["DQuantity"]);

                            reader.Close();

                            string updateMInventoryQuery = @"UPDATE MInventory 
                                                           SET MQuantity = MQuantity - @Quantity 
                                                           WHERE ManufacturerID = @ManufacturerID 
                                                           AND MBlanketID = @BlanketID";

                            SqlCommand updateMInventoryCmd = new SqlCommand(updateMInventoryQuery, con, transaction);
                            updateMInventoryCmd.Parameters.AddWithValue("@Quantity", quantity);
                            updateMInventoryCmd.Parameters.AddWithValue("@ManufacturerID", manufacturerId);
                            updateMInventoryCmd.Parameters.AddWithValue("@BlanketID", blanketId);
                            updateMInventoryCmd.ExecuteNonQuery();

                            string checkDInventoryQuery = @"SELECT 1 FROM DInventory 
                                                          WHERE DistributorID = @DistributorID 
                                                          AND DBlanketID = @BlanketID";

                            SqlCommand checkDInventoryCmd = new SqlCommand(checkDInventoryQuery, con, transaction);
                            checkDInventoryCmd.Parameters.AddWithValue("@DistributorID", distributorId);
                            checkDInventoryCmd.Parameters.AddWithValue("@BlanketID", blanketId);

                            object exists = checkDInventoryCmd.ExecuteScalar();

                            if (exists != null)
                            {
                   
                                string updateDInventoryQuery = @"UPDATE DInventory 
                                                              SET DQuantity = DQuantity + @Quantity 
                                                              WHERE DistributorID = @DistributorID 
                                                              AND DBlanketID = @BlanketID";

                                SqlCommand updateDInventoryCmd = new SqlCommand(updateDInventoryQuery, con, transaction);
                                updateDInventoryCmd.Parameters.AddWithValue("@Quantity", quantity);
                                updateDInventoryCmd.Parameters.AddWithValue("@DistributorID", distributorId);
                                updateDInventoryCmd.Parameters.AddWithValue("@BlanketID", blanketId);
                                updateDInventoryCmd.ExecuteNonQuery();
                            }
                            else
                            {
                              
                                string insertDInventoryQuery = @"INSERT INTO DInventory 
                                                              (DistributorID, DBlanketID, DQuantity) 
                                                              VALUES (@DistributorID, @BlanketID, @Quantity)";

                                SqlCommand insertDInventoryCmd = new SqlCommand(insertDInventoryQuery, con, transaction);
                                insertDInventoryCmd.Parameters.AddWithValue("@DistributorID", distributorId);
                                insertDInventoryCmd.Parameters.AddWithValue("@BlanketID", blanketId);
                                insertDInventoryCmd.Parameters.AddWithValue("@Quantity", quantity);
                                insertDInventoryCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            reader.Close();
                            throw new Exception("Order not found");
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
               
                System.Diagnostics.Debug.WriteLine("Error updating inventory: " + ex.Message);
                throw;
            }
        }

        [WebMethod]
        public OrderOperationResult CancelDistributorOrder(int orderId)
        {
            
            var currentStatus = GetDistributorOrderStatus(orderId);
            if (currentStatus != "Pending" && currentStatus != "Approved")
            {
                return new OrderOperationResult
                {
                    Success = false,
                    Message = "Order cannot be cancelled in its current status"
                };
            }

            return UpdateDistributorOrderStatus(orderId, "Cancelled");
        }

        [WebMethod]
        public DOrderDetails GetDistributorOrderDetails(int orderId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT o.*, b.Model, b.Material, 
                                   m.company_name AS ManufacturerName,
                                   d.business_name AS DistributorName
                                   FROM DOrder o
                                   JOIN Blanket b ON o.DBlanketID = b.BlanketID
                                   JOIN manufacturer m ON o.ManufacturerID = m.mid
                                   JOIN distributor d ON o.DistributorID = d.did
                                   WHERE o.DOrderID = @OrderID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new DOrderDetails
                            {
                                OrderId = orderId,
                                DistributorId = Convert.ToInt32(reader["DistributorID"]),
                                DistributorName = reader["DistributorName"].ToString(),
                                ManufacturerId = Convert.ToInt32(reader["ManufacturerID"]),
                                ManufacturerName = reader["ManufacturerName"].ToString(),
                                BlanketId = Convert.ToInt32(reader["DBlanketID"]),
                                BlanketModel = reader["Model"].ToString(),
                                BlanketMaterial = reader["Material"].ToString(),
                                Quantity = Convert.ToInt32(reader["DQuantity"]),
                                Status = reader["DStatus"].ToString(),
                                OrderDate = Convert.ToDateTime(reader["DOrderDate"])
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new DOrderDetails { Error = ex.Message };
            }

            return new DOrderDetails { Error = "Order not found" };
        }

        [WebMethod]
        public List<DOrderDetails> GetDistributorOrdersByManufacturer(int manufacturerId, string statusFilter = "")
        {
            var orders = new List<DOrderDetails>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT o.*, b.Model, b.Material, d.business_name AS DistributorName
                                   FROM DOrder o
                                   JOIN Blanket b ON o.DBlanketID = b.BlanketID
                                   JOIN distributor d ON o.DistributorID = d.did
                                   WHERE o.ManufacturerID = @ManufacturerID";

                    if (!string.IsNullOrEmpty(statusFilter))
                    {
                        query += " AND o.DStatus = @Status";
                    }

                    query += " ORDER BY o.DOrderDate DESC";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ManufacturerID", manufacturerId);

                    if (!string.IsNullOrEmpty(statusFilter))
                    {
                        cmd.Parameters.AddWithValue("@Status", statusFilter);
                    }

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new DOrderDetails
                            {
                                OrderId = Convert.ToInt32(reader["DOrderID"]),
                                DistributorId = Convert.ToInt32(reader["DistributorID"]),
                                DistributorName = reader["DistributorName"].ToString(),
                                ManufacturerId = manufacturerId,
                                BlanketId = Convert.ToInt32(reader["DBlanketID"]),
                                BlanketModel = reader["Model"].ToString(),
                                BlanketMaterial = reader["Material"].ToString(),
                                Quantity = Convert.ToInt32(reader["DQuantity"]),
                                Status = reader["DStatus"].ToString(),
                                OrderDate = Convert.ToDateTime(reader["DOrderDate"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                orders.Add(new DOrderDetails { Error = ex.Message });
            }

            return orders;
        }

        [WebMethod]
        public List<DOrderDetails> GetDistributorOrdersByDistributor(int distributorId, string statusFilter = "")
        {
            var orders = new List<DOrderDetails>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT o.*, b.Model, b.Material, m.company_name AS ManufacturerName
                                   FROM DOrder o
                                   JOIN Blanket b ON o.DBlanketID = b.BlanketID
                                   JOIN manufacturer m ON o.ManufacturerID = m.mid
                                   WHERE o.DistributorID = @DistributorID";

                    if (!string.IsNullOrEmpty(statusFilter))
                    {
                        query += " AND o.DStatus = @Status";
                    }

                    query += " ORDER BY o.DOrderDate DESC";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@DistributorID", distributorId);

                    if (!string.IsNullOrEmpty(statusFilter))
                    {
                        cmd.Parameters.AddWithValue("@Status", statusFilter);
                    }

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new DOrderDetails
                            {
                                OrderId = Convert.ToInt32(reader["DOrderID"]),
                                DistributorId = distributorId,
                                ManufacturerId = Convert.ToInt32(reader["ManufacturerID"]),
                                ManufacturerName = reader["ManufacturerName"].ToString(),
                                BlanketId = Convert.ToInt32(reader["DBlanketID"]),
                                BlanketModel = reader["Model"].ToString(),
                                BlanketMaterial = reader["Material"].ToString(),
                                Quantity = Convert.ToInt32(reader["DQuantity"]),
                                Status = reader["DStatus"].ToString(),
                                OrderDate = Convert.ToDateTime(reader["DOrderDate"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                orders.Add(new DOrderDetails { Error = ex.Message });
            }

            return orders;
        }

        [WebMethod]
        public List<ManufacturerBlanket> GetManufacturersForBlanket(int blanketId)
        {
            var manufacturers = new List<ManufacturerBlanket>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    
                    string query = @"SELECT m.mid, m.company_name, mi.MQuantity
                                   FROM manufacturer m
                                   JOIN MInventory mi ON m.mid = mi.ManufacturerID
                                   WHERE mi.MBlanketID = @BlanketID AND mi.MQuantity > 0";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@BlanketID", blanketId);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            manufacturers.Add(new ManufacturerBlanket
                            {
                                ManufacturerId = Convert.ToInt32(reader["mid"]),
                                ManufacturerName = reader["company_name"].ToString(),
                                AvailableQuantity = Convert.ToInt32(reader["MQuantity"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
             
                System.Diagnostics.Debug.WriteLine("Error in GetManufacturersForBlanket: " + ex.Message);
            }

            return manufacturers;
        }

        private string GetDistributorOrderStatus(int orderId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT DStatus FROM DOrder WHERE DOrderID = @OrderID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    con.Open();
                    object result = cmd.ExecuteScalar();

                    return result?.ToString() ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private string HandleSqlException(SqlException ex)
        {
            switch (ex.Number)
            {
                case 547: 
                    return "Error: Related data exists. Operation cannot be completed.";
                case 2627: 
                    return "Error: Duplicate entry.";
                default:
                    return "Database error: " + ex.Message;
            }
        }
    }

    public class DOrderDetails
    {
        public int OrderId { get; set; }
        public int DistributorId { get; set; }
        public string DistributorName { get; set; }
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
        public int BlanketId { get; set; }
        public string BlanketModel { get; set; }
        public string BlanketMaterial { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public string Error { get; set; }
    }

    public class ManufacturerBlanket
    {
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
        public int AvailableQuantity { get; set; }
    }
}