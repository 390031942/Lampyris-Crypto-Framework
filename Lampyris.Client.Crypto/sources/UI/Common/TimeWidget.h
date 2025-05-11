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
    // �ⲿ����ʱ��
    void     setTime(const QTime& time);
    // �ָ�ϵͳʱ�����
    void     resetToSystemTime();
private slots:
    void     updateTime();
private:
    QLabel*  timeLabel; // ������ʾʱ��ı�ǩ
    QTimer*  timer;     // ��ʱ�������ڸ���ϵͳʱ��
    bool     isUsingSystemTime; // �Ƿ�ʹ��ϵͳʱ��
};
