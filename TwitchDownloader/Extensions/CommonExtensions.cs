namespace TwitchDownloader.Extensions;

public static class CommonExtensions
{
    public static T As<T>(this object obj)
    {
        if (obj is not T castedObj)
            throw new InvalidCastException();

        return castedObj;
    }
}