using DataAccessLayer;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{

    public class clsProducts
    {
        public enum enMode { AddNewProduct = 0, UpdateProduct = 1 , DeleteProduct = 2 };

        public enMode Mode { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Double Price { get; set; }
        public Double CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public clsDTOs.UserDTO CreatedBy { get; set; }

        private bool isActive { get; set; }

        public clsDTOs.ProductDTO PDTO
        {
            get { return (new clsDTOs.ProductDTO(this.Id, this.Name, this.Description, this.Price, this.CostPrice, this.StockQuantity, this.CreatedAt)); }
        }

        public clsProducts(clsDTOs.ProductDTO PDTO, clsDTOs.UserDTO User, enMode mode = enMode.AddNewProduct)
        {
            this.Id = PDTO.Id;
            this.Name = PDTO.Name;
            this.Description = PDTO.Description;
            this.Price = PDTO.Price;
            this.CostPrice = PDTO.CostPrice;
            this.StockQuantity = PDTO.StockQuantity;
            this.CreatedAt = PDTO.CreatedAt;
            this.Mode = mode;
            this.CreatedBy = User;
        }

        private bool _HasPermission()
        {
            if (PDTO == null)
                throw new ArgumentNullException(nameof(PDTO));

            return (this.CreatedBy.Role == "Admin");
        }

        private bool _AddNewProduct()
        {
            if (PDTO == null)
                throw new ArgumentNullException(nameof(PDTO));

            this.Id = clsProductsData.AddNewProduct(PDTO);
            return (this.Id > 0);

        }

        private bool _UpdateProduct()
        {
            if (PDTO == null)
                throw new ArgumentNullException(nameof(PDTO));

            return clsProductsData.UpdateProduct(PDTO);

        }

        private bool _DeleteProduct()
        {
            if (PDTO == null)
                throw new ArgumentNullException(nameof(PDTO));

            return clsProductsData.DeleteProduct(PDTO);
        }

        public void Delete()
        {
            if (!_HasPermission())
                throw new UnauthorizedAccessException(
                    "User does not have permission to perform this action.");

            this.Mode = enMode.DeleteProduct;
        }

        public static clsDTOs.ProductDTO FindProduct(int ID)
        {
            return clsProductsData.FindProductByID(ID);
        }

        public bool AddStock(int Quantity)
        {
            return clsProductsData.AddStock(this.PDTO, Quantity);
        }

        public bool Save()
        {

            if (!_HasPermission())
                throw new UnauthorizedAccessException(
                    "User does not have permission to perform this action.");

            switch (this.Mode)
            {
                case enMode.AddNewProduct:
                    if (_AddNewProduct())
                    {
                        this.Mode = enMode.UpdateProduct;
                        return true;
                    }
                    return false;

                case enMode.UpdateProduct:
                    return _UpdateProduct();

                case enMode.DeleteProduct:
                    return _DeleteProduct();

                default:
                    throw new InvalidOperationException("Invalid product mode.");

            }

        }

    }
}
