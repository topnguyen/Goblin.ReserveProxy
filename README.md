![Logo](Logo.png)
# Goblin.ReserveProxy
> Author [**Top Nguyen**](http://topnguyen.net)

## Overview
- Goblin.ReserveProxy is a reserve proxy for Goblin by .NET Core.

- Support Authentication by both Bearer Token and Basic Authentication on both Header and Query Parameters.


Easy to you can access the Goblin in the Server hosted via localhost.

## Usage
- Adjust Destination Endpoint (`DestinationEndpoint`) in `appsettings.json`.

- Adjust Proxy Authentication (`ProxyAuthentication`) in `appsettings.json`.
    + Bearer Token
        * In Header: Set key `Authorization` with the value is `Bearer <AccessToken>`.
        * In Query: Set key `token` with the value is `<AccessToken>`.
    + Basic Authentication
        * In Header: Set key `Authorization` with the value is `Basic <UserName>:<Password>`.
        * In Query: Set key `username` with the value is `<UserName>` and set key `password` with the value is `<Password>`.

- Deploy Proxy (Copy build package) to your Server.

- Run the proxy by `dotnet Goblin.ReserveProxy.dll` or you can host the Proxy via IIS.

## License
Goblin.ReserveProxy is licensed under the [MIT License](LICENSE).