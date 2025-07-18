using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CozyComfortAPI.Models
{
    public class OrderOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int OrderId { get; set; }
    }
}
