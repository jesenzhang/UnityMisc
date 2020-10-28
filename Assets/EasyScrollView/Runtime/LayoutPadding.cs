using System;

namespace EasyScrollView
{
    [Serializable]
    public class LayoutPadding
    {
        public int left;
     
        public int right;

        public int top;
        
        public int bottom;

        public int horizontal => left + right;

        public int vertical => top + bottom;

    }
}