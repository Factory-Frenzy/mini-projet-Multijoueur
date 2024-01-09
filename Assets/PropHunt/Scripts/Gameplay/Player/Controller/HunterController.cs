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
        print("new sphere");
        // Calculate the position in front of the parent object
        Vector3 positionInFront = transform.position + transform.forward;

        // Instantiate the sphere at the calculated position and with the same rotation as the parent
        // GameObject spawnedSphere = Instantiate(Sphere, positionInFront, transform.rotation);
        ShootOnlineServerRpc(positionInFront, transform.rotation);
    }

    [ServerRpc]
    private void ShootOnlineServerRpc(Vector3 positionInFront, Quaternion rotation)
    {
        GameObject spawnedSphere = Instantiate(Sphere, positionInFront, rotation);
        spawnedSphere.GetComponent<NetworkObject>().Spawn();
        spawnedSphere.GetComponent<Rigidbody>().isKinematic = false;
        spawnedSphere.GetComponent<Rigidbody>().AddForce(transform.forward * 1000 + Vector3.up * 50);
    }
}
