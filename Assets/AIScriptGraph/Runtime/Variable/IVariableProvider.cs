namespace AIScripting
{
    public interface IVariableProvider
    {
        Variable GetVariable(string name);
        Variable<T> GetVariable<T>(string name, bool crateIfNotExits);
        void SetVariable(string key, Variable variable);
    }
}
