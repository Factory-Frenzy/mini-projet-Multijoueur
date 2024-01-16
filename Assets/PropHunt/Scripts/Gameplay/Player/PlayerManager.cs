using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{
    protected MovementController _movementController;
    protected ClassController _currentController;

    public Camera Camera;
    [NonSerialized] public NetworkVariable<bool> isHunter = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> _life = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone);
    public ActionInput _actionInput;
    public Animator _animator;

    [SerializeField] private PropController _propController;
    [SerializeField] private HunterController _hunterController;
    private readonly object _lock = new object();

    public ulong NetworkClientId
    {
        get { return NetworkManager.Singleton.LocalClientId; }
    }
    public int Life
    {
        get
        {
            return _life.Value;
        }
        set
        {
            lock (_lock)
            {
                _life.Value += value;

                if (NetworkManager.Singleton.IsServer)
                {
                    var networkObject = this.GetComponent<NetworkObject>();
                    GameManager.Instance.playerList.GetClientInfo(networkObject.OwnerClientId).Score -= 50;

                    if (_life.Value == 0)
                    {
                        GameManager.Instance.playerList.GetClientInfo(networkObject.OwnerClientId).IsAlive = false;
                        networkObject.Despawn();
                        if (GameManager.Instance.playerList.OneMoreDeath(networkObject.OwnerClientId))
                        {
                            print("FIN DU JEU");
                            var clientInfos = JsonConvert.SerializeObject(GameManager.Instance.playerList.clientInfos);
                            GameManager.Instance.SendClientsInfosClientRpc(clientInfos);
                            foreach (var sNetworkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
                            {
                                print(sNetworkObject.name);
                                if (sNetworkObject.name != "GameManager" && sNetworkObject.IsSpawned)
                                {
                                    sNetworkObject.Despawn();
                                }
                            }
                            NetworkManager.Singleton.SceneManager.LoadScene("EndGame", LoadSceneMode.Single);
                        }
                    }
                }

            }
        }
    }

    private void Awake()
    {
        _movementController = GetComponent<MovementController>();
        if (_propController == null)
        {
            _propController = GetComponentInChildren<PropController>();
        }
        if (_hunterController == null)
        {
            _hunterController = GetComponentInChildren<HunterController>();
            print(_hunterController);
        }
        if (_actionInput == null)
        {
            _actionInput = GetComponent<ActionInput>();
        }
        if (Camera == null) Camera = GetComponentInChildren<Camera>(true);
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SwapTeam();
        isHunter.OnValueChanged += (@previousValue, @newValue) => SwapTeam();
        if (IsOwner)
        {
            GetComponent<PlayerInput>().enabled = true;
            GetComponent<AudioListener>().enabled = true;
            _movementController.enabled = true;
            Camera.gameObject.SetActive(true);
            _movementController.SetAnimator(GetComponent<Animator>());
            return;
        }
        //isHunter.Value = !isHunter.Value;
        //isHunter.OnValueChanged += SwapTeamNetwork;
        Camera.gameObject.SetActive(false);
    }

    /// <summary>
    /// Swap from hunter team to Prop team and from Prop team to Hunter team.
    /// Is not networked  for the moment...
    /// </summary>
    public void SwapTeam()
    {
        //isHunter.Value = !isHunter.Value;
        if (isHunter.Value)
        {
            _movementController.ClassController = _hunterController;
            _actionInput.SetClassInput(_hunterController.ClassInput);
            _propController.Deactivate();
            _hunterController.Activate();
            return;
        }
        _movementController.ClassController = _propController;
        _actionInput.SetClassInput(_propController.ClassInput);
        _hunterController.Deactivate();
        _propController.Activate();
    }

    public void OnSwapTeam()
    {
        isHunter.Value = !isHunter.Value;
    }

    public void ToggleCursorLock()
    {
        bool isLocked = !_movementController.cursorLocked;
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        _movementController.cursorLocked = isLocked;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsOwner)
        {
            SceneManager.LoadScene("EndGame");
        }
    }
}
