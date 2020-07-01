using System;
using Xunit;
using GetText.Plural;

namespace GetText.Tests.Plural
{
    public class AstPluralRuleGeneratorTest
	{

		[Fact]
		public void ParseNumPluralsTest()
		{
			Assert.Equal(9, AstPluralRuleGenerator.ParseNumPlurals("nplurals=9; plural=n"));
		}

		[Fact]
		public void ParsePluralFormulaTextTest()
		{
			var formulaString = "n%10==1 && n%100!=11 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2";
			Assert.Equal(formulaString, AstPluralRuleGenerator.ParsePluralFormulaText("nplurals=9; plural=" + formulaString));
		}
	}
}
