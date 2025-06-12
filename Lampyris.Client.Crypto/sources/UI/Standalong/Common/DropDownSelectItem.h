#pragma once

// QT Include(s)
#include <QApplication>
#include <QWidget>
#include <QVBoxLayout>
#include <QLabel>
#include <QPushButton>
#include <QFrame>
#include <QMouseEvent>
#include <QPainter>
#include <QStyleOption>

class DropDownSelectItem : public QFrame {
    Q_OBJECT

public:
    explicit DropDownSelectItem(const QString& text,int index, QWidget* parent = nullptr)
        : QFrame(parent), m_text(text), m_selected(false), m_index(index) {
        setFixedHeight(30); // 设置每个 item 的高度
        setCursor(Qt::PointingHandCursor); // 鼠标样式
        setMouseTracking(true);
    }

    void setSelected(bool selected) {
        m_selected = selected;
        update(); // 更新绘制
    }

    bool isSelected() const {
        return m_selected;
    }

    const QString& text() const {
        return m_text;
    }
protected:
    void    paintEvent(QPaintEvent* event) override;
    void    mousePressEvent(QMouseEvent* event) override;
    void    enterEvent(QEnterEvent* event) override;
    void    leaveEvent(QEvent* event) override;
signals:    
    void    clicked();
private:
    QString m_text;
    bool    m_selected;
    int     m_index;
};