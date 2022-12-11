public abstract class BaseEndpoint
{
    protected Uri BasePath { get; set; }
    protected Uri Route { get; set; }
    protected Uri Action { get; set; }
    protected HttpVerb HttpVerb { get; set; }
    public abstract void Initialize();
}

public enum HttpVerb
{
    Get,
    Post,
    Put,
    Delete
}
