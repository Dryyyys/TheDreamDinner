using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListItem : MonoBehaviour
{
    public event Action<Lobby> OnJoined;

    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private TextMeshProUGUI _countPlayersText;
    [SerializeField] private Button _joinButton;
    public string LobbyId => _lobby?.Id;

    private Lobby _lobby;

    public void Init(string lobbyName, int currentPlayers, int maxPlayers, Lobby lobby)
    {
        _lobbyNameText.text = lobbyName;
        _countPlayersText.text = $"{currentPlayers}/{maxPlayers} игроков";
        _lobby = lobby;
        _joinButton.onClick.AddListener(OnJoinClicked);
    }

    private void OnDestroy()
    {
        _joinButton.onClick.RemoveListener(OnJoinClicked);
    }

    private async void OnJoinClicked()
    {
        Dictionary<string, string> data = new Dictionary<string, string>(); // как что кого будем передавать ?

        bool success = await JoinLobbyAsync(); // прокидывать события, если подключились или нет
        if (success)
        {
            OnJoined?.Invoke(_lobby);
        }

    }

    private async Task<bool> JoinLobbyAsync()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        return await LobbySession.Instance.JoinLobby(_lobby.Id, data);
    }
}
