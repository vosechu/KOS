using System;
using System.Linq;
using UnityEngine;
using kOS.Values;

namespace kOS
{
    public static class SteeringHelper
    {
        public static void KillRotation(this Vessel vessel, FlightCtrlState c)
        {
            var act = vessel.transform.InverseTransformDirection(vessel.rigidbody.angularVelocity).normalized;
            
            c.pitch = act.x;
            c.roll = act.y;
            c.yaw = act.z;

            c.killRot = true;
        }

        public static void SteerShipToward(this Vessel vessel, Direction targetDir, FlightCtrlState c)
        {
            // I take no credit for this, this is a stripped down, rearranged version of MechJeb's attitude control system
            var coM = vessel.findWorldCenterOfMass();
            var moI = vessel.findLocalMOI(coM);

            var target = targetDir.Rotation;
            var vesselR = vessel.transform.rotation;

            var delta = Quaternion.Inverse(Quaternion.Euler(90, 0, 0) * Quaternion.Inverse(vesselR) * target);

            var deltaEuler = ReduceAngles(delta.eulerAngles);
            deltaEuler.y *= -1;

            var torque = vessel.GetTorque(c.mainThrottle);
            var inertia = vessel.GetEffectiveInertia(torque);

            var err = deltaEuler * Math.PI / 180.0F;
            err += new Vector3d(inertia.x, inertia.z, inertia.y);

            var act = 120.0f * err;

            var precision = Mathf.Clamp((float)torque.x * 20f / moI.magnitude, 0.5f, 10f);
            var driveLimit = Mathf.Clamp01((float)(err.magnitude * 380.0f / precision));

            act.x = Mathf.Clamp((float)act.x, -driveLimit, driveLimit);
            act.y = Mathf.Clamp((float)act.y, -driveLimit, driveLimit);
            act.z = Mathf.Clamp((float)act.z, -driveLimit, driveLimit);

            c.roll = Mathf.Clamp((float)(c.roll + act.z), -driveLimit, driveLimit);
            c.pitch = Mathf.Clamp((float)(c.pitch + act.x), -driveLimit, driveLimit);
            c.yaw = Mathf.Clamp((float)(c.yaw + act.y), -driveLimit, driveLimit);
        }

        private static Vector3d Pow(Vector3d vector, float exponent)
        {
            return new Vector3d(Math.Pow(vector.x, exponent), Math.Pow(vector.y, exponent), Math.Pow(vector.z, exponent));
        }

        private static Vector3d GetEffectiveInertia(this Vessel vessel, Vector3d torque)
        {
            var coM = vessel.findWorldCenterOfMass();
            var moI = vessel.findLocalMOI(coM);
            var angularVelocity = Quaternion.Inverse(vessel.transform.rotation) * vessel.rigidbody.angularVelocity;
            var angularMomentum = new Vector3d(angularVelocity.x * moI.x, angularVelocity.y * moI.y, angularVelocity.z * moI.z);

            var retVar = Vector3d.Scale
            (
                Sign(angularMomentum) * 2.0f,
                Vector3d.Scale(Pow(angularMomentum, 2), Inverse(Vector3d.Scale(torque, moI)))
            );

            retVar.y *= 10;

            return retVar;
        }

        private static Vector3d GetTorque(this Vessel vessel, float thrust)
        {
            var coM = vessel.findWorldCenterOfMass();
            
            float pitchYaw = 0;
            float roll = 0;

            foreach (var part in vessel.parts)
            {
                var relCoM = part.Rigidbody.worldCenterOfMass - coM;

                var pod = part as CommandPod;
                if (pod != null)
                {
                    pitchYaw += Math.Abs(pod.rotPower);
                    roll += Math.Abs(pod.rotPower);
                }

                var rcsModule = part as RCSModule;
                if (rcsModule != null)
                {
                    var max = rcsModule.thrusterPowers.Aggregate<float, float>(0, Mathf.Max);

                    pitchYaw += max * relCoM.magnitude;
                }

                foreach (var module in part.Modules.OfType<ModuleReactionWheel>())
                {
                    pitchYaw += (module).PitchTorque;
                    roll += (module).RollTorque;
                }

                pitchYaw += (float)part.GetThrustTorque(vessel) * thrust;
            }
            
            return new Vector3d(pitchYaw, roll, pitchYaw);
        }

        private static double GetThrustTorque(this Part p, Vessel vessel)
        {
            var coM = vessel.CoM;

            if (p.State == PartStates.ACTIVE)
            {
                var engine = p as LiquidEngine;
                if (engine != null)
                {
                    if (engine.thrustVectoringCapable)
                    {
                        return Math.Sin(Math.Abs(engine.gimbalRange) * Math.PI / 180) * engine.maxThrust * (engine.Rigidbody.worldCenterOfMass - coM).magnitude;
                    }
                }
                else
                {
                    var fuelEngine = p as LiquidFuelEngine;
                    if (fuelEngine != null)
                    {
                        if (fuelEngine.thrustVectoringCapable)
                        {
                            return Math.Sin(Math.Abs(fuelEngine.gimbalRange) * Math.PI / 180) * fuelEngine.maxThrust * (fuelEngine.Rigidbody.worldCenterOfMass - coM).magnitude;
                        }
                    }
                    else
                    {
                        var atmosphericEngine = p as AtmosphericEngine;
                        if (atmosphericEngine != null)
                        {
                            if (atmosphericEngine.thrustVectoringCapable)
                            {
                                return Math.Sin(Math.Abs(atmosphericEngine.gimbalRange) * Math.PI / 180) * atmosphericEngine.maximumEnginePower * atmosphericEngine.totalEfficiency * (atmosphericEngine.Rigidbody.worldCenterOfMass - coM).magnitude;
                            }
                        }
                    }
                }
            }

            return 0;
        }

        private static Vector3d ReduceAngles(Vector3d input)
        {
            return new Vector3d(
                      (input.x > 180f) ? (input.x - 360f) : input.x,
                      (input.y > 180f) ? (input.y - 360f) : input.y,
                      (input.z > 180f) ? (input.z - 360f) : input.z
                  );
        }
        
        private static Vector3d Inverse(Vector3d input)
        {
            return new Vector3d(1 / input.x, 1 / input.y, 1 / input.z);
        }

        private static Vector3d Sign(Vector3d vector)
        {
            return new Vector3d(Math.Sign(vector.x), Math.Sign(vector.y), Math.Sign(vector.z));
        }
    }
}
