# Azure Health Data Services SDK

The Azure Health Data Services SDK helps you extend the functionality of Azure Health Data Services by providing a consistent toolset to build custom operations to modify the core service behavior.
With the growth of health data workloads on Azure, we’ve found that developers need custom behavior on top of our services. This SDK abstracts common patterns so you can focus on delivering your use cases.

## NuGet Packages
*NuGet packages coming soon!*

| Package Name | Description |
| --- | --- |
| [AzureHealth.DataServices.Core](https://www.nuget.org/packages/AzureHealth.DataServices.Core/)<br/>[![NuGet](https://img.shields.io/nuget/v/AzureHealth.DataServices.Core.svg?label=NuGet)](https://www.nuget.org/packages/AzureHealth.DataServices.Core)| .NET 6 SDK for creating custom operations when using Azure Health Data Services. |
| [AzureHealth.DataServices.Channels.Extensions](https://www.nuget.org/packages/AzureHealth.DataServices.Channels.Extensions/)<br/>[![NuGet](https://img.shields.io/nuget/v/AzureHealth.DataServices.Channels.Extensions.svg?label=NuGet)](https://www.nuget.org/packages/AzureHealth.DataServices.Channels.Extensions) | .NET 6 SDK for extending channels using Azure Health Data Services. |
| [AzureHealth.DataServices.Caching](https://www.nuget.org/packages/AzureHealth.DataServices.Caching/)<br/>[![NuGet](https://img.shields.io/nuget/v/AzureHealth.DataServices.Caching.svg?label=NuGet)](https://www.nuget.org/packages/AzureHealth.DataServices.Caching) | .NET 6 SDK for adding caching using Azure Health Data Services. |
| [AzureHealth.DataServices.Storage](https://www.nuget.org/packages/AzureHealth.DataServices.Storage/)<br/>[![NuGet](https://img.shields.io/nuget/v/AzureHealth.DataServices.Storage.svg?label=NuGet)](https://www.nuget.org/packages/AzureHealth.DataServices.Storage)| .NET 6 SDK to simplify Azure storage operations when using Azure Health Data Services. |

## Getting Started

The fastest way to test out the SDK and see it in action is [through our Quickstart sample](./samples/Quickstart/README.md). This sample will walk you through some common patterns that you'll need to create custom operations for Azure Health Data Services.

<!---
(Uncomment this when we have dev docs) Read [this guide](./docs/dev_setup.md) for help setting up your local and cloud environment for developing custom behaviors for Azure Health Data Services.
--->
Also check out our full list of [samples on how to use the SDK here](./samples/README.md) for even more inspiration on how to create your own custom operations.

## Common Fast Healthcare Interoperability Resources (FHIR®) Use Cases

Some FHIR Service use cases that this SDK can help you implement are:

- FHIR operations not [supported by the FHIR Service](https://docs.microsoft.com/azure/healthcare-apis/fhir/fhir-features-supported#extended-operations) yet.
  - Trial implementation guides.
  - Organization-specific operations.
  - Less widely adopted operations.
- Implementation  guide development.
- Transforming request and/or response payloads.
- Custom authorization logic, like consent.

### What about FHIR Proxy? 

FHIR Proxy was created in response to customer requests for customizing the Azure API for FHIR. With the release of Azure Health Data Services, we’ve come up with a new approach to customization.

- This SDK lets you go beyond the proxy pattern and gives you tools for other customization patterns.
- This SDK is designed to be used in smaller operation-specific modules. If you are customizing a certain behavior, you don’t need to proxy the rest of your API calls.
- This SDK is compute-agnostic and can be deployed on any .NET 6.0 server like Azure Functions, Azure App Service, Azure Kubernetes Service, etc.
- This SDK is released and versioned via NuGet packages.
- We have incorporated some coding best practices, like object-oriented pipelines and extended testing.

*If there is functionality in the FHIR Proxy that is not covered by the Health Data Services SDK, please submit an issue and we will look into adding a sample!*

## Key Concepts

When we say “custom operations” we are talking about a purpose-built solution which acts as a proxy for a single or small set of HTTP endpoints. This SDK is here to simplify developing their solutions. It’s recommended to use Azure API Management or similar for routing certain requests to this custom operation, so that the client only sees one endpoint. Azure API Management can also present a unified authorization experience to your clients, which is why our samples don’t have authorization on the endpoints. 

When building custom operations, you’ll come across these concepts of the SDK.

- **Operation Context**: Common object passed between components of a pipeline containing the request and response.
- **Pipeline**: Container for the actions of custom operations with filters, channels, and bindings executed in the order shown below.
  - **Filter:** A unit of action that modifies the request and/or result via the Operation Context. Filters can be chained together in a single input/output section of a pipeline.
  - **Channel:** Used to output data in a pipeline to an external system actor (ESA). This is usually an Azure service (like Storage, Event Hub, and/or Service Bus).
  - **Binding:** The target service for a custom operation (usually a FHIR service). This can be null for custom operations that don't need to have a destination.

For more information, read [the concept guide here](./docs/concepts.md).

## Resources

### Links

- [FHIR Service Documentation](https://docs.microsoft.com/azure/healthcare-apis/fhir/overview)
- [FHIR Server OSS Repository](https://github.com/microsoft/fhir-server)
 
### Sample Production Architecture

This architecture is a sample of how you could deploy and integrate the custom operations you build with the Azure Health Data Services SDK to a production environment with Azure Health Data Services.

![Example architecture diagram](./docs/images/ExampleArchitectureDiagram.png)

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Disclaimers

The Azure Health Data Services SDK is an open-source project. It is not a managed service, and it is not part of Microsoft Azure Health Data Services. Please review the information and licensing terms on this GitHub website before using the Azure Health Data Services SDK.


The Azure Health Data Services SDK GitHub is intended only for use in transferring and formatting data.  It is not intended for use as a medical device or to perform any analysis or any medical function and the performance of the software for such purposes has not been established.  You bear sole responsibility for any use of this software, including incorporation into any product intended for a medical purpose.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.


FHIR® is the registered trademark of HL7 and is used with the permission of HL7. 
