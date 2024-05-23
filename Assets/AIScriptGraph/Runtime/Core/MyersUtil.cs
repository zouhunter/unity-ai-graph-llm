using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace AIScripting
{
    public class IntDic
    {
        private Dictionary<int, int> _innerDic = new Dictionary<int, int>();
        public int this[int id]
        {
            get
            {
                int value = 0;
                _innerDic.TryGetValue(id, out value);
                return value;

            }
            set
            {
                _innerDic[id] = value;
            }
        }
        public int Count
        {
            get
            {
                return _innerDic.Count;
            }
        }
        public Dictionary<int, int>.Enumerator GetEnumerator()
        {
            return _innerDic.GetEnumerator();
        }
    }
    public class WayInfo
    {
        public int type;
        public int left;
        public int right;
    }
    public class MyersUtil
    {
#if UNITY_EDITOR
        public static void Test()
        {
            //string[] src_array = System.IO.File.ReadAllLines("a.txt");
            //string[] des_array = System.IO.File.ReadAllLines("b.txt");
            string[] src_array = "x A B C A B B A".Split(' ');
            string[] des_array = "y C B A B A C".Split(' ');
            var trace = Myers_FindTrace(src_array, des_array);

            var sb0 = new StringBuilder();
            for (int id = 0; id < trace.Count; id++)
            {
                var dic = trace[id];
                sb0.Append(string.Format(id + ".【当d=={0}时】", id));
                using (var k_enumerator = dic.GetEnumerator())
                {
                    while (k_enumerator.MoveNext())
                    {
                        var k = k_enumerator.Current.Key;
                        var x = k_enumerator.Current.Value;
                        sb0.Append(string.Format("当k=={0}时:", k));
                        sb0.AppendLine("x:" + x + " y:" + (x - k));
                    }
                }
            }
            Debug.Log(sb0);
            var stateIDs = Myers_FindStates(src_array.Length, des_array.Length, trace);

            var ways = Mayers_FindWays(stateIDs);
            var sb = new StringBuilder();
            for (int i = 0; i < ways.Count; i++)
            {
                var way = ways[i];
                sb.AppendLine(string.Format("type:{0} left:{1} right:{2}", way.type, way.left, way.right));
            }

            Debug.Log(sb);
            var detail = Mayers_StateDetail(stateIDs);
            Debug.Log("left:" + string.Join(',',detail.Key));
            Debug.Log("right:" + string.Join(',', detail.Value));
        }
#endif

        /// <summary>
        /// 分别表示匹配、插入、删除和其他状态的常量。
        /// </summary>
        /// <param name="stateID"></param>
        /// <returns></returns>
        public static KeyValuePair<List<int>, List<int>> Mayers_StateDetail(List<int> stateID)
        {
            // 初始化路径列表
            KeyValuePair<List<int>, List<int>> way = new KeyValuePair<List<int>, List<int>>(new List<int>(), new List<int>());
            // 遍历状态ID列表
            for (int i = 0; i < stateID.Count; i++)
            {
                switch (stateID[i])
                {
                    case 0:
                        // 状态0表示匹配，增加匹配路径
                        way.Key.Add(0);
                        way.Value.Add(0);
                        break;
                    case 1:
                        // 状态1表示插入，增加插入路径
                        way.Value.Add(1);
                        break;
                    case 2:
                        // 状态2表示删除，增加删除路径
                        way.Key.Add(2);
                        break;
                    default:
                        break;
                }
            }
            // 返回路径列表
            return way;
        }

        /// <summary>
        /// 根据状态列表（stateID），生成表示差异的路径列表（way），其中每个路径由源索引和目标索引组成。
        /// </summary>
        /// <param name="stateID"></param>
        /// <returns></returns>
        public static List<WayInfo> Mayers_FindWays(List<int> stateID)
        {
            // 初始化路径列表
            List<WayInfo> way = new List<WayInfo>();
            int srcIndex = 0; // 源索引初始化
            int dstIndex = 0; // 目标索引初始化

            // 遍历状态ID列表
            for (int i = 0; i < stateID.Count; i++)
            {
                switch (stateID[i])
                {
                    case 0:
                        // 状态0表示匹配，增加匹配路径
                        way.Add(new WayInfo() { type = 0, left = srcIndex, right = dstIndex });
                        srcIndex++; // 源索引和目标索引均增加
                        dstIndex++;
                        break;
                    case 1:
                        // 状态1表示插入，增加插入路径
                        way.Add(new WayInfo() { type = 1, left = srcIndex, right = dstIndex });
                        dstIndex++; // 仅目标索引增加
                        break;
                    case 2:
                        // 状态2表示删除，增加删除路径
                        way.Add(new WayInfo() { type = 2, left = srcIndex, right = dstIndex });
                        srcIndex++; // 仅源索引增加
                        break;
                    default:
                        // 其他状态不做处理
                        break;
                }
            }

            // 返回路径列表
            return way;
        }

        /// <summary>
        /// 根据给定的m、n值和追踪列表（trace），生成状态列表（stateID），表示匹配、插入或删除操作。
        /// </summary>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <param name="trace"></param>
        /// <returns></returns>
        public static List<int> Myers_FindStates(int m, int n, List<IntDic> trace)
        {
            // 初始化状态ID列表
            List<int> stateID = new List<int>();
            int x = m; // 源字符串长度
            int y = n; // 目标字符串长度
            int k, prev_k, prev_x, prev_y;

            // 从追踪列表的末尾开始回溯
            for (int d = trace.Count - 1; d > 0; d--)
            {
                k = x - y;

                var v = trace[d - 1]; // 当前d-1步对应的追踪字典

                // 确定前一步的k值
                if (k == -d || (k != d && v[k - 1] < v[k + 1]))
                {
                    prev_k = k + 1;
                }
                else
                {
                    prev_k = k - 1;
                }

                // 获取前一步的x和y值
                prev_x = v[prev_k];
                prev_y = prev_x - prev_k;

                // 回溯匹配操作
                while (x > prev_x && y > prev_y)
                {
                    stateID.Insert(0, 0); // 插入匹配状态
                    x -= 1;
                    y -= 1;
                }
                // 插入操作或删除操作
                if (x == prev_x)
                {
                    stateID.Insert(0, 1); // 插入状态
                }
                else
                {
                    stateID.Insert(0, 2); // 删除状态
                }

                x = prev_x; // 更新x和y值
                y = prev_y;
            }
            return stateID;
        }

        /// <summary>
        /// 实现了 Myers 差异算法，用于查找两个字符串数组之间的差异并生成追踪列表。该函数利用动态规划技术，逐步扩展可能的路径，直至找到最优路径。
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>

        public static List<IntDic> Myers_FindTrace(string[] src, string[] dst)
        {
            int m = src.Length; // 源字符串长度
            int n = dst.Length; // 目标字符串长度
            var max = m + n; // 最大可能路径长度
            var trace = new List<IntDic>(); // 初始化追踪列表
            var find = false; // 标记是否找到最优路径
            IntDic tempTrace = new IntDic(); // 用于初始化的临时字典

            // 遍历所有可能的d值
            for (int d = 0; d <= max && !find; d++)
            {
                var v = new IntDic(); // 当前d对应的最优解字典
                trace.Add(v); // 将当前字典添加到追踪列表
                IntDic last_v = null;

                // 获取上一步的最优解字典
                if (d == 0)
                {
                    last_v = tempTrace;
                }
                else
                {
                    last_v = trace[d - 1];
                }

                // 遍历所有可能的k值
                for (int k = -d; k <= d; k += 2)
                {
                    int x = 0;

                    // 计算当前k对应的x值
                    if (k == -d)
                    {
                        x = last_v[k + 1];
                    }
                    else if (k != d && last_v[k - 1] < last_v[k + 1])
                    {
                        x = last_v[k + 1];
                    }
                    else
                    {
                        x = last_v[k - 1] + 1;
                    }

                    int y = x - k;

                    // 尽可能地进行匹配操作
                    while (x < m && y < n && src[x] == dst[y])
                    {
                        x++;
                        y++;
                    }

                    // 记录当前坐标
                    v[k] = x;

                    // 如果找到最优路径，跳出循环
                    if (x == m && y == n)
                    {
                        find = true;
                        break;
                    }
                }
            }

            // 返回追踪列表
            return trace;
        }
    }
}
