using System.Collections.Generic;
using System.Linq;

namespace Unity.PlatformToolkit
{
    /// <summary>
    /// Combination of multiple instances of <see cref="ILifetimeToken"/>, use it when a dependency on multiple tokens is required.
    /// </summary>
    internal class CombinedAnyLifetimeToken : ILifetimeToken
    {
        private readonly IEnumerable<ILifetimeToken> m_Tokens;

        /// <summary>
        /// Instantiates new <see cref="CombinedAnyLifetimeToken"/>
        /// </summary>
        /// <param name="tokens">Tokens to combine. It's safe to pass null tokens, as they are ignored. Null tokens are tolerated,
        /// because it's convenient to have an optional token in generic implementations. For example the <see cref="AbstractAchievementSystem"/>
        /// takes an optional token. On platforms where the achievement system is dependent on an account that never signs out, this token can be ignored.
        /// </param>
        public CombinedAnyLifetimeToken(params ILifetimeToken[] tokens)
        {
            m_Tokens = tokens.Where(t => t != null);
        }

        public bool Disposed => m_Tokens.Any(t => t.Disposed);

        /// <summary>
        /// Calls <see cref="ILifetimeToken.ThrowOnDisposedAccess"/> on all combined tokens.
        /// </summary>
        public void ThrowOnDisposedAccess()
        {
            foreach (var token in m_Tokens)
            {
                token.ThrowOnDisposedAccess();
            }
        }
    }
}
