// Project Include(s)
#include "AppConfigManager.h"
#include "Base/ApplicationPath.h"

// QT Include(s)
#include <QFile>
#include <QStandardPaths>
#include <QDir>

// ���캯��
AppConfigDataObject::AppConfigDataObject(const QString& filePath)
    : m_settings(filePath, QSettings::IniFormat) {}

// д��������
void AppConfigDataObject::setValue(const QString& group, const QString& key, const QVariant& value) {
    m_settings.beginGroup(group);
    m_settings.setValue(key, value);
    m_settings.endGroup();
    m_settings.sync();  // ǿ�ƽ���������д���ļ�
}

// ��ȡ������
QVariant AppConfigDataObject::getValue(const QString& group, const QString& key, const QVariant& defaultValue) {
    m_settings.beginGroup(group);
    if (!m_settings.contains(key) && defaultValue.isValid()) { // ���key�����ڣ������ṩ��Ĭ��ֵ������Ҫ����
        m_settings.setValue(key, defaultValue);
        m_settings.sync();
    }
    QVariant value = m_settings.value(key, defaultValue);
    m_settings.endGroup();
    return value;
}

// ����������Ƿ����
bool AppConfigDataObject::contains(const QString& group, const QString& key) {
    m_settings.beginGroup(group);
    bool exists = m_settings.contains(key);
    m_settings.endGroup();
    return exists;
}

// ɾ��������
void AppConfigDataObject::remove(const QString& group, const QString& key) {
    m_settings.beginGroup(group);
    m_settings.remove(key);
    m_settings.endGroup();
}

// ��ȡ�ļ�·��
QString AppConfigDataObject::getFilePath() const {
    return m_settings.fileName();
}

AppConfigDataObject* AppConfigManager::getConfig(const QString& fileName) {
    auto filePath = QDir(ApplicationPath::getDocumentPath()).filePath(fileName);
    if (!m_configObjects.contains(fileName)) {
        // ����ļ������ڣ������ļ�
        if (!QFile::exists(filePath)) {
            QFile file(filePath);
            file.open(QIODevice::WriteOnly);  // �������ļ�
            file.close();
        }

        // �����µ� AppConfigDataObject
        AppConfigDataObject* dataObject = new AppConfigDataObject(filePath);
        m_configObjects.insert(fileName, dataObject);
    }

    return m_configObjects[fileName];
}