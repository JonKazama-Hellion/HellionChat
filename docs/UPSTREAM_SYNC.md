# Upstream Sync

HellionChat is a standalone EUPL-1.2 plugin that originated from
[Chat 2](https://github.com/Infiziert90/ChatTwo). Since v1.0.0 it
lives under its own namespace, IPC channels and source tree. I no
longer track upstream as a Git fork, but I do monitor Chat 2 commits
regularly and cherry-pick selectively where it makes sense.

This document covers how that works so anyone (including future-me)
can do it cleanly.

## A Word on Intent

HellionChat is not trying to replace Chat 2. I build it for myself,
and maybe for people who want the same things I do: a privacy-first
chat plugin with tighter defaults and no remote-access surface. If
that is not you, Chat 2 is the better choice and a well-maintained
project.

I am available to Infi if he ever has questions about HellionChat or
how I have diverged from the upstream code. What I will not do is
interfere with Chat 2's direction or push unsolicited opinions into
his project.

Long-term compatibility between Chat 2 and HellionChat is not
guaranteed and, frankly, not technically possible. I am building a
new UI from scratch and making deliberate architectural decisions that
pull in a different direction. Some upstream patches will simply stop
applying cleanly and that is expected.

## One-Time Setup

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

`upstream` is read-only. Never push to it.

## Reviewing What Is New Upstream

Before any feature cycle I run a quick check:

```bash
git fetch upstream
git log --oneline main..upstream/main | head -30
```

That shows every commit Infi or contributors landed since the last
sync. I read the messages and decide which ones apply to HellionChat.

## What I Cherry-Pick

**Always:** security fixes, Dalamud API compatibility patches,
BetterTTV and emote-cache fixes, regression fixes for upstream
behaviour HellionChat still relies on.

**Sometimes:** small bug fixes in `MessageManager.cs`,
`MessageStore.cs`, `ChatLogWindow.cs`, the Tabs system. These come in
when they touch code I have not heavily modified.

**Never:** webinterface changes (the entire webinterface tree is gone
in HellionChat), changes that conflict with the privacy filter, changes
that re-add upstream defaults I deliberately reversed (full-history
logging, Tell Exclusive defaults, etc.).

As HellionChat's UI moves further from the Chat 2 baseline, upstream
patches will increasingly require adaptation rather than a clean
apply. If a patch cannot be ported without breaking HellionChat
behaviour or the privacy model, I skip it rather than force a
compromised version in.

## How I Cherry-Pick

Always with `-x` so authorship and the original commit hash stay
visible:

```bash
git checkout -b sync/upstream-<topic> main
git cherry-pick -x <upstream-commit-sha>
```

`-x` appends a `(cherry picked from commit <sha>)` line to the commit
message. That preserves upstream-author credit and lets anyone reading
`git log` trace the change back to Chat 2. Commit messages stay
identical to the upstream original; I do not rewrite them to match the
HellionChat format.

## Conflict Handling

When a cherry-pick conflicts:

1. Resolve by hand. Do not rewrite upstream code to match HellionChat
   conventions; that is what the merge marker showed.
2. If the conflict is fundamental (touches code that no longer exists
   in HellionChat), abort the cherry-pick and note why in the
   relevant GitHub issue or backlog item. Some upstream patches are
   simply not portable and that is fine.
3. After a clean resolve the commit message stays as-is, with the
   `-x` footer Git appends automatically.

## Pushing the Sync

Cherry-picked commits go through the same review as any other change.
The sync branch lands in `main` via a no-fast-forward merge, then gets
a release tag if user-visible behaviour changed:

```bash
git checkout main
git merge --no-ff sync/upstream-<topic> -m "merge: upstream sync — <topic>"
```

## Contributing Back

HellionChat benefits from Chat 2's work, so I try to give something
back where I can. If I fix a bug or improve something that would be
useful to Chat 2 and is not HellionChat-specific, I submit a
good-will PR to [Infiziert90/ChatTwo](https://github.com/Infiziert90/ChatTwo).

A few things to note about that process:

- Good-will PRs are validated in a separate fork first to make sure
  the fix stands on its own without HellionChat context.
- They are written by hand. No AI-generated code goes to Infi's
  project. He did not ask for Pair-level AI involvement and I will
  not push that decision onto his codebase.
- This is not guaranteed for every change, only where it makes sense
  and where I am confident the fix is clean and self-contained.

## When Upstream Goes Silent

If Chat 2 stops receiving updates the remote stays configured and this
workflow stays documented. The moment maintenance picks back up I am
ready to pull again.

## When Upstream Takes a Direction I Cannot Follow

If a future Chat 2 release breaks compatibility with the HellionChat
privacy philosophy in a way that cannot be resolved (mandatory cloud
sync, removal of the local message store, an incompatible license
change), HellionChat continues from the last compatible cherry-pick.
The inherited history stays under EUPL-1.2 and stays attributed.
