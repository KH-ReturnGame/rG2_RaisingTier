using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(PolygonCollider2D))]
public class Tier : MonoBehaviour
{
    public int level = 1;
    public Sprite[] levelSprites;
    public float mergeDelay = 0.1f;
    
    // 레벨별 콜라이더 크기 배율 (필요시 조정)
    public float colliderSizeMultiplier = 1f;

    private bool canMerge = false;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private PolygonCollider2D polygonCollider;
    private GameManager gm;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        polygonCollider = GetComponent<PolygonCollider2D>();
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
            UpdateCollider();
        }
        else
        {
            Debug.LogWarning($"레벨 {level}에 해당하는 스프라이트가 없습니다!");
        }
    }
    
    // 스프라이트에 맞게 폴리곤 콜라이더 업데이트
    private void UpdateCollider()
    {
        if (polygonCollider != null && sr.sprite != null)
        {
            // 스프라이트의 Physics Shape을 콜라이더에 적용
            polygonCollider.enabled = false;
            polygonCollider.pathCount = 0;
            polygonCollider.enabled = true;
            
            // 스프라이트에서 자동으로 콜라이더 생성
            polygonCollider.autoTiling = true;
            polygonCollider.pathCount = sr.sprite.GetPhysicsShapeCount();
            
            // 각 패스에 대해 포인트 가져오기 및 적용
            for (int i = 0; i < polygonCollider.pathCount; i++)
            {
                List<Vector2> path = new List<Vector2>();
                sr.sprite.GetPhysicsShape(i, path);
                
                // 콜라이더 크기 약간 조정 (필요한 경우)
                if (colliderSizeMultiplier != 1.0f)
                {
                    Vector2 center = CalculateCenter(path);
                    for (int j = 0; j < path.Count; j++)
                    {
                        // 중심에서 각 점까지의 벡터를 구하고 크기 조절
                        Vector2 dirToPoint = path[j] - center;
                        path[j] = center + dirToPoint * colliderSizeMultiplier;
                    }
                }
                
                polygonCollider.SetPath(i, path);
            }
        }
    }
    
    // 폴리곤 중심점 계산
    private Vector2 CalculateCenter(List<Vector2> points)
    {
        if (points == null || points.Count == 0)
            return Vector2.zero;
            
        Vector2 sum = Vector2.zero;
        foreach (Vector2 point in points)
        {
            sum += point;
        }
        
        return sum / points.Count;
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