using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Provides simple control over an Animator, including the ability
/// to skip directly to the end of a specified animation state.
/// </summary>
public class AnimationController : MonoBehaviour
{
    [Tooltip("Animator component controlling the animation.")]
    public Animator animator;

    /// <summary>
    /// Instantly jumps the specified animation state to its final frame.
    /// </summary>
    /// <param name="stateName">The name of the animation state to fast-forward.</param>
    public void SkipToEndOfAnimation(string stateName)
    {
        animator.Play(stateName, 0, 1f);
    }
}
