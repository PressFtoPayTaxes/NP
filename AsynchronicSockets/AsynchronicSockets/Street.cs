using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsynchronicSockets
{
    public class Street
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Postcode { get; set; }
        public string Name { get; set; }
    }
}
