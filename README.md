NaGet
====================================

NaGet is a library and server to host NuGet packages. It is based on [Loic Sharma's BaGet](https://github.com/loic-sharma/BaGet), Commit https://github.com/loic-sharma/BaGet/commit/db839bf0387a34bb3e68357fa24c6e5b0d41ee72 on master. The projetc is double licensed under the license from [Loic Sharma's BaGet](https://github.com/loic-sharma/BaGet) and the MIT license.

[![Build status](https://ci.appveyor.com/api/projects/status/10q6d88s5if8xdef?svg=true)](https://ci.appveyor.com/project/SeppPenner/naget)
[![GitHub issues](https://img.shields.io/github/issues/SeppPenner/NaGet.svg)](https://github.com/SeppPenner/NaGet/issues)
[![GitHub forks](https://img.shields.io/github/forks/SeppPenner/NaGet.svg)](https://github.com/SeppPenner/NaGet/network)
[![GitHub stars](https://img.shields.io/github/stars/SeppPenner/NaGet.svg)](https://github.com/SeppPenner/NaGet/stargazers)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://raw.githubusercontent.com/SeppPenner/NaGet/master/License.txt)
[![Nuget](https://img.shields.io/badge/NaGet-Nuget-brightgreen.svg)](https://www.nuget.org/packages/NaGet/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NaGet.svg)](https://www.nuget.org/packages/NaGet/)
[![Known Vulnerabilities](https://snyk.io/test/github/SeppPenner/NaGet/badge.svg)](https://snyk.io/test/github/SeppPenner/NaGet)
[![Gitter](https://badges.gitter.im/Serilog-Sinks-AmazonS3/community.svg)](https://gitter.im/Serilog-Sinks-AmazonS3/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## Available for
* NetStandard 2.0
* NetStandard 2.1
* NetCore 3.1
* Net 5.0
* Net 6.0

## Net Core and Net Framework latest and LTS versions
* https://dotnet.microsoft.com/download/dotnet-framework
* https://dotnet.microsoft.com/download/dotnet-core
* https://dotnet.microsoft.com/download/dotnet

## Basic usage
Check out the how to use file [here](https://github.com/SeppPenner/NaGet/blob/master/HowToUse.md).

## Packages
Several packages are included within this project:

These folders contain the core components of NaGet:

* `NaGet` - The app's entry point that glues everything together.
* `NaGet.Core` - BaGet's core logic and services.
* `NaGet.Web` - The [NuGet server APIs](https://docs.microsoft.com/en-us/nuget/api/overview) and web UI.
* `NaGet.Protocol` - Libraries to interact with [NuGet servers' APIs](https://docs.microsoft.com/en-us/nuget/api/overview).

These folders contain database-specific components of NaGet:

* `NaGet.Database.MySql` - BaGet's MySQL database provider.
* `NaGet.Database.PostgreSql` - BaGet's PostgreSql database provider.
* `NaGet.Database.Sqlite` - BaGet's SQLite database provider.
* `NaGet.Database.SqlServer` - BaGet's Microsoft SQL Server database provider.

These folders contain cloud-specific components of NaGet:

* `NaGet.Aliyun` - BaGet's Alibaba Cloud(Aliyun) provider.
* `NaGet.Aws` - BaGet's Amazon Web Services provider.
* `NaGet.Azure` - BaGet's Azure provider.
* `NaGet.GoogleCloud` - BaGet's Google Cloud Platform provider.

## Features
Todo: Add all features from BaGet, add new features and highlight them

Change history
--------------

See the [Changelog](https://github.com/SeppPenner/NaGet/blob/master/Changelog.md).