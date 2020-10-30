using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueEvaluate
{
    private float _fromValue0, _fromValue1, _fromValue2, _fromValue3;
    private float _toValue0, _toValue1, _toValue2, _toValue3;
    private float _resultValue0, _resultValue1, _resultValue2, _resultValue3;
    
    private Func<float, float, float, float> _easeFunctionDelegate;
    private Action<object,float,float,float,float> _valuesSetter;
    
    private AnimationCurve _easeCurve = new AnimationCurve();
    
    private bool _useCurve = false;
    public object target;
    
    public ValueEvaluate SetEase(EaseEquation easeEquation)
    {
        _useCurve = false;
        _easeFunctionDelegate = EasingCoreFunction.Get(easeEquation);
        return this;
    }
		
    public ValueEvaluate SetEaseCurve(AnimationCurve easeEquation)
    {
        _useCurve = true;
        _easeCurve = easeEquation;
        return this;
    }
    public ValueEvaluate SetValuesSetter(Action<object,float,float,float,float> valuesSetter)
    {
        _valuesSetter = valuesSetter;
        return this;
    }

    public ValueEvaluate SetFromValue(float v0,float v1=0f,float v2=0f,float v3=0f)
    {
        _fromValue0 = v0;
        _fromValue1 = v1;
        _fromValue2 = v2;
        _fromValue3 = v3;
        return this;
    }
		
    public ValueEvaluate SetToValue(float v0,float v1=0f,float v2=0f,float v3=0f)
    {
        _toValue0 = v0;
        _toValue1 = v1;
        _toValue2 = v2;
        _toValue3 = v3;
        return this;
    }
    
    private float EvaluateCurve(float from,float to,float t)
    {
        return from + (to -from) *_easeCurve.Evaluate(t);
    }

    public void Evaluate(float normalizedPosition)
    {
        var func = _useCurve ? EvaluateCurve: _easeFunctionDelegate;
        _resultValue0 = func(_fromValue0, _toValue0, normalizedPosition);
        _resultValue1 = func(_fromValue1, _toValue1, normalizedPosition);
        _resultValue2 = func(_fromValue2, _toValue2, normalizedPosition);
        _resultValue3 = func(_fromValue3, _toValue3, normalizedPosition);
        _valuesSetter(target,_resultValue0,_resultValue1,_resultValue2,_resultValue3);
    }
    
    public void Clear()
    {
        _valuesSetter = null;
        _easeFunctionDelegate = EasingCoreFunction.Get(EaseEquation.Linear);
        _fromValue0 = _fromValue1 = _fromValue2 = _fromValue3 = _toValue0 = _toValue1 = 
            _toValue2 = _toValue3 = _resultValue0 = _resultValue1 = _resultValue2 = _resultValue3 = 0;
        _useCurve = false;
        _easeCurve = null;
    }

}
