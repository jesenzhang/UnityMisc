
using System.Collections.Generic;
using UnityEngine;

namespace EasyTween
{

    public class TweenMgr : MonoBehaviour
    {
        private static TweenMgr _instance = null;
        public static TweenMgr Instance {
			
            get {
                if (_instance == null) {
                    _instance = GameObject.FindObjectOfType(typeof(TweenMgr)) as TweenMgr;
                    if (_instance == null) {
                        _instance = new GameObject().AddComponent<TweenMgr>();
                        _instance.gameObject.name = _instance.GetType().Name;
                    }
                }
                return _instance;
				
            }
        }
        public static bool HasInstance {
            get {
                return !IsDestroyed;
            }
        }
        public static bool IsDestroyed {
            get {
                return (_instance == null) ? true : false;
            }
        }

        protected virtual void OnDestroy () {
            onDestruction();
            _instance = null;
        }
		
        protected void OnApplicationQuit () {
            onDestruction();
            _instance = null;
        }
    
        protected virtual void onDestruction()
        {
            StopAllCoroutines();
        }
        
        	private WaitForEndOfFrame wait = new WaitForEndOfFrame();

		#region Public Tween Starters

		private List<Tweenn> _tweens;
		private List<Tweenn> _runningTweens;
		private List<Tweenn> _runningTweensFixUpdate;
		private List<Tweenn> _runningTweensLateUpdate;
		private List<Tweenn> _abortTweens;

		public TweenMgr()
		{
			_tweens = new List<Tweenn>();
			_runningTweens = new List<Tweenn>();
			_runningTweensFixUpdate = new List<Tweenn>();
			_runningTweensLateUpdate = new List<Tweenn>();
			_abortTweens = new List<Tweenn>();
		}


		public void PlayTween(Tweenn t)
		{
			t.Reset();
			switch (t.UpdateMode)
			{
				case Tweenn.TweenUpdateMode.Update:
					_runningTweens.Add(t);
					break;
				case Tweenn.TweenUpdateMode.Fixed:
					_runningTweensFixUpdate.Add(t);
					break;
				case Tweenn.TweenUpdateMode.Late:
					_runningTweensLateUpdate.Add(t);
					break;
				default:
					_runningTweens.Add(t);
					break;
			}
		}
		
		public void StopTween(Tweenn t){
			
			switch (t.UpdateMode)
			{
				case Tweenn.TweenUpdateMode.Update:
					_runningTweens.Remove(t);
					break;
				case Tweenn.TweenUpdateMode.Fixed:
					_runningTweensFixUpdate.Remove(t);
					break;
				case Tweenn.TweenUpdateMode.Late:
					_runningTweensLateUpdate.Remove(t);
					break;
				default:
					_runningTweens.Remove(t);
					break;
			}
			t.Reset();
		}
		
		public void Kill(Tweenn t)
		{
			switch (t.UpdateMode)
			{
				case Tweenn.TweenUpdateMode.Update:
					_runningTweens.Remove(t);
					break;
				case Tweenn.TweenUpdateMode.Fixed:
					_runningTweensFixUpdate.Remove(t);
					break;
				case Tweenn.TweenUpdateMode.Late:
					_runningTweensLateUpdate.Remove(t);
					break;
				default:
					_runningTweens.Remove(t);
					break;
			}
			t.Clear();
			_tweens.Remove(t);
			_abortTweens.Add(t);
		}

		public Tweenn GenTween()
		{
			if (_abortTweens.Count > 0)
			{
				var t = _abortTweens[0];
				_tweens.Add(t);
				_abortTweens.RemoveAt(0);
				return t;
			}
			else
			{
				var t = new Tweenn();
				return t;
			}
		}

		void Update()
		{
			if (_runningTweens.Count>0)
			{
				for (int i = 0; i < _runningTweens.Count; i++)
				{
					var t = _runningTweens[i];
					if (t.Update())
					{
						_runningTweens.Remove(t);
						if(t.AutoKill)
							_abortTweens.Add(t);
					}
				}
			}
		}
		
		void FixedUpdate()
		{
			if (_runningTweensFixUpdate.Count>0)
			{
				for (int i = 0; i < _runningTweensFixUpdate.Count; i++)
				{
					var t = _runningTweensFixUpdate[i];
					if (t.Update())
					{
						_runningTweensFixUpdate.Remove(t);
						if(t.AutoKill)
							_abortTweens.Add(t);
					}
				}
			}
		}
		
		void LateUpdate()
        {
        	if (_runningTweensLateUpdate.Count>0)
        	{
	            for (int i = 0; i < _runningTweensLateUpdate.Count; i++)
	            {
		            var t = _runningTweensLateUpdate[i];
        			if (t.Update())
        			{
	                    _runningTweensLateUpdate.Remove(t);
	                    if(t.AutoKill)
		                    _abortTweens.Add(t);
        			}
        		}
        	}
        }
		
 
		#endregion

		#region Convenience Functions
		
		public void StopAllTweens(){
			foreach (var t in _tweens)
			{
				t.Stop();
			}
		}
		
		public void KillAllTweens()
		{
			StopAllTweens();
			foreach (var t in _tweens)
			{
				t.Clear();
				_abortTweens.Add(t);
			}
			_tweens.Clear();
		}
		
		public static void Dispose()
		{
			if(HasInstance) 
			{
			}
		}

		#endregion

    }
    
}
