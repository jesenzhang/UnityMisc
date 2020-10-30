
using System.Collections.Generic;
using UnityEngine;

namespace EasyTween
{

    public class TweenPlayerMgr : MonoBehaviour
    {
        private static TweenPlayerMgr _instance = null;
        public static TweenPlayerMgr Instance {
			
            get {
                if (_instance == null) {
                    _instance = GameObject.FindObjectOfType(typeof(TweenPlayerMgr)) as TweenPlayerMgr;
                    if (_instance == null) {
                        _instance = new GameObject().AddComponent<TweenPlayerMgr>();
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

		#region Public Starters

		private List<TweenPlayer> _players;
		private List<TweenPlayer> _runningPlayers;
		private List<TweenPlayer> _runningPlayersFixUpdate;
		private List<TweenPlayer> _runningPlayersLateUpdate;
		private List<TweenPlayer> _abortPlayers;

		public TweenPlayerMgr()
		{
			_players = new List<TweenPlayer>();
			_runningPlayers = new List<TweenPlayer>();
			_runningPlayersFixUpdate = new List<TweenPlayer>();
			_runningPlayersLateUpdate = new List<TweenPlayer>();
			_abortPlayers = new List<TweenPlayer>();
		}


		public void Play(TweenPlayer t)
		{
			switch (t.UpdateMode)
			{
				case TweenUpdateMode.Update:
					_runningPlayers.Add(t);
					break;
				case TweenUpdateMode.Fixed:
					_runningPlayersFixUpdate.Add(t);
					break;
				case TweenUpdateMode.Late:
					_runningPlayersLateUpdate.Add(t);
					break;
				default:
					_runningPlayers.Add(t);
					break;
			}
		}
		
		public void Stop(TweenPlayer t){
			
			switch (t.UpdateMode)
			{
				case TweenUpdateMode.Update:
					_runningPlayers.Remove(t);
					break;
				case TweenUpdateMode.Fixed:
					_runningPlayersFixUpdate.Remove(t);
					break;
				case TweenUpdateMode.Late:
					_runningPlayersLateUpdate.Remove(t);
					break;
				default:
					_runningPlayers.Remove(t);
					break;
			}
		}
		
		public void Kill(TweenPlayer t)
		{
			switch (t.UpdateMode)
			{
				case TweenUpdateMode.Update:
					_runningPlayers.Remove(t);
					break;
				case TweenUpdateMode.Fixed:
					_runningPlayersFixUpdate.Remove(t);
					break;
				case TweenUpdateMode.Late:
					_runningPlayersLateUpdate.Remove(t);
					break;
				default:
					_runningPlayers.Remove(t);
					break;
			}
			_players.Remove(t);
			_abortPlayers.Add(t);
		}

		public TweenPlayer Gen()
		{
			if (_abortPlayers.Count > 0)
			{
				var t = _abortPlayers[0];
				_players.Add(t);
				_abortPlayers.RemoveAt(0);
				return t;
			}
			else
			{
				var t = new TweenPlayer();
				return t;
			}
		}

		void Update()
		{
			if (_runningPlayers.Count>0)
			{
				for (int i = 0; i < _runningPlayers.Count; i++)
				{
					var t = _runningPlayers[i];
					t.Update();
					if(t.IsCompleted)
					{
						_runningPlayers.Remove(t);
						if(t.AutoKill)
							_abortPlayers.Add(t);
					}
				}
			}
		}
		
		void FixedUpdate()
		{
			if (_runningPlayersFixUpdate.Count>0)
			{
				for (int i = 0; i < _runningPlayersFixUpdate.Count; i++)
				{
					var t = _runningPlayersFixUpdate[i];
					t.Update();
					if(t.IsCompleted)
					{
						_runningPlayersFixUpdate.Remove(t);
						if(t.AutoKill)
							_abortPlayers.Add(t);
					}
				}
			}
		}
		
		void LateUpdate()
        {
        	if (_runningPlayersLateUpdate.Count>0)
        	{
	            for (int i = 0; i < _runningPlayersLateUpdate.Count; i++)
	            {
		            var t = _runningPlayersLateUpdate[i];
		            t.Update();
		            if(t.IsCompleted)
        			{
	                    _runningPlayersLateUpdate.Remove(t);
	                    if(t.AutoKill)
		                    _abortPlayers.Add(t);
        			}
        		}
        	}
        }
		
 
		#endregion

		#region Convenience Functions
		
		public void StopAll(){
			foreach (var t in _players)
			{
				t.Stop();
			}
		}
		
		public void KillAll()
		{
			StopAll();
			foreach (var t in _players)
			{
				t.Clear();
				_abortPlayers.Add(t);
			}
			_players.Clear();
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
