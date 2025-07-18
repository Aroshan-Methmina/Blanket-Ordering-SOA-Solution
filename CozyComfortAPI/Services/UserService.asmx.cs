using CozyComfortAPI.Models;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Services;

namespace CozyComfortAPI.Services
{
    /// <summary>
    /// Summary description for UserService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class UserService : System.Web.Services.WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["CozyComfort"].ConnectionString;

       

        [WebMethod]
        public string Login(string email, string password)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CozyComfort"].ConnectionString))
            {
                string query = "SELECT id AS UserID, user_type AS UserType FROM appuser WHERE email = @Email AND password = @Password";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string userType = reader["UserType"].ToString();
                    string userId = reader["UserID"].ToString();
                    return userType + ":" + userId; 
                }
                else
                {
                    return "invalid";
                }
            }
        }


        [WebMethod]
        public string Register(string email, string password, char userType, UserDetails details)
        {
            SqlTransaction transaction = null;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    transaction = con.BeginTransaction();

                    
                    SqlCommand cmdUser = new SqlCommand(
                        "INSERT INTO appuser (email, password, user_type) VALUES (@Email, @Password, @UserType); " +
                        "SELECT SCOPE_IDENTITY();",
                        con, transaction);

                    cmdUser.Parameters.AddWithValue("@Email", email);
                    cmdUser.Parameters.AddWithValue("@Password", password);
                    cmdUser.Parameters.AddWithValue("@UserType", userType.ToString());

                    int newUserId = Convert.ToInt32(cmdUser.ExecuteScalar());

                    
                    switch (userType)
                    {
                        case 'm': 
                            SqlCommand cmdManufacturer = new SqlCommand(
                                "INSERT INTO manufacturer (appuser_id, company_name, contact_person, " +
                                "manufacturer_contact, address, production_capacity) " +
                                "VALUES (@UserId, @CompanyName, @ContactPerson, @Contact, @Address, @Capacity)",
                                con, transaction);

                            cmdManufacturer.Parameters.AddWithValue("@UserId", newUserId);
                            cmdManufacturer.Parameters.AddWithValue("@CompanyName", details.CompanyName);
                            cmdManufacturer.Parameters.AddWithValue("@ContactPerson", details.ContactPerson);
                            cmdManufacturer.Parameters.AddWithValue("@Contact", details.ContactNumber);
                            cmdManufacturer.Parameters.AddWithValue("@Address", details.Address);
                            cmdManufacturer.Parameters.AddWithValue("@Capacity", details.ProductionCapacity);

                            cmdManufacturer.ExecuteNonQuery();
                            break;

                        case 'd':
                            SqlCommand cmdDistributor = new SqlCommand(
                                "INSERT INTO distributor (appuser_id, business_name, distributor_contact, " +
                                "warehouse_location, license_number) " +
                                "VALUES (@UserId, @BusinessName, @Contact, @Location, @License)",
                                con, transaction);

                            cmdDistributor.Parameters.AddWithValue("@UserId", newUserId);
                            cmdDistributor.Parameters.AddWithValue("@BusinessName", details.CompanyName);
                            cmdDistributor.Parameters.AddWithValue("@Contact", details.ContactNumber);
                            cmdDistributor.Parameters.AddWithValue("@Location", details.Address);
                            cmdDistributor.Parameters.AddWithValue("@License", details.LicenseNumber);

                            cmdDistributor.ExecuteNonQuery();
                            break;

                        case 's': 
                            SqlCommand cmdSeller = new SqlCommand(
                                "INSERT INTO seller (appuser_id, store_name, seller_contact, " +
                                "store_location, website) " +
                                "VALUES (@UserId, @StoreName, @Contact, @Location, @Website)",
                                con, transaction);

                            cmdSeller.Parameters.AddWithValue("@UserId", newUserId);
                            cmdSeller.Parameters.AddWithValue("@StoreName", details.CompanyName);
                            cmdSeller.Parameters.AddWithValue("@Contact", details.ContactNumber);
                            cmdSeller.Parameters.AddWithValue("@Location", details.Address);
                            cmdSeller.Parameters.AddWithValue("@Website", details.Website ?? "");

                            cmdSeller.ExecuteNonQuery();
                            break;
                    }

                    transaction.Commit();
                    return "success";
                }
            }
            catch (SqlException sqlEx)
            {
                transaction?.Rollback();
                
                return "Database error occurred during registration. Please try again later.";
            }
            catch (InvalidOperationException invEx)
            {
                transaction?.Rollback();
                
                return "Operation could not be completed. Please check your input and try again.";
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
               
                return "An unexpected error occurred. Please contact support.";
            }
        }

        [WebMethod]
        public UserDetails GetUserDetails(int userId, char userType)
        {
            UserDetails details = new UserDetails();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string tableName;
                    string idColumn;

                    switch (userType)
                    {
                        case 'm':
                            tableName = "manufacturer";
                            idColumn = "mid";
                            break;
                        case 'd':
                            tableName = "distributor";
                            idColumn = "did";
                            break;
                        case 's':
                            tableName = "seller";
                            idColumn = "sid";
                            break;
                        default:
                            throw new ArgumentException("Invalid user type");
                    }

                    SqlCommand cmd = new SqlCommand(
                        $"SELECT * FROM {tableName} WHERE appuser_id = @UserId",
                        con);

                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            details.UserId = userId;
                            details.UserType = userType;

                            if (userType == 'm')
                            {
                                details.CompanyName = reader["company_name"].ToString();
                                details.ContactPerson = reader["contact_person"].ToString();
                                details.ProductionCapacity = Convert.ToInt32(reader["production_capacity"]);
                            }
                            else if (userType == 'd')
                            {
                                details.CompanyName = reader["business_name"].ToString();
                                details.LicenseNumber = reader["license_number"].ToString();
                            }
                            else if (userType == 's')
                            {
                                details.CompanyName = reader["store_name"].ToString();
                                details.Website = reader["website"].ToString();
                            }

                            details.ContactNumber = reader[$"{tableName}_contact"].ToString();
                            details.Address = reader[userType == 'm' ? "address" :
                                            (userType == 'd' ? "warehouse_location" : "store_location")].ToString();
                        }
                    }

                    
                    SqlCommand cmdEmail = new SqlCommand(
                        "SELECT email FROM appuser WHERE id = @UserId",
                        con);

                    cmdEmail.Parameters.AddWithValue("@UserId", userId);
                    details.Email = cmdEmail.ExecuteScalar()?.ToString();
                }
            }
            catch (Exception ex)
            {
                details.Error = ex.Message;
            }

            return details;
        }

        [WebMethod]
        public string UpdatePassword(int userId, string oldPassword, string newPassword)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                  
                    SqlCommand cmdVerify = new SqlCommand(
                        "SELECT 1 FROM appuser WHERE id = @UserId AND password = @Password",
                        con);

                    cmdVerify.Parameters.AddWithValue("@UserId", userId);
                    cmdVerify.Parameters.AddWithValue("@Password", oldPassword);

                    con.Open();
                    object exists = cmdVerify.ExecuteScalar();

                    if (exists == null)
                    {
                        return "error: Current password is incorrect";
                    }

               
                    SqlCommand cmdUpdate = new SqlCommand(
                        "UPDATE appuser SET password = @NewPassword WHERE id = @UserId",
                        con);

                    cmdUpdate.Parameters.AddWithValue("@NewPassword", newPassword);
                    cmdUpdate.Parameters.AddWithValue("@UserId", userId);

                    return cmdUpdate.ExecuteNonQuery() > 0 ? "success" : "error: Password not updated";
                }
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
        }
    }
}