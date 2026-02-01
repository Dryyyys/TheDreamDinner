using NUnit.Framework;
using System.Collections.Generic;
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

    private Lobby _lobby;
    private List<PlayerLobbyItem> _players;

    //private void OnEnable()
    //{
    //    LobbySession.Instance.OnLobbyUpdated += OnLobbyUpdated;
    //}

    //private void OnDisable()
    //{
    //    LobbySession.Instance.OnLobbyUpdated -= OnLobbyUpdated;
    //}

    //private void OnLobbyUpdated(Lobby lobby)
    //{
    //    if (!gameObject.activeSelf)
    //        Open(lobby);
    //    else
    //        UpdateLobby(lobby);
    //}

    public void Open(Lobby lobby)
    {
        _lobby = lobby;
        _lobbyNamePanel.text = _lobby.Name;
        _players = new List<PlayerLobbyItem>();
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void Close()
    {
        _lobby = null;
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
