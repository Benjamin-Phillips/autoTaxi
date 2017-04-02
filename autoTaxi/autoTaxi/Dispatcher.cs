using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi{
    class Dispatcher{

        // Takes a list of cars and a request, and decides which 
        // car should handle that request. It adds the appropriate 
        // requests to that car.
        public static bool greedy(List<Car> cars, Request curRequest) {
            double shortestDist = double.PositiveInfinity;

            double dist;
            int bestCarIndex = -1;

            for(int i = 0; i < cars.Count; i++) {
                dist = distance(cars[i].pos, curRequest.start);
                if(dist < shortestDist && cars[i].Passengers + curRequest.passengers <= Car.capacity) {
                    shortestDist = dist;
                    bestCarIndex = i;
                }
            }

            if(bestCarIndex == -1) {
                return false;
            } else { 
                cars[bestCarIndex].Passengers += curRequest.passengers;
                cars[bestCarIndex].requests.Insert(0, new Request(new Position(0, 0), curRequest.start, curRequest.time, 0));
                cars[bestCarIndex].requests.Add(curRequest);
                greedySort(cars[bestCarIndex].requests);
                return true;
            }
        }

        public static double distance(Position start, Position end) {
            return Math.Sqrt(Math.Pow((start.x - end.x), 2.0) + Math.Pow((start.y - end.y), 2.0));
        }

        /* 
         * Assumes that the first index is taken by the request we are picking up 
         * immediately. Sorts the rest of the list based on closest distance relative
         * to the current request being considered on each iteration.
         */
        public static void greedySort(List<Request> requests) {
            int shortestDistIndex;
            double shortestDistance;

            for(int curPoint = 0; curPoint < requests.Count - 1; curPoint++) {
                shortestDistIndex = curPoint + 1;
                shortestDistance = distance(requests[curPoint].end, requests[shortestDistIndex].end);
                for(int j = curPoint + 1; j < requests.Count; j++) {
                    double tempDistance = distance(requests[j].end, requests[curPoint].end);
                    if (tempDistance < shortestDistance) {
                        shortestDistance = tempDistance;
                        shortestDistIndex = j;
                    } 
                }
                if(curPoint + 1 != shortestDistIndex) { //swap if they're not the same
                    Request temp = requests[curPoint + 1];
                    requests[curPoint + 1] = requests[shortestDistIndex];
                    requests[shortestDistIndex] = temp;
                }
            }
        }
    }
}
