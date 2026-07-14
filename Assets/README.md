# Glitch Compiler Unity project

Open this folder with Unity Hub using Unity 6 LTS. The source implementation is under `Assets/Scripts`.

Before the first play, create three scenes named `Bootstrap`, `MainMenu`, and `Level` in `Assets/Scenes`, add them to Build Settings in that order, and wire the serialized references described in the project plan. Unity generates `.meta`, prefab, scene, and ScriptableObject serialization files; those cannot be safely authored without an installed Unity Editor.

The initial usable loop is: assign `CanvasRenderer` and `IDEEditorController` in `Level`, call `Compile` from a UI Button, and give `LevelDefinition` a 512×512 target texture. V-Code, draw-command execution, pixel comparison, anomaly classes, and local JSON player progress are implemented in source.
