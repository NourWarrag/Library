using System;
using System.Security.AccessControl;

namespace Library.API.Entities
{
    public class StaffLogin
    {
        public Guid Id { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
    }
}
