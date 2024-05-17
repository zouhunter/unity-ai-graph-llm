using UnityEngine;

namespace UAI{ 
	public class SingletonBehaviour<T> : MonoBehaviour where T : Component
	{ 	
		private static T _instance;
		public static T Instance {
			get { 
				if (null == _instance)
				{
					_instance = FindObjectOfType(typeof(T)) as T;
				}
	
				if (null == _instance)
				{
					
					var go = new GameObject(typeof(T).Name); 
					// go.hideFlags = HideFlags.HideAndDontSave;
					_instance = go.AddComponent<T>();
				}

				return _instance;
			}
		}
	
		public SingletonBehaviour()
		{
			if (null != _instance)
			{
				//Destroy(this);
			}else{
				_instance = this as T;
			}
		}
	}
}