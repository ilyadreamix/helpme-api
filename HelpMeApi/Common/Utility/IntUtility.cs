namespace HelpMeApi.Common.Utility;

public static class IntUtility
{
    public static int SafeOffset(this int offset) =>
        offset >= 0 ? offset : 0;
    
    public static int SafeSize(this int size, int needSize = 25) =>
        size <= needSize && size > 0 ? size : needSize;
}
