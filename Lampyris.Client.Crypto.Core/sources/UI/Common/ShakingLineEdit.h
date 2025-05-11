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
 * 自绘制LineEdit，支持某些场合(如输入内容不合法)时触发震动和闪烁效果
 */
class ShakingLineEdit : public QLineEdit {
    Q_OBJECT
    Q_PROPERTY(QColor backgroundColor READ backgroundColor WRITE setBackgroundColor)

public:
    explicit ShakingLineEdit(QWidget* parent = nullptr) : QLineEdit(parent) {}

    // 触发震动和闪烁效果
    void triggerInvalidEffect() {
        shake();        // 左右震动
        flashRed();     // 背景渐变为淡红色并闪烁
    }

    // 设置背景颜色
    void setBackgroundColor(const QColor& color) {
        QPalette p = palette();
        p.setColor(QPalette::Base, color);
        setPalette(p);
    }

    // 获取背景颜色
    QColor backgroundColor() const {
        return palette().color(QPalette::Base);
    }

private:
    void shake() {
        // 创建一个属性动画，动画目标是 QLineEdit 的位置
        QPropertyAnimation* animation = new QPropertyAnimation(this, "pos");
        animation->setDuration(400); // 动画持续时间
        animation->setKeyValueAt(0, pos()); // 起始位置
        animation->setKeyValueAt(0.25, pos() + QPoint(-10, 0)); // 左移
        animation->setKeyValueAt(0.5, pos() + QPoint(10, 0)); // 右移
        animation->setKeyValueAt(0.75, pos() + QPoint(-10, 0)); // 左移
        animation->setKeyValueAt(1, pos()); // 回到原始位置
        animation->start(QAbstractAnimation::DeleteWhenStopped); // 动画结束后自动删除
    }

    void flashRed() {
        // 使用 QPropertyAnimation 实现颜色渐变
        QPropertyAnimation* animation = new QPropertyAnimation(this, "backgroundColor");
        animation->setDuration(800); // 渐变持续时间
        animation->setKeyValueAt(0, backgroundColor()); // 起始颜色
        animation->setKeyValueAt(0.25, QColor(255, 200, 200)); // 渐变到淡红色
        animation->setKeyValueAt(0.5, backgroundColor()); // 恢复原始颜色
        animation->setKeyValueAt(0.75, QColor(255, 200, 200)); // 渐变到淡红色
        animation->setKeyValueAt(1, backgroundColor()); // 恢复原始颜色
        animation->start(QAbstractAnimation::DeleteWhenStopped); // 动画结束后自动删除
    }
};