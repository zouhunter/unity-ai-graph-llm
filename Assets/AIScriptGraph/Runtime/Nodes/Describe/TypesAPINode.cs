using System;
using System.Collections;
using System.Collections.Generic;

using UFrame.NodeGraph;

using UnityEngine;
using System.Reflection;
using System.Text;

namespace AIScripting.Describe
{
    [CustomNode("TypesAPI", 0, Define.GROUP)]
    public class TypesAPINode : DescribeBaseNode
    {
        [Tooltip("�����б�")]
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
        /// ��ȡ���͵�����Ϣ
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static void WriteTypeClass(Type type,BindingFlags bindingFlags, StringBuilder sb)
        {
            // ����Ƿ���ö��
            bool isEnum = type.IsEnum;

            // ����Ƿ���ֵ���ͣ��ṹ����ֵ���ͣ�����������ֵ���Ͷ��ǽṹ�壩
            bool isValueType = type.IsValueType;

            // ��׼ȷ�أ�����Ƿ��ǽṹ��
            // �ṹ����ֵ���Ͳ��Ҳ���ö�٣�Ҳ���ǻ�Ԫ���ͣ���int, float�ȣ���Ҳ���� System.ValueType ����
            bool isStruct = type.IsValueType && !type.IsEnum && !type.IsPrimitive && type != typeof(ValueType);

            // ��ȡ���й�������
            PropertyInfo[] properties = type.GetProperties(bindingFlags);

            // ��ȡ���з���
            MethodInfo[] methods = type.GetMethods(bindingFlags);

            // ��ȡ���й����ֶ�
            FieldInfo[] fields = type.GetFields(bindingFlags);

            // ��ȡ���й��캯��
            ConstructorInfo[] constructors = type.GetConstructors(bindingFlags);

            // ��ȡ�������
            string className = type.Name;

            // ��ȡ��������
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
        /// �ж��Ƿ��ʱ
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
        /// //��ȡ����
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