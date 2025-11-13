using System.ComponentModel;
using UnityEngine;

public class PyramidTeleportOutside : MonoBehaviour
{
    private GameObject outdoorSpawnPoint;
    private GameObject pyramidOutdoors;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            outdoorSpawnPoint = GameObject.Find("Outdoor Spawnpoint");
            pyramidOutdoors = GameObject.FindGameObjectWithTag("pyramidIn");

            other.gameObject.transform.SetPositionAndRotation(outdoorSpawnPoint.transform.position, Quaternion.identity);
            //other.gameObject.transform.Rotate(new Vector3(0, 180, 0), Space.World);

            Destroy(pyramidOutdoors);
        }
        else
        {
            Debug.LogError("Collide avec autre chose qu'un joueur");
        }
    }
}
