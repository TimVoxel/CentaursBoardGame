using UnityEngine;

namespace CentaursBoardGame
{
    public class RobotImage : MonoBehaviour
    {
        [SerializeField] private Game _game;

        public void OnZoomInAnimationEnded()
        {
            _game.EnterShowAttackState();
        }
    }
}
