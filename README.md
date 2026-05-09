# Hellion Chat

> ## ⚠ This repository has moved
>
> Hellion Chat is no longer developed on GitHub. Active development, releases, issues and discussions have moved to:
>
> ### → [gitea.hellion-forge.cloud/JonKazama-Hellion/HellionChat](https://gitea.hellion-forge.cloud/JonKazama-Hellion/HellionChat)
>
> This repository is **frozen at v1.4.2** and archived. The plugin itself is alive and well, just at a new home.

---

## Why the move

Three reasons, in descending order:

1. **GDPR and data sovereignty.** My code now lives on a German server I control, instead of US data centres.
2. **Out of Microsoft's Copilot training pipeline.** I'm still actively learning, and I'd rather not feed the AI half-baked code from a hobby project.
3. **Full control over the build pipeline** without hitting GitHub's free quota limits.

The new URL runs under my own domain (`hellion-forge.cloud`), so even if I switch servers down the line, the link stays the same. You only have to migrate once.

---

## Migrating an existing install

The old `repo.json` has been removed from this repository, so Dalamud will show a small red notice at the top of the plugin installer for the old URL. No crash, but no more updates from here either.

> 💾 **Your settings and chat history are safe.** Dalamud only removes the plugin code when you uninstall, not your configuration. The new install picks up the same `pluginConfigs/HellionChat/` directory and finds your retention settings, theme choices and message database right where they were. If for some reason no config exists yet, the plugin creates a fresh one on first start.

To switch over:

1. **Uninstall the plugin:** in XIVLauncher, `/xlplugins` → find Hellion Chat → disable first, then delete. Your `pluginConfigs/HellionChat/` directory stays untouched.
2. **Remove the old repo URL:** `Settings → Experimental → Custom Plugin Repositories` → delete the old `raw.githubusercontent.com/JonKazama-Hellion/HellionChat/...` entry
3. **Add the new repo URL:**
   ```
   https://gitea.hellion-forge.cloud/JonKazama-Hellion/HellionChat/raw/branch/main/repo.json
   ```
4. **Enable and save**
5. **Reinstall:** `/xlplugins` → search for Hellion Chat → Install

Important: the old source has to be fully gone before the new one goes in. Otherwise Dalamud sees the plugin twice and that just creates noise.

---

## Issues, pull requests, discussions

This repository is archived and read-only. Any open issues or PRs that were here are closed. New issues, feature requests and discussions go to the new repository's tracker:

→ [gitea.hellion-forge.cloud/JonKazama-Hellion/HellionChat/issues](https://gitea.hellion-forge.cloud/JonKazama-Hellion/HellionChat/issues)

If you don't have a Gitea account and don't want to create one, you can also reach me directly:

- Discord: `@j.j_kazama`
- Email: `kontakt@hellion-media.de`

---

## Acknowledgements

Hellion Chat is a fork of [Chat 2](https://github.com/Infiziert90/ChatTwo) by **Infiziert90 (Infi)** and **Anna Clemens**. Their work is the foundation this plugin still runs on. Full attribution lives in the new repository under `NOTICE.md`.

---

Maintained under **Hellion Forge**, the modding and plugin line of **Hellion Online Media** | Bad Harzburg | [hellion-media.de](https://hellion-media.de)
