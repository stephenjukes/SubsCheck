namespace SubsCheck.Models;
public class WriteRequest<T>
{
    public IEnumerable<T> Data { get; set; }

    public string ResourceLocator { get; set; }
}
