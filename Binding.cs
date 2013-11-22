using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace kOS
{
    public class kOSBinding : Attribute
    {
        public string[] Contexts;
        public kOSBinding(params string[] contexts) { Contexts = contexts; }
    }
    
    public class BindingManager
    {
        public CPU Cpu;

        public Dictionary<String, BindingSetDlg> Setters = new Dictionary<String, BindingSetDlg>();
        public Dictionary<String, BindingGetDlg> Getters = new Dictionary<String, BindingGetDlg>();
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
                if (v is BoundVariable)
                {
                    ((BoundVariable)v).Get = dlg;
                }
            }
            else
            {
                var bv = Cpu.CreateBoundVariable(name);
                bv.Get = dlg;
                bv.Cpu = Cpu;
            }
        }

        public void AddSetter(String name, BindingSetDlg dlg)
        {
            var v = Cpu.FindVariable(name.ToLower());
            if (v != null)
            {
                if (v is BoundVariable)
                {
                    ((BoundVariable)v).Set = dlg;
                }
            }
            else
            {
                var bv = Cpu.CreateBoundVariable(name.ToLower());
                bv.Set = dlg;
                bv.Cpu = Cpu;
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

    public class Binding
    {
        public virtual void AddTo(BindingManager manager) { }

        public virtual void Update(float time) { }
    }

    public class BoundVariable : Variable
    {
        public BindingManager.BindingSetDlg Set;
        public BindingManager.BindingGetDlg Get;
        public CPU Cpu;

        public override object Value
        {
            get
            {
                return Get(Cpu);
            }
            set
            {
                Set(Cpu, value);
            }
        }
    }

    [kOSBinding]
    public class TestBindings : Binding
    {
        public override void AddTo(BindingManager manager)
        {
            //manager.AddGetter("TEST1", delegate(CPU cpu) { return 4; });
            //manager.AddSetter("TEST1", delegate(CPU cpu, object val) { cpu.PrintLine(val.ToString()); });
        }
    }
}
