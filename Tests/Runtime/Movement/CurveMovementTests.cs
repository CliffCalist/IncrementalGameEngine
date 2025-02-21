using System.Collections;
using NUnit.Framework;
using R3;
using UnityEngine;
using UnityEngine.TestTools;

namespace WhiteArrow.Incremental.Tests
{
    public class CurveMovementTests
    {
        private bool _isMovementDisposed;
        private CurveMovement _curveMovement;
        private CurveMovementDataAdapter _dataAdapter;
        private ReactivePropertyUpdater<Vector3> _positionUpdater;
        private Vector3 _selfPositionValue;



        [SetUp]
        public void SetUp()
        {
            _isMovementDisposed = false;

            var trajectory = new AnimationCurve(
                new Keyframe(0, 0),
                new Keyframe(1, 1)
            );

            var data = new CurveMovementData(trajectory);
            _dataAdapter = new(data);

            _selfPositionValue = Vector3.zero;
            _positionUpdater = new ReactivePropertyUpdater<Vector3>(() => _selfPositionValue, UnityFrameProvider.Update, true);
            _curveMovement = new CurveMovement(_dataAdapter, _positionUpdater);
        }

        [TearDown]
        public void TearDown()
        {
            if (_isMovementDisposed)
                _curveMovement.Dispose();
        }



        #region Constructor
        [Test]
        public void Constructor_ValidData_InitializesCorrectly()
        {
            Assert.NotNull(_curveMovement);
            Assert.AreEqual(Vector3.zero, _curveMovement.StartPosition.CurrentValue);
            Assert.AreEqual(1, _curveMovement.ElapsedTime.CurrentValue);
            Assert.AreEqual(1, _curveMovement.Progress.CurrentValue);
        }
        #endregion



        #region UpdatePosition
        [UnityTest]
        public IEnumerator SetTarget_ElapsedTimeChanges_Updates()
        {
            _curveMovement.SetTarget(Vector3.one);

            var initialElapsedTime = _curveMovement.ElapsedTime.CurrentValue;

            yield return new WaitForSeconds(0.5f);

            var updatedElapsedTime = _curveMovement.ElapsedTime.CurrentValue;

            Assert.Greater(updatedElapsedTime, initialElapsedTime);
        }
        #endregion



        #region Pause
        [Test]
        public void Pause_IsPausedSetToTrue_StopsUpdating()
        {
            _curveMovement.IsPaused.Value = true;
            Assert.IsTrue(_curveMovement.IsPaused.CurrentValue);
        }
        #endregion
    }
}
