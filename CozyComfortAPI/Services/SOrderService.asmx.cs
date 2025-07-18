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
    public class SOrderService : WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["CozyComfort"].ConnectionString;

        [WebMethod]
        public OrderOperationResult PlaceOrder(int sellerId, int distributorId, int blanketId, int quantity)
        {
            var result = new OrderOperationResult();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO SOrder (SellerID, DistributorID, BlanketID, Quantity, OrderStatus, OrderDate)
                                    VALUES (@SellerID, @DistributorID, @BlanketID, @Quantity, 'Pending', GETDATE());
                                    SELECT SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SellerID", sellerId);
                    cmd.Parameters.AddWithValue("@DistributorID", distributorId);
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
        public OrderOperationResult UpdateOrderStatus(int orderId, string newStatus)
        {
            var result = new OrderOperationResult();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE SOrder SET OrderStatus = @Status 
                                   WHERE SOrderID = @OrderID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        result.Success = true;
                        result.Message = "Order status updated successfully";
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

        [WebMethod]
        public OrderOperationResult CancelOrder(int orderId)
        {
            
            var currentStatus = GetOrderStatus(orderId);
            if (currentStatus != "Pending" && currentStatus != "Approved")
            {
                return new OrderOperationResult
                {
                    Success = false,
                    Message = "Order cannot be cancelled in its current status"
                };
            }

            return UpdateOrderStatus(orderId, "Cancelled");
        }

        [WebMethod]
        public SOrderDetails GetOrderDetails(int orderId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT o.*, b.Model, b.Material, d.business_name AS DistributorName
                                   FROM SOrder o
                                   JOIN Blanket b ON o.BlanketID = b.BlanketID
                                   JOIN distributor d ON o.DistributorID = d.did
                                   WHERE o.SOrderID = @OrderID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SOrderDetails
                            {
                                OrderId = orderId,
                                SellerId = Convert.ToInt32(reader["SellerID"]),
                                DistributorId = Convert.ToInt32(reader["DistributorID"]),
                                DistributorName = reader["DistributorName"].ToString(),
                                BlanketId = Convert.ToInt32(reader["BlanketID"]),
                                BlanketModel = reader["Model"].ToString(),
                                BlanketMaterial = reader["Material"].ToString(),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                Status = reader["OrderStatus"].ToString(),
                                OrderDate = Convert.ToDateTime(reader["OrderDate"])
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new SOrderDetails { Error = ex.Message };
            }

            return new SOrderDetails { Error = "Order not found" };
        }

        [WebMethod]
        public List<SOrderDetails> GetOrdersBySeller(int sellerId, string statusFilter = "")
        {
            var orders = new List<SOrderDetails>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT o.*, b.Model, b.Material, d.business_name AS DistributorName
                                   FROM SOrder o
                                   JOIN Blanket b ON o.BlanketID = b.BlanketID
                                   JOIN distributor d ON o.DistributorID = d.did
                                   WHERE o.SellerID = @SellerID";

                    if (!string.IsNullOrEmpty(statusFilter))
                    {
                        query += " AND o.OrderStatus = @Status";
                    }

                    query += " ORDER BY o.OrderDate DESC";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SellerID", sellerId);

                    if (!string.IsNullOrEmpty(statusFilter))
                    {
                        cmd.Parameters.AddWithValue("@Status", statusFilter);
                    }

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new SOrderDetails
                            {
                                OrderId = Convert.ToInt32(reader["SOrderID"]),
                                SellerId = sellerId,
                                DistributorId = Convert.ToInt32(reader["DistributorID"]),
                                DistributorName = reader["DistributorName"].ToString(),
                                BlanketId = Convert.ToInt32(reader["BlanketID"]),
                                BlanketModel = reader["Model"].ToString(),
                                BlanketMaterial = reader["Material"].ToString(),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                Status = reader["OrderStatus"].ToString(),
                                OrderDate = Convert.ToDateTime(reader["OrderDate"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                orders.Add(new SOrderDetails { Error = ex.Message });
            }

            return orders;
        }

        [WebMethod]
        public List<SOrderDetails> GetOrdersByDistributor(int distributorId, string statusFilter = "")
        {
            var orders = new List<SOrderDetails>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT o.*, b.Model, b.Material, s.store_name
                           FROM SOrder o
                           JOIN Blanket b ON o.BlanketID = b.BlanketID
                           JOIN seller s ON o.SellerID = s.sid
                           WHERE o.DistributorID = @DistributorID";

                    if (!string.IsNullOrEmpty(statusFilter))
                    {
                        query += " AND o.OrderStatus = @Status";
                    }

                    query += " ORDER BY o.OrderDate DESC";

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
                            orders.Add(new SOrderDetails
                            {
                                OrderId = Convert.ToInt32(reader["SOrderID"]),
                                SellerId = Convert.ToInt32(reader["SellerID"]),
                                DistributorId = distributorId,
                                BlanketId = Convert.ToInt32(reader["BlanketID"]),
                                BlanketModel = reader["Model"].ToString(),
                                BlanketMaterial = reader["Material"].ToString(),
                                StoreName = reader["store_name"].ToString(),  
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                Status = reader["OrderStatus"].ToString(),
                                OrderDate = Convert.ToDateTime(reader["OrderDate"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
              
                System.Diagnostics.Debug.WriteLine("Error in GetOrdersByDistributor: " + ex.Message);
            }

            return orders;
        }

        [WebMethod]
        public List<DistributorBlanket> GetDistributorsForBlanket(int blanketId)
        {
            var distributors = new List<DistributorBlanket>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    
                    string query = @"SELECT d.did, d.business_name, di.DQuantity
                                   FROM distributor d
                                   JOIN DInventory di ON d.did = di.DistributorID
                                   WHERE di.DBlanketID = @BlanketID AND di.DQuantity > 0";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@BlanketID", blanketId);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            distributors.Add(new DistributorBlanket
                            {
                                DistributorId = Convert.ToInt32(reader["did"]),
                                DistributorName = reader["business_name"].ToString(),
                                AvailableQuantity = Convert.ToInt32(reader["DQuantity"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetDistributorsForBlanket: " + ex.Message);
            }

            return distributors;
        }

        private string GetOrderStatus(int orderId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT OrderStatus FROM SOrder WHERE SOrderID = @OrderID";
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

    public class OrderOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int OrderId { get; set; }
    }

    public class SOrderDetails
    {
        public int OrderId { get; set; }
        public int SellerId { get; set; }
        public int DistributorId { get; set; }
        public string DistributorName { get; set; }
        public int BlanketId { get; set; }
        public string BlanketModel { get; set; }
        public string BlanketMaterial { get; set; }
        public string StoreName { get; set; }  
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public string Error { get; set; }
    }

    public class DistributorBlanket
    {
        public int DistributorId { get; set; }
        public string DistributorName { get; set; }
        public int AvailableQuantity { get; set; }
    }
}