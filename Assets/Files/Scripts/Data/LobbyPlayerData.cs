using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyPlayerData
{
    public string Id => _id;
    public string Nickname => _nickname;
    public bool IsReady
    {
        get => _isReady;
        set => _isReady = value;
    }

    private string _id;
    private string _nickname;
    private bool _isReady;

    public LobbyPlayerData(string id, string nickname)
    {
        _id = id;
        _nickname = nickname;
    }

    public LobbyPlayerData(Dictionary<string, PlayerDataObject> data)
    {
        UpdateState(data);
    }

    public void UpdateState(Dictionary<string, PlayerDataObject> data)
    {
        if (data.TryGetValue(LobbyPlayerKeys.Id, out var id))
            _id = id.Value;

        if (data.TryGetValue(LobbyPlayerKeys.Nickname, out var nick))
            _nickname = nick.Value;

        if (data.TryGetValue(LobbyPlayerKeys.IsReady, out var ready))
            bool.TryParse(ready.Value, out _isReady);
    }

    public Dictionary<string, string> Serialize()
    {
        return new()
        {
            { LobbyPlayerKeys.Id, _id },
            { LobbyPlayerKeys.Nickname, _nickname },
            { LobbyPlayerKeys.IsReady, _isReady.ToString() }
        };
    }
}
