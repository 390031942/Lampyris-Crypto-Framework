// Project Include(s)
#include "AppConfigManager.h"
#include "Base/ApplicationPath.h"

// QT Include(s)
#include <QFile>
#include <QStandardPaths>
#include <QDir>

// 构造函数
AppConfigDataObject::AppConfigDataObject(const QString& filePath)
    : m_settings(filePath, QSettings::IniFormat) {}

// 写入配置项
void AppConfigDataObject::setValue(const QString& group, const QString& key, const QVariant& value) {
    m_settings.beginGroup(group);
    m_settings.setValue(key, value);
    m_settings.endGroup();
    m_settings.sync();  // 强制将更改立即写入文件
}

// 读取配置项
QVariant AppConfigDataObject::getValue(const QString& group, const QString& key, const QVariant& defaultValue) {
    m_settings.beginGroup(group);
    if (!m_settings.contains(key) && defaultValue.isValid()) { // 如果key不存在，但是提供了默认值，则需要设置
        m_settings.setValue(key, defaultValue);
        m_settings.sync();
    }
    QVariant value = m_settings.value(key, defaultValue);
    m_settings.endGroup();
    return value;
}

// 检查配置项是否存在
bool AppConfigDataObject::contains(const QString& group, const QString& key) {
    m_settings.beginGroup(group);
    bool exists = m_settings.contains(key);
    m_settings.endGroup();
    return exists;
}

// 删除配置项
void AppConfigDataObject::remove(const QString& group, const QString& key) {
    m_settings.beginGroup(group);
    m_settings.remove(key);
    m_settings.endGroup();
}

// 获取文件路径
QString AppConfigDataObject::getFilePath() const {
    return m_settings.fileName();
}

AppConfigDataObject* AppConfigManager::getConfig(const QString& fileName) {
    auto filePath = QDir(ApplicationPath::getDocumentPath()).filePath(fileName);
    if (!m_configObjects.contains(fileName)) {
        // 如果文件不存在，创建文件
        if (!QFile::exists(filePath)) {
            QFile file(filePath);
            file.open(QIODevice::WriteOnly);  // 创建空文件
            file.close();
        }

        // 创建新的 AppConfigDataObject
        AppConfigDataObject* dataObject = new AppConfigDataObject(filePath);
        m_configObjects.insert(fileName, dataObject);
    }

    return m_configObjects[fileName];
}