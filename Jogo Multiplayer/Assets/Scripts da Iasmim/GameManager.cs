using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Plane.Gameplay;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    private List<NetworkObject> alivePlanes = new List<NetworkObject>();

    private NetworkVariable<bool> isGameOver = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Invoke(nameof(RegisterAllPlanes), 1f);
        }
    }

    private void RegisterAllPlanes()
    {
        alivePlanes.Clear();

        PlayerPlane[] players = FindObjectsOfType<PlayerPlane>();
        foreach (var plane in players)
        {
            alivePlanes.Add(plane.GetComponent<NetworkObject>());
        }
    }

    public void ReportDeath(NetworkObject deadPlane)
    {
        if (!IsServer || isGameOver.Value)
            return;

        if (alivePlanes.Contains(deadPlane))
        {
            alivePlanes.Remove(deadPlane);
        }

        // Mostra tela de derrota apenas para o jogador que morreu
        ShowResultClientRpc(deadPlane.OwnerClientId, false);

        // Se sobrou só um jogador, ele venceu
        if (alivePlanes.Count == 1)
        {
            var winner = alivePlanes[0];
            isGameOver.Value = true;

            ShowResultClientRpc(winner.OwnerClientId, true);
        }
    }

    [ClientRpc]
    private void ShowResultClientRpc(ulong clientId, bool isWinner)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            if (isWinner)
            {
                if (Instance != null && Instance.Win_UI != null)
                    Instance.Win_UI.SetActive(true);
            }
            else
            {
                if (Instance != null && Instance.Lose_UI != null)
                    Instance.Lose_UI.SetActive(true);
            }
        }
    }

    [SerializeField] public GameObject Win_UI;
    [SerializeField] public GameObject Lose_UI;
}
