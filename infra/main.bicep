@description('Prefix for resources deployed by this solution (App Service, Function App, monitoring, etc)')
param prefixName string = 'hdssdk${uniqueString(resourceGroup().id)}'

@description('Name of Azure Health Data Services workspace to deploy or use.')
param workspaceName string = '${prefixName}workspace'

@description('Name of the FHIR service to deloy or use.')
param fhirServiceName string = 'testing'

@description('Name of the Log Analytics workspace to deploy or use. Leave blank to skip deployment')
param logAnalyticsName string = '${prefixName}-la'

@description('Location to deploy resources')
param location string = resourceGroup().location

@description('Location to deploy resources')
param additionalTags object = {}

@description('ID of principals to give FHIR Contributor on the FHIR service')
param fhirContributorPrincipals array = []


@description('Tenant ID where resources are deployed')
var tenantId  = subscription().tenantId

@description('Tags for all Azure resources in the solution')
var appTags = union(
  {
    AppID: 'azure-health-data-service-sdk'
  }, 
  additionalTags
)

param fhirdeployZone bool = false

@description('Deploy Azure Health Data Services and FHIR service')
module fhir './fhir.bicep'= if (fhirdeployZone) {
  name: 'fhirDeploy'
  params: {
    workspaceName: workspaceName
    fhirServiceName: fhirServiceName
    location: location
    tenantId: tenantId
    appTags: appTags
  }
}

@description('Name for app insights resource used to monitor the Function App')
var appInsightsName = '${prefixName}-appins'

@description('Deploy monitoring and logging')
module monitoring './monitoring.bicep'= if (fhirdeployZone) {
  name: 'monitoringDeploy'
  params: {
    logAnalyticsName: logAnalyticsName
    appInsightsName: appInsightsName
    location: location
    appTags: appTags
  }
}

@description('Name for the App Service used to host the Function App.')
var appServiceName = '${prefixName}-appserv'

@description('Name for the Function App to deploy the SDK custom operations to.')
var functionAppName = '${prefixName}-func'

@description('Name for the storage account needed for the Function App')
var funcStorName = '${prefixName}funcsa'

@description('Deploy Azure Function to run SDK custom operations')
module function './azureFunction.bicep'= if (fhirdeployZone) {
  name: 'functionDeploy'
  params: {
    appServiceName: appServiceName
    functionAppName: functionAppName
    storageAccountName: funcStorName
    location: location
    appInsightsInstrumentationKey: fhirdeployZone ? monitoring.outputs.appInsightsInstrumentationKey : ''   
    appTags: appTags
    workspaceName: workspaceName
    fhirServiceName : fhirServiceName
  }
}

@description('Setup identity connection between FHIR and the function app')
module functionFhirIdentity './fhirIdentity.bicep'= if (fhirdeployZone) {
  name: 'fhirIdentity-function'
  params: {
    fhirId: fhirdeployZone ? fhir.outputs.fhirId : ''
    principalId: fhirdeployZone ? function.outputs.functionAppPrincipalId : ''
  }
}

@description('Setup identity connection between FHIR and the function app')
module specifiedIdentity './fhirIdentity.bicep' = [for principalId in  fhirContributorPrincipals: if (fhirdeployZone){
  name: 'fhirIdentity-${principalId}'
  params: {
    fhirId: fhirdeployZone ? fhir.outputs.fhirId : ''
    principalId: principalId
    principalType: 'User'
  }
}]

param StoragedeployZone bool = false

@description('Setup Storage Account and Blob Container')
module storageAccount './Storage.bicep' = if (StoragedeployZone) {
  name:'StorageDeploy'
  params: {
    location: location
    appTags: appTags
  }
  
}

param eventGriddeployZone bool = false

@description('Setup Azure Event Grid, Storage Account and Storage Queue')
module eventGrid './EventGrid.bicep' = if (eventGriddeployZone) {
  name:'EventGridDeploy'
  params: {
    location: location
    appTags: appTags
  }
  
}

param eventHubdeployZone bool = false

@description('Setup Azure Event Hub')
module eventHub './EventHub.bicep' = if (eventHubdeployZone) {
  name: 'EventHubDeploy'
  params: {
    location: location
    appTags: appTags
  }
  
}

param serviceBusDeployZone bool = false

@description('Setup the Azure Service Bus')
module serviceBus './ServiceBus.bicep' = if (serviceBusDeployZone) {
  name: 'ServiceBusDeploy'
  params: {
    location: location
    appTags: appTags
  }
}

param redisCacheDeployZone bool = false

@description('Setup the Azure Cache for Redis')
module redisCache './RedisCache.bicep' = if (redisCacheDeployZone) {
  name: 'RedisCacheDeploy'
  params:{
    location: location
    appTags : appTags
  }
}

param keyVaultDeployZone bool = false
@description('Specifies the value of the secret that you want to create.')
@secure()
param secretValue string = ''
@description('Specifies the object ID of a user, service principal or security group in the Azure Active Directory tenant for the vault. The object ID must be unique for the list of access policies. Get it by using Get-AzADUser or Get-AzADServicePrincipal cmdlets.')
param objectId string = ''

@description('Setup the Azure Key Vault')
module keyVault './KeyVault.bicep' = if (keyVaultDeployZone) {
  name: 'KeyVaultDeploy'
  params: {
    location: location
    appTags: appTags
    objectId : objectId
    secretName : 'secret-key'
    secretValue: secretValue
  }
}

output FhirServiceId string = fhirdeployZone ? fhir.outputs.fhirId : ''
output FhirServiceUrl string = fhirdeployZone ? 'https://${workspaceName}-${fhirServiceName}.fhir.azurehealthcareapis.com': ''
output appInsightsInstrumentationKey string = fhirdeployZone ? monitoring.outputs.appInsightsInstrumentationKey : ''
output BlobStorageConnectionString string = StoragedeployZone ? storageAccount.outputs.blobStorageConnectionString : ''
output BlobContainerName string = StoragedeployZone ? storageAccount.outputs.blobContainerName : ''
output EventGridTopicEndpoint string = eventGriddeployZone ? eventGrid.outputs.eventGridTopicEndpoint : ''
output eventGridTopicAccessKey string = eventGriddeployZone ? eventGrid.outputs.eventGridTopicAccessKey : ''
output eventGridEventType string = eventGriddeployZone ? eventGrid.outputs.eventGridEventType : ''
output eventHubConnString string = eventHubdeployZone ? eventHub.outputs.eventHubConnString : ''
output eventHubName string = eventHubdeployZone ? eventHub.outputs.eventHubName : ''
output eventHubSku string = eventHubdeployZone ? eventHub.outputs.eventHubSku : ''
output serviceBusConnectionString string = serviceBusDeployZone ? serviceBus.outputs.serviceBusConnectionString : ''
output serviceBusTopicName string = serviceBusDeployZone ? serviceBus.outputs.serviceBusTopicName : ''
output serviceBusQueueName string = serviceBusDeployZone ? serviceBus.outputs.serviceBusQueueName : ''
output serviceBusSubscriptionName string = serviceBusDeployZone ? serviceBus.outputs.serviceBusSubscriptionName : ''
output serviceBusSku string = serviceBusDeployZone ? serviceBus.outputs.serviceBusSku : ''
output redisCacheConnString string = redisCacheDeployZone ? redisCache.outputs.redisCacheConnString : ''
output keyVaultURL string = keyVaultDeployZone ? keyVault.outputs.keyVaultURL : ''
