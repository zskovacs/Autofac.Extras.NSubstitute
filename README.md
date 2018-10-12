# Autofac.Extras.NSubstitute

[![NuGet downloads](https://img.shields.io/nuget/dt/Autofac.Extras.NSubstitute.svg)](https://www.nuget.org/packages/Autofac.Extras.NSubstitute) 
[![NuGet version](https://img.shields.io/nuget/vpre/Autofac.Extras.NSubstitute.svg)](https://www.nuget.org/packages/Autofac.Extras.NSubstitute)

NSubstitute auto mocking integration for Autofac [Autofac IoC](https://github.com/autofac/Autofac).

## Getting Started

Given you have a system under test and a dependency:

```c#
public class SystemUnderTest
{
  public SystemUnderTest(IDependency dependency)
  {
  }
}

public interface IDependency
{
}
```

Use the `Autofac.Extras.NSubstitute` package's `AutoSubstitute` class to instantiate the system under test.

```c#
[Test]
public void Test()
{
  using (var as = new AutoSubstitute())
  {
    // The AutoSubstitute class will inject a mock IDependency
    // into the SystemUnderTest constructor
    var sut = as.Resolve<SystemUnderTest>();
  }
}
```

### Configuring Mocks

```c#
[Test]
public void Test()
{
  using (var as = new AutoSubstitute())
  {
    // Arrange
    as.Resolve<IDependency>().GetValue().Returns("expected value");
    var sut = as.Resolve<SystemUnderTest>();

    // Act
    var actual = sut.DoWork();

    // Assert
    as.Resolve<IDependency>().Received().GetValue();
    Assert.AreEqual("expected value", actual);
  }
}

public class SystemUnderTest
{
  private readonly IDependency dependency;

  public SystemUnderTest(IDependency strings)
  {
    this.dependency = strings;
  }

  public string DoWork()
  {
    return this.dependency.GetValue();
  }
}

public interface IDependency
{
  string GetValue();
}
```

### Configuring Specific Dependencies

You can configure the AutoSubstitute to provide a specific instance for a given service type:

```c#
[Test]
public void Test()
{
  using (var as = AutoSubstitute())
  {
    var dependency = new Dependency();
    as.Provide<IDependency>(dependency);

    // ...and the rest of the test.
  }
}
```

### Partial subs

```c#
[Test]
public void Test()
{
  using (var as = AutoSubstitute())
  {
    var dependency = Substitute.ForPartsOf<Dependency>();
    dependency.When(x => x.GetValue()).DoNotCallBase();
    dependency.GetValue().Returns("1,2,3");
    as.Provide<IDependency>(dependency);

    // ...and the rest of the test.
  }
}
```

Or you can use the built in one

```c#
[Test]
public void Test()
{
  using (var as = AutoSubstitute())
  {
    var dependency = as.ProvidePartsOf<IDependency, Dependency>();
    dependency.When(x => x.GetValue()).DoNotCallBase();
    dependency.GetValue().Returns("1,2,3");

    // ...and the rest of the test.
  }
}
```
