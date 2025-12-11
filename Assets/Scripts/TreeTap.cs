using UnityEngine;
using UnityEngine.EventSystems;

public class TreeTap : MonoBehaviour, IPointerDownHandler
{
    public GameManager gameManager;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (gameManager != null)
        {
            gameManager.AddCoin();
        }
    }
}
