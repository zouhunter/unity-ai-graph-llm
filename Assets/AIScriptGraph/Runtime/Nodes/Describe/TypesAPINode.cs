using System;
using System.Collections;
using System.Collections.Generic;

using UFrame.NodeGraph;

using UnityEngine;
using System.Reflection;
using System.Text;

namespace AIScripting.Describe
{
    [CustomNode("TypesAPI", 2, Define.GROUP)]
    public class TypesAPINode : DescribePrefixNode
    {
        [Tooltip("类型列表")]
        public Ref<List<string>> types;
        public BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        private Dictionary<string, Type> _typeDict = new Dictionary<string, Type>();

        protected override AsyncOp WriteContent(StringBuilder sb)
        {
            bool exists = false;
            foreach (var typeName in types.Value)
            {
                var type = GetType(typeName);
                if (type != null)
                {
                    exists = true;
                    WriteTypeClass(type,bindingFlags, sb);
                }
            }
            if (exists)
            {
                return AsyncOp.CompletedOp;
            }
            return null;
        }

        /// <summary>
        /// 获取类型的类信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static void WriteTypeClass(Type type,BindingFlags bindingFlags, StringBuilder sb)
        {
            // 检查是否是枚举
            bool isEnum = type.IsEnum;

            // 检查是否是值类型（结构体是值类型，但不是所有值类型都是结构体）
            bool isValueType = type.IsValueType;

            // 更准确地，检查是否是结构体
            // 结构体是值类型并且不是枚举，也不是基元类型（如int, float等），也不是 System.ValueType 本身
            bool isStruct = type.IsValueType && !type.IsEnum && !type.IsPrimitive && type != typeof(ValueType);

            // 获取所有公共属性
            PropertyInfo[] properties = type.GetProperties(bindingFlags);

            // 获取所有方法
            MethodInfo[] methods = type.GetMethods(bindingFlags);

            // 获取所有公共字段
            FieldInfo[] fields = type.GetFields(bindingFlags);

            // 获取所有构造函数
            ConstructorInfo[] constructors = type.GetConstructors(bindingFlags);

            // 获取类的名称
            string className = type.Name;

            // 获取基类类型
            Type baseType = type.BaseType;

            if (isEnum)
            {
                sb.AppendLine($"public enum {className}");
            }
            else if (isStruct)
            {
                sb.AppendLine($"public struct {className}");
            }
            else
            {
                sb.AppendLine($"public class {className}");
            }
            sb.AppendLine("{");
            foreach (var prop in properties)
            {
                if (CheckObsolete(prop))
                    continue;

                sb.AppendLine("    public " + prop.PropertyType.Name + " " + prop.Name + " { get; set; }");
            }
            foreach (var field in fields)
            {
                if (CheckObsolete(field))
                    continue;

                sb.AppendLine("    public " + field.FieldType.Name + " " + field.Name + ";");
            }
            foreach (var item in constructors)
            {
                if (CheckObsolete(item))
                    continue;
                var args = item.GetParameters();
                sb.Append("    public " + className + "(");
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    sb.Append(arg.ParameterType.Name + " " + arg.Name);
                    if (i < args.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.AppendLine(");");
            }
            foreach (var method in methods)
            {
                if (CheckObsolete(method))
                    continue;

                if (method.Name.Contains("get_") || method.Name.Contains("set_"))
                {
                    var methodName = method.Name.Substring(4);
                    if (Array.Find(properties, x => x.Name == methodName) != null)
                        continue;
                }

                var args = method.GetParameters();
                var genericArgs = method.GetGenericArguments();
                var returnType = method.ReturnType;
                sb.Append("    public " + returnType.Name + " " + method.Name);
                if (genericArgs != null && genericArgs.Length > 0)
                {
                    sb.Append("<");
                    for (var i = 0; i < genericArgs.Length; i++)
                    {
                        sb.Append(genericArgs[i].Name);
                        if (i < genericArgs.Length - 1)
                        {
                            sb.Append(", ");
                        }
                    }
                    sb.Append(">");
                }
                sb.Append("(");
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    sb.Append(arg.ParameterType.Name + " " + arg.Name);
                    if (i < args.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.AppendLine(");");

            }
            sb.AppendLine("}");
        }

        /// <summary>
        /// 判断是否过时
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        private static bool CheckObsolete(MemberInfo member)
        {
            if (member.GetCustomAttribute<ObsoleteAttribute>() != null)
                return true;
            return false;
        }

        /// <summary>
        /// //获取类型
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        private Type GetType(string typeFullName)
        {
            if (!_typeDict.TryGetValue(typeFullName, out var type))
            {
                type = Type.GetType(typeFullName);
                if (type == null)
                {

                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(typeFullName);
                        if (type != null)
                        {
                            break;
                        }
                    }
                }
            }
            if (type != null)
            {
                _typeDict[typeFullName] = type;
            }
            return type;
        }
    }
}
