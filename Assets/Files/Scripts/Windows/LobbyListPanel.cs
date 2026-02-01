using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListPanel : MonoBehaviour
{
    [SerializeField] private LobbyHubPanel _hubPanel;

    [SerializeField] private Transform _panelRoot;
    [SerializeField] private LobbyListItem _itemPrefab;

    [SerializeField] private Button _refreshLobbiesButton;
    [SerializeField] private Button _createLobbyButton;

    private List<LobbyListItem> _lobbies = new List<LobbyListItem>();

    private void OnEnable()
    {
        _refreshLobbiesButton.onClick.AddListener(RefreshLobbies);
        _createLobbyButton.onClick.AddListener(CreateLobby);
    }

    private void OnDisable()
    {
        _createLobbyButton.onClick.RemoveListener(CreateLobby);
        _refreshLobbiesButton.onClick.RemoveListener(RefreshLobbies);
    }

    private async Task<List<Lobby>> QueryAvailableLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 10 // что будем делать с количеством ? ћб ещЄ какие-то параметры ?
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
            return response.Results;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to query lobbies: {e.Message}");
            return new List<Lobby>();
        }
    }

    private LobbyListItem CreateLobbySlot(Lobby gameLobby)
    {
        LobbyListItem item = Instantiate(_itemPrefab, _panelRoot);
        item.OnJoinedClickedAction += JoinLobby;
        item.Init(gameLobby.Name, gameLobby.Players.Count, gameLobby.MaxPlayers, gameLobby);
        return item;
    }

    public async void CreateLobby()
    {
        var playerData = GetPlayerData();

        bool succeeded = await LobbySession.Instance.CreateLobby(maxPlayers: 4, isPrivate: false, data: playerData);

        if (succeeded)
        {
            _hubPanel.Open(LobbySession.Instance.CurrentLobby);
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        var playerData = GetPlayerData();
        bool success = await JoinLobbyAsync(lobby, playerData); // прокидывать событи€, если подключились или нет
        if (success)
        {
            _hubPanel.Open(lobby);
        }
    }

    private async Task<bool> JoinLobbyAsync(Lobby lobby, Dictionary<string, string> data)
    {
        return await LobbySession.Instance.JoinLobby(lobby.Id, data);
    }

    public async void RefreshLobbies()
    {
        ClearLobbiesList();
        List<Lobby> lobbies = await QueryAvailableLobbies();
        Debug.Log($"Was founded {lobbies.Count} lobbies");
        foreach (Lobby lobby in lobbies)
        {
            LobbyListItem item = CreateLobbySlot(lobby);
            _lobbies.Add(item);
        }
    }

    private void ClearLobbiesList()
    {
        foreach (var item in _lobbies)
        {
            item.OnJoinedClickedAction -= JoinLobby;
            Destroy(item.gameObject);
        }

        _lobbies.Clear(); 
    }

    private Dictionary<string, string> GetPlayerData()
    {
        var data = new Dictionary<string, string>
        {
            { "nickname", AuthenticationService.Instance.PlayerName }
        };

        return data;
    }
}
