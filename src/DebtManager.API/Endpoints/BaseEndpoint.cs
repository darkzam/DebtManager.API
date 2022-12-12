public abstract class BaseEndpoint<T> where T : class
{
    protected Uri BasePath { get; set; }
    protected Uri Route { get; set; }
    protected Uri Action { get; set; }
    protected HttpVerb HttpVerb { get; set; }
    protected WebApplication WebApplication { get; set; }
    public abstract void Initialize();

    protected BaseEndpoint(Uri baseRoute,
                           WebApplication webApplication)
    {
        BasePath = baseRoute ?? throw new ArgumentNullException(nameof(baseRoute));
        Action = new Uri(nameof(T), UriKind.Relative);
        Route = new Uri($"{BasePath.ToString()}{Action.ToString()}", UriKind.Relative);
        HttpVerb = HttpVerb.Get;
        WebApplication = webApplication ?? throw new ArgumentNullException(nameof(webApplication));
    }
}

public enum HttpVerb
{
    Get,
    Post,
    Put,
    Delete
}
