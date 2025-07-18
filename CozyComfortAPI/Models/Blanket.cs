using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CozyComfortAPI.Models
{
    public class Blanket
    {
        public int BlanketID { get; set; }
        public string Model { get; set; }
        public string Material { get; set; }
        public int ProductionCapacity { get; set; }
    }
}