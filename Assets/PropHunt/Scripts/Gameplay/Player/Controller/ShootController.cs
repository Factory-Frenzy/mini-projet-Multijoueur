using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShootController : NetworkBehaviour
{
    public ShootInfo ShootInfo;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            this.GetComponent<Rigidbody>().isKinematic = false;
            this.GetComponent<Rigidbody>().AddForce(transform.forward * 1000 + Vector3.up * 50);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            PlayerManager playerTouch = collision.gameObject.GetComponent<PlayerManager>();
            // Dommage pour les Props
            if (playerTouch && !playerTouch.isHunter.Value)
            {
                playerTouch.Life = -1;
                // Le SenderId vient de toucher un Prop, il gagne donc 1000 point de score
                GameManager.Instance.playerList.GetPlayerInfo(ShootInfo.SenderId).Score += 1000;
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
