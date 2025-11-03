using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{

	public class SinFloatAnimation : MonoBehaviour
	{
		[SerializeField] private Transform _targetTransform;
		[SerializeField] private float _multiplier;
        [SerializeField] private float _speedMultiplier;

        [SerializeField] private Transform? _startPosProvider;

        private Vector3 _startPos;

		private float _time = 0f;

        private void Awake()
        {
            _startPos = _targetTransform.position;
        }

        private void Update()
        {
            _time += Time.deltaTime * _speedMultiplier;

            if (_time >= 1f)
            {
                _time = 0f;
            }

            var sinValue = Mathf.Sin(_time * Mathf.PI * 2f) * _multiplier;

            var startPos = _startPosProvider != null 
                ? _startPosProvider.position
                : _startPos;

            var x = startPos.x + sinValue;
            var y = startPos.y + sinValue;

            _targetTransform.position = new Vector3(x, y, 0f);
        }
    }
}