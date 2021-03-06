﻿using System.Collections.Generic;

namespace AirTransit_Core.Models
{
    public class Contact
    {
        public Contact() { }
        public Contact(string phoneNumber, string name)
        {
            PhoneNumber = phoneNumber;
            Name = name;
        }
        
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string PublicKey { get; set; }

        public override string ToString()
        {
            return $"{Name} ({PhoneNumber})";
        }
    }
}
