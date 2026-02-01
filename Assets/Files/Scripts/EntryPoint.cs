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

    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
