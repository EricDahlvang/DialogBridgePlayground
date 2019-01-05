using System;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.Dialogs
{
    public abstract class PromptException : Exception
    {
        public PromptException(string message)
            : base(message) { }
        protected PromptException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    public sealed class TooManyAttemptsException : PromptException
    {
        public TooManyAttemptsException(string message)
            : base(message) { }
    }
}