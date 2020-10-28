using System.Collections;
using System.Collections.Generic;

namespace EasyTween
{
    public class TweenCallback : Sequential
    {
        protected override SequentialType SequentialType()
        {
            return EasyTween.SequentialType.Callback;
        }

        public override void Evaluate(object bindTarget,float runningTime)
        {
            throw new System.NotImplementedException();
        }

        public override void Clear()
        {
            throw new System.NotImplementedException();
        }

        public TweenCallback(float startTime, SequentialCallback callback)
        {
            this.startTime = startTime;
            this.onStart = callback;
        }
    }
    
}