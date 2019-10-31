#### 0.4.0 -2019-10-31
* FEATURE: Implement simple propertyname logging for the console logger https://github.com/TheAngryByrd/FsLibLog/pull/8

#### 0.3.0 - 2019-10-03
* FEATURE: Adds to LogProvider two new methods `getLoggerByQuotation` and `getLoggerByFunc`. Also deprecates `getCurrentLogger`. (https://github.com/TheAngryByrd/FsLibLog/pull/7)

#### 0.2.1 - 2019-07-23
* BUGFIX: Fixed openNestedContext typo
* MINOR: Replaced DisposableList and List.rev with DisposeableStack

#### 0.2.0 - 2019-06-24
* FEATURE: Add support for `OpenMappedContext` and `OpenNestedContext` for Serilog
* FEATURE: Added `addContext` and `addContextDestructured`.  These are the same as calling `OpenMappedContext` immediately before logging.
* FEATURE: Added Marten and Npgsql Adapters

#### 0.1.0 - 2019-02-11
* Initial release
