using BusinessLayer;
using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppTest
{
    internal class ConsoleTest
    {

        [Conditional("DEBUG")]
        public static void DisplayAllCustomers()
        {
            try
            {
                List<clsDTOs.UserDTO> CustomersList = clsAdminData.GetAllCustomersData();

                foreach (clsDTOs.UserDTO customer in CustomersList)
                {
                    Console.WriteLine($"\nID: " + customer.Id + " \nFirst Name: " +
                        customer.FirstName + " \nLast Name: " + customer.LastName +
                        " \nEmail: " + customer.Email + " \nCreated At: " + customer.CreatedAt);
                }
            }
            catch(Exception ex) {
                Console.WriteLine("[ERROR] " + ex.Message);
            }
            
            
        }

        public static void DisplayCustomerByID(int ID)
        {

            clsDTOs.UserDTO customer = clsCustomers.GetCustomerByID(ID);

            if (customer == null)
            {
                Console.WriteLine("Customer with ID " + ID + " not found.");
                return;
            }

            Console.WriteLine($"\nID: " + customer.Id + " \nFirst Name: " +
                        customer.FirstName + " \nLast Name: " + customer.LastName +
                        " \nEmail: " + customer.Email + " \nCreated At: " + customer.CreatedAt);
        } 

        public static void AddNewCustomer()
        {
            int newCustomerID = 0;
            string FirstName = "Mohammed";
            string LastName = "Omar";
            string Email = "Mohammed@gmail.com";
            string Role = "Customer";
            DateTime CreatedAt = DateTime.Now;
            string Password = "password123";

            clsCustomers newCustomer = new clsCustomers(new clsDTOs.UserDTO(newCustomerID, FirstName, LastName, Email, Role, CreatedAt));
            newCustomer.HashPassword = Password;

            newCustomer.Save();

            Console.WriteLine("Customer Added Successfully With ID = " + newCustomer.Id);
        }



        static void Main(string[] args)
        {

            //DisplayAllCustomers();

            DisplayCustomerByID(5);

            //AddNewCustomer();

        }
    }
}
