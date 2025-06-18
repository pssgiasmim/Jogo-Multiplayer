using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Plane.Gameplay;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    //public GameObject Lose_UI;
    //public GameObject Win_UI;

    // Lista dos aviões vivos
    private List<NetworkObject> alivePlanes = new List<NetworkObject>();

    
    

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Espera os jogadores spawnarem
            Invoke("RegisterAllPlanes", 1f);
        }
    }

    void RegisterAllPlanes()
    {
        PlayerPlane[] planes = FindObjectsOfType<PlayerPlane>();
        foreach (var plane in planes)
        {
            alivePlanes.Add(plane.GetComponent<NetworkObject>());
        }

       

    }

    

    // Chamado por um PlayerPlane ao morrer
    public void ReportDeath(NetworkObject deadPlane)
    {
        if (!IsServer) return;

        if (alivePlanes.Contains(deadPlane))
            alivePlanes.Remove(deadPlane);

        // Envia só para o cliente que morreu
        ShowLoseScreenClientRpc(deadPlane.OwnerClientId);

        // Se só sobrou 1 avião vivo
        if (alivePlanes.Count == 1)
        {
            var winner = alivePlanes[0];
            ShowWinScreenClientRpc(winner.OwnerClientId);
        }
    }

    // RPC: Mostra Game Over para o jogador que morreu
    [ClientRpc]
    void ShowLoseScreenClientRpc(ulong deadClientId, ClientRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClientId == deadClientId)
        {
            var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<PlayerPlane>();
            if (player != null && player.Lose_UI != null) 
            {
                player.Lose_UI.SetActive(true);
            }
                
        }
    }

    // RPC: Mostra Vitória para o último jogador
    [ClientRpc]
    void ShowWinScreenClientRpc(ulong winnerClientId, ClientRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClientId == winnerClientId)
        {
            var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<PlayerPlane>();
            if (player != null && player.Win_UI != null)
                player.Win_UI.SetActive(true);
        }
    }
}