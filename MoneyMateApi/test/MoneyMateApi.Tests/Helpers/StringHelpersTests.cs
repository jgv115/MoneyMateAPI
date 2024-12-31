using System.Collections.Generic;
using MoneyMateApi.Helpers;
using Xunit;

namespace MoneyMateApi.Tests.Helpers;

public class StringHelpersTests
{
    [Fact]
    public void GivenInputString_WhenGenerateNGramsCalled_ThenCorrectListReturned()
    {
        Assert.Equal(new List<string>
        {
            "Multiword Product",
            "multiword product",
            "MULTIWORD PRODUCT"
        }, StringHelpers.GenerateCapitilisationCombinations("multiword product"));
    }
}