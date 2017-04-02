using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi{
    public class Car{
        // Speed of each car in feet per second
        public const double speed = 36.6667; // 36.6667 fps = 25 mph
        public const int capacity = 4;
        private int passengers = 0;
        private static int id = 0;

        // Running delivery time avg. Update every time you drop off a person.
        public double deliveryTimeAvg {
            get; private set;
        }

        // Passengers in/waiting for this car
        public List<Request> requests {
            get; private set;
        }

        // Current position of the car
        public Position pos {
            get; set;
        }

        public int Id {
            get; private set;
        }

        public int Passengers {
            get {
                return passengers;
            }
            set {
                if (value >= 0) {
                    passengers = value;
                }
            }
        }

        // Constructor
        public Car(Position pos) {
            this.pos = pos;
            Id = id;
            id++;
            requests = new List<Request>();
        }

        public override string ToString() {
            return string.Format("id: {0}, pos: {1:f}, Psngr: {2}", Id, pos, Passengers);
        }
    }
}
