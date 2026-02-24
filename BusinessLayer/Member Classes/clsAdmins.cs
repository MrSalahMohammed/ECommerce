using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    internal class clsAdmins
    {

        public enum enMode { AddNewAdmin = 0, UpdateAdmin = 1, DeleteAdmin = 2 };

        public enMode Mode { get; set; }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        private string Role { get; }
        public DateTime CreatedAt { get; set; }
        public string HashPassword { get; set; }

        public clsDTOs.UserDTO ADTO
        {
            get { return (new clsDTOs.UserDTO(this.Id, this.FirstName, this.LastName, this.Email, this.Role, this.CreatedAt)); }
        }

        public clsAdmins(clsDTOs.UserDTO CDTO, enMode mode = enMode.AddNewAdmin)
        {
            this.Id = CDTO.Id;
            this.FirstName = CDTO.FirstName;
            this.LastName = CDTO.LastName;
            this.Email = CDTO.Email;
            this.Role = "Admin";
            this.CreatedAt = CDTO.CreatedAt;
            this.Mode = mode;
        }

        public static List<clsDTOs.UserDTO> GetAllCustomers()
        {
            return clsAdminData.GetAllCustomersData();
        }

        private bool _AddNewAdmin()
        {
            clsDTOs.UserDTO NewAdmin = new clsDTOs.UserDTO(0, this.FirstName, this.LastName, this.Email, this.Role, DateTime.Now);

            if (ADTO == null)
                throw new ArgumentNullException(nameof(ADTO));

            this.Id = clsAdminData.AddNewAdmin(NewAdmin, ADTO, HashPassword);
            return (this.Id > 0);
        }

        private bool _HasPermission()
        {
            if (ADTO == null)
                throw new ArgumentNullException(nameof(ADTO));

            return (this.Role == "Admin");
        }

        private bool _UpdateAdmin()
        {
            if (ADTO == null)
                throw new ArgumentNullException(nameof(ADTO));

            return clsAdminData.UpdateAdmin(this.ADTO, HashPassword);
        }

        private bool _DeleteAdmin()
        {
            if (ADTO == null)
                throw new ArgumentNullException(nameof(ADTO));

            return clsAdminData.DeleteAdmin(ADTO);
        }

        public void Delete()
        {
            if (!_HasPermission())
                throw new UnauthorizedAccessException(
                    "User does not have permission to perform this action.");

            this.Mode = enMode.DeleteAdmin;
        }

        public bool Save()
        {
            if (!_HasPermission())
                throw new UnauthorizedAccessException(
                    "User does not have permission to perform this action.");

            switch (this.Mode)
            {
                case enMode.AddNewAdmin:
                    if (_AddNewAdmin())
                    {
                        this.Mode = enMode.UpdateAdmin;
                        return true;
                    }
                    return false;

                case enMode.UpdateAdmin:
                    if (_UpdateAdmin())
                    {
                        this.Mode = enMode.UpdateAdmin;
                        return true;
                    }
                    return false;

                case enMode.DeleteAdmin:
                    if (_DeleteAdmin())
                    {
                        this.Mode = enMode.UpdateAdmin;
                        return true;
                    }
                    return false;

                default:
                    throw new InvalidOperationException("Invalid Admin mode.");

            }
        }

    }
}
