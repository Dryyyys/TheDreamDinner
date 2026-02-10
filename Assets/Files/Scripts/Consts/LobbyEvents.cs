using System;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyEvents
{
    public static event Action<Lobby> OnLobbyUpdated;
    public static event Action OnLobbyClosed;

    public static void RaiseLobbyUpdated(Lobby lobby) => OnLobbyUpdated?.Invoke(lobby);
    public static void RaiseLobbyClosed() => OnLobbyClosed?.Invoke();

}
