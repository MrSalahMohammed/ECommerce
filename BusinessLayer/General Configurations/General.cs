using BusinessLayer.Member_Classes;
using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class General
    {
        public static clsDTOs.UserDTO userDTO = new clsDTOs.UserDTO();

        public static clsDTOs.AddressDTO UserAddressDTO = null;

        public static clsCustomers CurrentCustomer = new clsCustomers();

        public static clsDTOs.OrderDTO OrderDTO = null;

        public static clsOrders CurrentOrder = new clsOrders();

        public static string HashPassword { get; set; }


        public static clsDTOs.UserDTO Login(string Email, string Password)
        {
            clsDTOs.UserDTO LoginUserDTO = clsCustomerData.Login(Email, Password);

            if (LoginUserDTO != null)
            {
                HashPassword = Password;
                userDTO = LoginUserDTO;
                return userDTO;
            }
            else
            {
                return null;
            }
        }
    }
}
