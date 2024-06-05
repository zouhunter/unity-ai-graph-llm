using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace MateAI
{
    public class AIToolBoxWindow : EditorWindow
    {
        public const string Name = "Unity-AI Tools";
        public static AIToolBoxWindow Instance { get; private set; }

        [SerializeReference] private List<Tool> _Tools;

        [SerializeField] private Vector2 _Scroll;
        [SerializeField] private int _CurrentTool = -1;
        public int CurrentTool
        {
            get
            {
                return _CurrentTool;
            }
            set
            {
                if (_CurrentTool != value)
                {
                    _CurrentTool = value;
                    OnSelectionChange();
                }
            }
        }
        private void OnEnable()
        {
            titleContent = new GUIContent(Name);
            Instance = this;

            InitializeTools();

            Undo.undoRedoPerformed += Repaint;

            OnSelectionChange();
        }

        private void InitializeTools()
        {
            if (_Tools == null)
            {
                _Tools = new List<Tool>();
            }
            else
            {
                for (int i = _Tools.Count - 1; i >= 0; i--)
                    if (_Tools[i] == null)
                        _Tools.RemoveAt(i);
            }

            var toolTypes = GetToolBoxTypes(typeof(Tool));
            for (int i = 0; i < toolTypes.Count; i++)
            {
                var toolType = toolTypes[i];
                if (IndexOfTool(toolType) >= 0)
                    continue;

                var tool = (Tool)Activator.CreateInstance(toolType);
                _Tools.Add(tool);
            }

            _Tools.Sort();

            for (int i = 0; i < _Tools.Count; i++)
                _Tools[i].OnEnable(i);
        }

        private List<Type> GetToolBoxTypes(Type baseType)
        {
            var allTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int iAssembly = 0; iAssembly < assemblies.Length; iAssembly++)
            {
                var types = assemblies[iAssembly].GetTypes();
                for (int iType = 0; iType < types.Length; iType++)
                {
                    var type = types[iType];
                    if (!baseType.IsAssignableFrom(type) || type.IsAbstract)
                        continue;
                    allTypes.Add(type);
                }
            }

            allTypes.Sort((a, b) => a.FullName.CompareTo(b.FullName));
            return allTypes;
        }

        private int IndexOfTool(Type type)
        {
            for (int i = 0; i < _Tools.Count; i++)
                if (_Tools[i].GetType() == type)
                    return i;

            return -1;
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;
            for (int i = 0; i < _Tools.Count; i++)
                _Tools[i].OnDisable();
        }

        private void OnSelectionChange()
        {
            for (int i = 0; i < _Tools.Count; i++)
                _Tools[i].OnSelectionChanged();

            Repaint();
        }

        public void CreateGUI()
        {
            var scrollViewElement = new ScrollView();
            for (int i = 0; i < _Tools.Count; i++)
            {
                var toolElement = _Tools[i].CreateGUI();
                scrollViewElement.Add(toolElement);
                scrollViewElement.Add(new Box());
            }
            scrollViewElement.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            rootVisualElement.Add(scrollViewElement);
        }

        public static new void Repaint() => ((EditorWindow)Instance).Repaint();

        [MenuItem("Window/AIToolBox")]
        public static void Open() => GetWindow<AIToolBoxWindow>();
        public static void Open<T>() where T : Tool
        {
            var window = GetWindow<AIToolBoxWindow>();
            window._CurrentTool = window.IndexOfTool(typeof(T));
        }

        public abstract class Tool : IComparable<Tool>
        {
            private int _Index;
            public bool IsExpanded
            {
                get { return Instance.CurrentTool == _Index; }
                set
                {
                    if (value)
                        Instance.CurrentTool = _Index;
                    else if (IsExpanded)
                        Instance.CurrentTool = -1;
                }
            }
            public bool IsVisible => Instance.CurrentTool == _Index || Instance.CurrentTool < 0;
            public virtual int DisplayOrder { get; } = 0;
            protected VisualElement element { get; private set; }
            public int CompareTo(Tool other)
                => DisplayOrder.CompareTo(other.DisplayOrder);

            public abstract string Name { get; }

            public abstract string Instructions { get; }

            public virtual void OnSelectionChanged()
            {
                if (element != null)
                {
                    element.style.display = IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }

            public virtual void OnEnable(int index)
            {
                _Index = index;
            }

            public virtual void OnDisable() { }

            public virtual VisualElement CreateGUI()
            {
                element = new VisualElement();
                element.style.display = IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
                var toggle = new ToolbarToggle();
                toggle.text = Name;
                toggle.value = IsExpanded;
                var foldout = new GroupBox();
                foldout.style.display = IsExpanded ? DisplayStyle.Flex : DisplayStyle.None;
                toggle.RegisterValueChangedCallback((evt) =>
                {
                    IsExpanded = evt.newValue;
                    foldout.style.display = IsExpanded ? DisplayStyle.Flex : DisplayStyle.None;
                });
                element.Add(toggle);
                element.Add(foldout);
                var container = new Box();
                container.style.marginLeft = 0;
                container.style.marginRight = 0;
                container.style.marginTop = 0;
                container.style.marginBottom = 0;

                if (!string.IsNullOrEmpty(Instructions))
                {
                    container.Add(new IMGUIContainer() { onGUIHandler = OnDrawHead });
                }

                container.Add(CreateBodyElement());
                foldout.Add(container);
                return element;
            }

            public virtual void OnDrawHead()
            {
                var instructions = Instructions;
                if (!string.IsNullOrEmpty(instructions))
                    EditorGUILayout.HelpBox(instructions, MessageType.Info);
            }

            protected abstract VisualElement CreateBodyElement();
        }
    }
}
