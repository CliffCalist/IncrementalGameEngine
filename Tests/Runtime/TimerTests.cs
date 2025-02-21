using System.Collections;
using NUnit.Framework;
using R3;
using UnityEngine;
using UnityEngine.TestTools;

namespace WhiteArrow.Incremental.Tests
{
    public class TimerTests
    {
        private const float TOLERANCE = 0.05f; // Acceptable error margin for floating point values


        #region Launch
        [UnityTest]
        public IEnumerator Launch_WhenDurationElapsed_ShouldComplete()
        {
            var timer = new Timer();
            bool completed = false;

            timer.Launch(1f).Subscribe(_ => completed = true);

            yield return new WaitForSeconds(1.1f);

            Assert.IsTrue(completed, "Timer should have completed, but it did not.");
            Assert.IsTrue(timer.IsStopped.CurrentValue, "Timer should be marked as stopped.");
        }


        [UnityTest]
        public IEnumerator Launch_WhenRestarted_ShouldCompleteAgain()
        {
            var timer = new Timer();
            bool firstCompletion = false;
            bool secondCompletion = false;

            timer.Launch(1f).Subscribe(_ => firstCompletion = true);
            yield return new WaitForSeconds(1.1f);

            Assert.IsTrue(firstCompletion, "First timer should have completed.");
            Assert.IsTrue(timer.IsStopped.CurrentValue, "Timer should be stopped after first completion.");

            timer.Launch(1.5f).Subscribe(_ => secondCompletion = true);
            yield return new WaitForSeconds(1.6f);

            Assert.IsTrue(secondCompletion, "Second timer should have completed.");
            Assert.IsTrue(timer.IsStopped.CurrentValue, "Timer should be stopped after second completion.");
        }
        #endregion



        #region Stop
        [UnityTest]
        public IEnumerator Stop_WhenCalledBeforeCompletion_ShouldPreventCompletion()
        {
            var timer = new Timer();
            bool completed = false;

            timer.Launch(2f).Subscribe(_ => completed = true);

            yield return new WaitForSeconds(1.0f);
            timer.Stop();

            yield return new WaitForSeconds(1.5f);

            Assert.IsFalse(completed, "Timer should have been stopped before completion.");
            Assert.IsTrue(timer.IsStopped.CurrentValue, "Timer should be marked as stopped.");
        }

        [UnityTest]
        public IEnumerator Stop_WhenStopConditionMet_ShouldPreventCompletion()
        {
            var stopCondition = new ReactiveProperty<bool>(false);
            var timer = new Timer(stopCondition);
            bool completed = false;

            timer.Launch(3f).Subscribe(_ => completed = true);

            yield return new WaitForSeconds(1.0f);
            stopCondition.Value = true;

            yield return new WaitForSeconds(2.5f);

            Assert.IsFalse(completed, "Timer should have been stopped by stopCondition, but it still completed.");
            Assert.IsTrue(timer.IsStopped.CurrentValue, "Timer should be marked as stopped.");
        }
        #endregion



        #region GetProgress
        [UnityTest]
        public IEnumerator GetProgress_WhenTimerRunning_ShouldUpdateCorrectly()
        {
            var timer = new Timer();
            float lastProgress = 0f;

            timer.Launch(2f);
            timer.GetProgress(progress => lastProgress = progress);

            yield return new WaitForSeconds(1.0f);

            Assert.Greater(lastProgress, 0.4f, "Progress should be greater than 0.4 after 1 second.");
            Assert.Less(lastProgress, 0.6f, "Progress should be less than 0.6 after 1 second.");
        }

        [UnityTest]
        public IEnumerator GetProgress_WhenTimerCompletes_ShouldBeOne()
        {
            var timer = new Timer();
            float progress = 0f;

            timer.Launch(1f);
            timer.GetProgress(p => progress = p);

            yield return new WaitForSeconds(1.1f);

            Assert.AreEqual(1f, progress, TOLERANCE, "Progress should be exactly 1 when the timer completes.");
        }

        [UnityTest]
        public IEnumerator GetProgress_WhenTimerStopped_ShouldNotBeOne()
        {
            var timer = new Timer();
            float progress = 0f;

            timer.Launch(2f);
            timer.GetProgress(p => progress = p);

            yield return new WaitForSeconds(1.0f);
            timer.Brake();

            yield return null; // Ensuring the observable updates

            Assert.Less(progress, 1f, "Progress should be less than 1 when the timer is stopped before completion.");
        }
        #endregion
    }
}