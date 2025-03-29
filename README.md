[中文说明](./README.CN.md)

# Asp.Net core minimal api demo

This is a demo project of the back-end of my vue3 front-end app demo. The functionality of this demo is identical to the mvc webapi style demo (another repository in my account). It demonstrates how to use Wechat for login and jwt-token authentication. The persistent data storage is SqlServer, with SqlSugar as the ORM lib. Http responses and other non-persistent data are stored using Redis.

## Configuration

See the /Models/EnvironmentConfig.cs file. Using the default config shoud be enough for development.

## Project Setup

```sh
dotnet restore
```

### Serve for Development

```sh
dotnet run dev
```

### Deploy for Production

This is a self contained console app, so just make it a system service/daemon and export the config to the system environment.
