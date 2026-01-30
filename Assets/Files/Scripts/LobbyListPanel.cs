using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListPanel : MonoBehaviour
{
    [SerializeField] private GameObject _listPanel;
    [SerializeField] private LobbyListItem _itemPrefab;

    [SerializeField] private Button _refreshLobbiesButton;

    private List<LobbyListItem> _lobbies = new List<LobbyListItem>();

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
        LobbyListItem item = Instantiate(_itemPrefab, _listPanel.transform);
        item.Init(gameLobby.Name, gameLobby.Players.Count, gameLobby.MaxPlayers, gameLobby);
        return item;
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
            Destroy(item.gameObject);

        _lobbies.Clear(); 
    }
}
