using System.Collections;
using UnityEngine;

// Khi bắt đầu giai đoạn chuẩn bị: các card trong handSlots khởi đầu chụm lại ở giữa giàn
// rồi xòe đều sang hai bên về vị trí gốc, kèm xoay nhẹ + delay theo khoảng cách → hiệu ứng chia bài.
public class BottomZoneDeal : MonoBehaviour
{
    [SerializeField] private float duration    = 0.40f;
    [SerializeField] private float fanRotation = 10f;  // độ xoay mỗi slot lệch tâm
    [SerializeField] private float startScale  = 0.80f;
    [SerializeField] private float dealStagger = 0.06f; // delay giữa các lá tính từ giữa ra

    private Coroutine routine;

    public void Play()
    {
        if (GameManager.Instance == null || GameManager.Instance.handSlots == null) return;
        if (!gameObject.activeInHierarchy) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(DealRoutine());
    }

    private IEnumerator DealRoutine()
    {
        Transform[] slots = GameManager.Instance.handSlots;
        int n = slots.Length;

        // Chờ 1 frame để layout group cập nhật vị trí cuối
        yield return null;

        Transform[]  cards          = new Transform[n];
        Vector3[]    targetLocal    = new Vector3[n];
        Quaternion[] targetRot      = new Quaternion[n];
        Vector3[]    targetScaleArr = new Vector3[n];

        int activeCount = 0;
        Vector3 centerWorld = Vector3.zero;

        for (int i = 0; i < n; i++)
        {
            if (slots[i] == null || slots[i].childCount == 0) continue;
            cards[i]          = slots[i].GetChild(0);
            targetLocal[i]    = cards[i].localPosition;
            targetRot[i]      = cards[i].localRotation;
            targetScaleArr[i] = cards[i].localScale;

            centerWorld += slots[i].position;
            activeCount++;
        }
        if (activeCount == 0) { routine = null; yield break; }
        centerWorld /= activeCount;

        float midIdx = (n - 1) * 0.5f;

        Vector3[]    startLocal    = new Vector3[n];
        Quaternion[] startRot      = new Quaternion[n];
        Vector3[]    startScaleArr = new Vector3[n];
        float[]      startDelay    = new float[n];

        for (int i = 0; i < n; i++)
        {
            if (cards[i] == null) continue;
            startLocal[i]    = slots[i].InverseTransformPoint(centerWorld);
            float offset     = i - midIdx;
            startRot[i]      = Quaternion.Euler(0f, 0f, -offset * fanRotation) * targetRot[i];
            startScaleArr[i] = targetScaleArr[i] * startScale;
            startDelay[i]    = Mathf.Abs(offset) * dealStagger;

            cards[i].localPosition = startLocal[i];
            cards[i].localRotation = startRot[i];
            cards[i].localScale    = startScaleArr[i];
        }

        // Tổng thời gian: delay xa nhất + duration một lá
        float maxDelay = Mathf.Abs(midIdx) * dealStagger;
        float total    = maxDelay + duration;
        float elapsed  = 0f;

        while (elapsed < total)
        {
            elapsed += Time.deltaTime;
            for (int i = 0; i < n; i++)
            {
                if (cards[i] == null) continue;
                float local = elapsed - startDelay[i];
                if (local <= 0f) continue;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(local / duration));
                cards[i].localPosition = Vector3.Lerp(startLocal[i], targetLocal[i], t);
                cards[i].localRotation = Quaternion.Slerp(startRot[i], targetRot[i], t);
                cards[i].localScale    = Vector3.Lerp(startScaleArr[i], targetScaleArr[i], t);
            }
            yield return null;
        }

        for (int i = 0; i < n; i++)
        {
            if (cards[i] == null) continue;
            cards[i].localPosition = targetLocal[i];
            cards[i].localRotation = targetRot[i];
            cards[i].localScale    = targetScaleArr[i];
        }
        routine = null;
    }
}
