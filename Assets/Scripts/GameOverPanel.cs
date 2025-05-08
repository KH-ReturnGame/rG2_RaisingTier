using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameOverPanel : MonoBehaviour
{
    public GameObject panelObject;          // 게임오버 패널 오브젝트
    public TextMeshProUGUI finalScoreText;  // 최종 점수 텍스트
    
    public Image gifDisplayImage;           // GIF 프레임을 표시할 이미지 컴포넌트
    public string gifFramesPath = "Sprites/GifFrames"; // 프레임 이미지가 있는 경로
    public float frameDelay = 0.05f;        // 프레임 간 지연 시간 (초)
    public bool loopAnimation = true;       // 애니메이션 반복 여부
    
    private Sprite[] frameSprites;          // 로드된 프레임 스프라이트 배열
    private Coroutine animationCoroutine;   // 애니메이션 코루틴 참조
    private bool isProcessingButton = false; // 버튼 처리 중인지 여부를 나타내는 플래그
    
    // 초기화
    void Awake()
    {
        // 패널 비활성화 상태로 시작
        if (panelObject != null)
            panelObject.SetActive(false);
            
        // 스프라이트 프레임 로드
        LoadFrameSprites();
    }
    
    // 스프라이트 프레임 로드 함수
    private void LoadFrameSprites()
    {
        // Resources 폴더에서 스프라이트 로드
        frameSprites = Resources.LoadAll<Sprite>(gifFramesPath);
        
        if (frameSprites == null || frameSprites.Length == 0)
        {
            Debug.LogError($"경로에서 스프라이트를 찾을 수 없습니다: Resources/{gifFramesPath}");
        }
        else
        {
            Debug.Log($"{frameSprites.Length}개의 프레임 스프라이트를 로드했습니다.");
        }
    }
    
    // 게임오버 패널 표시 함수
    public void ShowGameOverPanel(int finalScore)
    {
        // 최종 점수 표시
        if (finalScoreText != null)
            finalScoreText.text = $"Final Score: {finalScore}";
            
        // 패널 활성화
        if (panelObject != null)
            panelObject.SetActive(true);
            
        // 패널이 활성화된 후에 애니메이션 시작
        StartCoroutine(StartAnimationNextFrame());
        
        // 버튼 처리 플래그 초기화
        isProcessingButton = false;
    }
    
    // 다음 프레임에 애니메이션 시작하는 코루틴
    private IEnumerator StartAnimationNextFrame()
    {
        // 한 프레임 대기
        yield return null;
        
        // 애니메이션 시작
        StartGifAnimation();
    }
    
    // GIF 애니메이션 시작 함수
    public void StartGifAnimation()
    {
        // 이미지 컴포넌트가 없거나 프레임이 없으면 리턴
        if (gifDisplayImage == null || frameSprites == null || frameSprites.Length == 0)
        {
            Debug.LogWarning("GIF 애니메이션을 시작할 수 없습니다");
            return;
        }
        
        // 게임 오브젝트가 활성화되어 있는지 확인
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("GIF 애니메이션을 시작할 수 없습니다");
            return;
        }
            
        // 이미 실행 중인 애니메이션이 있다면 중지
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
            
        // 새 애니메이션 시작
        animationCoroutine = StartCoroutine(PlayGifAnimation());
    }
    
    // GIF 애니메이션 재생 코루틴
    private IEnumerator PlayGifAnimation()
    {
        // 이미지와 프레임이 존재하는지 다시 확인
        if (gifDisplayImage == null || frameSprites == null || frameSprites.Length == 0)
        {
            Debug.LogError("PlayGifAnimation: 이미지 또는 프레임이 없습니다.");
            yield break;
        }
        
        Debug.Log($"애니메이션 시작: {frameSprites.Length}개의 프레임");
        int frameIndex = 0;
        
        // 애니메이션 루프
        while (true)
        {
            try
            {
                // 현재 프레임 표시
                if (frameIndex < frameSprites.Length && gifDisplayImage != null)
                {
                    gifDisplayImage.sprite = frameSprites[frameIndex];
                }
                else
                {
                    Debug.LogWarning($"프레임 인덱스 오류: {frameIndex} / {frameSprites.Length}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"프레임 표시 중 오류 발생: {e.Message}");
            }
            
            // 다음 프레임 대기 (Time.timeScale이 0이어도 작동)
            yield return new WaitForSecondsRealtime(frameDelay);
            
            // 다음 프레임 인덱스 계산
            frameIndex = (frameIndex + 1) % frameSprites.Length;
            
            // 애니메이션 반복 여부 확인
            if (!loopAnimation && frameIndex == 0)
                break;
                
            // 게임 오브젝트가 비활성화되면 루프 탈출
            if (!gameObject.activeInHierarchy)
                break;
        }
        
        Debug.Log("애니메이션 종료");
    }
    
    // 게임오버 패널 숨기기 함수
    public void HideGameOverPanel()
    {
        // 애니메이션 중지
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        // 패널 비활성화
        if (panelObject != null)
            panelObject.SetActive(false);
    }
    
    // 재시작 버튼 클릭 시 호출되는 함수
    public void OnRestartButtonClicked()
    {
        // 버튼 중복 클릭 방지
        if (isProcessingButton)
            return;
            
        isProcessingButton = true;
        
        // 게임매니저 찾아서 게임 재시작
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            // 먼저 패널 숨기기
            HideGameOverPanel();
            
            // 게임 재시작
            gameManager.RestartGame();
        }
    }
}