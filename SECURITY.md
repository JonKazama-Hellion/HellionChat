# Security Policy

## Reporting a Vulnerability

If you find a security issue in HellionChat, please do not open a
public GitHub issue. Use one of the private channels below so I can
investigate and ship a fix before the details go public.

**Preferred:**
[Privately report a vulnerability](https://github.com/JonKazama-Hellion/HellionChat/security/advisories/new)
via GitHub Security Advisories. This routes the report directly to me
and keeps the conversation off the public timeline.

**Alternative:**

| Channel    | Address                    |
| ---------- | -------------------------- |
| Email      | `kontakt@hellion-media.de` |
| Discord DM | `@j.j_kazama`              |

I respond on weekdays during European business hours. For urgent
disclosures (active exploitation, user-data exposure) email is the
fastest path.

## Scope

### In scope

- Code paths that touch user-controlled input (chat messages, plugin
  config, file paths the user can influence)
- The privacy filter in `MessageStore.cs` and the export pipeline
- The configuration migration logic
- The `EmoteCache` HTTP client and path handling
- The Auto-Tell-Tabs spawn logic and history preload

### Out of scope

- Issues in upstream Chat 2 that HellionChat has not modified — report
  those at <https://github.com/Infiziert90/ChatTwo/issues>
- Issues in Dalamud itself — those go to
  <https://github.com/goatcorp/Dalamud>
- Issues in the FFXIV game client
- Anything that requires the user to install a malicious plugin first

## Disclosure Window

I aim to ship a fix within 14 days for high-severity issues and within
30 days for everything else. If a fix needs more time I will say so in
the private thread.

## Credits

Everyone who reports a real issue gets listed in the changelog of the
release that fixes it, unless they prefer to stay anonymous. No bug
bounty, nothing financial — this is a hobby plugin.
