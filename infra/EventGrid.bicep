@description('The name of the Event Grid custom topic.')
param eventGridTopicName string = 'topic-hdssdk'

@description('The name of the Event Grid custom topic\'s subscription.')
param eventGridSubscriptionName string = 'sub-hdssdk'

@description('The name of the Event Grid custom topic\'s subscription.')
param eventGridSubscriptionName_ref string = 'sub-refhdssdk'

param location string
param storageAccountName string = 'sthdssdk'
param blobContainerName string = 'blob-hdssdk'
param fallblobContainerName string = 'blob-fallbackhdssdk'
param appTags object = {}

@description('Azure storage account')
resource StorageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  tags: appTags
}

@description('Azure Blob Storage')
resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: '${storageAccountName}/default/${blobContainerName}'
  tags: appTags
  dependsOn: [
    StorageAccount
  ]
}

@description('Azure storage account queue service')
resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2021-09-01' = {
  name: 'default'
  parent: StorageAccount
}


@description('Azure storage account queues')
resource queue 'Microsoft.Storage/storageAccounts/queueServices/queues@2021-09-01' = {
  name: 'eventgridqueue'
  parent: queueService
}


@description('Azure storage account queues')
resource queue_ref 'Microsoft.Storage/storageAccounts/queueServices/queues@2021-09-01' = {
  name: 'eventgrid-ref'
  parent: queueService
}


@description('Azure Blob Storage for fallback connection')
resource blobContainer_fallback 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: '${storageAccountName}/default/${fallblobContainerName}'
  dependsOn: [
    StorageAccount
  ]
}

@description('Azure Event Grid Topic')
resource eventGridTopic 'Microsoft.EventGrid/topics@2020-06-01' = {
  name: eventGridTopicName
  location: location
  tags:appTags
}

@description('Azure Event Grid Topic subscription')
resource eventGridSubscription 'Microsoft.EventGrid/topics/eventSubscriptions@2022-06-15' = {
  parent: eventGridTopic
  name: eventGridSubscriptionName
  properties: {
    destination: {
      properties: {
        resourceId: StorageAccount.id
        queueName: 'eventgridqueue'
      }
      endpointType: 'StorageQueue'
    }
    filter: {
      includedEventTypes: [
        'Proxy'
      ]
      enableAdvancedFilteringOnArrays: true
    }
    eventDeliverySchema: 'EventGridSchema'
    retryPolicy: {
      maxDeliveryAttempts: 30
      eventTimeToLiveInMinutes: 1440
    }
  }
}

@description('Azure Event Grid Topic subscription refernce')
resource eventGridSubscription_ref 'Microsoft.EventGrid/topics/eventSubscriptions@2022-06-15' = {
  parent: eventGridTopic
  name: eventGridSubscriptionName_ref
  properties: {
    destination: {
      properties: {
        resourceId: StorageAccount.id
        queueName: 'eventgrid-ref'
      }
      endpointType: 'StorageQueue'
    }
    filter: {
      includedEventTypes: [
        'Reference'
      ]
      enableAdvancedFilteringOnArrays: true
    }
    eventDeliverySchema: 'EventGridSchema'
    retryPolicy: {
      maxDeliveryAttempts: 30
      eventTimeToLiveInMinutes: 1440
    }
  }
}

output eventGridTopicEndpoint string = eventGridTopic.properties.endpoint
var accesskey = listKeys(eventGridTopic.id, eventGridTopic.apiVersion).key1
output eventGridTopicAccessKey string = accesskey
output eventGridEventType string = eventGridSubscription.properties.filter.includedEventTypes[0]

