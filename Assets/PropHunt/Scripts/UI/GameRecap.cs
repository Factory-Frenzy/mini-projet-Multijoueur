using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameRecap : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI UI_Recap;
    
    private void Start()
    {
        if (GameManager.Instance.TeamWin.Value == Team.NOBODY)
        {
            GameManager.Instance.TeamWin.OnValueChanged += OnTeamWin;
        }
        else
        {
            DispScore();
        }
    }

    private void OnTeamWin(FixedString64Bytes previousvalue, FixedString64Bytes newvalue)
    {
        DispScore();
    }
    
    private void DispScore()
    {
        var winner = GameManager.Instance.TeamWin.Value;
        var playerInfo = GameManager.Instance.playerList.GetClientInfo(NetworkManager.Singleton.LocalClientId);
        var score = playerInfo.Score;
        var isAlive = playerInfo.IsAlive;
        UI_Recap.text = "Les grands gagnants sont " + winner + "\nVotre Score est : " + score + "\nVous êtes resté en vie jusqu'à la fin : " + isAlive;
    }
}
