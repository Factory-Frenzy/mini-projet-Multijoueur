using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI UI_Life;
    private void Start()
    {
        var playerManager = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
        UI_Life.text = "Life: "+ playerManager.Life.ToString();
        playerManager._life.OnValueChanged += UpdateUiLife;
    }

    private void UpdateUiLife(int previousValue, int newValue)
    {
        UI_Life.text = "Life: " + newValue.ToString();
    }
}
