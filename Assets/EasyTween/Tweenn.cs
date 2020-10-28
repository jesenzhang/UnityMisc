using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTween
{
	public enum TweenLoopType
	{
		Restart,
		PingPong,//目标值互换 
		Incremental,//增加
		TimePingPong,//时间值乒乓
	}

   /// <summary>
	/// The Tween class stores the animation information;
	/// </summary>
	public class Tweenn {
	   
	   public enum TweenParamType
	   {
		    Float, Vec2, Vec3, Color, Quaternion 
	   }
	   public enum TweenUpdateMode
	   {
		   Update,Fixed,Late
	   }

	    private float _delay,_elapsedDelay, _elapsedtime, _duration, _normalizedPosition,_loop;
	    private float _fromValue0, _fromValue1,_fromValue2,_fromValue3;
	    private float _toValue0, _toValue1,_toValue2,_toValue3;
	    private float _resultValue0, _resultValue1,_resultValue2,_resultValue3;
	    
	    private int _runTimes = 0;
	    private Func<float, float, float, float> _easeFunctionDelegate;
		private Action<float,float,float,float> _valuesSetter;
		private TweenUpdateMode _tweenUpdate = TweenUpdateMode.Update;
		private Func<float> _deltaTimeDelegate;
		private Action _onComplete;
		private const Action DoNothing = null;
		private bool _forward = true;
		private bool _autoKill = true;
		private bool _pause = false;
		private bool _useCurve = false;
		private AnimationCurve _easeCurve = new AnimationCurve();
		private TweenLoopType _loopType = TweenLoopType.Restart;
		private TweenParamType _paramType = TweenParamType.Float;
		public TweenLoopType LoopType => _loopType;

		public Tweenn SetLoop(TweenLoopType loopType,int loop = 1)
		{
			_loopType = loopType;
			_loop = loop;
			return this;
		}
		
		public TweenParamType ParamType => _paramType;
		public Tweenn SetParamType(TweenParamType paramType)
		{
			_paramType = paramType;
			return this;
		}
		
		public float Delay => _delay;
		public Tweenn SetDelay(float delay)
		{
			_delay = delay;
			return this;
		}
		
		public float Duration => _duration;
		public Tweenn SetDuration(float duration)
		{
			_duration = duration;
			return this;
		}
		
		public bool AutoKill => _autoKill;
		public Tweenn SetAutoKill(bool autoKill)
		{
			_autoKill = autoKill;
			return this;
		}
		
		public TweenUpdateMode UpdateMode => _tweenUpdate;
		public Tweenn SetUpdateMode(TweenUpdateMode tweenUpdate)
		{
			this._tweenUpdate = tweenUpdate;
			return this;
		}
		
		public Tweenn SetDeltaTimeFunc(Func<float> deltaTimeDelegate)
		{
			_deltaTimeDelegate = deltaTimeDelegate;
			return this;
		}
		
		public Tweenn SetDefaultTime(bool unScale)
		{
			_deltaTimeDelegate = TimeFunction(unScale);
			return this;
		}
		
		public Tweenn OnComplete(Action onComplete)
		{
			_onComplete = onComplete;
			return this;
		}
		
		
		public Tweenn SetValuesSetter(Action<float,float,float,float> valuesSetter)
		{
			_valuesSetter = valuesSetter;
			return this;
		}

		
		public Tweenn SetEase(EaseEquation easeEquation)
		{
			_useCurve = false;
			_easeFunctionDelegate = EasingCoreFunction.Get(easeEquation);
			return this;
		}
		
		public Tweenn SetEaseCurve(AnimationCurve easeEquation)
		{
			_useCurve = true;
			_easeCurve = easeEquation;
			return this;
		}
		
		
		public Tweenn SetFromValue(float v0,float v1=0f,float v2=0f,float v3=0f)
		{
			_fromValue0 = v0;
			_fromValue1 = v1;
			_fromValue2 = v2;
			_fromValue3 = v3;
			return this;
		}
		
		public Tweenn SetToValue(float v0,float v1=0f,float v2=0f,float v3=0f)
		{
			_toValue0 = v0;
			_toValue1 = v1;
			_toValue2 = v2;
			_toValue3 = v3;
			return this;
		}

		#region Tween Constructors

		public static Tweenn Gen()
		{
			return TweenMgr.Instance.GenTween();
		}

		public Tweenn()
		{
			Clear();
		}
		
		#endregion

		#region Animation

		public void Play()
		{
			TweenMgr.Instance.PlayTween(this);
		}
		
		public void Pause(bool pause)
		{
			_pause = pause;
		}
		
		public void Stop()
		{
			TweenMgr.Instance.StopTween(this);
		}
		
		public void Kill()
		{
			TweenMgr.Instance.Kill(this);
		}
		
		public void Reset()
		{
			_elapsedDelay = 0;
			_runTimes = 0;
			_elapsedtime = 0;
			_normalizedPosition = 0;
			_runTimes = 0;
			DoEase();
		}
		
		public void Clear()
		{
			_valuesSetter = null;
			_easeFunctionDelegate = EasingCoreFunction.Get(EaseEquation.Linear);
			_duration = 0;
			_onComplete = DoNothing;
			_paramType = TweenParamType.Float;
			_loopType = TweenLoopType.Restart;
			_loop = 1;
			_deltaTimeDelegate = TimeFunction(false);
			_elapsedDelay = 0;
			_runTimes = 0;
			_elapsedtime = 0;
			_normalizedPosition = 0;
			_forward = true;
			_delay = 0;
			_elapsedDelay = 0;
			_pause = false;
			_fromValue0 = _fromValue1 = _fromValue2 = _fromValue3 = _toValue0 = _toValue1 =
				_toValue2 = _toValue3 = _resultValue0 = _resultValue1 = _resultValue2 = _resultValue3 = 0;
			_autoKill = true;
			_useCurve = false;
			_easeCurve = null;
		}
		
		public bool Update()
		{
			if (_pause) return false;
			_elapsedDelay += _deltaTimeDelegate();
			if (_elapsedDelay < _delay)
			{
				return false;
			}

			_elapsedtime += _forward ? _deltaTimeDelegate() : -_deltaTimeDelegate();
			_normalizedPosition =Mathf.Clamp01(_elapsedtime/_duration);
			DoEase();
			return CheckRepeat();
		}

		private float EvaluateCurve(float from,float to,float t)
		{
			return from + (to -from) *_easeCurve.Evaluate(t);
		}

		private void DoEase()
		{
			var func = _useCurve ? EvaluateCurve: _easeFunctionDelegate;//(f, f1, arg3) => EvaluateCurve(f, f1, arg3) : _easeFunctionDelegate;
			if (_loopType == TweenLoopType.Incremental)
			{
				_resultValue0 = func(_fromValue0+_toValue0*_runTimes, _fromValue0+_toValue0*(_runTimes+1), _normalizedPosition);
				_resultValue1 = func(_fromValue1+_toValue1*_runTimes, _fromValue1+_toValue1*(_runTimes+1), _normalizedPosition);
				_resultValue2 = func(_fromValue2+_toValue2*_runTimes, _fromValue2+_toValue2*(_runTimes+1), _normalizedPosition);
				_resultValue3 = func(_fromValue3+_toValue3*_runTimes, _fromValue3+_toValue3*(_runTimes+1), _normalizedPosition);
			}else
			if (_loopType == TweenLoopType.PingPong)
			{ 
				bool f = _runTimes % 2 == 0;
				_resultValue0 = func(f?_fromValue0:_toValue0, f?_toValue0:_fromValue0, _normalizedPosition);
				_resultValue1 = func(f?_fromValue1:_toValue1, f?_toValue1:_fromValue1, _normalizedPosition);
				_resultValue2 = func(f?_fromValue2:_toValue2, f?_toValue2:_fromValue2, _normalizedPosition);
				_resultValue3 = func(f?_fromValue3:_toValue3, f?_toValue3:_fromValue3, _normalizedPosition);
			}
			else
			{
				_resultValue0 = func(_fromValue0, _toValue0, _normalizedPosition);
				_resultValue1 = func(_fromValue1, _toValue1, _normalizedPosition);
				_resultValue2 = func(_fromValue2, _toValue2, _normalizedPosition);
				_resultValue3 = func(_fromValue3, _toValue3, _normalizedPosition);
			}
			this._valuesSetter(_resultValue0,_resultValue1,_resultValue2,_resultValue3);
		}

		private void OnEnd()
		{
			_onComplete?.Invoke();
		}

		private bool CheckRepeat()
		{
			var forwardEnd = (_forward && _elapsedtime >= _duration);
			var backwardEnd = (!_forward && _elapsedtime <= 0);
			if (forwardEnd || backwardEnd)
			{
				if (_loop >= 1)
				{
					_runTimes++;
					if (_runTimes < _loop)
					{
						ReSetLoop();
						return false;
					}
					OnEnd();
					return true;
				}
				else if (_loop < 0)
				{
					_runTimes++;
					ReSetLoop();
					return false;
				}
			}
			return false;
		}

		private void ReSetLoop()
		{
			if (_loopType == TweenLoopType.TimePingPong)
			{
				_elapsedtime = _forward?_duration:0;
				_forward = !_forward;
			}
			else
			{
				_elapsedtime = _forward?0:_duration;
			}
		}

		#endregion

		public static Func<float> TimeFunction(bool unscaled)
		{
			return unscaled ? (Func<float>)(()=>Time.unscaledDeltaTime) : (Func<float>)(()=> Time.deltaTime);
		}
	}
}