using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;

namespace _Scripts.Managers.Matching
{
    public class BoardManager : MonoBehaviour
    {
        private CancellationTokenSource _cts;

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
            CustomUpdateAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid CustomUpdateAsync(CancellationToken token)
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        private void OnDisable()
        {
            _cts.Cancel();
        }
    }
}