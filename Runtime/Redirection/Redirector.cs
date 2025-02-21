using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;

namespace WhiteArrow.Incremental
{
    public class Redirector<TEntityViewModel> : DisposableBase
        where TEntityViewModel : IMovableByOneCallProvider
    {
        private readonly ObservableList<IRedirectionTargetProvider<TEntityViewModel>> _providers = new();
        public IReadOnlyObservableList<IRedirectionTargetProvider<TEntityViewModel>> TargetProviders => _providers;

        public ReadOnlyReactiveProperty<bool> HasFreeTarget { get; }


        public Redirector(params IRedirectionTargetProvider<TEntityViewModel>[] initialProviders)
        {
            HasFreeTarget = _providers.ObserveChanged()
                .Select(_ =>
                    Observable.CombineLatest(
                        _providers.Select(p =>
                            Observable.CombineLatest(
                                p.RedirectionTarget.IsEnabled,
                                p.RedirectionTarget.HasFreeSeat,
                                (isEnabled, hasFreeSeat) => isEnabled && hasFreeSeat
                            )
                        )
                    )
                    .Select(states => states.Any(s => s))
                )
                .Switch()
                .ToReadOnlyReactiveProperty();

            if (initialProviders.Count() > 0)
                _providers.AddRange(initialProviders);

            BuildPermanentDisposable(HasFreeTarget);
        }



        /// <summary>
        /// Adds new targets to the collection.
        /// </summary>
        /// <param name="newProviders">The targets to be added.</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided targets are null.</exception>
        public void AddTargets(IEnumerable<IRedirectionTargetProvider<TEntityViewModel>> newProviders)
        {
            ThrowIfDisposed();

            if (newProviders == null)
                throw new ArgumentNullException(nameof(newProviders));

            foreach (var target in newProviders.Where(p => p != null && !_providers.Contains(p)))
                _providers.Add(target);
        }

        public void AddTarget(IRedirectionTargetProvider<TEntityViewModel> newProvider)
        {
            ThrowIfDisposed();

            if (newProvider == null)
                throw new ArgumentNullException(nameof(newProvider));

            _providers.Add(newProvider);
        }



        /// <summary>
        /// Removes specified targets from the collection.
        /// </summary>
        /// <param name="providersToRemove">The targets to be removed.</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided targets are null.</exception>
        public void RemoveTargets(IEnumerable<IRedirectionTargetProvider<TEntityViewModel>> providersToRemove)
        {
            ThrowIfDisposed();

            if (providersToRemove == null)
                throw new ArgumentNullException(nameof(providersToRemove));

            foreach (var provider in providersToRemove.Where(t => t != null))
                _providers.Remove(provider);
        }

        public void RemoveTarget(IRedirectionTargetProvider<TEntityViewModel> providerToRemove)
        {
            ThrowIfDisposed();

            if (providerToRemove == null)
                throw new ArgumentNullException(nameof(providerToRemove));

            _providers.Remove(providerToRemove);
        }



        /// <summary>
        /// Attempts to redirect an entity to an available target.
        /// </summary>
        /// <param name="entity">The entity to be redirected.</param>
        /// <returns>An observable indicating whether the redirection was successful.</returns>
        public bool TryRedirect(TEntityViewModel entity)
        {
            ThrowIfDisposed();

            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var suitableProviders =
                _providers.Where(p => p.RedirectionTarget.IsEnabled.Value &&
                    p.RedirectionTarget.HasFreeSeat.CurrentValue).ToList();

            if (!suitableProviders.Any())
                return false;

            var randomIndex = UnityEngine.Random.Range(0, suitableProviders.Count);
            var provider = suitableProviders[randomIndex];
            return provider.RedirectionTarget.TryPlace(entity);
        }

        /// <summary>
        /// Attempts to redirect an entity to an available target of the specified type.
        /// </summary>
        /// <typeparam name="T">The expected type of the target.</typeparam>
        /// <param name="entity">The entity to be redirected.</param>
        /// <returns>
        /// An observable providing the redirected target of type <typeparamref name="T"/> if successful, 
        /// or an error if no suitable target was found.
        /// </returns>
        public bool TryRedirect<T>(TEntityViewModel entity, out T targetProvider) where T : class
        {
            ThrowIfDisposed();

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));


            var suitableProviders = _providers.Where(p => p.RedirectionTarget.IsEnabled.Value && p.RedirectionTarget.HasFreeSeat.CurrentValue).ToList();

            if (!suitableProviders.Any())
            {
                targetProvider = default;
                return false;
            }

            var randomIndex = UnityEngine.Random.Range(0, suitableProviders.Count);
            var freeProvider = suitableProviders[randomIndex];

            if (freeProvider is T castedProvider && freeProvider.RedirectionTarget.TryPlace(entity))
            {
                targetProvider = castedProvider;
                return true;
            }
            else
            {
                targetProvider = default;
                return false;
            }
        }



        protected override void DisposeProtected()
        {
            base.DisposeProtected();
            _providers.Clear();
        }
    }
}