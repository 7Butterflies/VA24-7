using System;
using System.Collections.Generic;
using System.Text;

namespace VA24_7_Shared.Model
{
    public class Person
    {
        public Guid PersonId { get; set; }
        public string FullName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string B2CObjectId { get; set; }
        public string Role { get; set; }
    }
}
