using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace AIScripting.Debugger
{
    [CustomEditor(typeof(DiffShowNode))]
    public class DiffShowDrawer : Editor
    {
        private DiffShowNode node;

        private void OnEnable()
        {
            node = target as DiffShowNode;
        }
    }
}
