using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MateAI.ScriptableBehaviourTree
{
    public class GraphColorUtil
    {
        public static Color GetColorByState(Status status, bool frame)
        {
            var color = Color.white;
            switch (status)
            {
                case Status.Inactive:
                    color = Color.gray;
                    break;
                case Status.Running:
                    color = Color.yellow;
                    break;
                case Status.Failure:
                    color = Color.red;
                    break;
                case Status.Success:
                    color = Color.green;
                    break;
                default:
                    break;
            }
            color.a = frame ? 1f : 0.3f;
            return color;
        }

        public static void SetColorByState(UnityEngine.UI.Graphic target,Status status,bool frame)
        {
            if (!target)
                return;
          
            target.color = GetColorByState(status,frame);
        }
    }
}
