/// <summary>
/// Tags each dialogue choice option with its empathy classification.
/// Used by CheckpointInteraction to report the player's choice to CheckpointManager
/// for Metric 2 (Visual Clarification) and Metric 3 (Positive Reinforcement).
/// </summary>
public enum ChoiceCategory
{
    /// <summary>No specific empathy classification.</summary>
    Neutral,

    /// <summary>Metric 2: Player uses visual/concrete language, leveraging DS strength (Observation).</summary>
    HighEmpathy,

    /// <summary>Metric 2: Player uses abstract/direct language, forcing DS weakness (Abstract processing).</summary>
    LowEmpathy,

    /// <summary>Metric 3: Player validates feelings or reframes failure positively.</summary>
    PositiveReinforcement,

    /// <summary>Metric 3: Player expresses urgency, blame, or impatience.</summary>
    NegativeReinforcement
}
