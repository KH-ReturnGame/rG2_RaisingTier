using UnityEngine;

public class ContainerBoundary : MonoBehaviour
{
    [SerializeField] private string killZoneObjectName = "killZone";

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Tier") && gameObject.name == killZoneObjectName)
        {
            FindObjectOfType<GameManager>().GameOver();
        }
        else
        {
        }
    }

}