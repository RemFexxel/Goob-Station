// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 MarkerWicker <markerWicker@proton.me>
// SPDX-FileCopyrightText: 2025 Princess Cheeseballs <66055347+Pronana@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Timing;

namespace Content.Shared.Movement.Components
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class InputMoverComponent : Component
    {
        // This class has to be able to handle server TPS being lower than client FPS.
        // While still having perfectly responsive movement client side.
        // We do this by keeping track of the exact sub-tick values that inputs are pressed on the client,
        // and then building a total movement vector based on those sub-tick steps.
        //
        // We keep track of the last sub-tick a movement input came in,
        // Then when a new input comes in, we calculate the fraction of the tick the LAST input was active for
        //   (new sub-tick - last sub-tick)
        // and then add to the total-this-tick movement vector
        // by multiplying that fraction by the movement direction for the last input.
        // This allows us to incrementally build the movement vector for the current tick,
        // without having to keep track of some kind of list of inputs and calculating it later.
        //
        // We have to keep track of a separate movement vector for walking and sprinting,
        // since we don't actually know our current movement speed while processing inputs.
        // We change which vector we write into based on whether we were sprinting after the previous input.
        //   (well maybe we do but the code is designed such that MoverSystem applies movement speed)
        //   (and I'm not changing that)

        public GameTick LastInputTick;
        public ushort LastInputSubTick;

        public Vector2 CurTickWalkMovement;
        public Vector2 CurTickSprintMovement;

        public MoveButtons HeldMoveButtons = MoveButtons.None;

        // I don't know if we even need this networked? It's mostly so conveyors can calculate properly.
        /// <summary>
        /// Direction to move this tick.
        /// </summary>
        public Vector2 WishDir;

        /// <summary>
        /// Entity our movement is relative to.
        /// </summary>
        public EntityUid? RelativeEntity;

        /// <summary>
        /// Although our movement might be relative to a particular entity we may have an additional relative rotation
        /// e.g. if we've snapped to a different cardinal direction
        /// </summary>
        [ViewVariables]
        public Angle TargetRelativeRotation = Angle.Zero;

        /// <summary>
        /// The current relative rotation. This will lerp towards the <see cref="TargetRelativeRotation"/>.
        /// </summary>
        [ViewVariables]
        public Angle RelativeRotation;

        /// <summary>
        /// If we traverse on / off a grid then set a timer to update our relative inputs.
        /// </summary>
        [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
        public TimeSpan LerpTarget;

        public const float LerpTime = 1.0f;

        public bool Sprinting => DefaultSprinting
        ? (HeldMoveButtons & MoveButtons.Walk) != 0x0
        : (HeldMoveButtons & MoveButtons.Walk) == 0x0;
        
        public bool DefaultSprinting = true;

        [ViewVariables(VVAccess.ReadWrite)]
        public bool CanMove = true;
    }

    [Serializable, NetSerializable]
    public sealed class InputMoverComponentState : ComponentState
    {
        public MoveButtons HeldMoveButtons;
        public NetEntity? RelativeEntity;
        public Angle TargetRelativeRotation;
        public Angle RelativeRotation;
        public TimeSpan LerpTarget;
        public bool CanMove, DefaultSprinting;
    }
}