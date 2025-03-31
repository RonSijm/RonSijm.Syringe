namespace RonSijm.Syringe.Tests.TestObjects.SampleObjects;

#pragma warning disable CS9113 // Parameter is unread.
// ReSharper disable once UnusedType.Global
public class SimpleObjectInLibraryWithInterface(SimpleObjectInLibrary1 simpleObjectInLibrary1) : ISimpleObjectInLibraryWithInterface
{
}