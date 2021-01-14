using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cubeage
{
    [ExecuteInEditMode]
    public class AutoBlink : MonoBehaviour
    {
		public bool isActive = true;
		public AvatarController avatarController;

		public float ratio_Close = 0;
		public float ratio_HalfClose = 50;
		public float ratio_Open = 100;

		private bool timerStarted = false;
		private bool isBlink = false;

		public float timeBlink = 0.4f;
		private float timeRemining = 0.0f;

		[Range(0.0f, 1.0f)]
		public float threshold = 0.3f;
		public float interval = 3.0f;

        private void Reset()
        {
			avatarController = gameObject.GetComponent<AvatarController>();
        }

        enum Status
		{
			Close,
			HalfClose,
			Open
		}


		private Status eyeStatus;

		void Awake()
		{
		}

		void Start()
		{
			ResetTimer();
			StartCoroutine("RandomChange");
		}


		void ResetTimer()
		{
			timeRemining = timeBlink;
			timerStarted = false;
		}


		void Update()
		{
			if (!timerStarted)
			{
				eyeStatus = Status.Close;
				timerStarted = true;
			}
			if (timerStarted)
			{
				timeRemining -= Time.deltaTime;
				if (timeRemining <= 0.0f)
				{
					eyeStatus = Status.Open;
					ResetTimer();
				}
				else if (timeRemining <= timeBlink * 0.3f)
				{
					eyeStatus = Status.HalfClose;
				}
			}
		}

		void LateUpdate()
		{
			if (!isActive)
				return;

			if (isBlink)
			{
				var controller = avatarController.GetController("eye");
				switch (eyeStatus)
				{
					case Status.Close:
						controller.Value = ratio_Close;
						break;
					case Status.HalfClose:
						controller.Value = ratio_HalfClose;
						break;
					case Status.Open:
						controller.Value = ratio_Open;
						isBlink = false;
						break;
				}
			}
		}

		// ランダム判定用関数
		IEnumerator RandomChange()
		{
			while (true)
			{
				var _seed = Random.Range(0.0f, 1.0f);
				if (!isBlink && _seed > threshold)
				{
					isBlink = true;
				}
				yield return new WaitForSeconds(interval);
			}
		}
	}
}