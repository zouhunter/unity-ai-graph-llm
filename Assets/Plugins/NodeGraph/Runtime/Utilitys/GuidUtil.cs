/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - guid生成                                                                        *
*//************************************************************************************/

namespace UFrame.NodeGraph
{
    public class GuidUtil
    {
        internal static string NewGuid(object target = null)
        {
            if(target != null)
                return target.GetHashCode().ToString();
            return new System.Guid().ToString().Substring(0,8);
        }
    }
}
