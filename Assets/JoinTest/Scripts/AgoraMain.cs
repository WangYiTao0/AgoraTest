using agora_gaming_rtc;
using agora_utilities;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace AgoraTest
{
    public class AgoraMain
    {
        private IRtcEngine _rtcEngine;
        private Text _messageText;
        private const float videoViewOffset = 100;

        public uint Uid;

        /// <summary>
        /// Engine 初期化
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="token"></param>
        public void InitEngine(string appID)
        {
            Debug.Log("InitializeEngine");
            if (_rtcEngine != null)
            {
                Debug.Log("Engine exists. Please unload it first!");
                return;
            }
            
            // init engine
            _rtcEngine = IRtcEngine.GetEngine(appID);
            
            //Enable Log
            _rtcEngine.SetLogFilter(LOG_FILTER.DEBUG | 
                                    LOG_FILTER.INFO | 
                                    LOG_FILTER.WARNING |
                                    LOG_FILTER.ERROR |
                                    LOG_FILTER.CRITICAL);
            _rtcEngine.SetLogFile("log.txt");
            
            //SetCallBack
            _rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccess;
            _rtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
            _rtcEngine.OnUserJoined += OnUserJoined;
       
            _rtcEngine.OnWarning += OnWarningHandler;

            _rtcEngine.OnError += OnError;
            _rtcEngine.OnConnectionLost += OnConnectionLostHandler;
            _rtcEngine.OnUserOffline += OnUserOffline;
        }
        
        /// <summary>
        /// JoinChannel
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="token"></param>
        public void JoinChannel(string channelName,string token = "")
        {
            Debug.Log($"calling join (channel = {channelName})");
            if (_rtcEngine == null)
                return;

            _rtcEngine.EnableAudio();
            _rtcEngine.EnableVideo();
            // allow camera output callback
            _rtcEngine.EnableVideoObserver();
            _rtcEngine.JoinChannelByKey(channelKey: token, channelName: channelName);
           // _rtcEngine.JoinChannel(channelName, null, 0);
            
     
        }
        /// <summary>
        /// For OutSide Control
        /// </summary>
        public void LeaveChannel()
        {
            Debug.Log("LeaveChannel");

            if (_rtcEngine == null)
                return;

            _rtcEngine.DisableAudio();
            _rtcEngine.DisableVideo();
            // leave channel
            _rtcEngine.LeaveChannel();
            // deregister video frame observers in native-c code
            _rtcEngine.DisableVideoObserver();
        }
        
        public string GetSdkVersion()
        {
            string ver = IRtcEngine.GetSdkVersion();
            return ver;
        }

        public void UnloadEngine()
        {
            Debug.Log("calling UnloadEngine");

            // delete
            if (_rtcEngine != null)
            {
                IRtcEngine.Destroy();  // Place this call in ApplicationQuit
                _rtcEngine = null;
            }
        }
        
        public void EnableVideo(bool pauseVideo)
        {
            if (_rtcEngine != null)
            {
                if (!pauseVideo)
                {
                    _rtcEngine.EnableVideo();
                }
                else
                {
                    _rtcEngine.DisableVideo();
                }
            }
        }

        public void MakeVideoView(uint uid)
        {
            GameObject go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            VideoSurface videoSurface = MakeImageSurface(uid.ToString());
            if (!ReferenceEquals(videoSurface, null))
            {
                // configure videoSurface
                videoSurface.SetForUser(uid);
                videoSurface.SetEnable(true);
                videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            }
        }

        private void DestroyVideoView(uint uid)
        {
            GameObject go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                Object.Destroy(go);
            }
        }
        
        //新建一个视频平面
        private VideoSurface MakeImageSurface(string goName)
        {
            GameObject go = new GameObject();

            if (go == null)
                return null;

            go.name = goName;

            // to be renderered onto
            go.AddComponent<RawImage>();

            // make the object draggable
            go.AddComponent<UIElementDragger>();
            GameObject canvas = GameObject.Find("VideoCanvas");
            if (canvas != null)
            {
                Debug.Log("add video view");
                go.transform.SetParent(canvas.transform);
            }
            else
            {
                Debug.Log("Canvas is null");
            }
            // set up transform
            go.transform.Rotate(0f, 0.0f, 180.0f);
            float xPos = Random.Range(videoViewOffset - Screen.width / 2f, Screen.width / 2f - videoViewOffset);
            float yPos = Random.Range(videoViewOffset, Screen.height / 2f - videoViewOffset);
            go.transform.localPosition = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(4f, 3f, 1f);

            // configure videoSurface
            VideoSurface videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelname"></param>
        /// <param name="uid"></param>
        /// <param name="elapsed"></param>
        private void OnJoinChannelSuccess(string channelname, uint uid, int elapsed)
        {
            Debug.Log("JoinChannelSuccessHandler: uid = " + uid);

            //MakeVideoView(0);
        }
        private void OnUserJoined(uint uid, int elapsed)
        {
            Debug.Log($"On User Joined with uid: {uid} elapsed: {elapsed}")
                ;
            // this is called in main thread
            //MakeVideoView(uid);
            Uid = uid;
        }
        
        private void OnLeaveChannelHandler(RtcStats stats)
        {
            Debug.Log($"Leave Channel Current user Count {stats.userCount}" );
            DestroyVideoView(0);
        }
        private void OnWarningHandler(int warn, string msg)
        {
            Debug.LogWarning($"Warning Code {warn}\n msg {msg} ");
        }

        private void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
        {
            Debug.Log($"OnUserOffLine uid: {uid}, reason: {reason}");
            DestroyVideoView(uid);
        }
        
        private void OnConnectionLostHandler()
        {
            Debug.Log("OnConnectionLost");
        }
        
        #region ErrorHandle
        
        private int _lastError { get; set; }
        private void OnError(int error, string msg)
        {
            if (error == _lastError)
            {
                return;
            }

            if (string.IsNullOrEmpty(msg))
            {
                msg = string.Format("Error code:{0} msg:{1}", error, IRtcEngine.GetErrorDescription(error));
            }

            switch (error)
            {
                case 101:
                    msg += "\nPlease make sure your AppId is valid and it does not require a certificate for this demo.";
                    break;
            }

            Debug.LogError(msg);
            if (_messageText != null)
            {
                if (_messageText.text.Length > 0)
                {
                    msg = "\n" + msg;
                }
                _messageText.text += msg;
            }

            _lastError = error;
        }

        #endregion
 

  
    }
}