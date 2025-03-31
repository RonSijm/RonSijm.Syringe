using RonSijm.Syringe.Tests.Features.TestHelpers;

namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.PropertyResolving
{
    public class ClassWithProperty
    {
        [Inject]
        public ChildClassWithProperty Child { get; set; }
    }

    public class ChildClassWithProperty
    {
        [Inject]
        public ClassWithProperty Parent { get; set; }
    }
}
