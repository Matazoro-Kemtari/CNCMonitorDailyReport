using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;

// 環境変数を読み込む
DotNetEnv.Env.Load(".env");

// DIコンテナに入れる
IServiceCollection services = new ServiceCollection();
_ = services.AddSingleton<IConfiguration>(_ => MyConfigurationBuilder());
_ = services.AddSingleton<ILogger, Logger>(_ => LogManager.GetCurrentClassLogger());





// インスタンス提供オブジェクトを作る
using var provider = services.BuildServiceProvider();



// ここから実験
var logger = provider.GetRequiredService<ILogger>();
logger.Trace("DIしたロガーで、Trace ログです。");
logger.Debug("DIしたロガーで、Debug ログです。");
logger.Info("DIしたロガーで、Info ログです。");
logger.Warn("DIしたロガーで、Warn ログです。");
logger.Error("DIしたロガーで、Error ログです。");
logger.Fatal("DIしたロガーで、Fatal ログです。");

var se = provider.GetRequiredService<IConfiguration>();
logger.Debug(se.GetValue("hogehoge", "みつからん"));

// ここまで




// 設定情報ライブラリを作る
static IConfigurationRoot MyConfigurationBuilder() =>
    // NOTE: https://tech-blog.cloud-config.jp/2019-7-11-how-to-configuration-builder/
    new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile(path: "appsettings.json", optional: true)
        .AddEnvironmentVariables()
        .Build();