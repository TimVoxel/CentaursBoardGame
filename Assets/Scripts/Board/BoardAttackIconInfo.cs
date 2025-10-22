using System.Text;
using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{
    [System.Serializable]
    public struct BoardAttackIconInfo
    {
        [SerializeField] private BoardAttackType _representedAttackType;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private string _name;
        [SerializeField] private string _explanation;
        
        public BoardAttackType RepresentedAttackType => _representedAttackType;
        public Sprite Sprite => _sprite;
        public string Name => _name;
        public string Explanation => _explanation;

        public BoardAttackIconInfo(BoardAttackType representedAttackType,
                                   Sprite sprite,
                                   string name,
                                   string description)
        {
            _representedAttackType = representedAttackType;
            _sprite = sprite;
            _name = name;
            _explanation = description;
        }

        public static string? GetDetails(BoardAttack attack)
        {
            //Could do this with polymorphism but I don't feel like making a separate info for every type of attack

            switch (attack)
            {
                case AttackCentreEntrancesAttack attackCentreEntrancesAttack:
                    return "Centre entrances are attacked!";

                case AttackRingAttack attackRingAttack:
                    return $"{attackRingAttack.Ring} ring is attacked!";

                case RotateRingAttack rotateRingAttack:
                    return $"{rotateRingAttack.Ring} ring is rotated {rotateRingAttack.SectorCount} sectors!";

                case ShuffleRingsAttack shuffleRingsAttack:
                    var stringBuilder = new StringBuilder();

                    stringBuilder.AppendLine("Crazy shuffle!");
                    
                    foreach (var rotation in shuffleRingsAttack.Rotations)
                    {
                        stringBuilder.AppendLine($"{rotation.Key} ring is rotated {rotation.Value} sectors!");
                    }

                    return stringBuilder.ToString();

                case ForceDiscardCardsAmount forceDiscardCardsAmount:
                    return forceDiscardCardsAmount.CardCount == 1
                        ? "Every player discards a card!"
                        : $"Every player discards {forceDiscardCardsAmount.CardCount} cards!";

                case ForceSwapHandsAttack forceSwapHandsAttack:
                    return $"Player {forceSwapHandsAttack.First.Name} swaps hands with player {forceSwapHandsAttack.Second.Name}";

                default:
                    throw new System.Exception($"Unexpected attack type: {attack.GetType()}");
            }
        }
    }
}