using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CozyComfortAPI.Models
{
    public class UserDetails
    {
        public int UserId { get; set; }
        public char UserType { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; } 
        public string ContactPerson { get; set; } 
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public int ProductionCapacity { get; set; } 
        public string LicenseNumber { get; set; } 
        public string Website { get; set; } 
        public string Error { get; set; } 
    }

}