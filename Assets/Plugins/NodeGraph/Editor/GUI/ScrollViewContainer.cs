/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 设置窗口                                                                        *
*//************************************************************************************/

using System;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace UFrame.NodeGraph
{
    public class ScrollViewContainer
    {
        private Rect currentPosition;
        private ScrollView scrollView;
        private IMGUIContainer content;
        private VisualElement scrollViewContent;
        private float zoomSize = 1;
        public readonly float minZoomSize = 0.3f;
        public readonly float maxZoomSize = 1.3f;
        private ZoomManipulator zoomMa;
        public Action onGUI { get; set; }
        public float ZoomSize
        {
            get
            {
                return zoomSize;
            }
            set
            {
                zoomSize = zoomMa.SetZoom(value);
            }
        }
        public Vector2 scrollPos
        {
            get
            {
                return scrollView.scrollOffset;
            }
        }

        public void Start(VisualElement root, Rect position)
        {
            this.currentPosition = position;
            CreateScrollViewContent(position);
            CreateScrollViewContainer(position);
            CreateScrollView(position);
            CreateZoomManipulator(position);
            root.Add(scrollView);
        }

        public void UpdateScale(Rect position)
        {
            currentPosition = position;

            scrollView.style.marginTop = position.y;
            scrollView.style.marginLeft = position.x;
            scrollView.style.width = position.width;
            scrollView.style.height = position.height;

            content.style.width = position.width / minZoomSize;//内部固定大小（但scale在作用下会实现与ScrollViewContent一样大）
            content.style.height = position.height / minZoomSize;

            scrollViewContent.style.width = position.width * zoomSize / minZoomSize;//缩放容器以动态改变ScrollView的内部尺寸
            scrollViewContent.style.height = position.height * zoomSize / minZoomSize;
        }

        private void CreateScrollView(Rect position)
        {
            scrollView = new ScrollView()
            {
                style =
                {
                     marginTop = position.y,
                     marginLeft = position.x,
                     width = position.width,
                     height = position.height,
                     backgroundColor = Color.clear
                 },
                //showHorizontal = true,
                //showVertical = true,
                horizontalScrollerVisibility = ScrollerVisibility.Auto,
                verticalScrollerVisibility = ScrollerVisibility.Auto,
            };
            scrollView.mouseWheelScrollSize = 0;
            //scrollView.clippingOptions = VisualElement.ClippingOptions.ClipContents;
            scrollView.Add(scrollViewContent);
        }
        private void CreateScrollViewContainer(Rect position)
        {
            scrollViewContent = new VisualElement()
            {
                style = {
                    //top = 10,
                    //left = 10,
                    width = position.width / minZoomSize,
                    height = position.height / minZoomSize,
                    backgroundColor = Color.clear,
                    position = Position.Absolute
                }
            };
            scrollViewContent.Add(content);
        }
        private void CreateScrollViewContent(Rect position)
        {
            content = new IMGUIContainer(OnGUI)
            {
                style =
                {
                  //top = 10,
                  //left = 10,
                  width =  position.width / minZoomSize,
                  height = position.height / minZoomSize,
                  backgroundColor = Color.clear,
                  position = Position.Absolute
                }
            };
        }
        private void CreateZoomManipulator(Rect position)
        {
            zoomMa = new ZoomManipulator(minZoomSize, maxZoomSize, content);
            zoomMa.onZoomChanged = OnZoomValueChanged;
            zoomMa.onScrollMove = (arg1) =>
            {
                //Debug.LogError((-arg1)  +"," +(- arg1 * new Vector2(position.width, position.height)));
                scrollView.scrollOffset = -arg1 * new Vector2(position.width,position.height);
            };
            zoomMa.scrollPosGet = () => {
                return scrollView.scrollOffset; 
            };
            scrollView.AddManipulator(zoomMa);
        }
        private void OnGUI()
        {
            if (onGUI != null)
            {
                onGUI.Invoke();
            }
            else
            {
                Debug.Log("empty on Gui!");
            }
        }
        private void OnZoomValueChanged(float arg2)
        {
            zoomSize = arg2;

            var width = currentPosition.width * zoomSize / minZoomSize;
            var height = currentPosition.height * zoomSize / minZoomSize;
            scrollViewContent.style.width = width;//scrollViewContent的大小随着缩放变化
            scrollViewContent.style.height = height;

            //居中显示调整为居左上角显示
            content.style.left = -(currentPosition.width / minZoomSize - width) * 0.5f;
            content.style.top = -(currentPosition.height / minZoomSize - height) * 0.5f;
        }

        public void Refesh()
        {
            content.MarkDirtyRepaint();
            scrollViewContent.MarkDirtyRepaint();
            scrollView.MarkDirtyRepaint();
        }
    }

    public class ZoomManipulator : MouseManipulator, IManipulator
    {
        private VisualElement targetElement;
        private float minSize = 0.3f;
        private float maxSize = 1.3f;
        public readonly float zoomStep = 0.05f;

        public System.Action<float> onZoomChanged { get; set; }
        public System.Action<Vector2> onScrollMove { get; set; }
        public System.Func<Vector2> scrollPosGet { get; set; }

        public ZoomManipulator(float minSize, float maxSize, VisualElement element)
        {
            this.minSize = minSize;
            this.maxSize = maxSize;
            this.targetElement = element;
            base.activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse,
                modifiers = EventModifiers.Alt
            });
        }

        public float SetZoom(float zoom)
        {
            var scale = Mathf.Clamp(zoom, minSize, maxSize);
            //targetElement.transform.scale = Vector3.one * scale;
            targetElement.style.scale = new StyleScale(Vector3.one * scale);
            if (onZoomChanged != null)
            {
                onZoomChanged.Invoke(scale);
            }
            return scale;
        }
        protected override void RegisterCallbacksOnTarget()
        {
            base.target.RegisterCallback<WheelEvent>(OnScroll, TrickleDown.NoTrickleDown);
            base.target.RegisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.NoTrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            base.target.UnregisterCallback<WheelEvent>(OnScroll, TrickleDown.NoTrickleDown);
            base.target.UnregisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.NoTrickleDown);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (evt.altKey)
            {
                var offset = -scrollPosGet.Invoke();
                Vector2 delta = evt.mouseDelta;
                offset += delta;
                onScrollMove?.Invoke(offset);
            }
        }

        private void OnScroll(WheelEvent e)
        {
            Vector2 zoomCenter = VisualElementExtensions.ChangeCoordinatesTo(base.target, this.targetElement, e.localMousePosition);
            float zoomScale = 1f - e.delta.y * zoomStep;
            this.Zoom(zoomCenter, zoomScale);
            e.StopPropagation();
        }

        private void Zoom(Vector2 zoomCenter, float zoomScale)
        {
            var offset = -scrollPosGet.Invoke();
            var scale = Mathf.Clamp(this.targetElement.transform.scale.x * zoomScale, minSize, maxSize);
            this.targetElement.transform.scale = scale * Vector2.one;
            onZoomChanged?.Invoke(scale);

            var percentx = zoomCenter.x/targetElement.style.width.value.value;
            var percenty = zoomCenter.y/targetElement.style.height.value.value;
            offset = -new Vector2(percentx, percenty);
            onScrollMove?.Invoke(offset);
        }
    }
}