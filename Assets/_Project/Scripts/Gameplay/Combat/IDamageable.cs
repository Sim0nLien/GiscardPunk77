namespace GiscardPunk77.Gameplay
{
    public interface IDamageable
    {
        bool IsDead { get; }

        bool TryApplyDamage(in DamageInfo damage);
    }
}
