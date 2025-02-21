using System;
using System.Collections;
using Moq;
using NUnit.Framework;
using R3;
using UnityEngine;
using UnityEngine.TestTools;

namespace WhiteArrow.Incremental.Tests
{
    public class MovableExtentionsTests
    {
        private Mock<IMovable> _movableMock;
        private ReactiveProperty<bool> _isMoving;


        [SetUp]
        public void SetUp()
        {
            _isMoving = new ReactiveProperty<bool>(true);
            _movableMock = new Mock<IMovable>();
            _movableMock.Setup(m => m.IsMoving).Returns(_isMoving.ToReadOnlyReactiveProperty());
        }

        [TearDown]
        public void TearDown()
        {
            _isMoving?.Dispose();
        }



        #region ObserveNonMoveByTime
        [UnityTest]
        public IEnumerator ObserveNonMoveByTime_TriggersAfterDelay_WhenStationary()
        {
            var delay = TimeSpan.FromMilliseconds(200);
            var wasTriggered = false;

            var observable = _movableMock.Object.ObserveNonMoveByTime(delay);
            var disposable = observable.Subscribe(_ => wasTriggered = true);

            _isMoving.Value = false;
            yield return new WaitForSeconds(0.3f);

            Assert.IsTrue(wasTriggered, "Observable did not trigger after the expected time.");
            disposable.Dispose();
        }

        [UnityTest]
        public IEnumerator ObserveNonMoveByTime_DoesNotTrigger_IfMovementResumesBeforeTimeout()
        {
            var delay = TimeSpan.FromMilliseconds(200);
            var wasTriggered = false;

            var observable = _movableMock.Object.ObserveNonMoveByTime(delay);
            var disposable = observable.Subscribe(_ => wasTriggered = true);

            _isMoving.Value = false;
            yield return new WaitForSeconds(0.1f);
            _isMoving.Value = true;
            yield return new WaitForSeconds(0.2f);

            Assert.IsFalse(wasTriggered, "Observable should not have triggered since movement resumed.");
            disposable.Dispose();
        }

        [UnityTest]
        public IEnumerator ObserveNonMoveByTime_TriggersAfterDelay_IfAlreadyStationary()
        {
            var delay = TimeSpan.FromMilliseconds(200);
            _isMoving.Value = false;
            var wasTriggered = false;

            var observable = _movableMock.Object.ObserveNonMoveByTime(delay);
            var disposable = observable.Subscribe(_ => wasTriggered = true);

            yield return new WaitForSeconds(0.3f);

            Assert.IsTrue(wasTriggered, "Observable did not trigger even though IMovable was stationary before invocation.");
            disposable.Dispose();
        }

        [Test]
        public void ObserveNonMoveByTime_ThrowsException_WhenTimeIsNonPositive()
        {
            var invalidTime = TimeSpan.Zero;

            Assert.Throws<ArgumentOutOfRangeException>(() => _movableMock.Object.ObserveNonMoveByTime(invalidTime));
        }
        #endregion
    }
}
