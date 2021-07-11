using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Entities
{
    public class Staff
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; }
        public Guid StaffLoginId { get; set; }
    }
}
