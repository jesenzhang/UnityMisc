using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EasyText
{
    [ExecuteInEditMode]
    public class SpriteEmoji : MaskableGraphic
    {
        #region 属性
        //默认shader
        private const string _defaultShader = "UI/UI-Sprite-Emoji";
        private Material _defaultMater = null;

        public SpriteAsset m_spriteAsset;
        private List<UIVertex> Verts = new List<UIVertex>();
        
        
        ////分割数量
        //[SerializeField]
        //private int _cellAmount = 1;
        ////动画速度
        //[SerializeField]
        //private float _speed;
        //顶点缓存数据
        readonly UIVertex[] _tempVerts = new UIVertex[4];


        public override Texture mainTexture
        {
            get
            {
                if (m_spriteAsset == null || m_spriteAsset.TexSource == null)
                    return base.mainTexture;
                else
                    return m_spriteAsset.TexSource;
            }
        }

        public override Material material
        {
            get
            {
                if (_defaultMater == null && m_spriteAsset != null)
                {
                    _defaultMater = new Material(Shader.Find(_defaultShader));
                    //是否开启动画
                    if (m_spriteAsset.IsStatic)
                        _defaultMater.DisableKeyword("EMOJI_ANIMATION");
                    else
                    {
                        _defaultMater.EnableKeyword("EMOJI_ANIMATION");
                        _defaultMater.SetFloat("_CellAmount", m_spriteAsset.Column);
                        _defaultMater.SetFloat("_Speed", m_spriteAsset.Speed);
                    }
                }
                return _defaultMater;
            }
        }
        #endregion

        
            
        public void ClearEmojiVert()
        {
             Verts.Clear();
        }
        
        public void AddEmojiVert(UIVertex vertex)
        {
            vertex.position = Utility.TransformWorld2Point(transform, vertex.position);
            if(!Verts.Contains(vertex))
                Verts.Add(vertex);
        }
        
        public void RemoveEmojiVert(UIVertex vertex)
        {
            if(Verts.Contains(vertex))
                Verts.Remove(vertex);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            //在这里可以做一个数据判断，如果数据一样 就不再刷新
            if (Verts != null)
            {
                for (int i = 0; i < Verts.Count; i++)
                {
                    int tempVertsIndex = i & 3;
                    _tempVerts[tempVertsIndex].position =Verts[i].position;// Utility.TransformWorld2Point(transform, _meshInfo.Vertices[i]);
                    _tempVerts[tempVertsIndex].uv0 = Verts[i].uv1;
                    _tempVerts[tempVertsIndex].color = Verts[i].color;
                    if (tempVertsIndex == 3)
                        vh.AddUIVertexQuad(_tempVerts);
                }
            }
        }
    }
}

