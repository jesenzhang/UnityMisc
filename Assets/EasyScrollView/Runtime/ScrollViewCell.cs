using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyScrollView
{
    public class ScrollViewCell
    {
        public RectTransform rectTransform;
        
        private Rect _mRect; 
        public int Index { get; set; } = -1;
        
        public int LayoutIndex { get; set; } = -1;
        
        public ScrollViewCell(GameObject gameObject,int dataIndex)
        {
            rectTransform = gameObject.GetComponent<RectTransform>();
            Index = dataIndex;
        }

        public void SetVisible(bool visible)
        {
            rectTransform.gameObject.SetActive(visible);
        }

        public void UpdateRect(float x, float y, float width, float height)
        {
            _mRect.x = x;
            _mRect.y = y;
            _mRect.width = width;
            _mRect.height = height;
            ApplyRect();
        }
        
        public void UpdatePosition(float x, float y)
        {
            _mRect.x = x;
            _mRect.y = y;
            ApplyRect();
        }
        
        public void Refresh()
        {
            ApplyRect();
        }
        
        public Vector2 Position()
        {
            return _mRect.position;
        }
        public Vector2 Size()
        {
            return _mRect.size;
        }
        
        public bool IsVisible(Vector2 pos,Vector2 size)
        {
            var s = Size();
            var min =pos - s;
            var max =pos + size;
            var v = Position();
            return v.x > min.x && v.x < max.x && v.y > min.y && v.y < max.y;
        }
        /// <summary>
        /// 是否相交
        /// </summary>
        /// <param name="otherRect"></param>
        /// <returns></returns>
        public bool Overlaps(ScrollViewCell otherRect)
        {
            return _mRect.Overlaps(otherRect._mRect);
        }

        /// <summary>
        /// 是否相交
        /// </summary>
        /// <param name="otherRect"></param>
        /// <returns></returns>
        public bool Overlaps(Rect otherRect)
        {
            return _mRect.Overlaps(otherRect,true);
        }

        private void ApplyRect()
        {
            /*rectTransform.anchoredPosition = new Vector2(_mRect.position.x + rectTransform.pivot.x * _mRect.size.x,
                _mRect.position.y + (rectTransform.pivot.y-1) * _mRect.size.y);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Size().y);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Size().x);*/
            
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _mRect.height);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _mRect.width);
            RectTransformUtil.SetPositionWithPivot(rectTransform,Vector2.up,_mRect.position);
        }
    }
    
}
