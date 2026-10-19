targetScope = 'resourceGroup'

@description('The Azure Developer CLI environment name.')
param environmentName string

@description('The Azure region where resources are deployed.')
param location string = resourceGroup().location

@minLength(2)
@maxLength(60)
@description('The globally unique App Service web app name.')
param webAppName string

@description('Short prefix used in the App Service plan name.')
param resourcePrefix string = 'calc'

@description('The azd service name. This must match the service key in azure.yaml.')
param serviceName string = 'web'

@allowed([
  'DOTNETCORE|10.0'
])
@description('The Linux App Service runtime stack, aligned with the active solution.')
param runtimeStack string = 'DOTNETCORE|10.0'

@allowed([
  'B1'
  'S1'
  'P0v3'
])
@description('The App Service plan SKU. B1 is the default single-instance development tier.')
param appServicePlanSkuName string = 'B1'

@description('Additional tags applied to all deployed resources.')
param tags object = {}

var normalizedEnvironmentName = toLower(replace(environmentName, '_', '-'))
var resourceToken = uniqueString(subscription().id, resourceGroup().id, location, environmentName)
var commonTags = union(tags, {
  'azd-env-name': environmentName
  environment: environmentName
  workload: 'calculator'
})

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${resourcePrefix}-${normalizedEnvironmentName}-${resourceToken}-plan'
  location: location
  kind: 'linux'
  tags: commonTags
  sku: {
    name: appServicePlanSkuName
    capacity: 1
  }
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  tags: union(commonTags, { 'azd-service-name': serviceName })
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    clientAffinityEnabled: true
    publicNetworkAccess: 'Enabled'
    siteConfig: {
      alwaysOn: true
      webSocketsEnabled: true
      ftpsState: 'Disabled'
      healthCheckPath: '/health'
      http20Enabled: true
      linuxFxVersion: runtimeStack
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'false'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
    }
  }
}

resource scmPublishingPolicy 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2023-12-01' = {
  name: 'scm'
  parent: webApp
  properties: {
    allow: false
  }
}

resource ftpPublishingPolicy 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2023-12-01' = {
  name: 'ftp'
  parent: webApp
  properties: {
    allow: false
  }
}

resource webAppLogs 'Microsoft.Web/sites/config@2023-12-01' = {
  name: 'logs'
  parent: webApp
  properties: {
    applicationLogs: {
      fileSystem: {
        level: 'Information'
      }
    }
    httpLogs: {
      fileSystem: {
        enabled: true
        retentionInDays: 3
        retentionInMb: 35
      }
    }
    detailedErrorMessages: {
      enabled: false
    }
    failedRequestsTracing: {
      enabled: false
    }
  }
}

output AZURE_LOCATION string = location
output RESOURCE_GROUP_ID string = resourceGroup().id
output SERVICE_WEB_NAME string = webApp.name
output SERVICE_WEB_URI string = 'https://${webApp.properties.defaultHostName}'
