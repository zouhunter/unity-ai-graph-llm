/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 自定义工具                                                                      *
*//************************************************************************************/

using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace UFrame.NodeGraph
{
    public class UserDefineUtility
    {
        private static List<Type> _controllerTypes;
        public static List<Type> CustomControllerTypes
        {
            get
            {
                if (_controllerTypes == null)
                {
                    _controllerTypes = BuildControllerTypeList();
                }
                return _controllerTypes;
            }
        }
        private static List<Type> BuildControllerTypeList()
        {
            var list = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var nodes = assembly.GetTypes()
                    .Where(t=> !t.IsGenericType)
                    .Where(t => !t.IsAbstract)
                    .Where(t => t != typeof(NodeGraphController))
                    .Where(t => typeof(NodeGraphController).IsAssignableFrom(t));
                list.AddRange(nodes);
            }
            return list;
        }
        private static Dictionary<Type, Type> userDrawer;

        internal static NodeGraphController CreateController(string controllerType)
        {
            var type = CustomControllerTypes.Find(x => x.FullName == controllerType);
            if (type != null)
            {
                var ctrl = System.Activator.CreateInstance(type);
                var gctrl = ctrl as NodeGraphController;
                return gctrl;
            }
            else
            {
                Debug.LogError("can not find controllerType:" + controllerType);
                foreach(var controller in CustomControllerTypes){
                    Debug.Log(controller.FullName);
                }
                return null;
            }
        }
        internal static object GetUserDrawer(Type type)
        {
            InitDrawerTypes();

            Type supportDrawer = null;

            if (userDrawer.ContainsKey(type))
            {
                supportDrawer = userDrawer[type];
            }
            else
            {
                supportDrawer = userDrawer.Where(x => type.IsSubclassOf(x.Key)).FirstOrDefault().Value;
            }
            if (supportDrawer != null)
            {
                var drawer = Activator.CreateInstance(supportDrawer);
                return drawer;
            }
            return null;
        }
        private static void InitDrawerTypes()
        {
            if (userDrawer == null)
            {
                userDrawer = new Dictionary<Type, Type>();
                var allDrawer = new List<Type>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var nodes = assembly.GetTypes()
                        .Where(t => t != typeof(NodeView) && t != typeof(ConnectionView))
                        .Where(t => typeof(NodeView).IsAssignableFrom(t) || typeof(ConnectionView).IsAssignableFrom(t));
                    allDrawer.AddRange(nodes);
                }

                foreach (var type in allDrawer)
                {
                    CustomViewAttribute attr = type.GetCustomAttributes(typeof(CustomViewAttribute), false).FirstOrDefault() as CustomViewAttribute;

                    if (attr != null)
                    {
                        foreach (var item in attr.targetTypes)
                        {
                            userDrawer.Add(item, type);
                        }
                    }
                }
            }
        }
    }

}