using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autoTaxi{
    public class Car{
        // Speed of each car in feet per second
        public const double speed = 36.6667; // 36.6667 fps = 25 mph

        // Running delivery time avg. Update every time you drop off a person.
        public double deliveryTimeAvg {
            get; private set;
        }

        // Passengers in/waiting for this car
        public List<Request> passengers {
            get; private set;
        }

        // Current position of the car
        public Position pos {
            get; private set;
        }

        // Constructor
        public Car(Position pos) {
            this.pos = pos;
            passengers = new List<Request>();
        }

        // Update current location using elapsed time, speed, and routes
        public void update(int elapsedTime) {
            double distanceTraveled = elapsedTime * speed;


        }
    }
}
