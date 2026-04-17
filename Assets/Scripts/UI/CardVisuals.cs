using UnityEngine;
using System.Collections;

public class CardVisuals : MonoBehaviour
{
    private Vector3 originalPosition;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
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
    public void ResetVisuals()
    {
        // 1. Hiện lại object
        gameObject.SetActive(true);

        // 2. Reset độ mờ (Alpha) và kích thước
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;
        transform.localScale = Vector3.one;

        // 3. Reset màu sắc ảnh nhân vật (nếu bị đổi sang màu đỏ lúc chết)
        CardUI ui = GetComponent<CardUI>();
        if (ui != null) ui.characterArt.color = Color.white;
    }
}