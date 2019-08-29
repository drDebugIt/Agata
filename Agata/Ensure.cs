using System;

namespace Agata
{
    internal static class Ensure
    {
        internal static T NotNull<T>(T value, string argName, string msg = null) where T : class
        {
            if (ReferenceEquals(value, null))
            {
                throw new ArgumentNullException(
                    msg ?? "Argument can't be null.",
                    argName);
            }

            return value;
        }
        
        internal static string NotBlank(string str, string argName, string msg = null)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException(
                    msg ?? "Argument can't be null or blank string.",
                    argName);
            }

            return str;
        }

        internal static void That(bool condition, string argName, string msg)
        {
            if (!condition)
            {
                throw new ArgumentException(msg, argName);
            }
        }
    }
}