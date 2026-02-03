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

    private List<LobbyListItem> _lobbiesItems = new List<LobbyListItem>();

    private void OnEnable()
    {
        _refreshLobbiesButton.onClick.AddListener(RefreshLobbies);
        _createLobbyButton.onClick.AddListener(CreateLobby);
        _hubPanel.PlayerDisconnected += RefreshLobbies;
    }

    private void OnDisable()
    {
        _createLobbyButton.onClick.RemoveListener(CreateLobby);
        _refreshLobbiesButton.onClick.RemoveListener(RefreshLobbies);
        _hubPanel.PlayerDisconnected -= RefreshLobbies;
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
        if (await GameLobby.Instance.TryCreateLobby())
            _hubPanel.Open(LobbySession.Instance.CurrentLobby);
    }

    public async void JoinLobby(Lobby lobby)
    {
        if (await GameLobby.Instance.TryJoinLobby(lobby))
            _hubPanel.Open(lobby);
    }

    public async void RefreshLobbies()
    {
        ClearLobbiesList();
        List<Lobby> lobbies = await GameLobby.Instance.QueryAvailableLobbies();

        foreach (Lobby lobby in lobbies)
        {
            LobbyListItem item = CreateLobbySlot(lobby);
            _lobbiesItems.Add(item);
        }
    }

    private void ClearLobbiesList()
    {
        foreach (var item in _lobbiesItems)
        {
            item.OnJoinedClickedAction -= JoinLobby;
            Destroy(item.gameObject);
        }

        _lobbiesItems.Clear(); 
    }

}
