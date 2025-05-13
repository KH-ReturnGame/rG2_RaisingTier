using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public Slider forceSlider;
    public TextMeshProUGUI valueText;

    void Start()
    {
        forceSlider.minValue = 2f;
        forceSlider.maxValue = 10f;
        forceSlider.wholeNumbers = true;

        // 이전 설정 불러오기
        float saved = PlayerPrefs.GetFloat("ForceMultiplier", 8f);
        forceSlider.value = saved;
        valueText.text = saved.ToString("0");

        forceSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    public void OnSliderChanged(float val)
    {
        // 저장
        PlayerPrefs.SetFloat("ForceMultiplier", val);
        valueText.text = val.ToString("0");

        // 실행 중인 Launcher가 있으면 즉시 반영
        var launcher = FindObjectOfType<Launcher>();
        if (launcher != null)
            launcher.forceMultiplier = val;
    }
}
