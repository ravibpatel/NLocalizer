namespace NLocalizer
{
    /// <summary>
    /// Used in <see cref="TranslationProperties"/> TranslationProperties class
    /// </summary>
    public class TranslationProperty
    {
        /// <summary>
        /// Translation message.
        /// </summary>
        public string Message;

        /// <summary>
        /// Is translation message property static?
        /// </summary>
        public bool IsStatic;

        /// <summary>
        /// Create TranslationProperty class
        /// </summary>
        /// <param name="message">Translation message.</param>
        /// <param name="isStatic">Is translation message property static?</param>
        public TranslationProperty(string message, bool isStatic)
        {
            Message = message;
            IsStatic = isStatic;
        }
    }
}
