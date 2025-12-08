using System;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    internal static class AccountErrorHandling
    {
        public static string HandleGetNameException(Exception e)
        {
            if (e is InvalidAccountException)
            {
                throw e;
            }
            else
            {
#if DEVELOPMENT_BUILD
                Debug.LogException(e);
#endif
                return string.Empty;
            }
        }

        public static Texture2D HandleGetPictureException(Exception e)
        {
            if (e is InvalidAccountException)
            {
                throw e;
            }
            else
            {
#if DEVELOPMENT_BUILD
                Debug.LogException(e);
#endif
                return null;
            }
        }
    }
}
