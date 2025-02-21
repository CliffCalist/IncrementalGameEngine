using System;
using R3;

namespace WhiteArrow.Incremental
{
    public static class MovableExtentions
    {
        /// <summary>
        /// Creates an Observable that triggers when the movable view model remains stationary for the specified time.
        /// </summary>
        /// <param name="time">The duration in seconds the view model must remain stationary to trigger the Observable.</param>
        /// <returns>An Observable that emits the movable view model instance after the specified time of no movement.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the time is less than or equal to zero.</exception>
        public static Observable<Unit> ObserveNonMoveByTime(this IMovable movable, TimeSpan time)
        {
            if (time == TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(time));

            return movable.IsMoving
                .Select(isMoving =>
                {
                    if (isMoving)
                        return Observable.Empty<Unit>();
                    else return Observable.Timer(time)
                        .TakeUntil(movable.IsMoving.Where(moving => moving));
                })
                .Switch();
        }
    }
}