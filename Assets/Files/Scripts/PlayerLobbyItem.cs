using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyItem : MonoBehaviour
{
    public event Action<Player> OnPlayerReady;
    public event Action<Player> OnPlayerNotReady;

    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private Toggle _playerStatus;

    private Player _player;

    public void Init(Player player)
    {
        _playerName.text = player.Profile.Name;
        _playerStatus.onValueChanged.AddListener(OnPlayerStatusChanged);
    }

    private void OnPlayerStatusChanged(bool status)
    {
        if (status)
            OnPlayerReady?.Invoke(_player);
        else
            OnPlayerNotReady?.Invoke(_player);

        Debug.Log("hjsdfsd");
    }

    private void OnDestroy()
    {
        _playerStatus.onValueChanged.RemoveListener(OnPlayerStatusChanged);
    }
}
