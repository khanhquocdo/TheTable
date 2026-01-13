using System;
using UnityEngine;

/// <summary>
/// Abstract base class for all quest steps.
/// Each step represents a single objective that must be completed.
/// Steps subscribe to events in Activate() and unsubscribe in Complete().
/// </summary>
public abstract class QuestStep : MonoBehaviour
{
    [Header("Step Info")]
    public string stepID;
    public string stepDescription;

    protected bool isCompleted = false;

    /// <summary>
    /// Check if this step has been completed
    /// </summary>
    public bool IsCompleted => isCompleted;

    /// <summary>
    /// Called when this step becomes active.
    /// Override to subscribe to relevant events.
    /// </summary>
    public virtual void Activate()
    {
        isCompleted = false;
        OnActivate();
    }

    /// <summary>
    /// Called when this step is completed.
    /// Override to unsubscribe from events.
    /// </summary>
    public virtual void Complete()
    {
        if (isCompleted) return;

        isCompleted = true;
        OnComplete();
    }

    /// <summary>
    /// Override this method to handle step activation logic
    /// </summary>
    protected abstract void OnActivate();

    /// <summary>
    /// Override this method to handle step completion logic
    /// </summary>
    protected abstract void OnComplete();

    /// <summary>
    /// Call this method from child classes to mark the step as complete
    /// </summary>
    protected void FinishStep()
    {
        if (!isCompleted)
        {
            Complete();
            QuestManager.Instance?.HandleStepCompleted(this);
        }
    }
}
