#pragma once

#include <QApplication>
#include <QWidget>
#include <QLineEdit>
#include <QPushButton>
#include <QVBoxLayout>
#include <QPropertyAnimation>
#include <QTimer>
#include <QPalette>

/*
 * �Ի���LineEdit��֧��ĳЩ����(���������ݲ��Ϸ�)ʱ�����𶯺���˸Ч��
 */
class ShakingLineEdit : public QLineEdit {
    Q_OBJECT
    Q_PROPERTY(QColor backgroundColor READ backgroundColor WRITE setBackgroundColor)

public:
    explicit ShakingLineEdit(QWidget* parent = nullptr) : QLineEdit(parent) {}

    // �����𶯺���˸Ч��
    void triggerInvalidEffect() {
        shake();        // ������
        flashRed();     // ��������Ϊ����ɫ����˸
    }

    // ���ñ�����ɫ
    void setBackgroundColor(const QColor& color) {
        QPalette p = palette();
        p.setColor(QPalette::Base, color);
        setPalette(p);
    }

    // ��ȡ������ɫ
    QColor backgroundColor() const {
        return palette().color(QPalette::Base);
    }

private:
    void shake() {
        // ����һ�����Զ���������Ŀ���� QLineEdit ��λ��
        QPropertyAnimation* animation = new QPropertyAnimation(this, "pos");
        animation->setDuration(400); // ��������ʱ��
        animation->setKeyValueAt(0, pos()); // ��ʼλ��
        animation->setKeyValueAt(0.25, pos() + QPoint(-10, 0)); // ����
        animation->setKeyValueAt(0.5, pos() + QPoint(10, 0)); // ����
        animation->setKeyValueAt(0.75, pos() + QPoint(-10, 0)); // ����
        animation->setKeyValueAt(1, pos()); // �ص�ԭʼλ��
        animation->start(QAbstractAnimation::DeleteWhenStopped); // �����������Զ�ɾ��
    }

    void flashRed() {
        // ʹ�� QPropertyAnimation ʵ����ɫ����
        QPropertyAnimation* animation = new QPropertyAnimation(this, "backgroundColor");
        animation->setDuration(800); // �������ʱ��
        animation->setKeyValueAt(0, backgroundColor()); // ��ʼ��ɫ
        animation->setKeyValueAt(0.25, QColor(255, 200, 200)); // ���䵽����ɫ
        animation->setKeyValueAt(0.5, backgroundColor()); // �ָ�ԭʼ��ɫ
        animation->setKeyValueAt(0.75, QColor(255, 200, 200)); // ���䵽����ɫ
        animation->setKeyValueAt(1, backgroundColor()); // �ָ�ԭʼ��ɫ
        animation->start(QAbstractAnimation::DeleteWhenStopped); // �����������Զ�ɾ��
    }
};