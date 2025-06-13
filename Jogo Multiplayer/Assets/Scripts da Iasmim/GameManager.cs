using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Plane.Gameplay;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public GameObject Lose_UI;
    public GameObject Win_UI;

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
        SendGameOverClientRpc(deadPlane.OwnerClientId);

        // Se só sobrou 1 avião vivo
        if (alivePlanes.Count == 1)
        {
            var winner = alivePlanes[0];
            SendVictoryClientRpc(winner.OwnerClientId);
        }
    }

    // RPC: Mostra Game Over para o jogador que morreu
    [ClientRpc]
    void SendGameOverClientRpc(ulong deadClientId, ClientRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClientId == deadClientId)
        {
            if (Lose_UI != null)
                Lose_UI.SetActive(true);
        }
    }

    // RPC: Mostra Vitória para o último jogador
    [ClientRpc]
    void SendVictoryClientRpc(ulong winnerClientId, ClientRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClientId == winnerClientId)
        {
            if (Win_UI != null)
                Win_UI.SetActive(true);
        }
    }
}