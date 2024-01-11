using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : NetworkBehaviour
{
    protected MovementController _movementController;
    public Camera Camera;
    protected ClassController _currentController;
    public NetworkVariable<bool> isHunter = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> _life = new NetworkVariable<int>(10,NetworkVariableReadPermission.Everyone);
    private readonly object _lock = new object();

    public ActionInput _actionInput;
    public Animator _animator;
    [SerializeField] PropController _propController;
    [SerializeField] HunterController _hunterController;
    
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
                _life.Value = _life.Value + value;
                print("Life Hunter="+isHunter.Value+": "+_life.Value);
                /*print(NetworkManager.Singleton.ConnectedClientsIds.Count);
                print(NetworkManager.Singleton.IsHost);
                print(NetworkManager.Singleton.IsServer);
                print(NetworkManager.Singleton.IsClient);*/
                if (NetworkManager.Singleton.IsServer)
                {
                    if (_life.Value == 0)
                    {
                        this.GetComponent<NetworkObject>().Despawn();
                        print("ID client death = "+GetComponent<NetworkObject>().OwnerClientId);
                        bool win = GameManager.Instance.playerList.OneMoreDeath(GetComponent<NetworkObject>().OwnerClientId);
                        if (win) print("L'équipe gagnante est "+GameManager.Instance.playerList.TeamWin);
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
        if(_hunterController == null)
        {
            _hunterController = GetComponentInChildren<HunterController>();
            print(_hunterController);
        }
        if(_actionInput == null)
        {
            _actionInput = GetComponent<ActionInput>();
        }
        if (Camera == null) Camera = GetComponentInChildren<Camera>(true);
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //_hunterController.Deactivate();
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
        Cursor.lockState = isLocked? CursorLockMode.Locked : CursorLockMode.None;
        _movementController.cursorLocked = isLocked;
    }
}
