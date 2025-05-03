using UnityEngine;

public class ContainerBoundary : MonoBehaviour
{
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Tier"))
            FindObjectOfType<GameManager>().GameOver();
    }
}