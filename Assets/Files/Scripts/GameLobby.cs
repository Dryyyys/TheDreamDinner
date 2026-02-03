using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameLobby : Singleton<GameLobby>
{
    public event Action<Lobby> LobbyUpdated;
    public event Action LobbyClosed;

    private List<LobbyPlayerData> _lobbyPlayerDatas = new();
    private LobbyPlayerData _localPlayerData;

    private void OnEnable()
    {
        LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        LobbyEvents.OnLobbyClosed += OnLobbyClosed;
    }

    private void OnLobbyClosed()
    {
        LobbyClosed?.Invoke();
    }

    private void OnDisable()
    {
        LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
        LobbyEvents.OnLobbyClosed -= OnLobbyClosed;
    }

    public async Task<bool> TryJoinLobby(Lobby lobby)
    {
        var playerData = LobbySession.Instance.GetCurrentPlayerData();
        bool success = await LobbySession.Instance.JoinLobby(lobby.Id, playerData);

        return success;
    }

    public async Task<bool> TryCreateLobby()
    {
        var playerData = LobbySession.Instance.GetCurrentPlayerData();
        bool success = await LobbySession.Instance.CreateLobby(maxPlayers: 4, isPrivate: false, data: playerData);

        return success;
    }

    public async Task<List<Lobby>> QueryAvailableLobbies()
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

    private void OnLobbyUpdated(Lobby lobby)
    {
        List<Dictionary<string, PlayerDataObject>> playerData = LobbySession.Instance.GetPlayersData();
        _lobbyPlayerDatas.Clear();

        foreach (var data in playerData)
        {
            LobbyPlayerData lobbyPlayerData = new LobbyPlayerData(data);

            if(lobbyPlayerData.Id == AuthenticationService.Instance.PlayerId)
                _localPlayerData = lobbyPlayerData;

            _lobbyPlayerDatas.Add(lobbyPlayerData);
        }

        LobbyUpdated?.Invoke(lobby);
    }

}
