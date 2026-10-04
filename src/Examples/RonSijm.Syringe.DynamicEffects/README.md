# Dynamic method-effect reproduction

This small console app distinguishes a working Fluxor setup from a missing dynamic registration. It references the **source projects**, not a published NuGet version.

Run from the repository root with a .NET 10 SDK:

```powershell
dotnet run --project .\src\Examples\RonSijm.Syringe.DynamicEffects
```

The app builds and initializes one Syringe provider and Fluxor store. It scans an initial state, reducer and constructor-injected startup method effect. Then it adds another set of service descriptors using `AddFluxorLibrary`, `LoadServiceDescriptors` and `Build`.

This exercises runtime module registration; it does not download another assembly or start Blazor.

| Check | Before the fix | Required after the fix |
|---|---|---|
| Startup `[EffectMethod]` | PASS | PASS |
| The same store survives registration | `True` | `True` |
| Dynamic typed `Effect<T>` with property injection | PASS | PASS |
| Two instance `[EffectMethod]` methods for one action | FAIL | Both PASS |
| Static `[EffectMethod]` | FAIL | PASS |
| Process exit code | `1` | `0` |

Each action waits up to two seconds for an actual reducer result. Unhandled effect exceptions propagate instead of being mistaken for an absent handler. The app does not manually resolve method hosts, add effects to the store, or work around the registration bug.

The defect is that scanning registers method-host services, while the dynamic after-build hook only attaches services implementing `IEffect`. The wrappers created by initial store construction are never created for newly loaded methods.

The reproduction is committed separately before the library fix. Running this **unchanged** sample after applying the fix provides the before/after check.

See [the investigation and remaining limitations](../../../docs/fluxor-dynamic-registration.md) for the related startup-injection and state-snapshot fixes, regression coverage and improvement suggestions.
