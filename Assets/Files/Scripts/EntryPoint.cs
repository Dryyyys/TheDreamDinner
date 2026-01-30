using Unity.Netcode;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] UnityAuthenticator authenticator;

    private async void Start()
    {
        await authenticator.SignIn();
    }

    public async void StartHost()
    {
        //NetworkManager.Singleton.StartHost();
        bool succeeded = await LobbySession.Instance.CreateLobby(maxPlayers: 4, isPrivate: false, data: null); 

        if(succeeded)
        {
            Debug.Log(LobbySession.Instance.LobbyCode);
            // подгружаем сцену с лобби 
        }
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
