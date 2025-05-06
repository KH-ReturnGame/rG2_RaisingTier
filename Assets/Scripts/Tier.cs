using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(PolygonCollider2D))]
public class Tier : MonoBehaviour
{
    // 티어 속성
    public int level = 1;                  // 티어 레벨
    public Sprite[] levelSprites;          // 레벨별 스프라이트 배열
    public float mergeDelay = 0.1f;        // 티어 합치기 지연 시간
    
    // 콜라이더 크기 조정 배율
    public float colliderSizeMultiplier = 1f;

    // 내부 변수
    private bool canMerge = false;          // 합치기 가능 여부
    private SpriteRenderer sr;              // 스프라이트 렌더러
    private Rigidbody2D rb;                 // 리지드바디
    private PolygonCollider2D polygonCollider;  // 다각형 콜라이더
    private GameManager gm;                 // 게임 매니저 참조

    // 초기화 함수
    void Awake()
    {
        // 컴포넌트 참조 가져오기
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        gm = FindObjectOfType<GameManager>();
    }

    // 시작 함수
    void Start()
    {
        UpdateSprite();  // 스프라이트 업데이트
        StartCoroutine(EnableMergeAfterDelay());  // 일정 시간 후 합치기 활성화
    }

    // 일정 시간 후 합치기 기능 활성화하는 코루틴
    private IEnumerator EnableMergeAfterDelay()
    {
        yield return new WaitForSeconds(mergeDelay);
        canMerge = true;
    }

    // 합치기 쿨다운 초기화 함수
    public void ResetMergeCooldown()
    {
        canMerge = false;
        StartCoroutine(EnableMergeAfterDelay());
    }

    // 레벨에 맞는 스프라이트로 업데이트
    public void UpdateSprite()
    {
        if (levelSprites != null && levelSprites.Length >= level && level > 0)
        {
            sr.sprite = levelSprites[level - 1];
            UpdateCollider();  // 콜라이더도 함께 업데이트
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
            // 콜라이더 초기화
            polygonCollider.enabled = false;
            polygonCollider.pathCount = 0;
            polygonCollider.enabled = true;
            
            // 스프라이트에서 자동 콜라이더 생성
            polygonCollider.autoTiling = true;
            polygonCollider.pathCount = sr.sprite.GetPhysicsShapeCount();
            
            // 각 경로에 대해 포인트 가져오기 및 적용
            for (int i = 0; i < polygonCollider.pathCount; i++)
            {
                List<Vector2> path = new List<Vector2>();
                sr.sprite.GetPhysicsShape(i, path);
                
                // 필요한 경우 콜라이더 크기 조정
                if (colliderSizeMultiplier != 1.0f)
                {
                    Vector2 center = CalculateCenter(path);
                    for (int j = 0; j < path.Count; j++)
                    {
                        // 중심에서 각 점까지의 벡터를 구해 크기 조절
                        Vector2 dirToPoint = path[j] - center;
                        path[j] = center + dirToPoint * colliderSizeMultiplier;
                    }
                }
                
                // 경로 설정
                polygonCollider.SetPath(i, path);
            }
        }
    }
    
    // 폴리곤 중심점 계산 함수
    private Vector2 CalculateCenter(List<Vector2> points)
    {
        if (points == null || points.Count == 0)
            return Vector2.zero;
            
        Vector2 sum = Vector2.zero;
        foreach (Vector2 point in points)
        {
            sum += point;
        }
        
        return sum / points.Count;  // 평균 위치 반환
    }

    // 충돌 이벤트 처리
    void OnCollisionEnter2D(Collision2D col)
    {
        // 합치기 조건 확인
        if (!canMerge) return;
        if (!col.collider.CompareTag("Tier")) return;

        Tier other = col.collider.GetComponent<Tier>();
        // 동일 레벨의 티어만 합치기 가능
        if (other == null || other == this || !other.canMerge || other.level != level)
            return;

        // 합치기 기능 비활성화하고 게임 매니저에 합치기 요청
        canMerge = other.canMerge = false;
        gm.Merge(this, other);
    }
}