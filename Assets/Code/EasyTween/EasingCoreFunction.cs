using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EaseEquation
{
    Linear,
    QuadOut,
    QuadIn,
    QuadInOut,
    QuadOutIn,
    ExpoOut,
    ExpoIn,
    ExpoInOut,
    ExpoOutIn,
    CubicOut,
    CubicIn,
    CubicInOut,
    CubicOutIn,
    QuartOut,
    QuartIn,
    QuartInOut,
    QuartOutIn,
    QuintOut,
    QuintIn,
    QuintInOut,
    QuintOutIn,
    CircOut,
    CircIn,
    CircInOut,
    CircOutIn,
    SineOut,
    SineIn,
    SineInOut,
    SineOutIn,
    ElasticOut,
    ElasticIn,
    ElasticInOut,
    ElasticOutIn,
    BounceOut,
    BounceIn,
    BounceInOut,
    BounceOutIn,
    BackOut,
    BackIn,
    BackInOut,
    BackOutIn,
}


public class EasingCoreFunction
{
    public static Func<float, float> GetCore(EaseEquation type)
    {
        switch (type)
        {
            case EaseEquation.Linear: return Linear;
            case EaseEquation.QuadOut: return QuadOut;
            case EaseEquation.QuadIn: return QuadIn;
            case EaseEquation.QuadInOut: return QuadInOut;
            case EaseEquation.QuadOutIn: return QuadOutIn;
            case EaseEquation.ExpoOut: return ExpoOut;
            case EaseEquation.ExpoIn: return ExpoIn;
            case EaseEquation.ExpoInOut: return ExpoInOut;
            case EaseEquation.ExpoOutIn: return ExpoOutIn;
            case EaseEquation.CubicOut: return CubicOut;
            case EaseEquation.CubicIn: return CubicIn;
            case EaseEquation.CubicInOut: return CubicInOut;
            case EaseEquation.CubicOutIn: return CubicOutIn;
            case EaseEquation.QuartOut: return QuartOut;
            case EaseEquation.QuartIn: return QuartIn;
            case EaseEquation.QuartInOut: return QuartInOut;
            case EaseEquation.QuartOutIn: return QuartOutIn;
            case EaseEquation.QuintOut: return QuintOut;
            case EaseEquation.QuintIn: return QuintIn;
            case EaseEquation.QuintInOut: return QuintInOut;
            case EaseEquation.QuintOutIn: return QuintOutIn;
            case EaseEquation.CircOut: return CircOut;
            case EaseEquation.CircIn: return CircIn;
            case EaseEquation.CircInOut: return CircInOut;
            case EaseEquation.CircOutIn: return CircOutIn;
            case EaseEquation.SineOut: return SineOut;
            case EaseEquation.SineIn: return SineIn;
            case EaseEquation.SineInOut: return SineInOut;
            case EaseEquation.SineOutIn: return SineOutIn;
            case EaseEquation.ElasticOut: return ElasticOut;
            case EaseEquation.ElasticIn: return ElasticIn;
            case EaseEquation.ElasticInOut: return ElasticInOut;
            case EaseEquation.ElasticOutIn: return ElasticOutIn;
            case EaseEquation.BounceOut: return BounceOut;
            case EaseEquation.BounceIn: return BounceIn;
            case EaseEquation.BounceInOut: return BounceInOut;
            case EaseEquation.BounceOutIn: return BounceOutIn;
            case EaseEquation.BackOut: return BackOut;
            case EaseEquation.BackIn: return BackIn;
            case EaseEquation.BackInOut: return BackInOut;
            case EaseEquation.BackOutIn: return BackOutIn;
            default: return Linear;
        }
    }

    public static Func<float, float,float,float> Get(EaseEquation type)
    {
        switch (type)
        {
            case EaseEquation.Linear: return Linear;
            case EaseEquation.QuadOut: return QuadOut;
            case EaseEquation.QuadIn: return QuadIn;
            case EaseEquation.QuadInOut: return QuadInOut;
            case EaseEquation.QuadOutIn: return QuadOutIn;
            case EaseEquation.ExpoOut: return ExpoOut;
            case EaseEquation.ExpoIn: return ExpoIn;
            case EaseEquation.ExpoInOut: return ExpoInOut;
            case EaseEquation.ExpoOutIn: return ExpoOutIn;
            case EaseEquation.CubicOut: return CubicOut;
            case EaseEquation.CubicIn: return CubicIn;
            case EaseEquation.CubicInOut: return CubicInOut;
            case EaseEquation.CubicOutIn: return CubicOutIn;
            case EaseEquation.QuartOut: return QuartOut;
            case EaseEquation.QuartIn: return QuartIn;
            case EaseEquation.QuartInOut: return QuartInOut;
            case EaseEquation.QuartOutIn: return QuartOutIn;
            case EaseEquation.QuintOut: return QuintOut;
            case EaseEquation.QuintIn: return QuintIn;
            case EaseEquation.QuintInOut: return QuintInOut;
            case EaseEquation.QuintOutIn: return QuintOutIn;
            case EaseEquation.CircOut: return CircOut;
            case EaseEquation.CircIn: return CircIn;
            case EaseEquation.CircInOut: return CircInOut;
            case EaseEquation.CircOutIn: return CircOutIn;
            case EaseEquation.SineOut: return SineOut;
            case EaseEquation.SineIn: return SineIn;
            case EaseEquation.SineInOut: return SineInOut;
            case EaseEquation.SineOutIn: return SineOutIn;
            case EaseEquation.ElasticOut: return ElasticOut;
            case EaseEquation.ElasticIn: return ElasticIn;
            case EaseEquation.ElasticInOut: return ElasticInOut;
            case EaseEquation.ElasticOutIn: return ElasticOutIn;
            case EaseEquation.BounceOut: return BounceOut;
            case EaseEquation.BounceIn: return BounceIn;
            case EaseEquation.BounceInOut: return BounceInOut;
            case EaseEquation.BounceOutIn: return BounceOutIn;
            case EaseEquation.BackOut: return BackOut;
            case EaseEquation.BackIn: return BackIn;
            case EaseEquation.BackInOut: return BackInOut;
            case EaseEquation.BackOutIn: return BackOutIn;
            default: return Linear;
        }
    }

    #region Linear
    static float Linear(float t) => t;
    static float Linear(float from, float to,float t) => (to - from)* t + from;
    #endregion
    
    #region Quad
    static float QuadIn(float t) => t * t;
    static float QuadOut(float t) => -t * (t - 2f);
    static float QuadInOut(float t) =>
        t < 0.5f
            ?  2f * t * t
            : -2f * t * t + 4f * t - 1f;
    static float QuadOutIn(float t) =>
        t > 0.5f
            ?  2f * t * t
            : -2f * t * t + 4f * t - 1f;
    static float QuadOut(float from, float to,float t) =>  from + (to - from) * QuadOut(t);
    static float QuadIn(float from, float to,float t) =>  from + (to - from) * QuadIn(t);
    static float QuadInOut(float from, float to,float t) =>  from + (to - from) * QuadInOut(t);
    static float QuadOutIn(float from, float to,float t) =>  from + (to - from) * QuadOutIn(t);
    #endregion
    
    #region Expo
    static float ExpoIn(float t) => Mathf.Approximately(0.0f, t) ? t : Mathf.Pow(2f, 10f * (t - 1f));
    static  float ExpoOut(float t) => Mathf.Approximately(1.0f, t) ? t : 1f - Mathf.Pow(2f, -10f * t);
    static float ExpoInOut(float t) =>
        t < 0.5f
                ?  0.5f * ExpoIn(2f*t)
                : 0.5f * ExpoOut(2f * t-1)+0.5f;
    static float ExpoOutIn(float t) =>
        t < 0.5f
            ?  0.5f * ExpoOut(2f*t)
            : 0.5f * ExpoIn(2f * t-1)+0.5f;
    static float ExpoOut(float from, float to,float t) => from + (to-from) * ExpoOut(t);
    static float ExpoIn(float from, float to,float t) => from + (to-from) * ExpoIn(t);
    static float ExpoInOut(float from, float to,float t)  => from + (to-from) * ExpoInOut(t);
    static float ExpoOutIn(float from, float to,float t) => from + (to-from) * ExpoOutIn(t);
    #endregion
    
    #region Cubic
    static float CubicIn(float t) => t * t * t;
    static float CubicOut(float t) => CubicIn(t - 1f) + 1f;
    static float CubicInOut(float t) =>
        t < 0.5f
            ? 0.5f * CubicIn(2f*t)
            : 0.5f * CubicOut(2f * t-1)+0.5f;
    static float CubicOutIn(float t) =>
        t < 0.5f
            ? 0.5f * CubicOut(2f*t)
            : 0.5f * CubicIn(2f * t-1)+0.5f;
    static float CubicOut(float from, float to,float t) =>  from + (to - from) * CubicOut(t);
    static float CubicIn(float from, float to,float t) =>  from + (to - from) * CubicIn(t);
    static float CubicInOut(float from, float to, float t) => from + (to - from) * CubicInOut(t);
    static float CubicOutIn(float from, float to,float t) => from + (to - from) * CubicOutIn(t);
    #endregion
    
    #region Quart
    static float QuartIn(float t) => t * t * t * t;
    static  float QuartOut(float t)
    {
        var u = t - 1f;
        return u * u * u * (1f - t) + 1f;
    }
    static float QuartInOut(float t) =>
        t < 0.5f
            ? 0.5f * QuartIn(2f*t)
            : 0.5f * QuartOut(2f * t-1)+0.5f;
    static float QuartOutIn(float t) =>
        t < 0.5f
            ? 0.5f * QuartOut(2f*t)
            : 0.5f * QuartIn(2f * t-1)+0.5f;
    static float QuartOut(float from, float to,float t) =>  from + (to - from) *QuartOut(t);
    static float QuartIn(float from, float to,float t) =>  from + (to - from) *QuartIn(t);
    static float QuartInOut(float from, float to,float t) =>  from + (to - from) *QuartInOut(t);
    static float QuartOutIn(float from, float to,float t) =>  from + (to - from) *QuartOutIn(t);
    
    #endregion
    
    #region Quint
    static float QuintIn(float t) => t * t * t * t * t;
    static float QuintOut(float t) => QuintIn(t - 1f) + 1f;
    static float QuintInOut(float t) =>
        t < 0.5f
            ? 0.5f * QuintIn(2f*t)
            : 0.5f * QuintOut(2f * t-1)+0.5f;
    static float QuintOutIn(float t) =>
        t < 0.5f
            ? 0.5f * QuintOut(2f*t)
            : 0.5f * QuintIn(2f * t-1)+0.5f;
    
    static float QuintOut(float from, float to,float t) =>  from + (to - from) *QuintOut(t);
    static float QuintIn(float from, float to,float t) =>  from + (to - from) *QuintIn(t);
    static float QuintInOut(float from, float to,float t) =>  from + (to - from) *QuintInOut(t);
    static float QuintOutIn(float from, float to,float t) =>  from + (to - from) *QuintOutIn(t);
    
    #endregion
    
    #region Circ
    static float CircIn(float t) => 1f - Mathf.Sqrt(1f - (t * t));
    static float CircOut(float t) => Mathf.Sqrt((2f - t) * t);
    static float CircInOut(float t) =>
        t < 0.5f
            ? 0.5f * CircIn(2f*t)
            : 0.5f * CircOut(2f * t-1)+0.5f;
    static float  CircOutIn(float t) =>
        t < 0.5f
            ? 0.5f * CircOut(2f*t)
            : 0.5f * CircIn(2f * t-1)+0.5f;
    static float CircOut(float from, float to,float t) =>  from + (to - from) *CircOut(t);
    static float CircIn(float from, float to,float t) =>  from + (to - from) *CircIn(t);
    static float CircInOut(float from, float to,float t) =>  from + (to - from) *CircInOut(t);
    static float CircOutIn(float from, float to,float t) =>  from + (to - from) *CircOutIn(t);
    
    #endregion
    
    #region  Sine
    static float SineIn(float t) => Mathf.Sin((t - 1f) * (Mathf.PI * 0.5f)) + 1f;

    static float SineOut(float t) => Mathf.Sin(t * (Mathf.PI * 0.5f));
    static float  SineInOut(float t) =>
        t < 0.5f
            ? 0.5f * CircIn(2f*t)
            : 0.5f * CircOut(2f * t-1)+0.5f;
    static float  SineOutIn(float t) =>
        t < 0.5f
            ? 0.5f * CircOut(2f*t)
            : 0.5f * CircIn(2f * t-1)+0.5f;
    static float SineOut(float from, float to,float t) =>  from + (to - from) *SineOut(t);
    static float SineIn(float from, float to,float t) =>  from + (to - from) *SineIn(t);
    static float SineInOut(float from, float to,float t) =>  from + (to - from) *SineInOut(t);
    static float SineOutIn(float from, float to,float t) =>  from + (to - from) *SineOutIn(t);
    
    #endregion
    
    
    #region  Elastic
    static float ElasticIn(float t) => Mathf.Sin(13f * (Mathf.PI * 0.5f) * t) * Mathf.Pow(2f, 10f * (t - 1f));
    static  float ElasticOut(float t) => Mathf.Sin(-13f * (Mathf.PI * 0.5f) * (t + 1)) * Mathf.Pow(2f, -10f * t) + 1f;
    static float ElasticInOut(float t) =>
        t < 0.5f
            ? 0.5f * CircIn(2f*t)
            : 0.5f * CircOut(2f * t-1)+0.5f;
    static float ElasticOutIn(float t) =>
        t < 0.5f
            ? 0.5f * CircOut(2f*t)
            : 0.5f * CircIn(2f * t-1)+0.5f;
    static float ElasticOut(float from, float to,float t) =>  from + (to - from) *ElasticOut(t);
    static float ElasticIn(float from, float to,float t) =>  from + (to - from) *ElasticIn(t);
    static float ElasticInOut(float from, float to,float t) =>  from + (to - from) *ElasticInOut(t);
    static float ElasticOutIn(float from, float to,float t) =>  from + (to - from) *ElasticOutIn(t);
    
    #endregion
    
    #region  Bounce
    static float BounceIn(float t) => 1f - BounceOut(1f - t);
    static float BounceOut(float t) =>
        t < 4f / 11.0f ?
            (121f * t * t) / 16.0f :
            t < 8f / 11.0f ?
                (363f / 40.0f * t * t) - (99f / 10.0f * t) + 17f / 5.0f :
                t < 9f / 10.0f ?
                    (4356f / 361.0f * t * t) - (35442f / 1805.0f * t) + 16061f / 1805.0f :
                    (54f / 5.0f * t * t) - (513f / 25.0f * t) + 268f / 25.0f;
    
    static float BounceInOut(float t) =>
        t < 0.5f
            ? 0.5f * BounceIn(2f*t)
            : 0.5f * BounceOut(2f * t-1)+0.5f;
    static float BounceOutIn(float t) =>
        t < 0.5f
            ? 0.5f * BounceOut(2f*t)
            : 0.5f * BounceIn(2f * t-1)+0.5f;
    static float BounceOut(float from, float to,float t) =>  from + (to - from) *BounceOut(t);
    static float BounceIn(float from, float to,float t) =>  from + (to - from) *BounceIn(t);
    static float BounceInOut(float from, float to,float t) =>  from + (to - from) *BounceInOut(t);
    static  float BounceOutIn(float from, float to,float t) =>  from + (to - from) *BounceOutIn(t);
    #endregion
    
    
    #region  Back
    static  float BackIn(float t) =>t * t * t - t * Mathf.Sin(t * Mathf.PI);
    static float BackOut(float t) =>  1f - BackIn(1f - t);
    
    static float BackInOut(float t) =>
        t < 0.5f
            ? 0.5f * BackIn(2f*t)
            : 0.5f * BackOut(2f * t-1)+0.5f;
    static float BackOutIn(float t) =>
        t < 0.5f
            ? 0.5f * BackOut(2f*t)
            : 0.5f * BackIn(2f * t-1)+0.5f;
    static float BackOut(float from, float to,float t) =>  from + (to - from) *BackOut(t);
    static float BackIn(float from, float to,float t) =>  from + (to - from) *BackIn(t);
    static float BackInOut(float from, float to,float t) =>  from + (to - from) *BackInOut(t);
    static float BackOutIn(float from, float to,float t) =>  from + (to - from) *BackOutIn(t);
    
    #endregion
}
