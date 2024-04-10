# Shane32.Analyzers

[![NuGet](https://img.shields.io/nuget/v/Shane32.Analyzers.svg)](https://www.nuget.org/packages/Shane32.Analyzers) [![Coverage Status](https://coveralls.io/repos/github/Shane32/Analyzers/badge.svg?branch=master)](https://coveralls.io/github/Shane32/Analyzers?branch=master)

## Analyzers

This package contains a collection of Roslyn analyzers and code fixes that can be used to enforce coding standards and best practices in C# code.

1. `IQueryable` cast analyzer

Prevents casting `IQueryable<T>` to `IEnumerable<T>`.  This is a common mistake that can cause performance issues by preventing the query from being executed asynchronously.

Use an explicit cast or `AsEnumerable()` instead.

## Credits

Glory to Jehovah, Lord of Lords and King of Kings, creator of Heaven and Earth, who through his Son Jesus Christ,
has reedemed me to become a child of God. -Shane32
