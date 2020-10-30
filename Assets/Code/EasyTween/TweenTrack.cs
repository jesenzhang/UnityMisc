using System.Collections.Generic;
using UnityEngine;

namespace EasyTween
{
    public class TweenTrack
    {
        private List<Sequential> _clips;

        public TweenTrack()
        {
            _clips = new List<Sequential>();
        }

        public void Add(Sequential sequential)
        {
            if(!_clips.Contains(sequential))
                _clips.Add(sequential);
        }
        
        public void Remove(Sequential sequential)
        {
            if(_clips.Contains(sequential))
                _clips.Remove(sequential);
        }

        private void Sort()
        {
            _clips.Sort((a, b) => a.startTime > b.startTime ? 1: a.startTime < b.startTime?-1:0);
        }

        public object bindTarget;

        public void Update(float elapsedtime)
        {
            if(_clips==null)
                return;
            for (int i = 0; i < _clips.Count; i++)
            {
               var clip = _clips[i];
               clip.Evaluate(bindTarget,elapsedtime);
            }
        }

        public void Clear()
        {
            if(_clips==null)
                return;
            for (int i = 0; i < _clips.Count; i++)
            {
                var clip = _clips[i];
                clip.Clear();
            }
            _clips.Clear();
        }
    }
}