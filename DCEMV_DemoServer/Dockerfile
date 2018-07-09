FROM microsoft/aspnetcore:1.1 AS base
WORKDIR /app
EXPOSE 80

COPY DCEMV_DemoServer/ScriptsReverseProxy/server4.crt /usr/local/share/ca-certificates/server4.crt
COPY DCEMV_DemoServer/ScriptsKubernetesDeploy/shared/payloola5.crt /usr/local/share/ca-certificates/payloola5.crt
RUN update-ca-certificates
COPY DCEMV_EMVSecurity/secret.lmk /secret.lmk

FROM microsoft/aspnetcore-build:1.1 AS build
WORKDIR /src
COPY DC_EMV_ALL.sln ./
COPY DCEMV_DemoServer/DCEMV_DemoServer.csproj DCEMV_DemoServer/
COPY DCEMV_EMVSecurity/DCEMV_EMVSecurity.csproj DCEMV_EMVSecurity/
COPY DCEMV_FormattingUtils/DCEMV_FormattingUtils.csproj DCEMV_FormattingUtils/
COPY DCEMV_ISO7816Protocol/DCEMV_ISO7816Protocol.csproj DCEMV_ISO7816Protocol/
COPY DCEMV_ServerShared/DCEMV_ServerShared.csproj DCEMV_ServerShared/
COPY DCEMV_Shared/DCEMV_Shared.csproj DCEMV_Shared/
COPY DCEMV_TLVProtocol/DCEMV_TLVProtocol.csproj DCEMV_TLVProtocol/
COPY DCEMV_TCPIPDriver/DCEMV_TCPIPDriver.csproj DCEMV_TCPIPDriver/
COPY DCEMV_SPDHProtocol/DCEMV_SPDHProtocol.csproj DCEMV_SPDHProtocol/
COPY DCEMV_EMVProtocol/DCEMV_EMVProtocol.csproj DCEMV_EMVProtocol/
RUN dotnet restore
COPY . .
WORKDIR /src/DCEMV_DemoServer
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

ENTRYPOINT ["dotnet", "DCEMV_DemoServer.dll"]
