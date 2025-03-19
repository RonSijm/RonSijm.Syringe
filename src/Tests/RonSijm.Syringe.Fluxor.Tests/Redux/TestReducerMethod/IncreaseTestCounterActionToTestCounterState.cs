using Fluxor;

namespace RonSijm.Syringe.Fluxor.Tests.Redux.TestReducerMethod;

public class IncreaseTestCounterActionToTestCounterState
{
    [ReducerMethod]
    public static TestCounterState IncreaseTestCounterActionToTestCounterStateReduce(TestCounterState state, IncreaseTestCounterAction action)
    {
        return new TestCounterState { Count = state.Count + 1 };
    }
}