using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EasyScrollView
{
    public delegate float EasingFunction(float t);
    
    class AutoScrollState
    {
        public static readonly EasingFunction DefaultEasingFunction = Linear;
        static float Linear(float t) => t;
        
        public bool Enable;
        public bool Elastic;
        public float Duration;
        public EasingFunction EasingFunction;
        public float StartTime;
        public Vector2 EndPosition;

        public Action OnComplete;

        public void Reset()
        {
            Enable = false;
            Elastic = false;
            Duration = 0f;
            StartTime = 0f;
            EasingFunction = DefaultEasingFunction;
            EndPosition = Vector2.zero;
            OnComplete = null;
        }
        
        public void Complete()
        {
            OnComplete?.Invoke();
            Reset();
        }
    }

    /// <summary>
    /// 左上角为原点 计算content的位置
    /// </summary>
    public class Scroller : UIBehaviour, IPointerUpHandler, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
    {
        /// <summary>
        /// 视口大小
        /// </summary> 
        [SerializeField]
        private RectTransform viewport = default;
        public RectTransform Viewport
        {
            set
            {
                viewport = value;
            }
            get { return viewport; }
        }
        
        [SerializeField] 
        private RectTransform content;

        public RectTransform Content
        {
            get => content;
            set => content = value;
        }

        
        /// <summary>
        /// 内容区域的大小
        /// </summary>
        [SerializeField]
        private Vector2 contentSize;
        public Vector2 ContentSize
        {
            set
            {
                contentSize = value;
                
                if (content != null)
                {
                    content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,contentSize.x);
                    content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,contentSize.y);
                }
            }
            get => contentSize;
        }
        
        /// <summary>
        /// 移动方向
        /// </summary>
        [SerializeField]
        private ScrollDirection scrollDirection = ScrollDirection.Vertical;
        public ScrollDirection ScrollDirection
        {
            set
            {
                scrollDirection = value;
            }
            get => scrollDirection;
        }

        
        
        public Vector2 ViewportSize => viewport.rect.size;
        
        public float ViewportSizeX => viewport.rect.size.x;
        
        public float ViewportSizeY => viewport.rect.size.y;
        
        /// <summary>
        /// 移动类型
        /// </summary>
        [SerializeField]
        private MovementType movementType = MovementType.Elastic;
        public MovementType MovementType
        {
            get => movementType;
            set => movementType = value;
        }

        /// <summary>
        /// 弹力
        /// </summary>
        [SerializeField]
        private float elasticity = 0.1f;
        public float Elasticity
        {
            get => elasticity;
            set => elasticity = value;
        }

        /// <summary>
        /// 滚动灵敏度
        /// </summary>
        [SerializeField]
        private float scrollSensitivity = 1f;
        public float ScrollSensitivity
        {
            get => scrollSensitivity;
            set => scrollSensitivity = value;
        }

        /// <summary>
        /// 惯性
        /// </summary>
        [SerializeField]
        private bool inertia = true;
        public bool Inertia
        {
            get => inertia;
            set => inertia = value;
        }

        /// <summary>
        /// 减速速率
        /// </summary>
        [SerializeField]
        private float decelerationRate = 0.03f;
        public float DecelerationRate
        {
            get => decelerationRate;
            set => decelerationRate = value;
        }

        /// <summary>
        /// 是否可以拖动
        /// </summary>
        [SerializeField] 
        private bool draggable = true;
        public bool Draggable
        {
            get => draggable;
            set => draggable = value;
        }

        /// <summary>
        ///  滚动条
        /// </summary>
        [SerializeField]
        public Scrollbar horizontalScrollbar;
        [SerializeField]
        public Scrollbar verticalScrollbar;
        
        #region Snap
        /// <summary>
        /// 是否开启snap
        /// </summary>
        [SerializeField] 
        private bool snapEnable = true;
        public bool SnapEnable
        {
            get => snapEnable;
            set => snapEnable = value;
        }
        
        /// <summary>
        /// 判断snap的移动速度阈值
        /// </summary>
        [SerializeField] 
        private float snapVelocityThreshold = 100f;
        public float SnapVelocityThreshold
        {
            get => snapVelocityThreshold;
            set => snapVelocityThreshold = value;
        }
        
        /// <summary>
        /// snap动作时间
        /// </summary>
        [SerializeField] 
        private float snapDuration = 0.35f;
        public float SnapDuration
        {
            get => snapDuration;
            set => snapDuration = value;
        }
        
        [SerializeField] 
        private EasingFunction snapEasing;
        public EasingFunction SnapEasing
        {
            get => snapEasing;
            set => snapEasing = value;
        }

        [SerializeField] 
        private bool enableFastSwap = true;
        public bool EnableFastSwap
        {
            get => enableFastSwap;
            set => enableFastSwap = value;
        }
      
        [SerializeField] 
        private float fastSwipeThreshold = 100f;
        public float FastSwipeThreshold
        {
            get => fastSwipeThreshold;
            set => fastSwipeThreshold = value;
        }
        
        #endregion

        #region AutoScrollState
        
        private AutoScrollState autoScrollState = new AutoScrollState();
        
        #endregion
      
        private Vector2 _beginDragPointerPosition;
        private Vector2 _scrollStartPosition;
        private Vector2 _prevPosition;
        [SerializeField]
        private Vector2 _currentPosition;

        private bool _hold;
        private bool _scrolling;
        private bool _dragging;
        private Vector2 _velocity;
 
        /// <summary>
        /// 内容区域内的位置
        /// </summary>
        public Vector2 Position
        {
            get => _currentPosition;
            set
            {
                autoScrollState.Reset();
                _velocity = Vector2.zero;
                _dragging = false;

                UpdatePosition(value);
            }
        }

        public Vector2 NormalizedPosition
        {
            get => _currentPosition/(contentSize-ViewportSize);
            set
            {
                var p = value * (contentSize - ViewportSize); 
                Position = p;
            }
        }
        
        private Action<Vector2> _onValueChanged;
        private Func<Vector2,Vector2> _snapMethod;
        private Func<int,Vector2> _fastSwapMethod;
        
        public void OnValueChanged(Action<Vector2> callback) => _onValueChanged = callback;
        
        public void SnapMethod(Func<Vector2,Vector2> method) => _snapMethod = method;
        
        public void FastSwapMethod(Func<int,Vector2> method) => _fastSwapMethod = method;

        protected override void Awake()
        {
            base.Awake();
            if (content != null)
            {
                ContentSize = Vector2.Max(viewport.rect.size, content.rect.size);
                RectTransformUtil.SetPositionWithPivot(content,Vector2.up,new Vector2(-_currentPosition.x,_currentPosition.y));
            }
        }

        /// <summary>
        /// 更新当前位置
        /// </summary>
        /// <param name="position"></param>
        /// <param name="updateScrollbar"></param>
        void UpdatePosition(Vector2 position, bool updateScrollbar = true)
        {
            if (scrollDirection == ScrollDirection.Vertical)
            {
                position.x = 0;
            }
            if (scrollDirection == ScrollDirection.Horizontal)
            {
                position.y = 0;
            }
          
            _currentPosition = position;
            _onValueChanged?.Invoke(_currentPosition);

            if (content)
            {
                RectTransformUtil.SetPositionWithPivot(content,Vector2.up,new Vector2(-_currentPosition.x,_currentPosition.y));
            }
            
            if (horizontalScrollbar && updateScrollbar)
            {
                horizontalScrollbar.value = Mathf.Clamp01(position.x / Mathf.Max(contentSize.x - ViewportSize.x, 1e-4f));
            }
            if (verticalScrollbar && updateScrollbar)
            {
                verticalScrollbar.value = Mathf.Clamp01(position.y / Mathf.Max(contentSize.y - ViewportSize.y, 1e-4f));
            }
        }
        
        /// <summary>
        /// 循环位置
        /// </summary>
        /// <param name="p"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Vector2 CircularPosition(Vector2 p, Vector2 size,Vector2 viewSize)
        {
            var ret = Vector2.zero;
            ret.x = size.x < viewSize.x ? 0 : p.x < -viewSize.x ? size.x - viewSize.x + (p.x + viewSize.x) % size.x : p.x % size.x;
            ret.y = size.y < viewSize.y ? 0 : p.y <  -viewSize.y ? size.y - viewSize.y + (p.y + viewSize.x) % size.y : p.y % size.y;
            return ret;
        }

        Vector2 CalculateMovementAmount(Vector2 sourcePosition, Vector2 destPosition)
        {
            var ret = Vector2.zero;
            if (movementType != MovementType.Unrestricted)
            {
                var x = Mathf.Clamp(destPosition.x, 0, contentSize.x - ViewportSize.x) - sourcePosition.x;
                var y = Mathf.Clamp(destPosition.y, 0, contentSize.y - ViewportSize.y) - sourcePosition.y;
                ret.x = x;
                ret.y = y;
                return ret;
            }

            var amount = destPosition - sourcePosition;
            if (!enableFastSwap)
            {
                amount = CircularPosition(destPosition, contentSize, ViewportSize) -
                             CircularPosition(sourcePosition, contentSize, ViewportSize);
                if (Mathf.Abs(amount.x) > contentSize.x * 0.5f)
                {
                    amount.x = Mathf.Sign(-amount.x) * (contentSize.x - Mathf.Abs(amount.x));
                }

                if (Mathf.Abs(amount.y) > contentSize.y * 0.5f)
                {
                    amount.y = Mathf.Sign(-amount.y) * (contentSize.y - Mathf.Abs(amount.y));
                }
            }
            
          
            return amount;
        }

        
        public void ScrollTo(Vector2 position, float duration, EasingFunction easingFunction, Action onComplete = null)
        {
            if (duration <= 0f)
            {
                Position = CircularPosition(position, contentSize,ViewportSize);
                onComplete?.Invoke();
                return;
            }

            autoScrollState.Reset();
            autoScrollState.Enable = true;
            autoScrollState.Duration = duration;
            autoScrollState.EasingFunction = easingFunction ?? AutoScrollState.DefaultEasingFunction;
            autoScrollState.StartTime = Time.unscaledTime;
            autoScrollState.EndPosition = _currentPosition + CalculateMovementAmount(_currentPosition, position);
            autoScrollState.OnComplete = onComplete;
            _velocity = Vector2.zero;
            _scrollStartPosition = _currentPosition;
        }

        public void MoveTo(Vector2 position)
        {
            ScrollTo(position, snapDuration, snapEasing);
        }

        Vector2 CalculateOffset(Vector2 position)
        {
            Vector2 ret =Vector2.zero;
            if (movementType == MovementType.Unrestricted)
            {
                return ret;
            }
            ret.x = position.x < 0f ? -position.x : (position.x >  contentSize.x - ViewportSize.x) ? contentSize.x-ViewportSize.x-position.x : 0;
            ret.y = position.y < 0f ? -position.y :(position.y > contentSize.y - ViewportSize.y)? contentSize.y-ViewportSize.y-position.y : 0;

            if (scrollDirection == ScrollDirection.Vertical)
            {
                ret.x = 0;
            }
            if (scrollDirection == ScrollDirection.Horizontal)
            {
                ret.y = 0;
            }
            
            return ret;
        }

        private float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }
        
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!draggable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _hold = true;
            _velocity = Vector2.zero;
            autoScrollState.Reset();
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!draggable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (_hold && snapEnable)
            {
                var snapPos = _snapMethod?.Invoke(_currentPosition) ?? _currentPosition;
                ScrollTo(snapPos, snapDuration, snapEasing);
            }
            _hold = false;
        }

      

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!draggable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            _hold = false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                viewport,
                eventData.position,
                eventData.pressEventCamera,
                out _beginDragPointerPosition);
            _scrollStartPosition = _currentPosition;
            _dragging = true;
            autoScrollState.Reset();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!draggable || eventData.button != PointerEventData.InputButton.Left || !_dragging)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                viewport,
                eventData.position,
                eventData.pressEventCamera,
                out var dragPointerPosition))
            {
                return;
            }

            var pointerDelta = dragPointerPosition - _beginDragPointerPosition;
            pointerDelta.x *= -1f;
            var position =  pointerDelta * scrollSensitivity + _scrollStartPosition;
            var offset = CalculateOffset(position);
            position += offset;
            if (movementType == MovementType.Elastic)
            {
                if (offset.magnitude>0f)
                {
                    if (offset.x != 0)
                        position.x += - RubberDelta(offset.x, ViewportSize.x);
                    if (offset.y != 0)
                        position.y += - RubberDelta(offset.y, ViewportSize.y);
                }
            } 
            UpdatePosition(position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!draggable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            _dragging = false;
            if (!inertia && snapEnable)
            {
                if (EnableFastSwap)
                {
                    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        viewport,
                        eventData.position,
                        eventData.pressEventCamera,
                        out var dragPointerPosition))
                    {
                        return;
                    }
                    var pointerDelta = dragPointerPosition - _beginDragPointerPosition;
                    pointerDelta.x *= -1f;

                    var direct = scrollDirection == ScrollDirection.Horizontal
                        ? Mathf.Sign(pointerDelta.x) * (1+Mathf.Sign(Mathf.Abs(pointerDelta.x) - fastSwipeThreshold))
                        : Mathf.Sign(pointerDelta.y) * (1+Mathf.Sign(Mathf.Abs(pointerDelta.y) - fastSwipeThreshold));
                    direct = direct == 0 ? 0 : Mathf.Sign(direct);
                    Debug.Log(direct);
                    var snapPos = _fastSwapMethod?.Invoke((int)direct) ?? _currentPosition;
                    ScrollTo(snapPos, snapDuration, snapEasing);
                }
                else
                {
                    var snapPos = _snapMethod?.Invoke(_currentPosition) ?? _currentPosition;
                    ScrollTo(snapPos, snapDuration, snapEasing);
                }
            }
        }

        
        public void OnScroll(PointerEventData eventData)
        {
            if (!draggable)
            {
                return;
            }
            var delta = eventData.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            Vector2 scrollDelta = Vector2.zero;
            scrollDelta.x = delta.x;
            scrollDelta.y = delta.y;
            if (eventData.IsScrolling())
            {
                _scrolling = true;
            }

            var position = _currentPosition + scrollDelta  * scrollSensitivity;
            if (movementType == MovementType.Clamped)
            {
                position += CalculateOffset(position);
            }
            if (autoScrollState.Enable)
            {
                autoScrollState.Reset();
            }

            UpdatePosition(position);
        }
        
        void Update()
        {
            if (!viewport)
                return;

            var deltaTime = Time.unscaledDeltaTime;
            var offset = CalculateOffset(_currentPosition);

            if (autoScrollState.Enable)
            {
                var position = Vector2.zero;

                if (autoScrollState.Elastic)
                {
                    position = Vector2.SmoothDamp(_currentPosition, _currentPosition + offset, ref _velocity,
                        elasticity, Mathf.Infinity, deltaTime);

                    if (_velocity.magnitude < 0.01f)
                    {
                        position.x = Mathf.Clamp( position.x, 0, contentSize.x-ViewportSize.x);
                        position.y = Mathf.Clamp( position.y, 0, contentSize.y-ViewportSize.y);
                        _velocity = Vector2.zero;
                        autoScrollState.Complete();
                    }
                }
                else
                {
                    var alpha = Mathf.Clamp01((Time.unscaledTime - autoScrollState.StartTime) /
                                               Mathf.Max(autoScrollState.Duration, float.Epsilon));
                    position = Vector2.LerpUnclamped(_scrollStartPosition, autoScrollState.EndPosition,
                        autoScrollState.EasingFunction(alpha));

                    if (Mathf.Approximately(alpha, 1f))
                    {
                        autoScrollState.Complete();
                    }
                }

                UpdatePosition(position);
            }
            else if (!(_dragging || _scrolling) && (!(offset.magnitude<0.001f) || !(_velocity.magnitude<0.001f)))
            {
                var position = _currentPosition;

                if (movementType == MovementType.Elastic && !(offset.magnitude<0.001f))
                {
                    autoScrollState.Reset();
                    autoScrollState.Enable = true;
                    autoScrollState.Elastic = true;
                }
                else if (inertia)
                {
                    _velocity *= Mathf.Pow(decelerationRate, deltaTime);

                    if ( _velocity.magnitude < 0.001f)
                    {
                        _velocity =Vector2.zero;
                    }

                    position += _velocity * deltaTime;

                    if (snapEnable && _velocity.magnitude < snapVelocityThreshold)
                    {
                        var snapPos = _snapMethod?.Invoke(position) ?? position;
                        ScrollTo(snapPos, snapDuration, snapEasing);
                    }
                }
                else
                {
                    autoScrollState.Reset();
                    autoScrollState.Enable = true;
                    _velocity = Vector2.zero;
                }

                if (!(_velocity.magnitude<0.001f))
                {
                    if (movementType == MovementType.Clamped)
                    {
                        offset = CalculateOffset(position);
                        position += offset;

                        if (position.magnitude<0.001f || (position - contentSize - ViewportSize).magnitude<0.001f)
                        {
                            _velocity = Vector2.zero;
                        }
                    }
                    UpdatePosition(position);
                }  
            }
            else
            {
               
            }
            
            if (!autoScrollState.Enable && (_dragging || _scrolling) && inertia)
            {
                var newVelocity = (_currentPosition - _prevPosition) / deltaTime;
                _velocity = Vector2.Lerp(_velocity, newVelocity, deltaTime * 10f);
            }
            
            _prevPosition = _currentPosition;
            _scrolling = false;
        }
    }
}
