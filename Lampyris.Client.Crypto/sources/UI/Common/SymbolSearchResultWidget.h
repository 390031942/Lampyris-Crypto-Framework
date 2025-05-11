#pragma once

// QT Include(s)
#include <QApplication>
#include <QWidget>
#include <QHBoxLayout>
#include <QVBoxLayout>
#include <QLineEdit>
#include <QPushButton>
#include <QLabel>
#include <QSpacerItem>
#include <QPropertyAnimation>
#include <QFocusEvent>
#include <QTimer>
#include <QDebug>
#include <QScrollBar>

class SymbolSearchResultWidget : public QWidget {
    Q_OBJECT
public:
                 SymbolSearchResultWidget(QWidget* parent);
    void         setHistory(const QStringList& history, int maxWidth);
protected:       
    void         mousePressEvent(QMouseEvent* event) override;
private:
    QVBoxLayout* layout;
    QScrollBar*  scrollBar;
};