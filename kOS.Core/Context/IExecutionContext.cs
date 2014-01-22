using System.Collections.Generic;
using kOS.Binding;
using kOS.Command;
using kOS.Expression;
using kOS.Persistance;
using kOS.Safe.Expression;
using kOS.Safe.Utilities;
using kOS.Utilities;

namespace kOS.Context
{
    public interface IExecutionContext
    {
        Vessel Vessel { get; }
        void OnSave(ConfigNode node);
        void OnLoad(ConfigNode node);

        IVolume SelectedVolume { get; set; }
        IList<IVolume> Volumes { get; }
        IDictionary<string, Variable> Variables { get; }
        IList<KOSExternalFunction> ExternalFunctions { get; }
        IExecutionContext ParentContext { get; set; }
        IExecutionContext ChildContext { get; set; }
        ExecutionState State { get; set; }
        int Line { get; set; }
        void VerifyMount();
        bool KeyInput(char c);
        bool Type(char c);
        bool SpecialKey(kOSKeys key);
        char[,] GetBuffer();
        void StdOut(string text);
        void Put(string text, int x, int y);
        void Update(float time);
        void Push(IExecutionContext newChild);
        bool Break();
        Variable FindVariable(string varName);
        Variable CreateVariable(string varName);
        Variable FindOrCreateVariable(string varName);
        BoundVariable CreateBoundVariable(string varName);
        bool SwitchToVolume(int volID);
        bool SwitchToVolume(string volName);
        IVolume GetVolume(object volID);
        IExecutionContext GetDeepestChildContext();
        T FindClosestParentOfType<T>() where T : class, IExecutionContext;
        void UpdateLock(string name);
        IExpression GetLock(string name);
        void Lock(ICommand command);
        void Lock(string name, IExpression expression);
        void Unlock(ICommand command);
        void Unlock(string name);
        void UnlockAll();
        void Unset(string name);
        void UnsetAll();
        bool ParseNext(ref string buffer, out string cmd, ref int lineCount, out int lineStart);
        void SendMessage(SystemMessage message);
        int GetCursorX();
        int GetCursorY();
        object CallExternalFunction(string name, string[] parameters);
        bool FindExternalFunction(string name);
        string GetVolumeBestIdentifier(IVolume selectedVolume);
    }
}