namespace MateAI.ScriptableBehaviourTree
{
    public interface IVariableProvider
    {
        Variable<T> GetVariable<T>(string name, bool crateIfNotExits);
        void SetVariable(string key, Variable variable);
    }
}
