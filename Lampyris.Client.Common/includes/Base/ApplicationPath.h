#pragma once

// QT Include(s)
#include <QFile>
#include <QStandardPaths>
#include <QDir>

class ApplicationPath {
public:
	static QString getDocumentPath() {
        // ��ȡ "�ҵ��ĵ�" �ļ���·��
        QString documentsPath = QStandardPaths::writableLocation(QStandardPaths::DocumentsLocation);

        QDir dir(documentsPath);
        QString folderPath = dir.filePath("Lampyris Crypto");

        if (!dir.exists("Lampyris Crypto")) {
            if (dir.mkpath("Lampyris Crypto")) {
                qDebug() << "App document folder created successfully!";
            }
            else {
                qDebug() << "Failed to create App document folder!";
            }
        }

        return folderPath;
	}
};