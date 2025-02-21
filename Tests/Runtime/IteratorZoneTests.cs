using System.Collections;
using Moq;
using NUnit.Framework;
using R3;
using UnityEngine;
using UnityEngine.TestTools;

namespace WhiteArrow.Incremental.Tests
{
    public class IteratorZoneTests
    {
        private Mock<IIteratable> _mockIteratableTarget;

        private Mock<IMovable> _mockMovableEntity;
        private ReactiveProperty<bool> _isMoving;
        private NonMonoMovableProvider _movableEntityProvider;

        private BoxCollider _zoneCollider;
        private IteratorZone _iteratorZone;


        private const float TOLERANCE = 0.1f;

        private const float ITERATION_TIME_RATE = 0.3F;
        private const float TIME_TO_FIRST_ITERATION = 0.5F;


        [SetUp]
        public void SetUp()
        {
            _mockIteratableTarget = new Mock<IIteratable>();

            _mockMovableEntity = new Mock<IMovable>();
            _isMoving = new(false);
            _mockMovableEntity.Setup(m => m.IsMoving).Returns(_isMoving);

            var movableEntityObject = new GameObject();
            movableEntityObject.AddComponent<BoxCollider>();

            _movableEntityProvider = movableEntityObject.AddComponent<NonMonoMovableProvider>();
            _movableEntityProvider.Set(_mockMovableEntity.Object);

            _zoneCollider = new GameObject().AddComponent<BoxCollider>();
            _zoneCollider.isTrigger = true;

            var settings = new IteratorZoneSettings(TIME_TO_FIRST_ITERATION, ITERATION_TIME_RATE);
            _iteratorZone = new IteratorZone(_zoneCollider, _mockIteratableTarget.Object, settings);
        }

        [TearDown]
        public void TearDown()
        {
            _iteratorZone.Dispose();
            _isMoving.Dispose();
            Object.Destroy(_zoneCollider.gameObject);
            Object.Destroy(_movableEntityProvider.gameObject);
        }


        #region Pause/Unpause
        [UnityTest]
        public IEnumerator Pause_ShouldPreventIterations()
        {
            _iteratorZone.GetType().GetMethod("OnTriggerEnter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_iteratorZone, new object[] { _movableEntityProvider.GetComponent<Collider>() });

            yield return new WaitForSeconds(TOLERANCE);
            _iteratorZone.Pause();
            Assert.IsTrue(_iteratorZone.IsPaused.CurrentValue, "IteratorZone should be paused.");

            yield return new WaitForSeconds(TIME_TO_FIRST_ITERATION + TOLERANCE);

            _mockIteratableTarget.Verify(t => t.NextIteration(), Times.Never, "NextIteration should not be called while paused.");
        }

        [UnityTest]
        public IEnumerator Unpause_ShouldResumeIterations()
        {
            _iteratorZone.GetType().GetMethod("OnTriggerEnter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_iteratorZone, new object[] { _movableEntityProvider.GetComponent<Collider>() });

            yield return new WaitForSeconds(TOLERANCE);
            _iteratorZone.Pause();
            yield return new WaitForSeconds(TIME_TO_FIRST_ITERATION + TOLERANCE);

            _iteratorZone.Unpause();
            Assert.IsFalse(_iteratorZone.IsPaused.CurrentValue, "IteratorZone should be unpaused.");

            yield return new WaitForSeconds(TIME_TO_FIRST_ITERATION + TOLERANCE);
            _mockIteratableTarget.Verify(t => t.NextIteration(), Times.AtLeastOnce, "NextIteration should resume after unpause.");
        }
        #endregion



        #region Triggering
        [UnityTest]
        public IEnumerator TriggerEnter_ShouldStartProcess()
        {
            _iteratorZone.GetType().GetMethod("OnTriggerEnter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_iteratorZone, new object[] { _movableEntityProvider.GetComponent<Collider>() });

            yield return new WaitForSeconds(TIME_TO_FIRST_ITERATION + TOLERANCE);
            _mockIteratableTarget.Verify(t => t.NextIteration(), Times.AtLeastOnce, "NextIteration should be called when entity enters the zone.");
        }

        [UnityTest]
        public IEnumerator TriggerExit_ShouldStopProcess()
        {
            _iteratorZone.GetType().GetMethod("OnTriggerEnter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_iteratorZone, new object[] { _movableEntityProvider.GetComponent<Collider>() });

            yield return new WaitForSeconds(TIME_TO_FIRST_ITERATION - TOLERANCE);
            _iteratorZone.GetType().GetMethod("OnTriggerExit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_iteratorZone, new object[] { _movableEntityProvider.GetComponent<Collider>() });

            yield return new WaitForSeconds(TIME_TO_FIRST_ITERATION + ITERATION_TIME_RATE + TOLERANCE);
            _mockIteratableTarget.Verify(t => t.NextIteration(), Times.Never, "NextIteration should stop when entity exits the zone.");
        }
        #endregion


        #region Time rate
        [UnityTest]
        public IEnumerator IterationTimeRate_ShouldBeMaintained()
        {
            _iteratorZone.GetType().GetMethod("OnTriggerEnter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_iteratorZone, new object[] { _movableEntityProvider.GetComponent<Collider>() });

            yield return new WaitForSeconds(TIME_TO_FIRST_ITERATION + TOLERANCE);

            _mockIteratableTarget.Verify(t => t.NextIteration(), Times.Once, "First NextIteration() should have been called after TIME_TO_FIRST_ITERATION.");

            yield return new WaitForSeconds(ITERATION_TIME_RATE);

            _mockIteratableTarget.Verify(t => t.NextIteration(), Times.Exactly(2), "NextIteration() should be called twice, once per iteration time rate.");

            yield return new WaitForSeconds(ITERATION_TIME_RATE);

            _mockIteratableTarget.Verify(t => t.NextIteration(), Times.Exactly(3), "NextIteration() should be called three times, once per iteration time rate.");
        }
        #endregion
    }
}