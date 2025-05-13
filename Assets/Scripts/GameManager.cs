using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // 게임 영역 및 오브젝트 관련 변수
    public BoxCollider2D containerInterior;  // 게임 영역의 콜라이더
    public GameObject tierPrefab;            // 생성할 티어 프리팹
    public int maxLevel = 8;                 // 최대 티어 레벨

    // 점수 시스템
    [Header("점수")]
    public int score = 0;                    // 현재 점수
    public TextMeshProUGUI scoreText;        // 점수 표시 UI 텍스트

    // 생명 시스템
    [Header("생명")]
    public int lives = 3;                    // 현재 생명 수
    public TextMeshProUGUI livesText;        // 생명 표시 UI 텍스트

    // 게임오버 패널
    public GameOverPanel gameOverPanel;      // 게임오버 패널 참조

    // 로더 레퍼런스
    public Launcher launcher;                // Launcher 컴포넌트 참조 (직접 할당)

    bool gameOverFlag = false;               // 게임오버 상태 플래그
    bool isRestarting = false;               // 재시작 중인지 여부를 나타내는 플래그

    // 게임 시작 시 초기화
    void Start()
    {
        UpdateScoreUI();
        UpdateLivesUI();
        
        // 시간 스케일이 0이라면 1로 복구 (이전 게임오버 상태에서 복구)
        if (Time.timeScale == 0)
            Time.timeScale = 1;
            
        // Launcher 참조가 없으면 찾아서 할당
        if (launcher == null)
            launcher = FindObjectOfType<Launcher>();
    }

    // 두 티어를 합치는 함수
    public void Merge(Tier a, Tier b)
    {
        // 합칠 수 없는 경우나 재시작 중이면 처리하지 않음
        if (a == null || b == null || a == b || isRestarting) return;

        // 이전 레벨 저장하고 새 레벨 계산
        int previousLevel = a.level;
        int newLevel = Mathf.Min(a.level + 1, maxLevel);
        // 두 티어의 중간 위치 계산
        Vector2 spawnPos = (a.transform.position + b.transform.position) * 0.5f;

        // 벽에 끼는 현상 방지 (오프셋 적용)
        float safeOffset = 0.1f;
        Vector2 containerCenter = containerInterior.bounds.center;
        Vector2 normal = (spawnPos - containerCenter).normalized;
        spawnPos += normal * safeOffset;

        // 기존 티어 제거
        Destroy(a.gameObject);
        Destroy(b.gameObject);

        // 새 티어 생성
        GameObject merged = Instantiate(tierPrefab, spawnPos, Quaternion.identity);

        // 새 티어의 속성 설정
        merged.tag = "Tier";
        var rb = merged.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;

        // 새 티어의 레벨 설정 및 스프라이트 업데이트
        var mf = merged.GetComponent<Tier>();
        mf.level = newLevel;
        mf.UpdateSprite();
        mf.ResetMergeCooldown();

        // 새 티어가 게임 영역 안에 있도록 위치 조정
        ClampInsideContainer(merged);

        // 점수 추가 (2^(레벨-1) 만큼의 점수)
        AddScore((int)Mathf.Pow(2, newLevel - 1));
    }

    // 점수 추가 함수
    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();
        
        Debug.Log($"+{points} 점! 현재 점수: {score}");
    }

    // 점수 UI 업데이트
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score : {score}";
        }
    }

    // 생명 UI 업데이트
    void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = $"Lives : {lives}";
        }
    }

    // 오브젝트가 게임 영역 안에 있도록 위치 조정
    void ClampInsideContainer(GameObject tierObj)
    {
        if (containerInterior == null)
        {
            Debug.LogError("containerInterior가 없음");
            return;
        }

        // 게임 영역의 경계와 오브젝트의 크기 정보 가져오기
        Bounds ib = containerInterior.bounds;
        var sr = tierObj.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        Vector3 ext = sr.bounds.extents;

        // 오브젝트 위치 조정
        Vector3 p = tierObj.transform.position;
        p.x = Mathf.Clamp(p.x, ib.min.x + ext.x, ib.max.x - ext.x);
        if (p.y - ext.y < ib.min.y)
            p.y = ib.min.y + ext.y;
        tierObj.transform.position = p;
    }

    // 게임오버 처리 함수
    public void GameOver()
    {
        if (gameOverFlag) return;

        // 생명 감소
        lives--;
        UpdateLivesUI();

        Debug.Log($"생명 감소 → 남은 생명: {lives}");

        // 생명이 0이 되면 게임 종료
        if (lives <= 0)
        {
            gameOverFlag = true;
            Debug.Log("게임 오버! 최종 점수: " + score);
            
            // 게임오버 패널 표시
            if (gameOverPanel != null)
            {
                gameOverPanel.ShowGameOverPanel(score);
                
                // 패널 표시 후 게임 일시정지
                StartCoroutine(PauseGameAfterDelay(0.1f));
            }
            else
            {
                Time.timeScale = 0; // 패널이 없는 경우 바로 일시정지
            }
        }
    }
    
    // 일정 시간 후 게임 일시정지하는 코루틴
    private IEnumerator PauseGameAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 0;  // 게임 일시정지
    }
    
    // 게임 재시작 함수
    public void RestartGame()
    {
        // 재시작 플래그 설정
        isRestarting = true;
        
        // 게임 상태 초기화
        score = 0;
        lives = 3;
        gameOverFlag = false;
        Time.timeScale = 1;  // 게임 속도 정상화
        
        // UI 업데이트
        UpdateScoreUI();
        UpdateLivesUI();
        
        // 씬에 있는 모든 Tier 오브젝트 제거
        StartCoroutine(CleanupAndRestartGame());
    }
    
    // 게임 오브젝트 정리 및 재시작 코루틴
    private IEnumerator CleanupAndRestartGame()
    {
        // 씬에 있는 모든 Tier 오브젝트 제거
        GameObject[] tiers = GameObject.FindGameObjectsWithTag("Tier");
        foreach (GameObject tier in tiers)
        {
            Destroy(tier);
        }
        
        // 한 프레임 대기하여 Destroy가 완료되도록 함
        yield return null;
        
        // Launcher 컴포넌트를 찾아서 새 티어 생성 메소드 호출
        if (launcher == null)
        {
            launcher = FindObjectOfType<Launcher>();
        }
        
        if (launcher != null)
        {
            // 런처가 있으면 직접 SpawnTier 호출
            launcher.SpawnTier();
            Debug.Log("새 티어가 생성되었습니다.");
        }
        else
        {
            Debug.LogError("Launcher를 찾을 수 없습니다!");
        }
        
        // 재시작 플래그 해제
        isRestarting = false;
    }
}
