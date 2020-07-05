using System;

namespace GetText
{
    /// <summary>
    /// Represents a Gettext catalog instance.
    /// </summary>
    public interface ICatalog
    {
        /// <summary>
        /// Returns <paramref name="text"/> translated into the selected language.
        /// Similar to <c>gettext</c> function.
        /// </summary>
        /// <param name="text">Text to translate.</param>
        /// <returns>Translated text.</returns>
        string GetString(FormattableStringAdapter text);

        /// <summary>
        /// Returns <paramref name="text"/> translated into the selected language.
        /// Similar to <c>gettext</c> function.
        /// </summary>
        /// <param name="text">Text to translate.</param>
        /// <returns>Translated text.</returns>
        string GetString(FormattableString text);

        /// <summary>
        /// Returns <paramref name="text"/> translated into the selected language.
        /// Similar to <c>gettext</c> function.
        /// </summary>
        /// <param name="text">Text to translate.</param>
        /// <param name="args">Optional arguments for <see cref="string.Format(string, object[])"/> method.</param>
        /// <returns>Translated text.</returns>
        string GetString(FormattableStringAdapter text, params object[] args);

        /// <summary>
        /// Returns the plural form for <paramref name="n"/> of the translation of <paramref name="text"/>.
        /// Similar to <c>gettext</c> function.
        /// </summary>
        /// <param name="text">Singular form of message to translate.</param>
        /// <param name="pluralText">Plural form of message to translate.</param>
        /// <param name="n">Value that determines the plural form.</param>
        /// <returns>Translated text.</returns>
        string GetPluralString(FormattableStringAdapter text, FormattableStringAdapter pluralText, long n);

        /// <summary>
        /// Returns the plural form for <paramref name="n"/> of the translation of <paramref name="text"/>.
        /// Similar to <c>gettext</c> function.
        /// </summary>
        /// <param name="text">Singular form of message to translate.</param>
        /// <param name="pluralText">Plural form of message to translate.</param>
        /// <param name="n">Value that determines the plural form.</param>
        /// <returns>Translated text.</returns>
        string GetPluralString(FormattableString text, FormattableString pluralText, long n);

        /// <summary>
        /// Returns the plural form for <paramref name="n"/> of the translation of <paramref name="text"/>.
        /// Similar to <c>gettext</c> function.
        /// </summary>
        /// <param name="text">Singular form of message to translate.</param>
        /// <param name="pluralText">Plural form of message to translate.</param>
        /// <param name="n">Value that determines the plural form.</param>
        /// <param name="args">Optional arguments for <see cref="string.Format(string, object[])"/> method.</param>
        /// <returns>Translated text.</returns>
        string GetPluralString(FormattableStringAdapter text, FormattableStringAdapter pluralText, long n, params object[] args);

        /// <summary>
        /// Returns <paramref name="text"/> translated into the selected language using given <paramref name="context"/>.
        /// Similar to <c>pgettext</c> function.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="text">Text to translate.</param>
        /// <returns>Translated text.</returns>
        string GetParticularString(string context, FormattableStringAdapter text);

        /// <summary>
        /// Returns <paramref name="text"/> translated into the selected language using given <paramref name="context"/>.
        /// Similar to <c>pgettext</c> function.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="text">Text to translate.</param>
        /// <returns>Translated text.</returns>
        string GetParticularString(string context, FormattableString text);

        /// <summary>
        /// Returns <paramref name="text"/> translated into the selected language using given <paramref name="context"/>.
        /// Similar to <c>pgettext</c> function.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="text">Text to translate.</param>
        /// <param name="args">Optional arguments for <see cref="string.Format(string, object[])"/> method.</param>
        /// <returns>Translated text.</returns>
        string GetParticularString(string context, FormattableStringAdapter text, params object[] args);

        /// <summary>
        /// Returns the plural form for <paramref name="n"/> of the translation of <paramref name="text"/> using given <paramref name="context"/>.
        /// Similar to <c>npgettext</c> function.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="text">Singular form of message to translate.</param>
        /// <param name="pluralText">Plural form of message to translate.</param>
        /// <param name="n">Value that determines the plural form.</param>
        /// <returns>Translated text.</returns>
        string GetParticularPluralString(string context, FormattableStringAdapter text, FormattableStringAdapter pluralText, long n);

        /// <summary>
        /// Returns the plural form for <paramref name="n"/> of the translation of <paramref name="text"/> using given <paramref name="context"/>.
        /// Similar to <c>npgettext</c> function.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="text">Singular form of message to translate.</param>
        /// <param name="pluralText">Plural form of message to translate.</param>
        /// <param name="n">Value that determines the plural form.</param>
        /// <returns>Translated text.</returns>
        string GetParticularPluralString(string context, FormattableString text, FormattableString pluralText, long n);

        /// <summary>
        /// Returns the plural form for <paramref name="n"/> of the translation of <paramref name="text"/> using given <paramref name="context"/>.
        /// Similar to <c>npgettext</c> function.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="text">Singular form of message to translate.</param>
        /// <param name="pluralText">Plural form of message to translate.</param>
        /// <param name="n">Value that determines the plural form.</param>
        /// <param name="args">Optional arguments for <see cref="string.Format(string, object[])"/> method.</param>
        /// <returns>Translated text.</returns>
        string GetParticularPluralString(string context, FormattableStringAdapter text, FormattableStringAdapter pluralText, long n, params object[] args);
    }
}