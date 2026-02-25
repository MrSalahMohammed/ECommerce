using BusinessLayer;
using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DataAccessLayer.clsDTOs;

namespace ConsoleAppTest
{
    internal class clsScreens
    {
        private static void ClearScreen()
        {
            Console.Clear();
        }

        public static void Start()
        {
            ClearScreen();
            bool InvalidOption = false;

            Console.WriteLine("============ Welcome! ============");
            Console.WriteLine("\nPlease select an option:\n\n");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. Exit");
            Console.Write("\n\n[Answer]: ");
            string Option = Console.ReadLine().Trim();

            switch (Option)
            {
                case "1":
                    LoginScreen();
                    break;
                case "2":
                    RegisterScreen();
                    break;
                case "3":
                    Console.WriteLine("\nThank you for using our application! Goodbye!");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid Option! Please select a valid option.");
                    InvalidOption = true;
                    Thread.Sleep(1500);
                    break;
            }

            if (InvalidOption)
            {
                Start();
            }

        }

        public static void LoginScreen()
        {
            ClearScreen();

            Console.WriteLine("============ Login Screen ============");
            Console.WriteLine("\nPlease enter your email and password to login:\n\n");

            clsDTOs.UserDTO userDTO = null;
            string Retry = "Y";
            bool Found = false;
            string Email = "";
            string Password = "";

            while (Retry == "Y")
            {

                Console.Write("Email: ");
                Email = Console.ReadLine().ToLower();
                Console.Write("Password: ");
                Password = Console.ReadLine().ToLower();

                try
                {
                    userDTO = General.Login(Email, Password);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR] " + ex.Message);
                }


                if (userDTO == null)
                {
                    Console.WriteLine("\nLogin Failed! Please check your email and password.");
                    Console.Write("\nDo you want to retry? (Y/N): ");
                    Retry = Console.ReadLine().ToString().ToUpper().Trim();
                    Console.WriteLine();
                }
                else
                {
                    Found = true;
                    break;
                }

            }

            if (Found)
            {
                Console.WriteLine("\nLogin Successfully!");

                try
                {
                    General.CurrentCustomer = new clsCustomers(userDTO);
                    General.HashPassword = Password;
                    General.CurrentCustomer.Addresses = General.CurrentCustomer.GetAddresses();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n[Warning] " + ex.Message);
                }

                Thread.Sleep(3000);
                CustomerMainMenu();

            }

        }

        public static void RegisterScreen()
        {
            ClearScreen();
            Console.WriteLine("============ Register Screen ============");
            Console.WriteLine("\nPlease Enter your details to register:\n\n");

            Console.WriteLine("First Name: ");
            string FirstName = Console.ReadLine().Trim();
            Console.WriteLine("Last Name: ");
            string LastName = Console.ReadLine().Trim();
            Console.WriteLine("Email: ");
            string Email = Console.ReadLine().Trim();
            Console.WriteLine("Password: ");
            string Password = Console.ReadLine().Trim();

            try
            {
                General.userDTO.FirstName = FirstName;
                General.userDTO.LastName = LastName;
                General.userDTO.Email = Email;
                General.HashPassword = Password;

                if (General.userDTO.FirstName == "" || General.userDTO.LastName == "" || General.userDTO.Email == "" || General.HashPassword == "")
                {
                    throw new Exception("All fields are required! Please fill in all the details.");
                }

                General.CurrentCustomer = new clsCustomers(General.userDTO);
                General.CurrentCustomer.HashPassword = Password;
                General.CurrentCustomer.Save();

                Console.WriteLine("\nRegistration Successful! You can now login with your credentials.");
                Thread.Sleep(1500);
                CustomerMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] " + ex.Message);
            }
        }

        public static void Logout()
        {
            General.CurrentCustomer = null;
            General.HashPassword = null;
            Console.WriteLine("\nYou have been logged out successfully!");
            Thread.Sleep(3000);
            Start();
        }

        public static void AdminScreen()
        {
            Console.WriteLine("============ Welcome to the Admin Screen! ============");
            Console.WriteLine("Please select an option:\n\n");
            Console.WriteLine("1. Display All Customers");
            Console.WriteLine("2. Display Customer By ID");
            Console.WriteLine("3. Add New Customer");
            Console.WriteLine("4. Update Customer");
            Console.WriteLine("5. Delete Customer");
            Console.WriteLine("6. Logout");
            string Option = Console.ReadLine().Trim();
            switch (Option)
            {
                case "1":
                    ClearScreen();
                    ConsoleTest.DisplayAllCustomers();
                    break;
                case "2":
                    ClearScreen();
                    Console.Write("Enter Customer ID: ");
                    int ID = int.Parse(Console.ReadLine());
                    ConsoleTest.DisplayCustomerByID(ID);
                    break;
                case "3":
                    ClearScreen();
                    ConsoleTest.AddNewCustomer();
                    break;
                case "4":
                    ClearScreen();
                    //ConsoleTest.UpdateCustomer();
                    break;
                case "5":
                    ClearScreen();
                    //ConsoleTest.DeleteCustomer();
                    break;
                case "6":
                    ClearScreen();
                    Start();
                    break;
                default:
                    Console.WriteLine("Invalid Option! Please select a valid option.");
                    break;
            }

        }

        public static void CustomerMainMenu()
        {
            bool InvalidOption = false;

            while (!InvalidOption)
            {
                ClearScreen();
                Console.WriteLine("============ Main Menu ============");
                Console.WriteLine("\nPlease select an option:\n\n");
                Console.WriteLine("1. Profile");
                Console.WriteLine("2. Store");
                Console.WriteLine("3. Cart");
                Console.WriteLine("4. Logout");
                Console.Write("\n\n[Answer]: ");
                string Option = Console.ReadLine().Trim();
                switch (Option)
                {
                    case "1":
                        CustomerProfileScreen();
                        break;
                    case "2":
                        StoreScreen();
                        break;
                    case "3":
                        CartScreen();
                        break;
                    case "4":
                        Logout();
                        break;
                    default:
                        Console.WriteLine("\nInvalid Option! Please select a valid option.");
                        InvalidOption = true;
                        Thread.Sleep(1500);
                        break;
                }
            }
        }

        public static void CustomerProfileScreen()
        {
            ClearScreen();
            Console.WriteLine("============ Profile ============");
            Console.WriteLine("\n\n1. View Profile Details");
            Console.WriteLine("2. Update Profile Details");
            Console.WriteLine("3. Back to Main Menu");
            Console.Write("\n\n[Answer]: ");
            string Option = Console.ReadLine().Trim();
            switch (Option)
            {
                case "1":
                    ViewProfileDetails();
                    break;
                case "2":
                    UpdateProfileDetails();
                    break;
                case "3":
                    ClearScreen();
                    CustomerMainMenu();
                    break;
                default:
                    Console.WriteLine("\nInvalid Option! Please select a valid option.");
                    Thread.Sleep(1500);
                    CustomerProfileScreen();
                    break;
            }

        }

        public static void ViewProfileDetails()
        {
            ClearScreen();
            Console.WriteLine("============ Profile Details ============");
            Console.WriteLine("\nFirst Name: " + General.CurrentCustomer.FirstName);
            Console.WriteLine("Last Name: " + General.CurrentCustomer.LastName);
            Console.WriteLine("Email: " + General.CurrentCustomer.Email);
            Console.WriteLine("Created At: " + General.CurrentCustomer.CreatedAt);
            Console.WriteLine("\nPress any key to go back to Profile Screen...");
            Console.ReadKey();
            CustomerProfileScreen();
        }

        public static void UpdateProfileDetails()
        {
            ClearScreen();
            Console.WriteLine("============ Update Profile Details ============");
            Console.WriteLine("\nPlease enter your new details:\n\n");
            Console.Write("First Name: ");
            string FirstName = Console.ReadLine().Trim();
            Console.Write("Last Name: ");
            string LastName = Console.ReadLine().Trim();
            Console.Write("Email: ");
            string Email = Console.ReadLine().Trim();
            try
            {
                General.CurrentCustomer.FirstName = FirstName;
                General.CurrentCustomer.LastName = LastName;
                General.CurrentCustomer.Email = Email;
                General.CurrentCustomer.Mode = clsCustomers.enMode.UpdateCustomer;
                if (General.CurrentCustomer.FirstName == "" || General.CurrentCustomer.LastName == "" || General.CurrentCustomer.Email == "")
                {
                    throw new Exception("All fields are required! Please fill in all the details.");
                }
                General.CurrentCustomer.Save();
                Console.WriteLine("\nProfile Updated Successfully!");
                Console.WriteLine("\nPress any key to go back to Profile Screen...");
                Console.ReadKey();

            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] " + ex.Message);
            }
            Thread.Sleep(3000);
            CustomerProfileScreen();

        }

        public static void StoreScreen()
        {
            ClearScreen();
            Console.WriteLine("============ Store ============");
            Console.WriteLine("\n\n1. View All Products");
            Console.WriteLine("2. Search Product");
            Console.WriteLine("3. Back to Main Menu");
            Console.Write("\n\n[Answer]: ");
            string Option = Console.ReadLine().Trim();
            switch (Option)
            {
                case "1":
                    //ViewAllProducts();
                    break;
                case "2":
                    //SearchProduct();
                    break;
                case "3":
                    ClearScreen();
                    CustomerMainMenu();
                    break;
                default:
                    Console.WriteLine("\nInvalid Option! Please select a valid option.");
                    Thread.Sleep(1500);
                    StoreScreen();
                    break;
            }
        }

        public static void CartScreen()
        {
            ClearScreen();
            Console.WriteLine("============ Cart ============");
            Console.WriteLine("\n\n1. View Cart Items");
            Console.WriteLine("2. Checkout");
            Console.WriteLine("3. Back to Main Menu");
            Console.Write("\n\n[Answer]: ");
            string Option = Console.ReadLine().Trim();
            switch (Option)
            {
                case "1":
                    //ViewCartItems();
                    break;
                case "2":
                    //Checkout();
                    break;
                case "3":
                    ClearScreen();
                    CustomerMainMenu();
                    break;
                default:
                    Console.WriteLine("\nInvalid Option! Please select a valid option.");
                    Thread.Sleep(1500);
                    CartScreen();
                    break;
            }
        }
    }
}
