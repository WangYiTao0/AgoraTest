using System.Collections;
using System.Collections.Generic;
using agora_gaming_rtc;
using UnityEngine;
using UnityEngine.UI;

public class AgoraChat : MonoBehaviour
{
    [SerializeField] private string _appID;
    [SerializeField] public string _channelName;

    VideoSurface _myView;
    VideoSurface _remoteView;
    IRtcEngine _rtcEngine;

    [SerializeField] private GameObject _myViewGO;
    [SerializeField] private GameObject _remoteViewGO;
    [SerializeField] private Button _joinBtn;
    [SerializeField] private Button _leaveBtn;
    
    void Awake()
    {
        SetupUI();
    }

    private void SetupUI()
    {
        _myView = _myViewGO.GetComponent<VideoSurface>();
        _remoteView = _remoteViewGO.gameObject.GetComponent<VideoSurface>();
        _remoteView.SetEnable(false);
        _myView.SetEnable(false);
        _joinBtn.onClick.AddListener(Join);
        _leaveBtn.onClick.AddListener(Leave);
    }

    void Start()
    {
         SetupAgora();
    }

    private void SetupAgora()
    {
        _rtcEngine = IRtcEngine.getEngine(_appID);
        _rtcEngine.OnUserJoined = OnUserJoined;
        _rtcEngine.OnUserOffline = OnUserOffline;
        _rtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccessHandler;
        _rtcEngine.OnLeaveChannel = OnLeaveChannelHandler;
    }

    void Join()
    {
        _rtcEngine.EnableVideo();
        _rtcEngine.EnableVideoObserver();
        _myView.SetEnable(true);
        _rtcEngine.JoinChannel(_channelName, "", 0);
    }

    void Leave()
    {
        _rtcEngine.LeaveChannel();
        _rtcEngine.DisableVideo();
        _rtcEngine.DisableVideoObserver();
    }

    /// <summary>
    /// When the local user joins the channel successfully,
    /// the agora engine will invoke this callback function.
    /// You should add more business logic here with respect to the local user.
    /// For this application, there isn’t much. So, we will just print a debug statement.
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="uid"></param>
    /// <param name="elapsed"></param>
    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        // can add other logics here, for now just print to the log
        Debug.LogFormat("Joined channel {0} successful, my uid = {1}", channelName, uid);
    }

    /// <summary>
    /// When the local user leaves the channel, you should clean up the user views.
    /// Calling the VideoSurface’s SetEnable(false) method will turn off the rendering.
    /// Otherwise, the last frame of the camera video will stay on the RawImage.
    /// </summary>
    /// <param name="stats"></param>
    void OnLeaveChannelHandler(RtcStats stats)
    {
        _myView.SetEnable(false);
        if (_remoteView != null)
        {
            _remoteView.SetEnable(false);
        }
    }

    void OnUserJoined(uint uid, int elapsed)
    {
        _remoteView.SetForUser(uid);
        _remoteView.SetEnable(true);
        _remoteView.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
        _remoteView.SetGameFps(30);
    }

    void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        
        _remoteView.SetEnable(false);
    }

    void OnApplicationQuit()
    {
        if (_rtcEngine != null)
        {
            IRtcEngine.Destroy(); 
            _rtcEngine = null;
        }
    }
}
