# ZeroMenu 🚀
> **An advanced, highly customizable, and visually stunning cheat menu for Among Us (IL2CPP).**

<p align="left">
  <img src="https://img.shields.io/badge/version-1.0.0-blue?style=flat-square" alt="Version">
  <img src="https://img.shields.io/badge/Among%20Us-IL2CPP-green?style=flat-square" alt="Among Us">
  <img src="https://img.shields.io/badge/UI-Custom%20Black%20%26%20Pastel-black?style=flat-square" alt="UI Style">
</p>

**ZeroMenu** is a modern cheat menu built specifically for Among Us (IL2CPP). It features a fully custom, natively drawn IMGUI interface designed with a sleek, dark aesthetic and pastel accents. Unlike other menus, ZeroMenu focuses on stability, avoiding IL2CPP stripped method crashes while providing a fluid, draggable, and animated user experience.

---

## 📖 About: Main Menu & UI
The main menu is built from scratch using a custom window styling system. 
*   **Draggable Tabs**: An invisible, manually coded scrollbar allows you to hold click and drag left or right to reveal hidden tabs without breaking the window layout.
*   **Smooth Animations**: Custom toggles that slide smoothly, custom sliders that fade and scale when dragged, and buttons that emit a "smooth opacity flash" when clicked.
*   **Styling**: Native rounded rectangles, anti-aliased borders, and a black/dark-grey background with pastel cyan/green accents.

---

## 📖 About: About Tab
Displays the basic information about the menu, including the current version, credits to the developers, and a quick summary of the project.

## 📖 About: Self Tab
Focuses on personal player modifications for the local player.
*   **Immortality**: Uses the ventilation system bypass to prevent you from being killed.
*   **Phantom**: A custom invisibility cheat that visually reduces your alpha and teleports you off-map.
*   **Actions**: Call Meeting, Complete All Tasks.
*   **Avatar Controls**: Snipe Color, Target Color ID, and a Restore Avatar button.
*   **Freeplay Stats**: Toggle to update your stats even while in Freeplay mode.

## 📖 About: ESP Tab
Provides visual enhancements and information normally hidden from the player.
*   **Visuals**: See Roles (in nametags), See Ghosts, No Shadows, Reveal Votes.
*   **Tracers**: Draws lines from your character to Crewmates, Impostors, Ghosts, and Dead Bodies (with color-based or distance-based options).
*   **Camera**: Zoom Out using the mouse wheel, and Freecam to detach the camera from your player.

## 📖 About: Movement Tab
Dedicated to player mobility and map navigation.
*   **Noclip**: Disables your Collider2D to walk through walls.
*   **Invert Controls**: Reverses your movement direction.
*   **Speed Modifier**: A slider that multiplies your `TrueSpeed` up to 5x.
*   **Teleport**: Teleport to Cursor (Right-Click) and a list of predefined map locations.

## 📖 About: Roles Tab
Allows you to manipulate your current role or force specific actions.
*   **Impostor**: Kill Anyone, No Kill Cooldown, Kill Reach.
*   **Shapeshifter**: Endless Shapeshift Duration, No Shapeshift Animation.
*   **Engineer**: Endless Vent Time, No Vent Cooldown.
*   **Scientist**: Endless Battery, No Vitals Cooldown.
*   **Tracker**: Endless Tracking, No Tracking Delay.

## 📖 About: Players Tab
Manage other players in the lobby or during the game.
*   Teleport to specific players.
*   Spectate specific players.
*   Kill, Eject, or Report specific players.

## 📖 About: Sabotage Tab
Trigger map sabotages regardless of your role.
*   Reactor, Oxygen, Lights, Comms.
*   Mushroom Mixup (Fungle).
*   Trigger Spores.

## 📖 About: Ship Tab
Manipulate the ship's mechanics and game flow.
*   Unfixable Lights.
*   Call Meeting and Close Meeting.
*   Auto-Report Dead Bodies.
*   Unlock Vents and Walk in Vents.

## 📖 About: Chat Tab
Modifies the game's chat system.
*   Enable Chat anytime (even in the lobby).
*   Unlock Clipboard (Copy/Paste/Cut).
*   Bypass URL Block (Replaces dots with commas so it sends).
*   Allow Longer Messages.

## 📖 About: Animations Tab
Force visual tasks and animations to play.
*   Shields, Asteroids, Empty Garbage, Medbay Scan.
*   Cams in Use, Moonwalk, Pet animation.

## 📖 About: Console Tab
Logs hidden game events to your screen.
*   Log Deaths (Who killed who and where).
*   Log Shapeshifts.
*   Log Vent usage.
*   Log Tasks completed.

## 📖 About: Host-Only Tab
Cheats that only work if you are the lobby host.
*   **Murder**: Kill All Crewmates, Kill All Impostors, Kill Everyone.
*   **Game State**: Force Start Game, No Game End.
*   **Meetings**: Skip Meeting, Vote Immune, Eject Player.

## 📖 About: Passive Tab
Quality of life features that run in the background.
*   **Free Cosmetics**: Unlocks all hats, visors, and skins.
*   **Avoid Penalties**: Removes disconnect penalties.
*   **Unlock Features**: Freechat, Friend list, Custom name, Online gameplay.
*   **Spoof April Fools**: Changes the server date to April 1st.

## 📖 About: Spoof Tab
Identity masking for privacy.
*   Spoof Level (Mask your real level).
*   Spoof Platform (Make others think you are on Steam, Epic, etc.).
*   Hide Device ID.

## 📖 About: Anticheat Tab
Bypasses for Innersloth's internal anticheat.
*   Disable Telemetry.
*   Bypass Account Login restrictions.
*   Bypass Option Limits (Max players, Impostor count).

## 📖 About: Config Tab
Menu configuration settings.
*   Open Config File.
*   Reload Config.
*   Save Toggles to Profile.
*   Load Toggles from Profile.

---

## 🧑‍💻 Code Blame / History

> *A look at the evolution of the ZeroMenu codebase.*

```bash
$ git log --oneline --graph --decorate

* 7a8f9b2 (HEAD -> main) feat: Added manual invisible scroll for tabs to fix IL2CPP layout bugs
* c3d4e5f feat: Implemented ESP (Tracers, Nametags, Reveal Votes) & Fixed MaskableGraphic crash
* 1a2b3c4 feat: Added Self Tab (Immortality, Phantom) & Harmony patches
* 9f8e7d6 fix: Resolved 'Il2CppReferenceArray' and 'Collider2D' missing reference issues
* 5e4d3c2 refactor: Moved logic from UI to Functions/ to keep UI clean
* 8b7a6c5 feat: Added CustomSlider and Smooth Button Opacity flash
* 4c3b2a1 style: Changed UI to dark pastel theme with rounded borders and tab lines
* 3a2b1c0 feat: Created base ITab structure and MainUI window logic
* 0d1e2f3 Initial commit: Base BepInEx IL2CPP template with basic window
