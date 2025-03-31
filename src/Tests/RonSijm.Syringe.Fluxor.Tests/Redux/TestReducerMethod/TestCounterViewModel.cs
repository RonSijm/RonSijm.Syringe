using Fluxor;

namespace RonSijm.Syringe.Fluxor.Tests.Redux.TestReducerMethod;

[FeatureState]
public class TestCounterViewModel
{
    [ReduceInto]
    public TestCounterState Counter { get; set; }
}