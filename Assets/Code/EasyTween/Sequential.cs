using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace EasyTween
{
    public delegate void SequentialCallback();
    
    public enum SequentialType
    {
        Tween,
        Sequence,
        Callback,
    }
    
    public enum LoopType
    {
        Restart,
        PingPong,//目标值互换 
        Incremental,//增加
        HoldOn,
    }

    public abstract class Sequential
    {
        protected abstract SequentialType SequentialType();
        public float startTime;
        public float endTime;
        public SequentialCallback onStart;
        public SequentialCallback onEnd;

        public abstract void Evaluate(object bindTarget,float runningTime);

        public abstract void Clear();
    }
}