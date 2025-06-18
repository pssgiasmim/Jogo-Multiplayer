using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Plane.Gameplay
{
    public class PlayerPlane : NetworkBehaviour
    {
        public Vector2 m_Angle = Vector2.zero;
        public Transform m_Base;
        Vector2 m_TurnSpeed = Vector2.zero;
        public GameObject Lose_UI, Win_UI;

        public GameObject m_ExplodeParticle;



        public static PlayerPlane m_Main;

        private void Awake()
        {
            m_Main = this;
        }
        // Start is called before the first frame update
        void Start()
        {

        }



        // Update is called once per frame
        void Update()
        {
            if (!IsOwner) return;

            float InputX = 0, InputY = 0;
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                //Lose_UI.SetActive(true); 
                InputX = -1;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                InputX = 1;
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                InputY = 1;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                InputY = -1;
            }

            Vector3 movement = 40 * Time.deltaTime * new Vector3(InputX, InputY, 0);

            //m_TurnSpeed.x -= 2000 * Time.deltaTime * InputX;
            //m_TurnSpeed.y = 500 * Time.deltaTime * InputY;
            //m_TurnSpeed -= 8*Time.deltaTime * m_TurnSpeed;

            //m_Angle += 10*Time.deltaTime * m_TurnSpeed;
            //m_Angle.x -= 3*Time.deltaTime * m_Angle.x;
            //m_Angle.x= Mathf.Clamp(m_Angle.x, -85, 85);
            //m_Angle.y = Mathf.Clamp(m_Angle.y, -45, 45);

            m_Angle.x = Mathf.Lerp(m_Angle.x, 60.0f * InputX, 5 * Time.deltaTime);
            m_Angle.y = Mathf.Lerp(m_Angle.y, 20.0f * InputY, 5 * Time.deltaTime);

            m_Base.localRotation = Quaternion.Euler(-1f * m_Angle.y, 0, -m_Angle.x);

            //transform.rotation = Quaternion.Euler(0, -0.8f*Time.deltaTime * m_Angle.x, 0) * transform.rotation;
            //transform.position += 25*Time.deltaTime * (transform.forward+new Vector3(0, .005f*m_TurnSpeed.y, 0));
            transform.position += movement;

            Vector3 pos = transform.position;
            pos.y = Mathf.Clamp(pos.y, 8, 30);
            pos.x = Mathf.Clamp(pos.x, -18, 18);
            pos.z = 0;
            transform.position = pos;

            //GetComponent<AudioSource>().pitch = 1 + 0.004f*Mathf.Abs(m_Angle.x);

            Collider[] hits = Physics.OverlapSphere(transform.position, 2.5f);
            foreach (Collider hit in hits)
            {
                if (hit.gameObject == gameObject)
                    continue;


                if (m_ExplodeParticle != null)
                {
                    GameObject obj = Instantiate(m_ExplodeParticle);
                    obj.transform.position = transform.position;
                    HandleExplosionServerRpc(transform.position);
                }

                if (IsOwner)
                {
                    GameManager.Instance.ReportDeath(GetComponent<NetworkObject>());
                }
                gameObject.SetActive(false);

                //GameControl.m_Current.HandleGameOver();
                //gameObject.SetActive(false);
                break;

            }


        }

        [ServerRpc(RequireOwnership = false)]
        public void HandleExplosionServerRpc(Vector3 pos)
        {
            HandleExplosionClientRpc(pos);
          NetworkObject.Despawn(true); // Desativa para todos os jogadores
        }

        [ClientRpc]
        public void HandleExplosionClientRpc(Vector3 pos)
        {
            if (m_ExplodeParticle != null)
            {

                // SetActive(true);
                ShowLoseScreenClientRpc(this.NetworkObjectId);
                GameObject obj = Instantiate(m_ExplodeParticle, pos, Quaternion.identity);
            }
        }
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


        private void OnCollisionEnter(Collision collision)
        {
            
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                var camera = GetComponentInChildren<Camera>();
                if (camera != null)
                {
                    Destroy(camera.gameObject);
                }
                else
                {
                    Debug.LogWarning("Câmera não encontrada no avião do jogador não-dono.");
                }
            }
            else
            {
                if (!IsOwner)
                {
                    Destroy(this.GetComponentInChildren<Camera>().gameObject);
                }
                else
                {
                    if (IsServer)
                    {
                        if (NetworkManager.Singleton == null)
                        {
                            Debug.LogError("NetworkManager.Singleton está NULL!");
                            return;
                        }

                        var clientes = NetworkManager.Singleton.ConnectedClients;
                        if (clientes == null)
                        {
                            Debug.LogError("ConnectedClients está NULL!");
                            return;
                        }

                        int index = 0;
                        bool found = false;

                        foreach (var kvp in clientes)
                        {
                            if (kvp.Key == OwnerClientId)
                            {
                                found = true;
                                break;
                            }
                            index++;
                        }

                        if (!found)
                        {
                            Debug.LogWarning("OwnerClientId não encontrado. Usando índice 0 como fallback.");
                            index = 0;
                        }

                        Vector3[] posicoesIniciais = new Vector3[]
                        {
                            new Vector3(-15, 10, 0),
                            new Vector3(15, 10, 0),
                            new Vector3(-15, 25, 0),
                            new Vector3(15, 25, 0),
                            new Vector3(0, 30, 0)
                        };

                        Debug.Log($"Index: {index}, Total Posições: {posicoesIniciais.Length}");

                        if (index >= 0 && index < posicoesIniciais.Length)
                        {
                            transform.position = posicoesIniciais[index];
                        }
                        else
                        {
                            Debug.LogWarning($"Índice inválido: {index}. Posição padrão usada.");
                            transform.position = new Vector3(0, 15, 0);
                        }
                    }

                    var rb = GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                    }
                    else
                    {
                        Debug.LogWarning("Rigidbody está faltando no Player!");
                    }
                }
            }
                base.OnNetworkSpawn();
        }

        public void AlteraPos()
        {
            this.gameObject.transform.position = new Vector3(15, 5, 0);
            GetComponent<Rigidbody>().isKinematic = false;
        }

        public void ShowLoseUI()
        {
            if (Lose_UI != null)
                Lose_UI.SetActive(true);
        }

        public void ShowWinUI()
        {
            if (Win_UI != null)
                Win_UI.SetActive(true);
        }

        public static PlayerPlane GetLocalPlayerPlane()
        {
            foreach (var plane in FindObjectsOfType<PlayerPlane>())
            {
                if (plane.IsOwner)
                    return plane;
            }
            return null;
        }
    }
}

