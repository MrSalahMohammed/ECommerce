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
        public static clsDTOs.UserDTO userDTO = null;
        public static string HashPassword { get; set; } 
    }
}
