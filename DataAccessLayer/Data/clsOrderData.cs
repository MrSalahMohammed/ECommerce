using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataAccessLayer.clsDTOs;

namespace DataAccessLayer.Data
{
    public class clsOrderData
    {

        static string ConnectionString = ConfigurationManager.AppSettings["DBConnectionString"];

        private static DataTable ConvertToDataTable(List<OrderItemDTO> items)
        {
            DataTable table = new DataTable();
            table.Columns.Add("ProductID", typeof(int));
            table.Columns.Add("Quantity", typeof(int));

            foreach (var item in items)
            {
                table.Rows.Add(item.ProductID, item.Quantity);
            }

            return table;
        }

        public static int PlaceOrder(clsDTOs.OrderDTO orderDTO, List<OrderItemDTO> items)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_PlaceOrder", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    DataTable itemsTable = ConvertToDataTable(items);

                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = orderDTO.CustomerID;
                    cmd.Parameters.Add("@ShippingAddressID", SqlDbType.Int).Value = orderDTO.ShippingAddressID;

                    SqlParameter itemsParam = cmd.Parameters.Add("@Items", SqlDbType.Structured);
                    itemsParam.TypeName = "dbo.OrderItemType";
                    itemsParam.Value = itemsTable;

                    SqlParameter outputParam = new SqlParameter("@NewOrderID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    return (int)outputParam.Value;
                }
            }
        }

        public static bool CancelOrder(clsDTOs.OrderDTO OrderDTO)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_CancelOrder", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@OrderID", OrderDTO.OrderId);
                        command.Parameters.AddWithValue("@UserID", OrderDTO.CustomerID);

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

        public static OrderDTO GetOrderById(int orderId)
        {
            OrderDTO order = null;
            List<OrderItemDTO> items = new List<OrderItemDTO>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SP_GetOrderDetails", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@OrderID", SqlDbType.Int).Value = orderId;

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (order == null)
                        {
                            order = new OrderDTO(
                                reader.GetInt32(reader.GetOrdinal("OrderID")),
                                reader.GetInt32(reader.GetOrdinal("UserID")),
                                reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                reader.GetDouble(reader.GetOrdinal("TotalAmount")),
                                reader.GetInt32(reader.GetOrdinal("StatusID")),
                                reader.GetInt32(reader.GetOrdinal("ShippingAddressID")),
                                items
                            );
                        }

                        // Add items
                        items.Add(new OrderItemDTO(
                            reader.GetInt32(reader.GetOrdinal("ProductID")),
                            reader.GetInt32(reader.GetOrdinal("Quantity"))
                        ));
                    }
                }
            }

            return order;
        }

        public static List<OrderSummaryDTO> GetAllOrdersByUser(int userId)
        {
            List<OrderSummaryDTO> orders = new List<OrderSummaryDTO>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SP_GetOrdersByUser", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new OrderSummaryDTO(
                            reader.GetInt32(reader.GetOrdinal("OrderID")),
                            reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                            reader.GetString(reader.GetOrdinal("StatusName"))
                        ));
                    }
                }
            }

            return orders;
        }


        public static bool UpdateOrderStatus(clsDTOs.OrderDTO orderDTO)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_UpdateOrderStatus", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@OrderID", orderDTO.OrderId);
                        command.Parameters.AddWithValue("@StatusID", orderDTO.StatusID);

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
