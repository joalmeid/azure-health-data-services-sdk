param storageAccountName string = 'sthdssdk'
param blobContainerName string = 'blob-hdssdk'
param location string
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
  tags:appTags
  dependsOn: [
    StorageAccount
  ]
}

var blobStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${StorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(StorageAccount.id, StorageAccount.apiVersion).keys[0].value}'

output blobStorageConnectionString string = blobStorageConnectionString
output blobContainerName string = blobContainerName
output storageAccountId string = StorageAccount.id
