using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LaunchGameButton : MonoBehaviour
{
    private const String GameSceneName = "Game";

    private void Start()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            Button btn = GetComponent<Button>();
            btn.enabled = false;
        }
    }

    public void LaunchGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
