namespace Data.Interfaces
{
    /// <summary>
    /// Represents the outcome of a repository operation, including:
    /// - <see cref="Succeeded"/>: whether the operation was successful,
    /// - <see cref="StatusCode"/>: an HTTP-style status code,
    /// - <see cref="Result"/>: the returned data when successful,
    /// - <see cref="Error"/>: an error message when not successful.
    /// </summary>
    public class RepositoryResult<T>
    {
        public bool Succeeded { get; set; }
        public int StatusCode { get; set; }
        public T? Result { get; set; }
        public string? Error { get; set; }
    }
}
