/*
 * @Descripttion: https://github.com/jesenzhang/UnityMisc.git
 * @version: 
 * @Author: jesen.zhang
 * @Date: 2020-07-14 14:16:03
 * @LastEditors: jesen.zhang
 * @LastEditTime: 2020-07-14 15:09:59
 * 浑天球组件 支持椭球体 三半长轴相等时退化为球体
 */ 


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = System.Random;

#if  UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(CelestialSphere))]
public class CelestialSphereEditor : Editor
{
    private int page = 0;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CelestialSphere celestial = target as  CelestialSphere;
        
        if(GUILayout.Button("拾取子节点"))
        {
            CollectChildren(celestial);
        }
        
        if(GUILayout.Button("随机摆放节点"))
        {
            celestial.ResetEllipsoid();
            RandomChildren(celestial);
        }
        
        if(GUILayout.Button("均匀摆放节点"))
        {
            celestial.ResetEllipsoid();
            celestial.PlaceChildrenAverage();
        }
        
        if(GUILayout.Button("重建椭球体"))
        {
            celestial.ResetEllipsoid();
        }
        
        serializedObject.ApplyModifiedProperties(); 
    }
    
    private void RandomChildren(CelestialSphere celestial)
    {
        Random r = new Random((int)DateTime.Now.Ticks);
        for (int i = 0; i < celestial.children.Count; i++)
        {
            Vector3 d = Vector3.zero;
            var sign = r.Next(0, 2) > 0 ? 1 : -1;
            d.x = sign * (float) r.NextDouble();
            sign = r.Next(0, 2) > 0 ? 1 : -1;
            d.y = sign * (float) r.NextDouble();
            sign = r.Next(0, 2) > 0 ? 1 : -1;
            d.z = sign * (float) r.NextDouble();
        
            var p = celestial.GetPointOnEllipsoid(d);
            celestial.children[i].transform.position =  p;
        }
    }
    
    
    private void CollectChildren(CelestialSphere celestial)
    {
        if(celestial.children==null)
            celestial.children =new List<Transform>();
        celestial.children.Clear();
        var count =  celestial.transform.childCount;
        for (var i = 0; i < count; i++)
        {
            var c = celestial.transform.GetChild(i);
            celestial.children.Add(c);
        }
    }
}
#endif

[ExecuteInEditMode]
public class CelestialSphere : MonoBehaviour,IBeginDragHandler,IDragHandler
{
    //子节点朝向
    public enum FaceMode
    {
        None =0,
        Forward,
        Normal,
    }
    public List<Transform> children;
    
    [Tooltip("椭圆x轴半长轴")]
    public float ellipsoidAxisA = 400;
    [Tooltip("椭圆y轴半长轴")]
    public float ellipsoidAxisB = 400;
    [Tooltip("椭圆z轴半长轴")]
    public float ellipsoidAxisC = 400;
    [Tooltip("椭圆的旋转")]
    public Vector3 ellipsoidRotation = Vector3.zero;
    [Tooltip("最近item的缩放大小")]
    public float maxScale = 1f;
    [Tooltip("最远item的缩放大小")]
    public float minScale = 0.5f;
    [Tooltip("正方向")]
    public Vector3 forward = Vector3.back;
    [Tooltip("是否控制水平方向")]
    public bool horizontal = true;
    [Tooltip("是否控制垂直方向")]
    public bool vertical = true;
    //旋转速度
    [Tooltip("旋转速度")]
    public float rotationSpeed = 0.001f;
    [Tooltip("自动旋转")]
    public bool autoRotation = false;
    [Tooltip("椭圆的自动旋转速度")]
    public Vector3 autoRotationSpeed;
    //阻尼时间
    [Tooltip("阻尼时间")]
    public float rotationDampingTime = 0.2f;
    [Tooltip("选中对象的缩放")]
    public float clickItemSize = 1.8f;
    [Tooltip("选中时锁定拖拽")]
    public bool lockClick = false;
 
    [Tooltip("点击跳转旋转速度")]
    public float turnRotationSpeed = 0.01f;
    [Tooltip("点击跳转缩放周期")]
    public float clickScaleDuration = 0.2f;

    [Tooltip("是否按照距离更新子节点排序SiblingIndex")]
    public bool updateSiblingIndex = true;
    
    [Tooltip("子节点朝向")]
    public FaceMode fceMode = FaceMode.None;
   
    
    private Vector3 _deltaRotation;
    private Vector3 _followAngles;
    private Vector3 _rotationVelocity;
    private Quaternion _originalRotation;
    private Quaternion _originalLocalRotation;
    private bool _rotationDamping;
    private Vector2 _beginPosition;
    private Transform _clickObject = null;
    private float _localTime;
    private Ellipsoid _ellipsoid;
    private Quaternion _deltaQuaternion;
    private bool _clickTurn = false;
    private bool _turnback = false;

    private float _forwardDistance;
    private Vector3 _nearPoint;
    private Vector3 _farPoint;
    
    private Ellipsoid Ellipsoid
    {
        get
        {
            if (_ellipsoid == null)
            {
                _ellipsoid = new Ellipsoid(Vector3.zero, ellipsoidAxisA * Vector3.right, ellipsoidAxisB * Vector3.up,ellipsoidAxisC * Vector3.forward,ellipsoidRotation);
                PreParams();
            }
            return _ellipsoid;
        }
    }

    public void ResetEllipsoid()
    {
        if(_ellipsoid==null)
            _ellipsoid = new Ellipsoid(Vector3.zero, ellipsoidAxisA * Vector3.right, ellipsoidAxisB * Vector3.up,ellipsoidAxisC * Vector3.forward,ellipsoidRotation);
        else
        {
            _ellipsoid.Set(Vector3.zero, ellipsoidAxisA * Vector3.right, ellipsoidAxisB * Vector3.up,ellipsoidAxisC * Vector3.forward,ellipsoidRotation);
        }
        PreParams();
    }

    void Awake()
    {
        ResetEllipsoid();
        SortChildren();
    }

    /// <summary>
    /// 预计算全局参数
    /// </summary>
    public void PreParams()
    {
        _nearPoint = GetPointOnEllipsoid(forward);
        _farPoint = GetPointOnEllipsoid(-1*forward);
        _forwardDistance = (_nearPoint - _farPoint).magnitude;
    }

    /// <summary>
    /// 得到椭球体上一点
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public Vector3 GetPointOnEllipsoid(Vector3 dir)
    {
        return transform.position + Ellipsoid.CaculateLineIntersection(Vector3.zero, dir.normalized);
    }
    
    /// <summary>
    /// 计算节点的深度
    /// </summary>
    /// <param name="child"></param>
    /// <returns></returns>
    private float ChildDepth(Transform child)
    {
        return Vector3.Dot(forward,(child.position - transform.position));
    }

    /// <summary>
    /// 排序节点的前后顺序
    /// </summary>
    private void SortChildren()
    {
        if (children != null && children.Count > 0)
        {
            children.Sort((a,b) =>
            {
                var d1 = ChildDepth(a);
                var d2 = ChildDepth(b);
                return d1 > d2 ? 1 : d1 < d2 ? -1 : 0;
            });

            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var distance = Mathf.Abs(Vector3.Dot(forward,(child.position - _nearPoint)));
                var max = ReferenceEquals(child,_clickObject) ? clickItemSize : maxScale;
                children[i].transform.localScale = Vector3.Lerp(Vector3.one*max, Vector3.one *minScale,distance/_forwardDistance);

                if (fceMode == FaceMode.Normal)
                {
                    children[i].transform.forward = (child.position - transform.position).normalized;
                }else if (fceMode == FaceMode.Forward)
                {
                    children[i].transform.forward = forward;
                }
                
                if(updateSiblingIndex)
                    children[i].SetSiblingIndex(i);
            }
        }
    }
    
    
    /// <summary>
    /// 在表面均匀摆放子节点
    /// </summary>
    /// <param name="celestial"></param>
    public void PlaceChildrenAverage()
    {
        var max = children.Count;
        var p1 = Mathf.PI * (3 - Mathf.Sqrt(5));
        var p2 = 2f / max;
        for (int i = 0; i < max; i++)
        {
            var y = i * p2 - 1 + (p2 / 2);
            var r = Mathf.Sqrt(1 - y * y);
            var phi = i * p1;
            var x = Mathf.Cos(phi) * r;
            var z = Mathf.Sin(phi) * r; 
            Vector3 d = Vector3.zero;
            d.x = x;
            d.y =y;
            d.z = z;
            var p = GetPointOnEllipsoid(d.normalized);
            children[i].transform.position =  p;
        }
    }
    
    /// <summary>
    /// 选中节点置前
    /// </summary>
    /// <param name="index"></param>
    public void TurnToFront(int index)
    {
        Transform child = children[index];
        TurnToFront(child);
    }
    
    public void TurnToFront(Transform child)
    {
        if (lockClick && _clickTurn)
        {
            _clickTurn = false;
            _localTime = 0;
            _turnback = true;
        }
        else
        {
            var vec1 = (child.transform.position - transform.position).normalized;
            _deltaQuaternion = Quaternion.FromToRotation(vec1, forward);
            _rotationDamping = false;
            _clickTurn = true;
            _clickObject = child;
            _localTime = 0;
            _turnback = false;
        }
    }


    private void LateUpdate()
    {
        if (_clickTurn)
        {
            if (_deltaQuaternion == Quaternion.identity)
            {
                return;
            }
            var delta = Quaternion.Slerp(Quaternion.identity,_deltaQuaternion, turnRotationSpeed);
            _deltaQuaternion = Quaternion.Inverse(delta) * _deltaQuaternion;
            UpdateChildren(delta);
            _deltaRotation = Vector3.zero;
            SortChildren();
        }
        else
        {
            if (_turnback && _clickObject)
            {
                _localTime += Time.deltaTime;
                var f = Mathf.Clamp(_localTime/clickScaleDuration,0, 1);
                _clickObject.localScale = Vector3.Lerp(Vector3.one*clickItemSize, Vector3.one *maxScale, f);
                if (f >= 1)
                {
                    _turnback = false;
                    _clickObject = null;
                }

            }
            else
            {
                //旋转
                if (_rotationDamping)
                {
                    _deltaRotation = Vector3.SmoothDamp(_deltaRotation, Vector3.zero, ref _rotationVelocity, rotationDampingTime);
                    Quaternion delta = Quaternion.Euler(_deltaRotation.x, _deltaRotation.y, _deltaRotation.z);
                    UpdateChildren(delta);
                    if (_deltaRotation == Vector3.zero)
                        _rotationDamping = false;
                    SortChildren();
                }
                else
                {
                    //自转
                    if (autoRotation)
                    {
                        Quaternion delta = Quaternion.Euler(autoRotationSpeed.x, autoRotationSpeed.y, autoRotationSpeed.z);
                        UpdateChildren(delta);
                        SortChildren();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 更新节点的位置
    /// </summary>
    /// <param name="delta">偏移增量</param>
    private void UpdateChildren(Quaternion delta)
    {
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var vec = (child.position - transform.position).normalized;
            var newVec = delta*vec;
            child.position = GetPointOnEllipsoid(newVec);
        }
    }
    
    
    public void BeginRotate(Vector2 touchPos)
    {
        if (!lockClick)
        {
            _clickTurn = false;
            _clickObject = null;
        }
        if (!_clickTurn)
        {
            _deltaQuaternion = Quaternion.identity;
            _beginPosition = touchPos;
            _originalRotation = transform.rotation;
            _originalLocalRotation = transform.localRotation;
            _rotationDamping = true;
        }
    }
    
    public void Rotate(Vector2 touchPos)
    {
        if (!_clickTurn)
        {
            var delta = touchPos - _beginPosition;
            //旋转
            _deltaRotation.y += horizontal ? -delta.x*rotationSpeed:0;
            _deltaRotation.x += vertical ? delta.y*rotationSpeed:0;
            var x = _deltaRotation.x % 360f;
            _deltaRotation.x = (x % 180f);
            var y = _deltaRotation.y % 360f;
            _deltaRotation.y = (y % 180f);
            _rotationDamping = true;
            _beginPosition = touchPos;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Rotate(eventData.position);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginRotate(eventData.position);
    }

    public void Rotate(Vector3 delta)
    {
        _deltaRotation = delta;
        _rotationDamping = true;
    }
    
}