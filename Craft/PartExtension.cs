using System.Linq;

namespace kOS.Craft
{
    public static class PartExtension
    {
        public static bool HasModule<T>(this Part p) where T : PartModule
        {
            return p.Modules.OfType<T>().Any();
        }

        public static bool IsSepratron(this Part p)
        {
            return p.ActivatesEvenIfDisconnected
                && p.IsEngine()
                && p.IsDecoupledInStage(p.inverseStage)
                && !p.isControlSource;
        }

        public static bool IsEngine(this Part p)
        {
            return (p is SolidRocket ||
                p is LiquidEngine ||
                p is LiquidFuelEngine ||
                p is AtmosphericEngine ||
                p.HasModule<ModuleEngines>());
        }

        public static bool IsDecoupledInStage(this Part p, int stage)
        {
            if ((p.IsDecoupler() || p.IsLaunchClamp()) && p.inverseStage == stage) return true;
            if (p.parent == null) return false;
            return p.parent.IsDecoupledInStage(stage);
        }

        public static bool IsDecoupler(this Part p)
        {
            return (p is Decoupler ||
             p is DecouplerGUI ||
             p is RadialDecoupler ||
             p.HasModule<ModuleDecouple>() ||
             p.HasModule<ModuleAnchoredDecoupler>());
        }

        public static bool IsParachute(this Part p)
        {
            return p is Parachutes ||
                p is HParachutes ||
                p.HasModule<ModuleParachute>();
        }

        public static bool IsLaunchClamp(this Part p)
        {
            return p.HasModule<LaunchClamp>();
        }
	
    }
}
