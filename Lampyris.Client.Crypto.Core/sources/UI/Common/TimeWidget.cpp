// Project Include(s)
#include "TimeWidget.h"

TimeWidget::TimeWidget(QWidget* parent) : QWidget(parent), isUsingSystemTime(true) {
    // 设置主布局
    QHBoxLayout* layout = new QHBoxLayout(this);
    layout->setContentsMargins(10, 10, 10, 10);
    layout->setSpacing(10);

    // 左侧时钟图标
    QLabel* iconLabel = new QLabel(this);
    iconLabel->setPixmap(QPixmap(":res/icons/clock.png").scaled(32, 32, Qt::KeepAspectRatio, Qt::SmoothTransformation));
    layout->addWidget(iconLabel);

    // 右侧时间显示
    timeLabel = new QLabel(this);
    timeLabel->setFont(QFont("Arial", 16, QFont::Bold));
    layout->addWidget(timeLabel);

    // 设置时间更新
    updateTime(); // 初始化时间显示
    timer = new QTimer(this);
    connect(timer, &QTimer::timeout, this, &TimeWidget::updateTime);
    timer->start(1000); // 每秒更新一次
}

void TimeWidget::updateTime() {
    if (isUsingSystemTime) {
        // 获取当前系统时间并更新到标签
        QString currentTime = QDateTime::currentDateTime().toString("hh:mm:ss");
        timeLabel->setText(currentTime);
    }
}

// 外部设置时间
void TimeWidget::setTime(const QTime& time) {
    isUsingSystemTime = false; // 停止使用系统时间
    timer->stop(); // 停止系统时间的自动更新
    timeLabel->setText(time.toString("hh:mm:ss")); // 显示外部设置的时间
}

// 恢复系统时间更新
void TimeWidget::resetToSystemTime() {
    isUsingSystemTime = true; // 恢复使用系统时间
    timer->start(1000); // 重新启动系统时间的自动更新
    updateTime(); // 立即更新一次时间
}
