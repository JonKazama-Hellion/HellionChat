# Third-party notices

HellionChat ships and depends on a number of third-party components.
This document lists them, their licences and which of them touch the
network. It is the inventory referenced by `PRIVACY.md`.

Last reviewed: 2026-05-03 (HellionChat v0.5.4).

---

## Direct NuGet dependencies

Pinned in `HellionChat/HellionChat.csproj`. Versions reflect the v1.0.0 build.

| Package | Version | Licence | Network | Purpose |
| --- | --- | --- | --- | --- |
| [MessagePack](https://github.com/MessagePack-CSharp/MessagePack-CSharp) | 3.1.4 | MIT | no | Binary serialisation for the SQLite message store. |
| [Microsoft.Data.Sqlite](https://learn.microsoft.com/dotnet/standard/data/sqlite/) | 10.0.7 | MIT | no | Local SQLite access for the message database. |
| [morelinq](https://github.com/morelinq/MoreLINQ) | 4.4.0 | Apache-2.0 | no | LINQ helper extensions. |
| [Pidgin](https://github.com/benjamin-hodgson/Pidgin) | 3.3.0 | MIT | no | Parser combinator library used for chat-input parsing. |
| [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) | 3.1.12 | [Six Labors Split License 1.0](https://github.com/SixLabors/ImageSharp/blob/main/LICENSE) (OSI-approved; free for open-source / non-commercial use, commercial licence required for closed-source commercial use) | no | Image decoding for cached emotes. |

Six Labors note: HellionChat is an EUPL-1.2-licensed open-source
project distributed at no cost. Use of ImageSharp 3.x under the
Six Labors Split License 1.0 is permitted on that basis. Anyone
forking HellionChat for closed-source or commercial redistribution
should review the
[Six Labors licence terms](https://github.com/SixLabors/ImageSharp/blob/main/LICENSE)
and obtain a commercial licence if required.

## SDK and tooling

| Component | Licence | Notes |
| --- | --- | --- |
| [Dalamud.NET.Sdk](https://github.com/goatcorp/Dalamud) 15.0.0 | AGPL-3.0 (Dalamud) / SDK terms per goatcorp | Plugin SDK; pulls in DalamudPackager 15.0.0. |
| [.NET 10 SDK](https://dotnet.microsoft.com/) | MIT | Build toolchain. |

## Bundled assets

| Asset | Licence | Source |
| --- | --- | --- |
| Exo 2 (`HellionFont.ttf`) | SIL Open Font License 1.1 | [Google Fonts / Natanael Gama](https://fonts.google.com/specimen/Exo+2). The OFL licence text travels embedded next to the font (`HellionFont-OFL.txt`) to satisfy the "licence must be distributed with the font" clause. |
| Hellion plugin icon (`images/icon.png`) | © Hellion Media, included under the project licence (EUPL-1.2). | Original artwork. |

---

## Upstream code

HellionChat is a fork of [Chat 2](https://github.com/Infiziert90/ChatTwo)
by Infiziert90 (Infi) and Anna Clemens, also licensed under EUPL-1.2.
The bulk of the code, including the message store architecture, the
channel logic, the hook system and the ImGui chat window, originates
from upstream. See `../NOTICE.md` and `UPSTREAM_SYNC.md` for the
attribution and the cherry-pick policy.

---

## Components that touch the network

Of everything listed above, **none** of the bundled or NuGet
components opens network connections on their own. All outbound
traffic is initiated explicitly by HellionChat's own source files
and is documented in `PRIVACY.md` under "Outbound network calls":

- `HellionChat/EmoteCache.cs` → BetterTTV API + CDN (opt-out via setting)
- `HellionChat/FontManager.cs` → Square Enix Lodestone font CDN (one-time
  download)

---

## Verifying this list

To regenerate the dependency inventory after a version bump:

```bash
dotnet list HellionChat.sln package --include-transitive
```

The "direct NuGet dependencies" table above only lists direct
references. Transitive dependencies pulled in by Dalamud SDK or by
the listed packages are covered by the SDK / package licences and
documented by their respective maintainers.

To re-audit the network-call inventory:

```bash
grep -rn -E "HttpClient|HttpRequest|new Uri\(|https?://" \
  --include="*.cs" HellionChat/
```

Any new hit that is not a click-through (`Util.OpenLink`) or a
payload-parsing call must be added to `PRIVACY.md` before release.
