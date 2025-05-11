#pragma once

// QT Include(s)
#include <QString>
#include <QSettings>
#include <QVariant>
#include <QMap>

// Project Include(s)
#include "Singleton.h"

class AppConfigDataObject {
public:
    explicit   AppConfigDataObject(const QString& filePath);
    void       setValue(const QString& group, const QString& key, const QVariant& value);
    QVariant   getValue(const QString& group, const QString& key, const QVariant& defaultValue = QVariant());
    bool       contains(const QString& group, const QString& key);
    void       remove(const QString& group, const QString& key);
    QString    getFilePath() const;
private:
    QSettings  m_settings; 
};

class AppConfigManager :public Singleton<AppConfigManager> {
public:
    // ��ȡ�򴴽�һ�� AppConfigDataObject
    AppConfigDataObject*                getConfig(const QString& fileName);
private:
    // �ļ��� -> AppConfigDataObject
    QMap<QString, AppConfigDataObject*> m_configObjects; 
};