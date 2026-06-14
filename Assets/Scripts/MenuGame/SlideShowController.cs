using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlidingSlideshow : MonoBehaviour
{
    [Header("Images")]
    public Image currentImage;
    public Image nextImage;

    [Header("Slides")]
    public Sprite[] slides;

    [Header("Settings")]
    public float stayTime = 3f;
    public float slideDuration = 0.8f;

    private int currentIndex = 0;

    private RectTransform currentRect;
    private RectTransform nextRect;

    private float width;

    void Start()
    {
        if (slides.Length == 0) return;

        currentRect = currentImage.GetComponent<RectTransform>();
        nextRect = nextImage.GetComponent<RectTransform>();

        width = currentRect.rect.width;

        currentImage.sprite = slides[0];

        StartCoroutine(SlideLoop());
    }

    IEnumerator SlideLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(stayTime);

            int nextIndex = (currentIndex + 1) % slides.Length;

            nextImage.sprite = slides[nextIndex];

            currentRect.anchoredPosition = Vector2.zero;
            nextRect.anchoredPosition = new Vector2(width, 0);

            float t = 0;

            while (t < slideDuration)
            {
                t += Time.deltaTime;

                float progress = t / slideDuration;

                currentRect.anchoredPosition =
                    Vector2.Lerp(
                        Vector2.zero,
                        new Vector2(-width, 0),
                        progress);

                nextRect.anchoredPosition =
                    Vector2.Lerp(
                        new Vector2(width, 0),
                        Vector2.zero,
                        progress);

                yield return null;
            }

            currentImage.sprite = slides[nextIndex];

            currentRect.anchoredPosition = Vector2.zero;

            currentIndex = nextIndex;
        }
    }
}