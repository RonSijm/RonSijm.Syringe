using Fluxor;

namespace RonSijm.Syringe.Fluxor.Tests.Redux.ThroughEffectWithInjection;

[FeatureState]
public class IncreaseTestCounterThroughEffectWithInjection_State
{
    public int Count { get; set; }
}