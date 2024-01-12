using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerList:NetworkBehaviour
{
    private int NbHunter = 0;
    private int NbProp = 0;
    [NonSerialized]
    public NetworkVariable<string> TeamWin = new NetworkVariable<string>(Team.NOBODY, NetworkVariableReadPermission.Everyone);
    [NonSerialized]
    public NetworkVariable<List<ScoreClass>> ScorePlayers = new NetworkVariable<List<ScoreClass>>(new List<ScoreClass>());
    public void InitPlayerList()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject.GetComponent<PlayerManager>().isHunter.Value)
            {
                NbHunter++;
            }
            else
            {
                NbProp++;
            }
            ScorePlayers.Value.Add(new ScoreClass(client.ClientId));
        }
    }

    public bool OneMoreDeath(ulong deathId)
    {
        var death = NetworkManager.Singleton.ConnectedClients[deathId];
        if (death == null) throw new InvalidOperationException("Message d'erreur OneMoreDeath");

        if (death.PlayerObject.GetComponent<PlayerManager>().isHunter.Value)
        {
            NbHunter--;
        }
        else
        {
            NbProp--;
        }
        GetPlayerInfo(deathId).IsAlive = false;
        return TeamWinCheck();
    }

    private bool TeamWinCheck()
    {
        if (NbHunter == 0)
        {
            TeamWin.Value = Team.PROB;
            return true;
        }
        else if (NbProp == 0)
        {
            TeamWin.Value = Team.HUNTER;
            return true;
        }
        else
        {
            return false;
        }
    }

    public ScoreClass GetPlayerInfo(ulong clientId)
    {
        foreach (var item in ScorePlayers.Value)
        {
            if (item.ClientId == clientId)
            {
                return item;
            }
        }
        return null;
    }
}

public struct Team
{
    public const string HUNTER = "Hunter";
    public const string PROB = "Prob";
    public const string NOBODY = "";
}

public class ScoreClass
{
    private int _score = 0;
    public ulong ClientId { get; set; }
    public int Score
    {
        get { return _score; }
        set
        {
            _score = value;
            ScoreUpdate?.Invoke(this, EventArgs.Empty);
        }
    }
    public bool IsAlive { get; set; }
    public event EventHandler ScoreUpdate;
    public ScoreClass(ulong clientId)
    {
        ClientId = clientId;
        IsAlive = true;
    }
}
