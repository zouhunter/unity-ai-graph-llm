using System;
using System.Collections.Generic;
using UnityEngine;

namespace MateAI.ScriptableBehaviourTree
{
    [System.Serializable]
    public class TreeInfo
    {
        public string id;
        public bool enable;
        public string desc;
        public Status status { get; set; }
        public int tickCount { get; set; }
        [SerializeReference]
        public BaseNode node;
        public ConditionInfo condition = new ConditionInfo();
        [SerializeReference]
        public List<TreeInfo> subTrees;
        public TreeInfo()
        {
            id = System.Guid.NewGuid().ToString();
        }
    }

    [Serializable]
    public class ConditionInfo
    {
        public bool enable;
        public MatchType matchType = MatchType.AllSuccess;
        public List<ConditionItem> conditions = new List<ConditionItem>();
    }

    [Serializable]
    public class ConditionItem
    {
        public Status status { get; set; }
        public int tickCount { get; set; }

        public MatchType matchType = MatchType.AllSuccess;
        [SerializeReference]
        public ConditionNode node;
        public int state;
        public bool subEnable;
        public List<SubConditionItem> subConditions = new List<SubConditionItem>();
    }

    [Serializable]
    public class SubConditionItem
    {
        public int state;
        public Status status { get; set; }
        public int tickCount { get; set; }

        [SerializeReference]
        public ConditionNode node;
    }
}
