using DemoPulse.Models;

namespace DemoPulse.Services.Parser
{
    public class UtilityAnalyzer
    {
        public void OnPlayerHurt(PlayerStats attackerStat, int damageHealth, string weapon)
        {
            attackerStat.TotalDamage += damageHealth;

            if (weapon.Contains("molotov") || weapon.Contains("incgrenade") ||
                weapon.Contains("hegrenade") || weapon.Contains("inferno"))
            {
                attackerStat.UtilityDamage += damageHealth;
            }
        }

        public void OnPlayerBlind(PlayerStats attackerStat, int attackerTeam, int victimTeam, float blindDuration)
        {
            if (blindDuration < 0.5f) return;

            if (victimTeam == attackerTeam)
            {
                attackerStat.TeamFlashes++;
            }
            else
            {
                attackerStat.EnemiesBlinded++;
                attackerStat.TotalBlindDuration += blindDuration;
            }
        }

        public void OnFlashbangDetonate(PlayerStats throwerStat)
        {
            throwerStat.FlashesThrown++;
        }
    }
}
