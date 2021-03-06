using System;

namespace GetText
{
    /// <summary>
    /// Adapter class to reprioritize string and FormattableString overloads when using interpolated strings as argument
    /// See also https://stackoverflow.com/questions/35770713/overloaded-string-methods-with-string-interpolation and https://stackoverflow.com/a/39035309
    /// </summary>
    public class FormattableStringAdapter
    {
        public string String { get; }

        public FormattableStringAdapter(string s)
        {
            String = s;
        }

        public static implicit operator FormattableStringAdapter(string s)
        {
            return new FormattableStringAdapter(s);
        }

        public static implicit operator FormattableStringAdapter(FormattableString fs)
        {
            throw new InvalidOperationException($"Missing FormattableString overload of method taking this type as argument {fs}");
        }
    }
}
