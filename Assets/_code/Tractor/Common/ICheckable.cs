namespace Tractor
{
    public interface ICheckable
    {
        float GetVariableValue(string conditionName);
        bool HasVariable(string conditionName);
    }
}
