using System;
using NSubstitute;
using Xunit;

namespace Autofac.Extras.NSubstitute.Test
{
    public class AutoSubstituteFixture
    {
        public interface IBar
        {
            bool Gone { get; }

            void Go();

            IBar Spawn();

            string Fuzz();
        }

        public interface IBaz
        {
            void Go();
        }

        [Fact]
        public void ByDefaultAbstractTypesAreResolvedToTheSameSharedInstance()
        {
            using (var fake = new AutoSubstitute())
            {
                var bar1 = fake.Resolve<IBar>();
                var bar2 = fake.Resolve<IBar>();

                Assert.Same(bar1, bar2);
            }
        }

        [Fact]
        public void ByDefaultConcreteTypesAreResolvedToTheSameSharedInstance()
        {
            using (var fake = new AutoSubstitute())
            {
                var baz1 = fake.Resolve<Baz>();
                var baz2 = fake.Resolve<Baz>();

                Assert.Same(baz1, baz2);
            }
        }

        [Fact]
        public void ByDefaultFakesAreNotStrict()
        {
            using (var fake = new AutoSubstitute())
            {
                var foo = fake.Resolve<Foo>();

                // Should not throw.
                foo.Go();
            }
        }

        [Fact]
        public void ByDefaultFakesDoNotCallBaseMethods()
        {
            using (var fake = new AutoSubstitute())
            {
                var bar = fake.Resolve<Bar>();
                bar.Go();
                Assert.False(bar.Gone);
            }
        }

        [Fact]
        public void ByDefaultFakesRespondToCalls()
        {
            using (var fake = new AutoSubstitute())
            {
                var bar = fake.Resolve<IBar>();
                var result = bar.Spawn();
                Assert.NotNull(result);
            }
        }

        [Fact]
        public void ProvidesImplementations()
        {
            using (var fake = new AutoSubstitute())
            {
                var baz = fake.Provide<IBaz, Baz>();

                Assert.NotNull(baz);
                Assert.True(baz is Baz);
            }
        }

        [Fact]
        public void ProvidesInstances()
        {
            using (var fake = new AutoSubstitute())
            {
                var bar = Substitute.For<IBar>();
                fake.Provide(bar);

                var foo = fake.Resolve<Foo>();
                foo.Go();

                bar.Received().Go();
            }
        }

        [Fact]
        public void ProvidesPartsOfInstances()
        {
            using (var fake = new AutoSubstitute())
            {
                var bar = Substitute.ForPartsOf<Bar>();
                bar.When(x => x.Fuzz()).DoNotCallBase();
                bar.Fuzz().Returns("Fuzz");

                fake.Provide<IBar>(bar);

                var foo = fake.Resolve<Foo>();
                var result = foo.Fuzz();

                Assert.Equal("Fuzz", result);
            }
        }

        [Fact]
        public void ProvidesPartsOfBuiltin()
        {
            using (var fake = new AutoSubstitute())
            {
                var bar = fake.ProvidePartsOf<IBar, Bar>();
                bar.When(x => x.Fuzz()).DoNotCallBase();
                bar.Fuzz().Returns("Fuzz");

                var foo = fake.Resolve<Foo>();
                var result = foo.Fuzz();

                Assert.Equal("Fuzz", result);
            }
        }

        public abstract class Bar : IBar
        {
            private bool _gone;

            public virtual string Fuzz()
            {
                return "Buzz";
            }

            public bool Gone
            {
                get { return this._gone; }
            }

            public virtual void Go()
            {
                this._gone = true;
            }

            public IBar Spawn()
            {
                throw new NotImplementedException();
            }

            public abstract void GoAbstractly();
        }

        public class Baz : IBaz
        {
            private bool _gone;

            public bool Gone
            {
                get { return this._gone; }
            }

            public virtual void Go()
            {
                this._gone = true;
            }
        }

        public class Foo
        {
            private readonly IBar _bar;

            private readonly IBaz _baz;

            public Foo(IBar bar, IBaz baz)
            {
                this._bar = bar;
                this._baz = baz;
            }

            public virtual void Go()
            {
                this._bar.Go();
                this._baz.Go();
            }

            public string Fuzz()
            {
                return this._bar.Fuzz();
            }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class ForTestAttribute : Attribute
        {
        }
    }
}
