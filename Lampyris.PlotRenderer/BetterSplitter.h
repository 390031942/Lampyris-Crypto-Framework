#pragma once
// QT Include(s)
#include <QSplitter>

/// <summary>
/// QSpliiter重载,当鼠标放到Handler上时改变鼠标图标
/// </summary>
class BetterSplitter : public QSplitter {
    Q_OBJECT
public:
    explicit BetterSplitter(Qt::Orientation orientation, QWidget* parent = nullptr)
        : QSplitter(orientation, parent) {}
protected:
    QSplitterHandle* createHandle() override;
};
