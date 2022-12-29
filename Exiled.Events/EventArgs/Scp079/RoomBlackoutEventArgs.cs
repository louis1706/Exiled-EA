// -----------------------------------------------------------------------
// <copyright file="RoomBlackoutEventArgs.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.EventArgs.Scp079
{
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using Exiled.Events.EventArgs.Interfaces;

    using MapGeneration;
    using PlayerRoles.PlayableScps.Scp079;

    /// <summary>
    ///     Contains all information before SCP-079 lockdowns a room.
    /// </summary>
    public class RoomBlackoutEventArgs : IPlayerEvent, IDeniableEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RoomBlackoutEventArgs" /> class.
        /// </summary>
        /// <param name="player">
        ///     <inheritdoc cref="Player" />
        /// </param>
        /// <param name="roomIdentifier">
        ///     <inheritdoc cref="Room" />
        /// </param>
        /// <param name="auxiliaryPowerCost">
        ///     <inheritdoc cref="AuxiliaryPowerCost" />
        /// </param>
        /// <param name="scp079HudTranslation">
        ///     <inheritdoc cref="Scp079HudTranslation" />
        /// </param>

        public RoomBlackoutEventArgs(Player player, RoomIdentifier roomIdentifier, float auxiliaryPowerCost, Scp079HudTranslation scp079HudTranslation)
        {
            Player = player;
            Room = Room.Get(roomIdentifier);
            AuxiliaryPowerCost = auxiliaryPowerCost;
            Scp079HudTranslation = scp079HudTranslation;
            IsAllowed = scp079HudTranslation is Scp079HudTranslation.Zoom;
        }

        /// <summary>
        ///     Gets the player who's controlling SCP-079.
        /// </summary>
        public Player Player { get; }

        /// <summary>
        ///     Gets the <see cref="RoomIdentifier" /> of the room that will be locked down.
        /// </summary>
        public Room Room { get; }

        /// <summary>
        ///     Gets or sets the <see cref="PlayerRoles.PlayableScps.Scp079.Scp079HudTranslation" /> send back to player.
        /// </summary>
        public Scp079HudTranslation Scp079HudTranslation { get; set; }

        /// <summary>
        ///     Gets or sets the amount of auxiliary power required to lockdown a room.
        /// </summary>
        public float AuxiliaryPowerCost { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not SCP-079 can lockdown a room.
        /// </summary>
        public bool IsAllowed { get; set; }
    }
}