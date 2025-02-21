using NUnit.Framework;
using R3;

namespace WhiteArrow.Incremental.Tests
{
    public class DataAdapterTests
    {
        private class TestDataAdapter : DataAdapter
        {
            public void AddDynamicChangingObservablePublic<T>(Observable<T> observable, bool emitOnAdd = true)
            {
                AddDynemicChangingsObservable(observable, emitOnAdd);
            }

            public void RemoveDynamicChangingObservablePublic<T>(Observable<T> observable)
            {
                RemoveDynemicChangingsObservable(observable);
            }
        }



        private TestDataAdapter _adapter;
        private Subject<Unit> _testObservable;
        private int _onChangedCount;



        [SetUp]
        public void SetUp()
        {
            _adapter = new TestDataAdapter();
            _testObservable = new Subject<Unit>();
            _onChangedCount = 0;
            _adapter.OnChanged.Subscribe(_ => _onChangedCount++);
        }



        [Test]
        public void AddObservable_WhenEmits_ShouldTriggerOnChanged()
        {
            _adapter.AddDynamicChangingObservablePublic(_testObservable);
            _testObservable.OnNext(Unit.Default);

            Assert.AreEqual(2, _onChangedCount); // One for add, one for event
        }

        [Test]
        public void AddObservable_WithEmitOnAddFalse_ShouldNotTriggerOnChanged()
        {
            _adapter.AddDynamicChangingObservablePublic(_testObservable, false);
            Assert.AreEqual(0, _onChangedCount);
        }

        [Test]
        public void RemoveObservable_AfterAdd_ShouldNotTriggerOnChangedOnNextEmit()
        {
            _adapter.AddDynamicChangingObservablePublic(_testObservable);
            _adapter.RemoveDynamicChangingObservablePublic(_testObservable);
            _testObservable.OnNext(Unit.Default);

            Assert.AreEqual(2, _onChangedCount); // One per add and remove
        }

        [Test]
        public void AddMultipleObservables_WhenBothEmit_ShouldTriggerOnChangedForEach()
        {
            var testObservable2 = new Subject<Unit>();
            _adapter.AddDynamicChangingObservablePublic(_testObservable);
            _adapter.AddDynamicChangingObservablePublic(testObservable2);
            _testObservable.OnNext(Unit.Default);
            testObservable2.OnNext(Unit.Default);

            Assert.AreEqual(4, _onChangedCount); // One per add, plus two emissions
        }
    }
}