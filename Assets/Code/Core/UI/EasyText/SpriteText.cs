using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace EasyText
{
    public class Utility
    {
        /// <summary>
        /// 获取Transform的世界坐标
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="canvas"></param>
        /// <returns></returns>
        public static Vector3 TransformPoint2World(Transform transform, Vector3 point)
        {
            return transform.localToWorldMatrix.MultiplyPoint(point);
        }

        /// <summary>
        /// 获取Transform的本地坐标
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 TransformWorld2Point(Transform transform, Vector3 point)
        {
            return transform.worldToLocalMatrix.MultiplyPoint(point);
        }

    }

    #region Struct
    /// <summary>
    /// 图片的信息
    /// </summary>
    public class SpriteTagInfo
    {
        /// <summary>
        /// 顶点索引id
        /// </summary>
        public int EmojiIndex;
        /// <summary>
        /// 顶点索引id
        /// </summary>
        public int Index;
#if UNITY_2019_1_OR_NEWER
            /// <summary>
            /// 为了兼容unity2019 单行的顶点的索引
            /// </summary>
            public int NewIndex;
#endif
        /// <summary>
        /// 图集id
        /// </summary>
        public int Id;
        /// <summary>
        /// 标签标签
        /// </summary>
        public string Tag;
        /// <summary>
        /// 标签大小
        /// </summary>
        public Vector2 Size;
        /// <summary>
        /// 表情位置
        /// </summary>
        public Vector3[] Pos = new Vector3[4];
        /// <summary>
        /// uv
        /// </summary>
        public Vector2[] UVs = new Vector2[4];
    }

    /// <summary>
    /// 超链接信息类
    /// </summary>
    public class HrefInfo
    {
        /// <summary>
        /// 超链接id
        /// </summary>
        public int Id;
        /// <summary>
        /// 顶点开始索引值
        /// </summary>
        public int StartIndex;
        /// <summary>
        /// 顶点结束索引值
        /// </summary>
        public int EndIndex;
#if UNITY_2019_1_OR_NEWER
            /// <summary>
            /// 顶点开始索引值
            /// </summary>
            public int NewStartIndex;
            /// <summary>
            /// 顶点结束索引值
            /// </summary>
            public int NewEndIndex;
#endif
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 超链接的值
        /// </summary>
        public string HrefValue;
        /// <summary>
        /// 碰撞盒范围
        /// </summary>
        public readonly List<Rect> Boxes = new List<Rect>();
    }
    #endregion

    [AddComponentMenu("UI/SpriteText", 10)]
    public class SpriteText : Text ,IPointerClickHandler
    {
        #region 属性
        // 用正则取  [图集ID#表情Tag] ID值==-1 ,表示为超链接
        private static readonly Regex _inputTagRegex = new Regex(@"\[(\-{0,1}\d{0,})#(.+?)\]", RegexOptions.Multiline);
        
        //表情位置索引信息
        // private List<SpriteTagInfo> _spriteInfo = new List<SpriteTagInfo>();
        //计算定点信息的缓存数组
        private readonly UIVertex[] m_TempVerts = new UIVertex[4];

        private StringBuilder _textBuilder = new StringBuilder();

        UIVertex _tempVertex = UIVertex.simpleVert;
        private List<int> _lastRenderIndexs = new List<int>();
        #region 超链接
        [System.Serializable]
        public class HrefClickEvent : UnityEvent<string, int> { }
        //点击事件监听
        public HrefClickEvent OnHrefClick = new HrefClickEvent();
        // 超链接信息列表  
        private readonly List<HrefInfo> _listHrefInfos = new List<HrefInfo>();

        public bool isVerticle = true;


        public Dictionary<int, List<SpriteTagInfo>> _spriteTagInfoDic;
        
        public Dictionary<int, List<SpriteTagInfo>> SpriteTagInfoDic
        {
            get
            {
                if(_spriteTagInfoDic==null)
                    _spriteTagInfoDic = new  Dictionary<int, List<SpriteTagInfo>>();
                return _spriteTagInfoDic;
            }
        }
        
        public Dictionary<int,SpriteEmoji> _subSpriteEmojis;
        
        public Dictionary<int,SpriteEmoji> SubSpriteEmojis
        {
            get
            {
                if(_subSpriteEmojis==null)
                    _subSpriteEmojis = new Dictionary<int,SpriteEmoji>();
                return _subSpriteEmojis;
            }
        }
         
        #endregion

        #endregion

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            text = _text;
        }

        [TextArea(3, 10)]
        [SerializeField]
        protected string _text = string.Empty;
        
        [SerializeField]
        public float charSpacing = 1;
        public override string text
        {
            get
            {
                return m_Text;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    if (String.IsNullOrEmpty(m_Text))
                        return;
                    m_Text = GetOutputText(value);
                }
                else if (_text != value)
                {
                    m_Text = GetOutputText(value);
                }
#if UNITY_EDITOR
                //编辑器赋值 如果是一样的 也可以刷新一下
                else
                {
                    m_Text = GetOutputText(value);
                  
                }
#endif
                //输入字符备份
                _text = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        private TagParser.TagParser tagParser;
        #region  内部函数
        //根据正则规则更新文本

        private int tagBegin = 0;
        private int tagEnd;
        private string GetOutputText(string inputText)
        {
            //回收各种对象
            ReleaseSpriteTageInfo();
            ReleaseHrefInfos();

            if (string.IsNullOrEmpty(inputText))
                return "";

            _textBuilder.Clear();
            int textIndex = 0;
            int newIndex = 0;
            string part = "";
            int tempIndex = 0;
            int vertSum = 0;
            
            if(tagParser==null)
                tagParser = new TagParser.TagParser();
            tagParser.Reset();
            tagParser.ParseText(inputText);
            
           
            tagBegin = 0;
            _textBuilder.Append(inputText);
            for (int i = 0; i < tagParser.Tags.Count; i++)
            {
                var tag = tagParser.Tags[i];
               // _textBuilder.AppendFormat(g.TextWithNoTags.Substring(tagBegin, tag.startIndex - tagBegin));
               // tagBegin += tag.endIndex - tag.startIndex;
                switch (tag.Name)
                {
                    case "sprite":
                    {
                        int spriteSub = 0;
                        string spriteTag = "";
                        for (int j = 0; j < tag.Properties.Count; j++)
                        {
                            var tempId = tag.Properties[j].key;
                            var tempTag = tag.Properties[j].value;
                           
                            if (tempId == "id")
                            {
                                spriteSub = int.Parse(tempTag);
                            }
                            if (tempId == "tag")
                            {
                                spriteTag = tempTag;
                            }
                        }
                        SpriteInfoGroup tempGroup = SpriteAssetManager.Instance.GetSpriteGroup(spriteSub, spriteTag);
                        if (tempGroup == null)
                            continue;
                        //清理标签
                        SpriteTagInfo tempSpriteTag = Pool<SpriteTagInfo>.Get();
                        tempSpriteTag.Index = (tag.endIndex-tag.lineIndex)*4;
                        
                        var s= fontSize/tempGroup.Size;
                       // _textBuilder.AppendFormat("<quad size={0} width={1}/>", tempGroup.Size * s, tempGroup.Width);

                        _textBuilder.Replace(string.Format("<{0}></sprite>",tag.content),
                            string.Format("<quad size={0} width={1}/>", tempGroup.Size * s, tempGroup.Width));
                        
                        tempSpriteTag.Id = spriteSub;
                        tempSpriteTag.Tag = spriteTag;
                        tempSpriteTag.Size = new Vector2(tempGroup.Size * tempGroup.Width, tempGroup.Size);
                        tempSpriteTag.UVs = tempGroup.ListSpriteInfor[0].Uv;
                        
                        //添加正则表达式的信息
                        SpriteTagInfoDic.TryGetValue(tempSpriteTag.Id, out var list);
                        if (list == null)
                        {
                            if (SpriteTagInfoDic.ContainsKey(tempSpriteTag.Id))
                            {
                                SpriteTagInfoDic.Remove(tempSpriteTag.Id);
                            }
                            list = new List<SpriteTagInfo>();
                            SpriteTagInfoDic.Add(tempSpriteTag.Id,list);
                        }
                        list.Add(tempSpriteTag);
                        SpriteEmoji spriteEmoji = GetSpriteEmoji(spriteSub);
                        spriteEmoji.m_spriteAsset =SpriteAssetManager.Instance.GetSpriteAsset(spriteSub);
                        spriteEmoji.material.mainTexture = SpriteAssetManager.Instance.GetSpriteTexture(spriteSub);
                        spriteEmoji.ClearEmojiVert();
                        spriteEmoji.SetAllDirty();
                        break;
                    }
                    case "herf":
                    {
                        int herfid = 0;
                       
                        for (int j = 0; j < tag.Properties.Count; j++)
                        {
                            var tempId = tag.Properties[j].key;
                            var tempTag = tag.Properties[j].value;
                            if (tempId == "id")
                            {
                                herfid = int.Parse(tempTag);
                            }
                        }
                        _textBuilder.Replace(string.Format("<{0}>{1}</herf>",tag.content,tag.Value),
                            string.Format("<color=blue>{0}</color>",tag.Value));
                        
                        var hrefInfo = Pool<HrefInfo>.Get();
                        hrefInfo.Id = Mathf.Abs(herfid);
                        hrefInfo.StartIndex =  (tag.startIndex-tag.lineIndex)*4;;// 超链接里的文本起始顶点索引
                        hrefInfo.EndIndex =  (tag.endIndex-tag.lineIndex)*4;;
#if UNITY_2019_1_OR_NEWER
                    hrefInfo.NewStartIndex = newStartIndex;
                    hrefInfo.NewEndIndex = newIndex - 1;
#endif
                        hrefInfo.Name = tag.Name;
                        hrefInfo.HrefValue = tag.Value;
                        _listHrefInfos.Add(hrefInfo);
                       
                       
                        break;
                    }
                }
            }
/*
            _textBuilder.Clear();
            
            foreach (Match match in _inputTagRegex.Matches(inputText))
            {
                int tempId = 0;
                if (!string.IsNullOrEmpty(match.Groups[1].Value) && !match.Groups[1].Value.Equals("-"))
                    tempId = int.Parse(match.Groups[1].Value);
                string tempTag = match.Groups[2].Value;
                var tagLength = tempTag.Length;
                //更新超链接
                if (tempId < 0)
                {
                    part = inputText.Substring(textIndex, match.Index - textIndex);
                    _textBuilder.Append(part);
                    
                    var lastLength = ReplaceRichText(_textBuilder.ToString()).Length;
                    int startIndex = (lastLength- vertSum)*4;;
                    _textBuilder.AppendFormat("<color=blue>{0}</color>", match.Groups[2].Value);
                  
                    vertSum += ReplaceRichText(_textBuilder.ToString()).Length - lastLength-tagLength;
                    int endIndex = startIndex + tagLength*4 ;
                    
#if UNITY_2019_1_OR_NEWER
                    newIndex += ReplaceRichText(part).Length * 4;
                    int newStartIndex = newIndex;
                    newIndex += match.Groups[2].Value.Length * 4 + 8;
#endif
                    
                    var hrefInfo = Pool<HrefInfo>.Get();
                    hrefInfo.Id = Mathf.Abs(tempId);
                    hrefInfo.StartIndex = startIndex;// 超链接里的文本起始顶点索引
                    hrefInfo.EndIndex = endIndex;
#if UNITY_2019_1_OR_NEWER
                    hrefInfo.NewStartIndex = newStartIndex;
                    hrefInfo.NewEndIndex = newIndex - 1;
#endif
                    hrefInfo.Name = match.Groups[2].Value;
                    hrefInfo.HrefValue = match.Groups[3].Value;
                    _listHrefInfos.Add(hrefInfo);
                }
                //更新表情
                else
                {
                    SpriteInfoGroup tempGroup = SpriteAssetManager.Instance.GetSpriteGroup(tempId, tempTag);
                    if (tempGroup == null)
                        continue;
                    part = inputText.Substring(textIndex, match.Index - textIndex);
                    _textBuilder.Append(part);
                    
               
                    var lastLength = ReplaceRichText(_textBuilder.ToString()).Length;
                  
#if UNITY_2019_1_OR_NEWER
                    newIndex += ReplaceRichText(part).Length * 4;
#endif
                    tempIndex = (lastLength- vertSum)*4;
                    var s= fontSize/tempGroup.Size;
                    _textBuilder.AppendFormat("<quad size={0} width={1}/>", tempGroup.Size * s, tempGroup.Width);
                    vertSum += ReplaceRichText(_textBuilder.ToString()).Length - lastLength-1;
                 
                    //清理标签
                    SpriteTagInfo tempSpriteTag = Pool<SpriteTagInfo>.Get();
                    tempSpriteTag.Index = tempIndex;
                    
#if UNITY_2019_1_OR_NEWER
                    tempSpriteTag.NewIndex = newIndex;
#endif
                    
                    tempSpriteTag.Id = tempId;
                    tempSpriteTag.Tag = tempTag;
                    tempSpriteTag.Size = new Vector2(tempGroup.Size * tempGroup.Width, tempGroup.Size);
                    tempSpriteTag.UVs = tempGroup.ListSpriteInfor[0].Uv;
                
                    //添加正则表达式的信息
                    SpriteTagInfoDic.TryGetValue(tempSpriteTag.Id, out var list);
                    if (list == null)
                    {
                        if (SpriteTagInfoDic.ContainsKey(tempSpriteTag.Id))
                        {
                            SpriteTagInfoDic.Remove(tempSpriteTag.Id);
                        }

                        list = new List<SpriteTagInfo>();
                        SpriteTagInfoDic.Add(tempSpriteTag.Id,list);
                    }
                    list.Add(tempSpriteTag);
                    SpriteEmoji spriteEmoji = GetSpriteEmoji(tempId);
                    spriteEmoji.m_spriteAsset =SpriteAssetManager.Instance.GetSpriteAsset(tempId);
                    spriteEmoji.material.mainTexture = SpriteAssetManager.Instance.GetSpriteTexture(tempId);
                    spriteEmoji.ClearEmojiVert();
                    spriteEmoji.SetAllDirty();
#if UNITY_2019_1_OR_NEWER
                    newIndex += 4;
#endif
                }

                
                textIndex = match.Index + match.Length;
            }

            _textBuilder.Append(inputText.Substring(textIndex, inputText.Length - textIndex));
            */
            return _textBuilder.ToString();
        }
        //处理表情信息
        private void DealSpriteTagInfo(VertexHelper toFill)
        {
            int index = -1;
#if UNITY_2019_1_OR_NEWER
            bool autoLF = AutoLF();
#endif
            //emoji
            foreach (var vp in SpriteTagInfoDic)
            {
                var _spriteInfo = vp.Value;
                var emojiId = vp.Key;
                SpriteEmoji spriteEmoji = GetSpriteEmoji(emojiId);
                spriteEmoji.m_spriteAsset =SpriteAssetManager.Instance.GetSpriteAsset(emojiId);
                spriteEmoji.material.mainTexture = SpriteAssetManager.Instance.GetSpriteTexture(emojiId);
                spriteEmoji.ClearEmojiVert();
                for (int i = 0; i < _spriteInfo.Count; i++)
                {
                    var info = _spriteInfo[i];
               
#if UNITY_2019_1_OR_NEWER
                index = autoLF ? _spriteInfo[i].Index : _spriteInfo[i].NewIndex;
#else
                    index = info.Index;
#endif                
                    float num1 = 1f / this.pixelsPerUnit;
                
                    if ((index + 4) <= toFill.currentVertCount)
                    {
                        // toFill.PopulateUIVertex(ref _tempVertex1, index);
                        for (int j = index; j < index + 4; j++)
                        {
                            toFill.PopulateUIVertex(ref _tempVertex, j);
                            //清理多余的乱码uv
                            _tempVertex.uv0 = Vector2.zero;
                            _tempVertex.uv1 = _spriteInfo[i].UVs[j-index];
                            _tempVertex.uv2 = new Vector2(info.Id,0);
                            //获取quad的位置 --> 转为世界坐标
                            info.Pos[j - index] = Utility.TransformPoint2World(transform, _tempVertex.position);
                            toFill.SetUIVertex(_tempVertex, j);
                            _tempVertex.position = info.Pos[j - index];
                            spriteEmoji.AddEmojiVert(_tempVertex);
                        }
                    }
                }
            }
            
          
        }
        //处理超链接的信息
        private void DealHrefInfo(VertexHelper toFill)
        {
            if (_listHrefInfos.Count > 0)
            {
#if UNITY_2019_1_OR_NEWER
                bool autoLF = AutoLF();
#endif
                // 处理超链接包围框  
                for (int i = 0; i < _listHrefInfos.Count; i++)
                {
                    _listHrefInfos[i].Boxes.Clear();
#if UNITY_2019_1_OR_NEWER
                    int startIndex = autoLF ? _listHrefInfos[i].StartIndex : _listHrefInfos[i].NewStartIndex;
                    int endIndex = autoLF ? _listHrefInfos[i].EndIndex : _listHrefInfos[i].NewEndIndex;
#else
                    int startIndex = _listHrefInfos[i].StartIndex+1;
                    int endIndex = _listHrefInfos[i].EndIndex+1;
#endif
                    if (startIndex >= toFill.currentVertCount)
                        continue;

                    toFill.PopulateUIVertex(ref _tempVertex, startIndex);
                    // 将超链接里面的文本顶点索引坐标加入到包围框  
                    var pos = _tempVertex.position;
                    var bounds = new Bounds(pos, Vector3.zero);
                    for (int j = startIndex; j <= endIndex; j++)
                    {
                        if (j >= toFill.currentVertCount)
                        {
                            break;
                        }
                        toFill.PopulateUIVertex(ref _tempVertex, j);
                        pos = _tempVertex.position;
                        if (pos.x < bounds.min.x)
                        {
                            // 换行重新添加包围框  
                            _listHrefInfos[i].Boxes.Add(new Rect(bounds.min, bounds.size));
                            bounds = new Bounds(pos, Vector3.zero);
                        }
                        else
                        {
                            bounds.Encapsulate(pos); // 扩展包围框  
                        }
                    }
                    //添加包围盒
                    _listHrefInfos[i].Boxes.Add(new Rect(bounds.min, bounds.size));
                }

                //添加下划线
                Vector2 extents = rectTransform.rect.size;
                var settings = GetGenerationSettings(extents);
                TextGenerator underlineText = Pool<TextGenerator>.Get();
                underlineText.Populate("_", settings);
                IList<UIVertex> tut = underlineText.verts;
                for (int m = 0; m < _listHrefInfos.Count; m++)
                {
                    for (int i = 0; i < _listHrefInfos[m].Boxes.Count; i++)
                    {
                        //计算下划线的位置
                        Vector3[] ulPos = new Vector3[4];
                        if (isVerticle)
                        {
                            ulPos[0] = _listHrefInfos[m].Boxes[i].position + new Vector2(- fontSize * 0.2f,0);
                            ulPos[1] = ulPos[0] + new Vector3(0,_listHrefInfos[m].Boxes[i].height);
                            ulPos[2] = _listHrefInfos[m].Boxes[i].position + new Vector2(0,_listHrefInfos[m].Boxes[i].height);
                            ulPos[3] = _listHrefInfos[m].Boxes[i].position;
                        }
                        else
                        {
                            ulPos[0] = _listHrefInfos[m].Boxes[i].position + new Vector2(0.0f, -fontSize * 0.2f);
                            ulPos[1] = ulPos[0] + new Vector3(_listHrefInfos[m].Boxes[i].width, 0.0f);
                            ulPos[2] = _listHrefInfos[m].Boxes[i].position + new Vector2(_listHrefInfos[m].Boxes[i].width, 0.0f);
                            ulPos[3] = _listHrefInfos[m].Boxes[i].position;
                        }

                     
                        //绘制下划线
                        for (int j = 0; j < 4; j++)
                        {
                            m_TempVerts[j] = tut[j];
                            m_TempVerts[j].color = Color.blue;
                            m_TempVerts[j].position = ulPos[j];
                     
                            m_TempVerts[j].uv1 = -Vector2.one;
                            m_TempVerts[j].uv2 = Vector2.zero;
                            if (j == 3)
                                toFill.AddUIVertexQuad(m_TempVerts);
                        }
                    }
                }
                //回收下划线的对象
                Pool<TextGenerator>.Release(underlineText);
            }
        }

        
        //回收SpriteTagInfo
        private void ReleaseSpriteTageInfo()
        {
            foreach (var vp in SpriteTagInfoDic)
            {
                var _spriteInfo = vp.Value;
                //记录之前的信息
                for (int i = 0; i < _spriteInfo.Count; i++)
                {
                    //回收信息到对象池
                    Pool<SpriteTagInfo>.Release(_spriteInfo[i]);
                }
                _spriteInfo.Clear();
            }

         
        }
        //回收超链接的信息
        private void ReleaseHrefInfos()
        {
            for (int i = 0; i < _listHrefInfos.Count; i++)
            {
                Pool<HrefInfo>.Release(_listHrefInfos[i]);
            }
            _listHrefInfos.Clear();
        }
        //是否换行
        private bool AutoLF()
        {
            //width
            var settings = GetGenerationSettings(Vector2.zero);
            float width = cachedTextGeneratorForLayout.GetPreferredWidth(m_Text, settings) / pixelsPerUnit;
            bool widthResult = width < rectTransform.sizeDelta.x || horizontalOverflow == HorizontalWrapMode.Overflow;
            //height
            settings = GetGenerationSettings(new Vector2(rectTransform.rect.size.x, 0.0f));
            float height = cachedTextGeneratorForLayout.GetPreferredHeight(m_Text, settings) / pixelsPerUnit;
            bool heightResult = height < rectTransform.sizeDelta.y || verticalOverflow == VerticalWrapMode.Overflow;
            return !widthResult || !heightResult;
        }

        //换掉富文本
        private string ReplaceRichText(string str)
        {
            str = Regex.Replace(str, @"<color=(.+?)>", "");
            str = str.Replace("</color>", "");
            str = str.Replace("<b>", "");
            str = str.Replace("</b>", "");
            str = str.Replace("<i>", "");
            str = str.Replace("</i>", "");
            str = str.Replace("\n", "");
            str = str.Replace("\t", "");
            str = str.Replace("\r", "");
            str = str.Replace(" ", "");

            return str;
        }
        #endregion

        // <summary>半角转成全角   
        /// 半角空格32,全角空格12288   
        /// 其他字符半角33~126,其他字符全角65281~65374,相差65248   
        /// </summary>   
        /// <param name="input"></param>   
        /// <returns></returns>   
        public static string DBCToSBC(string input)
        {
            char[] cc = input.ToCharArray();
            for (int i = 0; i < cc.Length; i++)
            {
                if (cc[i] == 32)
                {
                    // 表示空格   
                    cc[i] = (char)12288;
                    continue;
                }
                if (cc[i] < 127 && cc[i] > 32)
                {
                    cc[i] = (char)(cc[i] + 65248);
                }
            }
            return new string(cc);
        }

        /// <summary>全角转半角   
        /// 半角空格32,全角空格12288   
        /// 其他字符半角33~126,其他字符全角65281~65374,相差65248   
        /// </summary>   
        /// <param name="input"></param>   
        /// <returns></returns>   
        public static string SBCToDBC(string input)
        {
            char[] cc = input.ToCharArray();
            for (int i = 0; i < cc.Length; i++)
            {
                if (cc[i] == 12288)
                {
                    // 表示空格   
                    cc[i] = (char)32;
                    continue;
                }
                if (cc[i] > 65280 && cc[i] < 65375)
                {
                    cc[i] = (char)(cc[i] - 65248);
                }

            }
            return new string(cc);
        }

        private bool updateSubs = false;
        Vector3 tempPos = Vector3.zero;
        
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if ((UnityEngine.Object) this.font == (UnityEngine.Object) null)
                return;
           
            var rectSize = this.rectTransform.rect.size;
            var width = rectSize.x;
            var height = rectSize.y;

            Vector2 leftTop = this.rectTransform.rect.center + rectSize/2* new Vector2(-1,1);
            Vector2 rightTop = this.rectTransform.rect.center + rectSize/2 * new Vector2(1,1);
            Vector2 rightBottom = this.rectTransform.rect.center + rectSize/2 * new Vector2(1,-1);
            
            if (isVerticle)
            {
                var x = rectSize.x;
                rectSize.x = rectSize.y;
                rectSize.y = x;
            }
            this.m_DisableFontTextureRebuiltCallback = true;
            if (!cachedTextGenerator.PopulateWithErrors(this.text, this.GetGenerationSettings(rectSize),this.gameObject))
                return;
            IList<UIVertex> verts = cachedTextGenerator.verts;
            float num = 1f / this.pixelsPerUnit;
            int count = verts.Count;
         
            if (count <= 0)
            {
                toFill.Clear();
            }  
            else
            {
                Vector2 point = new Vector2(verts[0].position.x, verts[0].position.y) * num;
                Vector2 vector2 = this.PixelAdjustPoint(point) - point;
                toFill.Clear();
                int charColume = 0;
                int charRow = 0;
                var vertCount = count / 4;
                Matrix4x4 matrix = Matrix4x4.identity;
                
                for (int vertIndex = 0; vertIndex < vertCount; vertIndex++)
                {
                    int index = vertIndex * 4;
                    var p0 = verts[index];
                    var p1 = verts[index+1];
                    var p2 = verts[index+2];
                    var p3 = verts[index+3];
                    var w = Mathf.Abs((p2.position - p0.position).x*0.5f) ;
                    var h = Mathf.Abs((p2.position - p0.position).y) ;
                    Vector3 center = Vector3.Lerp(p0.position, p2.position, 0.5f);
                 
                    if (isVerticle)
                    {
                        var lineSpace = fontSize * (lineSpacing+1) * num;
                        var textSpace = fontSize * charSpacing * num;
                        var xOffset = rectSize.x / 2 - fontSize / 2f;
                        var yOffset = rectSize.y / 2 - fontSize / 2f;
                        
                        var delta=(rectSize.y - rectSize.x)*0.5f; 
                        var newx = rightTop.x + (center.y - leftTop.y) - delta;
                        var newy = rightTop.y - (center.x - leftTop.x) + delta; 
                        tempPos.x = -charColume * lineSpace + xOffset  + delta;
                        tempPos.y = -(charRow) * textSpace + yOffset - delta;
                        Matrix4x4 move = Matrix4x4.TRS(-center, Quaternion.identity, Vector3.one);
                        Matrix4x4 place = Matrix4x4.TRS(tempPos, Quaternion.identity, Vector3.one);
                        matrix = place * move;
                        var  preP3 = matrix.MultiplyPoint(p3.position);
                        if (preP3.y < rightBottom.y)
                        {
                            charColume++;
                            charRow=0;
                            tempPos.x = -charColume * lineSpace + xOffset  + delta;
                            tempPos.y = -(charRow) * textSpace + yOffset - delta;
                            place = Matrix4x4.TRS(tempPos, Quaternion.identity, Vector3.one);
                            matrix = place * move;
                        }
                        charRow++;
                    }

                    for (int k = 0; k < 4; ++k)
                    {
                        this.m_TempVerts[k] = verts[index+k];
                        this.m_TempVerts[k].position *= num;
                        this.m_TempVerts[k].position.x += vector2.x;
                        this.m_TempVerts[k].position.y += vector2.y;
                        this.m_TempVerts[k].uv1 = -Vector2.one;
                        this.m_TempVerts[k].uv2 = Vector2.zero;
                        
                        if (isVerticle)
                        {
                            this.m_TempVerts[k].position = matrix.MultiplyPoint(this.m_TempVerts[k].position);
                        }
                    }
                    toFill.AddUIVertexQuad(this.m_TempVerts);
                }
            }

            //更新顶点位置&去掉乱码uv
            DealSpriteTagInfo(toFill);
            //处理超链接的信息
            DealHrefInfo(toFill);
            updateSubs = true;
            //更新表情绘制
            // UpdateDrawSprite(true);
            m_DisableFontTextureRebuiltCallback = false;
        }

        private void LateUpdate()
        {
            if (updateSubs)
            {
                 foreach (var vp in SpriteTagInfoDic)
                {
                    var index = vp.Key;
                    SpriteEmoji spriteEmoji = GetSpriteEmoji(index);
                    spriteEmoji.SetAllDirty();
                    updateSubs = false;
                }
            }
        }

        //表情绘制
        private void UpdateDrawSprite(bool visable)
        {
            foreach (var vp in SpriteTagInfoDic)
            {
                var index = vp.Key;
                SpriteEmoji spriteEmoji = GetSpriteEmoji(index);
                spriteEmoji.m_spriteAsset =SpriteAssetManager.Instance.GetSpriteAsset(index);
                spriteEmoji.material.mainTexture = SpriteAssetManager.Instance.GetSpriteTexture(index);
                var _spriteInfo = vp.Value;
                spriteEmoji.ClearEmojiVert();
                for (int i = 0; i < _spriteInfo.Count; i++)
                {
                    var info = _spriteInfo[i];
                    // toFill.PopulateUIVertex(ref _tempVertex1, index);
                    for (int j = 0; j < 4; j++)
                    {
                        UIVertex vertex = Pool<UIVertex>.Get();
                        //清理多余的乱码uv
                        vertex.uv0 = Vector2.zero;
                        vertex.uv1 = info.UVs[j];
                        vertex.uv2 = new Vector2(info.Id,0);
                        vertex.position =info.Pos[j];
                        //  spriteEmoji.AddEmojiVert(vertex);
                    }
                }
            }
        }

        
        public SpriteEmoji GetSpriteEmoji(int index)
        {
            SubSpriteEmojis.TryGetValue(index, out var emoji);
            if(emoji==null)
            {
                var emojit = transform.Find("Sub Emoji" + index);
                if(emojit==null)
                    emojit = new GameObject("Sub Emoji"+index).transform;
                emojit.SetParent(this.transform);
                emojit.localPosition = Vector3.zero;
                emojit.localRotation = Quaternion.identity;
                emojit.localScale = Vector3.one;
                emoji = emojit.GetComponent<SpriteEmoji>();
                if(emoji==null)
                    emoji = emojit.gameObject.AddComponent<SpriteEmoji>();
                //  SubSpriteEmojis.Add(index,emoji);
            }
            return emoji;
        }
        
        #region 事件回调
        //响应点击事件-->检测是否在超链接的范围内
        public void OnPointerClick(PointerEventData eventData)
        {
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, eventData.position, eventData.pressEventCamera, out lp);

            foreach (var hrefInfo in _listHrefInfos)
            {
                var boxes = hrefInfo.Boxes;
                for (var i = 0; i < boxes.Count; ++i)
                {
                    if (boxes[i].Contains(lp))
                    {
                        OnHrefClick.Invoke(hrefInfo.HrefValue, hrefInfo.Id);
                        Debug.Log("click "+  hrefInfo.Id + "  "+hrefInfo.HrefValue);
                        return;
                    }
                }
            }
        }
        #endregion
    }
}