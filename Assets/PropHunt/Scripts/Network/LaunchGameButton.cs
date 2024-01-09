using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LaunchGameButton : MonoBehaviour
{
    private const string GameSceneName = "Game";
    //[SerializeField] private GameManager _gameManager;

    private void Start()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsHost)
        {
            Button btn = GetComponent<Button>();
            btn.gameObject.SetActive(false);
        }
    }

    public void LaunchGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        GameManager.Instance.StartGame();
    }
}
