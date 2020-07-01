using System;
using GetText.Plural;

namespace GetText.PluralCompile
{
    public class CompiledPluralRule : PluralRule
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CompiledPluralRule"/> class using specified maximum plural
		/// forms value and an evaluation delegate.
		/// </summary>
		/// <param name="numPlurals"></param>
		/// <param name="evaluatorDelegate"></param>
		public CompiledPluralRule(int numPlurals, PluralRuleEvaluatorDelegate evaluatorDelegate)
			: base(numPlurals, evaluatorDelegate)
		{
		}
	}
}
