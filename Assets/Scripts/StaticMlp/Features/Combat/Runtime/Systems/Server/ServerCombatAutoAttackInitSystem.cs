using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerCombatAutoAttackInitSystem : ISystem
    {
        public void Update()
        {
            if (!SW.HasResource<CombatAutoAttackConfig>())
            {
                SW.SetResource(new CombatAutoAttackConfig());
                return;
            }

            if (SW.GetResource<CombatAutoAttackConfig>() == null)
                throw new InvalidOperationException("Combat auto attack config resource exists but is null.");
        }
    }
}
