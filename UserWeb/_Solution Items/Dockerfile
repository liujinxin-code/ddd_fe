# 多阶段构建：.NET 8 SDK 编译发布，ASP.NET Core 8 运行时运行
# 上下文（build context）为仓库根目录：docker build -f Dockerfile -t ddd-api .

# ---------- 1. 构建阶段 ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 先拷贝后端相关工程（含解决方案与多层引用），利用层缓存加速
COPY UserWeb/ UserWeb/

# 还原 + 发布（Release）。Open.csproj 输出程序集名为 Open.dll
RUN dotnet restore UserWeb/1.Open/Open.csproj
RUN dotnet publish UserWeb/1.Open/Open.csproj -c Release -o /app/publish --no-restore

# ---------- 2. 运行阶段 ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 容器内仅监听 http（TLS 由前置 nginx 终结）
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

COPY --from=build /app/publish .

# 上传文件目录（wwwroot/images）通过 volume 持久化，避免容器重建丢图
VOLUME ["/app/wwwroot/images"]

EXPOSE 8080
ENTRYPOINT ["dotnet", "Open.dll"]
