using RonSijm.Syringe.Tests.Features.TestHelpers;

namespace RonSijm.Syringe.Tests.Features.Tests.SyringeServiceProvider.PropertyResolving;

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

public class ClassWithRequiredKeyedProperty
{
    public const string ExpectedKey = "TheKey";

    [Inject(Required = true, Key = ExpectedKey)]
    public ChildClassWithProperty KeyedChild { get; set; }
}

public enum TestServiceKey
{
    Alpha,
    Beta
}

public class ClassWithRequiredEnumKeyedProperty
{
    public const TestServiceKey ExpectedKey = TestServiceKey.Alpha;

    [Inject(Required = true, Key = ExpectedKey)]
    public ChildClassWithProperty EnumKeyedChild { get; set; }
}