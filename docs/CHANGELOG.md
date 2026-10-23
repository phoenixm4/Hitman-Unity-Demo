# Project Changes

## Additions

- Enabled looping for the Idle and Walking animation clips.
- Kept the kinematic WASD controller independent of animation; it drives Animator `Speed` when a compatible parameter exists.
- Added a separate Cinemachine `Player Follow Camera` with adjustable orbit distance and height.
- Added right-mouse-button-controlled orbit and pan/tilt camera rotation, starting aligned with the player's facing direction.
- Enabled generated mesh colliders for physical prop models so stairs, roads, curbs, and other environment geometry collide with the player.

## Removals

- Removed the `character` Animator Controller and `Locomotion` blend tree so animation can be rebuilt from scratch.
- Removed the separate Player animation-speed helper to avoid competing writes to Animator `Speed`.

## Adjustments

- Ground the character at startup and reduce CharacterController skin width to prevent sinking on Play.
- The Cinemachine camera follows the player position while mouse input controls its orbit and view; the Main Camera remains the render camera.
- Camera rotation is active only while holding the right mouse button; the cursor is released otherwise.