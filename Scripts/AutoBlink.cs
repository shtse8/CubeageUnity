using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cubeage
{
	
	public class EyeBlinkController
	{
		private Transform Upper { get; }
		private Transform Lower { get; }

		private readonly Quaternion _upperLocalRotation;
		private readonly Quaternion _lowerLocalRotation;
		private readonly Quaternion _upperLocalRotationTarget;
		private readonly Quaternion _lowerLocalRotationTarget;

		private readonly Vector3 _upperLocalPosition;
		private readonly Vector3 _lowerLocalPosition;
		private readonly Vector3 _upperLocalPositionTarget;
		private readonly Vector3 _lowerLocalPositionTarget;
		
		public EyeBlinkController(Transform upper, Transform lower, float ratio)
		{
			Upper = upper;
			Lower = lower;
			
			// Update Origins
			_upperLocalPosition = Upper.localPosition;
			_lowerLocalPosition = Lower.localPosition;
			_upperLocalRotation = Upper.localRotation;
			_lowerLocalRotation = Lower.localRotation;
				
			// Update Targets
			_upperLocalPositionTarget = Upper.parent.transform.InverseTransformPoint(Upper.position +
				(Lower.position - Upper.position) * ratio);
			_lowerLocalPositionTarget =
				Lower.parent.transform.InverseTransformPoint(Lower.position +
				                                             (Upper.position -
				                                              Lower.position) * (1 - ratio));

			_upperLocalRotationTarget = Quaternion.Inverse(Upper.parent.rotation) *
			                           Quaternion.Slerp(Upper.rotation, Lower.rotation, ratio);
			_lowerLocalRotationTarget = Quaternion.Inverse(Lower.parent.rotation) *
			                           Quaternion.Slerp(Lower.rotation, Upper.rotation, (1-ratio));
		}

		public void Set(float ratio)
		{
			Upper.localPosition = _upperLocalPosition + (_upperLocalPositionTarget - _upperLocalPosition) * ratio;
			Lower.localPosition = _lowerLocalPosition + (_lowerLocalPositionTarget - _lowerLocalPosition) * ratio;
			Upper.localRotation = Quaternion.Slerp(_upperLocalRotation, _upperLocalRotationTarget, ratio); 
			Lower.localRotation = Quaternion.Slerp(_lowerLocalRotation, _lowerLocalRotationTarget, ratio);
		}
	}
	
    [AddComponentMenu("Cubeage/Avatar Controller")]
    public class AutoBlink : MonoBehaviour
    {
		public bool isActive = true;
		
		public Transform upperLeftEyeLip;
		public Transform lowerLeftEyeLip;
		public Transform upperRightEyeLip;
		public Transform lowerRightEyeLip;
		
		[Range(0.1f, 1.0f)]
		public float duration = 0.3f;

		[Range(0.0f, 1.0f)]
		public float threshold = 0.3f;
		public float interval = 3.0f;

		[Range(0.0f, 1.0f)]
		public float ratio = 0.9f;
		
		[Range(0, 10)]
		public int steps = 2;
		
		private EyeBlinkController _leftEyeBlinkController;
		private EyeBlinkController _rightEyeBlinkController;
		private float _timeRemining;
		
		
        private void Reset()
        {
			upperLeftEyeLip = gameObject.GetComponentsInChildren<Transform>()
				.FirstOrDefault(x => x.name == "ShangYanPi_L_blink_ctrl");
			upperRightEyeLip = gameObject.GetComponentsInChildren<Transform>()
				.FirstOrDefault(x => x.name == "ShangYanPi_R_blink_ctrl");
			lowerLeftEyeLip = gameObject.GetComponentsInChildren<Transform>()
				.FirstOrDefault(x => x.name == "XiaYanPi_L_blink_ctrl");
			lowerRightEyeLip = gameObject.GetComponentsInChildren<Transform>()
				.FirstOrDefault(x => x.name == "XiaYanPi_R_blink_ctrl");

        }

		void Start()
		{
			StartCoroutine(nameof(RandomChange));
		}

		void Blink()
		{
			_leftEyeBlinkController = new EyeBlinkController(upperLeftEyeLip, lowerLeftEyeLip, ratio);
			_rightEyeBlinkController = new EyeBlinkController(upperRightEyeLip, lowerRightEyeLip, ratio);
			_timeRemining = duration;
		}

		void Update()
		{
			if (_timeRemining <= 0)
				return;
			
			_timeRemining -= Time.deltaTime;
			if (_timeRemining < 0)
				_timeRemining = 0;
			var setRatio = _timeRemining / duration;
			if (steps > 0)
				setRatio = Mathf.Ceil(setRatio / (1f / steps)) * (1f / steps);
			_rightEyeBlinkController.Set(setRatio);
			_leftEyeBlinkController.Set(setRatio);
		}

		// ランダム判定用関数
		private IEnumerator RandomChange()
		{
			while (true)
			{
				if (isActive)
				{
					var seed = Random.Range(0.0f, 1.0f);
					if (_timeRemining <= 0 && seed > threshold)
						Blink();
				}
				yield return new WaitForSeconds(interval);
			}
		}
	}
}