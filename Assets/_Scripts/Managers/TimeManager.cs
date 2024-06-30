using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Utility;
using UnityEngine;
using System.Threading;
using _Scripts.Core.Interfaces;

namespace _Scripts.Managers
{
    public class TimeManager:Singleton<TimeManager>
    {
        private List<ITimeDependent> _timeDependentObjects = new List<ITimeDependent>();
        private int _numberOfTimeDependentObjects;
        private long _currentTime;
        private CancellationTokenSource _cancellationTokenSource;

        private void Start()
        {
            Application.targetFrameRate = 60;
            _timeDependentObjects = FindObjectsOfType<MonoBehaviour>().OfType<ITimeDependent>().ToList();
            _currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _numberOfTimeDependentObjects = _timeDependentObjects.Count;
            _cancellationTokenSource = new CancellationTokenSource();
            TikTak(_cancellationTokenSource.Token).Forget();
        }

        private void OnDisable()
        {
            _cancellationTokenSource.Cancel();
        }

        private async UniTask TikTak(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _timeDependentObjects = FindObjectsOfType<MonoBehaviour>().OfType<ITimeDependent>().ToList();
                _numberOfTimeDependentObjects = _timeDependentObjects.Count;

                _currentTime++;
                for(int i=0;i<_numberOfTimeDependentObjects;i++)
                {
                    _timeDependentObjects[i].OnTimeUpdate(_currentTime);
                }
                await UniTask.Delay(1000, cancellationToken: cancellationToken);
            }
        }
    }
}
