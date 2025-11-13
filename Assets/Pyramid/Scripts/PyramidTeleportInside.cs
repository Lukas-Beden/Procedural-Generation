using System.ComponentModel;
using UnityEngine;

public class PyramidTeleportInside : MonoBehaviour
{
    private GameObject indoorSpawnPoint;
    [SerializeField] private GameObject pyramidIndoors;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Instantiate(pyramidIndoors, new Vector3(50,5000,50), Quaternion.identity);
            
            indoorSpawnPoint = GameObject.Find("Indoor Spawnpoint");

            other.gameObject.transform.SetPositionAndRotation(indoorSpawnPoint.transform.position, Quaternion.identity);
            other.gameObject.transform.Rotate(new Vector3(0,180,0), Space.World);
        }
        else
        {
            Debug.LogError("Collide avec autre chose qu'un joueur");
        }
    }
}
