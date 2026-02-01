using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyHubPanel : MonoBehaviour
{
    [SerializeField] private Transform _panelRoot;
    [SerializeField] private PlayerLobbyItem _playerPrefab;
    [SerializeField] private TextMeshProUGUI _lobbyNamePanel;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _closeLobbyButton;

    private Lobby _lobby;
    private List<PlayerLobbyItem> _players;

    private void OnEnable()
    {
        _closeLobbyButton.onClick.AddListener(Close);
        LobbySession.Instance.OnLobbyUpdated += OnLobbyUpdated;
        LobbySession.Instance.OnLobbyClosed += OnLobbyClosed;
    }

    private void OnDisable()
    {
        _closeLobbyButton.onClick.RemoveListener(Close);
        LobbySession.Instance.OnLobbyUpdated -= OnLobbyUpdated;
        LobbySession.Instance.OnLobbyClosed -= OnLobbyClosed;
    }

    private void OnLobbyUpdated(Lobby lobby)
    {
        Debug.Log(lobby.Players.Count);
        UpdateLobby(lobby);
    }

    public void Open(Lobby lobby)
    {
        gameObject.SetActive(true);
        _lobby = lobby;
        _lobbyNamePanel.text = _lobby.Name;
        _players = new List<PlayerLobbyItem>();
        UpdateUI();
    }

    public async void Close()
    {
        await LobbySession.Instance.LeaveLobby();
        CLearPlayersItems();
        gameObject.SetActive(false);
    }

    private async void OnLobbyClosed()
    {
        await LobbySession.Instance.DeleteLobbyAsync();
        CLearPlayersItems();
        gameObject.SetActive(false);
    }

    private void UpdateLobby(Lobby lobby)
    {
        _lobby = lobby;
        _lobbyNamePanel.text = lobby.Name;
        UpdateUI();
    }

    private void UpdateUI()
    {
        CLearPlayersItems();

        foreach (var player in _lobby.Players)
            CreatePlayerItem(player);
    }

    private PlayerLobbyItem CreatePlayerItem(Player player)
    {
        PlayerLobbyItem item = Instantiate(_playerPrefab, _panelRoot);
        item.Init(player);
        _players.Add(item);
        return item;
    }

    private void CLearPlayersItems()
    {
        foreach (var player in _players)
        {
            Destroy(player.gameObject);
        }

        _players.Clear();   
    }
}
