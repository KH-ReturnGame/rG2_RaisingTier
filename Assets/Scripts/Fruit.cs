// Assets/Scripts/Fruit.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Fruit : MonoBehaviour
{
    public int level = 1;
    public Sprite[] levelSprites;

    [Header("머지 쿨다운(")]
    public float mergeDelay = 0.1f;

    private bool canMerge = false;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private GameManager gm;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        gm = FindObjectOfType<GameManager>();
    }

    void Start()
    {
        UpdateSprite();
        StartCoroutine(EnableMergeAfterDelay());
    }

    private IEnumerator EnableMergeAfterDelay()
    {
        yield return new WaitForSeconds(mergeDelay);
        canMerge = true;
    }

    public void ResetMergeCooldown()
    {
        canMerge = false;
        StartCoroutine(EnableMergeAfterDelay());
    }

    public void UpdateSprite()
    {
        if (levelSprites.Length >= level)
            sr.sprite = levelSprites[level - 1];
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!canMerge) return;
        if (!col.collider.CompareTag("Fruit")) return;

        Fruit other = col.collider.GetComponent<Fruit>();
        if (other == null || other == this || !other.canMerge || other.level != level)
            return;

        canMerge = other.canMerge = false;
        gm.Merge(this, other);
    }
}
