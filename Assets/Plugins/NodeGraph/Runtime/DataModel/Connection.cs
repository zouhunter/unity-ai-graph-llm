/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 连接                                                                            *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.NodeGraph.DataModel
{

    public class Connection : ScriptableObject 
    {
        public virtual string Title => name;
        public string type { get; set; }
    }
}