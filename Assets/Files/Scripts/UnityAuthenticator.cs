using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class UnityAuthenticator : MonoBehaviour
{
    public async Task<bool> SignIn()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        bool signedIn = AuthenticationService.Instance.IsSignedIn;
        Debug.Log($"Signed in: {signedIn}");

        return signedIn;
    }
}
