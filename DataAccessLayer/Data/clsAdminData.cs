using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    
    public class clsAdminData
    {

        static string ConnectionString = ConfigurationManager.AppSettings["DBConnectionString"];

        public static List<clsDTOs.UserDTO> GetAllCustomersData()
        {
            try
            {
                List<clsDTOs.UserDTO> usersList = new List<clsDTOs.UserDTO>();

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_GetAllCustomers", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        conn.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                usersList.Add(new clsDTOs.UserDTO
                                    (
                                        reader.GetInt32(reader.GetOrdinal("PersonID")),
                                        reader.GetString(reader.GetOrdinal("FirstName")),
                                        reader.GetString(reader.GetOrdinal("LastName")),
                                        reader.GetString(reader.GetOrdinal("Email")),
                                        reader.GetString(reader.GetOrdinal("PersonRole")),
                                        reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                                    ));
                            }
                        }
                    }
                }

                return usersList;

            }catch (SqlException ex)
            {
                throw new Exception("[Error]: ", ex);
            }
        }

        public static int AddNewAdmin(clsDTOs.UserDTO CDTO, clsDTOs.UserDTO ADTO,string HashPassword)
        {
            int NewID = 0;

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
                        NewID = (int)outputIdParam.Value;
                    }

                    using (SqlCommand command = new SqlCommand("SP_MakeAdmin", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@newUserID", CDTO.Id);
                        command.Parameters.AddWithValue("@UserID", ADTO.Id);
                        command.Parameters.AddWithValue("@PasswordHash", HashPassword);
                        conn.Open();
                        command.ExecuteNonQuery();
                        conn.Close();

                        return NewID;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("[Error]: ", ex);
            }

        }

        public static bool MakeAdmin(clsDTOs.UserDTO CDTO, clsDTOs.UserDTO ADTO, string HashPassword)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_MakeAdmin", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@newUserID", CDTO.Id);
                        command.Parameters.AddWithValue("@UserID", ADTO.Id);
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
                throw new Exception("[Error]: ", ex);
            }

        }

        public static bool UpdateAdmin(clsDTOs.UserDTO ADTO, string HashPassword)
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

        public static clsDTOs.UserDTO FindAdminByID(int ID)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {

                    string query = $"SELECT PersonID, FirstName, LastName, Email, PersonRole, CreatedAt FROM Users " +
                        "WHERE (PersonID = @AdminID AND PersonRole = 'Admin' AND isActive = 1)";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@AdminID", ID);

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

        public static bool DeleteAdmin(clsDTOs.UserDTO ADTO)
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

        

    }
}
