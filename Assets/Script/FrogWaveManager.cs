using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogWaveManager : MonoBehaviour
{
    [Header("Wave Count Settings")]
    [Tooltip("첫 웨이브 개구리 수")]
    public int startFrogs = 3;

    [Tooltip("웨이브마다 증가할 개구리 수(1씩 증가)")]
    public int frogIncrementPerWave = 1;

    [Header("Timing")]
    [Tooltip("게임 시작 후 첫 웨이브까지 지연")]
    public float waveStartDelay = 0.2f;

    [Tooltip("웨이브와 웨이브 사이 휴식(선택)")]
    public float interWaveDelay = 0.3f;

    [Header("Enter (밖→안 이동)")]
    [Tooltip("스폰 직후 대기 시간(요청: 0.3초 권장)")]
    public float waitBeforeEnter = 0.3f;

    [Tooltip("화면 밖에서 테두리 안쪽으로 슬라이드 인하는 데 걸리는 시간")]
    public float enterDuration = 0.25f;

    [Tooltip("화면 테두리 안쪽으로 들어올 최종 위치 마진(안쪽 여백)")]
    public float innerMargin = 0.5f;

    [Tooltip("안쪽 도착 후 혀 발사까지 추가 지연(0이면 즉시 발사)")]
    public float fireDelayAfterEnter = 0.0f;

    [Header("Spawn")]
    public GameObject frogPrefab;

    [Tooltip("화면 밖(가장자리 바깥) 스폰 여유 거리")]
    public float edgeOffset = 1.0f;

    [Tooltip("개구리 간 최소 간격(겹침 방지)")]
    public float minFrogSpacing = 2.0f;

    [Tooltip("개구리 한 마리 배치 시도 횟수(간격 조건 만족용)")]
    public int placeAttemptsPerFrog = 30;

    private readonly List<FrogController> aliveFrogs = new List<FrogController>();

    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        // 게임 시작 대기
        while (!GameFlow.I || !GameFlow.I.IsRunning) yield return null;
        yield return new WaitForSeconds(waveStartDelay);

        int waveIndex = 0;

        // === 무한 웨이브 루프 ===
        while (true)
        {
            if (GameFlow.I.IsGameOver) yield break;

            int countThisWave = Mathf.Max(0, startFrogs + frogIncrementPerWave * waveIndex);
            aliveFrogs.Clear();

            // 1) 개구리 스폰(카메라 4면 '밖' + 간격 보장)
            Rect rect = CameraBounds2D.I.GetWorldRect(0f);
            List<Vector2> placed = new List<Vector2>();

            for (int i = 0; i < countThisWave; i++)
            {
                if (TryPlaceFrog(rect, edgeOffset, minFrogSpacing, placeAttemptsPerFrog, placed, out Vector2 pos))
                {
                    var frog = Instantiate(frogPrefab, pos, Quaternion.identity).GetComponent<FrogController>();
                    aliveFrogs.Add(frog);
                    placed.Add(pos);

                    // 2) 각 개구리: 0.3초 대기 → 화면 안쪽으로 슬라이드 인 → 발사 스케줄
                    frog.PrepareEnterAndFire(waitBeforeEnter, enterDuration, innerMargin, fireDelayAfterEnter);
                }
                else
                {
                    Debug.LogWarning("[FrogWave] 위치 배치 실패 -> minFrogSpacing↓ 또는 placeAttemptsPerFrog↑ 조정 권장");
                }
            }

            // 3) 모든 개구리 제거될 때까지 대기(= 혀 왕복 완료 후 FrogController가 자멸)
            while (aliveFrogs.Exists(f => f != null))
            {
                if (GameFlow.I.IsGameOver) yield break;
                yield return null;
            }

            // 4) 웨이브 종료 지연(선택)
            if (interWaveDelay > 0f)
                yield return new WaitForSeconds(interWaveDelay);

            waveIndex++; // 다음 웨이브(개구리 +1)
        }
    }

    // --- 배치 유틸 --- //
    private bool TryPlaceFrog(Rect camRect, float offset, float minSpacing, int attempts, List<Vector2> placed, out Vector2 pos)
    {
        // 4면을 골고루 활용하기 위해 시도마다 랜덤 면 선택 (바깥쪽으로)
        for (int a = 0; a < attempts; a++)
        {
            int side = Random.Range(0, 4); // 0:Left 1:Right 2:Top 3:Bottom
            pos = RandomPointOnSideOutside(camRect, side, offset);

            bool ok = true;
            for (int i = 0; i < placed.Count; i++)
            {
                if (Vector2.Distance(placed[i], pos) < minSpacing)
                {
                    ok = false;
                    break;
                }
            }
            if (ok) return true;
        }

        pos = Vector2.zero;
        return false;
    }

    private Vector2 RandomPointOnSideOutside(Rect r, int side, float offset)
    {
        switch (side)
        {
            case 0: // Left(왼쪽 바깥)
                return new Vector2(r.xMin - offset, Random.Range(r.yMin, r.yMax));
            case 1: // Right(오른쪽 바깥)
                return new Vector2(r.xMax + offset, Random.Range(r.yMin, r.yMax));
            case 2: // Top(위쪽 바깥)
                return new Vector2(Random.Range(r.xMin, r.xMax), r.yMax + offset);
            default: // Bottom(아래쪽 바깥)
                return new Vector2(Random.Range(r.xMin, r.xMax), r.yMin - offset);
        }
    }
}
