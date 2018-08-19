using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Digging_Game_3
{
    public class RigidBody
    {
        public Point3D position;
        public Vector3D velocity;
        public Vector3D force;
        public double mass = 1;
        public double fraction = 0;
        public void Update(double secs)
        {
            var preV = velocity;
            var f = force;
            if (fraction != 0)
            {
                f -= velocity * fraction * mass;
            }
            velocity += f / mass * secs;
            position += (preV + velocity) / 2 * secs;
        }
    }
}
