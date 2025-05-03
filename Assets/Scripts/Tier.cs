using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Tier : MonoBehaviour
{
    public int level = 1;
    public Sprite[] levelSprites;
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
        if (levelSprites != null && levelSprites.Length >= level && level > 0)
        {
            sr.sprite = levelSprites[level - 1];
        }
        else
        {
            Debug.LogWarning($"레벨 {level}에 해당하는 스프라이트가 없습니다!");
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!canMerge) return;
        if (!col.collider.CompareTag("Tier")) return;

        Tier other = col.collider.GetComponent<Tier>();
        if (other == null || other == this || !other.canMerge || other.level != level)
            return;

        canMerge = other.canMerge = false;
        gm.Merge(this, other);
    }
}