using Unity.Netcode;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] UnityAuthenticator authenticator;

    private async void Start()
    {
        await authenticator.SignIn();

    }
}
