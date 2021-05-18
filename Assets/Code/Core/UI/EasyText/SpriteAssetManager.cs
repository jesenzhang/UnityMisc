using System;
using System.Collections.Generic;
using EasyEngine.Base;
using UnityEngine;

namespace EasyText
{
    public class SpriteAssetManager : SingletonMono<SpriteAssetManager>
    {
        public SpriteAsset[] Assets;
        //所有的精灵消息
        public Dictionary<int, Dictionary<string, SpriteInfoGroup>> _indexSpriteInfo;
        public Dictionary<int, SpriteAsset> _spriteAssetDic;

        private bool _builded = false;
        public Dictionary<int, Dictionary<string, SpriteInfoGroup>> IndexSpriteInfo
        {
            get
            {
                if(_indexSpriteInfo==null)
                    _indexSpriteInfo = new Dictionary<int, Dictionary<string, SpriteInfoGroup>>();
                return _indexSpriteInfo;
            }
        }
        
        public Dictionary<int, SpriteAsset> SpriteAssetDic
        {
            get
            {
                if(_spriteAssetDic==null)
                    _spriteAssetDic = new Dictionary<int, SpriteAsset>();
                return _spriteAssetDic;
            }
        }
        public override void Init()
        {
            base.Init();
        }
        void OnEnable()
        {
          
        }
        
        public void ReBuild()
        {
            Clear();
            if(Assets!=null && Assets.Length>0)
            {
                foreach (var a in Assets)
                {
                    Reg(a);
                }
            }
        }

        public void Reg(SpriteAsset spriteAsset)
        {
            if (!SpriteAssetDic.ContainsKey(spriteAsset.Id))
            {
                SpriteAssetDic.Add(spriteAsset.Id,spriteAsset);
            }

            if (!IndexSpriteInfo.ContainsKey(spriteAsset.Id))
            {
                Dictionary<string, SpriteInfoGroup> spriteGroup = new Dictionary<string, SpriteInfoGroup>();
                foreach (var item in spriteAsset.ListSpriteGroup)
                {
                    if (!spriteGroup.ContainsKey(item.Tag) && item.ListSpriteInfor != null && item.ListSpriteInfor.Count > 0)
                        spriteGroup.Add(item.Tag, item);
                }
                IndexSpriteInfo.Add(spriteAsset.Id, spriteGroup);
            }
        }
        
        public void UnReg(SpriteAsset spriteAsset)
        {
            if (SpriteAssetDic.ContainsKey(spriteAsset.Id))
            {
                SpriteAssetDic.Remove(spriteAsset.Id);
            }
            if (IndexSpriteInfo.ContainsKey(spriteAsset.Id))
            {
                IndexSpriteInfo.Remove(spriteAsset.Id);
            }
        }

        
        public void Clear()
        {
            SpriteAssetDic.Clear();
            IndexSpriteInfo.Clear();
        }


        public SpriteInfoGroup GetSpriteGroup(int id,string tag)
        {
            ReBuild();
            if (IndexSpriteInfo.TryGetValue(id, out var dic))
            {
                if (dic.TryGetValue(tag, out var inforGroup))
                {
                    return inforGroup;
                }
            }
            return null;
        }
        
        public SpriteAsset GetSpriteAsset(int id)
        {
            ReBuild();
            if (SpriteAssetDic.TryGetValue(id, out var dic))
            {
                return dic;
            }
            return null;
        }
        
        public Texture GetSpriteTexture(int id)
        {
            var asset = GetSpriteAsset(id);
            return asset==null?null:asset.TexSource;
        }
        
    }
}
