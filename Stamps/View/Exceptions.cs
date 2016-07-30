using System;

namespace Stamps.View
{
    public static class Exceptions
    {
        public static T Throw<T>(Exception e)
        {
            throw e;
        }

        public static T Throw<T>(this Object obj, Exception e)
        {
            throw e;
        }
    }
}
