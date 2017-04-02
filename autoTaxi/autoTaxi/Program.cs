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

        private void testGreedySort() {
            List<Request> reqs = new List<Request>();
            Random rand = new Random();
            for (int i = 0; i < 15; i++) {
                reqs.Add(new Request(new Position(0, 0), new Position(0, rand.Next() % 100), 0));
            }
            Dispatcher.greedySort(reqs);

            for (int i = 0; i < 15; i++) {
                Console.WriteLine(reqs[i].end.y);
            }
        }

        private static void testDistance() {
            for(int i = 0; i < 5; i++) {
                for(int j = 0; j < 5; j++) {
                    Position temp = new Position(i, j);
                    Position temp2 = new Position(1, 1);
                    Console.WriteLine("Dist between " + temp.x + "," + temp.y + " and " + temp2.x + "," + temp2.y + " is " + Dispatcher.distance(temp, temp2));
                }
            }
        }
    }
}
