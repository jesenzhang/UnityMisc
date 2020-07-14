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
            RandomChildren(celestial);
        }
        
        serializedObject.ApplyModifiedProperties(); 
    }
    
    private void RandomChildren(CelestialSphere celestial)
    {
        Random r = new Random((int)DateTime.Now.Ticks);
        for (int i = 0; i < celestial.Childrens.Count; i++)
        {
            Vector3 d = Vector3.zero;
            var sign = r.Next(0, 2) > 0 ? 1 : -1;
            d.x = sign * (float) r.NextDouble();
            sign = r.Next(0, 2) > 0 ? 1 : -1;
            d.y = sign * (float) r.NextDouble();
            sign = r.Next(0, 2) > 0 ? 1 : -1;
            d.z = sign * (float) r.NextDouble();
        
            var p = celestial.GetPointOnEllipsoid(d);
            celestial.Childrens[i].transform.position =  p;
        }
    }
    
    private void CollectChildren(CelestialSphere celestial)
    { 
        if(celestial.Childrens==null)
            celestial.Childrens =new List<Transform>();
        celestial.Childrens.Clear();
        var count =  celestial.transform.childCount;
        for (var i = 0; i < count; i++)
        {
            var c = celestial.transform.GetChild(i);
            celestial.Childrens.Add(c);
        }
    }
}


#endif

public class Ellipsoid
{
    public Vector3 Center;
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;

    private Matrix4x4 M;
    private Matrix4x4 T;
    private Matrix4x4 R;
    private Matrix4x4 S;

  
    public Ellipsoid(Vector3 Center,Vector3 A,Vector3 B,Vector3 C,Vector3 Rotation)
    {
        this.Center = Center;
        var r = Matrix4x4.Rotate(Quaternion.Euler(Rotation));
        this.A = r.MultiplyVector(A);
        this.B = r.MultiplyVector(B);
        this.C = r.MultiplyVector(C);
        Init();
    }

    public void Init()
    {
        T = new Matrix4x4(
            new Vector4(1,0,0,Center.x),
            new Vector4(0,1,0,Center.y),
            new Vector4(0,0,1,Center.z),
            new Vector4(0,0,0,1));
        var a = A.normalized;
        var b = B.normalized;
        var c = C.normalized;
        R = new Matrix4x4(
            new Vector4(a.x,b.x,c.x,0),
            new Vector4(a.y,b.y,c.y,0),
            new Vector4(a.z,b.z,c.z,0),
            new Vector4(0,0,0,1));
        S = new Matrix4x4(
            new Vector4(A.magnitude,0,0,0),
            new Vector4(0,B.magnitude,0,0),
            new Vector4(0,0,C.magnitude,0),
            new Vector4(0,0,0,1));
        M = T * R * S;
    }

    public Vector3 CaculateLineIntersection(Vector3 lineStart,Vector3 lineDir)
    {
        var L = M.inverse.MultiplyPoint(lineStart);
        var V = M.inverse.MultiplyVector(lineDir);
        var W = L - Vector3.zero;
        var a =Vector3.Dot(V,V);
        var b =2* Vector3.Dot(V, W);
        var c = Vector3.Dot(W, W) - 1;
        var s = b * b - 4 * a * c;
        if (s > 0)
        {
            var p0 = lineStart + ((-b + Mathf.Sqrt(s)) / a) * 0.5f*lineDir;
            var p1 = lineStart + ((-b - Mathf.Sqrt(s)) / a) * 0.5f*lineDir;
            return p0;
        }
        if (s == 0)
        {
            var p = lineStart + (-b / a) * 0.5f*lineDir;
            return p;
        }
        return Vector3.zero;
    }
}


[ExecuteInEditMode]
public class CelestialSphere : MonoBehaviour,IBeginDragHandler,IDragHandler
{
    public List<Transform> Childrens;
    
    [Tooltip("椭圆x轴半长轴")]
    public float A;
    [Tooltip("椭圆y轴半长轴")]
    public float B;
    [Tooltip("椭圆z轴半长轴")]
    public float C;
    [Tooltip("椭圆的旋转")]
    public Vector3 Rotation;
    [Tooltip("最近item的缩放大小")]
    public float maxScale = 1f;
    [Tooltip("最远item的缩放大小")]
    public float minScale = 0.5f;
    [Tooltip("正方向")]
    public Vector3 Forward = Vector3.back;
    [Tooltip("是否控制水平方向")]
    public bool horizontal = true;
    [Tooltip("是否控制垂直方向")]
    public bool vertical = true;
    //旋转速度
    [Tooltip("旋转速度")]
    public float rotationSpeed = 0.001f;
    //旋转速度
    [Tooltip("点击跳转旋转速度")]
    public float turnRotationSpeed = 0.01f;
    //阻尼时间
    [Tooltip("阻尼时间")]
    public float rotationDampingTime = 0.2f;
    [Tooltip("选中对象的缩放")]
    public float clickItemSize = 1.8f;
    [Tooltip("选中时锁定拖拽")]
    public bool lockClick = false;
    [Tooltip("选中旋转时间")]
    public float clickTurnDuration = 0.2f;
    
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

    public Ellipsoid Ellipsoid
    {
        get
        {
            if(_ellipsoid==null)
                _ellipsoid = new Ellipsoid(Vector3.zero, A * Vector3.right, B * Vector3.up,C * Vector3.forward,Rotation);
            return _ellipsoid;
        }
    }

    void Awake()
    {
       
        SortChildren();
    }
    
    public Vector3 GetPointOnEllipsoid(Vector3 dir)
    {
        return transform.position + Ellipsoid.CaculateLineIntersection(Vector3.zero, dir.normalized);
    }
    
    private float ChildDepth(Transform child)
    {
        return Vector3.Dot(Forward,(child.position - transform.position));
    }

    private void SortChildren()
    {
        if (Childrens != null && Childrens.Count > 0)
        {
             
            Childrens.Sort((a,b) =>
            {
                if (ChildDepth(a) > ChildDepth(b))
                {
                    return 1;
                }
                if (ChildDepth(a) < ChildDepth(b))
                {
                    return -1;
                }

                return 0;
            });

            for (int i = 0; i < Childrens.Count; i++)
            {
                Transform child = Childrens[i];
                var maxDistance = GetPointOnEllipsoid(Forward);
                var minDistance = GetPointOnEllipsoid(-1*Forward);
                var dis = maxDistance - minDistance;
                var distance = Mathf.Abs(Vector3.Dot(Forward,(child.position - maxDistance)));
                var max = ReferenceEquals(child,_clickObject) ? clickItemSize : maxScale;
                Childrens[i].transform.localScale = Vector3.Lerp(Vector3.one*max, Vector3.one *minScale,distance/dis.magnitude);
                Childrens[i].SetSiblingIndex(i);
            }
        }
    }
    
    public void TurnToFront(int index)
    {
        Transform child = Childrens[index];
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
            _deltaQuaternion = Quaternion.FromToRotation(vec1, Forward);
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
            var delta = Quaternion.Slerp(Quaternion.identity,_deltaQuaternion, turnRotationSpeed);
            _deltaQuaternion = Quaternion.Inverse(delta) * _deltaQuaternion;
            for (int i = 0; i < Childrens.Count; i++)
            {
                Transform child = Childrens[i];
                var vec = (child.position - transform.position).normalized;
                var old = child.rotation;
                var newrotation = old* Quaternion.Inverse(old) * delta * old;
                Vector3 newVec = delta*vec;
                child.position  = GetPointOnEllipsoid(newVec);
            }
            _deltaRotation = Vector3.zero;
            SortChildren();
        }
        else
        {
            if (_turnback && _clickObject != null)
            {
                _localTime += Time.deltaTime;
                var f = Mathf.Clamp(_localTime/clickTurnDuration,0, 1);
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

                    for (int i = 0; i < Childrens.Count; i++)
                    {
                        Transform child = Childrens[i];
                        var vec = (child.position - transform.position).normalized;
                        Quaternion quaternion = Quaternion.Euler(_deltaRotation.x, _deltaRotation.y, _deltaRotation.z);
                        var old = child.rotation;
                        var newrotation = old* Quaternion.Inverse(old) * quaternion * old;
                        Vector3 newVec = quaternion*vec;
                       
                        child.position = GetPointOnEllipsoid(newVec);
                    }
 
            
                    if (_deltaRotation == Vector3.zero)
                        _rotationDamping = false;
            
                    SortChildren();
                }
            }
        }
    }

    public void BeginRotate(Vector2 touchPos)
    {
        if(!lockClick)
            _clickTurn = false;
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