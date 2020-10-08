using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDesktopWPF.Common
{
    public static class ExtensionMethod
    {
        // List<T> ForEach 확장메서드 (비동기 ForEach await 로 받기 위함)
        public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
        {
            foreach (var value in list)
            {
                await func(value);
            }
        }
    }
}
