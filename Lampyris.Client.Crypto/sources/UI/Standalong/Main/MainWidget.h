#pragma once
// QT Include(s)
#include <QWidget>

// Project Include(s)
#include "UI/Standalong/Main/MainTitleBar.h"
#include "UI/Standalong/Main/MainToolBar.h"
#include "UI/Standalong/Main/MainStatusBar.h"

class MainWidget:public QWidget {
    Q_OBJECT
public:
    explicit MainWidget(QWidget* parent = Q_NULLPTR);
private:
    MainTitleBar*  m_titleBar;
    MainToolBar*   m_toolBar;
    MainStatusBar* m_statusBar;
    QWidget*       m_centralWidget;
};
    