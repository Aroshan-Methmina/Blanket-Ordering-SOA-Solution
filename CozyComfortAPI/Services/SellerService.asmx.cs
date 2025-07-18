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
    public class SellerService : WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["CozyComfort"].ConnectionString;

        [WebMethod]
        public SellerOperationResult RegisterSeller(Seller seller)
        {
            var result = new SellerOperationResult();
            SqlTransaction transaction = null;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    transaction = con.BeginTransaction();

                  
                    string userQuery = @"INSERT INTO appuser (email, password, user_type) 
                                       VALUES (@Email, @Password, 's');
                                       SELECT SCOPE_IDENTITY();";

                    SqlCommand userCmd = new SqlCommand(userQuery, con, transaction);
                    userCmd.Parameters.AddWithValue("@Email", seller.Email);
                    userCmd.Parameters.AddWithValue("@Password", seller.Password);

                    int appUserId = Convert.ToInt32(userCmd.ExecuteScalar());

                
                    string sellerQuery = @"INSERT INTO seller (appuser_id, store_name, seller_contact, store_location, website)
                                         VALUES (@AppUserId, @StoreName, @Contact, @Location, @Website)";

                    SqlCommand sellerCmd = new SqlCommand(sellerQuery, con, transaction);
                    sellerCmd.Parameters.AddWithValue("@AppUserId", appUserId);
                    sellerCmd.Parameters.AddWithValue("@StoreName", seller.StoreName);
                    sellerCmd.Parameters.AddWithValue("@Contact", seller.ContactNumber);
                    sellerCmd.Parameters.AddWithValue("@Location", seller.StoreLocation);
                    sellerCmd.Parameters.AddWithValue("@Website", seller.Website ?? "");

                    sellerCmd.ExecuteNonQuery();

                    transaction.Commit();

                    result.Success = true;
                    result.Message = "Seller registered successfully";
                    result.SellerId = appUserId;
                }
            }
            catch (SqlException sqlEx)
            {
                transaction?.Rollback();
                result.Success = false;
                result.Message = HandleSqlException(sqlEx);
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                result.Success = false;
                result.Message = "Error: " + ex.Message;
            }

            return result;
        }

        [WebMethod]
        public int GetSellerIdByAppUserId(int appUserId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT sid FROM seller WHERE appuser_id = @AppUserId";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@AppUserId", appUserId);

                    con.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    return -1; 
                }
            }
            catch (Exception ex)
            {
               
                System.Diagnostics.Debug.WriteLine("Error in GetSellerIdByAppUserId: " + ex.Message);
                return -1;
            }
        }

        [WebMethod]
        public SellerOperationResult UpdateSellerProfile(Seller seller)
        {
            var result = new SellerOperationResult();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE seller SET
                                   store_name = @StoreName,
                                   seller_contact = @Contact,
                                   store_location = @Location,
                                   website = @Website
                                   WHERE appuser_id = @SellerId";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@StoreName", seller.StoreName);
                    cmd.Parameters.AddWithValue("@Contact", seller.ContactNumber);
                    cmd.Parameters.AddWithValue("@Location", seller.StoreLocation);
                    cmd.Parameters.AddWithValue("@Website", seller.Website ?? "");
                    cmd.Parameters.AddWithValue("@SellerId", seller.SellerId);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        result.Success = true;
                        result.Message = "Seller profile updated successfully";
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = "Seller not found";
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
        public Seller GetSellerDetails(int sellerId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT s.*, u.email 
                                   FROM seller s
                                   JOIN appuser u ON s.appuser_id = u.id
                                   WHERE s.appuser_id = @SellerId";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SellerId", sellerId);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Seller
                            {
                                SellerId = sellerId,
                                StoreName = reader["store_name"].ToString(),
                                ContactNumber = reader["seller_contact"].ToString(),
                                StoreLocation = reader["store_location"].ToString(),
                                Website = reader["website"].ToString(),
                                Email = reader["email"].ToString()
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new Seller { Error = ex.Message };
            }

            return new Seller { Error = "Seller not found" };
        }

        [WebMethod]
        public List<Seller> GetAllSellers()
        {
            var sellers = new List<Seller>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT s.appuser_id as seller_id, s.store_name, 
                                   s.seller_contact, s.store_location, s.website, u.email
                                   FROM seller s
                                   JOIN appuser u ON s.appuser_id = u.id";

                    SqlCommand cmd = new SqlCommand(query, con);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sellers.Add(new Seller
                            {
                                SellerId = Convert.ToInt32(reader["seller_id"]),
                                StoreName = reader["store_name"].ToString(),
                                ContactNumber = reader["seller_contact"].ToString(),
                                StoreLocation = reader["store_location"].ToString(),
                                Website = reader["website"].ToString(),
                                Email = reader["email"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sellers.Add(new Seller { Error = ex.Message });
            }

            return sellers;
        }

        [WebMethod]
        public SellerOperationResult DeleteSeller(int sellerId)
        {
            var result = new SellerOperationResult();
            SqlTransaction transaction = null;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    transaction = con.BeginTransaction();

                    string sellerQuery = "DELETE FROM seller WHERE appuser_id = @SellerId";
                    SqlCommand sellerCmd = new SqlCommand(sellerQuery, con, transaction);
                    sellerCmd.Parameters.AddWithValue("@SellerId", sellerId);
                    int sellerRows = sellerCmd.ExecuteNonQuery();

                    
                    string userQuery = "DELETE FROM appuser WHERE id = @SellerId";
                    SqlCommand userCmd = new SqlCommand(userQuery, con, transaction);
                    userCmd.Parameters.AddWithValue("@SellerId", sellerId);
                    int userRows = userCmd.ExecuteNonQuery();

                    transaction.Commit();

                    if (sellerRows > 0 && userRows > 0)
                    {
                        result.Success = true;
                        result.Message = "Seller deleted successfully";
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = "Seller not found";
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                transaction?.Rollback();
                result.Success = false;
                result.Message = HandleSqlException(sqlEx);
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                result.Success = false;
                result.Message = "Error: " + ex.Message;
            }

            return result;
        }

        private string HandleSqlException(SqlException ex)
        {
            switch (ex.Number)
            {
                case 2627: 
                    return "Error: This email is already registered.";
                case 547: 
                    return "Error: Related records exist. Cannot delete seller.";
                case 2601: 
                    return "Error: Duplicate seller information.";
                default:
                    return "Database error: " + ex.Message;
            }
        }
    }

    public class SellerOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int SellerId { get; set; }
    }
}