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

            public UserDTO(int id, string firstName, string lastName, string email, string role, DateTime CreatedAt)
            {
                this.Id = id;
                this.FirstName = firstName;
                this.LastName = lastName;
                this.Email = email;
                this.Role = role;
                this.CreatedAt = CreatedAt;
            }
        }

        public class AddressDTO
        {
            public int AddressID { get; set; }
            public string CountryName { get; set; }
            public int CityID { get; set; }
            public string CityName { get; set; }
            public string AdditionalInfo { get; set; }

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

    }
}
