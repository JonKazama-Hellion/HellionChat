using System.Numerics;

namespace HellionChat.Integrations;

// Local DTO mirroring Honorific's TitleData shape. We replicate the structure
// instead of referencing Honorific.dll because a hard build-time dependency
// would couple the two assemblies and break HellionChat at load time when
// Honorific is missing. Glow, Color3, GradientColourSet and GradientAnimationStyle
// are intentionally omitted — Cycle 1 renders text in the primary Color only;
// the "Honorific Full Fidelity" backlog item adds them later as a pure
// extension that won't break this DTO's existing consumers.
internal sealed record HonorificTitleData(
    string? Title,
    bool IsPrefix,
    bool IsOriginal,
    Vector3? Color
);
