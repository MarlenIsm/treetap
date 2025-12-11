using UnityEngine;

public class anim : MonoBehaviour
{
    [SerializeField] private Animator treeAnimator;

    // Метод вызывается при клике
    public void PlayClickAnimation()
    {
        treeAnimator.Play("TreeClick", 0, 0f);
    }
}
