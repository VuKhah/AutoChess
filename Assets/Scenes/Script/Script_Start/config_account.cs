using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class config_account : MonoBehaviour
{
    [SerializeField] private Image account_img;
    [SerializeField] private GameObject open_account_config;
    [SerializeField] private Button play_btn;

    void Start()
    {
        account_img.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Nếu đang mở bảng
            if (account_img.gameObject.activeSelf)
            {
                // Nếu click vào UI (account_img)
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return; // Không tắt
                }

                // Nếu click ra ngoài UI
                account_img.gameObject.SetActive(false);
                play_btn.gameObject.SetActive(true);
            }

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject == open_account_config)
                {
                    account_img.gameObject.SetActive(true);
                    play_btn.gameObject.SetActive(false);
                }
            }
        }
    }

}