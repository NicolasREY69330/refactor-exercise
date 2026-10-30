public static class ServiceLocator
{
    public static Dictionary<string, object> Services = new();

    public static void Register(string name, object service)
    {
        Services[name] = service;
    }

    public static T Get<T>(string name)
    {
        return (T)Services[name];
    }
}
