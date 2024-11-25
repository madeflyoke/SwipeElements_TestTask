using System;

namespace Utility
{
    public static class StringHelpers
    {
        public static string BuildDataNameByType<T>(string optionalPostfix=default)
        {
            var name = typeof(T).Name;
            if (optionalPostfix!=default)
            {
                optionalPostfix = optionalPostfix.Replace("_", String.Empty);
                name += "_" + optionalPostfix;
            }

            return name;
        }
    }
}
