using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EasyText.Editor
{
    public class DrawSpriteAsset
    {
        private string _assetPath = "配置文件暂未保存";
        private Vector2 _spritesScrollView = Vector2.zero;
       
        private SpriteAsset _spriteAsset;
        Rect last;
        ReorderableList reorderableList;

        public int SelectIndex = -1;
        public int SelectFrameIndex = -1;
        
        public DrawSpriteAsset(SpriteAsset spriteAsset)
        {
            _spriteAsset = spriteAsset;
        }

        /// <summary>
        /// 设置信息
        /// </summary>
        /// <param name="spriteAsset"></param>
        public void SetSpriteAsset(SpriteAsset spriteAsset)
        {
            _spriteAsset = spriteAsset;
        }

        /// <summary>
        /// 绘制
        /// </summary>
        public void Draw()
        {
            if (_spriteAsset)
            {
                //属性
                GUILayout.Label("属性:");
                GUILayout.BeginVertical("HelpBox");
                //id
                GUILayout.BeginHorizontal();
                GUILayout.Label("Id", GUILayout.Width(80));
                _spriteAsset.Id = EditorGUILayout.IntField(_spriteAsset.Id);
                GUILayout.EndHorizontal();
                //是否为静态表情
                GUILayout.BeginHorizontal();
                bool isStatic = GUILayout.Toggle(_spriteAsset.IsStatic, "是否为静态表情?");
                if (isStatic != _spriteAsset.IsStatic)
                {
                    if (EditorUtility.DisplayDialog("提示", "切换表情类型，会导致重新命名Tag,请确认操作", "确认", "取消"))
                    {
                        _spriteAsset.IsStatic = isStatic;
                    }
                }
                GUILayout.FlexibleSpace();
                //动画的速度
                if (!_spriteAsset.IsStatic)
                {
                    GUILayout.Label("动画速度", GUILayout.Width(80));
                    _spriteAsset.Speed = EditorGUILayout.FloatField(_spriteAsset.Speed);
                }
                GUILayout.EndHorizontal();
                //行列速度
                GUILayout.BeginHorizontal();
                GUILayout.Label("Row", GUILayout.Width(80));
                _spriteAsset.Row = EditorGUILayout.FloatField(_spriteAsset.Row);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Column", GUILayout.Width(80));
                _spriteAsset.Column = EditorGUILayout.FloatField(_spriteAsset.Column);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Offset", GUILayout.Width(80));
                _spriteAsset.Offset = EditorGUILayout.Vector2Field("",_spriteAsset.Offset);
                
                GUILayout.EndHorizontal();

                if (GUILayout.Button("生成信息"))
                {
                    UpdateSpriteGroup();
                }
                GUILayout.EndVertical();

                //具体的精灵信息
                if (_spriteAsset && _spriteAsset.ListSpriteGroup.Count > 0)
                {
                    List<SpriteInfoGroup> inforGroups = _spriteAsset.ListSpriteGroup;
                    GUILayout.Label("精灵信息:");
                    _spritesScrollView = GUILayout.BeginScrollView(_spritesScrollView, "HelpBox");
                    for (int i = 0; i < inforGroups.Count; i++)
                    {
                        GUILayout.BeginVertical("HelpBox");
                        //标题信息..........
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(i.ToString(), SelectIndex == i ? "OL Minus" : "OL Plus", GUILayout.Width(40), GUILayout.Height(40)))
                        {
                            if (SelectIndex == i)
                                SelectIndex = -1;
                            else
                                SelectIndex = i;

                            //_showSprites.Clear();
                        }

                        if (SelectIndex == -1)
                        {
                            reorderableList = null;
                            SelectFrameIndex = -1;
                        }

                        //表情预览
                        GUILayout.Label("", GUILayout.Width(40), GUILayout.Height(40));
                        var frames = inforGroups[i].ListSpriteInfor.Count;
                        var speed = _spriteAsset.Speed;
                        if (inforGroups[i].ListSpriteInfor.Count > 0)
                        {
                            Rect lastRect = GUILayoutUtility.GetLastRect();
                            var index = Mathf.FloorToInt(Time.realtimeSinceStartup * speed)%frames;
                            //渲染精灵图片
                            GUI.DrawTextureWithTexCoords(lastRect, _spriteAsset.TexSource, inforGroups[i].ListSpriteInfor[index].DrawTexCoord);
                        }
                        GUILayout.Label("Tag:");
                        inforGroups[i].Tag = EditorGUILayout.TextField(inforGroups[i].Tag);
                        GUILayout.Label("Size:");
                        inforGroups[i].Size = EditorGUILayout.FloatField(inforGroups[i].Size);
                        GUILayout.Label("Width:");
                        inforGroups[i].Width = EditorGUILayout.FloatField(inforGroups[i].Width);
                        GUILayout.EndHorizontal();
                        //具体信息
                        if (SelectIndex == i)
                        {
                            var inforGroup = inforGroups[i];
                            List<SpriteInfo> spriteInfors = inforGroup.ListSpriteInfor;
                            
                            if(reorderableList==null)
                                reorderableList = new ReorderableList(spriteInfors,typeof(SpriteInfo));
                            
                            //设置单个元素的高度
                            reorderableList.elementHeight = 150;
                     
                            //绘制单个元素
                            reorderableList.drawElementCallback =
                                (rect, index, isActive, isFocused) => {
                                    var element = spriteInfors[index];
                                    if(last==null)
                                        last = new Rect(0,0,0,0);
                                   //渲染精灵图片
                                   last.x = rect.x;
                                   last.y = rect.y;
                                   last.width = 80;
                                   last.height = 80;
                                 
                                    GUI.DrawTextureWithTexCoords(last, _spriteAsset.TexSource,element.DrawTexCoord);
                                    var tempColor = Handles.color;
                                    Handles.color = isFocused ? Color.red : Color.green;
                                    Handles.DrawWireCube(last.center,last.size);
                                    Handles.color = tempColor;
                                    Handles.EndGUI();
                             
                                    //渲染其他信息
                                    GUI.enabled = true;
                                  
                                    //id
                                    last.x  = rect.x +100;
                                    last.height = 30;
                                    last.width = 30;
                                    GUI.Label(last, "Id:");
                                    last.x += 40;
                                    GUI.Label(last, element.Id.ToString());
                                
                                    //Rect
                                    last.x = rect.x +100;
                                    last.y =rect.y+ 20;
                                    last.height = 20;
                                    last.width = rect.width - 150;
                                    GUI.Label(last,"Rect:");
                                    last.x = rect.x +150;
                                    last.width = rect.width - 150;
                                    element.Rect = EditorGUI.RectField(last,element.Rect);
                                  
                                    //uvs
                                    for (int u = 0; u < element.Uv.Length; u++)
                                    {
                                        last.x = rect.x +100;
                                        last.y =rect.y+ 60+ 20*u;
                                        last.height = 20;
                                        last.width = rect.width - 150;
                                        GUI.Label(last,"UV" + u + ":");
                                        last.x = rect.x +150;
                                        element.Uv[u] = EditorGUI.Vector2Field(last,"", element.Uv[u]);
                                    }

                                    GUI.enabled = true;
                                };
                            reorderableList.onSelectCallback = (list) => { SelectFrameIndex =spriteInfors[list.index].Id; };
                            reorderableList.onRemoveCallback = (list) =>
                            {
                                spriteInfors.RemoveAt(list.index);
                                
                            };
                            
                            reorderableList.onAddCallback = (list) =>
                            {
                                var spriteInfo = new SpriteInfo();
                                if (SelectIndex >= 0)
                                {
                                    var s = spriteInfors[SelectIndex];
                                    spriteInfo.Id = spriteInfors.Count;
                                    spriteInfo.Rect = new Rect(s.Rect.x,s.Rect.y,s.Rect.width,s.Rect.height);
                                    spriteInfo.Uv = (Vector2[])s.Uv.Clone();
                                    spriteInfo.DrawTexCoord =  new Rect(s.DrawTexCoord.x,s.DrawTexCoord.y,s.DrawTexCoord.width,s.DrawTexCoord.height);
                                }
                                else
                                {
                                    spriteInfo.Id = spriteInfors.Count;
                                    spriteInfo.Rect = new Rect(0,0,0,0);
                                    spriteInfo.Uv = new Vector2[]
                                    {
                                        Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, 
                                    };
                                    spriteInfo.DrawTexCoord =new Rect(0,0,0,0);
                                }
                               
                                spriteInfors.Add(spriteInfo);
                                reorderableList = new ReorderableList(spriteInfors,typeof(SpriteInfo));
                               
                            };
                            reorderableList.onReorderCallbackWithDetails = (list, index, newIndex) =>
                            {
                                /*
                                if (newIndex < index)
                                {
                                    var old = spriteInfors[index];
                                    for (int j = index; j > newIndex; j--)
                                    {
                                        spriteInfors[j] = spriteInfors[j-1];
                                    }
                                    spriteInfors[newIndex] = old;
                                }
                                else
                                {
                                    var old = spriteInfors[index];
                                    for (int j = index; j < newIndex; j++)
                                    {
                                        spriteInfors[j] = spriteInfors[j+1];
                                    }
                                    spriteInfors[newIndex] = old;
                                }*/
                            };
                            
                           // reorderableList.onReorderCallback = list => { ReOrderSpriteId(); };
                            reorderableList.onChangedCallback = list => { ReOrderSpriteId(); };
                            
                            reorderableList.DoLayoutList();
                            /*
                            for (int m = 0; m < spriteInfors.Count; m++)
                            {
                                GUILayout.BeginHorizontal("HelpBox");
                             
                                //渲染精灵图片
                                GUILayout.Label("", GUILayout.Width(80), GUILayout.Height(80));
                                Rect last = GUILayoutUtility.GetLastRect();
                                GUI.DrawTextureWithTexCoords(last, _spriteAsset.TexSource, spriteInfors[m].DrawTexCoord);
                                
                                Handles.color =  Color.green;
                                Handles.DrawWireCube(last.center,last.size);
                                Handles.EndGUI();
                                //间隔
                                GUILayout.Space(50);
                                //渲染其他信息
                                GUI.enabled = true;
                                GUILayout.BeginVertical();
                                //id
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Id:", GUILayout.Width(50));
                                GUILayout.Label(spriteInfors[m].Id.ToString());
                                GUILayout.EndHorizontal();
                                //Rect
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Rect:", GUILayout.Width(50));
                                spriteInfors[m].Rect = EditorGUILayout.RectField(spriteInfors[m].Rect);
                                GUILayout.EndHorizontal();
                                //uvs
                                for (int u = 0; u < spriteInfors[m].Uv.Length; u++)
                                {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Label("UV" + u + ":", GUILayout.Width(50));
                                    spriteInfors[m].Uv[u] = EditorGUILayout.Vector2Field("", spriteInfors[m].Uv[u]);
                                    GUILayout.EndHorizontal();
                                }

                                GUILayout.EndVertical();
                                GUI.enabled = true;

                                GUILayout.EndHorizontal();
                            }
*/
                        }
                        GUILayout.EndVertical();

                    }
                    GUILayout.EndScrollView();
                }


            }
        }


        /// <summary>
        /// 更新信息
        /// </summary>
        public void UpdateSpriteGroup()
        {
            if (_spriteAsset && _spriteAsset.TexSource && _spriteAsset.Row > 1 && _spriteAsset.Column > 1)
            {
                var count = _spriteAsset.IsStatic ? _spriteAsset.Row * _spriteAsset.Column : _spriteAsset.Row;
               // if (_spriteAsset.ListSpriteGroup.Count != count)
                {
                    _spriteAsset.ListSpriteGroup.Clear();
                    //更新
                    //----------------------------------
                    Vector2 texSize = new Vector2(_spriteAsset.TexSource.width, _spriteAsset.TexSource.height);
                    Vector2 size = new Vector2((_spriteAsset.TexSource.width / (float)_spriteAsset.Column)
                        , (_spriteAsset.TexSource.height / (float)_spriteAsset.Row));

                    if (_spriteAsset.IsStatic)
                    {
                        int index = -1;
                        for (int i = 0; i < _spriteAsset.Row; i++)
                        {
                            for (int j = 0; j < _spriteAsset.Column; j++)
                            {
                                index++;
                                SpriteInfoGroup infoGroup = Pool<SpriteInfoGroup>.Get();
                                SpriteInfo info = GetSpriteInfo(index, _spriteAsset.Offset,i, j, size, texSize);

                                infoGroup.Tag = "emoji_" + info.Id;
                                infoGroup.ListSpriteInfor.Add(info);
                                _spriteAsset.ListSpriteGroup.Add(infoGroup);
                            }
                        }
                    }
                    else
                    {
                        int index = -1;
                        for (int i = 0; i < _spriteAsset.Row; i++)
                        {
                            SpriteInfoGroup infoGroup = Pool<SpriteInfoGroup>.Get();
                            infoGroup.Tag = "emoji_" + i;
                            for (int j = 0; j < _spriteAsset.Column; j++)
                            {
                                index++;

                                SpriteInfo info = GetSpriteInfo(index, _spriteAsset.Offset,i, j, size, texSize);

                                infoGroup.ListSpriteInfor.Add(info);
                            }
                            _spriteAsset.ListSpriteGroup.Add(infoGroup);
                        }
                    }

                }
            }
        }


        public void ReOrderSpriteId()
        {
            int id = 0;
            for (int i = 0; i < _spriteAsset.ListSpriteGroup.Count; i++)
            {
                var listSpriteInfor = _spriteAsset.ListSpriteGroup[i].ListSpriteInfor;
                for (int j = 0; j < listSpriteInfor.Count; j++)
                {
                    listSpriteInfor[j].Id = id;
                    id++;
                }
            }
            
        }


        #region 内部函数
        //获取精灵信息
        private SpriteInfo GetSpriteInfo(int index, Vector2 offset,int row, int column, Vector2 size, Vector2 texSize)
        {
            SpriteInfo info = Pool<SpriteInfo>.Get();
            info.Id = index;
            info.Rect = new Rect(offset.x+ size.y * column, offset.y+texSize.y - (row + 1) * size.x, size.x, size.y);
            info.DrawTexCoord = new Rect(info.Rect.x / texSize.x, info.Rect.y / texSize.y
                , info.Rect.width / texSize.x, info.Rect.height / texSize.y);
            info.Uv = GetSpriteUV(texSize, info.Rect);
            return info;
        }

        //获取uv信息
        private static Vector2[] GetSpriteUV(Vector2 texSize, Rect _sprRect)
        {
            Vector2[] uv = new Vector2[4];
            uv[0] = new Vector2(_sprRect.x / texSize.x, (_sprRect.y + _sprRect.height) / texSize.y);
            uv[1] = new Vector2((_sprRect.x + _sprRect.width) / texSize.x, (_sprRect.y + _sprRect.height) / texSize.y);
            uv[2] = new Vector2((_sprRect.x + _sprRect.width) / texSize.x, _sprRect.y / texSize.y);
            uv[3] = new Vector2(_sprRect.x / texSize.x, _sprRect.y / texSize.y);
            return uv;
        }

        #endregion
    }

}