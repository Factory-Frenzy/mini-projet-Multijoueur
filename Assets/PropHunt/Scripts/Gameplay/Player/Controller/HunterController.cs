using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class HunterController : ClassController
{
    [SerializeField] private GameObject Sphere;
    public override void Activate()
    {
        gameObject.SetActive(true);
        _camera.transform.SetParent(transform);
        _camera.transform.localPosition = new Vector3(-0.4f, 0.85f, -1.4f);
        ResetAnimator();
    }

    public override void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Shoot()
    {
        // rajouter le blocage dans la scene Lobby
        if (GameManager.Instance.GetStatus() == GameEnum.IN_GAME)
        {
            Vector3 positionInFront = transform.position + transform.forward;
            ShootOnlineServerRpc(positionInFront, transform.rotation);
        }
    }

    [ServerRpc]
    private void ShootOnlineServerRpc(Vector3 positionInFront, Quaternion rotation, ServerRpcParams serverRpcParams = default)
    {
        GameObject spawnedSphere = Instantiate(Sphere, positionInFront, rotation);
        spawnedSphere.GetComponent<NetworkObject>().Spawn();
        spawnedSphere.GetComponent<Rigidbody>().isKinematic = false;
        spawnedSphere.GetComponent<Rigidbody>().AddForce(transform.forward * 1000 + Vector3.up * 50);
        ShootController shootController = spawnedSphere.GetComponent<ShootController>();
        shootController.ShootInfo = new ShootInfo() { SenderId = serverRpcParams.Receive.SenderClientId };
    }
}
