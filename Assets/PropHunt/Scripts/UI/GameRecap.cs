using System;
using System.Collections;
using System.Collections.Generic;
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
            GameManager.Instance.TeamWin.OnValueChanged += OnEndGame;
        }
        else
        {
            DispScore();
        }
    }

    private void OnEndGame(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        DispScore();
    }

    private void DispScore()
    {
        var winner = GameManager.Instance.TeamWin.Value;
        var playerInfo = GameManager.Instance.playerList.GetClientInfo(NetworkManager.Singleton.LocalClientId);
        var score = playerInfo.Score;
        var isAlive = playerInfo.IsAlive;
        UI_Recap.text = "Les grand gagnant sont " + winner + "\nVotre Score est : " + score + "\nVous etes resté en vie jusqu'a la fin : " + isAlive;
    }
}
