using Fluxor;

namespace RonSijm.Syringe.Fluxor.Tests.Redux.TestReduceFrom
{
    [FeatureState]
    public class ReduceFromMainModel
    {
        [ReduceFrom]
        public ReduceFromChildModel Child { get; set; }
    }

    [FeatureState]
    public class ReduceFromChildModel
    {
        public int Count { get; set; }
    }
}