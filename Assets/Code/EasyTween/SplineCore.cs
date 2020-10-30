using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
 
 namespace SplineSolver
 {
     public class SplineCore
     {
         #region 贝塞尔样条
         /// <summary>
         /// 德卡斯特里奥算法（De Casteljau’s Algorithm）N次贝塞尔曲线
         /// </summary>
         /// <param name="N">贝塞尔次数  </param>
         /// <param name="iter">迭代次数</param>
         /// <param name="t">采样点</param>
         /// <param name="controlPoints">控制点数组</param>
         /// <returns></returns>
         public static float DeCasteljauBezier(int N, int iter, float t,float[] controlPoints)
         {
             if (N == 1)
             {
                 return (1 - t) * controlPoints[iter] + t * controlPoints[iter + 1];
             }
             return (1 - t) * DeCasteljauBezier(N - 1, iter, t,controlPoints) + t * DeCasteljauBezier(N - 1, iter + 1, t,controlPoints);
         }
         
         public static Vector3 DeCasteljauBezier(int N, int iter, float t,Vector3[] controlPoints)
         {
             if (N == 1)
             {
                 return (1 - t) * controlPoints[iter] + t * controlPoints[iter + 1];
             }
             return (1 - t) * DeCasteljauBezier(N - 1, iter, t,controlPoints) + t * DeCasteljauBezier(N - 1, iter + 1, t,controlPoints);
         }
 
         /// <summary>
         /// 2次贝塞尔
         /// </summary>
         /// <param name="p0">控制点1</param>
         /// <param name="p1">控制点2</param>
         /// <param name="p2">控制点3</param>
         /// <param name="t">采样点【0-1】</param>
         /// <returns></returns>
         public static Vector3 Bezier (Vector3 p0, Vector3 p1, Vector3 p2, float t) {
             var ti = 1f - t;
             var t0 = ti * ti;
             var t1 = 2f * ti * t;
             var t2 = t * t;
             return (t0 * p0) + (t1 * p1) + (t2 * p2);
         }
         
         public static float Bezier (float p0, float p1, float p2, float t) {
             var ti = 1f - t;
             var t0 = ti * ti;
             var t1 = 2f * ti * t;
             var t2 = t * t;
             return (t0 * p0) + (t1 * p1) + (t2 * p2);
         }
 
         /// <summary>
         /// 2次贝塞尔一阶导数
         /// </summary>
         /// <param name="p0"></param>
         /// <param name="p1"></param>
         /// <param name="p2"></param>
         /// <param name="t"></param>
         /// <returns></returns>
         public static Vector3 BezierFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, float t) {
             return
                 2f * (1f - t) * (p1 - p0) +
                 2f * t * (p2 - p1);
         }
         
         public static float BezierFirstDerivative (float p0, float p1, float p2, float t) {
             return
                 2f * (1f - t) * (p1 - p0) +
                 2f * t * (p2 - p1);
         }
 
         /// <summary>
         /// 3次贝塞尔
         /// </summary>
         /// <param name="p0"></param>
         /// <param name="p1"></param>
         /// <param name="p2"></param>
         /// <param name="p3"></param>
         /// <param name="t"></param>
         /// <returns></returns>
         public static float CubicBezier(float p0,float p1,float p2,float p3,float t)
         {
             var ti = 1 - t;
             var t0 = ti * ti * ti;
             var t1 = 3 * ti * ti * t;
             var t2 = 3 * ti * t * t;
             var t3 = t * t * t;
             return (t0 * p0) + (t1 * p1) + (t2 * p2) + (t3 * p3);
         }
         
         public static Vector3 CubicBezier(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,float t)
         {
             var ti = 1 - t;
             var t0 = ti * ti * ti;
             var t1 = 3 * ti * ti * t;
             var t2 = 3 * ti * t * t;
             var t3 = t * t * t;
             return (t0 * p0) + (t1 * p1) + (t2 * p2) + (t3 * p3);
         }
         
         /// <summary>
         /// 3次贝塞尔一阶导数
         /// </summary>
         /// <param name="p0"></param>
         /// <param name="p1"></param>
         /// <param name="p2"></param>
         /// <param name="p3"></param>
         /// <param name="t"></param>
         /// <returns></returns>
         public static Vector3 CubicBezierFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
             var ti = 1f - t;
             return
                 3f * ti * ti * (p1 - p0) +
                 6f * ti * t * (p2 - p1) +
                 3f * t * t * (p3 - p2);
         }
         
         public static float CubicBezierFirstDerivative (float p0, float p1, float p2, float p3, float t) {
             var ti = 1f - t;
             return
                 3f * ti * ti * (p1 - p0) +
                 6f * ti * t * (p2 - p1) +
                 3f * t * t * (p3 - p2);
         }
         #endregion
 
         #region B样条
         /// <summary>
         /// 定义域细分节点
         /// </summary>
         /// <param name="p"></param>
         /// <param name="i"></param>
         /// <returns></returns>
         private static float Knot(int p,int i)
         {
             return i/p;
         }
 
         private static float CoxDeBoor(int p, int iter,float u)
         {
             var ki = Knot(p, iter);
             var ki1 = Knot(p, iter+1);
             var ki1p = Knot(p, iter+1+p);
             var kip = Knot(p, iter+p);
             if (p == 0)
             {
                 if (u >= ki && u <ki1) return 1;
                 return 0;
             }
             var param1 = (u - ki)/(kip - ki);
             var param2 = (ki1p -u)/(ki1p- ki1);
             return  param1* CoxDeBoor(p - 1, iter, u) + param2 * CoxDeBoor(p - 1, iter + 1, u);
         }
 
         /// <summary>
         /// 均匀B样条
         /// </summary>
         /// <param name="P">次数</param>
         /// <param name="u">插值时间</param>
         /// <param name="controlPoints"></param>
         /// <returns></returns>
         public static float AverageBSpline(int p,float u,float[] controlPoints)
         {
             float ret = 0;
             var n = controlPoints.Length-1;
             for (int i = 0; i <= n;i++)
             {
                 var baseParam = CoxDeBoor(p, i, u);
                 ret +=  baseParam * controlPoints[i];
             }
             return ret;
         }
         /// <summary>
         /// 均匀B样条
         /// </summary>
         /// <param name="p">次数</param>
         /// <param name="u">插值时间</param>
         /// <param name="controlPoints"></param>
         /// <returns></returns>
         public static Vector3 AverageBSpline(int p,float u,Vector3[] controlPoints)
         {
             Vector3 ret = Vector3.zero;
             var n = controlPoints.Length-1;
             for (int i = 0; i <= n;i++)
             {
                 var baseParam = CoxDeBoor(p, i, u);
                 ret +=  baseParam * controlPoints[i];
             }
             return ret;
         }
         
         #endregion
         
         
     }
 
 }