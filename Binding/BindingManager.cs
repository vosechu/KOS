using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace kOS.Binding
{
    public class BindingManager
    {
        public CPU Cpu;

        public Dictionary<string, BindingSetDlg> Setters = new Dictionary<string, BindingSetDlg>();
        public Dictionary<string, BindingGetDlg> Getters = new Dictionary<string, BindingGetDlg>();
        public List<Binding> Bindings = new List<Binding>();
        
        public delegate void BindingSetDlg      (CPU cpu, object val);
        public delegate object BindingGetDlg    (CPU cpu);

        public BindingManager(CPU cpu, String context)
        {
            Cpu = cpu;

            var contexts = new string[1];
            contexts[0] = context;

            foreach (var b in from t in Assembly.GetExecutingAssembly().GetTypes() 
                              let attr = (kOSBinding)t.GetCustomAttributes(typeof(kOSBinding), true).FirstOrDefault() 
                              where attr != null 
                              where !attr.Contexts.Any() || attr.Contexts.Intersect(contexts).Any() 
                              select (Binding)Activator.CreateInstance(t))
            {
                b.AddTo(this);
                Bindings.Add(b);
            }
        }

        public void AddGetter(String name, BindingGetDlg dlg)
        {
            var v = Cpu.FindVariable(name) ?? Cpu.FindVariable(name.Split(":".ToCharArray())[0]);

            if (v != null)
            {
                var variable = v as BoundVariable;
                if (variable != null)
                {
                    variable.Get = dlg;
                }
            }
            else
            {
                var bv = Cpu.CreateBoundVariable<BoundVariable>(name);
                bv.Get = dlg;
            }
        }

        public void AddSetter(String name, BindingSetDlg dlg)
        {
            var v = Cpu.FindVariable(name.ToLower());
            if (v != null)
            {
                var variable = v as BoundVariable;
                if (variable != null)
                {
                    variable.Set = dlg;
                }
            }
            else
            {
                var bv = Cpu.CreateBoundVariable<BoundVariable>(name);
                bv.Set = dlg;
            }
        }

        public void Update(float time)
        {
            foreach (var b in Bindings)
            {
                b.Update(time);
            }
        }
    }
}