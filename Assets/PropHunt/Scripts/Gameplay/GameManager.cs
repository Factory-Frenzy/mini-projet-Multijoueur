using System;
using Unity.Netcode;
using UnityEngine;
using DepthOfField = UnityEngine.Rendering.Universal.DepthOfField;

public class GameManager : NetworkBehaviour
{
    private NetworkVariable<GameEnum> _gameStatus = new();

    public static GameManager Instance;
    
    public const float gameDuration = 180f;
    
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
    
    public void StartGame()
    {
        if (!IsServer) return;
        _gameStatus.Value = GameEnum.IN_GAME;
        // todo: spawn props random
        // todo: spawn hunters at 0,0,0
        
        startTime = DateTime.Now;
        
        BlurHuntersCamera();
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
}