// Project Include(s)
#include "LauncherManager.h"
#include <interface/IPlugin.h>

// QT Include(s)
#include <QFile>
#include <QJsonDocument>
#include <QJsonObject>
#include <QJsonArray>
#include <QTimer>
#include <QElapsedTimer>
#include <QDebug>
#include <QApplication>

#if defined(Q_OS_WINDOWS)
#include <QMessageBox>
#elif defined(Q_OS_ANDROID)
#include <Platformdependent/Android/Toast.hpp>
#endif

LauncherManager::LauncherManager(const QString& configPath, const QStringList& args, QObject* parent)
    : QObject(parent), m_configPath(configPath), m_args(args) {
    // 设置默认值
    m_baseUrl = "https://172.16.2.85:7015";
    m_versionEndpoint = "/api/version";
    m_downloadEndpoint = "/api/download";
    m_currentVersion = "0.0.1";

    loadConfig();
}

void LauncherManager::loadConfig() {
    QFile configFile(m_configPath);
    if (!configFile.exists()) {
        qDebug() << "Config file does not exist. Creating a new one with default values.";
        createDefaultConfig();
    }

    QSettings settings(m_configPath, QSettings::IniFormat);

    m_baseUrl = settings.value(CONFIG_KEY_BASE_URL, "").toString();
    m_versionEndpoint = settings.value(CONFIG_KEY_VERSION_ENDPOINT, "").toString();
    m_downloadEndpoint = settings.value(CONFIG_KEY_DOWNLOAD_ENDPOINT, "").toString();
    m_currentVersion = settings.value(CONFIG_KEY_CURRENT_VERSION, "").toString();

    if (m_baseUrl.isEmpty() || m_versionEndpoint.isEmpty() || m_downloadEndpoint.isEmpty() || m_currentVersion.isEmpty()) {
        qDebug() << "Failed to load some configuration values. Using default values.";
        createDefaultConfig(); // 有问题也需要恢复默认文件
    }
}

void LauncherManager::createDefaultConfig() {
    QSettings settings(m_configPath, QSettings::IniFormat);

    // 写入默认配置项
    settings.setValue(CONFIG_KEY_BASE_URL, m_baseUrl);
    settings.setValue(CONFIG_KEY_VERSION_ENDPOINT, m_versionEndpoint);
    settings.setValue(CONFIG_KEY_DOWNLOAD_ENDPOINT, m_downloadEndpoint);
    settings.setValue(CONFIG_KEY_CURRENT_VERSION, m_currentVersion);
}

void LauncherManager::showFatalErrorTips(const QString& message) {
#if defined(Q_OS_WINDOWS)
    QMessageBox::critical(Q_NULLPTR, "Lampyris应用程序致命错误!", message);
#elif defined(Q_OS_ANDROID)
    ToastHelper::showToast(message, 1);
#endif
}

void LauncherManager::checkForUpdates() {
    emit updateStatus("正在检查更新...");
    QNetworkAccessManager* manager = new QNetworkAccessManager(this);
    QNetworkReply* reply = manager->get(QNetworkRequest(QUrl(m_baseUrl + m_versionEndpoint)));

    connect(reply, &QNetworkReply::finished, [this, reply]() {
        if (reply->error() == QNetworkReply::NoError) {
            QByteArray response = reply->readAll();
            QJsonDocument jsonDoc = QJsonDocument::fromJson(response);
            QJsonObject jsonObj = jsonDoc.object();

            QString latestVersion = jsonObj["version"].toString();
            QString pluginUrl = jsonObj["files"].toArray().first().toObject()["url"].toString();

            if (latestVersion != m_currentVersion) {
                emit updateStatus("正在更新应用程序动态链接库...");
                downloadPlugin(pluginUrl);
            }
            else {
                emit updateStatus("正在载入程序...");
                loadPlugin();
            }
        }
        else {
            emit updateStatus("检查更新失败: " + reply->errorString());

            // 启动重试机制
            int countdownSeconds = RETRY_INTERVAL_MS / 1000;

            QTimer* countdownTimer = new QTimer(this);
            countdownTimer->setInterval(1000);

            connect(countdownTimer, &QTimer::timeout, [this, countdownTimer, &countdownSeconds]() {
                if (countdownSeconds > 0) {
                    emit retryCountdown(countdownSeconds);
                    countdownSeconds--;
                }
                else {
                    countdownTimer->stop();
                    countdownTimer->deleteLater();
                    checkForUpdates(); // 倒计时结束后重试
                }
            });

            countdownTimer->start();
        }
        reply->deleteLater();
    });
}

void LauncherManager::downloadPlugin(const QString& url) {
    QNetworkAccessManager* manager = new QNetworkAccessManager(this);
    QNetworkReply* reply = manager->get(QNetworkRequest(QUrl(url)));

    QElapsedTimer timer;
    qint64 lastBytesReceived = 0;
    timer.start();

    connect(reply, &QNetworkReply::downloadProgress, [this, &timer, &lastBytesReceived](qint64 bytesReceived, qint64 bytesTotal) {
        if (bytesTotal > 0) {
            emit updateProgress(static_cast<int>((bytesReceived * 100) / bytesTotal));
        }
    });

    connect(reply, &QNetworkReply::finished, [this, reply]() {
        if (reply->error() == QNetworkReply::NoError) {
            QFile file(m_pluginPath);
            if (file.open(QIODevice::WriteOnly)) {
                file.write(reply->readAll());
                file.close();
                loadPlugin();
            }
        }
        else {
            showFatalErrorTips("更新应用程序失败，将启动旧版本...");
            loadPlugin();
        }
        reply->deleteLater();
    });
}

void LauncherManager::loadPlugin() {
    QPluginLoader loader(m_pluginPath);
    QObject* plugin = loader.instance();
    if (plugin) {
        IPlugin* pluginInterface = qobject_cast<IPlugin*>(plugin);
        if (pluginInterface) {
            emit updateStatus("正在加载应用程序...");
            int retCode = 0;
            if ((retCode = pluginInterface->main(m_args)) != 0) {
                showFatalErrorTips(QString::asprintf("加载应用程序失败,错误码:%d", retCode));
                QApplication::quit();
            }
            else {
                emit loadSucceed();
            }
            return;
        }
    }
    showFatalErrorTips("加载应用程序失败,无法正确加载插件");
    QApplication::quit();
}
