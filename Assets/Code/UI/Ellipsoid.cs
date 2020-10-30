
using UnityEngine;

/// <summary>
/// 椭球
/// </summary>
public class Ellipsoid
{
    //中心
    public Vector3 Center;
    //a轴半长轴
    public Vector3 A;
    //b轴半长轴
    public Vector3 B;
    //c轴半长轴
    public Vector3 C;
    //旋转
    public Vector3 Rotation;

    /// <summary>
    /// 变换矩阵
    /// </summary>
    private Matrix4x4 M;
    private Matrix4x4 T;
    private Matrix4x4 R;
    private Matrix4x4 S;

    public Ellipsoid(Vector3 center,Vector3 a,Vector3 b,Vector3 c,Vector3 r)
    {
        Set(center, a, b, c, r);
    }

    /// <summary>
    /// 设置椭球数据
    /// </summary>
    /// <param name="center"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="r"></param>
    public void Set(Vector3 center,Vector3 a,Vector3 b,Vector3 c,Vector3 r)
    {
        this.Center = center;
        this.Rotation = r;
        var mxr = Matrix4x4.Rotate(Quaternion.Euler(this.Rotation));
        this.A = mxr.MultiplyVector(a);
        this.B = mxr.MultiplyVector(b);
        this.C = mxr.MultiplyVector(c);
        
        T = new Matrix4x4(
            new Vector4(1,0,0,Center.x),
            new Vector4(0,1,0,Center.y),
            new Vector4(0,0,1,Center.z),
            new Vector4(0,0,0,1));
        var na = A.normalized;
        var nb = B.normalized;
        var nc = C.normalized;
        R = new Matrix4x4(
            new Vector4(na.x,nb.x,nc.x,0),
            new Vector4(na.y,nb.y,nc.y,0),
            new Vector4(na.z,nb.z,nc.z,0),
            new Vector4(0,0,0,1));
        S = new Matrix4x4(
            new Vector4(A.magnitude,0,0,0),
            new Vector4(0,B.magnitude,0,0),
            new Vector4(0,0,C.magnitude,0),
            new Vector4(0,0,0,1));
        M = T * R * S;
    }

    /// <summary>
    /// 获取球面上一点 
    /// </summary>
    /// <param name="lineStart">射线起始点</param>
    /// <param name="lineDir">射线方向</param>
    /// <returns></returns>
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
        if (Mathf.Abs(s) < 0.001f)
        {
            var p = lineStart + (-b / a) * 0.5f*lineDir;
            return p;
        }
        return Vector3.zero;
    }
}

