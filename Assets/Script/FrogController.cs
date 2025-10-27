using System.Collections;
using UnityEngine;

public class FrogController : MonoBehaviour
{
    [Header("Refs")]
    public TongueController tongue;   // 프리팹 자식에 붙은 TongueController
    public Transform mouth;           // 개구리 입 위치(없으면 자기 Transform)

    [Header("Tongue")]
    public float extendSpeed = 18f;
    public float retractSpeed = 24f;
    public float tongueThickness = 0.18f;
    public float extraPastScreen = 1.5f; // 화면 반대편을 넘어갈 추가 길이

    private bool fired = false;
    private Coroutine sequenceCo;

    private void Awake()
    {
        if (!tongue) tongue = GetComponentInChildren<TongueController>();
    }

    /// <summary>
    /// 스폰 후: waitBeforeEnter 동안 대기 → 화면 안쪽(innerMargin)까지 slide-in(enterDuration) → fireDelayAfterEnter 대기 → 혀 발사 → 복귀 후 자멸
    /// </summary>
    public void PrepareEnterAndFire(float waitBeforeEnter, float enterDuration, float innerMargin, float fireDelayAfterEnter)
    {
        if (sequenceCo != null) StopCoroutine(sequenceCo);
        sequenceCo = StartCoroutine(EnterAndFireSequence(waitBeforeEnter, enterDuration, innerMargin, fireDelayAfterEnter));
    }

    private IEnumerator EnterAndFireSequence(float waitBeforeEnter, float enterDuration, float innerMargin, float fireDelayAfterEnter)
    {
        // 0) 시작 대기
        float t = 0f;
        while (t < waitBeforeEnter)
        {
            if (!GameFlow.I || GameFlow.I.IsGameOver) yield break;
            t += Time.deltaTime;
            yield return null;
        }

        // 1) 화면 안쪽 목표 지점 계산(현재 위치가 어느 면 '밖'인지 보고, 그 면의 안쪽으로 살짝)
        Vector3 startPos = transform.position;
        Vector3 targetPos = ComputeInnerEdgeTarget(startPos, innerMargin);

        // 2) 슬라이드 인
        float dur = Mathf.Max(0.01f, enterDuration);
        t = 0f;
        while (t < dur)
        {
            if (!GameFlow.I || GameFlow.I.IsGameOver) yield break;
            float u = t / dur;
            transform.position = Vector3.Lerp(startPos, targetPos, u);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;

        // 3) 안쪽 도착 후 추가 지연(있다면)
        t = 0f;
        while (t < fireDelayAfterEnter)
        {
            if (!GameFlow.I || GameFlow.I.IsGameOver) yield break;
            t += Time.deltaTime;
            yield return null;
        }

        // 4) 혀 발사
        FireTongue();
    }

    /// <summary>
    /// 화면 안쪽 경계(inset by innerMargin) 라인 위 목표 위치를 계산
    /// (왼/오/상/하 바깥에서 들어왔다고 가정하고, 가장 가까운 바깥 면을 판정)
    /// </summary>
    private Vector3 ComputeInnerEdgeTarget(Vector3 fromWorld, float innerMargin)
    {
        Rect r = CameraBounds2D.I.GetWorldRect(0f);
        // 안쪽 경계(카메라 경계에서 margin만큼 안쪽)
        float xMinIn = r.xMin + innerMargin;
        float xMaxIn = r.xMax - innerMargin;
        float yMinIn = r.yMin + innerMargin;
        float yMaxIn = r.yMax - innerMargin;

        // 바깥쪽 거리 측정: 어떤 면 바깥인지 판정
        float dxLeft = Mathf.Abs(fromWorld.x - (r.xMin - 0.0001f));
        float dxRight = Mathf.Abs(fromWorld.x - (r.xMax + 0.0001f));
        float dyBot = Mathf.Abs(fromWorld.y - (r.yMin - 0.0001f));
        float dyTop = Mathf.Abs(fromWorld.y - (r.yMax + 0.0001f));

        // 가장 가까운(혹은 명확한) 바깥 면 선택
        // 우선순위: 실제로 바깥인지 체크 → 같은면이면 거리 작은 쪽
        bool isLeft = fromWorld.x < r.xMin;
        bool isRight = fromWorld.x > r.xMax;
        bool isBottom = fromWorld.y < r.yMin;
        bool isTop = fromWorld.y > r.yMax;

        // 기본값: 가장 가까운 면으로 스냅
        float best = float.MaxValue;
        Vector3 target = fromWorld;

        if (isLeft)
        {
            float yClamped = Mathf.Clamp(fromWorld.y, yMinIn, yMaxIn);
            target = new Vector3(xMinIn, yClamped, fromWorld.z);
            best = dxLeft;
        }
        if (isRight && dxRight < best)
        {
            float yClamped = Mathf.Clamp(fromWorld.y, yMinIn, yMaxIn);
            target = new Vector3(xMaxIn, yClamped, fromWorld.z);
            best = dxRight;
        }
        if (isTop && dyTop < best)
        {
            float xClamped = Mathf.Clamp(fromWorld.x, xMinIn, xMaxIn);
            target = new Vector3(xClamped, yMaxIn, fromWorld.z);
            best = dyTop;
        }
        if (isBottom && dyBot < best)
        {
            float xClamped = Mathf.Clamp(fromWorld.x, xMinIn, xMaxIn);
            target = new Vector3(xClamped, yMinIn, fromWorld.z);
            best = dyBot;
        }

        // 혹시 완전히 안쪽에 있는 경우(예외)엔 가장 가까운 테두리선으로 스냅
        if (!isLeft && !isRight && !isTop && !isBottom)
        {
            // 화면 안쪽이라면, 네 모서리까지 거리 비교 후 가장 가까운 변에 붙임
            float toLeft = Mathf.Abs(fromWorld.x - xMinIn);
            float toRight = Mathf.Abs(fromWorld.x - xMaxIn);
            float toTop = Mathf.Abs(fromWorld.y - yMaxIn);
            float toBot = Mathf.Abs(fromWorld.y - yMinIn);

            float minEdge = Mathf.Min(toLeft, toRight, toTop, toBot);
            if (minEdge == toLeft) target = new Vector3(xMinIn, Mathf.Clamp(fromWorld.y, yMinIn, yMaxIn), fromWorld.z);
            else if (minEdge == toRight) target = new Vector3(xMaxIn, Mathf.Clamp(fromWorld.y, yMinIn, yMaxIn), fromWorld.z);
            else if (minEdge == toTop) target = new Vector3(Mathf.Clamp(fromWorld.x, xMinIn, xMaxIn), yMaxIn, fromWorld.z);
            else target = new Vector3(Mathf.Clamp(fromWorld.x, xMinIn, xMaxIn), yMinIn, fromWorld.z);
        }

        return target;
    }

    public void FireTongue()
    {
        if (fired || !tongue) return;
        fired = true;

        Vector3 start = (mouth ? mouth.position : transform.position);
        Vector2 dir = ComputeFireDirection(start);
        float maxLen = ComputeMaxLengthPastScreen(start, dir, extraPastScreen);

        tongue.InitAndFire(start, dir, maxLen, extendSpeed, retractSpeed, tongueThickness, OnTongueFinished);
    }

    private Vector2 ComputeFireDirection(Vector3 start)
    {
        var bee = FindAnyObjectByType<BeeController>();
        if (!bee)
            return (Vector2)(CameraBounds2D.I.GetWorldRect().center - (Vector2)start).normalized;

        Vector2 dir = ((Vector2)bee.transform.position - (Vector2)start).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = Vector2.down;
        return dir;
    }

    private float ComputeMaxLengthPastScreen(Vector3 start, Vector2 dir, float extra)
    {
        var rect = CameraBounds2D.I.GetWorldRect(0f);
        float t = RayRectExitDistance((Vector2)start, dir, rect);
        return t + extra;
    }

    // 레이(시작, 방향)가 rect를 빠져나갈 때까지의 거리(간단 구현)
    private float RayRectExitDistance(Vector2 origin, Vector2 dir, Rect r)
    {
        float t = Mathf.Infinity;
        if (dir.x > 0) t = Mathf.Min(t, (r.xMax - origin.x) / dir.x);
        if (dir.x < 0) t = Mathf.Min(t, (r.xMin - origin.x) / dir.x);
        if (dir.y > 0) t = Mathf.Min(t, (r.yMax - origin.y) / dir.y);
        if (dir.y < 0) t = Mathf.Min(t, (r.yMin - origin.y) / dir.y);
        if (float.IsInfinity(t) || t < 0f) t = 5f; // 폴백
        return t;
    }

    private void OnTongueFinished()
    {
        Destroy(gameObject); // 혀 왕복 완료 시 개구리 제거
    }
}
