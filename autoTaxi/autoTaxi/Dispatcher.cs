using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi{
    class Dispatcher{
        private static List<List<Position>> permutations;

        //TODO: CONSIDER HOW TO HANDLE CAPACITY
        public static bool permutationAssign(List<Car> cars, Request newReq) {
            List<List<Position>> bestPermutations = new List<List<Position>>(); //one per car
            List<double> permutationLengthDelta = new List<double>(); //one per car

            foreach(Car c in cars) { //find best permutation for each car
                permutations = new List<List<Position>>(); //reset
                List<Position> endpoints = new List<Position>();
                endpoints.Add(newReq.start);
                foreach(Request r in c.requests) {
                    endpoints.Add(r.end); //init endpoints
                }
                List<Position> tmp = new List<Position>();
                tmp.Add(c.pos); //cars position must be in the permutation at point zero.
                generateLegalPermutations(tmp, endpoints, newReq.start, newReq.end); //stored in permutations var
                foreach(List<Position> perm in permutations) {
                    foreach(Position p in perm) {
                        Console.Write(p + " ");
                    }
                    Console.WriteLine();
                }
                double bestPermLength = double.MaxValue;
                int bestPermIndex = -1; //find index of shortest perm
                foreach(List<Position> perm in permutations) {
                    double routeLength = getRouteLength(perm);
                    if(routeLength < bestPermLength) { //new best permutation
                        bestPermLength = routeLength;
                        bestPermIndex = permutations.IndexOf(perm);
                    }
                }
                bestPermutations.Add(permutations[bestPermIndex]); //record perm
                List<Position> normalRoute = new List<Position>();
                normalRoute.Add(c.pos); //route must include car start point
                foreach(Request r in c.requests) {
                    normalRoute.Add(r.end);
                }
                permutationLengthDelta.Add(bestPermLength - getRouteLength(normalRoute)); //record length
                Console.Write("normal({0}): ", getRouteLength(normalRoute));
                foreach(Position p in normalRoute) { Console.Write(p + " "); }
                Console.Write("\nnew({0}): ", bestPermLength);
                foreach(Position p in permutations[bestPermIndex]) { Console.Write(p + " "); }
                Console.WriteLine("\n");
            }

            double bestLengthDelta = double.MaxValue;
            int bestCarIndex = -1; //find car w/best permutation delta
            foreach(double delta in permutationLengthDelta) { 
                if(delta < bestLengthDelta) {
                    bestCarIndex = permutationLengthDelta.IndexOf(delta);
                }
            }

            //sort cars route to match the best permutation
            Car bestCar = cars[bestCarIndex];
            List<Position> positions = bestPermutations[bestCarIndex];
            bestCar.requests.Add(newReq); //add new request dropoff
            bestCar.requests.Add(new Request(newReq.end, newReq.start, newReq.time, 0)); //add pickup
            for(int i = 0; i < bestCar.requests.Count - 1; i++) {
                Request temp = bestCar.requests[i];
                for(int j = i; j < bestCar.requests.Count; j++) {
                    if(bestCar.requests[j].end == positions[i]) { //request corresponding to next dropoff
                        temp = bestCar.requests[i]; //swap
                        bestCar.requests[i] = bestCar.requests[j];
                        bestCar.requests[j] = temp;
                        j = bestCar.requests.Count; //short circuit after swap
                    }
                }
            }
            return true; //always succeeds?
        }

        /// <summary>
        /// Method assumes pickup is also in the list of endpoints, generates all permutations s.t. dropoff comes after pickup.
        /// </summary>
        public static void generateLegalPermutations(List<Position> permutation, List<Position> endpoints, Position pickup, Position dropoff) {
            int permutationLength = (endpoints.Count + 1); //+1 to account for car.pos being in the permutation
            if(!endpoints.Contains(dropoff)) { //+1 when dropoff is not in endpoints
                permutationLength++; 
            }
            if(permutation.Count < permutationLength) { //create permutation using all endpoints + dropoff + car.pos
                List<Position> newEndpoints = endpoints;
                //if pickup is in the permutation and dropoff is not already in the endpoints add it to legal endpoints
                if(permutation.Contains(pickup) && !endpoints.Contains(dropoff)) {
                    newEndpoints = new List<Position>(endpoints);
                    newEndpoints.Add(dropoff);
                }

                /*
                foreach(Position pickup in pickups) {
                    Position dropoff = dropoffs[pickups.indexOf(pickup)] //parallel lists
                    if(permutation.Contains(pickup) && !permutation.Contains(dropoff)) {
                        newEndpoints.Add(dropoff);
                    }
                }
                */

                foreach(Position p in newEndpoints) { //create legal permutations recursively
                    if(!permutation.Contains(p)) { //if not in permutation, add it to a new permutation
                        List<Position> newPerm = new List<Position>(permutation); 
                        newPerm.Add(p);
                        generateLegalPermutations(newPerm, newEndpoints, pickup, dropoff);
                    }
                }
            } else {
                permutations.Add(permutation); //TODO MUST MAINTAIN LEGAL PERMUTATION FOR MULTIPLE PICKUPs & LEGAL CAPACITY.
            }
        }

        public static double getRouteLength(List<Position> route) {
            double length = 0;
            for(int i = 0; i < route.Count - 1; i++) {
                length += distance(route[i], route[i + 1]);
            }
            return length;
        }

        /// <summary>
        /// Assigns the new request to the car who's current route comes nearest to the request pickup point.
        /// If the car is over capacity it will not consider the first n segments.
        /// </summary>
        /// <returns>false if all cars are full, true otherwise</returns>
        public static bool closestPathAssign(List<Car> cars, Request newReq) {
            double closestPathDist = double.MaxValue;
            int closestPathIndex = -1;
            int bestCarIndex = -1;

            foreach(Car c in cars) {
                double deltaDistance; //distance from point to a line-segment
                if(c.requests.Count == 0) { //edge case: car has no requests
                    deltaDistance = Dispatcher.distance(c.pos, newReq.start);
                    updatePathIfBest(cars, ref closestPathDist, ref closestPathIndex, ref bestCarIndex, c, deltaDistance, 0);
                } else if(c.Passengers + newReq.passengers <= Car.capacity) { //if car has capacity for request
                    Position pathStart = c.pos; //start point of first line-segment starts at car
                    foreach(Request r in c.requests) { //find best time to pickup new request for each car
                        deltaDistance = pathPointDistance(newReq.start, pathStart, r.end);
                        updatePathIfBest(cars, ref closestPathDist, ref closestPathIndex, ref bestCarIndex, c, deltaDistance, c.requests.IndexOf(r));
                        pathStart = r.end; //set next line-segment start point
                    } //edge case: most convenient pickup time is after all passengers dropped off
                    deltaDistance = Dispatcher.distance(newReq.start, c.requests.Last().end);
                    updatePathIfBest(cars, ref closestPathDist, ref closestPathIndex, ref bestCarIndex, c, deltaDistance, c.requests.Count);
                } else { //edge case: overloaded car, drop passengers off first.
                    int passengers = c.Passengers + newReq.passengers;
                    for(int reqIndex = 0; reqIndex < c.requests.Count - 1; reqIndex++) {
                        passengers -= c.requests[reqIndex].passengers; //dropoff x passengers at request i
                        if(passengers <= Car.capacity) { //if room at this point in route, find distance
                            deltaDistance = pathPointDistance(newReq.start, c.requests[reqIndex].end, c.requests[reqIndex + 1].end);
                            updatePathIfBest(cars, ref closestPathDist, ref closestPathIndex, ref bestCarIndex, c, deltaDistance, reqIndex);
                        }
                    }
                }
            }

            assignRequest(cars[bestCarIndex], newReq, closestPathIndex);
            return true;
        }

        /// <summary>
        /// Updates the shortest distance, and indices of the best car/path
        /// </summary>
        private static void updatePathIfBest(List<Car> cars, ref double closestPathDist, ref int closestPathIndex, ref int bestCarIndex, Car c, double distance, int index) {
            if(distance < closestPathDist) {
                closestPathDist = distance;
                closestPathIndex = index;
                bestCarIndex = cars.IndexOf(c);
            }
        }

        /// <summary>
        /// calculates the shortest distance from a point to a line-segment. http://stackoverflow.com/a/1501725
        /// </summary>
        public static double pathPointDistance(Position point, Position start, Position end) {
            Position endStartDelta = end.distance(start); //(w - v)
            double segmentLengthSqr = endStartDelta.dotProduct(endStartDelta); //|w - v|^2
            if(segmentLengthSqr == 0.0) { //start == end
                return distance(point, start);
            }
            Position pointStartDelta = point.distance(start); //(p - v)
            double t = Math.Max(0, Math.Min(1, pointStartDelta.dotProduct(endStartDelta) / segmentLengthSqr));
            endStartDelta.scalarMultiply(t); // t * (w - v)
            Position projection = start.sum(endStartDelta); //v + [t * (w - v)]
            return distance(point, projection);
        }

        private static void assignRequest(Car car, Request req, int reqIndex) {
            car.Passengers += req.passengers; // Add passengers to the car
            car.requests.Add(req); // Add the dropoff request to the requests list
            Request mockRequest = new Request(req.end, req.start, -1, 0);
            req.Pickup = mockRequest;
            car.requests.Insert(reqIndex, mockRequest); //add pickup request
            nearestPathSort(car.requests, reqIndex);
        }

        // Takes a list of cars and a request, and decides which 
        // car should handle that request. It adds the appropriate 
        // requests to that car.
        public static bool greedyAssign(List<Car> cars, Request curRequest) {
            double shortestDist = double.PositiveInfinity;

            double dist;
            int bestCarIndex = -1;

            // Find the index of the car that is closest to the request start point 
            for (int i = 0; i < cars.Count; i++) {
                dist = distance(cars[i].pos, curRequest.start);
                if (dist < shortestDist && cars[i].Passengers + curRequest.passengers <= Car.capacity) {
                    shortestDist = dist;
                    bestCarIndex = i;
                }
            }

            // If best car index remains -1, then no car has the capacity to pick up the request
            if (bestCarIndex == -1) {
                return false;
            }
            else {
                Car bestCar = cars[bestCarIndex];
                bestCar.Passengers += curRequest.passengers; // Add passengers to the car
                bestCar.requests.Add(curRequest); // Add the request to the requests list

                greedySort(cars[bestCarIndex]);
                return true;
            }
        }

        public static double distance(Position start, Position end) {
            return Math.Sqrt(Math.Pow((start.x - end.x), 2.0) + Math.Pow((start.y - end.y), 2.0));
        }

        /* 
         * Assumes that the first item is the closest request to the car.
         * Starts at index one and sort the array based on what's closest 
         * to the previous item
         */
        public static void greedySort(Car car) {
            // find closest pickup or dropoff to car's current position
            //shortestDist = distance(bestCar.pos, bestCar.requests[0].needsPickedUp ? bestCar.requests[0].start : bestCar.requests[0].end);
            if(car.requests.Count <= 1) {
                return;
            }
            double shortestDist = double.PositiveInfinity;
            int bestIndex = 0;
            for (int i = 0; i < car.requests.Count; i++) {
                double tempDist = distance(car.pos, car.requests[i].needsPickedUp ? car.requests[i].start : car.requests[i].end);
                if (tempDist < shortestDist) {
                    bestIndex = i;
                    shortestDist = tempDist;
                }
            }
            // Swap first item with closest item to car
            Request temp = car.requests[0];
            car.requests[0] = car.requests[bestIndex];
            car.requests[bestIndex] = temp;

            int shortestDistIndex = 0;
            int curPoint = 0;
            List<Request> requests = car.requests;

            for (; curPoint < requests.Count - 1; curPoint++) {
                shortestDist = double.PositiveInfinity;
                for (int j = curPoint + 1; j < requests.Count; j++) {
                    double tempDistance = distance(requests[curPoint].needsPickedUp ? requests[curPoint].start : requests[curPoint].end,
                        requests[j].needsPickedUp ? requests[j].start : requests[j].end);

                    if (tempDistance < shortestDist) {
                        shortestDist = tempDistance;
                        shortestDistIndex = j;
                    }
                }
                if (curPoint + 1 != shortestDistIndex) { //swap if they're not the same
                    temp = requests[curPoint + 1];
                    requests[curPoint + 1] = requests[shortestDistIndex];
                    requests[shortestDistIndex] = temp;
                }
            }
        }

        /// <summary>
        /// greedily sorts requests starting from supplied index + 1 while maintaining that pickups
        /// come before dropoffs.
        /// </summary>
        public static void nearestPathSort(List<Request> requests, int index) {
            List<Request> legalChoices = new List<Request>();
            List<Request> illegalChoices = new List<Request>();
            findValidChoices(requests, legalChoices, illegalChoices, index + 1);
            Position curPos = requests[index].end;

            for(int i = index + 1; i < requests.Count - 1; i++) { //i = swap index
                double nearestPointDist = double.MaxValue;
                Request nearestRequest = null;

                foreach(Request r in legalChoices) { //pick best legal choice
                    double distance = Dispatcher.distance(curPos, r.end);
                    if(distance < nearestPointDist) {
                        nearestPointDist = distance;
                        nearestRequest = r;
                    } 
                }

                //swap position of best request with curReq, update curPos & choices
                Request swap = requests[i];
                int indexOfBest = requests.IndexOf(nearestRequest);
                requests[i] = nearestRequest;
                requests[indexOfBest] = swap;
                curPos = nearestRequest.end;
                legalChoices.Remove(nearestRequest);

                //check for new legal choices
                foreach(Request r in illegalChoices) {
                    if(nearestRequest.end == r.start) {
                        legalChoices.Add(r);
                        illegalChoices.Remove(r);
                        break;
                    }
                }
            }
        }

        private static void findValidChoices(List<Request> requests, List<Request> legalChoices, List<Request> illegalChoices, int index) {
            for(int i = index; i < requests.Count; i++) { //index of first choice to sort
                if(requests[i].passengers == 0) { //pickup request is legal destination
                    legalChoices.Add(requests[i]);
                } else if(requests.Contains(requests[i].Pickup) && //dropoff before pickup is illegal
                    requests.IndexOf(requests[i].Pickup) >= index) { //unless pickup comes before
                    illegalChoices.Add(requests[i]);
                } else {
                    legalChoices.Add(requests[i]);
                }
            }
        }
    }
}

