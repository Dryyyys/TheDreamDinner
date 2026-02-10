using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private Toggle _playerStatus;

    private bool _isPending;
    private string _playerId;

    public void Init(LobbyPlayerData data)
    {
        _playerId = data.Id;

        _playerName.text = data.Nickname;

        _playerStatus.onValueChanged.RemoveAllListeners();
        _playerStatus.isOn = data.IsReady;

        if (_playerId == AuthenticationService.Instance.PlayerId)
        {
            _playerStatus.interactable = true;
            _playerStatus.onValueChanged.AddListener(OnPlayerStatusChanged);
        }
        else
        {
            _playerStatus.interactable = false;
        }
    }

    public void UpdateData(LobbyPlayerData data)
    {
        if (_isPending)
            return;

        _playerStatus.SetIsOnWithoutNotify(data.IsReady);
    }

    private async void OnPlayerStatusChanged(bool status)
    {
        _isPending = true;
        _playerStatus.interactable = false;

        bool success = await GameLobby.Instance.SetPlayerReadyStatus(status);

        _isPending = false;
        _playerStatus.interactable = true;

        _playerStatus.SetIsOnWithoutNotify(status);
    }
}
