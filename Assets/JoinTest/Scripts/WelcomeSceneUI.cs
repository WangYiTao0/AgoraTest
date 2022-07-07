using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AgoraTest
{
    public class WelcomeSceneUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _joinBtn;
        [SerializeField] private TextMeshProUGUI _appIDText;

        private void Start()
        {
            _joinBtn.onClick.AddListener(OnJoinButtonClicked);
            _appIDText.SetText($" AppID: {GameController.Instance._appID }");
    }

        private void OnJoinButtonClicked()
        {
            string channelName = _inputField.text;
            GameController.Instance.JoinChannel(channelName);
        }
    }
}