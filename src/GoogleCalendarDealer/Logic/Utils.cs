using System.Text;

namespace GoogleCalendarDealer.Logic;

public class Utils
{
    /// <summary>
    /// Transforms an Array to a string, where each element is separated by a new line.
    /// </summary>
    /// <param name="arr">The array to be transformed.</param>
    /// <returns>The string representing the array.</returns>
    public static string ArrayAsString<T>(List<T> arr)
    {
        var sb = new StringBuilder();
        arr.ForEach(element => sb.AppendLine(element?.ToString()));
        return sb.ToString();
    } 
}