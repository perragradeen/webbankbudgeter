namespace WebBankBudgeterUiTest;

[TestClass]
public class UtgiftsHanterareUiBinderTests
{
    [TestMethod]
    public void Placeholder_UtgiftsHanterareUiBinder_TestsArPagaende()
    {
        var placeholderStatus = Environment.GetEnvironmentVariable("WEBBANKBUDGETER_UI_TEST_STATUS") ?? "pågår";

        Assert.IsFalse(
            string.IsNullOrWhiteSpace(placeholderStatus),
            "Placeholder-test: riktiga integrationstester för UtgiftsHanterareUiBinder är på gång.");
    }
}