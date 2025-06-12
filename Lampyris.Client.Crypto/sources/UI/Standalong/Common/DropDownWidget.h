#pragma once
// QT Include(s)
#include <QApplication>
#include <QWidget>
#include <QVBoxLayout>
#include <QPushButton>

// Project Include(s)
#include "DropDownSelectItem.h"

class DropDownWidget : public QWidget {
    Q_OBJECT

public:
    explicit     DropDownWidget(QWidget* parent = nullptr);
    void         setOptions(QStringList items);
protected:
    void         paintEvent(QPaintEvent* event) override;
signals:         
    void         itemSelected(const QString& text);
    void         itemSelected(int index);
private:
    QVBoxLayout* m_layout;
};