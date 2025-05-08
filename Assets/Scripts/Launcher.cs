using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class Launcher : MonoBehaviour
{
    // 티어 발사 관련 변수
    public GameObject tierPrefab;            // 발사할 티어 프리팹
    public float maxForce = 20f;             // 최대 발사 힘
    public float forceMultiplier = 2f;       // 힘 배수

    // 궤적 관련 변수
    public int trajectoryPoints = 15;        // 궤적 포인트 수
    public float timeBetweenPoints = 0.1f;   // 포인트 간 시간 간격
    public float[] spawnProbabilities = { 0.6f, 0.25f, 0.1f, 0.05f };  // 각 레벨별 등장 확률
    
    // 궤적 시각화 설정
    [Header("궤적 시각화 설정")]
    [Range(0f, 1f)]
    public float startAlpha = 1.0f;          // 궤적 시작 투명도
    [Range(0f, 1f)]
    public float endAlpha = 0.0f;            // 궤적 끝 투명도
    public Color trajectoryColor = Color.white;  // 궤적 색상
    
    public Material trajectoryMaterial;      // 궤적 재질
    public float lineWidth = 0.1f;           // 궤적 선 굵기

    // 내부 변수
    LineRenderer lr;                         // 궤적 표시 라인 렌더러
    GameObject currentTier;                  // 현재 발사 대기 중인 티어
    Rigidbody2D currentTierRb;               // 티어의 리지드바디
    Vector2 startPos;                        // 시작 위치
    
    // 스폰 플래그 - 여러 번 호출되는 것을 방지
    private bool isSpawningTier = false;

    // 초기화 함수
    void Awake()
    {
        // 라인 렌더러 설정
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 0;
        lr.sortingOrder = 10;
        
        SetupLineRenderer();
        SetupTrajectoryGradient();
    }
    
    // 라인 렌더러 초기 설정
    void SetupLineRenderer()
    {
        if (trajectoryMaterial == null)
        {
            trajectoryMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        
        lr.material = trajectoryMaterial;
        
        // 알파 블렌딩 설정
        lr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        
        // 선 굵기 설정
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
    }

    // 게임 시작 시 호출
    void Start()
    {
        startPos = transform.position;
        SpawnTier();  // 첫 티어 생성
    }
    
    // 궤적의 그라데이션 설정
    void SetupTrajectoryGradient()
    {
        Gradient gradient = new Gradient();
        
        // 색상 키 설정
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0] = new GradientColorKey(trajectoryColor, 0f);
        colorKeys[1] = new GradientColorKey(trajectoryColor, 1f);
        
        // 알파(투명도) 키 설정
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(startAlpha, 0f);
        alphaKeys[1] = new GradientAlphaKey(endAlpha, 1f);
        
        gradient.SetKeys(colorKeys, alphaKeys);
        
        lr.colorGradient = gradient;
    }

    // 매 프레임 호출되는 업데이트 함수
    void Update()
    {
        if (currentTier == null) return;

        // 마우스 버튼을 누르고 있을 때 궤적 표시
        if (Input.GetMouseButton(0))
        {
            Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = startPos - (Vector2)mw;
            float force = Mathf.Min(dir.magnitude, maxForce) * forceMultiplier;
            DrawTrajectory(startPos, dir.normalized * force);
        }

        // 티어 발사
        if (Input.GetMouseButtonUp(0))
            FireTier();
    }

    // 새 티어 생성 - public으로 변경하고 중복 호출 방지
    public void SpawnTier()
    {
        // 이미 스폰 중이거나 현재 티어가 있으면 리턴
        if (isSpawningTier || currentTier != null)
            return;
            
        if (tierPrefab == null)
        {
            Debug.LogError("프리팹이 없음");
            return;
        }

        // 스폰 플래그 설정
        isSpawningTier = true;
        
        // 코루틴으로 처리하여 한 프레임 기다린 후 생성
        StartCoroutine(SpawnTierRoutine());
    }
    
    // 티어 생성 코루틴
    private IEnumerator SpawnTierRoutine()
    {
        // 한 프레임 대기 (이전 티어 정리 확인)
        yield return null;
        
        // 기존에 발사 위치에 티어가 있는지 확인
        Collider2D[] colliders = Physics2D.OverlapCircleAll(startPos, 0.5f);
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Tier"))
            {
                // 기존 티어가 발견되면 제거
                Destroy(col.gameObject);
            }
        }
        
        // 한 프레임 더 대기하여 Destroy가 완료되도록 함
        yield return null;

        // 티어 인스턴스 생성 및 설정
        currentTier = Instantiate(tierPrefab, startPos, Quaternion.identity);
        currentTierRb = currentTier.GetComponent<Rigidbody2D>();
        currentTierRb.bodyType = RigidbodyType2D.Kinematic;  // 움직임 없는 상태로 설정
        currentTierRb.gravityScale = 0f;  // 중력 영향 없음
        currentTier.tag = "Tier";

        // 티어 레벨 랜덤 설정
        var tier = currentTier.GetComponent<Tier>();
        if (tier != null)
        {
            tier.level = GetRandomTierLevel();
            tier.UpdateSprite();
        }

        // 궤적 초기화
        lr.positionCount = 0;
        
        // 스폰 플래그 해제
        isSpawningTier = false;
        
        Debug.Log("새 티어 생성 완료: 레벨 " + tier.level);
    }

    // 확률에 따라 랜덤 레벨 결정
    int GetRandomTierLevel()
    {
        float rand = Random.value;
        float cumulative = 0f;

        for (int i = 0; i < spawnProbabilities.Length; i++)
        {
            cumulative += spawnProbabilities[i];
            if (rand <= cumulative)
                return i + 1;
        }
        return 1;
    }

    // 티어 발사 함수
    void FireTier()
    {
        if (currentTier == null)
            return;
            
        // 물리 동작 활성화
        currentTierRb.bodyType = RigidbodyType2D.Dynamic;
        currentTierRb.gravityScale = 1f;

        // 발사 방향과 힘 계산
        Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = startPos - (Vector2)mw;
        float force = Mathf.Min(dir.magnitude, maxForce) * forceMultiplier;
        currentTierRb.AddForce(dir.normalized * force, ForceMode2D.Impulse);

        // 회전 추가
        float torque = -force * 10f;  
        currentTierRb.angularVelocity = -360f;

        // 궤적 숨기고 현재 티어 참조 제거
        lr.positionCount = 0;
        currentTier = null;
        currentTierRb = null;

        // 일정 시간 후 새 티어 생성
        CancelInvoke(nameof(SpawnTier));  // 이전 Invoke 취소
        Invoke(nameof(SpawnTier), 0.5f);
    }

    // 발사 궤적 표시 함수
    void DrawTrajectory(Vector2 p0, Vector2 v0)
    {
        lr.positionCount = trajectoryPoints;
        Vector2 gravity = Physics2D.gravity;
        
        // 포물선 궤적 계산 및 표시
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = timeBetweenPoints * i;
            Vector2 pt = p0 + v0 * t + 0.5f * gravity * t * t;
            lr.SetPosition(i, pt);
        }
    }
    
    // Inspector에서 값이 변경될 때 호출되는 함수
    void OnValidate()
    {
        if (lr != null)
        {
            SetupTrajectoryGradient();
        }
    }
}