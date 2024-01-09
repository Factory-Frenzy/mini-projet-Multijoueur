using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    private NetworkVariable<GameEnum> _gameStatus = new();

    public static GameManager Instance;
    
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
        _gameStatus.Value = GameEnum.IN_GAME;
    }
}