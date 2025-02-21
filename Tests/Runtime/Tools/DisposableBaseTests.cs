using System;
using Moq;
using NUnit.Framework;

namespace WhiteArrow.Incremental.Tests
{
    public class DisposableBaseTests
    {
        private class TestDisposable : DisposableBase
        {
            public bool DisposeCalled { get; private set; }
            public bool IsDisposedPublic => _isDisposed;

            public void AddDisposable(IDisposable disposable)
            {
                BuildPermanentDisposable(disposable);
            }

            protected override void DisposeProtected()
            {
                base.DisposeProtected();
                DisposeCalled = true;
            }
        }

        private class ChildDisposable : TestDisposable
        {
            protected override void DisposeProtected()
            {
                base.DisposeProtected();
            }
        }

        private class GrandchildDisposable : ChildDisposable
        {
            protected override void DisposeProtected()
            {
                base.DisposeProtected();
            }
        }



        #region Dispose
        [Test]
        public void Dispose_WhenCalled_SetsIsDisposed()
        {
            var testDisposable = new TestDisposable();

            testDisposable.Dispose();

            Assert.IsTrue(testDisposable.IsDisposedPublic);
        }

        [Test]
        public void Dispose_WhenCalled_InvokesDisposeProtected()
        {
            var testDisposable = new TestDisposable();

            testDisposable.Dispose();

            Assert.IsTrue(testDisposable.DisposeCalled);
        }

        [Test]
        public void Dispose_WhenHierarchyExists_CallsDisposeInCorrectOrder()
        {
            var mockBase = new Mock<IDisposable>();
            var mockChild = new Mock<IDisposable>();
            var mockGrandchild = new Mock<IDisposable>();

            var grandchildDisposable = new GrandchildDisposable();
            grandchildDisposable.AddDisposable(mockGrandchild.Object);
            grandchildDisposable.AddDisposable(mockChild.Object);
            grandchildDisposable.AddDisposable(mockBase.Object);

            grandchildDisposable.Dispose();

            var sequence = new Moq.MockSequence();
            mockGrandchild.InSequence(sequence).Setup(d => d.Dispose());
            mockChild.InSequence(sequence).Setup(d => d.Dispose());
            mockBase.InSequence(sequence).Setup(d => d.Dispose());
        }
        #endregion



        #region BuildPermanentDisposable
        [Test]
        public void BuildPermanentDisposable_WhenDisposed_DisposesAllAddedDisposables()
        {
            var testDisposable = new TestDisposable();
            var mockDisposable = new Mock<IDisposable>();

            testDisposable.AddDisposable(mockDisposable.Object);
            testDisposable.Dispose();

            mockDisposable.Verify(d => d.Dispose(), Times.Once);
        }

        [Test]
        public void BuildPermanentDisposable_WhenAlreadyDisposed_ThrowsObjectDisposedException()
        {
            var testDisposable = new TestDisposable();
            var mockDisposable = new Mock<IDisposable>();

            testDisposable.Dispose();

            Assert.Throws<ObjectDisposedException>(() => testDisposable.AddDisposable(mockDisposable.Object));
        }

        [Test]
        public void BuildPermanentDisposable_WhenGivenNull_ThrowsArgumentNullException()
        {
            var testDisposable = new TestDisposable();

            Assert.Throws<ArgumentNullException>(() => testDisposable.AddDisposable(null));
        }
        #endregion
    }
}
