#### 0.8.0 - 2021-11-12
* FEATURE: [Adds Fable support](https://github.com/TheAngryByrd/FsLibLog/pull/21). Operators have a breaking change from (!!) to (!!!) to avoid overlap with Fable's unboxing operator. Credits [@PaigeM89](https://github.com/PaigeM89)
#### 0.7.0 - 2021-10-27
* FEATURE: [Adds Microsoft.Extensions.Logging as a provider](https://github.com/TheAngryByrd/FsLibLog/pull/25)

#### 0.6.0 - 2021-09-17
* FEATURE: [Adds Logging operators](https://github.com/TheAngryByrd/FsLibLog/pull/19)
* FEATURE: [Adds interpolated strings](https://github.com/TheAngryByrd/FsLibLog/pull/23)
* INFRASTRUCTURE : [Moved to GitHub Workflows](https://github.com/TheAngryByrd/FsLibLog/pull/20)
* INFRASTRUCTURE: [Adds basic tests using a TestProvider](https://github.com/TheAngryByrd/FsLibLog/pull/22)

#### 0.5.2 - 2020-07-20
* BUGFIX: [Warning Level 5 compliance](https://github.com/TheAngryByrd/FsLibLog/pull/14)

#### 0.5.1 - 2019-11-20
* BUGFIX: [Fix message parameter replacement for npgsql adapter](https://github.com/TheAngryByrd/FsLibLog/pull/11)

#### 0.5.0 -2019-11-08
* BREAKING: [Remove ConsoleProvider from main code](https://github.com/TheAngryByrd/FsLibLog/pull/10)

#### 0.4.1 -2019-11-08
* BUGFIX: [fix edge case with {}s in logs](https://github.com/TheAngryByrd/FsLibLog/pull/9)

#### 0.4.0 -2019-10-31
* FEATURE: [Implement simple propertyname logging for the console logger](https://github.com/TheAngryByrd/FsLibLog/pull/8)

#### 0.3.0 - 2019-10-03
* FEATURE: [Adds to LogProvider two new methods `getLoggerByQuotation` and `getLoggerByFunc`. Also deprecates `getCurrentLogger`.](https://github.com/TheAngryByrd/FsLibLog/pull/7)

#### 0.2.1 - 2019-07-23
* BUGFIX: Fixed openNestedContext typo
* MINOR: Replaced DisposableList and List.rev with DisposeableStack

#### 0.2.0 - 2019-06-24
* FEATURE: Add support for `OpenMappedContext` and `OpenNestedContext` for Serilog
* FEATURE: Added `addContext` and `addContextDestructured`.  These are the same as calling `OpenMappedContext` immediately before logging.
* FEATURE: Added Marten and Npgsql Adapters

#### 0.1.0 - 2019-02-11
* Initial release
