using DataAccessLayer;
using DataAccessLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataAccessLayer.clsDTOs;

namespace BusinessLayer.Member_Classes
{
    public class clsOrders
    {
        int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public double TotalAmount { get; set; }
        public int StatusId { get; set; }
        public int ShippingAddressId { get; set; }
        public List<clsDTOs.OrderItemDTO> Items { get; set; }

        public clsDTOs.OrderDTO Order
        {
            get
            {
                return new clsDTOs.OrderDTO(
                this.OrderId, this.CustomerId, this.CreatedAt,
                this.TotalAmount, this.StatusId, this.ShippingAddressId, this.Items);
            }
        }

        public clsOrders()
        {

        }

        public clsOrders(clsDTOs.OrderDTO orderDTO)
        {
            this.OrderId = orderDTO.OrderId;
            this.CustomerId = orderDTO.OrderId;
            this.CreatedAt = orderDTO.CreatedAt;
            this.TotalAmount = orderDTO.TotalAmount;
            this.StatusId = orderDTO.StatusID;
            this.ShippingAddressId = orderDTO.ShippingAddressID;
            this.Items = orderDTO.Items;
        }

        public int PlaceOrder(List<OrderItemDTO> items)
        {

            if (this.Order == null)
                throw new ArgumentNullException(nameof(Order));

            if (items == null || items.Count == 0)
                throw new ArgumentException("Order must contain at least one item.");

            foreach (var item in items)
            {
                if (item.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero.");
            }

            return clsOrderData.PlaceOrder(Order, items);
        }

        public bool CancelOrder()
        {
            if (this.Order == null)
                throw new ArgumentNullException(nameof(Order));

            return clsOrderData.CancelOrder(this.Order);
        }

        public clsDTOs.OrderDTO GetOrdersByOrderId(int customerId)
        {
            return clsOrderData.GetOrderById(customerId);
        }

        public List<clsDTOs.OrderSummaryDTO> GetOrdersByCustomerId(int customerId)
        {
            return clsOrderData.GetAllOrdersByUser(customerId);

        }

        public bool UpdateOrderStatus()
        {
            if (this.Order.OrderId <= 0)
                throw new ArgumentException("Invalid order ID.");
            if (this.Order.StatusID <= 0 || this.Order.StatusID > 3 )
                throw new ArgumentException("Invalid status ID.");
            return clsOrderData.UpdateOrderStatus(this.Order);
        }
    }
}
