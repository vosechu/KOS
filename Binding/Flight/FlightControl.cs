using System;
using System.Collections.Generic;
using UnityEngine;
using kOS.Craft;

namespace kOS.Binding.Flight
{
    [kOSBinding("ksp")]
    public class FlightControls : Binding
    {
        private Vessel vessel;
        private CPU cpu;
        private readonly List<LockableControl> controls = new List<LockableControl>();

        public override void AddTo(BindingManager manager)
        {
            cpu = manager.Cpu;
            vessel = manager.Cpu.Vessel;

            controls.Add(new LockableControl("THROTTLE", "throttle", cpu, manager));
            controls.Add(new LockableControl("STEERING", "steering", cpu, manager));

            controls.Add(new LockableControl("WHEELSTEERING", "wheelsteering", cpu, manager));
            controls.Add(new LockableControl("WHEELTHROTTLE", "wheelthrottle", cpu, manager));

            vessel.OnFlyByWire += OnFlyByWire;
        }
        
        public void OnFlyByWire(FlightCtrlState c)
        {
            foreach (LockableControl control in controls)
            {
                control.OnFlyByWire(ref c);
            }
        }

        public override void Update(float time)
        {
            if (vessel != cpu.Vessel)
            {
                // Try to re-establish connection to vessel
                if (vessel != null)
                {
                    vessel.OnFlyByWire -= OnFlyByWire;
                    vessel = null;
                }

                if (cpu.Vessel != null)
                {
                    vessel = cpu.Vessel;
                    vessel.OnFlyByWire += OnFlyByWire;

                    foreach (LockableControl c in controls)
                    {
                        c.UpdateVessel(vessel);
                    }
                }
            }

            base.Update(time);
        }

        public class LockableControl
        {
            public String name;
            public bool locked;
            public object Value;
            public Vessel vessel;
            readonly string propertyName;
            public CPU cpu;

            public LockableControl(String name, String propertyName, CPU cpu, BindingManager manager)
            {
                this.name = name;
                this.cpu = cpu;
                this.vessel = cpu.Vessel;
                locked = false;
                Value = 0;
                
                manager.AddGetter(name, c => Value);
                manager.AddSetter(name, delegate { });

                this.propertyName = propertyName;
            }

            public void OnFlyByWire(ref FlightCtrlState c)
            {
                Expression e = cpu.GetDeepestChildContext().GetLock(propertyName);

                if (e == null) return;

                Value = e.GetValue();

                if (propertyName == "throttle")
                {
                    c.mainThrottle = (float)e.Double();
                }

                if (propertyName == "wheelthrottle")
                {
                    c.wheelThrottle = (float)Utils.Clamp(e.Double(), -1, 1);
                }

                if (propertyName == "steering")
                {
                    if (Value is String && ((string)Value).ToUpper() == "KILL")
                    {
                        vessel.KillRotation(c);
                    }
                    else if (Value is Direction)
                    {
                        vessel.SteerShipToward((Direction)Value, c);
                    }
                    else if (Value is Vector)
                    {
                        vessel.SteerShipToward(((Vector)Value).ToDirection(), c);
                    }
                    else if (Value is Node)
                    {
                        vessel.SteerShipToward(((Node)Value).GetBurnVector().ToDirection(), c);
                    }
                }

                if (propertyName == "wheelsteering")
                {
                    float bearing = 0;

                    if (Value is VesselTarget)
                    {
                        bearing = vessel.GetTargetBearing(((VesselTarget)Value).target);
                    }
                    else if (Value is GeoCoordinates)
                    {
                        bearing = ((GeoCoordinates)Value).GetBearing(vessel);
                    }
                    else if (Value is double)
                    {
                        bearing = (float)(Math.Round((double)Value) - Mathf.Round(FlightGlobals.ship_heading));
                    }

                    if (vessel.horizontalSrfSpeed > 0.1f)
                    { 
                        if (Mathf.Abs(VesselUtils.AngleDelta(vessel.GetHeading(), vessel.GetVelocityHeading())) <= 90)
                        {
                            c.wheelSteer = Mathf.Clamp(bearing / -10, -1, 1);
                        }
                        else
                        {
                            c.wheelSteer = -Mathf.Clamp(bearing / -10, -1, 1);
                        }
                    }
                }

                if (cpu.GetLock(name) == null)
                {
                    locked = false;
                }
            }

            internal void UpdateVessel(Vessel vessel)
            {
                this.vessel = vessel;
            }
        }
    }
}
