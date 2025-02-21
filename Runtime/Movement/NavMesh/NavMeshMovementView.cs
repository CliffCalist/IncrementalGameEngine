using System;
using R3;
using UnityEngine;
using UnityEngine.AI;

namespace WhiteArrow.Incremental
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshMovementView : MonoBehaviour
    {
        public virtual void Bind(INavMeshMovementProvider provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));


            var agent = GetComponent<NavMeshAgent>();

            if (IsAgentInitialized())
                BindToAgent();
            else
            {
                Observable.EveryUpdate()
                    .Where(_ => IsAgentInitialized())
                    .Take(1)
                    .Subscribe(_ => BindToAgent());
            }



            bool IsAgentInitialized() => agent.isActiveAndEnabled && agent.isOnNavMesh;

            void BindToAgent()
            {
                provider.NavMeshMovement.LastCalculatedSelfPosition
                    .Where(p => p.HasValue)
                    .ObserveOnMainThread()
                    .Subscribe(p =>
                    {
                        agent.Warp(p.Value);
                    })
                    .AddTo(this);

                provider.NavMeshMovement.TargetPosition
                    .ObserveOnMainThread()
                    .Subscribe(p =>
                    {
                        if (p.HasValue)
                        {
                            if (!agent.SetDestination(p.Value))
                                Debug.LogWarning($"Can't set destination for {nameof(INavMeshMovementProvider)}", agent);
                        }
                        else if (agent.hasPath)
                            agent.ResetPath();
                    })
                    .AddTo(this);
            }
        }
    }
}