using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi {
    class Program {
        static void Main(string[] args) {
            List<Request> requests = Request.generateRequests(5, 10000, 69);
            Console.WriteLine(10000.0 / requests.Count);
        }
    }
}
