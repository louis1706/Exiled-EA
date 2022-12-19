// -----------------------------------------------------------------------
// <copyright file="PreAuthenticating.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reflection;
using Exiled.API.Enums;
using GameCore;
using Mirror.LiteNetLib4Mirror;
using PlayerRoles.PlayableScps.Scp079;
using PluginAPI.Events;
using Log = Exiled.API.Features.Log;

namespace Exiled.Events.Patches.Events.Player
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using Exiled.Events.EventArgs.Player;
    using Handlers;

    using HarmonyLib;

    using LiteNetLib;
    using LiteNetLib.Utils;

    using NorthwoodLib.Pools;

    using static HarmonyLib.AccessTools;

    /// <summary>
    ///     Patches <see cref="ReservedSlot.HasReservedSlot(string userId, out bool bypass)" />.
    ///     Adds the <see cref="Player.ReservedSlot" /> event.
    /// </summary>
    [HarmonyPatch(typeof(ReservedSlot), "HasReservedSlot")]
    internal static class ReservedSlotPatch
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ReservedSlotPatchTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            LocalBuilder jumpConditions = generator.DeclareLocal(typeof(ReservedSlotEventResult));

            Label continueConditions = generator.DefineLabel();

            Label allowUnconditional = generator.DefineLabel();
            Label returnTrue = generator.DefineLabel();
            Label returnFalse = generator.DefineLabel();


            int offset = -1;
            int index = newInstructions.FindLastIndex(
                instruction => instruction.LoadsField(Field(typeof(PlayerCheckReservedSlotCancellationData), nameof(PlayerCheckReservedSlotCancellationData.HasReservedSlot)))) + offset;

            newInstructions[index].WithLabels(continueConditions);

            newInstructions.InsertRange(
                index,
                new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Newobj, GetDeclaredConstructors(typeof(ReservedSlotsCheckEventArgs))[0]),
                    new(OpCodes.Dup),
                    new(OpCodes.Call, Method(typeof(Player), nameof(Player.OnReservedSlot))),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(ReservedSlotsCheckEventArgs), nameof(ReservedSlotsCheckEventArgs.Result))),
                    new(OpCodes.Stloc, jumpConditions.LocalIndex),

                    // Let normal NW code proceed. UseBaseGameSystem - 0 -> Allow base game check
                    new(OpCodes.Ldloc, jumpConditions.LocalIndex),
                    new(OpCodes.Brfalse_S, continueConditions),

                    // Allow use of reserved slots, returning true CanUseReservedSlots - 1 - return true
                    new(OpCodes.Ldloc, jumpConditions.LocalIndex),
                    new(OpCodes.Ldc_I4_1),
                    new(OpCodes.Beq, returnTrue),

                    // Reserved slot rejection - CannotUseReservedSlots - 2 - return false
                    new(OpCodes.Ldloc, jumpConditions.LocalIndex),
                    new(OpCodes.Ldc_I4_2),
                    new(OpCodes.Beq, returnFalse),

                    // Allow unconditional connection - AllowConnectionUnconditionally - 3 - return true with bypass to true
                    new(OpCodes.Ldloc, jumpConditions.LocalIndex),
                    new(OpCodes.Ldc_I4_1),
                    new(OpCodes.Beq, allowUnconditional),

                    //Return true, but set bypass to true.
                    new CodeInstruction(OpCodes.Ldc_I4_1).WithLabels(allowUnconditional),
                    new CodeInstruction(OpCodes.Starg, 1),
                    //Return True
                    new CodeInstruction(OpCodes.Ldc_I4_1).WithLabels(returnTrue),
                    new(OpCodes.Ret),

                    //Return false
                    new CodeInstruction(OpCodes.Ldc_I4_0).WithLabels(returnFalse),
                    new(OpCodes.Ret),

                });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}