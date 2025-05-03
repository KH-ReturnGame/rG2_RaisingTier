using UnityEngine;
using UnityEngine.UI; // UI 요소 사용을 위해 추가
using TMPro; // TextMeshPro 사용을 위해 추가
using System.Collections; // IEnumerator 사용

public class GameManager : MonoBehaviour
{
    public BoxCollider2D containerInterior;
    public GameObject tierPrefab;
    public int maxLevel = 8;

    [Header("점수 시스템")]
    public int score = 0;
    public TextMeshProUGUI scoreText; // TMP Text 컴포넌트 연결용

    [Header("생명 시스템")]
    public int lives = 3;
    public TextMeshProUGUI livesText; // TMP Text 컴포넌트 연결용
    
    bool gameOverFlag = false;

    void Start()
    {
        // 점수와 생명 초기화
        UpdateScoreUI();
        UpdateLivesUI();
    }

    public void Merge(Tier a, Tier b)
    {
        if (a == null || b == null || a == b) return;

        int previousLevel = a.level;
        int newLevel = Mathf.Min(a.level + 1, maxLevel);
        Vector2 spawnPos = (a.transform.position + b.transform.position) * 0.5f;

        Destroy(a.gameObject);
        Destroy(b.gameObject);

        GameObject merged = Instantiate(tierPrefab, spawnPos, Quaternion.identity);

        merged.tag = "Tier";
        var rb = merged.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;

        var mf = merged.GetComponent<Tier>();
        mf.level = newLevel;
        mf.UpdateSprite();
        mf.ResetMergeCooldown();

        ClampInsideContainer(merged);

        AddScore((int)Mathf.Pow(2, newLevel - 1));
    }

    // 점수 추가 함수
    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();
        
        // 점수 추가 효과 (선택적)
        Debug.Log($"+{points} 점! 현재 점수: {score}");
    }

    // 점수 UI 업데이트
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    // 생명 UI 업데이트
    void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
        }
    }

    void ClampInsideContainer(GameObject tierObj)
    {
        if (containerInterior == null)
        {
            Debug.LogError("containerInterior가 없음");
            return;
        }

        Bounds ib = containerInterior.bounds;
        var sr = tierObj.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        Vector3 ext = sr.bounds.extents;

        Vector3 p = tierObj.transform.position;
        p.x = Mathf.Clamp(p.x, ib.min.x + ext.x, ib.max.x - ext.x);
        if (p.y - ext.y < ib.min.y)
            p.y = ib.min.y + ext.y;
        tierObj.transform.position = p;
    }

    public void GameOver()
    {
        // 이미 게임오버 상태라면 리턴
        if (gameOverFlag) return;
        
        // 생명 감소
        lives--;
        UpdateLivesUI();
        
        Debug.Log($"생명 감소! 남은 생명: {lives}");
        
        // 생명이 0이 되면 게임 오버
        if (lives <= 0)
        {
            gameOverFlag = true;
            Debug.Log("게임 오버! 최종 점수: " + score);
            Time.timeScale = 0;
        }
        else
        {
            // 잠시 멈추고 게임 계속하기
            StartCoroutine(PauseThenContinue());
        }
    }
    
    // 잠시 게임을 멈췄다가 계속하는 코루틴
    IEnumerator PauseThenContinue()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(1.0f); // 실제 시간 1초 기다림
        Time.timeScale = 1;
    }
    
    // 게임 재시작 함수 (필요시 구현)
    public void RestartGame()
    {
        // 현재 씬 재로드 등의 재시작 로직
        score = 0;
        lives = 3;
        gameOverFlag = false;
        Time.timeScale = 1;
        
        UpdateScoreUI();
        UpdateLivesUI();
        
        // 씬 재로드가 필요한 경우:
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}