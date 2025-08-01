// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TinManTim <73014819+Tin-Man-Tim@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Destructible;

public abstract class SharedDestructibleSystem : EntitySystem
{
    /// <summary>
    ///     Force entity to be destroyed and deleted.
    /// </summary>
    public bool DestroyEntity(EntityUid owner)
    {
        var ev = new DestructionAttemptEvent();
        RaiseLocalEvent(owner, ev);
        if (ev.Cancelled)
            return false;

        var eventArgs = new DestructionEventArgs();
        RaiseLocalEvent(owner, eventArgs);

        QueueDel(owner);
        return true;
    }

    /// <summary>
    ///     Force entity to break.
    /// </summary>
    public void BreakEntity(EntityUid owner)
    {
        var eventArgs = new BreakageEventArgs();
        RaiseLocalEvent(owner, eventArgs);
    }
}

/// <summary>
///     Raised before an entity is about to be destroyed and deleted
/// </summary>
public sealed class DestructionAttemptEvent : CancellableEntityEventArgs
{

}

/// <summary>
///     Raised when entity is destroyed and about to be deleted.
/// </summary>
public sealed class DestructionEventArgs : EntityEventArgs
{

}

/// <summary>
///     Raised when entity was heavy damage and about to break.
/// </summary>
public sealed class BreakageEventArgs : EntityEventArgs
{

}