namespace HanumanInstitute.LibMpv;

/// <summary>
/// Represents an operation on MPV list options.
/// </summary>
public enum ListOptionOperation
{
    /// <summary>
    /// Set a list of items (using the list separator, interprets escapes).
    /// </summary>
    Set,
    /// <summary>
    /// Append single item (does not interpret escapes).
    /// </summary>
    Append,
    /// <summary>
    /// Append 1 or more items (same syntax as -set).
    /// </summary>
    Add,
    /// <summary>
    /// Prepend 1 or more items (same syntax as -set).
    /// </summary>
    Pre,
    /// <summary>
    /// Clear the option (remove all items).
    /// </summary>
    Clr,
    /// <summary>
    /// Delete item if present (does not interpret escapes).
    /// </summary>
    Remove,
    /// <summary>
    /// Delete 1 or more items by integer index (deprecated).
    /// </summary>
    Del,
    /// <summary>
    /// Append an item, or remove if if it already exists (no escapes).
    /// </summary>
    Toggle
}