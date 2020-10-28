
using System;
using UnityEngine;

namespace EasyTween
{
    public class Tween : Sequential
    {
        private ValueEvaluate _evaluate;
        private float _duration;
        public float Duration => _duration;
        private LoopType _loopType = LoopType.Restart;
        private int _loopTime = 1;
        
        protected override SequentialType SequentialType()
        {
            return EasyTween.SequentialType.Tween;
        }

        public Tween SetEase(EaseEquation easeEquation)
        {
            _evaluate.SetEase(easeEquation);
            return this;
        }
		
        public Tween SetEaseCurve(AnimationCurve easeEquation)
        {
            _evaluate.SetEaseCurve(easeEquation);
            return this;
        }
        public Tween SetValuesSetter(Action<object,float,float,float,float> valuesSetter)
        {
            _evaluate.SetValuesSetter(valuesSetter);
            return this;
        }

        public Tween SetFromValue(float v0,float v1=0f,float v2=0f,float v3=0f)
        {
            _evaluate.SetFromValue(v0, v1, v2, v3);
            return this;
        }
		
        public Tween SetToValue(float v0,float v1=0f,float v2=0f,float v3=0f)
        {
            _evaluate.SetToValue(v0, v1, v2, v3);
            return this;
        }
    
        public Tween SetDuration(float duration)
        {
            _duration = duration;
            return this;
        }
        
        public Tween SetLoop(LoopType loopType,int loop = 1)
        {
            _loopType = loopType;
            _loopTime = loop;
            return this;
        }

        private float CaculateNormalize(float runningTime)
        {
            float normalize = 0f;
            if (_loopType == LoopType.Restart)
            {
                normalize = (runningTime - startTime)% _duration /_duration;
            }else
            if (_loopType == LoopType.PingPong)
            {
                var t = runningTime - startTime;
                var n = Mathf.Floor(t / _duration);
                var modn = n % 2;
                var modt = t % _duration /_duration;
                normalize = modt + modn*(1-2*modt);
            }
            else if(_loopType == LoopType.HoldOn)
            {
                normalize = 1f;
            }
            else if(_loopType == LoopType.Incremental)
            {
                normalize = (runningTime - startTime)/(_duration);
            }
            return normalize;
        }

        public override void Evaluate(object bindTarget,float runningTime)
        {
            float normalize = runningTime < startTime? 0 :
                runningTime > endTime ? 1 : CaculateNormalize(runningTime);
            _evaluate.target = bindTarget;
            _evaluate.Evaluate(normalize);
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public Tween()
        {
            _evaluate = new ValueEvaluate();
        }
    }
}