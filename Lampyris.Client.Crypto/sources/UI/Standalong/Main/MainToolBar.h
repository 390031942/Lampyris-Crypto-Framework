#pragma once

// QT Include(s)
#include <QApplication>
#include <QMainWindow>
#include <QToolBar>
#include <QToolButton>
#include <QIcon>
#include <QDebug>
#include <QSpacerItem>
#include <QLayout>
#include <QButtonGroup.h>

// Project Include(s)
#include "MainToolButton.h"

class MainToolBar : public QWidget {
public:
                    MainToolBar(QWidget* parent = nullptr);
    MainToolButton* addButton(const QString& defaultIconPath, const QString& checkedIconPath, const QString& text);
    void            addSpacer();
private:
    QBoxLayout*     layout;
    QButtonGroup*   buttonGroup;
};