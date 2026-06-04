# Unity Leap Motion Hand Interaction

Pick up and move 3D objects on screen with your bare hands using a Leap Motion / Ultraleap controller. Built in Unity (C#) for natural, smooth, low-latency pinch-to-grab interaction.

## What it does

- Reads tracked hands each frame from the Leap service (Ultraleap Unity package).
- Detects a pinch (thumb tip to index tip) with hysteresis so grab/release does not flicker near the threshold.
- Picks up the nearest `Grabbable` object on pinch and drags it with the hand.
- Smooths the noisy pinch point with a One Euro Filter, then drives the object with a critically-damped follow so motion feels intuitive, not twitchy.
- Throws the object with its release velocity when you open the pinch.
- Works with both hands simultaneously (one interactor per hand).

## Why it feels smooth

Raw Leap palm/pinch positions jitter frame to frame. The pipeline is:

```
Leap Hand -> PinchDetector (hysteresis) -> OneEuroFilter (de-jitter) ->
Grabbable.OnGrabHold (critically-damped Lerp/Slerp) -> throw on release
```

The One Euro Filter removes jitter at low speed while staying responsive at high speed (lag scales down as the hand moves faster), which is the standard fix for shaky hand-tracked manipulation.

## Project layout

```
Assets/Scripts/
  Hands/
    OneEuroFilter.cs        One Euro low-pass filter for Vector3
    PinchDetector.cs        Pinch detection with enter/exit hysteresis
  Interaction/
    Grabbable.cs            Per-object grab state + smooth follow + throw
    HandGrabInteractor.cs   Per-hand: hover, grab nearest, drag with offset
    LeapInteractionManager.cs  Scene entry point, pulls Leap frames
  Demo/
    SampleSceneSpawner.cs   Spawns the 2-3 demo objects to grab
```

## Setup

1. Open in Unity 2022.3 LTS.
2. The Ultraleap Tracking package (`com.ultraleap.tracking`) is pulled from the OpenUPM scoped registry in `Packages/manifest.json`.
3. Add a `LeapServiceProvider` to the scene (from the Ultraleap package). The `LeapInteractionManager` finds it automatically.
4. Add `LeapInteractionManager` and `SampleSceneSpawner` to an empty GameObject. Press Play with the controller connected.
5. To use your own 3D assets: assign your prefab to `SampleSceneSpawner._assetPrefab`, or add a `Collider` + `Grabbable` to any model.

## Tuning

- `PinchDetector.PinchActivateDistance` / `PinchDeactivateDistance` - pinch sensitivity.
- `OneEuroFilterVector3(minCutoff, beta)` - lower `minCutoff` = smoother, higher `beta` = more responsive at speed.
- `LeapInteractionManager._grabRadius` - how close the pinch must be to grab.

## Requirements

- Unity 2022.3 LTS
- Ultraleap / Leap Motion controller and the Ultraleap Tracking Service
