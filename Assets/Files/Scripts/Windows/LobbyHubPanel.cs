using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyHubPanel : MonoBehaviour
{
    public event Action PlayerDisconnected;

    [SerializeField] private Transform _panelRoot;
    [SerializeField] private PlayerLobbyItem _playerPrefab;
    [SerializeField] private TextMeshProUGUI _lobbyNamePanel;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _closeLobbyButton;

    private Lobby _lobby;
    private Dictionary<string, PlayerLobbyItem> _playerItems;

    private void OnEnable()
    {
        _startGameButton.onClick.AddListener(StartGame);
        _closeLobbyButton.onClick.AddListener(Close);
        GameLobby.Instance.LobbyUpdated += OnLobbyUpdated;
        GameLobby.Instance.LobbyClosed += OnLobbyClosed;
    }

    private void OnDisable()
    {
        _startGameButton.onClick.RemoveListener(StartGame);
        _closeLobbyButton.onClick.RemoveListener(Close);
        GameLobby.Instance.LobbyUpdated -= OnLobbyUpdated;
        GameLobby.Instance.LobbyClosed -= OnLobbyClosed;
    }

    private void OnLobbyUpdated(Lobby lobby)
    {
        UpdateLobby(lobby);
    }

    public void Open(Lobby lobby)
    {
        gameObject.SetActive(true);
        _lobby = lobby;
        _lobbyNamePanel.text = _lobby.Name;
        _playerItems = new Dictionary<string, PlayerLobbyItem>();

        if (GameLobby.Instance.IsPlayerHost == false)
            _startGameButton.gameObject.SetActive(false);
        else
            _startGameButton.interactable = false;
        
        UpdateUI();
    }

    public async void Close()
    {
        await LobbySession.Instance.LeaveLobby();
        ClearPlayersItems();
        PlayerDisconnected?.Invoke();
        gameObject.SetActive(false);
    }

    private async void OnLobbyClosed()
    {
        await LobbySession.Instance.DeleteLobbyAsync();
        ClearPlayersItems();
        PlayerDisconnected?.Invoke();
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
        _startGameButton.interactable = GameLobby.Instance.IsAllPlayersReady;
        var lobbyPlayers = GameLobby.Instance.LobbyPlayers;

        UpdatePlayersItems(lobbyPlayers);
    }

    private void UpdatePlayersItems(List<LobbyPlayerData> lobbyPlayers)
    {
        foreach (var player in lobbyPlayers)
        {
            if (_playerItems.TryGetValue(player.Id, out var item))
            {
                item.UpdateData(player);
            }
            else
            {
                var newItem = CreatePlayerItem(player);
                _playerItems.Add(player.Id, newItem);
            }
        }

        var existingIds = new HashSet<string>(_playerItems.Keys);

        foreach (var id in existingIds)
        {
            if (!lobbyPlayers.Exists(p => p.Id == id))
            {
                Destroy(_playerItems[id].gameObject);
                _playerItems.Remove(id);
            }
        }
    }

    private PlayerLobbyItem CreatePlayerItem(LobbyPlayerData player)
    {
        PlayerLobbyItem item = Instantiate(_playerPrefab, _panelRoot);
        item.Init(player);
        return item;
    }

    private void ClearPlayersItems()
    {
        foreach (var item in _playerItems.Values)
            Destroy(item.gameObject);

        _playerItems.Clear();
    }

    private async void StartGame()
    {
       await SceneManager.LoadSceneAsync("Game");
    }
}
