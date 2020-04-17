using System;

namespace Ryujinx.Common
{
    public static class EnumUtils
    {
        public static T[] GetValues<T>() where T : Enum => (T[])Enum.GetValues(typeof(T));

        public static int GetCount<T>() where T : Enum => Enum.GetNames(typeof(T)).Length;
    }
}
