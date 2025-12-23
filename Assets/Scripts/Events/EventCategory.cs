namespace Streets.Events
{
    /// <summary>
    /// Categories of events that can occur on the road.
    /// Events of different categories can overlap (e.g., Atmospheric + Creature).
    /// </summary>
    public enum EventCategory
    {
        /// <summary>
        /// Ambient scares: fog thickens, lights flicker, whispers, temperature drops.
        /// Can overlap with everything.
        /// </summary>
        Atmospheric,

        /// <summary>
        /// Hostile entities: stalkers, things crossing the road, pursuers.
        /// Can overlap with Atmospheric.
        /// </summary>
        Creature,

        /// <summary>
        /// Physical blockers: roadblocks, car pile-ups, broken bridges.
        /// Can overlap with Atmospheric.
        /// </summary>
        Obstacle,

        /// <summary>
        /// Visual disturbances: figures in distance, shadows, hallucinations.
        /// Can overlap with Atmospheric and Creature.
        /// </summary>
        Apparition
    }

    /// <summary>
    /// Difficulty tiers for events. Higher sanity = easier events.
    /// </summary>
    public enum EventDifficulty
    {
        /// <summary>
        /// Pure atmosphere, no threat (e.g., distant sound, shadow flicker)
        /// </summary>
        Harmless = 0,

        /// <summary>
        /// Creepy but minor threat (e.g., brief sighting, eerie sound nearby)
        /// </summary>
        Unsettling = 1,

        /// <summary>
        /// Moderate threat requiring attention (e.g., creature nearby, small obstacle)
        /// </summary>
        Dangerous = 2,

        /// <summary>
        /// Serious threat (e.g., aggressive creature, major obstacle)
        /// </summary>
        Terrifying = 3,

        /// <summary>
        /// Extreme threat, high chance of death (e.g., fast pursuer, trap)
        /// </summary>
        Nightmare = 4
    }
}
