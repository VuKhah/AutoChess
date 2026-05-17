using UnityEngine;
using System.Collections;

public class CardVisuals : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Coroutine poseRoutine;
    private Vector3 settledScale = Vector3.one;

    private static readonly Quaternion UprightRotation = Quaternion.identity;
    private static readonly Quaternion BoardRotation = Quaternion.Euler(0f, 0f, 90f);

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void RefreshSettledScale() => settledScale = transform.localScale;

    public void SetUprightPose()
    {
        StopPoseRoutine();
        transform.localRotation = UprightRotation;
        RefreshSettledScale();
    }

    public void SetBoardPose()
    {
        StopPoseRoutine();
        transform.localRotation = BoardRotation;
        RefreshSettledScale();
    }

    public void PlayDeployToBoard()
    {
        StopPoseRoutine();
        poseRoutine = StartCoroutine(DeployToBoardAnimation());
    }

    private IEnumerator DeployToBoardAnimation()
    {
        RefreshSettledScale();

        Quaternion startRotation = transform.localRotation;
        Quaternion overshootRotation = Quaternion.Euler(0f, 0f, 102f);
        Vector3 liftScale = settledScale * 1.08f;
        RectTransform rect = transform as RectTransform;
        Vector2 settledPosition = rect != null ? rect.anchoredPosition : Vector2.zero;
        Vector2 liftedPosition = settledPosition + new Vector2(0f, 18f);

        yield return AnimatePose(startRotation, overshootRotation, settledScale, liftScale, settledPosition, liftedPosition, 0.13f);
        yield return AnimatePose(overshootRotation, BoardRotation, liftScale, settledScale, liftedPosition, settledPosition, 0.16f);

        transform.localRotation = BoardRotation;
        transform.localScale = settledScale;
        if (rect != null) rect.anchoredPosition = settledPosition;
        poseRoutine = null;
    }

    private IEnumerator AnimatePose(
        Quaternion fromRotation,
        Quaternion toRotation,
        Vector3 fromScale,
        Vector3 toScale,
        Vector2 fromPosition,
        Vector2 toPosition,
        float duration)
    {
        RectTransform rect = transform as RectTransform;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
            transform.localScale = Vector3.Lerp(fromScale, toScale, t);
            if (rect != null)
                rect.anchoredPosition = Vector2.Lerp(fromPosition, toPosition, t);
            yield return null;
        }
    }

    private void StopPoseRoutine()
    {
        if (poseRoutine == null) return;
        StopCoroutine(poseRoutine);
        poseRoutine = null;
    }

    // Hiệu ứng lao vào tấn công
    public IEnumerator AttackAnimation(Vector3 targetWorldPos, float duration)
    {
        // 1. Lưu lại vị trí Local ban đầu (thường là 0,0,0 vì nó nằm trong Slot)
        Vector3 originalLocalPos = Vector3.zero;
        Vector3 startWorldPos = transform.position;
        float elapsed = 0f;

        // 2. Lao lên tấn công (Sử dụng World Position để lao vào điểm giữa)
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startWorldPos, targetWorldPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. Rung lắc khi va chạm
        yield return StartCoroutine(ShakeEffect(0.1f, 5f));

        // 4. Quay về vị trí cũ (Sử dụng Local Position để ĐẢM BẢO nó về đúng tâm Slot)
        elapsed = 0f;
        Vector3 currentLocalPos = transform.localPosition;
        while (elapsed < duration)
        {
            // Ép nó về (0,0,0) của Slot cha
            transform.localPosition = Vector3.Lerp(currentLocalPos, originalLocalPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Chốt hạ lần cuối để tránh sai số
        transform.localPosition = Vector3.zero;
    }

    public IEnumerator ShakeEffect(float duration, float magnitude)
    {
        Vector3 pos = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.position = new Vector3(pos.x + x, pos.y + y, pos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = pos;
    }

    // Hiệu ứng nứt vỡ và biến mất
    public IEnumerator DieAnimation()
    {
        float elapsed = 0f;
        float duration = 0.5f;
        Vector3 initialScale = transform.localScale;

        // Hiệu ứng "Nứt vỡ": Đổi màu đỏ lợt và thu nhỏ dần
        CardUI ui = GetComponent<CardUI>();
        if (ui != null) ui.characterArt.color = new Color(1f, 0.5f, 0.5f); // Đỏ lợt

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            // Vừa thu nhỏ vừa làm mờ
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, percent);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, percent);

            yield return null;
        }

        // Sau khi diễn xong thì ẩn hẳn đi
        gameObject.SetActive(false);
    }
    // Hiệu ứng nảy to rồi thu về — dùng khi merge thành công
    public IEnumerator BurstAnimation()
    {
        Vector3 normal = transform.localScale;
        Vector3 big    = normal * 1.35f;
        float half = 0.12f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(normal, big, elapsed / half);
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(big, normal, elapsed / half);
            yield return null;
        }
        transform.localScale = normal;
        RefreshSettledScale();
    }

    public void ResetVisuals()
    {
        // 1. Hiện lại object
        gameObject.SetActive(true);

        // 2. Reset độ mờ (Alpha) và kích thước
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;
        transform.localScale = settledScale;

        // 3. Reset màu sắc ảnh nhân vật (nếu bị đổi sang màu đỏ lúc chết)
        CardUI ui = GetComponent<CardUI>();
        if (ui != null) ui.characterArt.color = Color.white;
    }
}
