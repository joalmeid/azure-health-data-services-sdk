param eventHubNamespaceName string = 'eh-hdssdk'
param eventHubName string = 'evh-hdssdk'
param location string
param appTags object = {}


resource eventHubNamespace 'Microsoft.EventHub/namespaces@2021-01-01-preview' = {
  name: eventHubNamespaceName
  location: location
  tags:appTags
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 1
  }
  properties: {
    zoneRedundant: true
  }
}

resource eventHubNamespace_RootManageSharedAccessKey 'Microsoft.EventHub/namespaces/AuthorizationRules@2022-01-01-preview' = {
  parent: eventHubNamespace
  name: 'RootManageSharedAccessKey'
  properties: {
    rights: [
      'Listen'
      'Manage'
      'Send'
    ]
  }
}


resource eventHubNamespace_eventHub 'Microsoft.EventHub/namespaces/eventhubs@2021-01-01-preview' = {
  parent: eventHubNamespace
  name: eventHubName
  properties: {
    messageRetentionInDays: 1
    partitionCount: 2
  }
  dependsOn:[
    eventHubNamespace
  ]
}

resource eventHubNamespaceName_eventHubName_ListenSend 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2021-01-01-preview' = {
  parent: eventHubNamespace_eventHub
  name: 'ListenSend'
  properties: {
    rights: [
      'Listen'
      'Send'
    ]
  }
  dependsOn: [
    eventHubNamespace_eventHub
    eventHubNamespace
  ]
}

var eventHubConnectionString = listKeys(eventHubNamespace_RootManageSharedAccessKey.id, eventHubNamespace_RootManageSharedAccessKey.apiVersion).primaryConnectionString

output eventHubConnString string = eventHubConnectionString
output eventHubName string = eventHubNamespace_eventHub.name
output eventHubSku string = eventHubNamespace.sku.name
