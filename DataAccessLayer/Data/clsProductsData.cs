using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{

    

    public class clsProductsData
    {
        static string ConnectionString = ConfigurationManager.AppSettings["DBConnectionString"];

        public static int AddNewProduct(clsDTOs.ProductDTO PDTO)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_AddProduct", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ProductName", PDTO.Name);
                        command.Parameters.AddWithValue("@ProductDescription", PDTO.Description);
                        command.Parameters.AddWithValue("@Price", PDTO.Price);
                        command.Parameters.AddWithValue("@CostPrice", PDTO.CostPrice);
                        command.Parameters.AddWithValue("@StockQuantity", PDTO.StockQuantity);
                        var outputIdParam = new SqlParameter("@NewProductID", SqlDbType.Int)
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
                throw new Exception("[Error]: " + ex);
            }
            
        }

        public static bool UpdateProduct(clsDTOs.ProductDTO PDTO)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_UpdateProduct", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ProductID", PDTO.Id);
                        command.Parameters.AddWithValue("@ProductName", PDTO.Name);
                        command.Parameters.AddWithValue("@ProductDescription", PDTO.Description);
                        command.Parameters.AddWithValue("@Price", PDTO.Price);
                        command.Parameters.AddWithValue("@CostPrice", PDTO.CostPrice);

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

        public static bool AddStock(clsDTOs.ProductDTO PDTO, int Quantity)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_AddStock", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ProductID", PDTO.Id);
                        command.Parameters.AddWithValue("@QuantityToAdd", Quantity);

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

        public static clsDTOs.ProductDTO FindProductByID(int ID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {

                    string query = $"SELECT ProductID, ProductName, ProductDescription, Price, StockQuantity, CreatedAt FROM Products " +
                        "WHERE (ProductID = @ProductID AND isActive = 1)";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@ProductID", ID);

                        conn.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return (new clsDTOs.ProductDTO
                                    (
                                        reader.GetInt32(reader.GetOrdinal("ProductID")),
                                        reader.GetString(reader.GetOrdinal("ProductName")),
                                        reader.GetString(reader.GetOrdinal("ProductDescription")),
                                        reader.GetDouble(reader.GetOrdinal("Price")),
                                        reader.GetDouble(reader.GetOrdinal("CostPrice")),
                                        reader.GetInt16(reader.GetOrdinal("StockQuantity")),
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

        public static bool DeleteProduct(clsDTOs.ProductDTO PDTO)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_DeactivateProduct", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ProductID", PDTO.Id);

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
