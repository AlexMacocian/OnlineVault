using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineVault.Models
{
    public class Domain
    {
        public string Name;
        public List<Entry> Entries = new List<Entry>();

        public Domain(String Name)
        {
            this.Name = Name;
        }
    }
}
