// Project Include(s)
#include "TimeWidget.h"

TimeWidget::TimeWidget(QWidget* parent) : QWidget(parent), isUsingSystemTime(true) {
    // ����������
    QHBoxLayout* layout = new QHBoxLayout(this);
    layout->setContentsMargins(0, 0, 0, 0);
    layout->setSpacing(5);

    // ���ʱ��ͼ��
    QLabel* iconLabel = new QLabel(this);
    iconLabel->setPixmap(QPixmap(":res/icons/clock.png").scaled(16, 16, Qt::KeepAspectRatio, Qt::SmoothTransformation));
    layout->addWidget(iconLabel);

    // �Ҳ�ʱ����ʾ
    timeLabel = new QLabel(this);
    layout->addWidget(timeLabel);

    // ����ʱ�����
    updateTime(); // ��ʼ��ʱ����ʾ
    timer = new QTimer(this);
    connect(timer, &QTimer::timeout, this, &TimeWidget::updateTime);
    timer->start(1000); // ÿ�����һ��

    timeLabel->setStyleSheet(QString("color: white;"));
}

void TimeWidget::updateTime() {
    if (isUsingSystemTime) {
        // ��ȡ��ǰϵͳʱ�䲢���µ���ǩ
        QString currentTime = QDateTime::currentDateTime().toString("hh:mm:ss");
        timeLabel->setText(currentTime);
    }
}

// �ⲿ����ʱ��
void TimeWidget::setTime(const QTime& time) {
    isUsingSystemTime = false; // ֹͣʹ��ϵͳʱ��
    timer->stop(); // ֹͣϵͳʱ����Զ�����
    timeLabel->setText(time.toString("hh:mm:ss")); // ��ʾ�ⲿ���õ�ʱ��
}

// �ָ�ϵͳʱ�����
void TimeWidget::resetToSystemTime() {
    isUsingSystemTime = true; // �ָ�ʹ��ϵͳʱ��
    timer->start(1000); // ��������ϵͳʱ����Զ�����
    updateTime(); // ��������һ��ʱ��
}
