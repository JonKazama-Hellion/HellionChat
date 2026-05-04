<!--
Thanks for contributing to HellionChat. Please fill in the sections
below so the review goes quickly. Delete sections that genuinely do
not apply, but do not delete the whole template.

If this is a security fix, stop here and use a private security
advisory instead:
https://github.com/JonKazama-Hellion/HellionChat/security/advisories/new
-->

## Summary

<!-- One or two sentences. What does this PR change and why. -->

## Type of change

<!-- Tick all that apply. -->

- [ ] Bug fix (non-breaking change that fixes an issue)
- [ ] New feature (non-breaking change that adds behaviour)
- [ ] Breaking change (config migration, removed feature, or behaviour
      change that user-visible defaults rely on)
- [ ] Documentation only
- [ ] Translation update
- [ ] Build, CI or tooling change
- [ ] Upstream cherry-pick from Chat 2

## Linked issue

<!-- e.g. "Closes #42" or "Refs #42". For trivial typo fixes, "n/a". -->

## How I tested this

<!--
- Built locally with `dotnet build -c Release`
- Ran `dotnet test`
- Loaded the plugin in-game on Windows / Linux / macOS via XIVLauncher
- Specific scenarios I exercised in-game
-->

## User-visible changes

<!--
Anything the end user will notice. New settings, changed defaults,
new commands, new translations, removed behaviour. If none, write
"none".
-->

## Compatibility notes

<!--
- Does this require a configuration migration? If yes, which version
  bump and is it covered by the existing migration tests?
- Does this change the schema in MessageStore?
- Does this change the repo.json or HellionChat.yaml manifest fields?
- Does this affect the upstream cherry-pick path? See docs/UPSTREAM_SYNC.md.
-->

## Checklist

- [ ] I have read [CONTRIBUTING.md](../CONTRIBUTING.md) and
      [CODE_OF_CONDUCT.md](../CODE_OF_CONDUCT.md).
- [ ] My change matches the existing code style (`.editorconfig`).
- [ ] I added or updated tests where the existing test infrastructure
      made that practical, or I have explained why tests are not
      applicable.
- [ ] I updated the README, in-plugin strings or documentation if my
      change is user-visible.
- [ ] I did not include any AI-generated code without disclosing it
      in the PR description (see [AI_DISCLOSURE.md](../docs/AI_DISCLOSURE.md)).
- [ ] I confirm my contribution is released under the
      [EUPL-1.2](../LICENSE).
