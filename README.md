# CodeOps

[![Release Status](https://img.shields.io/github/actions/workflow/status/HubiBoar/CodeOps/publish.yml)](https://github.com/HubiBoar/CodeOps/actions/workflows/publish.yml)

**CodeOps** Is a framework aiming to move as much DevOps work from yamls etc. into C# as part of your application code.<br>
It is Cloud agnostic and can be extended to support any Cloud Provider, for now it focues on Azure.


## [CodeOps.EnvironmentAsCode](src/Examples/Samples/EnvironmentExample.cs)

[![NuGet Version](https://img.shields.io/nuget/v/CodeOps.EnvironmentAsCode)](https://www.nuget.org/packages/CodeOps.EnvironmentAsCode/)

**CodeOps.EnvironmentAsCode** Is a library for programmatically expressing environments for your app <br>such as Development, Test, Staging, Production etc.   


## [CodeOps.InfrastructureAsCode](src/Examples/Samples/InfraAsCodeExample.cs)

[![NuGet Version](https://img.shields.io/nuget/v/CodeOps.InfrastructureAsCode)](https://www.nuget.org/packages/CodeOps.InfrastructureAsCode/)

**CodeOps.InfrastructureAsCode** Is a library for programmatically designing your Cloud Infrastructure.


## [CodeOps.ConfigurationAsCode](src/Examples/Samples/ConfigAsCodeExample.cs)

[![NuGet Version](https://img.shields.io/nuget/v/CodeOps.ConfigurationAsCode)](https://www.nuget.org/packages/CodeOps.ConfigurationAsCode/)

**CodeOps.ConfigurationAsCode** Is a library for managing configuration inside the application code, <br>with the ability to validate and deploy it to an external source of Configuration such as Azure AppConfiguration.


## [CodeOps.DeploymentAsCode](src/Examples/WebApp/Host/Environment/InfraAsCode.Deployment.cs)

[![NuGet Version](https://img.shields.io/nuget/v/CodeOps.DeploymentAsCode)](https://www.nuget.org/packages/CodeOps.DeploymentAsCode/)

**CodeOps.DeploymentAsCode** Is a library helping with easier application code deployment into <br>for example Azure Containers Registry as a Docker and then into Azure App Serivice.
