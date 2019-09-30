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
        public RigidBody() { }
        public RigidBody(RigidBody rb)
        {
            position = rb.position;
            velocity = rb.velocity;
            force = rb.force;
            mass = rb.mass;
            theta = rb.theta;
            omega = rb.omega;
            alpha = rb.alpha;
            momentOfInertia = rb.momentOfInertia;
        }
        public Point3D position;
        public Vector3D velocity;
        public Vector3D force;
        public double mass = 1;

        public double theta = 0;
        public double omega = 0;
        public double alpha = 0;
        public double momentOfInertia = 1.0 / 6*10; //rectangle's inertia: 1/12 m * (w*2 + h*2)

        public Point3D _position;
        public Vector3D _velocity;
        public Vector3D _force;
        public double _theta;
        public double _omega;
        public double _alpha;

        //public void ApplyFraction(ref Vector3D f,double fraction)
        //{
        //    f = -velocity * fraction * mass;
        //}
        void BackUp()
        {
            _position = position;
            _velocity = velocity;
            _force = force;
            _theta = theta;
            _omega = omega;
            _alpha = alpha;
        }
        void Restore()
        {
            position = _position;
            velocity = _velocity;
            force = _force;
            theta = _theta;
            omega = _omega;
            alpha = _alpha;
        }
        public Vector3D GetVelocityAt(Point3D relativePosition)
        {
            var mat = new Matrix3D();
            mat.Rotate(new Quaternion(new Vector3D(0, 0, 1), theta / Math.PI * 180));
            return velocity + Vector3D.CrossProduct(relativePosition * mat - new Point3D(), new Vector3D(0, 0, -1)) * omega;
        }
        public bool Update(double secs,Func<RigidBody,bool>accepted=null)
        {
            BackUp();
            {
                var preV = velocity;
                var f = force;
                velocity += f / mass * secs;
                //if (appliedFraction != 0) velocity -= velocity * appliedFraction * secs;
                position += (preV + velocity) / 2 * secs;
            }
            {
                var preV = omega;
                var f = alpha;
                omega += f / momentOfInertia * secs;
                theta += (preV + omega) / 2 * secs;
                theta %= 2.0 * Math.PI;
            }
            if (accepted == null || accepted(this)) return true;
            Restore();
            return false;
        }
    }
}
