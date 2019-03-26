![Logo](Logo.png)
# Grafana.ReserveProxy
> Author [**Top Nguyen**](http://topnguyen.net)

## Overview
Grafana.ReserveProxy is a reserve proxy for Grafana by .NET Core.

Easy to you can access the Grafana in the Server hosted via localhost.

## Usage
- Just download the [build](build) folder.
- Adjust Grafana Endpoint (`ServiceRootUrl`) in the `appsettings.json`.
- Deploy the `build` to your Server.
- Run the proxy by `dotnet Grafana.ReserveProxy.dll` or you can host by IIS.
- After you access the Proxy, if face the `Unauthenticated` or `401` issue, 
please access `<domain>/login` to signin into the Grafana Dashboard and everything will be fine.

## Note
Feel free to adjust the code in the file [ReverseProxyMiddleware](src/Grafana.ReserveProxy/ReverseProxyMiddleware.cs)
 to fit with your business.

## License
Grafana.ReserveProxy is licensed under the [MIT License](LICENSE).