namespace MongoDB.Client.Unused
{
    /// <summary>
    /// The cursor type.
    /// </summary>
    public enum CursorType
    {
        /// <summary>
        /// A non-tailable cursor. This is sufficient for most uses.
        /// </summary>
        NonTailable = 0,
        /// <summary>
        /// A tailable cursor.
        /// </summary>
        Tailable,
        /// <summary>
        /// A tailable cursor with a built-in server sleep.
        /// </summary>
        TailableAwait
    }
}
