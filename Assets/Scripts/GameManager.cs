using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoxCollider2D containerInterior;
    public GameObject fruitPrefab;

    public int maxLevel = 3;

    bool gameOverFlag = false;

    public void Merge(Fruit a, Fruit b)
    {
        if (a == null || b == null || a == b) return;

        int newLevel = Mathf.Min(a.level + 1, maxLevel);
        Vector2 spawnPos = (a.transform.position + b.transform.position) * 0.5f;

        Destroy(a.gameObject);
        Destroy(b.gameObject);

        GameObject merged = Instantiate(fruitPrefab, spawnPos, Quaternion.identity);

        merged.tag = "Fruit";
        var rb = merged.GetComponent<Rigidbody2D>();
        rb.bodyType     = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;

        var mf = merged.GetComponent<Fruit>();
        mf.level = newLevel;
        mf.UpdateSprite();
        mf.ResetMergeCooldown();

        ClampInsideContainer(merged);
    }

    void ClampInsideContainer(GameObject fruitObj)
    {
        if (containerInterior == null)
        {
            Debug.LogError("GameManager: containerInterior가 할당되지 않았습니다!");
            return;
        }

        Bounds ib = containerInterior.bounds;
        var sr = fruitObj.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        Vector3 ext = sr.bounds.extents;

        Vector3 p = fruitObj.transform.position;
        p.x = Mathf.Clamp(p.x, ib.min.x + ext.x, ib.max.x - ext.x);
        if (p.y - ext.y < ib.min.y)
            p.y = ib.min.y + ext.y;
        fruitObj.transform.position = p;
    }

    public void GameOver()
    {
        if (gameOverFlag) return;
        gameOverFlag = true;
        Debug.Log("게임 오버");
        Time.timeScale = 0;
    }
}
