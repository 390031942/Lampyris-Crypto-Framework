#pragma once

// QT Include(s)
#include <QWidget>
#include <QString>
#include <QStringList>

// Project Include(s)
#include "LauncherManager.h"
#include "ui_LauncherWidget.h" 

class LauncherWidget : public QWidget {
    Q_OBJECT

public:
    explicit           LauncherWidget(const QString& configPath,
                                      const QStringList& args, 
                                      QWidget* parent = nullptr);

private:
    LauncherManager*   m_launcherManager;
    Ui::LauncherWidget m_ui;
};
