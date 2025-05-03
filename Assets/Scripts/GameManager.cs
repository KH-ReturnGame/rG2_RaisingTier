using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public BoxCollider2D containerInterior;
    public GameObject tierPrefab;
    public int maxLevel = 8;

    [Header("점수 시스템")]
    public int score = 0;
    public TextMeshProUGUI scoreText;

    [Header("생명 시스템")]
    public int lives = 3;
    public TextMeshProUGUI livesText;
    
    bool gameOverFlag = false;

    void Start()
    {
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

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();
        
        Debug.Log($"+{points} 점! 현재 점수: {score}");
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score : {score}";
        }
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = $"Lives : {lives}";
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
        if (gameOverFlag) return;

        lives--;
        UpdateLivesUI();
        
        Debug.Log($"생명 감소! 남은 생명: {lives}");
        
        if (lives <= 0)
        {
            gameOverFlag = true;
            Debug.Log("게임 오버! 최종 점수: " + score);
            Time.timeScale = 0;
        }
        else
        {
        }
    }
    
    public void RestartGame()
    {
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