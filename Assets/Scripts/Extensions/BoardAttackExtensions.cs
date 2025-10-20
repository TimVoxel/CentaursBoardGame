using System.IO;

public static class BoardAttackExtensions
{
    public static void WriteTo(this BoardAttack attack, TextWriter writer)
    {
        switch (attack)
        {
            case ShuffleRingsAttack shuffleAttack:
                writer.WriteLine($"Attack Type: {shuffleAttack.Type}");
                writer.WriteLine("Rings and sectors:");
                foreach (var kvp in shuffleAttack.Rotations)
                {
                    writer.WriteLine($"  Ring: {kvp.Key}, Sector Count: {kvp.Value}");
                }
                break;

            case AttackRingAttack ringAttack:
                writer.WriteLine($"Attack Type: {ringAttack.Type}");
                writer.WriteLine($"  Ring: {ringAttack.Ring}");
                break;

            case AttackCentreEntrancesAttack centreAttack:
                writer.WriteLine($"Attack Type: {centreAttack.Type}");
                break;

            case ForceDiscardCardsAmount discardAttack:
                writer.WriteLine($"Attack Type: {discardAttack.Type}");
                writer.WriteLine($"  Card Count: {discardAttack.CardCount}");
                break;

            case ForceSwapHandsAttack swapHandsAttack:
                writer.WriteLine($"Attack Type: {swapHandsAttack.Type}");
                writer.WriteLine($"  First Player: {swapHandsAttack.First.WriteToString()}");
                writer.WriteLine($"  Second Player: {swapHandsAttack.Second.WriteToString()}");
                break;

            case RotateRingAttack rotateAttack:
                writer.WriteLine($"Attack Type: {rotateAttack.Type}");
                writer.WriteLine($"  Ring: {rotateAttack.Ring}");
                writer.WriteLine($"  Sector Count: {rotateAttack.SectorCount}");
                break;

            default:
                throw new System.Exception($"Unexpected attack type: {attack.GetType()}");
        }
    }
}