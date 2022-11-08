using Microsoft.Extensions.Configuration;

// 環境変数を読み込む
DotNetEnv.Env.Load(".env");

// NOTE: https://tech-blog.cloud-config.jp/2019-7-11-how-to-configuration-builder/
IConfiguration configuration =
    new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(path: "appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();