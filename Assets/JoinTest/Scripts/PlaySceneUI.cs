using System;
using UnityEngine;
using UnityEngine.UI;

namespace AgoraTest
{
    public class PlaySceneUI : MonoBehaviour
    {
        [SerializeField] private Button _leaveBtn;

        private void Start()
        {
            _leaveBtn.onClick.AddListener(OnLeaveBtnClick);
        }

        private void OnLeaveBtnClick()
        {
            GameController.Instance.OnLeaveButtonClicked();
        }
    }
}