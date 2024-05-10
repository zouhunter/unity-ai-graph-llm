/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 抽象节点                                                                        *
*//************************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace UFrame.NodeGraph.DataModel
{
	public abstract class Node : ScriptableObject
    {
        public virtual string Title => name;
        public virtual void Initialize(NodeData data)
        {
            InitInputPoints(data);
            InitOutPoints(data);
        }

        protected void InitInputPoints(NodeData data)
        {
            var inPoints = CreateInPoints();
            if (inPoints != null)
            {
                var updated = new List<ConnectionPointData>();
                foreach (var point in inPoints)
                {
                    var old = data.InputPoints.Find(x => x.Label == point.label && !updated.Contains(x));
                    if (old != null)
                    {
                        updated.Add(old);
                        old.Max = point.max;
                        old.Type = point.type;
                    }
                    else
                    {
                        old = data.AddInputPoint(point.label, point.type, point.max);
                        updated.Add(old);
                    }
                }
                data.InputPoints.Clear();
                data.InputPoints.AddRange(updated);
            }
        }
        protected void InitOutPoints(NodeData data)
        {
            var outputs = CreateOutPoints();
            if (outputs != null)
            {
                var updated = new List<ConnectionPointData>();
                foreach (var point in outputs)
                {
                    var old = data.OutputPoints.Find(x => (x.Label == point.label /*|| x.Type == point.type*/) && !updated.Contains(x));
                    if (old != null)
                    {
                        updated.Add(old);
                        old.Label = point.label;
                        old.Max = point.max;
                        old.Type = point.type;
                    }
                    else
                    {
                        old = data.AddOutputPoint(point.label, point.type, point.max);
                        updated.Add(old);
                    }
                }
                data.OutputPoints.Clear();
                data.OutputPoints.AddRange(updated);
            }
        }
        protected virtual IEnumerable<Point> CreateInPoints() => null;
        protected virtual IEnumerable<Point> CreateOutPoints() => null;
    }

}
