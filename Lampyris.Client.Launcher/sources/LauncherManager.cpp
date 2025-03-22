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
    // ����Ĭ��ֵ
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
        createDefaultConfig(); // ������Ҳ��Ҫ�ָ�Ĭ���ļ�
    }
}

void LauncherManager::createDefaultConfig() {
    QSettings settings(m_configPath, QSettings::IniFormat);

    // д��Ĭ��������
    settings.setValue(CONFIG_KEY_BASE_URL, m_baseUrl);
    settings.setValue(CONFIG_KEY_VERSION_ENDPOINT, m_versionEndpoint);
    settings.setValue(CONFIG_KEY_DOWNLOAD_ENDPOINT, m_downloadEndpoint);
    settings.setValue(CONFIG_KEY_CURRENT_VERSION, m_currentVersion);
}

void LauncherManager::showFatalErrorTips(const QString& message) {
#if defined(Q_OS_WINDOWS)
    QMessageBox::critical(Q_NULLPTR, "LampyrisӦ�ó�����������!", message);
#elif defined(Q_OS_ANDROID)
    ToastHelper::showToast(message, 1);
#endif
}

void LauncherManager::checkForUpdates() {
    emit updateStatus("���ڼ�����...");
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
                emit updateStatus("���ڸ���Ӧ�ó���̬���ӿ�...");
                downloadPlugin(pluginUrl);
            }
            else {
                emit updateStatus("�����������...");
                loadPlugin();
            }
        }
        else {
            emit updateStatus("������ʧ��: " + reply->errorString());

            // �������Ի���
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
                    checkForUpdates(); // ����ʱ����������
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
            showFatalErrorTips("����Ӧ�ó���ʧ�ܣ��������ɰ汾...");
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
            emit updateStatus("���ڼ���Ӧ�ó���...");
            int retCode = 0;
            if ((retCode = pluginInterface->main(m_args)) != 0) {
                showFatalErrorTips(QString::asprintf("����Ӧ�ó���ʧ��,������:%d", retCode));
                QApplication::quit();
            }
            else {
                emit loadSucceed();
            }
            return;
        }
    }
    showFatalErrorTips("����Ӧ�ó���ʧ��,�޷���ȷ���ز��");
    QApplication::quit();
}
