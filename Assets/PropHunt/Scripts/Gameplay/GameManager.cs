using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using DepthOfField = UnityEngine.Rendering.Universal.DepthOfField;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public List<Transform> spawnPoints;
    public const float gameDuration = 180f;
    public float hunterBlurDuration = 10f;
    public NetworkVariable<FixedString64Bytes> TeamWin = new NetworkVariable<FixedString64Bytes>(Team.NOBODY);
    public PlayerList playerList = null;

    private NetworkVariable<GameEnum> _gameStatus = new();
    private bool hunterBlurEnabled = false;
    private DateTime startTime;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public GameEnum GetStatus()
    {
        return _gameStatus.Value;
    }
    
    IEnumerator SpawnPlayersWithDelay()
    {
        yield return new WaitForSeconds(1);
        
        _gameStatus.Value = GameEnum.IN_GAME;
        startTime = DateTime.Now;
        
        BlurHuntersCamera();
        SpawnPlayersRandomly();
        StartCoroutine(GivingSurvivalPoints());
    }

    
    public void StartGame()
    {
        if (!IsServer) return;

        playerList = new PlayerList();
        StartCoroutine(SpawnPlayersWithDelay());
    }

    void Update()
    {
        if (!IsHost) return;
        
        if (hunterBlurEnabled)
        {
            // Stop hunters blur after 10s
            if (DateTime.Now > startTime.AddSeconds(hunterBlurDuration))
            {
                hunterBlurEnabled = false;
                RpcDisableBlurClientRpc();
            }
        }

        // Stop game after 3min
        if (_gameStatus.Value == GameEnum.IN_GAME && DateTime.Now > startTime.AddSeconds(gameDuration))
        {
            _gameStatus.Value = GameEnum.FINISH;
            TeamWin.Value = Team.PROB;
            print("Fin du jeu sur time out");
        }
    }
    
    private void BlurHuntersCamera()
    {
        RpcEnableBlurClientRpc();
    } 
    
    private DepthOfField depthOfField;
    
    [ClientRpc]
    void RpcEnableBlurClientRpc()
    {
        Camera playerCamera = FindObjectOfType<Camera>();
        if (depthOfField != null)
        {
            playerCamera.enabled = false;
            hunterBlurEnabled = true;
        }
    }

    [ClientRpc]
    void RpcDisableBlurClientRpc()
    {
        Camera playerCamera = FindObjectOfType<Camera>();
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            hunterBlurEnabled = false;
        }
    }
    
    
    private void SpawnPlayersRandomly()
    {
        // Assurez-vous que la liste des points d'apparition est suffisamment grande pour accueillir tous les joueurs
        if (spawnPoints.Count < NetworkManager.Singleton.ConnectedClientsList.Count)
        {
            Debug.LogError("Pas assez de points d'apparition pour tous les joueurs !");
            return;
        }

        // Mélange la liste des points d'apparition
        ShuffleSpawnPoints();

        // Distribue les joueurs parmi les points d'apparition
        int index = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                Transform spawnPoint = spawnPoints[index];
                client.PlayerObject.transform.position = spawnPoint.position;

                index++;

                // Assurez-vous de ne pas dépasser la taille de la liste des points d'apparition
                if (index >= spawnPoints.Count)
                {
                    Debug.LogWarning("Tous les points d'apparition ont été utilisés. Certains joueurs peuvent apparaître au même endroit.");
                    break;
                }
            }
        }
    }

    void ShuffleSpawnPoints()
    {
        // Mélange la liste des points d'apparition en utilisant l'algorithme de Fisher-Yates
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            int randomIndex = Random.Range(i, spawnPoints.Count);
            Transform temp = spawnPoints[i];
            spawnPoints[i] = spawnPoints[randomIndex];
            spawnPoints[randomIndex] = temp;
        }
    }

    private IEnumerator GivingSurvivalPoints()
    {
        yield return new WaitForSeconds(15);
        foreach (var item in playerList.clientInfos)
        {
            if (item.IsAlive)
            {
                item.Score += 10;
                print("Le Client:"+item.ClientId+" vient de gagner +10 point. Total = "+item.Score);
            }
        }
        if (TeamWin.Value == Team.NOBODY)
        {
            StartCoroutine(GivingSurvivalPoints());
        }
    }
}

public class PlayerList
{
    private int NbHunter = 0;
    private int NbProp = 0;

    public List<ClientInfo> clientInfos;
    public PlayerList()
    {
        clientInfos = new List<ClientInfo>();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject.GetComponent<PlayerManager>().isHunter.Value)
            {
                NbHunter++;
            }
            else
            {
                NbProp++;
            }
            clientInfos.Add(new ClientInfo(client.ClientId));
        }
    }

    public bool OneMoreDeath(ulong deathId)
    {
        var death = NetworkManager.Singleton.ConnectedClients[deathId];
        if (death == null) throw new InvalidOperationException("Message d'erreur OneMoreDeath");

        if (death.PlayerObject.GetComponent<PlayerManager>().isHunter.Value)
        {
            NbHunter--;
        }
        else
        {
            NbProp--;
        }

        return TeamWinCheck();
    }

    private bool TeamWinCheck()
    {
        if (NbHunter == 0)
        {
            Debug.Log("team win : prob");
            GameManager.Instance.TeamWin.Value = Team.PROB;
            return true;
        }
        else if (NbProp == 0)
        {
            Debug.Log("team win : hunter");
            GameManager.Instance.TeamWin.Value = Team.HUNTER;
            return true;
        }
        else
        {
            return false;
        }
    }

    public ClientInfo GetClientInfo(ulong clientId)
    {
        foreach (var clientInfo in clientInfos)
        {
            if (clientInfo.ClientId == clientId)
            {
                return clientInfo;
            }
        }
        return null;
    }
}

public struct Team
{
    public const string HUNTER = "Hunter";
    public const string PROB = "Prob";
    public const string NOBODY = "";
}

public class ClientInfo : INetworkSerializable
{
    public ulong ClientId;
    public bool IsAlive;
    public int Score;
    public ClientInfo(ulong ClientId)
    {
        this.ClientId = ClientId;
        this.IsAlive = true;
        this.Score = 0;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref IsAlive);
        serializer.SerializeValue(ref Score);
    }
}