#pragma once

// QT Include(s)
#include <QObject>
#include <QString>
#include <QStringList>
#include <QNetworkAccessManager>
#include <QNetworkReply>
#include <QSettings>
#include <QPluginLoader>

#define CONFIG_KEY_BASE_URL           "General/baseUrl"
#define CONFIG_KEY_VERSION_ENDPOINT   "General/versionEndpoint"
#define CONFIG_KEY_DOWNLOAD_ENDPOINT  "General/downloadEndpoint"
#define CONFIG_KEY_CURRENT_VERSION    "General/currentVersion"

#define RETRY_INTERVAL_MS 5000

class LauncherManager : public QObject {
    Q_OBJECT

public:
    explicit        LauncherManager(const QString& configPath, 
                                    const QStringList& args, 
                                    QObject* parent = nullptr);
                    
    void            checkForUpdates();
    void            downloadPlugin(const QString& url);
    void            loadPlugin();
                    
signals:            
    void            updateStatus(const QString& status); 
    void            updateProgress(int progress);        
    void            retryCountdown(int seconds);         
    void            loadSucceed();
private:            
    void            loadConfig();
    void            createDefaultConfig();
    void            showFatalErrorTips(const QString& message);

    QString         m_configPath;
    QStringList     m_args;
    QString         m_baseUrl;
    QString         m_versionEndpoint;
    QString         m_downloadEndpoint;
    QString         m_currentVersion;
    QString         m_pluginPath;
};
