/***********************************************************************************//*
*  ����: �޺���                                                                       *
*  ʱ��: 2021-04-24                                                                   *
*  �汾: master_5122a2                                                                *
*  ����:                                                                              *
*   - guid����                                                                        *
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
