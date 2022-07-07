using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AgoraTest
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance;
        
        static AgoraMain app = null;

        private string WelcomeSceneName = "WelcomeScene";

        private string PlaySceneName = "VideoScene";
        
        [SerializeField]
        public string _appID = "your_appid";
        [SerializeField]
        private string _token = "your_token";
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }
        
      
        public void JoinChannel(string channelName)
        {
            if (ReferenceEquals(app, null))
            {
                app = new AgoraMain();
                app.InitEngine(_appID);
            }
            
            app.JoinChannel(channelName, _token);
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(PlaySceneName, LoadSceneMode.Single);
        }
        
        public void OnLeaveButtonClicked()
        {
            if (!ReferenceEquals(app, null))
            {
                app.LeaveChannel(); // leave channel
                app.UnloadEngine(); // delete engine
                app = null; // delete app
                SceneManager.LoadScene(WelcomeSceneName, LoadSceneMode.Single);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == PlaySceneName)
            {
                if (!ReferenceEquals(app, null))
                {
                    //TODO : Create New Video Surface
                    app.MakeVideoView(app.Uid);
                    GameObject textVersionGameObject = GameObject.Find("VersionText");
                    textVersionGameObject.GetComponent<TextMeshProUGUI>().text = "SDK Version : " + app.GetSdkVersion();
                }

                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!ReferenceEquals(app, null))
            {
                app.EnableVideo(pauseStatus);
            }
        }

        private void OnApplicationQuit()
        {
            if (!ReferenceEquals(app, null))
            {
                app.UnloadEngine();
            }
        }
    }
}