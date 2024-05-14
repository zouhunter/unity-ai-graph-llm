/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 连接信息                                                                        *
*//************************************************************************************/

using System;

namespace UFrame.NodeGraph.DataModel
{
    public struct CustomConnectionInfo
    {
        public CustomConnectionAttribute connection;
        public Type type;

        public CustomConnectionInfo(Type t, CustomConnectionAttribute n)
        {
            connection = n;
            type = t;
        }

        public Connection CreateInstance()
        {
            if (type == null || string.IsNullOrEmpty(type.FullName))
            {
                type = typeof(Connection);
            }
            var c = System.Activator.CreateInstance(type) as Connection;
            c.name = type.FullName;
            c.type = connection.Name;
            return c;
        }
    }
}