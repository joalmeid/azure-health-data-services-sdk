@description('Specify the name of the Azure Redis Cache to create.')
param redisCacheName string = 'redis-hdssdk'

@description('Location of all resources')
param location string 
param appTags object = {}

resource redisCache 'Microsoft.Cache/Redis@2021-06-01' = {
  name: redisCacheName
  location: location
  tags: appTags
  properties: {
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    sku: {
      capacity: 0
      family: 'C'
      name: 'Basic'
    }
    redisConfiguration:{
      'maxmemory-reserved': '2'
      'maxfragmentationmemory-reserved' : '12'
      'maxmemory-delta' : '2'
    }
  }
}


var redisConnectionString = listKeys(redisCache.id, redisCache.apiVersion).primaryConnectionString
output redisCacheConnString string = redisConnectionString
