using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("Health Icons")]
    [SerializeField] private GameObject heartIconPrefab;
    [SerializeField] private Transform heartContainer;
    [SerializeField] private float iconSpacing = 50f;

    private List<GameObject> heartIcons = new List<GameObject>();

    private void Start()
    {
        if (BeeHealth.I != null)
        {
            // 초기 HP에 맞춰 하트 아이콘 생성
            InitializeHearts(BeeHealth.I.GetCurrentHealth());

            // HP 변경 이벤트 등록
            BeeHealth.I.OnHealthChanged.AddListener(UpdateHearts);
        }
    }

    private void InitializeHearts(int health)
    {
        if (heartIconPrefab == null || heartContainer == null) return;

        // 기존 하트 제거
        foreach (var heart in heartIcons)
        {
            if (heart != null) Destroy(heart);
        }
        heartIcons.Clear();

        // 새로운 하트 생성
        for (int i = 0; i < health; i++)
        {
            GameObject heart = Instantiate(heartIconPrefab, heartContainer);
            heartIcons.Add(heart);

            // 위치 조정 (선택 사항)
            RectTransform rt = heart.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(i * iconSpacing, 0);
            }
        }
    }

    private void UpdateHearts(int currentHealth)
    {
        if (heartIconPrefab == null || heartContainer == null) return;

        // 현재 HP보다 하트가 많으면 제거
        while (heartIcons.Count > currentHealth)
        {
            int lastIndex = heartIcons.Count - 1;
            if (heartIcons[lastIndex] != null)
            {
                Destroy(heartIcons[lastIndex]);
            }
            heartIcons.RemoveAt(lastIndex);
        }

        // 현재 HP보다 하트가 적으면 추가
        while (heartIcons.Count < currentHealth)
        {
            GameObject heart = Instantiate(heartIconPrefab, heartContainer);
            heartIcons.Add(heart);

            // 위치 조정
            RectTransform rt = heart.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2((heartIcons.Count - 1) * iconSpacing, 0);
            }
        }
    }

    private void OnDestroy()
    {
        if (BeeHealth.I != null)
        {
            BeeHealth.I.OnHealthChanged.RemoveListener(UpdateHearts);
        }
    }
}