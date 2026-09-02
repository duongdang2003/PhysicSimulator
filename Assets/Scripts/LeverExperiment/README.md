# Lever experiment setup

Create a `LeverExperiment` object and assign the scene references in the Inspector:

- `LeverView`: beam, fulcrum, left weight and right weight.
- `LeverUI`: readout, four sliders, and the force/ruler/reset buttons.
- Optional `ForceVector` and `MeasurementRuler` references.
- Add `LeverController` to the scene and assign the experiment and camera to enable dragging weights.

Set the mass sliders to `0.05..1` kg and distance sliders to `0.2..1.3` m (`20..130` cm). Weight size scales with the cube root of mass. The model uses `F = m × 9.81` and `M = F × d`; the view only displays the resulting state.
