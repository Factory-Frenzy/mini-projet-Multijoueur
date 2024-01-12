using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.Build.Pipeline;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using DepthOfField = UnityEngine.Rendering.Universal.DepthOfField;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    private NetworkVariable<GameEnum> _gameStatus = new();

    public static GameManager Instance;
    
    // Liste des points d'apparition préréglés
    public List<Transform> spawnPoints;
    
    public const float gameDuration = 180f;
    public PlayerList playerList;
    public float hunterBlurDuration = 10f;
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
    }

    
    public void StartGame()
    {
        if (!IsServer) return;

        playerList.InitPlayerList();
        StartCoroutine(SpawnPlayersWithDelay());
        StartCoroutine(GivingSurvivalPoints());
    }

    private IEnumerator GivingSurvivalPoints()
    {
        yield return new WaitForSeconds(15);
        foreach (var item in playerList.ScorePlayers.Value)
        {
            if (item.IsAlive)
            {
                item.Score += 10;
            }
        }
        if (GameManager.Instance.playerList.TeamWin.Value == Team.NOBODY)
        {
            StartCoroutine(GivingSurvivalPoints());
        }
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
            // todo: display scene finish with scoreboard
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
}