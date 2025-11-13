using UnityEngine;

public class paintScript : MonoBehaviour
{
    private PortalConnector connector;
    [SerializeField] private PortalType newPortalType;

    void Awake()
    {
        connector = GameObject.FindGameObjectWithTag("portalConnector").GetComponent<PortalConnector>();
        Debug.Log("0");
    }

    void Start()
    {
        if (connector.portalTypeObtained.Contains(newPortalType)) {
            foreach(PortalType p in  connector.portalTypeObtained)
            {
                Debug.Log(p);
            }
            Destroy(gameObject);
        }
        Debug.Log(newPortalType);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
        connector.utilisablePortalType.Add(newPortalType);
        connector.portalTypeObtained.Add(newPortalType);
        Debug.Log("2");
    }

}
