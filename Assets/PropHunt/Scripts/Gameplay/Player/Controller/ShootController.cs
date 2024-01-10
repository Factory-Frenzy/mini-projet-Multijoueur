using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootController : MonoBehaviour
{
    public ShootInfo ShootInfo;

    private void OnCollisionEnter(Collision collision)
    {
        PlayerManager playermanager = collision.gameObject.GetComponent<PlayerManager>();
        if (playermanager)
        {
            //playermanager.NetworkClientId
            playermanager.Life = -1;
        }
    }
}
