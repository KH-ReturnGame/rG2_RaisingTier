using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class Launcher : MonoBehaviour
{
    public GameObject tierPrefab;
    public float maxForce = 20f;
    public float forceMultiplier = 2f;

    public int trajectoryPoints = 15;
    public float timeBetweenPoints = 0.1f;
    public float[] spawnProbabilities = { 0.5f, 0.3f, 0.15f, 0.05f };
    
    [Header("궤적 시각화 설정")]
    [Range(0f, 1f)]
    public float startAlpha = 1.0f;
    [Range(0f, 1f)]
    public float endAlpha = 0.0f;
    public Color trajectoryColor = Color.white;
    
    public Material trajectoryMaterial;
    public float lineWidth = 0.1f;

    LineRenderer lr;
    GameObject currentTier;
    Rigidbody2D currentTierRb;
    Vector2 startPos;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 0;
        lr.sortingOrder = 10;
        
        SetupLineRenderer();
        SetupTrajectoryGradient();
    }
    
    void SetupLineRenderer()
    {
        if (trajectoryMaterial == null)
        {
            trajectoryMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        
        lr.material = trajectoryMaterial;
        
        lr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
    }

    void Start()
    {
        startPos = transform.position;
        SpawnTier();
    }
    
    void SetupTrajectoryGradient()
    {
        Gradient gradient = new Gradient();
        
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0] = new GradientColorKey(trajectoryColor, 0f);
        colorKeys[1] = new GradientColorKey(trajectoryColor, 1f);
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(startAlpha, 0f);
        alphaKeys[1] = new GradientAlphaKey(endAlpha, 1f);
        
        gradient.SetKeys(colorKeys, alphaKeys);
        
        lr.colorGradient = gradient;
    }

    void Update()
    {
        if (currentTier == null) return;

        if (Input.GetMouseButton(0))
        {
            Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = startPos - (Vector2)mw;
            float force = Mathf.Min(dir.magnitude, maxForce) * forceMultiplier;
            DrawTrajectory(startPos, dir.normalized * force);
        }

        if (Input.GetMouseButtonUp(0))
            FireTier();
    }

    void SpawnTier()
    {
        if (tierPrefab == null)
        {
            Debug.LogError("프리팹이 없음");
            return;
        }

        currentTier = Instantiate(tierPrefab, startPos, Quaternion.identity);
        currentTierRb = currentTier.GetComponent<Rigidbody2D>();
        currentTierRb.bodyType = RigidbodyType2D.Kinematic;
        currentTierRb.gravityScale = 0f;
        currentTier.tag = "Tier";

        var tier = currentTier.GetComponent<Tier>();
        if (tier != null)
        {
            tier.level = GetRandomTierLevel();
            tier.UpdateSprite();
        }

        lr.positionCount = 0;
    }

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

    void FireTier()
    {
        currentTierRb.bodyType = RigidbodyType2D.Dynamic;
        currentTierRb.gravityScale = 1f;

        Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = startPos - (Vector2)mw;
        float force = Mathf.Min(dir.magnitude, maxForce) * forceMultiplier;
        currentTierRb.AddForce(dir.normalized * force, ForceMode2D.Impulse);

        float torque = -force * 10f;  
        currentTierRb.angularVelocity = -360f;

        lr.positionCount = 0;
        currentTier = null;
        currentTierRb = null;

        Invoke(nameof(SpawnTier), 0.5f);
    }

    void DrawTrajectory(Vector2 p0, Vector2 v0)
    {
        lr.positionCount = trajectoryPoints;
        Vector2 gravity = Physics2D.gravity;
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = timeBetweenPoints * i;
            Vector2 pt = p0 + v0 * t + 0.5f * gravity * t * t;
            lr.SetPosition(i, pt);
        }
    }
    
    void OnValidate()
    {
        if (lr != null)
        {
            SetupTrajectoryGradient();
        }
    }
}