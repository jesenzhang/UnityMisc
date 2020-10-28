using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyScrollView
{
    [RequireComponent(typeof(Scroller))]
    public class ScrollView : MonoBehaviour
    {
        [SerializeField]
        private Scroller scroller;
        protected Scroller Scroller => scroller ? scroller : (scroller = GetComponent<Scroller>());

        [SerializeField] 
        private RectTransform content;

        public RectTransform Content
        {
            get => content;
            set => content = value;
        }
        
        public RectTransform ViewPort
        {
            get => Scroller.Viewport;
        }
        
        /// <summary>
        /// 布局模式 分页
        /// </summary>
        [SerializeField]
        public LayoutMode layoutMode;
        
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
        
        [SerializeField]
        protected LayoutPadding m_Padding;
        public LayoutPadding Padding
        {
            get { return m_Padding; }
            set {m_Padding=value; }
        }

        [SerializeField] 
        protected LayoutAlignment m_ChildAlignment = LayoutAlignment.UpperLeft;
        public LayoutAlignment ChildAlignment
        {
            get { return m_ChildAlignment; }
            set { m_ChildAlignment =value; }
        }
        
        
        public GameObject cellPrefab;
        
        //固定元素大小
        public bool fixedCellSize = true;
      
        public Vector2 cellSize;
        /// 单元格间隙（水平，垂直）
        public Vector2 cellSpacingSize;
        
        /// <summary>
        /// 数据提供者
        /// </summary>
        private IList _dataProviders;

        private List<ScrollViewCell> _cellPool;
        private List<ScrollViewCell> _visibleCells;
   
        
        /// <summary>
        /// 是否初始化
        /// </summary>
        protected bool initialized;

        /// <summary>
        /// 当前位置.
        /// </summary>
        protected Vector2 currentPosition;
        
        protected Vector2 lastPosition;
        
       
        /// <summary>
        /// 当前虚拟内容区的大小
        /// </summary>
        protected Vector2 contentSize;
        public Vector2 ContentSize
        {
            set
            {
                contentSize = value;
                Scroller.ContentSize = contentSize;
                if (content != null)
                {
                    if (fixedContent || infinity)
                    {
                        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,Scroller.ViewportSize.x);
                        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,Scroller.ViewportSize.y);
                    }
                    else
                    {
                        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,contentSize.x);
                        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,contentSize.y);
                        RectTransformUtil.SetPositionWithPivot(content,Vector2.up,new Vector2(-currentPosition.x,currentPosition.y));
                    }
                }
            }
            get => contentSize;
        }

        
        [SerializeField] 
        private bool infinity = false;
        public bool Infinity
        {
            get => infinity;
            set
            {
                infinity = value;
                if (infinity)
                {
                    fixedContent = true;
                    Scroller.MovementType = MovementType.Unrestricted;
                }
            }
        }
        
        [SerializeField] 
        private bool fixedContent = true;
        public bool FixedContent
        {
            get => fixedContent;
            set
            {
                fixedContent = value;
            }
        }

        private Action<int> _onSelectionChanged;
        public void OnSelectionChanged(Action<int> callback) => _onSelectionChanged = callback;
        
        private Func<GameObject,int,Vector2> _onCellUpdate;
        public void OnCellUpdate(Func<GameObject,int,Vector2> callback) => _onCellUpdate = callback;
        
        private Action<GameObject,int,Vector2> _onCellMoved;
        public void OnCellMoved(Action<GameObject,int,Vector2> callback) => _onCellMoved = callback;

        private ScrollViewCell _headCell;
        private ScrollViewCell _tailCell;

        //分页时 页大小
        private Vector2 _pageSize;
        private int _pageIndex = 0;
        private int _pageCol ;
        private int _pageRow;
        private int _pageCellCount;
        private int _pageCount;
        private Vector2 _sizeWithOutPadding;
        private Vector2 _sizeWithPadding;
        
        
        static Vector2 _verParam = new Vector2(1,-1); 
        private void Awake()
        {
            if (!initialized)
            {
                Initialize();
                initialized = true;
            }
       //     UpdateContents(new int[200]);
        }

        private void OnEnable()
        {
           
        }

        protected virtual void Initialize()
        {
            ContentSize =  Scroller.ViewportSize;
            _visibleCells = new List<ScrollViewCell>();
            _cellPool = new List<ScrollViewCell>();
            Scroller.ScrollDirection = ScrollDirection;
            Scroller.OnValueChanged(OnScrollerValueChanged);
            Scroller.SnapMethod(SnapMethod);
            Scroller.FastSwapMethod(FastSwap);
            Infinity = infinity;
            Relayout();
        }

        void OnScrollerValueChanged(Vector2 p)
        {
            UpdatePosition(p);
        }

        public void MoveToPage(float page)
        {
            MoveToPage((int) page);
        }

        public void MoveToPage(int page)
        {
            _pageIndex = Mathf.Clamp(page, 0, _pageCount-1);
            var pagePos = Vector2.zero;
            pagePos.x = ScrollDirection == ScrollDirection.Horizontal ? _pageIndex * _pageSize.x : 0;
            pagePos.y = ScrollDirection == ScrollDirection.Vertical ? _pageIndex * _pageSize.y : 0;
            Scroller.MoveTo(pagePos);
        }

        public void NextPage()
        {
            _pageIndex++;
            MoveToPage(_pageIndex);
        }
        
        public void LastPage()
        {
            _pageIndex--;
            MoveToPage(_pageIndex);
        }
        
        /// <summary>
        /// 设置固定大小分页时的参数
        /// </summary>
        void SetPageParams()
        {
            _pageSize = Scroller.ViewportSize;
            _pageCol = Mathf.FloorToInt((_pageSize.x - Padding.horizontal + cellSpacingSize.x) / (cellSize.x + cellSpacingSize.x));
            _pageRow = Mathf.FloorToInt((_pageSize.y - Padding.vertical + cellSpacingSize.y) / (cellSize.y + cellSpacingSize.y));
            _pageCellCount = _pageRow* _pageCol;
            var dataCount = _dataProviders.Count;
            _pageCount = Mathf.CeilToInt( dataCount / (float)_pageCellCount);
            _sizeWithOutPadding.x = _pageCol*(cellSize.x+cellSpacingSize.x)-cellSpacingSize.x;
            _sizeWithOutPadding.y = _pageRow*(cellSize.y+cellSpacingSize.y)-cellSpacingSize.y;
            _sizeWithPadding.x = _sizeWithOutPadding.x + Padding.horizontal;
            _sizeWithPadding.y = _sizeWithOutPadding.y + Padding.vertical;
            cellSize = Vector2.Min(cellSize,_sizeWithOutPadding);
            contentSize.x = ScrollDirection == ScrollDirection.Horizontal
                ? _pageCount *_pageSize.x
                :  _pageSize.x;
            contentSize.y = ScrollDirection == ScrollDirection.Horizontal
                ? _pageSize.y
                : _pageCount *_pageSize.y;
            ContentSize =  Vector2.Max(_pageSize,contentSize);
        }

        void SetFreeParams()
        {
            _pageSize = Scroller.ViewportSize;
            _pageCol = Mathf.FloorToInt((_pageSize.x - Padding.horizontal + cellSpacingSize.x) / (cellSize.x + cellSpacingSize.x));
            _pageRow= Mathf.FloorToInt((_pageSize.y - Padding.vertical + cellSpacingSize.y) / (cellSize.y + cellSpacingSize.y));
            var dataCount = _dataProviders.Count;
            _sizeWithOutPadding.x = _pageCol*(cellSize.x+cellSpacingSize.x)-cellSpacingSize.x;
            _sizeWithOutPadding.y = _pageRow*(cellSize.y+cellSpacingSize.y)-cellSpacingSize.y;
            _sizeWithPadding.x = _sizeWithOutPadding.x + Padding.horizontal;
            _sizeWithPadding.y = _sizeWithOutPadding.y + Padding.vertical;
            contentSize = Vector2.zero;
            if (fixedCellSize)
            {
                contentSize.x = ScrollDirection == ScrollDirection.Horizontal
                    ? Mathf.CeilToInt((float)dataCount / _pageRow)* (cellSize.x + cellSpacingSize.x) - cellSpacingSize.x + Padding.horizontal
                    :  _pageSize.x;
                contentSize.y = ScrollDirection == ScrollDirection.Horizontal
                    ? _pageSize.y
                    : Mathf.CeilToInt((float) dataCount / _pageCol) * (cellSize.y + cellSpacingSize.y) - cellSpacingSize.y +
                      Padding.vertical;
            }
            
            ContentSize = Vector2.Max(_pageSize,contentSize);
        }
        
        Vector2 PagePosition(int page)
        {
            _pageIndex = infinity ? page : Mathf.Clamp(page, 0, _pageCount-1);
            var pagePos = Vector2.zero;
            pagePos.x = ScrollDirection == ScrollDirection.Horizontal ? _pageIndex * _pageSize.x : 0;
            pagePos.y = ScrollDirection == ScrollDirection.Vertical ? _pageIndex * _pageSize.y : 0;
            return pagePos;
        }
        
        Vector2 FastSwap(int forward)
        {
            if (layoutMode == LayoutMode.Paging)
            {
                return PagePosition(_pageIndex+forward);
            }
            return currentPosition;
        }
        
        Vector2 SnapMethod(Vector2 current)
        {
            if (layoutMode == LayoutMode.Paging)
            {
                var xy = current / _pageSize;
                var startPage = ScrollDirection == ScrollDirection.Horizontal ?  Mathf.RoundToInt(xy.x) :  Mathf.RoundToInt(xy.y);
                return PagePosition(startPage);
            }
            return current;
        }

        public virtual void UpdateContents(IList dataSource)
        {
            _dataProviders = dataSource;
            if (_dataProviders == null) return;
            if (!fixedCellSize)
            {
                _sizeList = new List<Vector2>(_dataProviders.Count);
                for (int i = 0; i < _dataProviders.Count; i++)
                {
                    _sizeList.Add(Vector2.zero);
                }
            }
            if(layoutMode == LayoutMode.Paging)
                SetPageParams();
            if(layoutMode == LayoutMode.Free)
                SetFreeParams();
            Relayout();
        }
        
        protected virtual void Relayout() => UpdatePosition(currentPosition);


        protected float GetAlignmentOnAxis(int axis)
        {
            if (axis == 0)
                return ((int)ChildAlignment % 3) * 0.5f;
            else
                return ((int)ChildAlignment / 3) * 0.5f;
        }

        protected float GetStartOffset(int axis,float availableSpace, float requiredSpaceWithoutPadding)
        {
            float requiredSpace = requiredSpaceWithoutPadding + (axis == 0 ? Padding.horizontal : Padding.vertical);
            float surplusSpace = availableSpace - requiredSpace;
            float alignmentOnAxis = GetAlignmentOnAxis(axis);
            return (axis == 0 ? Padding.left : Padding.top) + surplusSpace * alignmentOnAxis;
        }
        
        protected int FirstPageIndex(Vector2 firstPosition)
        {
            var pageIndexX = Mathf.FloorToInt((firstPosition.x) / (_pageSize.x ));
            var pageIndexY =  Mathf.FloorToInt( (firstPosition.y) / (_pageSize.y));
                
            if (scrollDirection == ScrollDirection.Horizontal)
            {
                return pageIndexX;
            }else
            if (scrollDirection == ScrollDirection.Vertical)
            {
                return pageIndexY;
            }
            else
            {
                return pageIndexX*pageIndexY;
            }
        }
        
        void SortCells()
        {
            _visibleCells.Sort((a, b) => { return a.LayoutIndex > b.LayoutIndex ? 1 : a.LayoutIndex < b.LayoutIndex ? -1 : 0; });
        }
        
        /// <summary>
        /// 回收不可见cell
        /// </summary>
        void UpdateVisibleCells(Vector2 delta)
        {
            int count = _visibleCells.Count;
            int index = 0;
            while (index < count)
            {
                var cell = _visibleCells[index];
                if (infinity || fixedContent)
                {
                    var pos = cell.Position();
                    pos.x -= delta.x;
                    pos.y += delta.y;
                    cell.UpdatePosition(pos.x,pos.y);
                }
                
                if (IsVisible(cell))
                {
                    cell.SetVisible(true);
                    _onCellMoved?.Invoke(cell.rectTransform.gameObject,cell.Index,cell.Position());
                    index++;
                }
                else
                {
                    cell.SetVisible(false);
                    _cellPool.Add(cell);
                    _visibleCells.Remove(cell);
                    count--;
                }
            }
            SortCells();
        }

        protected virtual void Refresh()
        {
            if(_dataProviders==null || _dataProviders.Count<=0)
                return;

            UpdateCells();
        }
        
        protected virtual void UpdatePosition(Vector2 position)
        {
            currentPosition = position;
            if (content)
            {
                if(infinity || fixedContent)
                {  RectTransformUtil.SetPositionWithPivot(content,Vector2.up,Vector3.zero);}
                else
                {   RectTransformUtil.SetPositionWithPivot(content,Vector2.up,new Vector2(-currentPosition.x,currentPosition.y));}
            }
            Refresh();
            lastPosition = currentPosition;
        }

        protected ScrollViewCell GetCell()
        {
            if (_cellPool.Count > 0)
            {
                var cell = _cellPool[0];
                _cellPool.RemoveAt(0);
                return cell;
            }
            var cellObj = Instantiate(cellPrefab,content);
            ScrollViewCell unicell = new ScrollViewCell(cellObj,0);
            return unicell;
        }
        
        protected bool IsVisible(ScrollViewCell cell)
        {
            var vp = Scroller.ViewportSize;
            var s = cell.Size();

            var min = Vector2.zero;
            var max = Vector2.zero;
            
            min.x = currentPosition.x - s.x;
            max.y = -currentPosition.y + s.y;
            max.x = currentPosition.x + vp.x;
            min.y = -currentPosition.y - vp.y;
            var offset =  Vector2.zero;

            offset.x += infinity||fixedContent ? currentPosition.x:0;
            offset.y -= infinity||fixedContent ? currentPosition.y:0;
            
            var v = cell.Position()+offset;
            return v.x > min.x && v.x < max.x && v.y > min.y && v.y < max.y;

        }
 
        Vector2 UpdateCell(ScrollViewCell cell)
        {
            if (_onCellUpdate == null)
                return cellSize;
            var size = _onCellUpdate.Invoke(cell.rectTransform.gameObject, cell.Index);
            return fixedCellSize ? cellSize : size;
        }

        #region TryUni
        
        private List<Vector2> _sizeList;
        private Vector2 _offset = Vector2.zero;
        
        void UpdateCells()
        {
            UpdateVisibleCells(currentPosition - lastPosition);
            //单页区域的align位移
            var alignOffsetX = GetStartOffset(0,ViewPort.rect.size.x, _sizeWithOutPadding.x);
            var alignOffsetY = GetStartOffset(1,ViewPort.rect.size.y, _sizeWithOutPadding.y);
            
            //单页区域的布局起始位置
            var pageStartX =  ScrollDirection == ScrollDirection.Horizontal ?  layoutMode == LayoutMode.Paging ? alignOffsetX : Padding.left : alignOffsetX;
            var pageStartY =  ScrollDirection == ScrollDirection.Horizontal ? alignOffsetY : layoutMode == LayoutMode.Paging ? alignOffsetY : Padding.top ;
            
            //头尾cell
            _headCell = _visibleCells.Count>=1 ? _visibleCells[0] : null;
            _tailCell = _visibleCells.Count>=1 ? _visibleCells[_visibleCells.Count - 1] :null;
            
            TryAddCell(_headCell,true,1,pageStartX,pageStartY);
            TryAddCell(_tailCell,false,1,pageStartX,pageStartY);
        }
        
        void TryAddCell(ScrollViewCell lastCell,bool forward,int indexOffset,float pagePosx,float pagePosy)
        {
            if (_dataProviders == null || _dataProviders.Count == 0)
                return;
            var beginLayoutIndex = lastCell!=null ? lastCell.LayoutIndex: forward ? 0:-indexOffset;
            var lastCellPos = lastCell!=null ? lastCell.Position():new Vector2(pagePosx,-pagePosy);
            var lastCellSize =  lastCell!=null ? lastCell.Size():Vector2.zero;
          
            var layoutIndex =  forward ? beginLayoutIndex-indexOffset:beginLayoutIndex+indexOffset;
            var dataIndex =  infinity ?  (_dataProviders.Count+layoutIndex % _dataProviders.Count) % _dataProviders.Count : layoutIndex ;
            layoutIndex = infinity ? layoutIndex : dataIndex;
            
            if (dataIndex < _dataProviders.Count && dataIndex>=0)
            {
                var cell = GetCell();
                cell.Index = dataIndex;
                cell.LayoutIndex = layoutIndex;
                var size = UpdateCell(cell);
                if(!fixedCellSize)
                    _sizeList[dataIndex] = size;

                var offset = infinity||fixedContent ? -currentPosition : Vector2.zero;
                if (fixedCellSize)
                {
                    var cellIndex = layoutMode == LayoutMode.Paging ? (infinity ? (_pageCellCount+cell.LayoutIndex %_pageCellCount) % _pageCellCount : cell.LayoutIndex%_pageCellCount): cell.LayoutIndex;
                    var pageIndex = layoutMode == LayoutMode.Paging ?  Mathf.FloorToInt((float)cell.LayoutIndex / _pageCellCount) : 0;
                    var pageStartX = ScrollDirection == ScrollDirection.Horizontal ? pageIndex * _pageSize.x : 0;
                    var pageStartY = ScrollDirection == ScrollDirection.Vertical ? -pageIndex * _pageSize.y : 0;
                    var col = ScrollDirection == ScrollDirection.Horizontal ? Mathf.FloorToInt(cellIndex / (float)_pageRow) :  (_pageCol+cellIndex % _pageCol)% _pageCol;
                    var row = ScrollDirection == ScrollDirection.Horizontal ?  (_pageRow + cellIndex % _pageRow)%_pageRow :   Mathf.FloorToInt( cellIndex / (float)_pageCol);
         
                    var x = pageStartX + pagePosx + col*(cellSize.x+cellSpacingSize.x)+ offset.x;
                    var y = pageStartY - pagePosy - row*(cellSize.y+cellSpacingSize.y)- offset.y;
                    cell.UpdateRect(x,y,cellSize.x,cellSize.y);
                }
                else
                { 
                    if (scrollDirection == ScrollDirection.Horizontal)
                    {
                        var x = forward ? lastCellPos.x - size.x - cellSpacingSize.x :  lastCellPos.x +lastCellSize.x + cellSpacingSize.x;
                        var y = lastCellPos.y;
                        
                        float alignmentOnAxis = GetAlignmentOnAxis(1);
                        
                        y =  -Padding.top - (ViewPort.rect.size.y-Padding.vertical) * alignmentOnAxis + size.y * (alignmentOnAxis) ;
                        
                        cell.UpdateRect(x,y,size.x,size.y);
                    }
                    if (scrollDirection == ScrollDirection.Vertical)
                    {
                        var y = forward ? lastCellPos.y + size.y + cellSpacingSize.y : lastCellPos.y - lastCellSize.y - cellSpacingSize.y;
                        var x = lastCellPos.x ;
                        
                        float alignmentOnAxis = GetAlignmentOnAxis(0);
                        
                        x = Padding.left + (ViewPort.rect.size.x-Padding.horizontal) * alignmentOnAxis - size.x * (alignmentOnAxis) ;
                        
                        cell.UpdateRect(x,y,size.x,size.y);
                    }
                }

                cell.rectTransform.gameObject.name = "" + cell.LayoutIndex;
                if (IsVisible(cell))
                {
                    _visibleCells.Add(cell);
                    cell.SetVisible(true);
                    if (!fixedCellSize)
                    {
                        var min = cell.Position() - new Vector2(pagePosx, pagePosy) * _verParam - offset * _verParam;
                        var max = cell.Position() + (cell.Size() + cellSpacingSize)* _verParam - offset* _verParam ;
                        max.x += Padding.horizontal - pagePosx;
                        max.y -= Padding.vertical - pagePosy;
                        var absX = Mathf.Abs(max.x);
                        var absY = Mathf.Abs(max.y);
                        
                        _offset.x =Mathf.Min(_offset.x,min.x);
                        _offset.y =Mathf.Max(_offset.y,min.y);
                      
                        Debug.Log("min "+min + "offset "+_offset);
                        if (_offset.x < 0)
                        {
                            absX -= _offset.x;
                        }
                        if (_offset.y > 0)
                        {
                            absY += _offset.y;
                        }
                        
                        if (absX > ContentSize.x || absY > ContentSize.y || _offset !=Vector2.zero)
                        {
                            max.x = Mathf.Max(absX, ContentSize.x);
                            max.y = Mathf.Max(absY, ContentSize.y);
                            ContentSize= max;
                            foreach (var c in _visibleCells)
                            {
                                c.UpdatePosition(c.Position().x - Mathf.Abs(_offset.x),c.Position().y +  Mathf.Abs(_offset.y));
                               // c.Refresh();
                            }
                            _offset = Vector2.zero;
                        }
                    }
                    TryAddCell(cell,forward,indexOffset,pagePosx, pagePosy);
                }
                else
                {
                    cell.SetVisible(false);
                    _cellPool.Add(cell);
                }
            }
        }

       
        
        #endregion
      
    }

}
