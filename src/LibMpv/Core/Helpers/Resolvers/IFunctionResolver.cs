namespace HanumanInstitute.LibMpv.Core;

/// <summary>Supports loading functions from native libraries. Provides a more flexible alternative to P/Invoke.</summary>
public interface IFunctionResolver
{
    /// <summary>Creates a delegate which invokes a native function.</summary>
    /// <typeparam name="T">The function delegate.</typeparam>
    /// <param name="libraryName">The library name which contains the function.</param>
    /// <param name="functionName">The name of the function for which to create the delegate.</param>
    /// <param name="throwOnError">Whether to throw an exception if library is not found or fails to load.</param>
    /// <returns>A new delegate which points to the native function.</returns>
    T? GetFunctionDelegate<T>(string libraryName, string functionName, bool throwOnError = true);
}
