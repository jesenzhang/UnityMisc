using System.Collections;
using System.Collections.Generic;

namespace EasyTween
{
    
    public class TweenSequence : Sequential
    {
        private List<Tweenn> _tweenSequence;
        protected override SequentialType SequentialType()
        {
            return EasyTween.SequentialType.Sequence;
        }

        public override void Evaluate(object bindTarget,float runningTime)
        {
            throw new System.NotImplementedException();
        }

        public override void Clear()
        {
            throw new System.NotImplementedException();
        }
    }

}
