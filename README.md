# Static Leak Lab

A teaching project for the **Mono → CoreCLR** transition in Unity. Without Domain Reload,
static state is **not** reset between Play sessions. This project collects the most common
static patterns that break when that happens, so you can:

1. **See** the bugs and leaks happen live (dashboard + Console),
2. **Detect** them with the Auditor,
3. **Fix** them with the new reset attributes, and check that the dashboard goes back to normal.

- Editor: `6000.6.8f1` (URP). The scripts use no version-specific API, so they should run as-is in a 6.8 / CoreCLR build.
- Scene: `Assets/_Project/Scenes/StaticLeakDemo.unity` (the only scene in Build Profiles).
- **Enter Play Mode Options are already set to *Reload Scene only* (Domain Reload disabled)**, which is what
  6.8 does by default. Change it in *Project Settings ▸ Editor ▸ Enter Play Mode Settings*.

## How to run the demo

1. Open `StaticLeakDemo` and press **Play**. Session 1 looks normal: every dashboard row is white.
2. Stop, then press **Play** again. Rows turn **red** when their value differs from what a fresh domain would show,
   and the Console fills with exceptions.
3. Press Play a few more times. The numbers keep growing: duplicated handlers, ghost objects, texture memory.
4. Control experiment: enable Domain Reload (*Project Settings ▸ Editor ▸ When entering Play Mode ▸ Reload Domain and Scene*).
   Every session now looks like session 1.

> The statics also survive *Stop*. To start from a clean slate with Domain Reload off, trigger a
> script recompile (edit any script, or *Assets ▸ Refresh* after a change).

## The hazards

| # | File | Pattern | Symptom from session 2 onward |
|---|------|---------|-------------------------------|
| 1 | `Models/ScoreModel.cs` | `static int`, static **auto-property**, static **event** | Score starts at the previous value. The event holds a delegate to a destroyed `ScoreView`, so the score display freezes and the Console shows `MissingReferenceException` on every kill |
| 2 | `Models/IdGenerator.cs` | **static constructor** + counter | The static constructor runs once per domain, not per session (its log line appears only once). Ids continue at 8, 16, 24… |
| 3 | `Models/GameRandom.cs` | `static readonly` **mutable** object (`System.Random`) | The seeded level is no longer deterministic: the "first roll" changes every session |
| 4 | `Models/EventBus.cs` | static field in a **generic** class (`EventBus<T>`) | One handler list per closed type. Lambdas capturing `this` pile up, so in session N every kill scores N times |
| 5 | `Systems/EntityRegistry.cs` | static `List<Transform>` | Destroyed objects stay referenced: *registered* grows while *alive* stays at 8. The C# wrappers can never be garbage-collected |
| 6 | `Systems/ServiceLocator.cs` | static `Dictionary<Type, object>` | `Register` throws `InvalidOperationException` ("already registered"), and `Resolve` returns a destroyed service |
| 7 | `Systems/TextureCache.cs` | static cache of `new Texture2D` | **Native memory leak**: +2 MB per session (8 × 256 KB) that the asset GC can never release |
| 8 | `Systems/LogCounter.cs` | `[RuntimeInitializeOnLoadMethod]` subscribes to an **engine event** | `Application.logMessageReceived` gets one more handler each session, so every log is counted N times |
| 9 | `Systems/PersistentRoot.cs` | lazy-init `static bool` + cached `GameObject` | The guard skips creation and the cached object is destroyed, so you get `MissingReferenceException` |
| 10 | `Presenters/EnemyPresenter.cs`, `GameBootstrap.cs` | `static` field on a **MonoBehaviour** | The per-scene counters (enemies spawned, session count) never reset |
| ✓ | `Models/SafeStatics.cs` | `const`, `static readonly` of an immutable value, pure static method | **Control group.** Not a hazard. Use it to discuss Auditor false positives |

## Suggested lesson flow

1. **Demo the pain.** Run 3 sessions with Domain Reload off, then walk through the dashboard rows and the Console.
2. **Why it happens.** Under Mono, Domain Reload threw away the whole AppDomain, and every static with it. Disabling it
   (and, under CoreCLR, the move away from AppDomains) means the managed heap outlives the Play session.
   Static roots keep alive:
   - *values* (wrong game state),
   - *delegates* (duplicated or stale callbacks),
   - *references to UnityEngine.Objects* (fake-null ghosts that leak managed memory),
   - *native assets* (real memory leaks).
3. **Detect.** Run the Auditor on `Assets/_Project/Scripts`. Check that it reports #1–#10 and decide how to
   treat `SafeStatics`.
4. **Fix.** Apply the reset attributes hazard by hazard and re-run several Play sessions. Each fixed row on the
   dashboard should stay white, and the Console should stay clean. Points to discuss:
   - #4 needs a reset **per closed generic type**.
   - #5, #7 and #9 need cleanup (`Object.Destroy` for the textures), not only a reset.
   - #8 needs an **unsubscribe**, because the engine event lives outside the managed static.

## Project layout

```
Assets/_Project/
  Scripts/
    Models/      StaticLeakLab.Models.asmdef      (no references)
    Views/       StaticLeakLab.Views.asmdef       (no references)
    Systems/     StaticLeakLab.Systems.asmdef     -> Models
    Presenters/  StaticLeakLab.Presenters.asmdef  -> Models, Views, Systems
  Scenes/        StaticLeakDemo.unity
  Audio/ Animations/ Graphics/{3D,Materials,Shaders,2D}/ Settings/
```

`dashboard_session2.png` is a Game view capture of session 2 with Domain Reload off, ready for slides.
