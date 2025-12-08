using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    /// <summary>
    /// Utility for multicasting 3rd party delegates. Invokes all delegates, even when some of them throw exceptions.
    /// Exceptions are logged. Delegates are invoked on the main thread.
    /// </summary>
    static internal class SafeInvoker
    {
        public static void Invoke(Action eventToInvoke)
        {
            if (eventToInvoke == null)
                return;

            var invocationList = eventToInvoke.GetInvocationList();

            foreach (var invocation in invocationList)
            {
                var invocationTyped = invocation as Action;
                try
                {
                    invocationTyped?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
        }

        public static void Invoke<TArg0>(Action<TArg0> eventToInvoke, TArg0 arg0)
        {
            if (eventToInvoke == null)
                return;

            var invocationList = eventToInvoke.GetInvocationList();
            foreach (var invocation in invocationList)
            {
                var invocationTyped = invocation as Action<TArg0>;
                try
                {
                    invocationTyped?.Invoke(arg0);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
        }

        public static void Invoke<TArg0, TArg1>(Action<TArg0, TArg1> eventToInvoke, TArg0 arg0, TArg1 arg1)
        {
            if (eventToInvoke == null)
                return;

            var invocationList = eventToInvoke.GetInvocationList();
            foreach (var invocation in invocationList)
            {
                var invocationTyped = invocation as Action<TArg0, TArg1>;
                try
                {
                    invocationTyped?.Invoke(arg0, arg1);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
        }

        public static void Invoke<TArg0, TArg1, TArg2>(Action<TArg0, TArg1, TArg2> eventToInvoke, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            if (eventToInvoke == null)
                return;

            var invocationList = eventToInvoke.GetInvocationList();
            foreach (var invocation in invocationList)
            {
                var invocationTyped = invocation as Action<TArg0, TArg1, TArg2>;
                try
                {
                    invocationTyped?.Invoke(arg0, arg1, arg2);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
        }

        public async static Task InvokeOnMainThread(Action eventToInvoke)
        {
            await Awaitable.MainThreadAsync();
            Invoke(eventToInvoke);
        }

        public async static Task InvokeOnMainThread<TArg0>(Action<TArg0> eventToInvoke, TArg0 arg0)
        {
            await Awaitable.MainThreadAsync();
            Invoke(eventToInvoke, arg0);
        }

        public async static Task InvokeOnMainThread<TArg0, TArg1>(Action<TArg0, TArg1> eventToInvoke, TArg0 arg0, TArg1 arg1)
        {
            await Awaitable.MainThreadAsync();
            Invoke(eventToInvoke, arg0, arg1);
        }

        public async static Task InvokeOnMainThread<TArg0, TArg1, TArg2>(Action<TArg0, TArg1, TArg2> eventToInvoke, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            await Awaitable.MainThreadAsync();
            Invoke(eventToInvoke, arg0, arg1, arg2);
        }
    }
}
