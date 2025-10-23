namespace SubsCheck.Models;

public class WriteRequest<T>
{
    public T Data { get; set; }

    public string ResourceLocator { get; set; }
}
