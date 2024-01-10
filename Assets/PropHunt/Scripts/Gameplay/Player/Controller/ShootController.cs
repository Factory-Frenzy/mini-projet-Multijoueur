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
            PlayerManager playermanager = collision.gameObject.GetComponent<PlayerManager>();
            if (playermanager)
            {
                playermanager.Life = -1;
            }
            this.GetComponent<NetworkObject>().Despawn();
            Destroy(this.gameObject);
        }
    }
}
