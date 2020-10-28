using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTween
{
    
    public static class TweenExtension
    {
        public static Tweenn MoveTo(this Transform trans, Vector3 start, Vector3 target, float duration, EaseEquation easeEquation, bool unscaled = false, 
            TweenLoopType loopType = TweenLoopType.Restart,int loop=1,
            Action onComplete = null)
        {
            var t = Tweenn.Gen();
            t.SetFromValue(start.x, start.y, start.z);
            t.SetToValue(target.x, target.y, target.z);
            t.SetDefaultTime(unscaled);
            t.SetEase(easeEquation);
            t.SetDuration(duration);
            t.SetLoop(loopType,loop);
            t.OnComplete(onComplete);
            t.SetValuesSetter((x, y, z, w) =>
            {
                trans.position =  new Vector3(x,y,z);
            });
            t.Play();
            return t;
        }
        
        public static Tweenn MoveTo(this Transform trans, Vector3 start, Vector3 target, float duration, AnimationCurve easeEquation, bool unscaled = false, 
            TweenLoopType loopType = TweenLoopType.Restart,int loop=1,
            Action onComplete = null)
        {
            var t = Tweenn.Gen();
            t.SetFromValue(start.x, start.y, start.z);
            t.SetToValue(target.x, target.y, target.z);
            t.SetDefaultTime(unscaled);
            t.SetEaseCurve(easeEquation);
            t.SetDuration(duration);
            t.SetLoop(loopType,loop);
            t.OnComplete(onComplete);
            t.SetValuesSetter((x, y, z, w) =>
            {
                trans.position =  new Vector3(x,y,z);
            });
            t.Play();
            return t;
        }
    
        
        public static TweenPlayer MoveTo2(this Transform trans, Vector3 start, Vector3 target, float duration, AnimationCurve easeEquation, bool unscaled = false, 
            LoopType loopType = LoopType.Restart,int loop=1,
            Action onComplete = null)
        {
            var player = TweenPlayer.Gen();
            TweenTrack track = new TweenTrack();
            player.Add(track);
            Tween t = new Tween();
            t.SetFromValue(start.x, start.y, start.z);
            t.SetToValue(target.x, target.y, target.z);
            player.SetDefaultTime(unscaled);
            t.SetEase(EaseEquation.Linear);
            t.SetLoop(loopType,loop);
            track.bindTarget = trans;
            t.SetValuesSetter((o,x, y, z, w) =>
            {
                ((Transform)o).position =  new Vector3(x,y,z);
            });
            t.startTime = 0;
            t.SetDuration(duration);
            t.endTime = duration * loop;
            player.SetDuration(duration * loop);
            track.Add(t);
            player.Play();
            return player;
        }

       
        public static void EaseVector2(this Vector2 changing,Func<float, float, float, float>easingFunction, Vector2 from, Vector2 to, float t)
        {
            float newX = easingFunction(from.x, to.x, t);
            float newY = easingFunction(from.y, to.y, t);
            changing.x = newX;
            changing.y = newY;
        }
        
        public static Vector2 EaseVector2(Func<float, float, float, float>easingFunction, Vector2 from, Vector2 to, float t){
            float newX = easingFunction(from.x, to.x, t);
            float newY = easingFunction(from.y, to.y, t);
            return new Vector2(newX, newY);
        }
        
        public static void EaseVector2(Func<float, float, float, float>easingFunction, Vector2 from, Vector2 to, float t,ref Vector2 changing){
            float newX = easingFunction(from.x, to.x, t);
            float newY = easingFunction(from.y, to.y, t);
            changing.x = newX;
            changing.y = newY;
        }
        
        public static void EaseVector3(this Vector3 changing,Func<float, float, float, float>easingFunction, Vector3 from, Vector3 to, float t)
        {
            float newX = easingFunction(from.x, to.x, t);
            float newY = easingFunction(from.y, to.y, t);
            float newZ = easingFunction(from.z, to.z, t);
            changing.x = newX;
            changing.y = newY;
            changing.z = newZ;
        }
        
        public static Vector3 EaseVector3(Func<float, float, float, float>easingFunction, Vector3 from, Vector3 to, float t){
            float newX = easingFunction(from.x, to.x, t);
            float newY = easingFunction(from.y, to.y, t);
            float newZ = easingFunction(from.z, to.z, t);

            return new Vector3(newX, newY, newZ);
        }
        
        public static void EaseVector3(Func<float, float, float, float>easingFunction, Vector3 from, Vector3 to, float t,ref Vector3  changing){
            float newX = easingFunction(from.x, to.x, t);
            float newY = easingFunction(from.y, to.y, t);
            float newZ = easingFunction(from.z, to.z, t);
            changing.x = newX;
            changing.y = newY;
            changing.z = newZ;
        }
        
        public static void EaseQuaternion(this Quaternion changing,Func<float, float, float, float>easingFunction, Quaternion from, Quaternion to, float t)
        {
            float newX = easingFunction(from.x, to.x, t);
            float newY = easingFunction(from.y, to.y, t);
            float newZ = easingFunction(from.z, to.z, t);
            float newW = easingFunction(from.w, to.w, t);

            changing.x = newX;
            changing.y = newY;
            changing.z = newZ;
            changing.w = newW;
        }
        
        public static Quaternion EaseQuaternion(Func<float, float, float, float>easingFunction, Quaternion from, Quaternion to, float t){
            float newX = easingFunction(from.x, to.x, t);
            float newY = easingFunction(from.y, to.y, t);
            float newZ = easingFunction(from.z, to.z, t);
            float newW = easingFunction(from.w, to.w, t);

            return new Quaternion(newX, newY, newZ, newW);
        }
        
        
        public static void EaseQuaternion(Func<float, float, float, float>easingFunction, Quaternion from, Quaternion to, float t,ref Quaternion changing){
            float newX = easingFunction(from.x, to.x, t);
            float newY = easingFunction(from.y, to.y, t);
            float newZ = easingFunction(from.z, to.z, t);
            float newW = easingFunction(from.w, to.w, t);

            changing.x = newX;
            changing.y = newY;
            changing.z = newZ;
            changing.w = newW;
        }
        
        public static void EaseQuaternion(this Color changing,Func<float, float, float, float>easingFunction, Color from, Color to, float t)
        {
            float newR = easingFunction(from.r, to.r, t);
            float newG = easingFunction(from.g, to.g, t);
            float newB = easingFunction(from.b, to.b, t);
            float newA = easingFunction(from.a, to.a, t);

            changing.r = newR;
            changing.g = newG;
            changing.b = newB;
            changing.a = newA;
        }
        
        public static Color EaseColor(Func<float, float, float, float>easingFunction, Color from, Color to, float t){
            float newR = easingFunction(from.r, to.r, t);
            float newG = easingFunction(from.g, to.g, t);
            float newB = easingFunction(from.b, to.b, t);
            float newA = easingFunction(from.a, to.a, t);

            return new Color(newR, newG, newB, newA);
        }
        
        public static void EaseColor(Func<float, float, float, float>easingFunction, Color from, Color to, float t,ref Color changing){
            float newR = easingFunction(from.r, to.r, t);
            float newG = easingFunction(from.g, to.g, t);
            float newB = easingFunction(from.b, to.b, t);
            float newA = easingFunction(from.a, to.a, t);

            changing.r = newR;
            changing.g = newG;
            changing.b = newB;
            changing.a = newA;
        }
    }

}
