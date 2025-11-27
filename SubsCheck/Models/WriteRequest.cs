namespace SubsCheck.Models;

public class WriteRequest<TData, TError>
{
    public IEnumerable<TData> Data { get; set; }

    public string ResourceLocator { get; set; }

    public List<TError> Errors { get; set; }
}
