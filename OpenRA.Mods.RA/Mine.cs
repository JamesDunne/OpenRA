﻿#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.RA.Activities;
using OpenRA.Mods.RA.Move;
using OpenRA.Traits;
using OpenRA.FileFormats;

namespace OpenRA.Mods.RA
{
	class MineInfo : ITraitInfo
	{
		public readonly string[] CrushClasses = { };
		[WeaponReference] public readonly string Weapon = "ATMine";
		public readonly bool AvoidFriendly = true;
		public readonly string[] DetonateClasses = { };

		public object Create(ActorInitializer init) { return new Mine(init, this); }
	}

	class Mine : ICrushable, IOccupySpace, ISync
	{
		readonly Actor self;
		readonly MineInfo info;
		[Sync] readonly CPos location;

		public Mine(ActorInitializer init, MineInfo info)
		{
			this.self = init.self;
			this.info = info;
			this.location = init.Get<LocationInit,CPos>();
		}

		public void WarnCrush(Actor crusher) {}

		public void OnCrush(Actor crusher)
		{
			if (crusher.HasTrait<MineImmune>() || self.Owner.Stances[crusher.Owner] == Stance.Ally)
				return;

			var mobile = crusher.TraitOrDefault<Mobile>();
			if (mobile != null && !info.DetonateClasses.Intersect(mobile.Info.Crushes).Any())
				return;

			Combat.DoExplosion(self, info.Weapon, crusher.CenterLocation, 0);
			self.QueueActivity(new RemoveSelf());
		}

		public bool CrushableBy(string[] crushClasses, Player owner)
		{
			return info.CrushClasses.Intersect(crushClasses).Any();
		}

		public CPos TopLeft { get { return location; } }

		public IEnumerable<Pair<CPos, SubCell>> OccupiedCells() { yield return Pair.New(TopLeft, SubCell.FullCell); }
		public PPos PxPosition { get { return Util.CenterOfCell( location ); } }
	}

	/* tag trait for stuff that shouldnt trigger mines */
	class MineImmuneInfo : TraitInfo<MineImmune> { }
	class MineImmune { }
}
