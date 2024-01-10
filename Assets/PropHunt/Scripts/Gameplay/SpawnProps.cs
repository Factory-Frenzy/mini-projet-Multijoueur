using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnProps : MonoBehaviour
{
    public Prop PropToSpawn;
    public int minNumberOfProps = 1;
    public int maxNumberOfProps = 5;
    public List<Transform> SpawnPoints;

    void Start()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        int nPropsToSpawn = Random.Range(minNumberOfProps, maxNumberOfProps);

        SpawnPoints.Shuffle();

        for (int i = 0; i < nPropsToSpawn; i++)
        {
            var sp = SpawnPoints[i];
            var transform = PropToSpawn.transform;
            var go = Instantiate(PropToSpawn.gameObject, sp.position, transform.rotation);
            go.GetComponent<NetworkObject>().Spawn();
        }
    }
}


static class MyExtensions
{
    private static System.Random rng = new();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}