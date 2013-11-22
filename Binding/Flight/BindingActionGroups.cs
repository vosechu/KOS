using kOS.Craft;

namespace kOS.Binding.Flight
{
    [kOSBinding("ksp")]
    public class BindingActionGroups : Binding
    {
        public override void AddTo(BindingManager manager)
        {
            manager.AddSetter("SAS", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, (bool) val));
            manager.AddSetter("GEAR", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Gear, (bool) val));
            manager.AddSetter("LEGS", (cpu, val) => cpu.Vessel.LandingLegsCtrl((bool) val));
            manager.AddSetter("CHUTES", (cpu, val) => cpu.Vessel.DeployParachutes((bool) val));
            manager.AddSetter("LIGHTS", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Light, (bool) val));
            manager.AddSetter("PANELS", (cpu, val) => cpu.Vessel.SolarPanelCtrl((bool) val));
            manager.AddSetter("BRAKES", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, (bool) val));
            manager.AddSetter("RCS", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.RCS, (bool) val));
            manager.AddSetter("ABORT", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Abort, (bool) val));
            manager.AddSetter("AG1", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Custom01, (bool) val));
            manager.AddSetter("AG2", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Custom02, (bool) val));
            manager.AddSetter("AG3", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Custom03, (bool) val));
            manager.AddSetter("AG4", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Custom04, (bool) val));
            manager.AddSetter("AG5", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Custom05, (bool) val));
            manager.AddSetter("AG6", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Custom06, (bool) val));
            manager.AddSetter("AG7", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Custom07, (bool) val));
            manager.AddSetter("AG8", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Custom08, (bool) val));
            manager.AddSetter("AG9", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Custom09, (bool) val));
            manager.AddSetter("AG10", (cpu, val) => cpu.Vessel.ActionGroups.SetGroup(KSPActionGroup.Custom10, (bool) val));

            manager.AddGetter("SAS", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.SAS]);
            manager.AddGetter("GEAR", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Gear]);
            manager.AddGetter("LEGS", cpu => cpu.Vessel.GetLandingLegStatus());
            manager.AddGetter("CHUTES", cpu => cpu.Vessel.GetChuteStatus());
            manager.AddGetter("LIGHTS", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Light]);
            manager.AddGetter("PANELS", cpu => cpu.Vessel.GetSolarPanelStatus());
            manager.AddGetter("BRAKES", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Brakes]);
            manager.AddGetter("RCS", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.RCS]);
            manager.AddGetter("ABORT", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Abort]);
            manager.AddGetter("AG1", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Custom01]);
            manager.AddGetter("AG2", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Custom02]);
            manager.AddGetter("AG3", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Custom03]);
            manager.AddGetter("AG4", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Custom04]);
            manager.AddGetter("AG5", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Custom05]);
            manager.AddGetter("AG6", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Custom06]);
            manager.AddGetter("AG7", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Custom07]);
            manager.AddGetter("AG8", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Custom08]);
            manager.AddGetter("AG9", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Custom09]);
            manager.AddGetter("AG10", cpu => cpu.Vessel.ActionGroups[KSPActionGroup.Custom10]);
        }
    }
}