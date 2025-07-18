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
    [System.ComponentModel.ToolboxItem(false)]
    public class UOrderService : WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["CozyComfort"].ConnectionString;

        [WebMethod]
        public OrderOperationResult PlaceUOrder(int sellerId, string userName, string userContact, int blanketId, int quantity, DateTime? expectedDeliveryDate)
        {
            var result = new OrderOperationResult();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO UOrder (SellerID, User_name, User_contact, BlanketID, Quantity, Status, OrderDate, ExpectedDeliveryDate)
                                     VALUES (@SellerID, @UserName, @UserContact, @BlanketID, @Quantity, 'Pending', GETDATE(), @ExpectedDeliveryDate);
                                     SELECT SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SellerID", sellerId);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@UserContact", userContact);
                    cmd.Parameters.AddWithValue("@BlanketID", blanketId);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@ExpectedDeliveryDate", expectedDeliveryDate.HasValue ? (object)expectedDeliveryDate.Value : DBNull.Value);

                    con.Open();
                    int newOrderId = Convert.ToInt32(cmd.ExecuteScalar());

                    result.Success = true;
                    result.Message = "User order placed successfully.";
                    result.OrderId = newOrderId;
                }
            }
            catch (SqlException ex)
            {
                result.Success = false;
                result.Message = HandleSqlException(ex);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error: " + ex.Message;
            }

            return result;
        }

        [WebMethod]
        public OrderOperationResult CancelUOrder(int orderId)
        {
            string status = GetUOrderStatus(orderId);
            if (status != "Pending" && status != "Approved")
            {
                return new OrderOperationResult
                {
                    Success = false,
                    Message = "Order cannot be cancelled at this stage."
                };
            }

            return UpdateUOrderStatus(orderId, "Cancelled");
        }

        [WebMethod]
        public OrderOperationResult UpdateUOrderStatus(int orderId, string newStatus)
        {
            var result = new OrderOperationResult();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE UOrder SET Status = @Status WHERE OrderID = @OrderID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();

                    result.Success = rows > 0;
                    result.Message = rows > 0 ? "Order status updated." : "Order not found.";
                }
            }
            catch (SqlException ex)
            {
                result.Success = false;
                result.Message = HandleSqlException(ex);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error: " + ex.Message;
            }

            return result;
        }

        [WebMethod]
        public List<UOrderDetails> GetUOrdersBySeller(int sellerId, string statusFilter = "")
        {
            var orders = new List<UOrderDetails>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT u.*, b.Model 
                                     FROM UOrder u
                                     JOIN Blanket b ON u.BlanketID = b.BlanketID
                                     WHERE u.SellerID = @SellerID";

                    if (!string.IsNullOrEmpty(statusFilter))
                        query += " AND u.Status = @Status";

                    query += " ORDER BY u.OrderDate DESC";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SellerID", sellerId);
                    if (!string.IsNullOrEmpty(statusFilter))
                        cmd.Parameters.AddWithValue("@Status", statusFilter);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new UOrderDetails
                            {
                                OrderId = Convert.ToInt32(reader["OrderID"]),
                                SellerId = sellerId,
                                UserName = reader["User_name"].ToString(),
                                UserContact = reader["User_contact"].ToString(),
                                BlanketId = Convert.ToInt32(reader["BlanketID"]),
                                BlanketModel = reader["Model"].ToString(),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                Status = reader["Status"].ToString(),
                                OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                                ExpectedDeliveryDate = reader["ExpectedDeliveryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ExpectedDeliveryDate"]),
                                Error = null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                orders.Add(new UOrderDetails { Error = ex.Message });
            }

            return orders;
        }

        private string GetUOrderStatus(int orderId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT Status FROM UOrder WHERE OrderID = @OrderID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "";
                }
            }
            catch
            {
                return "";
            }
        }

        private string HandleSqlException(SqlException ex)
        {
            switch (ex.Number)
            {
                case 547:
                    return "Foreign key constraint failed.";
                case 2627:
                    return "Duplicate entry detected.";
                default:
                    return "SQL Error: " + ex.Message;
            }
        }
    }

    public class UOrderDetails
    {
        public int OrderId { get; set; }
        public int SellerId { get; set; }
        public string UserName { get; set; }
        public string UserContact { get; set; }
        public int BlanketId { get; set; }
        public string BlanketModel { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string Error { get; set; }
    }
}
