using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace UFrame.NodeGraph.ObjDrawer
{
	public sealed class PropertyModification
	{
		public object target;

		public string propertyPath;

		public string value;
	}
}
