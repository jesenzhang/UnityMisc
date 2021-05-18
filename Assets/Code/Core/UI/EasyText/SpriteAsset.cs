using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EasyEngine.Base;

namespace EasyText
{
  
    public class SpriteAsset : ScriptableObject
    {
        /// <summary>
        /// 图集ID
        /// </summary>
        public int Id;
        /// <summary>
        /// 静态表情
        /// </summary>
        public bool IsStatic;
        /// <summary>
        /// 图片资源
        /// </summary>
        public Texture TexSource;
        /// <summary>
        /// 行
        /// </summary>
        public float Row;
        public Vector2 Offset;
        /// <summary>
        /// 列
        /// </summary>
        public float Column;
        /// <summary>
        /// 动态表情的切换速度
        /// </summary>
        public float Speed = 10;
        /// <summary>
        /// 所有sprite信息 SpriteAssetInfor类为具体的信息类
        /// </summary>
        public List<SpriteInfoGroup> ListSpriteGroup;
    }

    [System.Serializable]
    public class SpriteInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id;
        ///// <summary>
        ///// 名称
        ///// </summary>
        //public string Name;
        ///// <summary>
        ///// 中心点
        ///// </summary>
        //public Vector2 Pivot;
        /// <summary>
        ///坐标&宽高
        /// </summary>
        public Rect Rect;

        /// <summary>
        /// 绘画参数
        /// </summary>
        public Rect DrawTexCoord;

        ///// <summary>
        ///// 精灵
        ///// </summary>
        //public Sprite Sprite;
        ///// <summary>
        ///// 标签
        ///// </summary>
        //public string Tag;
        /// <summary>
        /// uv
        /// </summary>
        public Vector2[] Uv;
    }

    [System.Serializable]
    public class SpriteInfoGroup
    {
        public int Id = 0;
        public string Tag = "";
        public List<SpriteInfo> ListSpriteInfor = new List<SpriteInfo>();
        public float Width = 1.0f;
        public float Size = 24.0f;
    }
}