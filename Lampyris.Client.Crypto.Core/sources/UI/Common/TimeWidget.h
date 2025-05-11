#pragma once
// QT Include(s)
#include <QApplication>
#include <QWidget>
#include <QHBoxLayout>
#include <QLabel>
#include <QTimer>
#include <QDateTime>
#include <QTime>

class TimeWidget : public QWidget {
    Q_OBJECT

public:
    explicit TimeWidget(QWidget* parent = nullptr);
    // 外部设置时间
    void     setTime(const QTime& time);
    // 恢复系统时间更新
    void     resetToSystemTime();
private slots:
    void     updateTime();
private:
    QLabel*  timeLabel; // 用于显示时间的标签
    QTimer*  timer;     // 定时器，用于更新系统时间
    bool     isUsingSystemTime; // 是否使用系统时间
};
