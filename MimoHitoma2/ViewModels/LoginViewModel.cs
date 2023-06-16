using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MimoHitoma2.ViewModels
{
    public class LoginViewModel
    {
        public string account { get; set; }
        public string password { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}