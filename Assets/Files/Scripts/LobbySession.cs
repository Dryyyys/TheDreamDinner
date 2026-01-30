
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
    public event Action<Lobby> OnLobbyUpdated;
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

            Debug.Log($"Created lobby with ID: {_lobby.Id}");
        }
        catch(System.Exception exception)
        {
            Debug.LogError($"Failed attempt to create lobby: {exception.Message}");
            return false;
        }

        return true;
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

            if (task.IsCompletedSuccessfully)
            {
                _lobby = task.Result;
                OnLobbyUpdated?.Invoke(_lobby); 
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
        if(data == null) return null;

        Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();

        foreach(KeyValuePair<string, string> kvp in data)
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

        if (_lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId)
            await LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);

        _lobby = null;
    }
}