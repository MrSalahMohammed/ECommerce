using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class clsCustomers
    {
        public enum enMode { AddNewCustomer = 0, UpdateCustomer = 1, DeleteCustomer = 2 };

        public enMode Mode { get; set; }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        private string Role { get; }
        public DateTime CreatedAt { get; set; }
        public string HashPassword { get; set; }

        public clsDTOs.UserDTO CDTO
        {
            get { return (new clsDTOs.UserDTO(this.Id, this.FirstName, this.LastName, this.Email, this.Role, this.CreatedAt)); }
        }

        public clsCustomers(clsDTOs.UserDTO CDTO, enMode mode = enMode.AddNewCustomer)
        {
            this.Id = CDTO.Id;
            this.FirstName = CDTO.FirstName;
            this.LastName = CDTO.LastName;
            this.Email = CDTO.Email;
            this.Role = "Customer";
            this.CreatedAt = CDTO.CreatedAt;
            this.Mode = mode;
        }

        public static clsDTOs.UserDTO GetCustomerByID(int ID)
        {
            return clsCustomerData.GetCustomerByID(ID);
        }

        private bool _MakeAdmin()
        {
            if (CDTO == null)
                throw new ArgumentNullException(nameof(CDTO));

            this.Id = clsAdminData.AddNewAdmin(this.CDTO, General.userDTO, General.HashPassword);
            return (this.Id > 0);
        }

        public bool _AddNewCustomer()
        {
            this.Id = clsCustomerData.AddNewCustomer(CDTO, this.HashPassword);
            return (this.Id > 0);
        }

        private bool _UpdateCustomer()
        {
            if (CDTO == null)
                throw new ArgumentNullException(nameof(CDTO));

            return clsCustomerData.UpdateCustomer(this.CDTO, HashPassword);
        }

        private bool _DeleteCustomer()
        {
            if (CDTO == null)
                throw new ArgumentNullException(nameof(CDTO));

            return clsCustomerData.DeleteCustomer(CDTO);
        }

        public List<clsDTOs.AddressDTO> GetAddresses()
        {
            return clsCustomerData.GetAllCustomerAddresses(CDTO);
        }

        public void Delete()
        {
            this.Mode = enMode.DeleteCustomer;
        }

        public bool Save()
        {

            switch (this.Mode)
            {
                case enMode.AddNewCustomer:
                    if (_AddNewCustomer())
                    {
                        this.Mode = enMode.UpdateCustomer;
                        return true;
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to add new customer.");
                    }
                case enMode.UpdateCustomer:
                    return false;
                case enMode.DeleteCustomer: 
                    return true;
                default:
                    throw new InvalidOperationException("Invalid mode for saving customer.");
            }
             
            
        }

    }
}
