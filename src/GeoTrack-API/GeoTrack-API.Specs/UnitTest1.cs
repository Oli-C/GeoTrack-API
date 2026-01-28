using Reqnroll;
using Xunit;

namespace GeoTrack_API.Specs.StepDefinitions;

[Binding]
public sealed class SmokeSteps
{
    [Given("the test environment is ready")]
    public void GivenTheTestEnvironmentIsReady()
    {
        // Intentionally empty - this is a minimal smoke step.
    }

    [Then("the scenario should pass")]
    public void ThenTheScenarioShouldPass()
    {
        Assert.True(true);
    }
}
