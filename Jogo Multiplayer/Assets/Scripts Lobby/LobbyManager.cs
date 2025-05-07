using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using TMPro;
using Unity.VisualScripting;

public class LobbyManager : MonoBehaviour
{
    Lobby hostLobby, joinedLobby;
    public TextMeshProUGUI textoCodigo;
    public TMP_InputField inputFieldtextoInserido;

    // Start is called before the first frame update
    async void Start()
    {
        await UnityServices.InitializeAsync();
        await LogaAnonimamente();
    }

    async Task LogaAnonimamente()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            return;
        }

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Login: " + AuthenticationService.Instance.PlayerId);
    }

    public async Task CriarSala()
    {
        hostLobby = await LobbyService.Instance.CreateLobbyAsync("Nome", 50);
        joinedLobby = hostLobby;
        textoCodigo.text = hostLobby.LobbyCode;
        //InvokeReapeting chama a função a cada segundo;
        InvokeRepeating("HeartBeat", 10, 10);
    }

    public async void BotaoCriar()
    {
        await CriarSala();
    }

    public async void BotaoEntrar()
    {
        await EntrarSala();
    }

    public async Task EntrarSala()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(inputFieldtextoInserido.text);
        }
    }

    public async void HeartBeat()
    {
        if (hostLobby != null)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            Debug.Log("Enviou Ping");
        }
    }


    
}
