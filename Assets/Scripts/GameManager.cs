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
    [Header("점수 시스템")]
    public int score = 0;                    // 현재 점수
    public TextMeshProUGUI scoreText;        // 점수 표시 UI 텍스트

    // 생명 시스템
    [Header("생명 시스템")]
    public int lives = 3;                    // 현재 생명 수
    public TextMeshProUGUI livesText;        // 생명 표시 UI 텍스트

    bool gameOverFlag = false;               // 게임오버 상태 플래그


    // 게임 시작 시 초기화
    void Start()
    {
        UpdateScoreUI();
        UpdateLivesUI();
    }

    // 두 티어을 합치는 함수
    public void Merge(Tier a, Tier b)
    {
        // 합칠 수 없는 경우 처리
        if (a == null || b == null || a == b) return;

        // 이전 레벨 저장하고 새 레벨 계산
        int previousLevel = a.level;
        int newLevel = Mathf.Min(a.level + 1, maxLevel);
        // 두 티어의 중간 위치 계산
        Vector2 spawnPos = (a.transform.position + b.transform.position) * 0.5f;

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

        // 새 티어이 게임 영역 안에 있도록 위치 조정
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
            Time.timeScale = 0;  // 게임 일시정지
        }
    }
    
    // 게임 재시작 함수 (제작중)
    public void RestartGame()
    {
        // 게임 상태 초기화
        score = 0;
        lives = 3;
        gameOverFlag = false;
        Time.timeScale = 1;  // 게임 속도 정상화
        
        // UI 업데이트
        UpdateScoreUI();
        UpdateLivesUI();
    }
}
