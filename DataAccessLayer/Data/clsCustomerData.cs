using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace DataAccessLayer
{
    public class clsCustomerData
    {

        static string ConnectionString = ConfigurationManager.AppSettings["DBConnectionString"];

        public static clsDTOs.UserDTO GetCustomerByID(int ID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {

                    string query = $"SELECT PersonID, FirstName, LastName, Email, PersonRole, CreatedAt FROM Users " +
                        "WHERE (PersonID = @CustomerID AND PersonRole = 'Customer' AND isActive = 1)";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@CustomerID", ID);

                        conn.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return (new clsDTOs.UserDTO
                                    (
                                        reader.GetInt32(reader.GetOrdinal("PersonID")),
                                        reader.GetString(reader.GetOrdinal("FirstName")),
                                        reader.GetString(reader.GetOrdinal("LastName")),
                                        reader.GetString(reader.GetOrdinal("Email")),
                                        reader.GetString(reader.GetOrdinal("PersonRole")),
                                        reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                                    ));
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }

            }
            catch (SqlException ex)
            {
                throw new Exception("[Error]: ", ex);
            }
            
        }

        public static int AddNewCustomer(clsDTOs.UserDTO CDTO, string HashPassword)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_RegisterUser", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@FirstName", CDTO.FirstName);
                        command.Parameters.AddWithValue("@LastName", CDTO.LastName);
                        command.Parameters.AddWithValue("@Email", CDTO.Email);
                        command.Parameters.AddWithValue("@PasswordHash", HashPassword);
                        var outputIdParam = new SqlParameter("@NewUserID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputIdParam);
                        conn.Open();
                        command.ExecuteNonQuery();
                        conn.Close();

                        return (int)outputIdParam.Value;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("[Error]: ", ex);
            }
            
        }

        public static bool UpdateCustomer(clsDTOs.UserDTO ADTO, string HashPassword)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_UpdateUser", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserID", ADTO.Id);
                        command.Parameters.AddWithValue("@FirstName", ADTO.FirstName);
                        command.Parameters.AddWithValue("@LastName", ADTO.LastName);
                        command.Parameters.AddWithValue("@Email", ADTO.Email);
                        command.Parameters.AddWithValue("@PasswordHash", HashPassword);

                        conn.Open();

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return true;
                        }

                        return false;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("[Error]: " + ex);
            }
        }

        public static bool DeleteCustomer(clsDTOs.UserDTO ADTO)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_DeactivateUser", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserID", ADTO.Id);

                        conn.Open();

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return true;
                        }

                        return false;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("[Error]: " + ex);
            }
        }

        public static int AddAddress(clsDTOs.AddressDTO AddressDTO, clsDTOs.UserDTO userDTO)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {

                    using (SqlCommand command = new SqlCommand("SP_AddAddress", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserID", userDTO.Id);
                        command.Parameters.AddWithValue("@CityID", AddressDTO.CityID);
                        command.Parameters.AddWithValue("@AdditionalInfo", AddressDTO.AdditionalInfo);
                        var outputIdParam = new SqlParameter("@NewAddressID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputIdParam);
                        conn.Open();
                        command.ExecuteNonQuery();
                        return (int)outputIdParam.Value;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("[Error]: ", ex);
            }
        }

        public static List<clsDTOs.AddressDTO> GetAllCustomerAddresses(clsDTOs.UserDTO userDTO)
        {
            try
            {
                List<clsDTOs.AddressDTO> usersList = new List<clsDTOs.AddressDTO>();

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_GetUserAllAddressesc", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@UserID", userDTO.Id);

                        conn.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                usersList.Add(new clsDTOs.AddressDTO
                                    (
                                        reader.GetInt32(reader.GetOrdinal("AddressID")),
                                        reader.GetInt32(reader.GetOrdinal("CityID")),
                                        reader.GetString(reader.GetOrdinal("CountryName")),
                                        reader.GetString(reader.GetOrdinal("CityName")),
                                        reader.GetString(reader.GetOrdinal("AdditionalInfo"))
                                    ));
                            }
                        }
                    }
                }

                return usersList;

            }
            catch (SqlException ex)
            {
                throw new Exception("[Error]: ", ex);
            }
        }
    }
}
