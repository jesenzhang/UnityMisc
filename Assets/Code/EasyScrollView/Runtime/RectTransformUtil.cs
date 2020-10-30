
using UnityEngine;

namespace EasyScrollView
{
    public static class RectTransformUtil
    {
        public static void SetPositionWithPivot(RectTransform rectTransform,Vector2 pivot,Vector3 pos)
        {
            var pivot1 = rectTransform.pivot;
            var rect = rectTransform.rect;
            var offset = Vector2.zero;
            var parent = rectTransform.parent as RectTransform;
            var size = parent!=null ? parent.rect.size :Vector2.zero;
            var anchorMin = rectTransform.anchorMin;
            var anchorMax = rectTransform.anchorMax;
            if (anchorMax == anchorMin)
            {
                offset = (pivot - anchorMax)*size;
            }

            Vector3 newPos = pos + (Vector3)(rect.size*(pivot1 - pivot) + offset);
            rectTransform.anchoredPosition3D = newPos;
        }
    }
}

