using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;

namespace Spells
{
    public class AkaliMota : GameScript
    {
        private Particle _visualMark;
        private Champion _owningChampion;
		private Spell _owningSpell;
        private AttackableUnit _markTarget;
		private bool _listenerAdded;
	
        public void OnActivate(Champion owner)
        {
            _owningChampion = owner;
			_markTarget = null;
			_owningSpell = null;
			_listenerAdded = false;
			_visualMark = null;
        }

        private void OnProc(AttackableUnit target, bool isCrit)
        {
            if (_visualMark == null)
            {
                return;
            }

            if ((_markTarget == null) || (_markTarget != target))
            {
                return;
            }

            if (_owningSpell == null)
            {
                _owningSpell = _owningChampion.GetSpellByName("AkaliMota"); // Since _owningSpell is set on-cast, this should likely be unnessasary, but is here for safety reasons
            }

            ApiFunctionManager.LogInfo("Mark got procced, removing it earlier");
            ApiFunctionManager.RemoveParticle(_visualMark);
            ApiFunctionManager.AddParticleTarget(_owningChampion, "akali_mark_impact_tar.troy", target);

            var ap = _owningChampion.GetStats().AbilityPower.Total * 0.5f;
            var damage = 20 + _owningSpell.Level * 25 + ap;
            target.TakeDamage(_owningChampion, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_PASSIVE, false);

            var energy = 15 + _owningSpell.Level * 5;

            _owningChampion.GetStats().CurrentMana += energy;

            _visualMark = null;
            _markTarget = null;
        }

        public void OnDeactivate(Champion owner)
        {
        }

        public void OnStartCasting(Champion owner, Spell spell, AttackableUnit target)
        {
			_owningSpell = spell; // set the spell that should be used for calculating ability target
        }

        public void OnFinishCasting(Champion owner, Spell spell, AttackableUnit target)
        {
            spell.AddProjectileTarget("AkaliMota", target);
            if (!_listenerAdded)
            {
                ApiEventManager.OnAutoAttackHit.AddListener(this, _owningChampion, OnProc);
            }
        }

        public void ApplyEffects(Champion owner, AttackableUnit target, Spell spell, Projectile projectile)
        {
            _markTarget = target;
            var ap = owner.GetStats().AbilityPower.Total * 0.4f;
            var damage = 15 + spell.Level * 20 + ap;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            _visualMark = ApiFunctionManager.AddParticleTarget(owner, "akali_markOftheAssasin_marker_tar_02.troy", target, 1, "");
            projectile.setToRemove();
            ApiFunctionManager.CreateTimer(6.0f, () =>
            {
                if (_visualMark == null)
                    return;
                ApiFunctionManager.LogInfo("6 second timer finished, removing the mark of the assassin");
                ApiFunctionManager.RemoveParticle(_visualMark);
                _markTarget = null;
                _visualMark = null;
            });
        }

        public void OnUpdate(double diff)
        {
        }
    }
}