using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTween
{
    public enum TweenUpdateMode
    {
        Update,Fixed,Late
    }
    
    public class TweenPlayer
    {
        public enum LoopType
        {
            Restart,
            PingPong,//目标值互换
            HoldOn,
        }
        
        private List<TweenTrack> _tracks;
        private bool _pause = false,_forward=true;
        private float _elapsedtime;
        private int _runTimes;
        private TweenPlayer.LoopType _loopType = LoopType.Restart;
        private int _loopTime = 1;
        private float _duration;
        public float Duration => _duration;
        private TweenUpdateMode _tweenUpdate = TweenUpdateMode.Update;
        private Func<float> _deltaTimeDelegate;
        
        private Action _onStart;
        private Action _onComplete;

        private bool _completed = false;
        
        private bool _autoKill = false;
        public bool AutoKill => _autoKill;
        public bool IsCompleted => _completed;
        
        public TweenPlayer()
        {
            _tracks = new List<TweenTrack>();
            Clear();
            _completed = false;
        }

        public void Add(TweenTrack track)
        {
            if(!_tracks.Contains(track))
                _tracks.Add(track);
        }
        
        public void Remove(TweenTrack track)
        {
            if(_tracks.Contains(track))
                _tracks.Remove(track);
        }

        public void Clear()
        {
            if (_tracks == null) return;
            _tracks.Clear();
            Reset();
            _onStart = null;
            _onComplete = null;
            _deltaTimeDelegate = null;
            _loopType = LoopType.Restart;
            _loopTime = 1;
            _tweenUpdate = TweenUpdateMode.Update;
            _duration = 0;
            
        }

        public TweenPlayer SetAutoKill(bool autoKill)
        {
            _autoKill = autoKill;
            return this;
        }
        
        public TweenPlayer SetDuration(float duration)
        {
            _duration = duration;
            return this;
        }
        
        public TweenPlayer SetLoop(LoopType loopType,int loop = 1)
        {
            _loopType = loopType;
            _loopTime = loop;
            return this;
        }

        public TweenUpdateMode UpdateMode => _tweenUpdate;
        public TweenPlayer SetUpdateMode(TweenUpdateMode tweenUpdate)
        {
            this._tweenUpdate = tweenUpdate;
            return this;
        }
		
        public TweenPlayer SetDeltaTimeFunc(Func<float> deltaTimeDelegate)
        {
            _deltaTimeDelegate = deltaTimeDelegate;
            return this;
        }
		
        public TweenPlayer SetDefaultTime(bool unScale)
        {
            _deltaTimeDelegate = TimeFunction(unScale);
            return this;
        }
        
        public void Update()
        {
            if (_pause) return;
            _elapsedtime += _forward ? _deltaTimeDelegate() : -_deltaTimeDelegate();

            if (_tracks == null) return;
            Evaluate(_elapsedtime);
        }
        
        public void Evaluate(float runningTime)
        {
            _elapsedtime = runningTime;
            for (int i = 0; i < _tracks.Count; i++)
            {
                var track = _tracks[i];
                track.Update(_elapsedtime);
            }
            CheckRepeat();
        }
        
        private void CheckRepeat()
        {
            var forwardEnd = (_forward && _elapsedtime > _duration);
            var backwardEnd = (!_forward && _elapsedtime < 0);
            if (forwardEnd || backwardEnd)
            {
                if (_loopTime >= 1)
                {
                    _runTimes++;
                    if (_runTimes < _loopTime)
                    {
                        ReSetLoop();
                    }
                    else
                    {
                        OnEnd();
                    }
                }
                else if (_loopTime < 0)
                {
                    _runTimes++;
                    ReSetLoop();
                }
            }
        }

        private void ReSetLoop()
        {
            if (_loopType == LoopType.PingPong)
            {
                _elapsedtime = _forward?_duration:0;
                _forward = !_forward;
            }
            else if (_loopType == LoopType.HoldOn)
            {
                _elapsedtime = _forward?_duration:0;
            }
            else
            {
                _elapsedtime = _forward?0:_duration;
            }
        }
        
        private void OnEnd()
        {
            _pause = true;
            _onComplete?.Invoke();
            _completed = true;
            
        }
        
        public static TweenPlayer Gen()
        {
            return TweenPlayerMgr.Instance.Gen();
        }
        
        public void Play()
        {
            Reset();
            _pause = false;
            TweenPlayerMgr.Instance.Play(this);
        }
        
        public void Pause()
        {
            _pause = true;
        }
        
        public void Stop()
        {
            TweenPlayerMgr.Instance.Stop(this);
            Reset();
        }
        
        public void Kill()
        {
            TweenPlayerMgr.Instance.Kill(this);
            Clear();
        }
        
        public void Reset()
        {
            _forward = true;
            _pause = true;
            _elapsedtime = 0;
            _runTimes = 0;
            _completed = false;
        }
        
        public static Func<float> TimeFunction(bool unscaled)
        {
            return unscaled ? (Func<float>)(()=>Time.unscaledDeltaTime) : (Func<float>)(()=> Time.deltaTime);
        }
    }
}