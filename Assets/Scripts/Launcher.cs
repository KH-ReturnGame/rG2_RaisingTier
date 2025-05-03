using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Launcher : MonoBehaviour
{
    public GameObject fruitPrefab;
    public float maxForce = 20f;
    public float forceMultiplier = 2f;
    public int trajectoryPoints = 30;
    public float timeBetweenPoints = 0.1f;

    LineRenderer lr;
    GameObject currentFruit;
    Rigidbody2D currentRb;
    Vector2 startPos;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 0;
    }

    void Start()
    {
        startPos = transform.position;
        SpawnFruit();
    }

    void Update()
    {
        if (currentFruit == null) return;

        if (Input.GetMouseButton(0))
        {
            Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = startPos - (Vector2)mw;
            float force = Mathf.Min(dir.magnitude, maxForce) * forceMultiplier;
            DrawTrajectory(startPos, dir.normalized * force);
        }

        if (Input.GetMouseButtonUp(0))
            FireFruit();
    }

    void SpawnFruit()
    {
        if (fruitPrefab == null)
        {
            Debug.LogError("Fruit Prefab이 없음");
            return;
        }

        currentFruit = Instantiate(fruitPrefab, startPos, Quaternion.identity);
        currentRb    = currentFruit.GetComponent<Rigidbody2D>();
        currentRb.bodyType     = RigidbodyType2D.Kinematic;
        currentRb.gravityScale = 0f;
        currentFruit.tag       = "Fruit";
        lr.positionCount = 0;
    }

    void FireFruit()
    {
        currentRb.bodyType     = RigidbodyType2D.Dynamic;
        currentRb.gravityScale = 1f;

        Vector3 mw  = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = startPos - (Vector2)mw;
        float force = Mathf.Min(dir.magnitude, maxForce) * forceMultiplier;
        currentRb.AddForce(dir.normalized * force, ForceMode2D.Impulse);

        lr.positionCount = 0;
        currentFruit = null;
        currentRb    = null;

        Invoke(nameof(SpawnFruit), 1f);
    }

    void DrawTrajectory(Vector2 p0, Vector2 v0)
    {
        lr.positionCount = trajectoryPoints;
        Vector2 gravity = Physics2D.gravity;
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = timeBetweenPoints * i;
            Vector2 pt = p0 + v0 * t + 0.5f * gravity * t * t;
            lr.SetPosition(i, pt);
        }
    }
}
