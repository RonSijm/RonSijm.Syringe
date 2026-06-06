using RonSijm.Syringe.Tests.Features.TestHelpers;

namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.PropertyResolving
{
    public class ClassWithRequiredProperty
    {
        [Inject(Required = true)]
        public ChildClassWithProperty RequiredChild { get; set; }
    }

    public class ClassWithOptionalProperty
    {
        [Inject(Required = false)]
        public ChildClassWithProperty OptionalChild { get; set; }
    }
}
