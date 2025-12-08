#if PT_NUNIT_PRESENT && UNITY_INCLUDE_TESTS
#define PT_ENABLE_EXCEPTION_TESTING
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;

#if PT_ENABLE_EXCEPTION_TESTING
using NUnit.Framework;
#endif

namespace Unity.PlatformToolkit
{
    internal enum ExceptionPoint
    {
        /// <summary>
        /// Thrown in file write or file delete operations. Place after data is changed, but before the internal commit is completed.
        /// </summary>
        PreCommit,
        /// <summary>
        /// Thrown after modifying a file fails, to simulate deletion of an empty save in case of an exception. Place after a commit has failed
        /// </summary>
        NewSaveCommitFailureCleanup,


        /// <summary>
        ///
        /// </summary>
        AchievementSystemInitialize,

        /// <summary>
        /// Thrown in <exception cref="AbstractAchievementSystem{T}.FetchNativeAchievementData"></exception>
        /// </summary>
        AchievementDataFetch
    }

    internal static class ExceptionTesting
    {
        private static readonly HashSet<ExceptionPoint> m_EnabledExceptionPoints = new HashSet<ExceptionPoint>();
        private static readonly HashSet<ExceptionPoint> m_ArmedExceptionPoints = new HashSet<ExceptionPoint>();
        private static readonly HashSet<ExceptionPoint> m_TriggerredExceptionPoints = new HashSet<ExceptionPoint>();

#if PT_ENABLE_EXCEPTION_TESTING
        [Conditional("UNITY_INCLUDE_TESTS")]
        public static void EnableExceptionPoint(ExceptionPoint exceptionPoint)
        {
            m_EnabledExceptionPoints.Add(exceptionPoint);
        }

        [Conditional("UNITY_INCLUDE_TESTS")]
        public static void IgnoreIfExceptionPointDisabled(ExceptionPoint exceptionPoint)
        {
            if (!m_EnabledExceptionPoints.Contains(exceptionPoint))
                Assert.Ignore();
        }

        public static IDisposable ArmExceptionPoint(ExceptionPoint exceptionPoint)
        {
            if (!m_EnabledExceptionPoints.Contains(exceptionPoint))
                Assert.Fail("Attempting to arm an exception point, that is not supported by the platform. Call IgnoreIfExceptionPointDisabled prior to ignore the test.");

            m_ArmedExceptionPoints.Add(exceptionPoint);
            return new ArmExceptionPointCleanUp(exceptionPoint);
        }
#endif

        [Conditional("UNITY_INCLUDE_TESTS")]
        public static void TriggerException(ExceptionPoint exceptionPoint)
        {
            if (m_ArmedExceptionPoints.Contains(exceptionPoint))
            {
                m_TriggerredExceptionPoints.Add(exceptionPoint);
                throw new Exception();
            }
        }

#if PT_ENABLE_EXCEPTION_TESTING
        [Conditional("UNITY_INCLUDE_TESTS")]
        private static void DisarmExceptionPoint(ExceptionPoint exceptionPoint)
        {
            try
            {
                if (!m_TriggerredExceptionPoints.Contains(exceptionPoint))
                    Assert.Fail("Exception was not triggered. This is almost certainly a bug. Make sure that TriggerException is called at an expected point.");
            }
            finally
            {
                m_ArmedExceptionPoints.Remove(exceptionPoint);
                m_TriggerredExceptionPoints.Remove(exceptionPoint);
            }
        }
#endif

#if PT_ENABLE_EXCEPTION_TESTING
        private class ArmExceptionPointCleanUp : IDisposable
        {
            private readonly ExceptionPoint m_ExceptionPoint;

            public ArmExceptionPointCleanUp(ExceptionPoint exceptionPoint)
            {
                m_ExceptionPoint = exceptionPoint;
            }

            public void Dispose()
            {
                DisarmExceptionPoint(m_ExceptionPoint);
            }
        }
#endif
    }
}
