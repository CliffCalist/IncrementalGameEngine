using System;
using System.Linq;
using ObservableCollections;
using R3;

namespace WhiteArrow.Incremental
{
    public static class ObservableCollectionExtensions
    {
        #region ObserveAllElements
        /// <summary>
        /// Observes changes in an IObservableCollection and combines results from a selector for each item into a single observable.
        /// Automatically handles additions, removals, and replacements in the list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <typeparam name="TElement">The type of the observable values produced by the selector.</typeparam>
        /// <param name="collection">The IObservableCollection to observe.</param>
        /// <param name="elementSelector">A function that projects each item in the list into an observable value.</param>
        /// <returns>An observable that combines results from all items in the list.</returns>
        public static Observable<TElement[]> ObserveAllElements<T, TElement>(
            this IObservableCollection<T> collection,
            Func<T, Observable<TElement>> elementSelector)
        {
            var initial = Observable.Return(Observable.CombineLatest(collection.Select(elementSelector)));
            var updates = collection.ObserveChanged().Select(_ => Observable.CombineLatest(collection.Select(elementSelector)));
            return initial.Concat(updates).Switch();
        }

        /// <summary>
        /// Observes changes in an IObservableCollection and combines results from a selector for each item into a single observable.
        /// Allows transforming the combined array into a single result.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <typeparam name="TElement">The type of the observable values produced by the selector.</typeparam>
        /// <typeparam name="TFinal">The type of the final combined result.</typeparam>
        /// <param name="collection">The IObservableCollection to observe.</param>
        /// <param name="elementSelector">A function that projects each item in the list into an observable value.</param>
        /// <param name="resultSelector">A function that transforms the combined array into a single result.</param>
        /// <returns>An observable that produces a single result based on all items in the list.</returns>
        public static Observable<TFinal> ObserveAllElements<T, TElement, TFinal>(
            this IObservableCollection<T> collection,
            Func<T, Observable<TElement>> elementSelector,
            Func<TElement[], TFinal> resultSelector)
        {
            return ObserveAllElements(collection, elementSelector).Select(resultSelector);
        }


        /// <summary>
        /// Observes changes in an <see cref="IObservableCollection{T}"/> and determines whether all elements satisfy a given condition.
        /// Automatically handles additions, removals, and replacements in the list.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="collection">The <see cref="IObservableCollection{T}"/> to observe.</param>
        /// <param name="elementSelector">
        /// A function that projects each item in the collection into an <see cref="Observable{bool}"/> indicating whether the item satisfies the condition.
        /// </param>
        /// <returns>
        /// An <see cref="Observable{bool}"/> that emits <c>true</c> if all elements in the collection satisfy the condition
        /// defined by <paramref name="elementSelector"/>, and <c>false</c> otherwise.
        /// </returns>
        public static Observable<bool> ObserveAllElementsTrue<T>(
            this IObservableCollection<T> collection,
            Func<T, Observable<bool>> elementSelector)
        {
            return ObserveAllElements(
                collection,
                elementSelector,
                states => states.All(s => s));
        }
        #endregion



        #region ObserveAddWithExisting
        /// <summary>
        /// Creates an observable stream for additions to the collection, including existing elements.
        /// </summary>
        /// <typeparam name="T">The type of elements in the observable collection.</typeparam>
        /// <param name="collection">The observable collection to monitor.</param>
        /// <returns>An observable stream of added elements.</returns>
        public static Observable<T> ObserveAddWithExisting<T>(this IObservableCollection<T> collection)
        {
            return Observable.Create<T>(observer =>
            {
                foreach (var item in collection)
                    observer.OnNext(item);

                return collection.ObserveAdd()
                    .Select(arg => arg.Value)
                    .Subscribe(observer);
            });
        }

        public static Observable<TInner> ObserveAddWithExisting<T, TInner>(
            this IObservableCollection<T> collection,
            Func<T, IObservableCollection<TInner>> innerSelector)
        {
            return collection
                .ObserveAddWithExisting()
                .Select(e => innerSelector(e).ObserveAddWithExisting())
                .Switch();
        }

        public static Observable<TInner2> ObserveAddWithExisting<T, TInner1, TInner2>(
            this IObservableCollection<T> collection,
            Func<T, IObservableCollection<TInner1>> innerSelector1,
            Func<TInner1, IObservableCollection<TInner2>> innerSelector2)
        {
            return collection
                .ObserveAddWithExisting(innerSelector1)
                .Select(e => innerSelector2(e).ObserveAddWithExisting())
                .Switch();
        }

        public static Observable<TInner3> ObserveAddWithExisting<T, TInner1, TInner2, TInner3>(
            this IObservableCollection<T> collection,
            Func<T, IObservableCollection<TInner1>> innerSelector1,
            Func<TInner1, IObservableCollection<TInner2>> innerSelector2,
            Func<TInner2, IObservableCollection<TInner3>> innerSelector3)
        {
            return collection
                .ObserveAddWithExisting(innerSelector1, innerSelector2)
                .Select(e => innerSelector3(e).ObserveAddWithExisting())
                .Switch();
        }

        public static Observable<TInner4> ObserveAddWithExisting<T, TInner1, TInner2, TInner3, TInner4>(
            this IObservableCollection<T> collection,
            Func<T, IObservableCollection<TInner1>> innerSelector1,
            Func<TInner1, IObservableCollection<TInner2>> innerSelector2,
            Func<TInner2, IObservableCollection<TInner3>> innerSelector3,
            Func<TInner3, IObservableCollection<TInner4>> innerSelector4)
        {
            return collection
                .ObserveAddWithExisting(innerSelector1, innerSelector2, innerSelector3)
                .Select(e => innerSelector4(e).ObserveAddWithExisting())
                .Switch();
        }
        #endregion
    }
}