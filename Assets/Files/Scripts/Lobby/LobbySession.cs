
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbySession : Singleton<LobbySession>
{
    public Lobby CurrentLobby => _lobby;

    public string LobbyCode => _lobby?.LobbyCode;

    private Lobby _lobby;
    private Coroutine _heartbeatCoroutine;
    private Coroutine _refreshCoroutine;

    private bool _isLobbyAlive;

    private float _heartbeatInterval = 30f;  
    private float _refreshInterval = 1f;

    public async Task<bool> CreateLobby(int maxPlayers, bool isPrivate, Dictionary<string, string> data)
    {
        Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);
        Player player = new Player(AuthenticationService.Instance.PlayerId, null, playerData);
        CreateLobbyOptions options = new CreateLobbyOptions()
        {
            IsPrivate = isPrivate,
            Player = player
        };

        try
        {
            _lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", maxPlayers, options); // передавать название лобби ?\

            _isLobbyAlive = true;
            _heartbeatCoroutine = StartCoroutine(HeartbeatLobbyCoroutine(_lobby.Id, _heartbeatInterval));
            _refreshCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, _refreshInterval));
            LobbyEvents.RaiseLobbyUpdated(_lobby);
            Debug.Log($"Created lobby with ID: {_lobby.Id}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed attempt to create lobby: {exception.Message}");
            return false;
        }

        return true;
    }

    public async Task<bool> JoinLobby(string lobbyId, Dictionary<string, string> data)
    {
        Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);
        Player player = new Player(AuthenticationService.Instance.PlayerId, null, playerData);
        JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions()
        {
            Player = player
        };

        try
        {
            _lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);
            Debug.Log($"Player with ID: {player.Id} joined the lobby {_lobby.Id}");
            _isLobbyAlive = true;
            _refreshCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, _refreshInterval));
            LobbyEvents.RaiseLobbyUpdated(_lobby);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed attempt to join lobby: {exception.Message}");
            return false;
        }
    }

    public async Task LeaveLobby()
    {
        if (_lobby == null)
            return;

        bool isHost = _lobby.HostId == AuthenticationService.Instance.PlayerId;

        if (!isHost)
            await LobbyService.Instance.RemovePlayerAsync(_lobby.Id, AuthenticationService.Instance.PlayerId);

        await DeleteLobbyAsync();
    }

    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float interval)
    {
        while (_isLobbyAlive)
        {
            var task = LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);

            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
            {
                _isLobbyAlive = false;
                Debug.LogError("Heartbeat failed");
                yield break;
            }

            yield return new WaitForSecondsRealtime(interval);
        }
    }

    private IEnumerator RefreshLobbyCoroutine(string lobbyId, float interval)
    {
        while (_isLobbyAlive)
        {
            var task = LobbyService.Instance.GetLobbyAsync(lobbyId);

            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted) 
            {
                Debug.Log("Lobby no longer exists");
                _isLobbyAlive = false;
                LobbyEvents.RaiseLobbyClosed();
                yield break;
            }

            if (task.IsCompletedSuccessfully)
            {
                Lobby newLobby = task.Result;
                _lobby = newLobby;
                LobbyEvents.RaiseLobbyUpdated(_lobby);
            }
            else
            {
                Debug.LogError("Failed to refresh lobby");
            }

            yield return new WaitForSecondsRealtime(interval);
        }
    }

    private Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
    {
        if (data == null) return null;

        Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();

        foreach (KeyValuePair<string, string> kvp in data)
        {
            PlayerDataObject dataObject = new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Member, value: kvp.Value);
            playerData.Add(kvp.Key, dataObject);
        }

        return playerData;
    }

    public async Task DeleteLobbyAsync()
    {
        _isLobbyAlive = false;

        if (_heartbeatCoroutine != null)
            StopCoroutine(_heartbeatCoroutine);

        if (_refreshCoroutine != null)
            StopCoroutine(_refreshCoroutine);

        string lobbyId = _lobby.Id;

        if (_lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            await LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
            Debug.Log($"Lobby with ID: {lobbyId} was deleted");
        }

        //_lastLobbySnapshot = null;
        _lobby = null;
    }

    public List<Dictionary<string, PlayerDataObject>> GetPlayersData()
    {
        List<Dictionary<string, PlayerDataObject>> data = new List<Dictionary<string, PlayerDataObject>>();

        foreach(Player player in _lobby.Players)
            data.Add(player.Data);
        
        return data;
    }

    public Dictionary<string, string> GetCurrentPlayerData()
    {
        string id = AuthenticationService.Instance.PlayerId;
        string name = AuthenticationService.Instance.PlayerName;

        LobbyPlayerData lobbyPlayerData = new LobbyPlayerData(id, name);

        return lobbyPlayerData.GetData();
    }

    public async Task<bool> UpdatePlayerData(string playerId, Dictionary<string, string> data)
    {
        Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);

        UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions()
        {
            Data = playerData
        };

        try
        {
            _lobby = await LobbyService.Instance.UpdatePlayerAsync(_lobby.Id, playerId, updatePlayerOptions);
            LobbyEvents.RaiseLobbyUpdated(_lobby);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed attempt to update player data (player id = {playerId}) : {e.Message}");
            return false;
        }
    }
}