using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using static UnityEngine.PlayerLoop.EarlyUpdate;
using TMPro;
public class GameUIScore : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI score_TextMeshPro;
    [SerializeField] private TextMeshProUGUI life_TextMeshPro;
    void Start()
    {
        print(GameManager.Instance);
        GameManager.Instance.playerList.GetPlayerInfo(NetworkManager.Singleton.LocalClientId).ScoreUpdate += ScoreUIUpdate;
        NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<PlayerManager>().OnLifeUpdate += LifeUIUpdate;
        var lifeStart = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<PlayerManager>().Life;
        score_TextMeshPro.text = "Life: " + lifeStart.ToString();
    }

    private void ScoreUIUpdate(object sender, System.EventArgs e)
    {
        /*if (NetworkManager.Singleton.IsClient)
        {
            print("Score: " + (sender as ScoreClass).Score.ToString());
            m_TextMeshPro.text = "Score: "+(sender as ScoreClass).Score.ToString();
        }*/
        print("Score: " + (sender as ScoreClass).Score.ToString());
        score_TextMeshPro.text = "Score: " + (sender as ScoreClass).Score.ToString();
    }

    private void LifeUIUpdate(object sender, System.EventArgs e)
    {
        print("Life: " + (sender as PlayerManager).Life.ToString());
        score_TextMeshPro.text = "Life: " + (sender as PlayerManager).Life.ToString();
    }
}
