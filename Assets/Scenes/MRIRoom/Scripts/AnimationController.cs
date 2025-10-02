using UnityEngine;
using UnityEngine.Events;

public class AnimationController : MonoBehaviour
{
    public Animator animator;

    void Start()
    {

    }

    public void SkipToEndOfAnimation(string stateName)
    {
        // Forcer l'animation à sa fin
        animator.Play(stateName, 0, 1f);  // 1f : fin de l'animation
    }
}
