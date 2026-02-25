using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class clsDTOs
    {
        public class UserDTO
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Role { get; }
            public DateTime CreatedAt { get; set; }

            public UserDTO() { this.Role = "Customer"; this.CreatedAt = DateTime.Now; }

            public UserDTO(int id, string firstName, string lastName, string email, string role = "Customer", DateTime? createdAt = null)
            {
                this.Id = id;
                this.FirstName = firstName;
                this.LastName = lastName;
                this.Email = email;
                this.Role = role;
                this.CreatedAt = createdAt ?? DateTime.Now;
            }
        }

        public class AddressDTO
        {
            public int AddressID { get; set; }
            public string CountryName { get; set; }
            public int CityID { get; set; }
            public string CityName { get; set; }
            public string AdditionalInfo { get; set; }

            public AddressDTO() { }

            public AddressDTO(int AddressID, int CityID, string CountryName, string CityName, string AdditionalInfo)
            {
                this.AddressID = AddressID;
                this.CityID = CityID;
                this.CountryName = CountryName;
                this.CityName = CityName;
                this.AdditionalInfo = AdditionalInfo;
            }
        }

        public class ProductDTO
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public Double Price { get; set; }
            public Double CostPrice { get; set; }
            public int StockQuantity { get; set; }
            public DateTime CreatedAt { get; set; }
            public ProductDTO(int id, string name, string description, Double price, Double CostPrice, int stockQuantity, DateTime createdAt)
            {
                this.Id = id;
                this.Name = name;
                this.Description = description;
                this.Price = price;
                this.StockQuantity = stockQuantity;
                this.CreatedAt = createdAt;
                this.CostPrice = CostPrice;
            }
        }

        public class OrderItemDTO
        {
            public int ProductID { get; set; }
            public int Quantity { get; set; }

            public OrderItemDTO(int productID, int quantity)
            {
                this.ProductID = productID;
                this.Quantity = quantity;
            }
        }

        public class OrderDTO
        {
            public int OrderId { get; set; }
            public int CustomerID { get; set; }
            public DateTime CreatedAt { get; set; }
            public double TotalAmount { get; set; }
            public int StatusID { get; set; }
            public int ShippingAddressID { get; set; }

            public List<OrderItemDTO> Items { get; set; }

            public OrderDTO(int id, int customerID, DateTime orderDate, double totalAmount, int statusID, int shippingAddressID, List<OrderItemDTO> Items)
            {
                this.OrderId = id;
                this.CustomerID = customerID;
                this.CreatedAt = orderDate;
                this.TotalAmount = totalAmount;
                StatusID = statusID;
                ShippingAddressID = shippingAddressID;
                this.Items = Items;
            }

            public void AddItem(OrderItemDTO item)
            {
                Items.Add(item);
            }
        }

        public class OrderSummaryDTO
        {
            public int OrderID { get; set; }
            public DateTime CreatedAt { get; set; }
            public decimal TotalAmount { get; set; }
            public string StatusName { get; set; }

            public OrderSummaryDTO(int orderID, DateTime createdAt, decimal totalAmount, string statusName)
            {
                OrderID = orderID;
                CreatedAt = createdAt;
                TotalAmount = totalAmount;
                StatusName = statusName;
            }
        }

    }
}
