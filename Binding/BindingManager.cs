using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kOS.Context;

namespace kOS.Binding
{
    public class BindingManager
    {
        public CPU Cpu;

	private readonly List<SmoothVariable> updatable = new List<SmoothVariable>(); 
        private readonly List<Binding> bindings = new List<Binding>();
        
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
                bindings.Add(b);
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

	public void AddSmooth(string name, BindingGetDlg dlg)
	{
	    AddGetter(name, dlg);
	    var smoothName = name + ":SMOOTH";

		var v = Cpu.FindVariable(smoothName) ?? Cpu.FindVariable(smoothName.Split(":".ToCharArray())[0]);

		if (v != null)
		{
		    var variable = v as SmoothVariable;
		    if (variable != null)
		    {
			variable.Get = dlg;
		    }
		}
		else
		{
		    var bv = Cpu.CreateBoundVariable<SmoothVariable>(smoothName);
		    bv.Get = dlg;
		    updatable.Add(bv);
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
            foreach (var b in bindings)
            {
                b.Update(time);
            }
            foreach (var smoothVariable in updatable)
            {
                smoothVariable.Update();
            }
        }
    }
}