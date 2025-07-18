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
    public class DistributorService : WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["CozyComfort"].ConnectionString;

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
        public DistributorDetails GetDistributorDetails(int distributorId)
        {
            DistributorDetails details = new DistributorDetails();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT d.*, u.email 
                                FROM distributor d
                                JOIN appuser u ON d.appuser_id = u.id
                                WHERE d.did = @DistributorId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@DistributorId", distributorId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    details.DistributorId = distributorId;
                    details.BusinessName = reader["business_name"].ToString();
                    details.ContactNumber = reader["distributor_contact"].ToString();
                    details.WarehouseLocation = reader["warehouse_location"].ToString();
                    details.LicenseNumber = reader["license_number"].ToString();
                    details.Email = reader["email"].ToString();
                }
            }

            return details;
        }

        [WebMethod]
        public DistributorDetails[] GetAllDistributors()
        {
            List<DistributorDetails> distributors = new List<DistributorDetails>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT d.*, u.email 
                         FROM distributor d
                         JOIN appuser u ON d.appuser_id = u.id";

                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DistributorDetails distributor = new DistributorDetails
                    {
                        DistributorId = Convert.ToInt32(reader["did"]),
                        BusinessName = reader["business_name"].ToString(),
                        ContactNumber = reader["distributor_contact"].ToString(),
                        WarehouseLocation = reader["warehouse_location"].ToString(),
                        LicenseNumber = reader["license_number"].ToString(),
                        Email = reader["email"].ToString()
                    };
                    distributors.Add(distributor);
                }
            }

            return distributors.ToArray();
        }


        [WebMethod]
        public string GetDistributorCompanyName(int distributorId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT business_name FROM distributor WHERE did = @DistributorID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@DistributorID", distributorId);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "Unknown Distributor";
                }
            }
            catch (SqlException)
            {
                return "Database error while retrieving company name.";
            }
            catch (Exception)
            {
                return "Unexpected error occurred while retrieving company name.";
            }
        }


        [WebMethod]
        public string UpdateDistributorProfile(int distributorId, string businessName,
            string contactNumber, string warehouseLocation, string licenseNumber)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE distributor SET 
                                    business_name = @BusinessName,
                                    distributor_contact = @ContactNumber,
                                    warehouse_location = @WarehouseLocation,
                                    license_number = @LicenseNumber
                                    WHERE did = @DistributorId";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@BusinessName", businessName);
                    cmd.Parameters.AddWithValue("@ContactNumber", contactNumber);
                    cmd.Parameters.AddWithValue("@WarehouseLocation", warehouseLocation);
                    cmd.Parameters.AddWithValue("@LicenseNumber", licenseNumber);
                    cmd.Parameters.AddWithValue("@DistributorId", distributorId);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    return rowsAffected > 0 ? "success" : "error: Distributor not found";
                }
            }
            catch (SqlException)
            {
                return "Database error while updating profile.";
            }
            catch (Exception)
            {
                return "An unexpected error occurred while updating profile.";
            }
        }

    
    }

    public class DistributorDetails
    {
        public int DistributorId { get; set; }
        public string BusinessName { get; set; }
        public string ContactNumber { get; set; }
        public string WarehouseLocation { get; set; }
        public string LicenseNumber { get; set; }
        public string Email { get; set; }
        public string Error { get; set; }
    }
}