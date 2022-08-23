@description('Service Bus Name.')
param serviceBusNamespaceName string = 'sb-hdssdk'
@description('Service Bud Queue Name.')
param serviceBusQueueName string = 'sbq-hdssdk'
@description('Service Bus Topic Name.')
param serviceBusTopicName string = 'sbt-hdsdsdk'
@description('Service Bus Topic Subscription Name.')
param serviceBusTopicSubName string = 'sub-hdssdktopic'
@description('Location for all resources.')
param location string

param appTags object = {}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-01-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  tags: appTags
  sku: {
    name: 'Standard'
  }
  properties: {}
}

resource serviceBusNamasapce_RootManageSharedAccessKey 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2022-01-01-preview' = {
  parent: serviceBusNamespace
  name: 'RootManageSharedAccessKey'
  properties: {
    rights: [
      'Listen'
      'Manage'
      'Send'
    ]
  }
}

resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2022-01-01-preview' = {
  parent: serviceBusNamespace
  name: serviceBusQueueName
  properties: {
    lockDuration: 'PT5M'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
    defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
    deadLetteringOnMessageExpiration: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 10
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }
}

resource serviceBusTopic 'Microsoft.ServiceBus/namespaces/topics@2022-01-01-preview' = {
  parent: serviceBusNamespace
  name: serviceBusTopicName
  properties: {
    defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    status: 'Active'
    supportOrdering: true
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: false
    enableExpress: false
  }
}

resource serviceBusTopic_sub 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-01-01-preview' = {
  parent: serviceBusTopic
  name: serviceBusTopicSubName
  properties: {
    isClientAffine: false
    lockDuration: 'PT30S'
    requiresSession: false
    defaultMessageTimeToLive: 'P14D'
    deadLetteringOnMessageExpiration: false
    deadLetteringOnFilterEvaluationExceptions: false
    maxDeliveryCount: 100
    status: 'Active'
    enableBatchedOperations: true
    autoDeleteOnIdle: 'P10675198DT2H48M5.477S'
  }
  dependsOn: [
    serviceBusNamespace
  ]
}

resource serviceBusTopic_sub_Default 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2022-01-01-preview' = {
  parent: serviceBusTopic_sub
  name: '$Default'
  properties: {
    action: {
    }
    filterType: 'SqlFilter'
    sqlFilter: {
      sqlExpression: '1=1'
      compatibilityLevel: 20
    }
  }
  dependsOn: [
    serviceBusTopic
    serviceBusNamespace
  ]
}

var serviceBusConnectionString = listKeys(serviceBusNamasapce_RootManageSharedAccessKey.id, serviceBusNamasapce_RootManageSharedAccessKey.apiVersion).primaryConnectionString

output serviceBusConnectionString string = serviceBusConnectionString
output serviceBusTopicName string = serviceBusTopic.name
output serviceBusQueueName string = serviceBusQueue.name
output serviceBusSubscriptionName string = serviceBusTopic_sub.name
output serviceBusSku string = serviceBusNamespace.sku.name
