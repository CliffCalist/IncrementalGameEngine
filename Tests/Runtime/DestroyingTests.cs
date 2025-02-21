using System;
using System.Collections;
using Moq;
using NUnit.Framework;
using R3;
using UnityEngine.TestTools;

namespace WhiteArrow.Incremental.Tests
{
    public class DestroyingTests
    {
        private Mock<IDestroyable> _mockDestroyable;



        [SetUp]
        public void SetUp()
        {
            _mockDestroyable = new Mock<IDestroyable>();
        }



        [Test]
        public void Destr_ShouldCallDestroy()
        {
            // Act
            Destroying.Destr(_mockDestroyable.Object);

            // Assert
            _mockDestroyable.Verify(d => d.Destroy(), Times.Once);
        }

        [Test]
        public void Destr_ShouldThrow_WhenDestroyableIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => Destroying.Destr(null));
        }

        [Test]
        public void DelayDestr_ShouldImmediatelyDestroy_WhenTimeIsZero()
        {
            // Act
            var observable = Destroying.DelayDestr(_mockDestroyable.Object, 0);

            // Assert
            _mockDestroyable.Verify(d => d.Destroy(), Times.Once);
            Assert.IsNotNull(observable);
        }

        [Test]
        public void DelayDestr_ShouldThrow_WhenDestroyableIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => Destroying.DelayDestr(null, 1f));
        }

        [Test]
        public void DelayDestr_ShouldThrow_WhenTimeIsNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Destroying.DelayDestr(_mockDestroyable.Object, -1f));
        }

        [Test]
        public void SubscribedDelayDestr_ShouldReturnSubscription()
        {
            // Act
            using var subscription = Destroying.SubscribedDelayDestr(_mockDestroyable.Object, 1f);

            // Assert
            Assert.IsNotNull(subscription);
        }

        [UnityTest]
        public IEnumerator DelayDestr_ShouldCallDestroy_AfterTimeElapsed()
        {
            // Arrange
            float delayTime = 1f;
            var observable = Destroying.DelayDestr(_mockDestroyable.Object, delayTime);
            observable.Subscribe();

            // Act
            yield return new UnityEngine.WaitForSeconds(delayTime + 0.1f); // Adding some buffer time

            // Assert
            _mockDestroyable.Verify(d => d.Destroy(), Times.Once);
        }
    }
}
