using System;
using System.Collections.Generic;
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
    public class MyersUtil
    {
#if UNITY_EDITOR
        public static void Test()
        {
            //string[] src_array = System.IO.File.ReadAllLines("a.txt");
            //string[] des_array = System.IO.File.ReadAllLines("b.txt");
            string[] src_array = "A B C A B B A".Split(' ');
            string[] des_array = "C B A B A C".Split(' ');
            var trace = Myers_FindTrace(src_array, des_array);

            for (int id = 0; id < trace.Count; id++)
            {
                var dic = trace[id];
                Debug.Log(string.Format(id + ".【当d=={0}时】", id));
                using (var k_enumerator = dic.GetEnumerator())
                {
                    while (k_enumerator.MoveNext())
                    {
                        var k = k_enumerator.Current.Key;
                        var x = k_enumerator.Current.Value;
                        Debug.Log(string.Format("当k=={0}时:", k));
                        Debug.Log("x:" + x + " y:" + (x - k));
                    }
                }
            }
            var stateIDs = Myers_FindStates(src_array.Length, des_array.Length, trace);

            var way = Mayers_FindWays(stateIDs);

            for (int i = 0; i < way.Count; i++)
            {
                Debug.Log(i + ":" + " x=" + way[i].Key + " y=" + way[i].Value);
            }
            Console.Read();
        }
#endif

        public static List<KeyValuePair<int, int>> Mayers_FindWays(List<int> stateID)
        {
            List<KeyValuePair<int, int>> way = new List<KeyValuePair<int, int>>();
            int srcIndex = 0;
            int dstIndex = 0;
            for (int i = 0; i < stateID.Count; i++)
            {
                switch (stateID[i])
                {
                    case 0:
                        way.Add(new KeyValuePair<int, int>(srcIndex, dstIndex));
                        srcIndex++;
                        dstIndex++;
                        break;
                    case 1:
                        way.Add(new KeyValuePair<int, int>(srcIndex, dstIndex));
                        dstIndex++;
                        break;
                    case 2:
                        way.Add(new KeyValuePair<int, int>(srcIndex, dstIndex));
                        srcIndex++;
                        break;
                    default:
                        break;
                }
            }

            return way;
        }

        public static List<int> Myers_FindStates(int m, int n, List<IntDic> trace)
        {
            List<int> stateID = new List<int>();
            var way = new List<KeyValuePair<int, int>>();
            int x = m;
            int y = n;
            int k, prev_k, prev_x, prev_y;

            for (int d = trace.Count - 1; d > 0; d--)
            {
                k = x - y;

                var v = trace[d - 1];

                if (k == -d || (k != d && v[k - 1] < v[k + 1]))
                {
                    prev_k = k + 1;
                }
                else
                {
                    prev_k = k - 1;
                }

                prev_x = v[prev_k];
                prev_y = prev_x - prev_k;

                while (x > prev_x && y > prev_y)
                {
                    stateID.Insert(0, 0);
                    //相同
                    x -= 1;
                    y -= 1;
                }

                if (x == prev_x)
                {
                    //插入
                    stateID.Insert(0, 1);
                }
                else
                {
                    //删除
                    stateID.Insert(0, 2);
                }

                x = prev_x;
                y = prev_y;
            }
            return stateID;
        }

        public static List<IntDic> Myers_FindTrace(string[] src, string[] dst)
        {
            int m = src.Length;
            int n = dst.Length;
            var max = m + n;
            var trace = new List<IntDic>();
            var find = false;
            IntDic tempTrace = new IntDic();
            for (int d = 0; d <= max && !find; d++)
            {
                //当前d对应的最优解字典
                var v = new IntDic();
                trace.Add(v);
                IntDic last_v = null;
                if (d == 0)
                {
                    last_v = tempTrace;
                }
                else
                {
                    last_v = trace[d - 1];
                }

                for (int k = -d; k <= d; k += 2)
                {
                    int x = 0;

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

                    while (x < m && y < n && src[x] == dst[y])
                    {
                        x++;
                        y++;
                    }

                    v[k] = x;//坐标记录

                    if (x == m && y == n)
                    {
                        //跳出循环
                        find = true;
                        break;
                    }
                }
            }
            return trace;
        }
    }
}
