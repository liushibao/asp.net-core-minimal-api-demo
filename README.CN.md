[English read me](./README.md)

# Asp.Net core minimal api demo

这是我的Vue3前端Demo项目的配套后端。展示了微信登录和使用jwt-token身份验证。持久数据存储采用SqlServer，用SqlSugar做ORM。Http返回数据和其他临时数据使用Redis缓存。Asp.Net core minimal api很适合在微服务场景使用，风格与nodejs express或者nancy framework类似。

## 服务器配置

详见/Models/EnvironmentConfig.cs文件。开发环境使用默认配置即可。

## 项目安装

```sh
dotnet restore
```

### 开发环境运行

```sh
dotnet run dev
```

### 生产环境部署

这是一个可独立运行的控制台程序，可使用system service/daemon部署，将配置参数写入系统环境即可。
