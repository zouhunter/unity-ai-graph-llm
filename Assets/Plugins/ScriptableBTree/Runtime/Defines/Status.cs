
namespace MateAI.ScriptableBehaviourTree
{
    public enum Status
    {
        Inactive, // 未激活
        Running,  // 运行中 （接续执行）
        Failure,  // 失败
        Success,  // 成功
        Interrupt,// 中断 （重新执行）
    }
}
