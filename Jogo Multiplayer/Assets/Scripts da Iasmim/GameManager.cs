using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Plane.Gameplay;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public GameObject Lose_UI;
    public GameObject Win_UI;

    // Lista dos aviões vivos (sincronizado entre todos os clientes)
    private List<NetworkObject> alivePlanes = new List<NetworkObject>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Espera todos os jogadores spawnarem e registra os aviões
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

    // Chamado por um PlayerPlane quando morre
    public void ReportDeath(NetworkObject deadPlane)
    {
       //if (!IsServer) return;

        if (alivePlanes.Contains(deadPlane))
            alivePlanes.Remove(deadPlane);

        // Notifica o cliente do avião morto para exibir a tela de Game Over
        SendGameOverClientRpc(deadPlane);

        // Se só sobrou um...
        if (alivePlanes.Count == 1)
        {
            NetworkObject winner = alivePlanes[0];
            SendVictoryClientRpc(alivePlanes[0]);
        }
    }

    
    public void SendGameOverClientRpc(NetworkObject deadPlane)
    {
        deadPlane.gameObject.GetComponent<PlayerPlane>().Lose_UI.SetActive(true);
            //if (Lose_UI != null)
            //clientId.Lose_UI.SetActive(true);

    }


    public void SendVictoryClientRpc(NetworkObject winner)
    {
        winner.gameObject.GetComponent<PlayerPlane>().Win_UI.SetActive(true);
        
        //if (NetworkManager.Singleton.LocalClientId == clientId)
        //{
            //if (Win_UI != null)
                //Win_UI.SetActive(true);
        //}
    }
}

