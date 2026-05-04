# Upstream sync workflow

HellionChat is a standalone EUPL-1.2 fork of [Chat 2](https://github.com/Infiziert90/ChatTwo).
We pull selected patches from upstream manually instead of running an
automated mirror. This file documents how that works so anyone (including
future-me) can do it cleanly.

## One-time setup

Add the upstream repo as a remote on a fresh clone:

```bash
git remote add upstream https://github.com/Infiziert90/ChatTwo.git
git fetch upstream
```

Verify both remotes are wired up:

```bash
git remote -v
# origin    https://github.com/JonKazama-Hellion/HellionChat.git (fetch)
# origin    https://github.com/JonKazama-Hellion/HellionChat.git (push)
# upstream  https://github.com/Infiziert90/ChatTwo.git (fetch)
# upstream  https://github.com/Infiziert90/ChatTwo.git (push)
```

You never push to `upstream`. It is read-only for us.

## Reviewing what is new upstream

Before any feature cycle starts I run a quick check:

```bash
git fetch upstream
git log --oneline main..upstream/main | head -30
```

That shows every commit Infi or contributors landed since the last sync.
Read the messages, decide which ones apply.

## What we cherry-pick

**Always:** security fixes, API-version compatibility patches (Dalamud
API 15 → 16 → ...), BetterTTV / emote-cache fixes, regression fixes for
the upstream behaviour we still rely on.

**Sometimes:** small bug fixes in `MessageManager.cs`, `MessageStore.cs`,
`ChatLogWindow.cs`, the Tabs system. Pull them when they touch code we
have not heavily modified.

**Never:** webinterface changes (the entire webinterface tree is gone in
HellionChat), changes that conflict with the privacy filter, changes that
re-add upstream defaults we deliberately reversed (full-history logging,
Tell Exclusive defaults, etc.).

## How we cherry-pick

Always with `-x` so authorship and the original commit hash stay
visible:

```bash
git checkout -b sync/upstream-<topic> main
git cherry-pick -x <upstream-commit-sha>
```

`-x` adds a `(cherry picked from commit <sha>)` line to the commit
message. That preserves the upstream-author credit and lets anyone
reading `git log` trace the change back to ChatTwo. Co-Author trail
intact, no AI lines, no "Hellion" prefix on commits that were not
authored by us.

## Conflict handling

When a cherry-pick conflicts:

1. Resolve the conflict by hand. Do not "fix" upstream code to match
   Hellion conventions; that is what the merge marker showed us.
2. If the conflict is fundamental (touches code that no longer exists
   in our fork), abort the cherry-pick and document why in
   `Hellion Chat Backlog.md` instead. Some upstream patches are not
   portable; that is fine.
3. After a successful resolve, the commit message stays identical to
   the upstream message, with the `-x` cherry-pick footer Git appends
   automatically. Do not rewrite the message to match our format.

## Pushing the sync

Cherry-picked commits go through the same review as our own work: the
sync branch lands in `main` via a no-fast-forward merge, then a release
tag if the user-visible behaviour changes (otherwise just merged).

```bash
git checkout main
git merge --no-ff sync/upstream-<topic> -m "merge: upstream sync — <topic>"
```

## When upstream goes silent

If Chat 2 stops receiving updates entirely we keep this workflow alive
anyway. The remote stays configured, the documentation stays here. The
moment maintenance picks back up we are ready to pull again.

## When upstream takes a direction we cannot follow

If a future ChatTwo release breaks compatibility with our privacy
philosophy in a way we cannot resolve (e.g. mandatory cloud sync,
removal of the local message store, a license change that makes EUPL
incompatible), HellionChat continues on its own from the last
compatible cherry-pick. The history we already inherited stays under
EUPL-1.2 and stays attributed.
