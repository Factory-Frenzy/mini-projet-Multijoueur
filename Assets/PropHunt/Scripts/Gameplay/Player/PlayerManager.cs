using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : NetworkBehaviour
{
    protected MovementController _movementController;
    public Camera Camera;
    protected ClassController _currentController;
    public bool isHunter = true;

    public ActionInput _actionInput;
    public Animator _animator;
    [SerializeField] PropController _propController;
    [SerializeField] HunterController _hunterController;

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
        if (IsOwner)
        {
            GetComponent<PlayerInput>().enabled = true;
            GetComponent<AudioListener>().enabled = true;
            _movementController.enabled = true;
            Camera.gameObject.SetActive(true);
            _movementController.SetAnimator(GetComponent<Animator>());
            SwapTeam();
            return;
        }
        Camera.gameObject.SetActive(false);
    }

    /// <summary>
    /// Swap from hunter team to Prop team and from Prop team to Hunter team.
    /// Is not networked  for the moment...
    /// </summary>
    public void SwapTeam()
    {
        isHunter = !isHunter;
        if (isHunter)
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

    public void ToggleCursorLock()
    {
        bool isLocked = !_movementController.cursorLocked;
        Cursor.lockState = isLocked? CursorLockMode.Locked : CursorLockMode.None;
        _movementController.cursorLocked = isLocked;
    }
}
