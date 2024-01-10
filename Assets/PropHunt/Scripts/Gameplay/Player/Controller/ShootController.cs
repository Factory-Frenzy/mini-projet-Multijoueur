using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShootController : MonoBehaviour
{
    public ShootInfo ShootInfo;

    private void OnCollisionEnter(Collision collision)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            PlayerManager playerTouch = collision.gameObject.GetComponent<PlayerManager>();
            // Dommage pour les Props
            if (playerTouch && !playerTouch.isHunter.Value)
            {
                playerTouch.Life = -1;
            }
            else
            {
                GameObject senderPlayer = NetworkManager.Singleton.ConnectedClients[ShootInfo.SenderId].PlayerObject.gameObject;
                senderPlayer.GetComponent<PlayerManager>().Life = -1;
            }
            this.GetComponent<NetworkObject>().Despawn();
            //Destroy(this.gameObject);
        }
    }
}
